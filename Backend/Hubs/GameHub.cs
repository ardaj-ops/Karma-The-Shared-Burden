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
        private static ConcurrentDictionary<string, GameRoom> _activeRooms = new ConcurrentDictionary<string, GameRoom>();

        // --- AUTOMATICKÉ MAZÁNÍ MÍSTNOSTI PŘI ODPOJENÍ ---
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
            if (_activeRooms.ContainsKey(roomName))
            {
                await Clients.Caller.SendAsync("LobbyError", "Místnost s tímto názvem už existuje!");
                return;
            }

            var newRoom = new GameRoom(roomName);
            _activeRooms.TryAdd(roomName, newRoom);

            await JoinLobby(roomName, playerName, heroClass);
        }

        public async Task JoinLobby(string roomName, string playerName, string heroClass)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                if (room.Players.Count >= 5)
                {
                    await Clients.Caller.SendAsync("LobbyError", "Místnost je plná (max 5 hráčů)!");
                    return;
                }

                var newPlayer = new Player(Context.ConnectionId, playerName);
                newPlayer.HeroClass = heroClass;

                if (HeroDatabase.Heroes.TryGetValue(heroClass, out var template))
                {
                    newPlayer.MaxHp = template.MaxHp;
                    newPlayer.StartingDeck = new List<string>(template.StartingDeck); 
                }

                room.Players.Add(newPlayer);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                
                var playerNames = room.Players.Select(p => p.Name).ToList();
                await Clients.Group(roomName).SendAsync("LobbyUpdate", playerNames);

                if (room.Players.Count == 1)
                {
                    await Clients.Caller.SendAsync("YouAreHost");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("LobbyError", "Místnost nenalezena!");
            }
        }

        public async Task StartGame(string roomName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                foreach (var player in room.Players)
                {
                    player.InitializeGame(); 
                    
                    // Na startu líže jen do 3 karet
                    player.DrawCards(3); 

                    if (HeroDatabase.Heroes.TryGetValue(player.HeroClass, out var template) && template.StartingRelic != null)
                    {
                        var myRelic = new Relic(
                            template.StartingRelic.Id, 
                            $"{template.StartingRelic.Name} ({player.Name})", 
                            template.StartingRelic.Description
                        );
                        room.TeamRelics.Add(myRelic);
                    }

                    // Odesíláme komplet data
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

                    // Generování nepřátel
                    if (nextNode.Type == NodeType.Encounter) 
                    { 
                        room.EnemyName = "Kultista"; 
                        room.EnemyMaxHp = 60; 
                        room.EnemyHp = 60; 
                    }
                    else if (nextNode.Type == NodeType.EliteEncounter) 
                    { 
                        room.EnemyName = "Temný Rytíř"; 
                        room.EnemyMaxHp = 150; 
                        room.EnemyHp = 150; 
                    }
                    else if (nextNode.Type == NodeType.Boss) 
                    { 
                        room.EnemyName = "Pán Karmy"; 
                        room.EnemyMaxHp = 350; 
                        room.EnemyHp = 350; 
                    }

                    await Clients.Group(roomName).SendAsync("EnteredNode", nextNode.Type.ToString(), nextNode, room.EnemyName, room.EnemyHp, room.EnemyMaxHp);
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
                
                room.PlayedCardsThisTurn.AddOrUpdate(playerName, 
                    new List<CardPlayData> { cardData }, 
                    (key, existingList) => { existingList.Add(cardData); return existingList; });

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

                            // Přičtení štítů a životů z karet
                            if (player != null && CardDatabase.Cards.TryGetValue(card.CardId, out var fullCard))
                            {
                                player.Block += fullCard.Block;
                                player.Hp += fullCard.Heal;
                                if (player.Hp > player.MaxHp) player.Hp = player.MaxHp;
                            }
                        }
                    }

                    // --- APLIKACE KARMY NA POŠKOZENÍ ---
                    double damageMultiplier = 1.0;
                    if (room.CurrentKarma >= 10) 
                    {
                        damageMultiplier = 0.8; // Čisté Světlo: -20 % poškození
                        summary.Add("Čisté Světlo snižuje vaše poškození o 20 %.");
                    }
                    else if (room.CurrentKarma <= -10)
                    {
                        damageMultiplier = 1.3; // Hluboká Temnota: +30 % poškození
                        summary.Add("Hluboká Temnota zvyšuje vaše poškození o 30 %!");
                    }

                    int actualDamage = (int)Math.Round(baseTotalDamage * damageMultiplier);

                    room.EnemyHp -= actualDamage;
                    room.CurrentKarma += totalKarmaShift;
                    
                    room.PlayedCardsThisTurn.Clear();
                    room.PlayersReady.Clear();

                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, actualDamage, room.CurrentKarma, room.EnemyHp);
                    
                    if (room.EnemyHp <= 0)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            if (currentNode.Type == NodeType.Boss)
                            {
                                await Clients.Group(roomName).SendAsync("GameOver", "Vítězství! Pán Karmy padl, kampaň je dokončena!");
                                _activeRooms.TryRemove(roomName, out _);
                            }
                            else
                            {
                                await Clients.Group(roomName).SendAsync("BattleWon", "Nepřítel poražen! Vyberte další cestu na mapě.");
                            }
                        }
                    }
                    else
                    {
                        foreach (var p in room.Players)
                        {
                            p.Mana = p.MaxMana;
                            p.Block = 0; // Štíty padají na začátku nového tahu

                            // --- APLIKACE KARMY NA ZAČÁTKU TAHU ---
                            if (room.CurrentKarma >= 10)
                            {
                                p.Block += 3; // Čisté Světlo dává štíty
                            }
                            else if (room.CurrentKarma <= -10)
                            {
                                p.Hp -= 1; // Hluboká Temnota vysává život
                                if (p.Hp < 0) p.Hp = 0;
                            }
                            
                            // Dobírání přesně do 3 karet
                            int cardsToDraw = Math.Max(0, 3 - p.Hand.Count);
                            p.DrawCards(cardsToDraw);

                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", 
                                p.Hand, p.Mana, p.Gold, p.DrawPile.Count, p.DiscardPile.Count, p.Hp, p.MaxHp, p.Block);
                        }
                    }
                }
            }
        }
    }
}