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

        // --- UPRAVENO: Přijímáme parametr heroClass ---
        public async Task JoinGame(string playerName, string heroClass)
        {
            var newPlayer = new Player(Context.ConnectionId, playerName);
            newPlayer.HeroClass = heroClass;

            // --- NOVÉ: NAČTENÍ DAT Z DATABÁZE HRDINŮ ---
            // Zkusíme najít hrdinu ve tvém slovníku "Heroes"
            // POZNÁMKA: Místo "HeroDatabase" použij název třídy, kde máš svůj slovník definovaný!
            if (HeroDatabase.Heroes.TryGetValue(heroClass, out HeroTemplate template))
            {
                newPlayer.MaxHp = template.Hp;
                newPlayer.StartingDeck = new List<string>(template.StartingCards); 
            }
            else
            {
                // Nouzová pojistka, kdyby někdo zkusil hacknout hru a poslal špatné jméno
                newPlayer.MaxHp = 50;
                newPlayer.StartingDeck = new List<string> { "Card_01", "Card_02", "Card_04" };
            }

            _waitingRoom.Add(newPlayer);
            
            // Informujeme lobby i s vybraným hrdinou
            await Clients.All.SendAsync("PlayerJoinedWaitingRoom", $"{newPlayer.Name} ({heroClass})", _waitingRoom.Count, PlayersNeededToStart);

            if (_waitingRoom.Count >= PlayersNeededToStart)
            {
                _gameCounter++;
                string roomName = "GameRoom_" + _gameCounter;
                var newGame = new GameRoom(roomName); // Tady se automaticky volá GenerateMap()
                
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
                    player.InitializeGame(); // Tohle díky naší minulé úpravě nastaví i plné HP!
                    player.DrawCards(5);
                    await Clients.Client(player.ConnectionId).SendAsync("ReceiveInitialState", player.Hand, player.Mana);
                }

                // Posíláme celou strukturu mapy všem hráčům
                await Clients.Group(roomName).SendAsync("GameStarted", roomName, newGame.Map);
                
                // Hráči začínají v prvním uzlu (Encounter)
                var startNode = newGame.Map[0];
                await Clients.Group(roomName).SendAsync("EnteredNode", startNode.Type.ToString(), startNode);
            }
        }

        // --- POHYB PO MAPĚ ---
        public async Task MoveToNextNode(string roomName, int nodeId)
        {
            if (_activeRooms.TryGetValue(roomName, out GameRoom room))
            {
                var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                var nextNode = room.Map.FirstOrDefault(n => n.Id == nodeId);

                // Kontrola, zda je uzel propojený a existuje
                if (nextNode != null && currentNode.ConnectedTo.Contains(nodeId))
                {
                    room.CurrentNodeId = nodeId;
                    await Clients.Group(roomName).SendAsync("EnteredNode", nextNode.Type.ToString(), nextNode);

                    // Pokud je to truhla, rovnou vygenerujeme odměnu
                    if (nextNode.Type == NodeType.Treasure)
                    {
                        string relic = "Zlatá podkova (+1 Mana každé kolo)"; // TODO: Systém relikvií
                        room.TeamRelics.Add(relic);
                        await Clients.Group(roomName).SendAsync("ReceiveTreasure", relic);
                    }
                }
            }
        }

        public async Task SelectCard(string roomName, string playerName, string cardId, int karmaShift, int damage)
        {
            if (_activeRooms.TryGetValue(roomName, out GameRoom room))
            {
                var player = room.Players.FirstOrDefault(p => p.Name == playerName);
                if (player != null && player.Hand.Contains(cardId))
                {
                    player.Hand.Remove(cardId);
                    player.DiscardPile.Add(cardId);
                }

                var cardData = new CardPlayData { CardId = cardId, KarmaShift = karmaShift, Damage = damage };
                room.PlayedCardsThisTurn.TryAdd(playerName, cardData);

                await Clients.Group(roomName).SendAsync("PlayerReadiedUp", playerName);

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
                    
                    if (room.EnemyHp <= 0)
                    {
                        // Místo konce celé hry nyní uzel označíme za splněný a nabídneme mapu
                        var currentNode = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
                        if (currentNode != null) currentNode.IsCompleted = true;

                        await Clients.Group(roomName).SendAsync("BattleWon", "Nepřítel poražen! Vyberte další cestu na mapě.");
                        
                        // Pokud to byl finální Boss, pak teprve konec hry
                        if (currentNode.Type == NodeType.Boss)
                        {
                            await Clients.Group(roomName).SendAsync("GameOver", "Vítězství! Celá kampaň dokončena.");
                            _activeRooms.TryRemove(roomName, out _);
                        }
                    }
                    else
                    {
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