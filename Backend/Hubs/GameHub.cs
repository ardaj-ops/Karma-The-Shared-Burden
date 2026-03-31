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
    }

    public class GameHub : Hub
    {
        private static ConcurrentDictionary<string, GameRoom> _activeRooms = new ConcurrentDictionary<string, GameRoom>();
        private static ConcurrentDictionary<string, List<ActiveEnemy>> _roomEnemies = new ConcurrentDictionary<string, List<ActiveEnemy>>();

        private object GetTeamStats(GameRoom room)
        {
            return room.Players.Select(p => new { name = p.Name, hp = p.Hp, maxHp = p.MaxHp, block = p.Block }).ToList();
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
                        await Clients.Group(roomKvp.Key).SendAsync("LobbyUpdate", room.Players.Select(p => p.Name).ToList());
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
                
                await Clients.Group(roomName).SendAsync("LobbyUpdate", room.Players.Select(p => p.Name).ToList());
                if (room.Players.Count == 1) await Clients.Caller.SendAsync("YouAreHost");
            }
            else { await Clients.Caller.SendAsync("LobbyError", "Místnost nenalezena!"); }
        }

        public async Task StartGame(string roomName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
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
                        player.Hand, player.Mana, CardDatabase.Cards, player.Gold, 
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
                    p.DiscardPile.AddRange(p.Hand);
                    p.Hand.Clear();
                    p.DrawCards(5);
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
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, enemyList);
                    }
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, enemyList);
                }
                else if (targetNode.Type == NodeType.Shop)
                {
                    targetNode.IsCompleted = true;
                    var allCards = CardDatabase.Cards.Values.ToList();
                    
                    foreach (var p in room.Players)
                    {
                        var shopCards = allCards.OrderBy(x => rng.Next()).Take(4).Select(c => new { id = c.Id, name = c.Name, desc = c.Description, cost = c.Cost, price = rng.Next(40, 80) }).ToList();
                        var shopRelics = RelicDatabase.LootRelics.OrderBy(x => rng.Next()).Take(2).Select(r => new { id = r.Id, name = r.Name, desc = r.Description, price = rng.Next(120, 200) }).ToList();
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                        await Clients.Client(p.ConnectionId).SendAsync("EnterShop", shopCards, shopRelics, 50); 
                    }
                }
                else if (targetNode.Type == NodeType.Event)
                {
                    targetNode.IsCompleted = true;
                    var eventData = new { 
                        title = "Záhadná svatyně", 
                        desc = "Uprostřed cesty nacházíte starou svatyni se zářícím oltářem. Z ní vyzařuje zvláštní, poklidná energie.", 
                        options = new[] { 
                            new { id = 1, text = "Pomodlit se (+10 Max HP)" }, 
                            new { id = 2, text = "Prohledat obětiny (+50 Zlaťáků, ale -10 HP)" },
                            new { id = 3, text = "Odejít bez povšimnutí" }
                        } 
                    };
                    
                    foreach (var p in room.Players)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                    }
                    await Clients.Group(roomName).SendAsync("EnterEvent", eventData);
                }
                else if (targetNode.Type == NodeType.RestPlace)
                {
                    targetNode.IsCompleted = true;
                    foreach (var p in room.Players)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                        await Clients.Client(p.ConnectionId).SendAsync("EnterRestPlace");
                    }
                }
                else
                {
                    targetNode.IsCompleted = true;
                    foreach (var p in room.Players)
                    {
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                    }
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());
                }
                await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
            }
        }

        // --- AKCE V TÁBOŘE (REST PLACE) ---
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

        public async Task ResolveEventOption(string roomName, string playerName, int optionId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var p = room.Players.FirstOrDefault(x => x.Name == playerName);
                if (p != null)
                {
                    if(optionId == 1) { p.MaxHp += 10; p.Hp += 10; }
                    if(optionId == 2) { p.Gold += 50; p.Hp -= 10; if(p.Hp < 1) p.Hp = 1; }
                    await Clients.Client(p.ConnectionId).SendAsync("EventResolved", p.Gold, p.Hp, p.MaxHp);
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                }
            }
        }

        public async Task SelectCard(string roomName, string playerName, string cardId, int karmaShift, int damage, string targetEnemyId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null && player.Hand.Contains(cardId))
                {
                    player.Hand.Remove(cardId);
                    player.DiscardPile.Add(cardId);
                }
                var cardData = new CardPlayData { CardId = cardId, KarmaShift = karmaShift, Damage = damage, TargetEnemyId = targetEnemyId };
                room.PlayedCardsThisTurn.AddOrUpdate(playerName, new List<CardPlayData> { cardData }, (key, existingList) => { existingList.Add(cardData); return existingList; });
                await Clients.Group(roomName).SendAsync("CardPlayedLog", playerName, cardId);
            }
        }

        public async Task PlayerReady(string roomName, string playerName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                room.PlayersReady.Add(playerName);
                await Clients.Group(roomName).SendAsync("PlayerReadyLog", playerName, room.PlayersReady.Count, room.Players.Count);

                if (room.PlayersReady.Count >= room.Players.Count)
                {
                    int totalKarmaShift = 0;
                    int totalDamageDealt = 0;
                    var summary = new List<string>();
                    var enemies = _roomEnemies.ContainsKey(roomName) ? _roomEnemies[roomName] : new List<ActiveEnemy>();

                    foreach (var kvp in room.PlayedCardsThisTurn)
                    {
                        var player = room.Players.FirstOrDefault(p => p.Name == kvp.Key);
                        foreach(var card in kvp.Value)
                        {
                            totalKarmaShift += card.KarmaShift;
                            summary.Add($"{kvp.Key} zahrál {card.CardId}");

                            // --- APLIKACE BONUSŮ Z VYLEPŠENÝCH KARET (Ty s + na konci) ---
                            string baseCardId = card.CardId.EndsWith("+") ? card.CardId.Substring(0, card.CardId.Length - 1) : card.CardId;
                            bool isUpgraded = card.CardId.EndsWith("+");

                            if (player != null && CardDatabase.Cards.TryGetValue(baseCardId, out var fullCard))
                            {
                                int block = fullCard.Block + (isUpgraded && fullCard.Block > 0 ? 3 : 0);
                                int heal = fullCard.Heal + (isUpgraded && fullCard.Heal > 0 ? 2 : 0);

                                player.Block += block;
                                player.Hp += heal;
                                if (player.Hp > player.MaxHp) player.Hp = player.MaxHp;
                            }

                            if (card.Damage > 0)
                            {
                                double damageMultiplier = 1.0;
                                if (room.CurrentKarma >= 10) damageMultiplier = 0.8;
                                else if (room.CurrentKarma <= -10) damageMultiplier = 1.3;

                                int actualDamage = (int)Math.Round(card.Damage * damageMultiplier);
                                
                                var target = enemies.FirstOrDefault(e => e.Id == card.TargetEnemyId && e.Hp > 0);
                                if (target == null) target = enemies.FirstOrDefault(e => e.Hp > 0);

                                if (target != null)
                                {
                                    target.Hp -= actualDamage;
                                    totalDamageDealt += actualDamage;
                                    summary.Add($"⚔️ {kvp.Key} zasáhl {target.Name} za {actualDamage} DMG.");
                                    if (target.Hp <= 0) { target.Hp = 0; summary.Add($"💀 {target.Name} byl poražen!"); }
                                }
                            }
                        }
                    }

                    room.CurrentKarma += totalKarmaShift;
                    bool allDead = enemies.Count > 0 && enemies.All(e => e.Hp <= 0);

                    if (!allDead && enemies.Count > 0)
                    {
                        summary.Add($"--- TAH NEPŘÁTEL ---");
                        foreach (var enemy in enemies.Where(e => e.Hp > 0))
                        {
                            var action = enemy.CurrentAction;
                            summary.Add($"{enemy.Name} provedl: {action.Name}!");
                            if (action.Heal > 0) { enemy.Hp += action.Heal; if (enemy.Hp > enemy.MaxHp) enemy.Hp = enemy.MaxHp; }
                            if (action.DamageToAll > 0)
                            {
                                foreach (var p in room.Players)
                                {
                                    int dmgTaken = Math.Max(0, action.DamageToAll - p.Block);
                                    p.Hp -= dmgTaken;
                                    if (p.Hp < 0) p.Hp = 0; 
                                }
                            }
                            enemy.CurrentAction = EnemyDatabase.GetRandomActionForEnemy(enemy.TemplateName);
                        }
                    }

                    room.PlayedCardsThisTurn.Clear();
                    room.PlayersReady.Clear();
                    
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, totalDamageDealt, room.CurrentKarma, enemies);
                    
                    if (allDead && enemies.Count > 0)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            
                            Random rng = new Random();
                            var allCards = CardDatabase.Cards.Values.ToList();
                            
                            // ŠKÁLOVÁNÍ ZLAŤÁKŮ PODLE OBTÍŽNOSTI BOJE
                            int goldReward = 0;
                            if (currentNode.Type == NodeType.Encounter) goldReward = rng.Next(15, 31);
                            else if (currentNode.Type == NodeType.EliteEncounter) goldReward = rng.Next(40, 71);
                            else if (currentNode.Type == NodeType.Boss) goldReward = rng.Next(100, 151);

                            foreach (var p in room.Players)
                            {
                                p.Gold += goldReward;
                                var cardChoices = allCards.OrderBy(x => rng.Next()).Take(3).ToList();
                                Relic? relicChoice = null; 
                                if (currentNode.Type == NodeType.EliteEncounter || currentNode.Type == NodeType.Boss)
                                {
                                    relicChoice = RelicDatabase.GetRandomRelic();
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
                        room.TeamRelics.Add(new Relic(relicId, $"{relicName} ({player.Name})", relicDesc));
                        await Clients.Group(roomName).SendAsync("UpdateRelics", room.TeamRelics.ToList());
                    }

                    await Clients.Client(player.ConnectionId).SendAsync("RewardClaimed", player.StartingDeck);
                }
            }
        }
    }
}