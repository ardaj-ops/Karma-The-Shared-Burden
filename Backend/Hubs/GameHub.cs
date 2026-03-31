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
                    player.DrawCards(5);

                    if (HeroDatabase.Heroes.TryGetValue(player.HeroClass, out var template) && template.StartingRelic != null)
                    {
                        room.TeamRelics.Add($"{template.StartingRelic.Name} (od {player.Name})");
                    }

                    // Posíláme peníze a velikost balíčků!
                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveInitialState", player.Hand, player.Mana, CardDatabase.Cards, player.Gold, player.DrawPile.Count, player.DiscardPile.Count);
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
                    await Clients.Group(roomName).SendAsync("EnteredNode", nextNode.Type.ToString(), nextNode);
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
                    int totalDamage = 0;
                    int totalKarmaShift = 0;
                    var summary = new List<string>();

                    foreach (var kvp in room.PlayedCardsThisTurn)
                    {
                        foreach(var card in kvp.Value)
                        {
                            totalDamage += card.Damage;
                            totalKarmaShift += card.KarmaShift;
                            summary.Add($"{kvp.Key} zahrál {card.CardId}");
                        }
                    }

                    room.EnemyHp -= totalDamage;
                    room.CurrentKarma += totalKarmaShift;
                    
                    room.PlayedCardsThisTurn.Clear();
                    room.PlayersReady.Clear();

                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, totalDamage, room.CurrentKarma, room.EnemyHp);
                    
                    if (room.EnemyHp <= 0)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            if (currentNode.Type == NodeType.Boss)
                            {
                                await Clients.Group(roomName).SendAsync("GameOver", "Vítězství! Boss padl, kampaň je dokončena!");
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
                            p.DrawCards(1);
                            // Aktualizovaná data pro nové kolo
                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana, p.Gold, p.DrawPile.Count, p.DiscardPile.Count);
                        }
                    }
                }
            }
        }
    }
}