using Microsoft.AspNetCore.SignalR;
using RoguelikeCardGame.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; 
using System;

namespace RoguelikeCardGame.Hubs
{
    public class GameHub : Hub
    {
        // --------------------------------------------------------
        // GLOBÁLNÍ STAV SERVERU
        // --------------------------------------------------------
        private static ConcurrentDictionary<string, GameRoom> _activeRooms = new ConcurrentDictionary<string, GameRoom>();

        private object GetTeamStats(GameRoom room)
        {
            return room.Players.Select(p => new { name = p.Name, hp = p.Hp, maxHp = p.MaxHp, block = p.Block, heroClass = p.HeroClass }).ToList();
        }

        // --------------------------------------------------------
        // LOBBY A PŘIPOJOVÁNÍ
        // --------------------------------------------------------
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
                        room.StopBattle(); // Zastavení 3D timeru
                        _activeRooms.TryRemove(roomKvp.Key, out _);
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
            
            var newRoom = new GameRoom(roomName);
            // ZDE PROPOJUJEME SERVER S 3D SMYČKOU Z GAMEROOMU
            newRoom.OnTickUpdate += async (room) => await Broadcast3DState(room);
            newRoom.OnEnemyAttack += async (room, enemy) => await HandleEnemyRealTimeAttack(room, enemy);
            
            _activeRooms.TryAdd(roomName, newRoom);
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

        // --------------------------------------------------------
        // START HRY A POSTUP MAPOU
        // --------------------------------------------------------
        public async Task StartGame(string roomName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var combinedCards = new Dictionary<string, CardTemplate>(CardDatabase.Cards);
                foreach (var kvp in UpgradedCardsDatabase.UpgradedCards) combinedCards[kvp.Key] = kvp.Value;

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
                
                // Před bojem vyčistíme efekty a lízneme
                foreach (var p in room.Players)
                {
                    p.Mana = p.MaxMana;
                    p.Block = 0;
                    p.Effects.Clear(); 
                    p.DiscardPile.AddRange(p.Hand);
                    p.Hand.Clear();
                    p.DrawCards(5);
                    p.CardsPlayedThisTurn = 0;
                }

                // BOJ (Encounter / Elite / Boss)
                if (targetNode.Type == NodeType.Encounter || targetNode.Type == NodeType.EliteEncounter || targetNode.Type == NodeType.Boss)
                {
                    Random rng = new Random();
                    int enemyCount = targetNode.Type == NodeType.Encounter ? rng.Next(1, 5) : rng.Next(1, 3);
                    string tier = targetNode.Type == NodeType.Boss ? "Boss" : (targetNode.Type == NodeType.EliteEncounter ? "Elite" : "Normal");

                    room.ActiveEnemies.Clear();
                    for (int i = 0; i < enemyCount; i++)
                    {
                        var template = EnemyDatabase.GetRandomEnemy(tier, room.CurrentAct);
                        room.ActiveEnemies.Add(new ActiveEnemy {
                            Name = template.Name + (enemyCount > 1 ? $" {i + 1}" : ""),
                            TemplateName = template.Name, MaxHp = template.MaxHp, Hp = template.MaxHp,
                            CurrentAction = EnemyDatabase.GetRandomActionForEnemy(template.Name),
                            AttackCooldown = template.AttackCooldown,
                            CurrentCooldown = template.AttackCooldown + rng.Next(-500, 500) // Drobný rozptyl
                        });
                    }

                    // Aplikace startovních relikvií
                    foreach (var p in room.Players)
                    {
                        RelicManager.ApplyCombatStartRelics(p, room, room.ActiveEnemies);
                        RelicManager.ApplyTurnStartRelics(p, room); // Start tahu pro pasivní manu atd.
                        await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, room.ActiveEnemies);
                    }
                    
                    // 3D SPAWN - Rozestavení do arény a start timeru
                    room.Initialize3DArena();
                    room.StartBattle();

                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, room.ActiveEnemies);
                }
                // NEBOJOVÉ MÍSTNOSTI
                else 
                {
                    room.StopBattle(); // Pokud jsme v obchodě, 3D timer neběží
                    targetNode.IsCompleted = true;
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());

