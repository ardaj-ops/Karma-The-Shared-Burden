namespace RoguelikeCardGame.Models
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // NOVÉ: Kolik energie (many) karta stojí zahra
        public int Cost { get; set; } 
        
        public int Damage { get; set; }
        
        // NOVÉ: Kolikrát karta udeří (např. pro kombo útoky 3x 4 damage)
        public int HitCount { get; set; } 
        
        public int Block { get; set; }
        public int Heal { get; set; }
        public int KarmaShift { get; set; }
        
        // NOVÉ: Kolik nových karet si hráč po zahrání lízne z balíčku
        public int DrawCards { get; set; } 

        // Konstruktor jsme rozšířili o nové parametry s výchozími hodnotami
        public Card(string id, string name, string description, int cost, int damage = 0, int block = 0, int heal = 0, int karmaShift = 0, int drawCards = 0, int hitCount = 1)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
            Damage = damage;
            Block = block;
            Heal = heal;
            KarmaShift = karmaShift;
            DrawCards = drawCards;
            HitCount = hitCount;
        }
    }
}