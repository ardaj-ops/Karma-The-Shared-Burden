using Microsoft.AspNetCore.SignalR;
using RoguelikeCardGame.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; 
using System;

namespace RoguelikeCardGame.Hubs
{
    // NOVÉ: Objekt reprezentující fyzického nepřítele v aktuální místnosti
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
        
        // Zde držíme seznam vygenerovaných nepřátel pro každou místnost
        private static ConcurrentDictionary<string, List<ActiveEnemy>> _roomEnemies = new ConcurrentDictionary<string, List<ActiveEnemy>>();

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
                    player.DrawCards(3); 

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
                await Clients.Group(roomName).SendAsync("GameStarted", roomName, room.Map);
            }
        }

        public async Task MoveToNextNode(string roomName, int nodeId)
        {
            if (_activeRooms.TryGetValue(roomName, out var room) && room != null)
            {
                var nextNode = room.Map.FirstOrDefault(n => n.Id == nodeId);
                if (nextNode != null)
                {
                    room.CurrentNodeId = nodeId;

                    // --- LOSOVÁNÍ SKUPINY NEPŘÁTEL ---
                    var enemyList = new List<ActiveEnemy>();
                    
                    if (nextNode.Type == NodeType.Encounter || nextNode.Type == NodeType.EliteEncounter || nextNode.Type == NodeType.Boss)
                    {
                        Random rng = new Random();
                        int enemyCount = 1;

                        if (nextNode.Type == NodeType.Encounter) enemyCount = rng.Next(1, 5); // 1-4 běžní nepřátelé
                        else if (nextNode.Type == NodeType.EliteEncounter) enemyCount = rng.Next(1, 3); // 1-2 elitní
                        
                        string tier = nextNode.Type == NodeType.Boss ? "Boss" : (nextNode.Type == NodeType.EliteEncounter ? "Elite" : "Normal");

                        for (int i = 0; i < enemyCount; i++)
                        {
                            var template = EnemyDatabase.GetRandomEnemy(tier, 1); // Bere z Act 1
                            var action = EnemyDatabase.GetRandomActionForEnemy(template.Name);
                            
                            enemyList.Add(new ActiveEnemy {
                                Name = template.Name + (enemyCount > 1 ? $" {i + 1}" : ""), // Přidá číslování, pokud je jich víc
                                TemplateName = template.Name,
                                MaxHp = template.MaxHp,
                                Hp = template.MaxHp,
                                CurrentAction = action
                            });
                        }

                        _roomEnemies[roomName] = enemyList;
                        
                        // Posíláme pole nepřátel do front-endu (který už na to má připravený renderEnemies)
                        await Clients.Group(roomName).SendAsync("EnteredNode", nextNode.Type.ToString(), nextNode, enemyList);
                    }
                    else
                    {
                        // Pokud stoupl na Poklad / Odpočinek
                        await Clients.Group(roomName).SendAsync("EnteredNode", nextNode.Type.ToString(), nextNode, new List<ActiveEnemy>());
                    }
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

                    // 1. Zpracování karet (Léčení, Štíty, Kumulace DMG a Karmy)
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

                    // 2. APLIKACE KARMY NA POŠKOZENÍ A STATUS
                    double damageMultiplier = 1.0;
                    if (room.CurrentKarma >= 10) { damageMultiplier = 0.8; summary.Add("Čisté Světlo snižuje poškození o 20 %."); }
                    else if (room.CurrentKarma <= -10) { damageMultiplier = 1.3; summary.Add("Hluboká Temnota zvyšuje poškození o 30 %!"); }

                    int actualDamage = (int)Math.Round(baseTotalDamage * damageMultiplier);
                    room.CurrentKarma += totalKarmaShift;

                    // 3. PRŮRAZNÉ POŠKOZENÍ (Cleave) DO SKUPINY NEPŘÁTEL
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

                    // 4. TAH PŘEŽIVŠÍCH NEPŘÁTEL
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
                            
                            // Vygenerování nové akce pro tohoto nepřítele
                            enemy.CurrentAction = EnemyDatabase.GetRandomActionForEnemy(enemy.TemplateName);
                        }
                    }

                    // Reset karet po tahu
                    room.PlayedCardsThisTurn.Clear();
                    room.PlayersReady.Clear();

                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, actualDamage, room.CurrentKarma, enemies);
                    
                    // 5. VYHODNOCENÍ KONCE BOJE NEBO PŘÍPRAVA NA DALŠÍ TAH
                    if (allDead)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            if (currentNode.Type == NodeType.Boss)
                            {
                                await Clients.Group(roomName).SendAsync("GameOver", "Vítězství! Boss padl, kampaň je dokončena!");
                                _activeRooms.TryRemove(roomName, out _);
                                _roomEnemies.TryRemove(roomName, out _);
                            }
                            else
                            {
                                await Clients.Group(roomName).SendAsync("BattleWon", $"Nepřátelé poraženi! Vyberte další cestu na mapě.");
                            }
                        }
                    }
                    else
                    {
                        foreach (var p in room.Players)
                        {
                            p.Mana = p.MaxMana;
                            p.Block = 0; // Štíty padají
                            
                            if (room.CurrentKarma >= 10) p.Block += 3;
                            else if (room.CurrentKarma <= -10) { p.Hp -= 1; if (p.Hp < 0) p.Hp = 0; }
                            
                            int cardsToDraw = Math.Max(0, 3 - p.Hand.Count);
                            p.DrawCards(cardsToDraw);

                            // Odeslání aktualizovaného stavu (včetně pole nepřátel)
                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", 
                                p.Hand, p.Mana, p.Gold, p.DrawPile.Count, p.DiscardPile.Count, p.Hp, p.MaxHp, p.Block, enemies);
                        }
                    }
                }
            }
        }
    }
}