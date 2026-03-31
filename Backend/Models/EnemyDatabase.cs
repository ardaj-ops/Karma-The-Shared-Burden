using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class EnemyDatabase
    {
        // Jako klíč budeme používat jméno nepřítele, aby se nám s ním snadno pracovalo
        public static Dictionary<string, EnemyTemplate> Enemies = new Dictionary<string, EnemyTemplate>
        {
            // ==========================================
            // ACT 1 - NORMÁLNÍ NEPŘÁTELÉ (10x)
            // ==========================================
            new EnemyTemplate("1_N_01", "Kultista Temnoty", 60, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Dýka ve tmě", DamageToAll = 5 }, new EnemyAction { Name = "Temný rituál", DamageToAll = 2, Heal = 10 },
                new EnemyAction { Name = "Fanatismus", DamageToAll = 7 }, new EnemyAction { Name = "Otrávená čepel", DamageToAll = 4 },
                new EnemyAction { Name = "Vzývání stínů", Heal = 15 }, new EnemyAction { Name = "Šílený výpad", DamageToAll = 6 },
                new EnemyAction { Name = "Oběť krve", DamageToAll = 8 }, new EnemyAction { Name = "Zlověstný šepot", DamageToAll = 3, Heal = 5 },
                new EnemyAction { Name = "Posedlost", DamageToAll = 5, Heal = 5 }, new EnemyAction { Name = "Zákeřný sek", DamageToAll = 4 }
            }),
            new EnemyTemplate("1_N_02", "Oživlá Kostra", 45, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Škrábnutí", DamageToAll = 4 }, new EnemyAction { Name = "Hození kosti", DamageToAll = 6 },
                new EnemyAction { Name = "Složení kostí", Heal = 10 }, new EnemyAction { Name = "Kostlivý stisk", DamageToAll = 5 },
                new EnemyAction { Name = "Praskot lebky", DamageToAll = 7 }, new EnemyAction { Name = "Hrabání z hrobu", Heal = 5 },
                new EnemyAction { Name = "Zákeřné kousnutí", DamageToAll = 3, Heal = 3 }, new EnemyAction { Name = "Pád", DamageToAll = 8 },
                new EnemyAction { Name = "Nářek mrtvých", DamageToAll = 2 }, new EnemyAction { Name = "Záchvěv smrti", DamageToAll = 5 }
            }),
            new EnemyTemplate("1_N_03", "Zdivočelý Vlk", 55, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Kousnutí", DamageToAll = 6 }, new EnemyAction { Name = "Drásání", DamageToAll = 5 },
                new EnemyAction { Name = "Vytí na měsíc", Heal = 8 }, new EnemyAction { Name = "Skok na krk", DamageToAll = 9 },
                new EnemyAction { Name = "Zběsilost", DamageToAll = 4, Heal = 4 }, new EnemyAction { Name = "Zadápnutí", DamageToAll = 7 },
                new EnemyAction { Name = "Trhání masa", DamageToAll = 8 }, new EnemyAction { Name = "Ústup do stínů", Heal = 10 },
                new EnemyAction { Name = "Smečkový útok", DamageToAll = 10 }, new EnemyAction { Name = "Zuřivé chňapnutí", DamageToAll = 5 }
            }),
            new EnemyTemplate("1_N_04", "Zloděj duší", 50, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Krádež", DamageToAll = 4, Heal = 5 }, new EnemyAction { Name = "Stínový krok", Heal = 12 },
                new EnemyAction { Name = "Karmická dýka", DamageToAll = 8 }, new EnemyAction { Name = "Vysátí", DamageToAll = 3, Heal = 8 },
                new EnemyAction { Name = "Oslepení", DamageToAll = 5 }, new EnemyAction { Name = "Bodnutí do zad", DamageToAll = 10 },
                new EnemyAction { Name = "Přelud", Heal = 15 }, new EnemyAction { Name = "Drtivá rána", DamageToAll = 7 },
                new EnemyAction { Name = "Zlodějský trik", DamageToAll = 6 }, new EnemyAction { Name = "Úsměv smrti", DamageToAll = 9 }
            }),
            new EnemyTemplate("1_N_05", "Kyselý Sliz", 80, "Normal", 1, new List<EnemyAction> { // Sliz má víc HP, ale dává menší rány
                new EnemyAction { Name = "Žíravina", DamageToAll = 3 }, new EnemyAction { Name = "Pohlcení", DamageToAll = 2, Heal = 5 },
                new EnemyAction { Name = "Rozdělení", Heal = 15 }, new EnemyAction { Name = "Kyselý plivanec", DamageToAll = 4 },
                new EnemyAction { Name = "Rozleknutí", DamageToAll = 5 }, new EnemyAction { Name = "Toxický opar", DamageToAll = 3 },
                new EnemyAction { Name = "Regenerace", Heal = 10 }, new EnemyAction { Name = "Natažení", DamageToAll = 6 },
                new EnemyAction { Name = "Oblepení", DamageToAll = 2 }, new EnemyAction { Name = "Kyselý výbuch", DamageToAll = 8 }
            }),
            new EnemyTemplate("1_N_06", "Gobliní Zvěd", 40, "Normal", 1, new List<EnemyAction> { // Málo HP, ale otravný
                new EnemyAction { Name = "Rychlý bod", DamageToAll = 5 }, new EnemyAction { Name = "Útěk s kořistí", Heal = 10 },
                new EnemyAction { Name = "Hození dýky", DamageToAll = 7 }, new EnemyAction { Name = "Jedová šipka", DamageToAll = 4 },
                new EnemyAction { Name = "Kopnutí pod koleno", DamageToAll = 6 }, new EnemyAction { Name = "Panika", DamageToAll = 3, Heal = 5 },
                new EnemyAction { Name = "Výsměch", DamageToAll = 2 }, new EnemyAction { Name = "Skrytí", Heal = 15 },
                new EnemyAction { Name = "Bodnutí do slabin", DamageToAll = 9 }, new EnemyAction { Name = "Kousnutí", DamageToAll = 4 }
            }),
            new EnemyTemplate("1_N_07", "Obří Netopýr", 45, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Sání krve", DamageToAll = 4, Heal = 6 }, new EnemyAction { Name = "Netopýří pískot", DamageToAll = 3 },
                new EnemyAction { Name = "Slet shůry", DamageToAll = 7 }, new EnemyAction { Name = "Zahryznutí", DamageToAll = 5, Heal = 5 },
                new EnemyAction { Name = "Zmatení", DamageToAll = 2 }, new EnemyAction { Name = "Temný let", Heal = 10 },
                new EnemyAction { Name = "Upíří polibek", DamageToAll = 6, Heal = 6 }, new EnemyAction { Name = "Rojení", DamageToAll = 8 },
                new EnemyAction { Name = "Mávnutí křídel", DamageToAll = 5 }, new EnemyAction { Name = "Tichý lov", DamageToAll = 9 }
            }),
            new EnemyTemplate("1_N_08", "Nemrtvý Voják", 70, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Rezavý meč", DamageToAll = 6 }, new EnemyAction { Name = "Úder štítem", DamageToAll = 4 },
                new EnemyAction { Name = "Pochod smrti", DamageToAll = 5 }, new EnemyAction { Name = "Odhodlání", Heal = 10 },
                new EnemyAction { Name = "Těžká rána", DamageToAll = 9 }, new EnemyAction { Name = "Vzpomínka na bitvu", DamageToAll = 7 },
                new EnemyAction { Name = "Složení kostí", Heal = 15 }, new EnemyAction { Name = "Bodnutí", DamageToAll = 5 },
                new EnemyAction { Name = "Výpad", DamageToAll = 8 }, new EnemyAction { Name = "Krvavá přísaha", DamageToAll = 6, Heal = 5 }
            }),
            new EnemyTemplate("1_N_09", "Lesní Pavouk", 50, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Jedové kousnutí", DamageToAll = 5 }, new EnemyAction { Name = "Vystřelení pavučiny", DamageToAll = 3 },
                new EnemyAction { Name = "Skok ze tmy", DamageToAll = 8 }, new EnemyAction { Name = "Kokon", Heal = 15 },
                new EnemyAction { Name = "Nekróza", DamageToAll = 6 }, new EnemyAction { Name = "Ostré nohy", DamageToAll = 7 },
                new EnemyAction { Name = "Hostina", DamageToAll = 4, Heal = 8 }, new EnemyAction { Name = "Úkryt", Heal = 10 },
                new EnemyAction { Name = "Pavučinová past", DamageToAll = 9 }, new EnemyAction { Name = "Děsivé mručení", DamageToAll = 2 }
            }),
            new EnemyTemplate("1_N_10", "Bandita z Cesty", 65, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Přepadení", DamageToAll = 8 }, new EnemyAction { Name = "Rána obuškem", DamageToAll = 5 },
                new EnemyAction { Name = "Kopnutí", DamageToAll = 4 }, new EnemyAction { Name = "Lok lékvaru", Heal = 20 },
                new EnemyAction { Name = "Podříznutí", DamageToAll = 9 }, new EnemyAction { Name = "Surový sek", DamageToAll = 7 },
                new EnemyAction { Name = "Zastrašení", DamageToAll = 3 }, new EnemyAction { Name = "Hození nože", DamageToAll = 6 },
                new EnemyAction { Name = "Zoufalý útok", DamageToAll = 10 }, new EnemyAction { Name = "Zahojení ran", Heal = 15 }
            }),

            // ==========================================
            // ACT 1 - ELITNÍ NEPŘÁTELÉ (5x)
            // ==========================================
            new EnemyTemplate("1_E_01", "Temný Rytíř", 150, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Těžký sek", DamageToAll = 12 }, new EnemyAction { Name = "Krvavá žeň", DamageToAll = 8, Heal = 15 },
                new EnemyAction { Name = "Drtivý dopad", DamageToAll = 15 }, new EnemyAction { Name = "Proražení", DamageToAll = 10 },
                new EnemyAction { Name = "Temná aura", DamageToAll = 5, Heal = 10 }, new EnemyAction { Name = "Švihnutí mečem", DamageToAll = 9 },
                new EnemyAction { Name = "Odplata", DamageToAll = 11 }, new EnemyAction { Name = "Brutální výpad", DamageToAll = 14 },
                new EnemyAction { Name = "Železná pěst", DamageToAll = 7 }, new EnemyAction { Name = "Vysátí naděje", DamageToAll = 6, Heal = 20 }
            }),
            new EnemyTemplate("1_E_02", "Démonický Inkvizitor", 140, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Očistný oheň", DamageToAll = 14 }, new EnemyAction { Name = "Soud", DamageToAll = 18 },
                new EnemyAction { Name = "Modlitba temnot", Heal = 25 }, new EnemyAction { Name = "Bičování", DamageToAll = 10 },
                new EnemyAction { Name = "Hořící kříž", DamageToAll = 12, Heal = 5 }, new EnemyAction { Name = "Kletba", DamageToAll = 8 },
                new EnemyAction { Name = "Falešná naděje", Heal = 30 }, new EnemyAction { Name = "Zúčtování", DamageToAll = 20 },
                new EnemyAction { Name = "Krvavá spravedlnost", DamageToAll = 15 }, new EnemyAction { Name = "Upálení", DamageToAll = 16 }
            }),
            new EnemyTemplate("1_E_03", "Krvavý Řezník", 160, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Sekáček", DamageToAll = 15 }, new EnemyAction { Name = "Hák na maso", DamageToAll = 10, Heal = 10 },
                new EnemyAction { Name = "Ostrý nůž", DamageToAll = 12 }, new EnemyAction { Name = "Vykrvácení", DamageToAll = 8 },
                new EnemyAction { Name = "Masakr", DamageToAll = 22 }, new EnemyAction { Name = "Zakousnutí", DamageToAll = 5, Heal = 15 },
                new EnemyAction { Name = "Drcení kostí", DamageToAll = 16 }, new EnemyAction { Name = "Hostina", Heal = 35 },
                new EnemyAction { Name = "Záchvat zuřivosti", DamageToAll = 20 }, new EnemyAction { Name = "Krvavá koupel", DamageToAll = 14 }
            }),
            new EnemyTemplate("1_E_04", "Kamenný Golem", 200, "Elite", 1, new List<EnemyAction> { // Hodně HP, rány pomalejší ale bolí
                new EnemyAction { Name = "Zemětřesení", DamageToAll = 10 }, new EnemyAction { Name = "Vržený balvan", DamageToAll = 18 },
                new EnemyAction { Name = "Krystalizace", Heal = 20 }, new EnemyAction { Name = "Drcení", DamageToAll = 15 },
                new EnemyAction { Name = "Tlaková vlna", DamageToAll = 12 }, new EnemyAction { Name = "Nabírání síly", Heal = 30 },
                new EnemyAction { Name = "Úder pěstí", DamageToAll = 14 }, new EnemyAction { Name = "Zasypání kamením", DamageToAll = 16 },
                new EnemyAction { Name = "Nezlomnost", Heal = 15 }, new EnemyAction { Name = "Rozdupnutí", DamageToAll = 22 }
            }),
            new EnemyTemplate("1_E_05", "Nekromant", 130, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Oživení mrtvých", DamageToAll = 8, Heal = 10 }, new EnemyAction { Name = "Zápach hniloby", DamageToAll = 12 },
                new EnemyAction { Name = "Dotek smrti", DamageToAll = 18 }, new EnemyAction { Name = "Temný štít", Heal = 25 },
                new EnemyAction { Name = "Kradení duší", DamageToAll = 10, Heal = 15 }, new EnemyAction { Name = "Zákeřný blesk", DamageToAll = 15 },
                new EnemyAction { Name = "Hltání Karmy", DamageToAll = 5, Heal = 30 }, new EnemyAction { Name = "Bolest", DamageToAll = 16 },
                new EnemyAction { Name = "Přízračná kosa", DamageToAll = 14 }, new EnemyAction { Name = "Vzkříšení těla", Heal = 40 }
            }),

            // ==========================================
            // ACT 1 - BOSSOVÉ (3x)
            // ==========================================
            new EnemyTemplate("1_B_01", "Pán Karmy", 350, "Boss", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Zákon Rovnováhy", DamageToAll = 15, Heal = 15 }, new EnemyAction { Name = "Soudný den", DamageToAll = 25 },
                new EnemyAction { Name = "Absolutní Očištění", Heal = 50 }, new EnemyAction { Name = "Drtivá tíha osudu", DamageToAll = 20 },
                new EnemyAction { Name = "Karmický výkyv", DamageToAll = 18, Heal = 10 }, new EnemyAction { Name = "Absolutní stín", DamageToAll = 22 },
                new EnemyAction { Name = "Trest za hříchy", DamageToAll = 19 }, new EnemyAction { Name = "Oslňující záře", DamageToAll = 12, Heal = 30 },
                new EnemyAction { Name = "Vymazání z existence", DamageToAll = 30 }, new EnemyAction { Name = "Falešný Úsvit", DamageToAll = 10, Heal = 20 }
            }),
            new EnemyTemplate("1_B_02", "Prastarý Drak", 400, "Boss", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Ohnivý dech", DamageToAll = 20 }, new EnemyAction { Name = "Úder ocasem", DamageToAll = 25 },
                new EnemyAction { Name = "Spánek na zlatě", Heal = 40 }, new EnemyAction { Name = "Drtivé čelisti", DamageToAll = 30 },
                new EnemyAction { Name = "Dračí vztek", DamageToAll = 15, Heal = 15 }, new EnemyAction { Name = "Křídla hurikánu", DamageToAll = 18 },
                new EnemyAction { Name = "Tavení zbroje", DamageToAll = 22 }, new EnemyAction { Name = "Pohlcení plamenů", Heal = 60 },
                new EnemyAction { Name = "Magma", DamageToAll = 28 }, new EnemyAction { Name = "Armagedon", DamageToAll = 35 }
            }),
            new EnemyTemplate("1_B_03", "Královna Pavouků", 320, "Boss", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Kyselý jed", DamageToAll = 18 }, new EnemyAction { Name = "Pohlcení obětí", DamageToAll = 10, Heal = 30 },
                new EnemyAction { Name = "Smrtící stisk", DamageToAll = 26 }, new EnemyAction { Name = "Spřádání sítí", Heal = 40 },
                new EnemyAction { Name = "Roj malých pavouků", DamageToAll = 22 }, new EnemyAction { Name = "Zákeřný skok", DamageToAll = 24 },
                new EnemyAction { Name = "Injekce parazita", DamageToAll = 15, Heal = 10 }, new EnemyAction { Name = "Bodnutí žihadlem", DamageToAll = 28 },
                new EnemyAction { Name = "Zatemnění", DamageToAll = 12 }, new EnemyAction { Name = "Zkáza hnízda", DamageToAll = 32 }
            })
        };
        };
        // --- CHYTRÉ FUNKCE PRO VÝBĚR (Teď berou v potaz i AKT!) ---
        public static EnemyTemplate GetRandomEnemy(string tier, int act)
        {
            var possibleEnemies = Enemies.Where(e => e.Tier == tier && e.Act == act).ToList();
            if (possibleEnemies.Count == 0) return Enemies[0]; // Záchranná brzda

            Random rng = new Random();
            return possibleEnemies[rng.Next(possibleEnemies.Count)];
        }

        public static EnemyAction GetRandomActionForEnemy(string enemyName)
        {
            var enemy = Enemies.FirstOrDefault(e => e.Name == enemyName);
            if (enemy != null && enemy.Actions.Count > 0)
            {
                Random rng = new Random();
                return enemy.Actions[rng.Next(enemy.Actions.Count)];
            }
            return new EnemyAction { Name = "Zmatení", DamageToAll = 0 };
        }
    }
}