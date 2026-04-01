using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class HeroDatabase
    {
        public static Dictionary<string, HeroTemplate> Heroes = new Dictionary<string, HeroTemplate>
        {
            { "Paladin", new HeroTemplate("Paladin", "Světlonoš", 80, "Ušlechtilý ochránce, který léčí tým a vede svět ke světlu.",
                new Relic("Hero_01", "Svatý kodex", "Na začátku boje získáš 5 Bloku a tým získá +2 Karmu."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "P_01", "P_11" },
                new List<string>()) },

            { "Warlock", new HeroTemplate("Warlock", "Stínový mág", 60, "Temný vyvolávač, který obětuje vlastní krev pro ničivou magii.",
                new Relic("Hero_02", "Zkrvavený grimoár", "Na začátku boje ztratíš 2 HP, ale získáš 1 Manu navíc."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "W_01", "W_21" },
                new List<string>()) },

            { "Monk", new HeroTemplate("Monk", "Vyrovnávač", 70, "Mistr vnitřního klidu, který těží z dokonalé karmické rovnováhy.",
                new Relic("Hero_03", "Mnišské korále", "Na začátku každého boje získáš 1 Obratnost (Dexterity)."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "M_01", "M_21" },
                new List<string>()) },

            { "Berserker", new HeroTemplate("Berserker", "Krvavý berserk", 90, "Nezastavitelný válečník, jehož hněv a síla rostou s utrženými ranami.",
                new Relic("Hero_04", "Zubatá sekyra", "Na začátku každého boje získáš 1 Sílu (Strength)."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "B_01", "B_26" },
                new List<string>()) },

            { "Druid", new HeroTemplate("Druid", "Ochránce přírody", 75, "Strážce hvozdu využívající regeneraci a hněv divoké přírody.",
                new Relic("Hero_05", "Zlatý žalud", "Na začátku každého boje získáš 1 Regeneraci."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "D_01", "D_21" },
                new List<string>()) },

            { "Rogue", new HeroTemplate("Rogue", "Karmický vrah", 65, "Mistr stínů a úskoků, který oslabuje nepřátele dříve, než stačí mrknout.",
                new Relic("Hero_06", "Stínový plášť", "Na začátku boje aplikuje 1 Oslabení (Weak) na všechny nepřátele."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "R_01", "R_21" },
                new List<string>()) },

            { "Bard", new HeroTemplate("Bard", "Umělec", 70, "Povzbuzuje družinu svými písněmi a zajišťuje stálý přísun nových možností.",
                new Relic("Hero_07", "Rozladěná loutna", "Na začátku každého boje si lízneš 1 kartu navíc."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Bd_02", "Bd_16" },
                new List<string>()) },

            { "Pyromancer", new HeroTemplate("Pyromancer", "Pyromant", 60, "Nestálý mág, který chce vidět svět v plamenech.",
                new Relic("Hero_08", "Věčná pochodeň", "Na začátku boje aplikuje 1 Hoření (Flame) na všechny nepřátele."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Py_02", "Py_03" },
                new List<string>()) }
        };
    }
}