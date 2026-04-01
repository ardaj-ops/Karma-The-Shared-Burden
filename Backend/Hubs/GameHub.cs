using Microsoft.AspNetCore.SignalR;
using RoguelikeCardGame.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; 
using System;

namespace RoguelikeCardGame.Hubs
{
    public class ActiveEnemy
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty; 
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public EnemyAction CurrentAction { get; set; } = new EnemyAction();

        // NOVÉ: Paměť pro efekty nepřátel
        public Dictionary<string, int> Effects { get; set; } = new Dictionary<string, int>();
        public void AddEffect(string type, int amount)
        {
            if (Effects.ContainsKey(type)) Effects[type] += amount;
            else Effects[type] = amount;
        }
    }

    public class GameHub : Hub
    {
        private static ConcurrentDictionary<string, GameRoom> _activeRooms = new ConcurrentDictionary<string, GameRoom>();
        private static ConcurrentDictionary<string, List<ActiveEnemy>> _roomEnemies = new ConcurrentDictionary<string, List<ActiveEnemy>>();

        private object GetTeamStats(GameRoom room)
        {
            return room.Players.Select(p => new { name = p.Name, hp = p.Hp, maxHp = p.MaxHp, block = p.Block, heroClass = p.HeroClass }).ToList();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            foreach (var roomKvp in _activeRooms)
            {
                var room = roomKvp.Value;
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    room.Players.Remove(player);
                    if (room.Players.Count == 0)
                    {
                        _activeRooms.TryRemove(roomKvp.Key, out _);
                        _roomEnemies.TryRemove(roomKvp.Key, out _);
                    }
                    else
                    {
                        await Clients.Group(roomKvp.Key).SendAsync("LobbyUpdate", room.Players.Select(p => new { name = p.Name, heroClass = p.HeroClass }).ToList());
                    }
                    break;
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateLobby(string roomName, string playerName, string heroClass)
        {
            if (_activeRooms.ContainsKey(roomName)) { await Clients.Caller.SendAsync("LobbyError", "Místnost už existuje!"); return; }
            _activeRooms.TryAdd(roomName, new GameRoom(roomName));
            await JoinLobby(roomName, playerName, heroClass);
        }

        public async Task JoinLobby(string roomName, string playerName, string heroClass)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                if (room.Players.Count >= 5) { await Clients.Caller.SendAsync("LobbyError", "Místnost je plná!"); return; }

                var newPlayer = new Player(Context.ConnectionId, playerName) { HeroClass = heroClass };
                if (HeroDatabase.Heroes.TryGetValue(heroClass, out var template))
                {
                    newPlayer.MaxHp = template.MaxHp;
                    newPlayer.StartingDeck = new List<string>(template.StartingDeck); 
                }

                room.Players.Add(newPlayer);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                
                await Clients.Group(roomName).SendAsync("LobbyUpdate", room.Players.Select(p => new { name = p.Name, heroClass = p.HeroClass }).ToList());
                
                if (room.Players.Count == 1) await Clients.Caller.SendAsync("YouAreHost");
            }
            else { await Clients.Caller.SendAsync("LobbyError", "Místnost nenalezena!"); }
        }

        public async Task StartGame(string roomName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var combinedCards = new Dictionary<string, CardTemplate>(CardDatabase.Cards);
                foreach (var kvp in UpgradedCardsDatabase.UpgradedCards)
                {
                    combinedCards[kvp.Key] = kvp.Value;
                }

                foreach (var player in room.Players)
                {
                    player.Gold = 50; 
                    player.InitializeGame(); 
                    player.DrawCards(5); 

                    if (HeroDatabase.Heroes.TryGetValue(player.HeroClass, out var template) && template.StartingRelic != null)
                    {
                        room.TeamRelics.Add(new Relic(template.StartingRelic.Id, $"{template.StartingRelic.Name} ({player.Name})", template.StartingRelic.Description));
                    }

                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveInitialState", 
                        player.Hand, player.Mana, combinedCards, player.Gold, 
                        player.DrawPile, player.DiscardPile, player.Hp, player.MaxHp, player.Block, player.StartingDeck);
                }
                await Clients.Group(roomName).SendAsync("UpdateRelics", room.TeamRelics.ToList());
                await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                await Clients.Group(roomName).SendAsync("GameStarted", roomName, room.Map);
            }
        }

        public async Task VoteNextNode(string roomName, string playerName, int nodeId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                room.MapVotes[playerName] = nodeId;
                await Clients.Group(roomName).SendAsync("UpdateMapVotes", room.MapVotes);

                int requiredVotes = (int)Math.Floor(room.Players.Count / 2.0) + 1;
                var voteCounts = room.MapVotes.GroupBy(v => v.Value).Select(g => new { NodeId = g.Key, Count = g.Count() });
                var winningNode = voteCounts.FirstOrDefault(v => v.Count >= requiredVotes);

                if (winningNode != null)
                {
                    room.MapVotes.Clear(); 
                    await MoveToNextNode(roomName, winningNode.NodeId); 
                }
            }
        }

        private async Task MoveToNextNode(string roomName, int nodeId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var targetNode = room.Map.FirstOrDefault(n => n.Id == nodeId);
                if (targetNode == null) return;

                if (room.CurrentNodeId == -1) { if (targetNode.Floor != 0) return; }
                else
                {
                    var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                    if (currentNode == null || !currentNode.ConnectedTo.Contains(nodeId)) return;
                }

                room.CurrentNodeId = nodeId;
                var enemyList = new List<ActiveEnemy>();
                Random rng = new Random();
                
                foreach (var p in room.Players)
                {
                    p.Mana = p.MaxMana;
                    p.Block = 0;
                    p.Effects.Clear(); 
                    p.DiscardPile.AddRange(p.Hand);
                    p.Hand.Clear();
                    p.DrawCards(5);
                    p.CardsPlayedThisTurn = 0; // Nová pomocná proměnná pro Relikvie
                }

                if (targetNode.Type == NodeType.Encounter || targetNode.Type == NodeType.EliteEncounter || targetNode.Type == NodeType.Boss)
                {
                    int enemyCount = targetNode.Type == NodeType.Encounter ? rng.Next(1, 5) : rng.Next(1, 3);
                    string tier = targetNode.Type == NodeType.Boss ? "Boss" : (targetNode.Type == NodeType.EliteEncounter ? "Elite" : "Normal");

                    for (int i = 0; i < enemyCount; i++)
                    {
                        var template = EnemyDatabase.GetRandomEnemy(tier, room.CurrentAct);
                        enemyList.Add(new ActiveEnemy {
                            Name = template.Name + (enemyCount > 1 ? $" {i + 1}" : ""),
                            TemplateName = template.Name, MaxHp = template.MaxHp, Hp = template.MaxHp,
                            CurrentAction = EnemyDatabase.GetRandomActionForEnemy(template.Name)
                        });
                    }

                    _roomEnemies[roomName] = enemyList;
                    
                    foreach (var p in room.Players)
                    {
                        RelicManager.ApplyCombatStartRelics(p, room, enemyList);
                        RelicManager.ApplyTurnStartRelics(p, room);
                        
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, enemyList);
                    }
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, enemyList);
                }
                else if (targetNode.Type == NodeType.Shop)
                {
                    targetNode.IsCompleted = true;
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());
                    
                    foreach (var p in room.Players)
                    {
                        var playerShop = ShopManager.GenerateShopForPlayer(p);
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                        await Clients.Client(p.ConnectionId).SendAsync("EnterShop", playerShop.Cards, playerShop.Relics, playerShop.RemoveCost); 
                    }
                }
                else if (targetNode.Type == NodeType.Event)
                {
                    targetNode.IsCompleted = true;
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());

                    var randomEvent = EventDatabase.GetRandomEvent();
                    foreach (var p in room.Players)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                    }
                    await Clients.Group(roomName).SendAsync("EnterEvent", randomEvent);
                }
                else if (targetNode.Type == NodeType.RestPlace)
                {
                    targetNode.IsCompleted = true;
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());

                    foreach (var p in room.Players)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                        await Clients.Client(p.ConnectionId).SendAsync("EnterRestPlace");
                    }
                }
                else if (targetNode.Type == NodeType.Treasure)
                {
                    targetNode.IsCompleted = true;
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());

                    List<string> offeredRelicsThisTurn = new List<string>();

                    foreach (var p in room.Players)
                    {
                        var treasureLoot = TreasureManager.GenerateTreasureForPlayer(p);
                        p.Gold += treasureLoot.Gold;

                        var availableRelics = RelicDatabase.LootRelics
                            .Where(r => !room.TeamRelics.Any(tr => tr.Id == r.Id) && !offeredRelicsThisTurn.Contains(r.Id))
                            .ToList();
                            
                        Relic? relicChoice = null;
                        if (availableRelics.Count > 0)
                        {
                            relicChoice = availableRelics[rng.Next(availableRelics.Count)];
                            offeredRelicsThisTurn.Add(relicChoice.Id);
                        }

                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                        await Clients.Client(p.ConnectionId).SendAsync("ShowRewardScreen", treasureLoot.Cards, relicChoice, treasureLoot.Gold, p.Gold);
                    }
                }
                else
                {
                    targetNode.IsCompleted = true;
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());
                    foreach (var p in room.Players)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                    }
                }
                
                await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
            }
        }

        public async Task RestPlaceAction(string roomName, string playerName, string actionType, string cardId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var p = room.Players.FirstOrDefault(x => x.Name == playerName);
                if (p != null)
                {
                    if (actionType == "heal")
                    {
                        int healAmount = (int)(p.MaxHp * 0.3);
                        // ZMĚNA: Relikvie mohou upravit množství léčení v Táboráku
                        healAmount = RelicManager.ApplyCampfireRelics(p, room, healAmount);
                        p.Hp += healAmount;
                        if (p.Hp > p.MaxHp) p.Hp = p.MaxHp;
                    }
                    else if (actionType == "upgrade" && !string.IsNullOrEmpty(cardId))
                    {
                        if (p.StartingDeck.Contains(cardId) && !cardId.EndsWith("+"))
                        {
                            p.StartingDeck.Remove(cardId);
                            string upgradedCard = cardId + "+";
                            p.StartingDeck.Add(upgradedCard);
                            
                            if(p.DrawPile.Contains(cardId)) { p.DrawPile.Remove(cardId); p.DrawPile.Add(upgradedCard); }
                            if(p.DiscardPile.Contains(cardId)) { p.DiscardPile.Remove(cardId); p.DiscardPile.Add(upgradedCard); }
                            if(p.Hand.Contains(cardId)) { p.Hand.Remove(cardId); p.Hand.Add(upgradedCard); }
                        }
                    }
                    
                    await Clients.Client(p.ConnectionId).SendAsync("RestActionCompleted", p.Hp, p.StartingDeck);
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                }
            }
        }

        public async Task BuyShopItem(string roomName, string playerName, string itemId, string type, int price)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var p = room.Players.FirstOrDefault(x => x.Name == playerName);
                if (p != null && p.Gold >= price)
                {
                    p.Gold -= price;
                    if (type == "Card")
                    {
                        p.StartingDeck.Add(itemId);
                        p.DrawPile.Add(itemId);
                    }
                    else if (type == "Relic")
                    {
                        var r = RelicDatabase.LootRelics.FirstOrDefault(x => x.Id == itemId);
                        if (r != null) {
                            room.TeamRelics.Add(new Relic(itemId, $"{r.Name} ({p.Name})", r.Description));
                            await Clients.Group(roomName).SendAsync("UpdateRelics", room.TeamRelics.ToList());
                        }
                    }
                    await Clients.Client(p.ConnectionId).SendAsync("ShopPurchaseSuccess", p.Gold, p.StartingDeck);
                }
            }
        }

        public async Task RemoveCardFromDeck(string roomName, string playerName, string cardId, int price)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var p = room.Players.FirstOrDefault(x => x.Name == playerName);
                if (p != null && p.Gold >= price && p.StartingDeck.Contains(cardId))
                {
                    p.Gold -= price;
                    p.StartingDeck.Remove(cardId);
                    p.DrawPile.Remove(cardId);
                    p.DiscardPile.Remove(cardId);
                    p.Hand.Remove(cardId);
                    await Clients.Client(p.ConnectionId).SendAsync("ShopPurchaseSuccess", p.Gold, p.StartingDeck);
                }
            }
        }

        public async Task ResolveEventOption(string roomName, string playerName, string eventId, int optionId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var p = room.Players.FirstOrDefault(x => x.Name == playerName);
                if (p != null)
                {
                    EventDatabase.ApplyEventEffect(eventId, optionId, p);
                    await Clients.Client(p.ConnectionId).SendAsync("EventResolved", p.Gold, p.Hp, p.MaxHp);
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                }
            }
        }

        public async Task SelectCard(string roomName, string playerName, string cardId, int karmaShift, int damage, string targetEnemyId, string targetPlayerName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null && player.Hand.Contains(cardId))
                {
                    player.Hand.Remove(cardId);
                    player.DiscardPile.Add(cardId);
                    player.CardsPlayedThisTurn++; // Pro relikvie počítající zahrané karty
                    
                    room.CurrentKarma += karmaShift;
                    
                    CardTemplate fullCard = null;
                    
                    if (cardId.EndsWith("+") && UpgradedCardsDatabase.UpgradedCards.TryGetValue(cardId, out var upgCard))
                    {
                        fullCard = upgCard;
                    }
                    else
                    {
                        string baseCardId = cardId.EndsWith("+") ? cardId.Substring(0, cardId.Length - 1) : cardId;
                        if (CardDatabase.Cards.TryGetValue(baseCardId, out var baseCard))
                        {
                            if (cardId.EndsWith("+"))
                            {
                                fullCard = new CardTemplate(
                                    baseCard.Id + "+", "⭐ " + baseCard.Name, baseCard.Description + " (Vylepšeno)",
                                    Math.Max(0, baseCard.Cost - 1), 
                                    baseCard.KarmaShift,
                                    baseCard.Damage > 0 ? baseCard.Damage + 3 : 0, 
                                    baseCard.Block > 0 ? baseCard.Block + 3 : 0, 
                                    baseCard.Heal > 0 ? baseCard.Heal + 2 : 0, 
                                    baseCard.DrawCards, baseCard.HitCount, baseCard.TargetEffects, baseCard.SelfEffects
                                );
                            }
                            else
                            {
                                fullCard = baseCard;
                            }
                        }
                    }

                    if (fullCard != null)
                    {
                        var enemies = _roomEnemies.ContainsKey(roomName) ? _roomEnemies[roomName] : new List<ActiveEnemy>();

                        // ZMĚNA: Některé relikvie reagují přímo na zahrání karty (např. 10. zahraná karta dává Blok)
                        RelicManager.ApplyCardPlayedRelics(player, room, fullCard);

                        // --- 1. Aplikace Self Efektů ---
                        foreach(var eff in fullCard.SelfEffects) player.AddEffect(eff.Type, eff.Amount);

                        // --- 2. Zpracování Obrany ---
                        int dex = player.Effects.ContainsKey(EffectType.Dexterity) ? player.Effects[EffectType.Dexterity] : 0;
                        int block = fullCard.Block > 0 ? fullCard.Block + dex : 0;
                        
                        block = RelicManager.ModifyBlock(block, player, room);
                        player.Block += block;

                        // --- 3. Zpracování Léčení ---
                        if (fullCard.Heal > 0)
                        {
                            var targetPlayer = string.IsNullOrEmpty(targetPlayerName) ? player : room.Players.FirstOrDefault(p => p.Name == targetPlayerName);
                            if (targetPlayer != null) 
                            {
                                targetPlayer.Hp += fullCard.Heal;
                                if (targetPlayer.Hp > targetPlayer.MaxHp) targetPlayer.Hp = targetPlayer.MaxHp;
                                
                                foreach(var eff in fullCard.TargetEffects) targetPlayer.AddEffect(eff.Type, eff.Amount);
                            }
                        }

                        // --- 4. Zpracování Útoku ---
                        if (fullCard.Damage > 0 || (fullCard.TargetEffects.Count > 0 && fullCard.Heal == 0))
                        {
                            var target = enemies.FirstOrDefault(e => e.Id == targetEnemyId && e.Hp > 0) ?? enemies.FirstOrDefault(e => e.Hp > 0); 

                            if (target != null)
                            {
                                foreach(var eff in fullCard.TargetEffects) target.AddEffect(eff.Type, eff.Amount);

                                if (fullCard.Damage > 0)
                                {
                                    int str = player.Effects.ContainsKey(EffectType.Strength) ? player.Effects[EffectType.Strength] : 0;
                                    int baseDmg = fullCard.Damage + str;
                                    
                                    if (player.Effects.ContainsKey(EffectType.Weak) && player.Effects[EffectType.Weak] > 0) 
                                        baseDmg = (int)(baseDmg * 0.75);

                                    baseDmg = RelicManager.ModifyDamage(baseDmg, player, room);

                                    double karmaMultiplier = 1.0;
                                    if (room.CurrentKarma >= 10) karmaMultiplier = 0.8;
                                    else if (room.CurrentKarma <= -10) karmaMultiplier = 1.3;

                                    int actualDamage = (int)Math.Round(baseDmg * karmaMultiplier) * (fullCard.HitCount > 0 ? fullCard.HitCount : 1);
                                    
                                    if (target.Effects.ContainsKey(EffectType.Vulnerable) && target.Effects[EffectType.Vulnerable] > 0)
                                        actualDamage = (int)(actualDamage * 1.5);

                                    target.Hp -= actualDamage;
                                    
                                    // ZMĚNA: Zachycení smrti nepřítele pro Relikvie
                                    if (target.Hp <= 0) 
                                    {
                                        target.Hp = 0;
                                        RelicManager.OnEnemyKilled(player, room);
                                    }
                                }
                            }
                        }

                        int drawAmount = fullCard.DrawCards;
                        if (drawAmount > 0)
                        {
                            player.DrawCards(drawAmount);
                            var enemiesForSync = _roomEnemies.ContainsKey(roomName) ? _roomEnemies[roomName] : new List<ActiveEnemy>();
                            await Clients.Client(player.ConnectionId).SendAsync("ReceiveNewTurnState", 
                                player.Hand, player.Mana, player.Gold, player.DrawPile, player.DiscardPile, player.Hp, player.MaxHp, player.Block, enemiesForSync);
                        }
                    }

                    await Clients.Group(roomName).SendAsync("CardPlayedLog", playerName, cardId);
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                    
                    var summaryForUI = new List<string>();
                    var currentEnemies = _roomEnemies.ContainsKey(roomName) ? _roomEnemies[roomName] : new List<ActiveEnemy>();
                    await Clients.Group(roomName).SendAsync("TurnResolved", summaryForUI, fullCard?.Damage ?? 0, room.CurrentKarma, currentEnemies);
                }
            }
        }

        // Pomocná metoda pro vyhodnocení efektů na konci kola
        private void ProcessEffectsAtEndOfTurn(Dictionary<string, int> effects, ref int hp, ref int block, List<string> summary, string name)
        {
            if (effects.ContainsKey(EffectType.Poison) && effects[EffectType.Poison] > 0) {
                int dmg = effects[EffectType.Poison];
                hp -= dmg;
                summary.Add($"☠️ {name} dostal {dmg} poškození jedem.");
                effects[EffectType.Poison] -= 1;
            }
            if (effects.ContainsKey(EffectType.Flame) && effects[EffectType.Flame] > 0) {
                int dmg = effects[EffectType.Flame];
                hp -= dmg;
                summary.Add($"🔥 {name} dostal {dmg} poškození hořením.");
                effects[EffectType.Flame] -= 1;
            }
            if (effects.ContainsKey(EffectType.Regen) && effects[EffectType.Regen] > 0) {
                int heal = effects[EffectType.Regen];
                hp += heal;
                summary.Add($"💖 {name} se vyléčil o {heal} HP.");
                effects[EffectType.Regen] -= 1;
            }
            if (effects.ContainsKey(EffectType.Weak) && effects[EffectType.Weak] > 0) effects[EffectType.Weak] -= 1;
            if (effects.ContainsKey(EffectType.Vulnerable) && effects[EffectType.Vulnerable] > 0) effects[EffectType.Vulnerable] -= 1;
        }

        public async Task PlayerReady(string roomName, string playerName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                room.PlayersReady.Add(playerName);
                await Clients.Group(roomName).SendAsync("PlayerReadyLog", playerName, room.PlayersReady.Count, room.Players.Count);

                if (room.PlayersReady.Count >= room.Players.Count)
                {
                    var summary = new List<string>();
                    var enemies = _roomEnemies.ContainsKey(roomName) ? _roomEnemies[roomName] : new List<ActiveEnemy>();
                    bool allDead = enemies.Count > 0 && enemies.All(e => e.Hp <= 0);

                    foreach (var p in room.Players)
                    {
                        int hp = p.Hp; int block = p.Block;
                        ProcessEffectsAtEndOfTurn(p.Effects, ref hp, ref block, summary, p.Name);
                        p.Hp = hp; if (p.Hp < 0) p.Hp = 0; if (p.Hp > p.MaxHp) p.Hp = p.MaxHp;
                    }

                    if (!allDead && enemies.Count > 0)
                    {
                        summary.Add($"--- TAH NEPŘÁTEL ---");
                        foreach (var enemy in enemies.Where(e => e.Hp > 0))
                        {
                            int eHp = enemy.Hp; int eBlock = 0;
                            ProcessEffectsAtEndOfTurn(enemy.Effects, ref eHp, ref eBlock, summary, enemy.Name);
                            enemy.Hp = eHp; if (enemy.Hp < 0) enemy.Hp = 0;

                            if (enemy.Hp > 0)
                            {
                                var action = enemy.CurrentAction;
                                summary.Add($"{enemy.Name} provedl: {action.Name}!");
                                if (action.Heal > 0) { enemy.Hp += action.Heal; if (enemy.Hp > enemy.MaxHp) enemy.Hp = enemy.MaxHp; }
                                
                                if (action.DamageToAll > 0)
                                {
                                    int enemyBaseDmg = action.DamageToAll;
                                    if (enemy.Effects.ContainsKey(EffectType.Weak) && enemy.Effects[EffectType.Weak] > 0)
                                        enemyBaseDmg = (int)(enemyBaseDmg * 0.75);

                                    foreach (var p in room.Players)
                                    {
                                        int finalDmg = enemyBaseDmg;
                                        if (p.Effects.ContainsKey(EffectType.Vulnerable) && p.Effects[EffectType.Vulnerable] > 0)
                                            finalDmg = (int)(finalDmg * 1.5);

                                        int dmgTaken = Math.Max(0, finalDmg - p.Block);
                                        p.Hp -= dmgTaken;
                                        if (p.Hp < 0) p.Hp = 0; 
                                    }
                                }
                                enemy.CurrentAction = EnemyDatabase.GetRandomActionForEnemy(enemy.TemplateName);
                            }
                            else 
                            {
                                // Pokud zemřel na JED nebo OHEŇ na konci tahu, aplikuj Relikvie všem
                                foreach (var p in room.Players) RelicManager.OnEnemyKilled(p, room);
                            }
                        }
                    }

                    allDead = enemies.Count > 0 && enemies.All(e => e.Hp <= 0);

                    room.PlayersReady.Clear();
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, 0, room.CurrentKarma, enemies);
                    
                    if (allDead && enemies.Count > 0)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            Random rng = new Random();
                            
                            foreach (var p in room.Players)
                            {
                                RelicManager.ApplyCombatEndRelics(p, room);
                            }

                            int goldReward = 0;
                            if (currentNode.Type == NodeType.Encounter) goldReward = rng.Next(15, 31);
                            else if (currentNode.Type == NodeType.EliteEncounter) goldReward = rng.Next(40, 71);
                            else if (currentNode.Type == NodeType.Boss) goldReward = rng.Next(100, 151);

                            List<string> offeredRelicsThisTurn = new List<string>();

                            foreach (var p in room.Players)
                            {
                                p.Gold += goldReward;
                                
                                string classPrefix = GetPrefixForClass(p.HeroClass);
                                var classSpecificCards = CardDatabase.Cards.Values
                                    .Where(c => c.Id.StartsWith(classPrefix) || c.Id.StartsWith("Z_"))
                                    .ToList();

                                if (classSpecificCards.Count < 3) classSpecificCards = CardDatabase.Cards.Values.ToList();

                                var cardChoices = classSpecificCards.OrderBy(x => rng.Next()).Take(3).ToList();

                                Relic? relicChoice = null; 
                                if (currentNode.Type == NodeType.EliteEncounter || currentNode.Type == NodeType.Boss)
                                {
                                    var availableRelics = RelicDatabase.LootRelics
                                        .Where(r => !room.TeamRelics.Any(tr => tr.Id == r.Id) && !offeredRelicsThisTurn.Contains(r.Id))
                                        .ToList();
                                        
                                    if (availableRelics.Count > 0)
                                    {
                                        relicChoice = availableRelics[rng.Next(availableRelics.Count)];
                                        offeredRelicsThisTurn.Add(relicChoice.Id);
                                    }
                                }
                                await Clients.Client(p.ConnectionId).SendAsync("ShowRewardScreen", cardChoices, relicChoice, goldReward, p.Gold);
                            }

                            if (currentNode.Type == NodeType.Boss)
                            {
                                if (room.CurrentAct == 1 || room.CurrentAct == 2)
                                {
                                    room.CurrentAct++;
                                    foreach(var node in room.Map) { node.IsCompleted = false; }
                                    room.CurrentNodeId = -1; 
                                    await Clients.Group(roomName).SendAsync("GameStarted", roomName, room.Map); 
                                }
                                else
                                {
                                    await Clients.Group(roomName).SendAsync("GameOver", "🏆 Porazili jste finálního Bosse! Zachránili jste vesmír a vyhráli celou hru!");
                                    _activeRooms.TryRemove(roomName, out _);
                                    _roomEnemies.TryRemove(roomName, out _);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var p in room.Players)
                        {
                            p.Mana = p.MaxMana;
                            p.Block = 0; 
                            p.CardsPlayedThisTurn = 0; // ZMĚNA: Reset počítadla karet pro relikvie
                            
                            RelicManager.ApplyTurnStartRelics(p, room);

                            if (room.CurrentKarma >= 10) p.Block += 3;
                            else if (room.CurrentKarma <= -10) { p.Hp -= 1; if (p.Hp < 0) p.Hp = 0; }
                            
                            p.DiscardPile.AddRange(p.Hand);
                            p.Hand.Clear();
                            p.DrawCards(5);

                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, enemies);
                        }
                    }
                }
            }
        }

        private string GetPrefixForClass(string heroClass)
        {
            switch (heroClass)
            {
                case "Paladin": return "P_";
                case "Warlock": return "W_";
                case "Monk": return "M_";
                case "Berserker": return "B_";
                case "Druid": return "D_";
                case "Rogue": return "R_";
                case "Bard": return "Bd_";
                case "Pyromancer": return "Py_";
                default: return "Z_"; 
            }
        }

        public async Task ClaimReward(string roomName, string playerName, string cardId, string relicId, string relicName, string relicDesc)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null)
                {
                    if (!string.IsNullOrEmpty(cardId))
                    {
                        player.StartingDeck.Add(cardId);
                        player.DrawPile.Add(cardId); 
                    }

                    if (!string.IsNullOrEmpty(relicId))
                    {
                        if (!room.TeamRelics.Any(r => r.Id == relicId)) 
                        {
                            room.TeamRelics.Add(new Relic(relicId, $"{relicName} ({player.Name})", relicDesc));
                            await Clients.Group(roomName).SendAsync("UpdateRelics", room.TeamRelics.ToList());
                        }
                    }

                    await Clients.Client(player.ConnectionId).SendAsync("RewardClaimed", player.StartingDeck);
                }
            }
        }
    }
}