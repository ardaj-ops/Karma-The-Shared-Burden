namespace RoguelikeCardGame.Models
{
    public class CardTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // NOVÉ: Popisek karty
        public int Cost { get; set; }
        public int KarmaShift { get; set; }
        public int Damage { get; set; }
        
        // Nové mechaniky
        public int Block { get; set; }
        public int Heal { get; set; }
        public int DrawCards { get; set; }
        public int HitCount { get; set; }

        public CardTemplate() { }

        // Konstruktor přizpůsobený tvému zápisu v databázi
        public CardTemplate(string id, string name, string description, 
                            int cost = 1, int karmaShift = 0, int damage = 0, 
                            int block = 0, int heal = 0, int drawCards = 0, int hitCount = 1)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
            KarmaShift = karmaShift;
            Damage = damage;
            Block = block;
            Heal = heal;
            DrawCards = drawCards;
            HitCount = hitCount;
        }
    }
}