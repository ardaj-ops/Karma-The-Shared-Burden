using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    // Konstanty pro názvy efektů (zabrání překlepům v kódu)
    public static class EffectType
    {
        // --- DEBUFFY (Negativní) ---
        public const string Poison = "Poison";         // Bere HP na konci tahu, stacky se snižují o 1
        public const string Flame = "Flame";           // Jako jed, ale můžeme na něj vázat speciální Pyromancer komba
        public const string Vulnerable = "Vulnerable"; // Cíl dostává o 50% více poškození (trvání = počet kol)
        public const string Weak = "Weak";             // Cíl dává o 25% menší poškození (trvání = počet kol)

        // --- BUFFY (Pozitivní) ---
        public const string Strength = "Strength";     // Každý útok dává o X zranění více (nesnižuje se)
        public const string Dexterity = "Dexterity";   // Každá obranná karta dává o X bloku více (nesnižuje se)
        public const string Regen = "Regen";           // Vyléčí X HP na konci tahu, stacky se snižují o 1
    }

    // Pomocná třída, kterou budeme přidávat do CardTemplate
    public class EffectApplication
    {
        public string Type { get; set; }
        public int Amount { get; set; }

        public EffectApplication(string type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}