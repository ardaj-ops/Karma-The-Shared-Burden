using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    // Jak vypadá jedna akce nepřítele
    public class EnemyAction
    {
        public string Name { get; set; } = string.Empty;
        public int DamageToAll { get; set; }
        public int Heal { get; set; }
        
        // Automaticky generovaný popisný text pro front-end
        public string IntentDescription => DamageToAll > 0 
            ? $"⚔️ {Name} (Útok všem: {DamageToAll} DMG" + (Heal > 0 ? $", Vyléčí si: {Heal} HP)" : ")")
            : $"✨ {Name} (Vyléčí si: {Heal} HP)";
    }

    // Jak vypadá samotný nepřítel
    public class EnemyTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MaxHp { get; set; }
        public string Tier { get; set; } = "Normal"; 
        public int Act { get; set; } = 1;// NOVÉ: Důležité pro losování z mapy
        public List<EnemyAction> Actions { get; set; } = new List<EnemyAction>();

        public EnemyTemplate() { }

        // Upravený konstruktor
        public EnemyTemplate(string id, string name, int maxHp, string tier, List<EnemyAction> actions)
        {
            Id = id;
            Name = name;
            MaxHp = maxHp;
            Tier = tier;
            Actions = actions;
        }
    }
}