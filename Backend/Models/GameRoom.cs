using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    // Pomocná třída pro uložení dat o zahrané kartě v daném tahu
    public class CardPlayData
    {
        public string CardId { get; set; }
        public int KarmaShift { get; set; }
        public int Damage { get; set; }
    }

    public class GameRoom
    {
        public string RoomName { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentKarma { get; set; } = 0;
        
        // Životy společného nepřítele (parametr k nastavení)
        public int EnemyHp { get; set; } = 100; 

        // Paměť pro aktuální tah: Kdo (jméno) co zahrál (data o kartě)
        public ConcurrentDictionary<string, CardPlayData> PlayedCardsThisTurn { get; set; } = new ConcurrentDictionary<string, CardPlayData>();

        public GameRoom(string roomName)
        {
            RoomName = roomName;
        }
    }
}