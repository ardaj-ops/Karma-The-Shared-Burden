using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class HeroDatabase
    {
        public static Dictionary<string, HeroTemplate> Heroes = new Dictionary<string, HeroTemplate>
        {
            { "Paladin", new HeroTemplate("Paladin", "Světlonoš", 80, "Léčí a chrání tým.",
                new Relic("Rel_Pal", "Aura Světla", "Kdykoliv zahraješ kartu, která léčí, přidá týmu +1 Karmu."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "P_01", "P_11" },
                new List<string>()) },

            { "Warlock", new HeroTemplate("Warlock", "Stínový mág", 60, "Obětuje životy za obrovskou sílu.",
                new Relic("Rel_War", "Krvavý pakt", "Tvé karty s negativní Karmou udělují o 2 poškození více."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "W_01", "W_21" },
                new List<string>()) },

            { "Monk", new HeroTemplate("Monk", "Vyrovnávač", 70, "Udržuje Karmu v rovnováze.",
                new Relic("Rel_Monk", "Zenové korálky", "Pokud je Karma na konci tvého tahu přesně 0, lízneš si kartu navíc."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "M_01", "M_21" },
                new List<string>()) },

            { "Berserker", new HeroTemplate("Berserker", "Krvavý berserk", 90, "Čím méně má životů, tím je silnější.",
                new Relic("Rel_Ber", "Zkrvavená sekera", "Pokud máš méně než 50% HP, začínáš tah s 1 Manou navíc."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "B_01", "B_26" },
                new List<string>()) }, // Má o jeden základní úder víc místo obrany, je to přece Berserker!

            { "Druid", new HeroTemplate("Druid", "Ochránce přírody", 75, "Využívá sílu lesa k ochraně a poškození.",
                new Relic("Rel_Dru", "Žalud moudrosti", "Na začátku každého souboje vyléčí všem hráčům 3 HP."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "D_01", "D_21" },
                new List<string>()) },

            { "Rogue", new HeroTemplate("Rogue", "Karmický vrah", 65, "Specialista na komba a dobírání karet.",
                new Relic("Rel_Rog", "Stínový plášť", "První karta zahraná každé kolo tě nestojí žádnou Manu, pokud má cenu 1."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "R_01", "R_21" },
                new List<string>()) },

            { "Bard", new HeroTemplate("Bard", "Support", 70, "Posiluje ostatní svými písněmi.",
                new Relic("Rel_Bard", "Kouzelná loutna", "Na konci tahu rozdělí 2 Bloky všem ostatním hráčům."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Bd_02", "Bd_16" },
                new List<string>()) },

            { "Pyromancer", new HeroTemplate("Pyromancer", "Pyromant", 60, "Spaluje nepřátele ohněm.",
                new Relic("Rel_Pyro", "Věčný plamen", "Na konci každého tvého tahu udělí 3 poškození nepříteli."),
                new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Z_Obrana", "Py_02", "Py_03" },
                new List<string>()) }
        };
    }
}