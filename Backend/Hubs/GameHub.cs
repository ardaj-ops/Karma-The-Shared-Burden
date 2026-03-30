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
        private static ConcurrentBag<Player> _waitingRoom = new ConcurrentBag<Player>();
        private static ConcurrentDictionary<string, GameRoom> _activeRooms = new ConcurrentDictionary<string, GameRoom>();
        
        private static int _gameCounter = 0;
        private const int PlayersNeededToStart = 5;

        public async Task JoinGame(string playerName)
        {
            var newPlayer = new Player(Context.ConnectionId, playerName);
            
            // --- TESTOVACÍ BALÍČEK ---
            newPlayer.StartingDeck = new List<string> { "Py_50", "Card_01", "Card_02", "Card_03", "Card_04", "Card_05", "Card_06", "Card_07" };

            _waitingRoom.Add(newPlayer);
            await Clients.All.SendAsync("PlayerJoinedWaitingRoom", newPlayer.Name, _waitingRoom.Count, PlayersNeededToStart);

            if (_waitingRoom.Count >= PlayersNeededToStart)
            {
                _gameCounter++;
                string roomName = "GameRoom_" + _gameCounter;
                var newGame = new GameRoom(roomName);
                
                _activeRooms.TryAdd(roomName, newGame);

                while (!_waitingRoom.IsEmpty && newGame.Players.Count < PlayersNeededToStart)
                {
                    if (_waitingRoom.TryTake(out Player p))
                    {
                        newGame.Players.Add(p);
                        await Groups.AddToGroupAsync(p.ConnectionId, roomName);
                    }
                }

                // INICIALIZACE DECKŮ PŘI STARTU HRY
                foreach (var player in newGame.Players)
                {
                    player.InitializeGame(); // Zamíchá balíček a nastaví manu na max
                    player.DrawCards(5);     // Lízne 5 karet
                    
                    // Pošleme hráči jeho osobní stav do začátku
                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveInitialState", player.Hand, player.Mana);
                }

                await Clients.Group(roomName).SendAsync("GameStarted", roomName);
            }
        }

        public async Task SelectCard(string roomName, string playerName, string cardId, int karmaShift, int damage)
        {
            if (_activeRooms.TryGetValue(roomName, out GameRoom room))
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                
                if (player != null)
                {
                    // Odebereme zahranou kartu z ruky a dáme do odhazovacího balíčku
                    if (player.Hand.Contains(cardId))
                    {
                        player.Hand.Remove(cardId);
                        player.DiscardPile.Add(cardId);
                    }
                }

                // Uložíme zahranou kartu pro vyhodnocení na konci tahu
                var cardData = new CardPlayData { CardId = cardId, KarmaShift = karmaShift, Damage = damage };
                room.PlayedCardsThisTurn.TryAdd(playerName, cardData);

                await Clients.Group(roomName).SendAsync("PlayerReadiedUp", playerName);

                // Pokud zahráli všichni hráči, tah se vyhodnotí
                if (room.PlayedCardsThisTurn.Count >= room.Players.Count)
                {
                    int totalDamage = 0;
                    int totalKarmaShift = 0;
                    var summary = new List<string>();

                    foreach (var kvp in room.PlayedCardsThisTurn)
                    {
                        totalDamage += kvp.Value.Damage;
                        totalKarmaShift += kvp.Value.KarmaShift;
                        summary.Add($"{kvp.Key} zahrál {kvp.Value.CardId}");
                    }

                    room.EnemyHp -= totalDamage;
                    room.CurrentKarma += totalKarmaShift;
                    room.PlayedCardsThisTurn.Clear();

                    await Clients.Group(roomName).SendAsync("TurnResolved", summary, totalDamage, room.CurrentKarma, room.EnemyHp);
                    
                    // --- KONTROLA SMRTI NEBO DALŠÍ KOLO ---
                    if (room.EnemyHp <= 0)
                    {
                        // Boss je mrtev, hra končí vítězstvím!
                        await Clients.Group(roomName).SendAsync("GameOver", "Vítězství! Boss byl poražen.");
                        _activeRooms.TryRemove(roomName, out _); // Úklid paměti na serveru
                    }
                    else
                    {
                        // Boss stále žije, připravíme další kolo
                        foreach (var p in room.Players)
                        {
                            p.Mana = p.MaxMana; // Obnovíme manu
                            p.DrawCards(1);     // Lízneme 1 novou kartu
                            
                            // Pošleme hráči jeho aktualizovaný stav
                            await Clients.Client(p.ConnectionId).SendAsync("ReceiveNewTurnState", p.Hand, p.Mana);
                        }
                    }
                }
            }
        }
    }
}