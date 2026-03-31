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

        // Sběr dat o týmu pro vykreslování UI
        private object GetTeamStats(GameRoom room)
        {
            return room.Players.Select(p => new {
                name = p.Name,
                hp = p.Hp,
                maxHp = p.MaxHp,
                block = p.Block
            }).ToList();
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
                        var playerNames = room.Players.Select(p => p.Name).ToList();
                        await Clients.Group(roomKvp.Key).SendAsync("LobbyUpdate", playerNames);
                    }
                    break;
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateLobby(string roomName, string playerName, string heroClass)
        {
            if (_activeRooms.ContainsKey(roomName)) { await Clients.Caller.SendAsync("LobbyError", "Místnost s tímto názvem už existuje!"); return; }
            _activeRooms.TryAdd(roomName, new GameRoom(roomName));
            await JoinLobby(roomName, playerName, heroClass);
        }

        public async Task JoinLobby(string roomName, string playerName, string heroClass)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                if (room.Players.Count >= 5) { await Clients.Caller.SendAsync("LobbyError", "Místnost je plná (max 5 hráčů)!"); return; }

                var newPlayer = new Player(Context.ConnectionId, playerName) { HeroClass = heroClass };
                if (HeroDatabase.Heroes.TryGetValue(heroClass, out var template))
                {
                    newPlayer.MaxHp = template.MaxHp;
                    newPlayer.StartingDeck = new List<string>(template.StartingDeck); 
                }

                room.Players.Add(newPlayer);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                
                var playerNames = room.Players.Select(p => p.Name).ToList();
                await Clients.Group(roomName).SendAsync("LobbyUpdate", playerNames);
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
                    player.InitializeGame(); 
                    // OPRAVA: Startujeme s 5 kartami
                    player.DrawCards(5); 

                    if (HeroDatabase.Heroes.TryGetValue(player.HeroClass, out var template) && template.StartingRelic != null)
                    {
                        var myRelic = new Relic(template.StartingRelic.Id, $"{template.StartingRelic.Name} ({player.Name})", template.StartingRelic.Description);
                        room.TeamRelics.Add(myRelic);
                    }

                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveInitialState", 
                        player.Hand, player.Mana, CardDatabase.Cards, player.Gold, 
                        player.DrawPile.Count, player.DiscardPile.Count, player.Hp, player.MaxHp, player.Block);
                }

                await Clients.Group(roomName).SendAsync("UpdateRelics", room.TeamRelics.ToList());
                await Clients.Group(roomName).SendAsync("UpdateTeamStats", GetTeamStats(room));
                await Clients.Group(roomName).SendAsync("GameStarted", roomName, room.Map);
            }
        }

        // --- PŘESUN NA MAPĚ (OŠETŘENÍ CESTY A NÁHODNÍ NEPŘÁTELÉ) ---
        public async Task MoveToNextNode(string roomName, int nodeId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var targetNode = room.Map.FirstOrDefault(n => n.Id == nodeId);
                if (targetNode == null) return;

                // --- 1. KONTROLA, ZDA JE UZEL DOSTUPNÝ ---
                if (room.CurrentNodeId == -1) 
                {
                    // První krok - musí být v 0. patře
                    if (targetNode.Floor != 0) return; 
                }
                else
                {
                    // Další kroky - musí být propojený z aktuálního
                    var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                    if (currentNode == null || !currentNode.ConnectedTo.Contains(nodeId)) return;
                }

                room.CurrentNodeId = nodeId;
                var enemyList = new List<ActiveEnemy>();
                
                // --- 2. GENEROVÁNÍ LOKACE ---
                if (targetNode.Type == NodeType.Encounter || targetNode.Type == NodeType.EliteEncounter || targetNode.Type == NodeType.Boss)
                {
                    Random rng = new Random();
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
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, enemyList);
                }
                else
                {
                    // RestPlace, Shop, Treasure, Event
                    targetNode.IsCompleted = true; // Rovnou označíme za splněné, protože tu ještě není minihra
                    await Clients.Group(roomName).SendAsync("EnteredNode", targetNode.Type.ToString(), targetNode, new List<ActiveEnemy>());
                }
            }
        }

        public async Task SelectCard(string roomName, string playerName, string cardId, int karmaShift, int damage)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null && player.Hand.Contains(cardId))
                {
                    player.Hand.Remove(cardId);
                    player.DiscardPile.Add(cardId);
                }

                var cardData = new CardPlayData { CardId = cardId, KarmaShift = karmaShift, Damage = damage };
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
                    int baseTotalDamage = 0;
                    int totalKarmaShift = 0;
                    var summary = new List<string>();

                    foreach (var kvp in room.PlayedCardsThisTurn)
                    {
                        var player = room.Players.FirstOrDefault(p => p.Name == kvp.Key);
                        foreach(var card in kvp.Value)
                        {
                            baseTotalDamage += card.Damage;
                            totalKarmaShift += card.KarmaShift;
                            summary.Add($"{kvp.Key} zahrál {card.CardId}");

                            if (player != null && CardDatabase.Cards.TryGetValue(card.CardId, out var fullCard))
                            {
                                player.Block += fullCard.Block;
                                player.Hp += fullCard.Heal;
                                if (player.Hp > player.MaxHp) player.Hp = player.MaxHp;
                            }
                        }
                    }

                    double damageMultiplier = 1.0;
                    if (room.CurrentKarma >= 10) { damageMultiplier = 0.8; summary.Add("Čisté Světlo snižuje poškození o 20 %."); }
                    else if (room.CurrentKarma <= -10) { damageMultiplier = 1.3; summary.Add("Hluboká Temnota zvyšuje poškození o 30 %!"); }

                    int actualDamage = (int)Math.Round(baseTotalDamage * damageMultiplier);
                    room.CurrentKarma += totalKarmaShift;

                    var enemies = _roomEnemies.ContainsKey(roomName) ? _roomEnemies[roomName] : new List<ActiveEnemy>();
                    int remainingDamage = actualDamage;

                    foreach (var enemy in enemies.Where(e => e.Hp > 0))
                    {
                        if (remainingDamage <= 0) break;

                        if (enemy.Hp <= remainingDamage)
                        {
                            remainingDamage -= enemy.Hp;
                            enemy.Hp = 0;
                            summary.Add($"💀 {enemy.Name} byl poražen!");
                        }
                        else
                        {
                            enemy.Hp -= remainingDamage;
                            remainingDamage = 0;
                        }
                    }
                    
                    bool allDead = enemies.All(e => e.Hp <= 0);

                    if (!allDead)
                    {
                        summary.Add($"--- TAH NEPŘÁTEL ---");
                        foreach (var enemy in enemies.Where(e => e.Hp > 0))
                        {
                            var action = enemy.CurrentAction;
                            summary.Add($"{enemy.Name} provedl: {action.Name}!");
                            
                            if (action.Heal > 0)
                            {
                                enemy.Hp += action.Heal;
                                if (enemy.Hp > enemy.MaxHp) enemy.Hp = enemy.MaxHp;
                            }

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
                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, actualDamage, room.CurrentKarma, enemies);
                    
                    if (allDead)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            
                            Random rng = new Random();
                            var allCards = CardDatabase.Cards.Values.ToList();

                            foreach (var p in room.Players)
                            {
                                var cardChoices = allCards.OrderBy(x => rng.Next()).Take(3).ToList();
                                
                                Relic? relicChoice = null; 
                                if (currentNode.Type == NodeType.EliteEncounter || currentNode.Type == NodeType.Boss)
                                {
                                    relicChoice = RelicDatabase.GetRandomRelic();
                                }

                                await Clients.Client(p.ConnectionId).SendAsync("ShowRewardScreen", cardChoices, relicChoice);
                            }

                            if (currentNode.Type == NodeType.Boss)
                            {
                                if (room.CurrentAct == 1)
                                {
                                    room.CurrentAct = 2;
                                    foreach(var node in room.Map) { node.IsCompleted = false; }
                                    room.CurrentNodeId = -1; // -1 resetuje startovní patro
                                    await Clients.Group(roomName).SendAsync("GameStarted", roomName, room.Map); 
                                }
                                else if (room.CurrentAct == 2)
                                {
                                    room.CurrentAct = 3;
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
                            
                            // --- OPRAVA: Zahození zbylé ruky a dobití do 5 karet ---
                            p.DiscardPile.AddRange(p.Hand);
                            p.Hand.Clear();
                            p.DrawCards(5);

                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", 
                                p.Hand, p.Mana, p.Gold, p.DrawPile.Count, p.DiscardPile.Count, p.Hp, p.MaxHp, p.Block, enemies);
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

                    await Clients.Client(player.ConnectionId).SendAsync("RewardClaimed");
                }
            }
        }
    }
}