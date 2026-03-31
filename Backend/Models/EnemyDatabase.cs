using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    public static class EnemyDatabase
    {
        public static List<EnemyTemplate> Enemies = new List<EnemyTemplate>
        {
            // ==========================================
            // ACT 1 - NORMÁLNÍ (10), ELITY (5), BOSSOVÉ (3)
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
            new EnemyTemplate("1_N_05", "Kyselý Sliz", 80, "Normal", 1, new List<EnemyAction> { 
                new EnemyAction { Name = "Žíravina", DamageToAll = 3 }, new EnemyAction { Name = "Pohlcení", DamageToAll = 2, Heal = 5 },
                new EnemyAction { Name = "Rozdělení", Heal = 15 }, new EnemyAction { Name = "Kyselý plivanec", DamageToAll = 4 },
                new EnemyAction { Name = "Rozleknutí", DamageToAll = 5 }, new EnemyAction { Name = "Toxický opar", DamageToAll = 3 },
                new EnemyAction { Name = "Regenerace", Heal = 10 }, new EnemyAction { Name = "Natažení", DamageToAll = 6 },
                new EnemyAction { Name = "Oblepení", DamageToAll = 2 }, new EnemyAction { Name = "Kyselý výbuch", DamageToAll = 8 }
            }),
            new EnemyTemplate("1_N_06", "Gobliní Zvěd", 40, "Normal", 1, new List<EnemyAction> { 
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

            new EnemyTemplate("1_E_01", "Temný Rytíř", 150, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Těžký sek", DamageToAll = 12 }, new EnemyAction { Name = "Krvavá žeň", DamageToAll = 8, Heal = 15 },
                new EnemyAction { Name = "Drtivý dopad", DamageToAll = 15 }, new EnemyAction { Name = "Proražení štítů", DamageToAll = 10 },
                new EnemyAction { Name = "Temná aura", DamageToAll = 5, Heal = 10 }, new EnemyAction { Name = "Švihnutí mečem", DamageToAll = 9 },
                new EnemyAction { Name = "Odplata padlých", DamageToAll = 11 }, new EnemyAction { Name = "Brutální výpad", DamageToAll = 14 },
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
            new EnemyTemplate("1_E_04", "Kamenný Golem", 200, "Elite", 1, new List<EnemyAction> { 
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
            }),

            // ==========================================
            // ACT 2 - NORMÁLNÍ (10), ELITY (5), BOSSOVÉ (3)
            // ==========================================
            new EnemyTemplate("2_N_01", "Pouštní Štír", 85, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Bodnutí hrotem", DamageToAll = 12 }, new EnemyAction { Name = "Sevření klepet", DamageToAll = 15 },
                new EnemyAction { Name = "Jed", DamageToAll = 8 }, new EnemyAction { Name = "Zahrabání do písku", Heal = 20 },
                new EnemyAction { Name = "Zuřivé bodání", DamageToAll = 10, Heal = 5 }, new EnemyAction { Name = "Krunýř", Heal = 15 },
                new EnemyAction { Name = "Písečná bouře", DamageToAll = 14 }, new EnemyAction { Name = "Krutý stisk", DamageToAll = 18 },
                new EnemyAction { Name = "Nervový jed", DamageToAll = 9 }, new EnemyAction { Name = "Vyčkávání", Heal = 25 }
            }),
            new EnemyTemplate("2_N_02", "Oživlá Mumie", 95, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kletba faraonů", DamageToAll = 14 }, new EnemyAction { Name = "Škrcení obinadlem", DamageToAll = 12 },
                new EnemyAction { Name = "Vysátí života", DamageToAll = 8, Heal = 15 }, new EnemyAction { Name = "Nákaza", DamageToAll = 10 },
                new EnemyAction { Name = "Těžký krok", DamageToAll = 16 }, new EnemyAction { Name = "Balzamování", Heal = 30 },
                new EnemyAction { Name = "Náhlý útok", DamageToAll = 20 }, new EnemyAction { Name = "Hnilobný dech", DamageToAll = 11 },
                new EnemyAction { Name = "Tlení", DamageToAll = 9 }, new EnemyAction { Name = "Vstání z mrtvých", Heal = 20 }
            }),
            new EnemyTemplate("2_N_03", "Stínový Asasín", 75, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kritický zásah", DamageToAll = 25 }, new EnemyAction { Name = "Splynutí se stíny", Heal = 15 },
                new EnemyAction { Name = "Otrávená šipka", DamageToAll = 12 }, new EnemyAction { Name = "Přeseknutí tepny", DamageToAll = 18 },
                new EnemyAction { Name = "Klam", Heal = 20 }, new EnemyAction { Name = "Zákeřný kop", DamageToAll = 14 },
                new EnemyAction { Name = "Oslepující prášek", DamageToAll = 10 }, new EnemyAction { Name = "Skrytá dýka", DamageToAll = 22 },
                new EnemyAction { Name = "Chladnokrevnost", DamageToAll = 15 }, new EnemyAction { Name = "Útěk do tmy", Heal = 25 }
            }),
            new EnemyTemplate("2_N_04", "Krvavý Mág", 80, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Vařící se krev", DamageToAll = 15 }, new EnemyAction { Name = "Krvavý rituál", DamageToAll = 5, Heal = 25 },
                new EnemyAction { Name = "Oběť", DamageToAll = 20 }, new EnemyAction { Name = "Temný šíp", DamageToAll = 18 },
                new EnemyAction { Name = "Žízeň po krvi", DamageToAll = 10, Heal = 15 }, new EnemyAction { Name = "Hltání Karmy", DamageToAll = 12 },
                new EnemyAction { Name = "Vyvolání", Heal = 30 }, new EnemyAction { Name = "Bolestivé kouzlo", DamageToAll = 16 },
                new EnemyAction { Name = "Vysátí morku", DamageToAll = 14, Heal = 10 }, new EnemyAction { Name = "Upíří aura", DamageToAll = 8, Heal = 20 }
            }),
            new EnemyTemplate("2_N_05", "Písečný Červ", 110, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Spolknutí", DamageToAll = 18 }, new EnemyAction { Name = "Drcení pod zemí", DamageToAll = 14 },
                new EnemyAction { Name = "Erupce z písku", DamageToAll = 22 }, new EnemyAction { Name = "Trávení", Heal = 25 },
                new EnemyAction { Name = "Zemetřesení", DamageToAll = 15 }, new EnemyAction { Name = "Ostré zuby", DamageToAll = 16 },
                new EnemyAction { Name = "Písečný vír", DamageToAll = 12 }, new EnemyAction { Name = "Tunelování", Heal = 20 },
                new EnemyAction { Name = "Skus drtiče", DamageToAll = 25 }, new EnemyAction { Name = "Úkryt", Heal = 30 }
            }),
            new EnemyTemplate("2_N_06", "Prokletý Žoldnéř", 100, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Rána halapartnou", DamageToAll = 16 }, new EnemyAction { Name = "Krvavé zlato", DamageToAll = 10, Heal = 15 },
                new EnemyAction { Name = "Výpad vzteku", DamageToAll = 20 }, new EnemyAction { Name = "Bolest", DamageToAll = 14 },
                new EnemyAction { Name = "Ignorování zranění", Heal = 25 }, new EnemyAction { Name = "Seknutí na krk", DamageToAll = 18 },
                new EnemyAction { Name = "Špinavý trik", DamageToAll = 12 }, new EnemyAction { Name = "Odhodlání", Heal = 20 },
                new EnemyAction { Name = "Zastrašení", DamageToAll = 15 }, new EnemyAction { Name = "Zběsilý útok", DamageToAll = 22 }
            }),
            new EnemyTemplate("2_N_07", "Přízrak Zkázy", 70, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kvílení duší", DamageToAll = 14 }, new EnemyAction { Name = "Mrazivý dotek", DamageToAll = 18 },
                new EnemyAction { Name = "Neviditelnost", Heal = 30 }, new EnemyAction { Name = "Vysávání naděje", DamageToAll = 12, Heal = 15 },
                new EnemyAction { Name = "Strach", DamageToAll = 15 }, new EnemyAction { Name = "Nekrotický výboj", DamageToAll = 20 },
                new EnemyAction { Name = "Hltání esence", DamageToAll = 10, Heal = 20 }, new EnemyAction { Name = "Fantomový šíp", DamageToAll = 16 },
                new EnemyAction { Name = "Ústup", Heal = 20 }, new EnemyAction { Name = "Smrtící stín", DamageToAll = 25 }
            }),
            new EnemyTemplate("2_N_08", "Hladový Ghúl", 95, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kousnutí do kosti", DamageToAll = 16 }, new EnemyAction { Name = "Trhání masa", DamageToAll = 14, Heal = 10 },
                new EnemyAction { Name = "Zuřivý hlad", DamageToAll = 20 }, new EnemyAction { Name = "Požírání mrtvol", Heal = 35 },
                new EnemyAction { Name = "Zákeřný skok", DamageToAll = 15 }, new EnemyAction { Name = "Nakažlivý dech", DamageToAll = 12 },
                new EnemyAction { Name = "Infekce", DamageToAll = 10 }, new EnemyAction { Name = "Zběsilost", DamageToAll = 18 },
                new EnemyAction { Name = "Ohryzávání", DamageToAll = 13, Heal = 15 }, new EnemyAction { Name = "Šílenství", DamageToAll = 22 }
            }),
            new EnemyTemplate("2_N_09", "Jedovatá Zmije", 65, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Zuby plné jedu", DamageToAll = 18 }, new EnemyAction { Name = "Rychlé uštknutí", DamageToAll = 22 },
                new EnemyAction { Name = "Svléknutí kůže", Heal = 25 }, new EnemyAction { Name = "Ovinutí", DamageToAll = 15 },
                new EnemyAction { Name = "Slepý útok", DamageToAll = 20 }, new EnemyAction { Name = "Toxický mrak", DamageToAll = 14 },
                new EnemyAction { Name = "Regenerace šupin", Heal = 20 }, new EnemyAction { Name = "Smrtící skus", DamageToAll = 25 },
                new EnemyAction { Name = "Vykouknutí z písku", DamageToAll = 16 }, new EnemyAction { Name = "Prsknutí kyseliny", DamageToAll = 19 }
            }),
            new EnemyTemplate("2_N_10", "Kultistický Fanatik", 85, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Temné kázání", DamageToAll = 15 }, new EnemyAction { Name = "Krvavá oběť", DamageToAll = 25 },
                new EnemyAction { Name = "Soustředění temnoty", Heal = 30 }, new EnemyAction { Name = "Výbuch šílenství", DamageToAll = 20 },
                new EnemyAction { Name = "Modlitba k Pánovi", Heal = 25 }, new EnemyAction { Name = "Dýka do srdce", DamageToAll = 18 },
                new EnemyAction { Name = "Zákeřná kletba", DamageToAll = 12 }, new EnemyAction { Name = "Hromadná oběť", DamageToAll = 22 },
                new EnemyAction { Name = "Získání síly", DamageToAll = 10, Heal = 15 }, new EnemyAction { Name = "Totální odevzdání", DamageToAll = 28 }
            }),

            new EnemyTemplate("2_E_01", "Anubis, Hlídač Hrobek", 250, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Vážení duší", DamageToAll = 25 }, new EnemyAction { Name = "Písek času", DamageToAll = 20, Heal = 20 },
                new EnemyAction { Name = "Soudný pohled", DamageToAll = 30 }, new EnemyAction { Name = "Úder žezlem", DamageToAll = 28 },
                new EnemyAction { Name = "Prokletí hrobky", DamageToAll = 22 }, new EnemyAction { Name = "Balzamovací rituál", Heal = 50 },
                new EnemyAction { Name = "Temný záblesk", DamageToAll = 18 }, new EnemyAction { Name = "Pohlcení esence", DamageToAll = 15, Heal = 30 },
                new EnemyAction { Name = "Krok boha", DamageToAll = 35 }, new EnemyAction { Name = "Strážní aura", Heal = 40 }
            }),
            new EnemyTemplate("2_E_02", "Královská Mumie", 280, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kletba faraonů", DamageToAll = 28 }, new EnemyAction { Name = "Písečný hrob", DamageToAll = 35 },
                new EnemyAction { Name = "Zlatý sarkofág", Heal = 60 }, new EnemyAction { Name = "Zardoušení", DamageToAll = 25 },
                new EnemyAction { Name = "Probuzení služebníků", Heal = 40 }, new EnemyAction { Name = "Zlověstný pád", DamageToAll = 20 },
                new EnemyAction { Name = "Vysátí mládí", DamageToAll = 18, Heal = 20 }, new EnemyAction { Name = "Mor", DamageToAll = 24 },
                new EnemyAction { Name = "Rozkaz zkázy", DamageToAll = 30 }, new EnemyAction { Name = "Ticho smrti", DamageToAll = 22 }
            }),
            new EnemyTemplate("2_E_03", "Pán Písku", 300, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Písečné tornádo", DamageToAll = 35 }, new EnemyAction { Name = "Pohlcení do duny", DamageToAll = 28 },
                new EnemyAction { Name = "Obnova zrnek", Heal = 70 }, new EnemyAction { Name = "Tvrdý balvan", DamageToAll = 25 },
                new EnemyAction { Name = "Zemětřesení", DamageToAll = 30 }, new EnemyAction { Name = "Skleněný meč", DamageToAll = 22 },
                new EnemyAction { Name = "Oslepující bouře", DamageToAll = 20 }, new EnemyAction { Name = "Krystalická kůže", Heal = 50 },
                new EnemyAction { Name = "Písečný vír", DamageToAll = 26 }, new EnemyAction { Name = "Hněv pouště", DamageToAll = 40 }
            }),
            new EnemyTemplate("2_E_04", "Temný Džin", 240, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Tři kletby", DamageToAll = 32 }, new EnemyAction { Name = "Magický plamen", DamageToAll = 28 },
                new EnemyAction { Name = "Splněné přání smrti", DamageToAll = 40 }, new EnemyAction { Name = "Nasátí magie", Heal = 60 },
                new EnemyAction { Name = "Oheň a kouř", DamageToAll = 25 }, new EnemyAction { Name = "Zrcadlový klam", Heal = 40 },
                new EnemyAction { Name = "Drtivý stisk", DamageToAll = 30 }, new EnemyAction { Name = "Smrtící iluze", DamageToAll = 20 },
                new EnemyAction { Name = "Plazmový výboj", DamageToAll = 26 }, new EnemyAction { Name = "Ničivé kouzlo", DamageToAll = 35 }
            }),
            new EnemyTemplate("2_E_05", "Hlídač Podsvětí", 260, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Trojitý skus", DamageToAll = 35 }, new EnemyAction { Name = "Brána utrpení", DamageToAll = 30 },
                new EnemyAction { Name = "Hltání hříšníků", Heal = 60 }, new EnemyAction { Name = "Hořící řetězy", DamageToAll = 28 },
                new EnemyAction { Name = "Oheň podsvětí", DamageToAll = 25 }, new EnemyAction { Name = "Řev temnot", DamageToAll = 22 },
                new EnemyAction { Name = "Očištění krví", Heal = 50 }, new EnemyAction { Name = "Zkáza těl", DamageToAll = 32 },
                new EnemyAction { Name = "Bič bolesti", DamageToAll = 26 }, new EnemyAction { Name = "Vymazání naděje", DamageToAll = 38 }
            }),

            new EnemyTemplate("2_B_01", "Faraon Zatracených", 600, "Boss", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Božský soud", DamageToAll = 40 }, new EnemyAction { Name = "Slzy nebes", Heal = 100 },
                new EnemyAction { Name = "Plagues of Egypt", DamageToAll = 35 }, new EnemyAction { Name = "Zkáza království", DamageToAll = 45 },
                new EnemyAction { Name = "Slovo Faraona", DamageToAll = 30, Heal = 30 }, new EnemyAction { Name = "Pyramida stínů", Heal = 80 },
                new EnemyAction { Name = "Oko Hora", DamageToAll = 25 }, new EnemyAction { Name = "Rozkaz k popravě", DamageToAll = 50 },
                new EnemyAction { Name = "Nesmrtelnost", Heal = 120 }, new EnemyAction { Name = "Slunce v zatmění", DamageToAll = 38 }
            }),
            new EnemyTemplate("2_B_02", "Písečný Leviatan", 700, "Boss", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Polknutí celého týmu", DamageToAll = 55 }, new EnemyAction { Name = "Písečné tsunami", DamageToAll = 45 },
                new EnemyAction { Name = "Erupce z hlubin", DamageToAll = 50 }, new EnemyAction { Name = "Trávení obětí", Heal = 150 },
                new EnemyAction { Name = "Hluboký ponor", Heal = 100 }, new EnemyAction { Name = "Drtivý skus", DamageToAll = 40 },
                new EnemyAction { Name = "Třes třiceti stupňů", DamageToAll = 35 }, new EnemyAction { Name = "Smrtící písek", DamageToAll = 38 },
                new EnemyAction { Name = "Pohlcení magie", Heal = 80 }, new EnemyAction { Name = "Absolutní destrukce", DamageToAll = 60 }
            }),
            new EnemyTemplate("2_B_03", "Archdémon Karmy", 650, "Boss", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Rozbití vah", DamageToAll = 45 }, new EnemyAction { Name = "Těžký hřích", DamageToAll = 50 },
                new EnemyAction { Name = "Očištění skrze oheň", Heal = 120 }, new EnemyAction { Name = "Karmická pomsta", DamageToAll = 40, Heal = 40 },
                new EnemyAction { Name = "Pád do nicoty", DamageToAll = 55 }, new EnemyAction { Name = "Absolutní temnota", DamageToAll = 35 },
                new EnemyAction { Name = "Oslňující smrt", DamageToAll = 38 }, new EnemyAction { Name = "Kradení štěstí", Heal = 90 },
                new EnemyAction { Name = "Vymazání rovnováhy", DamageToAll = 48 }, new EnemyAction { Name = "Konečný soud", DamageToAll = 65 }
            }),

            // ==========================================
            // ACT 3 - NORMÁLNÍ (10), ELITY (5), BOSSOVÉ (3)
            // ==========================================
            new EnemyTemplate("3_N_01", "Stín Prázdnoty", 180, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Pohlcení světla", DamageToAll = 15, Heal = 10 }, new EnemyAction { Name = "Absolutní tma", DamageToAll = 22 },
                new EnemyAction { Name = "Zkroucení mysli", DamageToAll = 18 }, new EnemyAction { Name = "Roztrhání reality", DamageToAll = 25 },
                new EnemyAction { Name = "Krok do nikam", Heal = 30 }, new EnemyAction { Name = "Nekonečný pád", DamageToAll = 20 },
                new EnemyAction { Name = "Mrazivý prázdno", DamageToAll = 14, Heal = 15 }, new EnemyAction { Name = "Dotek propasti", DamageToAll = 28 },
                new EnemyAction { Name = "Vymazání vzpomínek", DamageToAll = 16 }, new EnemyAction { Name = "Hladová temnota", Heal = 40 }
            }),
            new EnemyTemplate("3_N_02", "Vymahač Karmy", 220, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Těžké břemeno", DamageToAll = 25 }, new EnemyAction { Name = "Vyrovnání dluhů", DamageToAll = 15, Heal = 20 },
                new EnemyAction { Name = "Úder kladivem osudu", DamageToAll = 30 }, new EnemyAction { Name = "Slepá spravedlnost", DamageToAll = 22 },
                new EnemyAction { Name = "Zadržení", Heal = 25 }, new EnemyAction { Name = "Drcení karmy", DamageToAll = 28 },
                new EnemyAction { Name = "Očistný soud", DamageToAll = 20 }, new EnemyAction { Name = "Trest za pýchu", DamageToAll = 35 },
                new EnemyAction { Name = "Absoluce", Heal = 40 }, new EnemyAction { Name = "Rozsudek smrti", DamageToAll = 24 }
            }),
            new EnemyTemplate("3_N_03", "Padlý Anděl", 200, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Krvavá peří", DamageToAll = 18 }, new EnemyAction { Name = "Ztracená milost", DamageToAll = 24 },
                new EnemyAction { Name = "Zářivý smutek", Heal = 35 }, new EnemyAction { Name = "Odsouzení nebes", DamageToAll = 32 },
                new EnemyAction { Name = "Nářek", DamageToAll = 15, Heal = 15 }, new EnemyAction { Name = "Planoucí meč", DamageToAll = 28 },
                new EnemyAction { Name = "Zborcení křídel", DamageToAll = 22 }, new EnemyAction { Name = "Pokání", Heal = 45 },
                new EnemyAction { Name = "Božský hněv", DamageToAll = 35 }, new EnemyAction { Name = "Poslední modlitba", DamageToAll = 20 }
            }),
            new EnemyTemplate("3_N_04", "Krystalický Golem", 250, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Tříštění prostoru", DamageToAll = 20 }, new EnemyAction { Name = "Zpevnění hran", Heal = 40 },
                new EnemyAction { Name = "Krystalový déšť", DamageToAll = 28 }, new EnemyAction { Name = "Lom světla", DamageToAll = 15, Heal = 20 },
                new EnemyAction { Name = "Drtivý krok", DamageToAll = 35 }, new EnemyAction { Name = "Odraz poškození", DamageToAll = 25 },
                new EnemyAction { Name = "Záření", Heal = 30 }, new EnemyAction { Name = "Rezonance", DamageToAll = 22 },
                new EnemyAction { Name = "Ostré úlomky", DamageToAll = 18 }, new EnemyAction { Name = "Geoda zkázy", DamageToAll = 30 }
            }),
            new EnemyTemplate("3_N_05", "Oko Propasti", 160, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Pronikavý pohled", DamageToAll = 30 }, new EnemyAction { Name = "Rozklad hmoty", DamageToAll = 25 },
                new EnemyAction { Name = "Zaostření", Heal = 25 }, new EnemyAction { Name = "Paprsek nicoty", DamageToAll = 40 },
                new EnemyAction { Name = "Mrknutí smrti", DamageToAll = 20 }, new EnemyAction { Name = "Vize hrůzy", DamageToAll = 28 },
                new EnemyAction { Name = "Krvácení sítnice", DamageToAll = 18, Heal = 10 }, new EnemyAction { Name = "Ukrutná jasnozřivost", Heal = 35 },
                new EnemyAction { Name = "Pláč propasti", DamageToAll = 22 }, new EnemyAction { Name = "Vypálení duše", DamageToAll = 35 }
            }),
            new EnemyTemplate("3_N_06", "Krvavá Valkýra", 190, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Nálet", DamageToAll = 28 }, new EnemyAction { Name = "Kopí osudu", DamageToAll = 35 },
                new EnemyAction { Name = "Sběr duší", Heal = 40 }, new EnemyAction { Name = "Odměna Valhally", DamageToAll = 20, Heal = 20 },
                new EnemyAction { Name = "Krvavý tanec", DamageToAll = 30 }, new EnemyAction { Name = "Válečný pokřik", DamageToAll = 25 },
                new EnemyAction { Name = "Seknutí křídlem", DamageToAll = 22 }, new EnemyAction { Name = "Triumf", Heal = 30 },
                new EnemyAction { Name = "Poslední soud", DamageToAll = 38 }, new EnemyAction { Name = "Vražedná spirála", DamageToAll = 26 }
            }),
            new EnemyTemplate("3_N_07", "Požírač Myšlenek", 175, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Vysátí intelektu", DamageToAll = 25, Heal = 25 }, new EnemyAction { Name = "Ztráta paměti", DamageToAll = 32 },
                new EnemyAction { Name = "Noční můra", DamageToAll = 28 }, new EnemyAction { Name = "Hostina rozumu", Heal = 50 },
                new EnemyAction { Name = "Halucinace", DamageToAll = 20 }, new EnemyAction { Name = "Mentální blok", DamageToAll = 24 },
                new EnemyAction { Name = "Mozková mrtvice", DamageToAll = 38 }, new EnemyAction { Name = "Paranoia", DamageToAll = 22 },
                new EnemyAction { Name = "Spánek", Heal = 35 }, new EnemyAction { Name = "Útok na nervy", DamageToAll = 26 }
            }),
            new EnemyTemplate("3_N_08", "Zkroucený Mnich", 210, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Úder tisíce dlaní", DamageToAll = 30 }, new EnemyAction { Name = "Zakázaná mantra", Heal = 45 },
                new EnemyAction { Name = "Bod zlomu", DamageToAll = 35 }, new EnemyAction { Name = "Tichá bolest", DamageToAll = 20 },
                new EnemyAction { Name = "Obrácený Zen", DamageToAll = 28 }, new EnemyAction { Name = "Zastavení srdce", DamageToAll = 40 },
                new EnemyAction { Name = "Medidace krve", DamageToAll = 15, Heal = 30 }, new EnemyAction { Name = "Narušení toku", DamageToAll = 25 },
                new EnemyAction { Name = "Očista", Heal = 40 }, new EnemyAction { Name = "Smrtící aura", DamageToAll = 22 }
            }),
            new EnemyTemplate("3_N_09", "Kosmický Horor", 230, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Mimo prostor", Heal = 50 }, new EnemyAction { Name = "Uchopení chapadly", DamageToAll = 32 },
                new EnemyAction { Name = "Nepochopitelný tvar", DamageToAll = 25 }, new EnemyAction { Name = "Ztráta příčetnosti", DamageToAll = 38 },
                new EnemyAction { Name = "Mezidimenzionální trhlina", DamageToAll = 20, Heal = 20 }, new EnemyAction { Name = "Píseň hvězd", DamageToAll = 28 },
                new EnemyAction { Name = "Rozklad atomů", DamageToAll = 35 }, new EnemyAction { Name = "Vdechnutí vakua", Heal = 40 },
                new EnemyAction { Name = "Šepot prastarých", DamageToAll = 22 }, new EnemyAction { Name = "Extinkce", DamageToAll = 42 }
            }),
            new EnemyTemplate("3_N_10", "Zvěstovatel Zkázy", 150, "Normal", 3, new List<EnemyAction> { 
                new EnemyAction { Name = "Apokalypsa hned", DamageToAll = 45 }, new EnemyAction { Name = "První polnice", DamageToAll = 35 },
                new EnemyAction { Name = "Otevírání pečetí", Heal = 30 }, new EnemyAction { Name = "Krvavý déšť", DamageToAll = 38 },
                new EnemyAction { Name = "Zatěžkávací zkouška", DamageToAll = 28 }, new EnemyAction { Name = "Pád meteoritu", DamageToAll = 50 },
                new EnemyAction { Name = "Píseň konce", DamageToAll = 30, Heal = 10 }, new EnemyAction { Name = "Zhasnutí slunce", DamageToAll = 42 },
                new EnemyAction { Name = "Zádušní mše", Heal = 40 }, new EnemyAction { Name = "Konečný akord", DamageToAll = 35 }
            }),

            new EnemyTemplate("3_E_01", "Strážce Rovnováhy", 450, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Kalibrace osudu", DamageToAll = 35 }, new EnemyAction { Name = "Kosmické misky vah", Heal = 60 },
                new EnemyAction { Name = "Absolutní neutralita", DamageToAll = 45 }, new EnemyAction { Name = "Vykoupení", DamageToAll = 25, Heal = 30 },
                new EnemyAction { Name = "Srovnání skóre", DamageToAll = 38 }, new EnemyAction { Name = "Zákaz změn", DamageToAll = 40 },
                new EnemyAction { Name = "Tíha bytí", DamageToAll = 30 }, new EnemyAction { Name = "Znovuzrození hmoty", Heal = 80 },
                new EnemyAction { Name = "Úder spravedlnosti", DamageToAll = 50 }, new EnemyAction { Name = "Resetování", DamageToAll = 35 }
            }),
            new EnemyTemplate("3_E_02", "Avatar Temnoty", 400, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Pohlcení světla", DamageToAll = 40 }, new EnemyAction { Name = "Černá díra", DamageToAll = 55 },
                new EnemyAction { Name = "Nasátí zoufalství", Heal = 70 }, new EnemyAction { Name = "Dusivý stín", DamageToAll = 35 },
                new EnemyAction { Name = "Exploze prázdnoty", DamageToAll = 50 }, new EnemyAction { Name = "Mráz mrtvých hvězd", DamageToAll = 38 },
                new EnemyAction { Name = "Závoj ticha", Heal = 50 }, new EnemyAction { Name = "Nekonečná noc", DamageToAll = 42 },
                new EnemyAction { Name = "Kradení esence", DamageToAll = 30, Heal = 40 }, new EnemyAction { Name = "Zničení naděje", DamageToAll = 60 }
            }),
            new EnemyTemplate("3_E_03", "Nebešťan Hněvu", 420, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Boží trest", DamageToAll = 48 }, new EnemyAction { Name = "Spalující plamen", DamageToAll = 35 },
                new EnemyAction { Name = "Zlaté kopí", DamageToAll = 55 }, new EnemyAction { Name = "Aura nezranitelnosti", Heal = 80 },
                new EnemyAction { Name = "Srážení k zemi", DamageToAll = 42 }, new EnemyAction { Name = "Soudný blesk", DamageToAll = 50 },
                new EnemyAction { Name = "Očistný oheň", DamageToAll = 38, Heal = 20 }, new EnemyAction { Name = "Modlitba zkázy", Heal = 60 },
                new EnemyAction { Name = "Křídla spravedlnosti", DamageToAll = 45 }, new EnemyAction { Name = "Neúprosný tlak", DamageToAll = 40 }
            }),
            new EnemyTemplate("3_E_04", "Prázdný Drak", 500, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Dech antihmoty", DamageToAll = 60 }, new EnemyAction { Name = "Mávnutí prázdnotou", DamageToAll = 45 },
                new EnemyAction { Name = "Regenerace z hvězd", Heal = 100 }, new EnemyAction { Name = "Roztříštění dimenze", DamageToAll = 55 },
                new EnemyAction { Name = "Čelist nicoty", DamageToAll = 50 }, new EnemyAction { Name = "Pád komety", DamageToAll = 65 },
                new EnemyAction { Name = "Nekonečný spánek", Heal = 80 }, new EnemyAction { Name = "Ocasní drtič", DamageToAll = 48 },
                new EnemyAction { Name = "Anomálie", DamageToAll = 40, Heal = 30 }, new EnemyAction { Name = "Kosmický řev", DamageToAll = 52 }
            }),
            new EnemyTemplate("3_E_05", "Velekněz Konce", 380, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Rituál zkázy", DamageToAll = 45 }, new EnemyAction { Name = "Čtení z nekronomikonu", Heal = 75 },
                new EnemyAction { Name = "Krvavý obětní dar", DamageToAll = 55 }, new EnemyAction { Name = "Zastření mysli", DamageToAll = 40 },
                new EnemyAction { Name = "Proklínání pokolení", DamageToAll = 38 }, new EnemyAction { Name = "Zakázané kouzlo", DamageToAll = 60 },
                new EnemyAction { Name = "Temná liturgie", Heal = 60 }, new EnemyAction { Name = "Extrakce duše", DamageToAll = 35, Heal = 35 },
                new EnemyAction { Name = "Slovo moci", DamageToAll = 50 }, new EnemyAction { Name = "Předzvěst", DamageToAll = 42 }
            }),

            new EnemyTemplate("3_B_01", "Ztělesnění Karmy", 1200, "Boss", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Absolutní soud", DamageToAll = 65 }, new EnemyAction { Name = "Vynulování rovnováhy", DamageToAll = 55 },
                new EnemyAction { Name = "Vysátí veškeré naděje", Heal = 150 }, new EnemyAction { Name = "Zrcadlení hříchů", DamageToAll = 75 },
                new EnemyAction { Name = "Posvátný trest", DamageToAll = 60, Heal = 50 }, new EnemyAction { Name = "Tíha vesmíru", DamageToAll = 80 },
                new EnemyAction { Name = "Nekonečný cyklus", DamageToAll = 50 }, new EnemyAction { Name = "Rozhřešení", Heal = 200 },
                new EnemyAction { Name = "Zkáza eonů", DamageToAll = 70 }, new EnemyAction { Name = "Smazání existence", DamageToAll = 85 }
            }),
            new EnemyTemplate("3_B_02", "Bůh Prázdnoty", 1500, "Boss", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Roztržení reality", DamageToAll = 80 }, new EnemyAction { Name = "Pohlcení dimenze", Heal = 250 },
                new EnemyAction { Name = "Výdech mrtvé hvězdy", DamageToAll = 70 }, new EnemyAction { Name = "Zastavení času", DamageToAll = 65 },
                new EnemyAction { Name = "Temná singularita", DamageToAll = 90 }, new EnemyAction { Name = "Křik padlých světů", DamageToAll = 60 },
                new EnemyAction { Name = "Nulová entropie", Heal = 180 }, new EnemyAction { Name = "Srážka galaxií", DamageToAll = 85 },
                new EnemyAction { Name = "Černý déšť", DamageToAll = 75 }, new EnemyAction { Name = "Návrat do nicoty", DamageToAll = 100 }
            }),
            new EnemyTemplate("3_B_03", "Královna Zlomených Duší", 1300, "Boss", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Chorus bolesti", DamageToAll = 65 }, new EnemyAction { Name = "Dotek agónie", DamageToAll = 85 },
                new EnemyAction { Name = "Sklizeň padlých", Heal = 200 }, new EnemyAction { Name = "Výkřik tisíce úst", DamageToAll = 75 },
                new EnemyAction { Name = "Paralýza srdce", DamageToAll = 60 }, new EnemyAction { Name = "Zlomit vůli", DamageToAll = 70 },
                new EnemyAction { Name = "Obnova z těl", Heal = 150 }, new EnemyAction { Name = "Tep neštěstí", DamageToAll = 55, Heal = 55 },
                new EnemyAction { Name = "Tříštění mysli", DamageToAll = 80 }, new EnemyAction { Name = "Nekonečný žal", DamageToAll = 95 }
            })
        };

        // --- CHYTRÉ FUNKCE PRO VÝBĚR ---
        public static EnemyTemplate GetRandomEnemy(string tier, int act)
        {
            var possibleEnemies = Enemies.Where(e => e.Tier == tier && e.Act == act).ToList();
            if (possibleEnemies.Count == 0) return Enemies[0];

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