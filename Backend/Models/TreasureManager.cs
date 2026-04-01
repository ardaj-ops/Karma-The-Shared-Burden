using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    public class TreasureLoot
    {
        public int Gold { get; set; }
        public List<CardTemplate> Cards { get; set; } = new List<CardTemplate>();
    }

    public static class TreasureManager
    {
        public static TreasureLoot GenerateTreasureForPlayer(Player player)
        {
            Random rng = new Random();
            int gold = rng.Next(50, 151); // 50 až 150 zlaťáků
            
            // Filtrace karet podle třídy (stejně jako u odměn z bossů)
            string classPrefix = GetPrefixForClass(player.HeroClass);
            var classSpecificCards = CardDatabase.Cards.Values
                .Where(c => c.Id.StartsWith(classPrefix) || c.Id.StartsWith("Z_"))
                .ToList();
            
            if (classSpecificCards.Count < 3) 
                classSpecificCards = CardDatabase.Cards.Values.ToList();
                
            var cards = classSpecificCards.OrderBy(x => rng.Next()).Take(3).ToList();
            
            return new TreasureLoot 
            {
                Gold = gold,
                Cards = cards
            };
        }

        private static string GetPrefixForClass(string heroClass)
        {
            switch (heroClass)
            {
                case "Paladin": return "P_";
                case "Warlock": return "W_";
                case "Monk": return "M_";
                case "Berserker": return "B_";
                case "Druid": return "D_";
                case "Rogue": return "R_";
                case "Bard": return "Bd_";
                case "Pyromancer": return "Py_";
                default: return "Z_"; 
            }
        }
    }
}