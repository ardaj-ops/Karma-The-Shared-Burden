using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class CardTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Cost { get; set; }
        public int KarmaShift { get; set; }
        public int Damage { get; set; }
        
        // Mechaniky
        public int Block { get; set; }
        public int Heal { get; set; }
        public int DrawCards { get; set; }
        public int HitCount { get; set; }

        // NOVÉ: Seznam efektů, které karta aplikuje (Poison, Strength, atd.)
        public List<EffectApplication> TargetEffects { get; set; } = new List<EffectApplication>();
        public List<EffectApplication> SelfEffects { get; set; } = new List<EffectApplication>();

        public CardTemplate() { }

        // Konstruktor přizpůsobený tvému zápisu v databázi + podpora efektů
        public CardTemplate(string id, string name, string description, 
                            int cost = 1, int karmaShift = 0, int damage = 0, 
                            int block = 0, int heal = 0, int drawCards = 0, int hitCount = 1,
                            List<EffectApplication>? targetEffects = null, 
                            List<EffectApplication>? selfEffects = null)
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
            
            if (targetEffects != null) TargetEffects = targetEffects;
            if (selfEffects != null) SelfEffects = selfEffects;
        }
    }
}