                    if (targetNode.Type == NodeType.Shop)
                    {
                        foreach (var p in room.Players)
                        {
                            var playerShop = ShopManager.GenerateShopForPlayer(p);
                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                            await Clients.Client(p.ConnectionId).SendAsync("EnterShop", playerShop.Cards, playerShop.Relics, playerShop.RemoveCost); 
                        }
                    }
                    else if (targetNode.Type == NodeType.Event)
                    {
                        var randomEvent = EventDatabase.GetRandomEvent();
                        foreach (var p in room.Players) await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                        await Clients.Group(roomName).SendAsync("EnterEvent", randomEvent);
                    }
                    else if (targetNode.Type == NodeType.RestPlace)
                    {
                        foreach (var p in room.Players)
                        {
                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                            await Clients.Client(p.ConnectionId).SendAsync("EnterRestPlace");
                        }
                    }
                    else if (targetNode.Type == NodeType.Treasure)
                    {
                        List<string> offeredRelicsThisTurn = new List<string>();
                        foreach (var p in room.Players)
                        {
                            var treasureLoot = TreasureManager.GenerateTreasureForPlayer(p);
                            p.Gold += treasureLoot.Gold;
                            var availableRelics = RelicDatabase.LootRelics.Where(r => !room.TeamRelics.Any(tr => tr.Id == r.Id) && !offeredRelicsThisTurn.Contains(r.Id)).ToList();
                            
                            Relic? relicChoice = null;
                            if (availableRelics.Count > 0) { relicChoice = availableRelics[new Random().Next(availableRelics.Count)]; offeredRelicsThisTurn.Add(relicChoice.Id); }

                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile, p.DiscardPile, p.Hp, p.MaxHp, p.Block, new List<ActiveEnemy>());
                            await Clients.Client(p.ConnectionId).SendAsync("ShowRewardScreen", treasureLoot.Cards, relicChoice, treasureLoot.Gold, p.Gold);
                        }
                    }
                }
                await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
            }
        }

        // --------------------------------------------------------
        // 3D REAL-TIME AKCE (Nové metody pro Three.js)
        // --------------------------------------------------------
        
        // Frontend volá toto, když hráč mačká WASD
        public async Task MovePlayer(string roomName, string playerName, float x, float y, float z)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null && player.Hp > 0)
                {
                    player.X = x; player.Y = y; player.Z = z;
                }
            }
        }

        // Timer z GameRoomu zavolá tuto metodu každých 100ms
        private async Task Broadcast3DState(GameRoom room)
        {
            var playerData = room.Players.Select(p => new { name = p.Name, x = p.X, y = p.Y, z = p.Z, hp = p.Hp, mana = p.Mana }).ToList();
            var enemyData = room.ActiveEnemies.Where(e => e.Hp > 0).Select(e => new { id = e.Id, x = e.X, y = e.Y, z = e.Z, hp = e.Hp }).ToList();

            // Pošleme surová data o pozicích a maně do Three.js
            await Clients.Group(room.RoomName).SendAsync("Update3DState", playerData, enemyData);
        }

        // ZDE STŘÍLÍŠ KARTY V REÁLNÉM ČASE (Bývalý SelectCard)
        public async Task CastCard(string roomName, string playerName, string cardId, int karmaShift, string targetEnemyId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null && player.Hp > 0 && player.Hand.Contains(cardId))
                {
                    CardTemplate fullCard = GetCardData(cardId);
                    if (fullCard == null || player.Mana < fullCard.Cost) return; // Nemá manu

                    // Odečtení many, zahození karty
                    player.Mana -= fullCard.Cost;
                    player.Hand.Remove(cardId);
                    player.DiscardPile.Add(cardId);
                    player.CardsPlayedThisTurn++;
                    room.CurrentKarma += karmaShift;

                    RelicManager.ApplyCardPlayedRelics(player, room, fullCard);

                    // 1. Self efekty a Blok
                    foreach(var eff in fullCard.SelfEffects) player.AddEffect(eff.Type, eff.Amount);
                    int dex = player.Effects.ContainsKey(EffectType.Dexterity) ? player.Effects[EffectType.Dexterity] : 0;
                    player.Block += RelicManager.ModifyBlock(fullCard.Block > 0 ? fullCard.Block + dex : 0, player, room);

                    // 2. Útok a dosah (3D vzdálenost)
                    if (fullCard.Damage > 0 || fullCard.TargetEffects.Count > 0)
                    {
                        var target = room.ActiveEnemies.FirstOrDefault(e => e.Id == targetEnemyId && e.Hp > 0);
                        if (target != null)
                        {
                            // FYZIKA: Výpočet vzdálenosti hráče a nestvůry (Pythagorova věta ve 3D)
                            float distance = (float)Math.Sqrt(Math.Pow(target.X - player.X, 2) + Math.Pow(target.Y - player.Y, 2) + Math.Pow(target.Z - player.Z, 2));
                            
                            // Pokud je karta na blízko a stojíš daleko, zrušíme útok (Miss)
                            float cardRange = fullCard.Damage > 10 ? 3.0f : 15.0f; // Silné rány na blízko, slabší na dálku (příklad)

                            if (distance <= cardRange)
                            {
                                foreach(var eff in fullCard.TargetEffects) target.AddEffect(eff.Type, eff.Amount);
                                
                                if (fullCard.Damage > 0)
                                {
                                    int str = player.Effects.ContainsKey(EffectType.Strength) ? player.Effects[EffectType.Strength] : 0;
                                    int actualDamage = RelicManager.ModifyDamage(fullCard.Damage + str, player, room);
                                    
                                    // Vliv karmy
                                    if (room.CurrentKarma >= 10) actualDamage = (int)(actualDamage * 0.8);
                                    else if (room.CurrentKarma <= -10) actualDamage = (int)(actualDamage * 1.3);

                                    target.Hp -= actualDamage;

                                    if (target.Hp <= 0)
                                    {
                                        target.Hp = 0;
                                        RelicManager.OnEnemyKilled(player, room);
                                        await CheckEndBattle(room);
                                    }
                                    
                                    // Pošleme do 3D zprávu, ať se tam vykreslí zásah/výbuch
                                    await Clients.Group(roomName).SendAsync("SpawnHitEffect", target.X, target.Y, target.Z, actualDamage);
                                }
                            }
                            else
                            {
                                // Hráč kouzlil do vzduchu (miss)
                                await Clients.Client(player.ConnectionId).SendAsync("ShowActionMessage", "Jsi příliš daleko!");
                            }
                        }
                    }

                    // 3. Lízání nových karet
                    if (fullCard.DrawCards > 0) player.DrawCards(fullCard.DrawCards);

                    // Poslat update UI hráči
                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveNewTurnState", player.Hand, player.Mana, player.Gold, player.DrawPile, player.DiscardPile, player.Hp, player.MaxHp, player.Block, room.ActiveEnemies);
                    await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                }
            }
        }

        // Timer z GameRoomu zavolá toto, když monstrum dosáhne cooldownu
        private async Task HandleEnemyRealTimeAttack(GameRoom room, ActiveEnemy enemy)
        {
            var action = enemy.CurrentAction;
            
            // Monstrum si vybere náhodného živého hráče jako cíl
            var alivePlayers = room.Players.Where(p => p.Hp > 0).ToList();
            if (alivePlayers.Count == 0) return;
            var targetPlayer = alivePlayers[new Random().Next(alivePlayers.Count)];

            // FYZIKA: Dosáhne monstrum na hráče?
            var template = EnemyDatabase.Enemies.FirstOrDefault(e => e.Name == enemy.TemplateName);
            float range = template != null ? template.AttackRange : 2.0f;
            float distance = (float)Math.Sqrt(Math.Pow(targetPlayer.X - enemy.X, 2) + Math.Pow(targetPlayer.Y - enemy.Y, 2) + Math.Pow(targetPlayer.Z - enemy.Z, 2));

            if (distance <= range)
            {
                if (action.DamageToAll > 0)
                {
                    int dmg = Math.Max(0, action.DamageToAll - targetPlayer.Block);
                    targetPlayer.Hp -= dmg;
                    if (targetPlayer.Hp <= 0) targetPlayer.Hp = 0; // Smrt hráče
                    
                    await Clients.Group(room.RoomName).SendAsync("SpawnHitEffect", targetPlayer.X, targetPlayer.Y, targetPlayer.Z, dmg);
                    await Clients.Group(room.RoomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                }
            }
            else
            {
                // Pokud je monstrum daleko, mělo by k němu jít (pohyb monster dořešíme ve frontend Three.js nebo v backend updatu pozic)
            }

            enemy.CurrentAction = EnemyDatabase.GetRandomActionForEnemy(enemy.TemplateName);
        }

        // Kontrola, zda boj neskončil
        private async Task CheckEndBattle(GameRoom room)
        {
            bool allDead = room.ActiveEnemies.All(e => e.Hp <= 0);
            if (allDead)
            {
                room.StopBattle();
                
                var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                if (currentNode != null)
                {
                    currentNode.IsCompleted = true;
                    Random rng = new Random();
                    
                    int goldReward = currentNode.Type == NodeType.Encounter ? rng.Next(15, 31) : (currentNode.Type == NodeType.EliteEncounter ? rng.Next(40, 71) : rng.Next(100, 151));

                    foreach (var p in room.Players)
                    {
                        RelicManager.ApplyCombatEndRelics(p, room);
                        p.Gold += goldReward;

                        // Výběr karet pro odměnu
                        string classPrefix = GetPrefixForClass(p.HeroClass);
                        var classSpecificCards = CardDatabase.Cards.Values.Where(c => c.Id.StartsWith(classPrefix) || c.Id.StartsWith("Z_")).ToList();
                        if (classSpecificCards.Count < 3) classSpecificCards = CardDatabase.Cards.Values.ToList();
                        var cardChoices = classSpecificCards.OrderBy(x => rng.Next()).Take(3).ToList();

                        await Clients.Client(p.ConnectionId).SendAsync("ShowRewardScreen", cardChoices, null, goldReward, p.Gold);
                    }

                    if (currentNode.Type == NodeType.Boss)
                    {
                        if (room.CurrentAct == 1 || room.CurrentAct == 2)
                        {
                            room.CurrentAct++;
                            foreach(var node in room.Map) node.IsCompleted = false;
                            room.CurrentNodeId = -1; 
                            await Clients.Group(room.RoomName).SendAsync("GameStarted", room.RoomName, room.Map); 
                        }
                        else
                        {
                            await Clients.Group(room.RoomName).SendAsync("GameOver", "🏆 Porazili jste finálního Bosse! Zachránili jste vesmír ve 3D!");
                            _activeRooms.TryRemove(room.RoomName, out _);
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------
        // POMOCNÉ FUNKCE (Nezměněno, pouze přesunuto)
        // --------------------------------------------------------
        private CardTemplate GetCardData(string cardId)
        {
            if (cardId.EndsWith("+") && UpgradedCardsDatabase.UpgradedCards.TryGetValue(cardId, out var upgCard)) return upgCard;
            string baseCardId = cardId.EndsWith("+") ? cardId.Substring(0, cardId.Length - 1) : cardId;
            if (CardDatabase.Cards.TryGetValue(baseCardId, out var baseCard)) return baseCard;
            return null;
        }

        private string GetPrefixForClass(string heroClass)
        {
            switch (heroClass) { case "Paladin": return "P_"; case "Warlock": return "W_"; case "Monk": return "M_"; case "Berserker": return "B_"; case "Druid": return "D_"; case "Rogue": return "R_"; case "Bard": return "Bd_"; case "Pyromancer": return "Py_"; default: return "Z_"; }
        }
        
        // Nechat pro klid duše metody pro Shop a Event (zůstaly stejné)
        public async Task RestPlaceAction(string roomName, string playerName, string actionType, string cardId) { /* ... zkráceno pro přehlednost, doplň si z minula ... */ }
        public async Task BuyShopItem(string roomName, string playerName, string itemId, string type, int price) { /* ... */ }
        public async Task RemoveCardFromDeck(string roomName, string playerName, string cardId, int price) { /* ... */ }
        public async Task ResolveEventOption(string roomName, string playerName, string eventId, int optionId) { /* ... */ }
        public async Task ClaimReward(string roomName, string playerName, string cardId, string relicId, string relicName, string relicDesc) { /* ... */ }
    }
}