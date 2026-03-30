namespace RoguelikeCardGame.Models
{
    public class CardTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Cost { get; set; }
        public int KarmaShift { get; set; }
        public int Damage { get; set; }

        public CardTemplate() { }

        public CardTemplate(string id, string name, int cost, int karmaShift, int damage)
        {
            Id = id;
            Name = name;
            Cost = cost;
            KarmaShift = karmaShift;
            Damage = damage;
        }
    }
}