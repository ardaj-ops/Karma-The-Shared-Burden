using Microsoft.AspNetCore.SignalR;
using RoguelikeCardGame.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; 

namespace RoguelikeCardGame.Hubs
{
    public class GameHub : Hub
    {
        // Teď už nepotřebujeme globální čekárnu, místnosti si zakládají hráči sami
        private static ConcurrentDictionary<string, GameRoom> _activeRooms = new ConcurrentDictionary<string, GameRoom>();
        
        // 1. VYTVOŘENÍ LOBBY (Zakladatel)
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

        // 2. PŘIPOJENÍ DO LOBBY
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
                
                // Řekneme všem v místnosti, že se někdo připojil
                var playerNames = room.Players.Select(p => p.Name).ToList();
                await Clients.Group(roomName).SendAsync("LobbyUpdate", playerNames);

                // Pokud je tento hráč první (zakladatel), pošleme mu extra signál, aby viděl tlačítko START
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

        // 3. ODSTARTOVÁNÍ HRY ZAKLADATELEM
        public async Task StartGame(string roomName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                foreach (var player in room.Players)
                {
                    player.InitializeGame(); 
                    player.DrawCards(5);
                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveInitialState", player.Hand, player.Mana, CardDatabase.Cards);
                }

                // Pošleme mapu, ale NEDÁVÁME je hned do boje. Hráči si musí kliknout na mapu.
                await Clients.Group(roomName).SendAsync("GameStarted", roomName, room.Map);
            }
        }

        // 4. POHYB PO MAPĚ
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

        // 5. ZAHRÁNÍ KARTY (Hráč může hrát vícekrát, nekončí se tím tah)
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
                
                // Přidáme kartu do seznamu karet tohoto hráče pro dané kolo
                room.PlayedCardsThisTurn.AddOrUpdate(playerName, 
                    new List<CardPlayData> { cardData }, 
                    (key, existingList) => { existingList.Add(cardData); return existingList; });

                await Clients.Group(roomName).SendAsync("CardPlayedLog", playerName, cardId);
            }
        }

        // 6. UKONČENÍ TAHU (Tlačítkem)
        public async Task PlayerReady(string roomName, string playerName)
        {
            if (_activeRooms.TryGetValue(roomName, out var room))
            {
                room.PlayersReady.Add(playerName);
                await Clients.Group(roomName).SendAsync("PlayerReadyLog", playerName, room.PlayersReady.Count, room.Players.Count);

                // Pokud všichni odklikli konec tahu, vyhodnotíme jej!
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
                    
                    // Reset pro další kolo
                    room.PlayedCardsThisTurn.Clear();
                    room.PlayersReady.Clear();

                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, totalDamage, room.CurrentKarma, room.EnemyHp);
                    
                    if (room.EnemyHp <= 0)
                    {
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null)
                        {
                            currentNode.IsCompleted = true;
                            // Pokud to byl Boss (poslední uzel), konec hry
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
                        // Nepřítel žije, dáváme nové karty a manu
                        foreach (var p in room.Players)
                        {
                            p.Mana = p.MaxMana;
                            p.DrawCards(1);
                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana);
                        }
                    }
                }
            }
        }
    }
}