using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    // --- POMOCNÉ TŘÍDY PRO DATABÁZI ---
    public class EnemyAction
    {
        public string Name { get; set; } = string.Empty;
        public int DamageToAll { get; set; } = 0; // Ve 3D to může znamenat plošný útok (AoE) nebo přímý hit
        public int Heal { get; set; } = 0;
    }

    public class EnemyTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxHp { get; set; }
        public string Tier { get; set; }
        public int Act { get; set; }
        public List<EnemyAction> Actions { get; set; }

        // --- 3D STATISTIKY ---
        public float Speed { get; set; } = 2.0f;
        public float AttackRange { get; set; } = 2.0f;
        public float AttackCooldown { get; set; } = 3000f; // v milisekundách (3s)

        public EnemyTemplate(string id, string name, int maxHp, string tier, int act, List<EnemyAction> actions)
        {
            Id = id; Name = name; MaxHp = maxHp; Tier = tier; Act = act; Actions = actions;
        }
    }

    public static class EnemyDatabase
    {
        public static List<EnemyTemplate> Enemies = new List<EnemyTemplate>
        {
            // ================== ACT 1 - NORMÁLNÍ (10x) ==================
            new EnemyTemplate("1_N_01", "Kultista Temnoty", 60, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Dýka ve tmě", DamageToAll = 5 }, new EnemyAction { Name = "Temný rituál", DamageToAll = 2, Heal = 10 },
                new EnemyAction { Name = "Fanatismus", DamageToAll = 7 }, new EnemyAction { Name = "Otrávená čepel", DamageToAll = 4 }
            }) { Speed = 2.5f, AttackRange = 1.5f, AttackCooldown = 2500f },
            
            new EnemyTemplate("1_N_02", "Oživlá Kostra", 45, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Škrábnutí", DamageToAll = 4 }, new EnemyAction { Name = "Hození kosti", DamageToAll = 6 },
                new EnemyAction { Name = "Složení kostí", Heal = 10 }, new EnemyAction { Name = "Kostlivý stisk", DamageToAll = 5 }
            }) { Speed = 1.8f, AttackRange = 1.5f, AttackCooldown = 3500f },

            new EnemyTemplate("1_N_03", "Zdivočelý Vlk", 55, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Kousnutí", DamageToAll = 6 }, new EnemyAction { Name = "Drásání", DamageToAll = 5 },
                new EnemyAction { Name = "Skok na krk", DamageToAll = 9 }, new EnemyAction { Name = "Smečkový útok", DamageToAll = 10 }
            }) { Speed = 4.5f, AttackRange = 2.0f, AttackCooldown = 2000f }, // Rychlý útočník

            new EnemyTemplate("1_N_04", "Zloděj duší", 50, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Krádež", DamageToAll = 4, Heal = 5 }, new EnemyAction { Name = "Stínový krok", Heal = 12 },
                new EnemyAction { Name = "Bodnutí do zad", DamageToAll = 10 }, new EnemyAction { Name = "Zlodějský trik", DamageToAll = 6 }
            }) { Speed = 3.5f, AttackRange = 1.5f, AttackCooldown = 2200f },

            new EnemyTemplate("1_N_05", "Kyselý Sliz", 80, "Normal", 1, new List<EnemyAction> { 
                new EnemyAction { Name = "Žíravina", DamageToAll = 3 }, new EnemyAction { Name = "Pohlcení", DamageToAll = 2, Heal = 5 },
                new EnemyAction { Name = "Kyselý plivanec", DamageToAll = 4 }, new EnemyAction { Name = "Kyselý výbuch", DamageToAll = 8 }
            }) { Speed = 1.2f, AttackRange = 3.0f, AttackCooldown = 4000f }, // Pomalý, ale plive na dálku

            new EnemyTemplate("1_N_06", "Gobliní Zvěd", 40, "Normal", 1, new List<EnemyAction> { 
                new EnemyAction { Name = "Rychlý bod", DamageToAll = 5 }, new EnemyAction { Name = "Hození dýky", DamageToAll = 7 },
                new EnemyAction { Name = "Jedová šipka", DamageToAll = 4 }, new EnemyAction { Name = "Skrytí", Heal = 15 }
            }) { Speed = 4.0f, AttackRange = 4.0f, AttackCooldown = 2500f },

            new EnemyTemplate("1_N_07", "Obří Netopýr", 45, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Sání krve", DamageToAll = 4, Heal = 6 }, new EnemyAction { Name = "Slet shůry", DamageToAll = 7 },
                new EnemyAction { Name = "Upíří polibek", DamageToAll = 6, Heal = 6 }, new EnemyAction { Name = "Tichý lov", DamageToAll = 9 }
            }) { Speed = 5.0f, AttackRange = 1.0f, AttackCooldown = 1800f }, // Extrémně rychlý, musí blízko

            new EnemyTemplate("1_N_08", "Nemrtvý Voják", 70, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Rezavý meč", DamageToAll = 6 }, new EnemyAction { Name = "Úder štítem", DamageToAll = 4 },
                new EnemyAction { Name = "Těžká rána", DamageToAll = 9 }, new EnemyAction { Name = "Krvavá přísaha", DamageToAll = 6, Heal = 5 }
            }) { Speed = 2.0f, AttackRange = 2.0f, AttackCooldown = 3200f },

            new EnemyTemplate("1_N_09", "Lesní Pavouk", 50, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Jedové kousnutí", DamageToAll = 5 }, new EnemyAction { Name = "Vystřelení pavučiny", DamageToAll = 3 },
                new EnemyAction { Name = "Skok ze tmy", DamageToAll = 8 }, new EnemyAction { Name = "Pavučinová past", DamageToAll = 9 }
            }) { Speed = 3.5f, AttackRange = 3.5f, AttackCooldown = 2800f },

            new EnemyTemplate("1_N_10", "Bandita z Cesty", 65, "Normal", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Přepadení", DamageToAll = 8 }, new EnemyAction { Name = "Rána obuškem", DamageToAll = 5 },
                new EnemyAction { Name = "Hození nože", DamageToAll = 6 }, new EnemyAction { Name = "Zoufalý útok", DamageToAll = 10 }
            }) { Speed = 3.0f, AttackRange = 2.0f, AttackCooldown = 2600f },

            // ================== ACT 1 - ELITY (5x) ==================
            new EnemyTemplate("1_E_01", "Temný Rytíř", 150, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Těžký sek", DamageToAll = 12 }, new EnemyAction { Name = "Krvavá žeň", DamageToAll = 8, Heal = 15 },
                new EnemyAction { Name = "Drtivý dopad", DamageToAll = 15 }, new EnemyAction { Name = "Brutální výpad", DamageToAll = 14 }
            }) { Speed = 2.5f, AttackRange = 2.5f, AttackCooldown = 3500f },

            new EnemyTemplate("1_E_02", "Démonický Inkvizitor", 140, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Očistný oheň", DamageToAll = 14 }, new EnemyAction { Name = "Soud", DamageToAll = 18 },
                new EnemyAction { Name = "Hořící kříž", DamageToAll = 12, Heal = 5 }, new EnemyAction { Name = "Zúčtování", DamageToAll = 20 }
            }) { Speed = 2.0f, AttackRange = 6.0f, AttackCooldown = 4000f }, // Kástí z dálky

            new EnemyTemplate("1_E_03", "Krvavý Řezník", 160, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Sekáček", DamageToAll = 15 }, new EnemyAction { Name = "Hák na maso", DamageToAll = 10, Heal = 10 },
                new EnemyAction { Name = "Masakr", DamageToAll = 22 }, new EnemyAction { Name = "Záchvat zuřivosti", DamageToAll = 20 }
            }) { Speed = 3.2f, AttackRange = 2.0f, AttackCooldown = 3000f },

            new EnemyTemplate("1_E_04", "Kamenný Golem", 200, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Zemětřesení", DamageToAll = 10 }, new EnemyAction { Name = "Vržený balvan", DamageToAll = 18 },
                new EnemyAction { Name = "Tlaková vlna", DamageToAll = 12 }, new EnemyAction { Name = "Rozdupnutí", DamageToAll = 22 }
            }) { Speed = 1.0f, AttackRange = 5.0f, AttackCooldown = 5000f }, // Pomalý, ale trefuje zdaleka

            new EnemyTemplate("1_E_05", "Nekromant", 130, "Elite", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Oživení mrtvých", DamageToAll = 8, Heal = 10 }, new EnemyAction { Name = "Zápach hniloby", DamageToAll = 12 },
                new EnemyAction { Name = "Dotek smrti", DamageToAll = 18 }, new EnemyAction { Name = "Zákeřný blesk", DamageToAll = 15 }
            }) { Speed = 2.2f, AttackRange = 8.0f, AttackCooldown = 4500f },

            // ================== ACT 1 - BOSSOVÉ (3x) ==================
            new EnemyTemplate("1_B_01", "Pán Karmy", 350, "Boss", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Zákon Rovnováhy", DamageToAll = 15, Heal = 15 }, new EnemyAction { Name = "Soudný den", DamageToAll = 25 },
                new EnemyAction { Name = "Vymazání z existence", DamageToAll = 30 }, new EnemyAction { Name = "Drtivá tíha osudu", DamageToAll = 20 }
            }) { Speed = 2.8f, AttackRange = 10.0f, AttackCooldown = 4000f },

            new EnemyTemplate("1_B_02", "Prastarý Drak", 400, "Boss", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Ohnivý dech", DamageToAll = 20 }, new EnemyAction { Name = "Úder ocasem", DamageToAll = 25 },
                new EnemyAction { Name = "Drtivé čelisti", DamageToAll = 30 }, new EnemyAction { Name = "Armagedon", DamageToAll = 35 }
            }) { Speed = 3.5f, AttackRange = 12.0f, AttackCooldown = 4500f },

            new EnemyTemplate("1_B_03", "Královna Pavouků", 320, "Boss", 1, new List<EnemyAction> {
                new EnemyAction { Name = "Kyselý jed", DamageToAll = 18 }, new EnemyAction { Name = "Smrtící stisk", DamageToAll = 26 },
                new EnemyAction { Name = "Zákeřný skok", DamageToAll = 24 }, new EnemyAction { Name = "Zkáza hnízda", DamageToAll = 32 }
            }) { Speed = 4.0f, AttackRange = 6.0f, AttackCooldown = 3200f },

            // ================== ACT 2 - NORMÁLNÍ (10x) ==================
            new EnemyTemplate("2_N_01", "Pouštní Štír", 85, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Bodnutí hrotem", DamageToAll = 12 }, new EnemyAction { Name = "Sevření klepet", DamageToAll = 15 }
            }) { Speed = 3.0f, AttackRange = 2.0f, AttackCooldown = 2500f },
            new EnemyTemplate("2_N_02", "Oživlá Mumie", 95, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kletba faraonů", DamageToAll = 14 }, new EnemyAction { Name = "Těžký krok", DamageToAll = 16 }
            }) { Speed = 1.5f, AttackRange = 2.5f, AttackCooldown = 3500f },
            new EnemyTemplate("2_N_03", "Stínový Asasín", 75, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kritický zásah", DamageToAll = 25 }, new EnemyAction { Name = "Skrytá dýka", DamageToAll = 22 }
            }) { Speed = 5.0f, AttackRange = 1.5f, AttackCooldown = 1800f },
            new EnemyTemplate("2_N_04", "Krvavý Mág", 80, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Vařící se krev", DamageToAll = 15 }, new EnemyAction { Name = "Temný šíp", DamageToAll = 18 }
            }) { Speed = 2.5f, AttackRange = 8.0f, AttackCooldown = 3800f },
            new EnemyTemplate("2_N_05", "Písečný Červ", 110, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Spolknutí", DamageToAll = 18 }, new EnemyAction { Name = "Erupce z písku", DamageToAll = 22 }
            }) { Speed = 3.5f, AttackRange = 4.0f, AttackCooldown = 4000f },
            new EnemyTemplate("2_N_06", "Prokletý Žoldnéř", 100, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Rána halapartnou", DamageToAll = 16 }, new EnemyAction { Name = "Zběsilý útok", DamageToAll = 22 }
            }) { Speed = 3.0f, AttackRange = 3.0f, AttackCooldown = 3000f },
            new EnemyTemplate("2_N_07", "Přízrak Zkázy", 70, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Kvílení duší", DamageToAll = 14 }, new EnemyAction { Name = "Nekrotický výboj", DamageToAll = 20 }
            }) { Speed = 4.0f, AttackRange = 5.0f, AttackCooldown = 2800f },
            new EnemyTemplate("2_N_08", "Hladový Ghúl", 95, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Zuřivý hlad", DamageToAll = 20 }, new EnemyAction { Name = "Kousnutí do kosti", DamageToAll = 16 }
            }) { Speed = 3.8f, AttackRange = 1.5f, AttackCooldown = 2400f },
            new EnemyTemplate("2_N_09", "Jedovatá Zmije", 65, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Zuby plné jedu", DamageToAll = 18 }, new EnemyAction { Name = "Rychlé uštknutí", DamageToAll = 22 }
            }) { Speed = 4.5f, AttackRange = 2.0f, AttackCooldown = 2000f },
            new EnemyTemplate("2_N_10", "Kultistický Fanatik", 85, "Normal", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Krvavá oběť", DamageToAll = 25 }, new EnemyAction { Name = "Výbuch šílenství", DamageToAll = 20 }
            }) { Speed = 2.8f, AttackRange = 4.0f, AttackCooldown = 3200f },

            // ================== ACT 2 - ELITY (5x) ==================
            new EnemyTemplate("2_E_01", "Anubis, Hlídač Hrobek", 250, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Soudný pohled", DamageToAll = 30 }, new EnemyAction { Name = "Úder žezlem", DamageToAll = 28 }
            }) { Speed = 3.2f, AttackRange = 4.0f, AttackCooldown = 3500f },
            new EnemyTemplate("2_E_02", "Královská Mumie", 280, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Písečný hrob", DamageToAll = 35 }, new EnemyAction { Name = "Rozkaz zkázy", DamageToAll = 30 }
            }) { Speed = 2.0f, AttackRange = 6.0f, AttackCooldown = 4000f },
            new EnemyTemplate("2_E_03", "Pán Písku", 300, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Písečné tornádo", DamageToAll = 35 }, new EnemyAction { Name = "Hněv pouště", DamageToAll = 40 }
            }) { Speed = 2.5f, AttackRange = 8.0f, AttackCooldown = 4200f },
            new EnemyTemplate("2_E_04", "Temný Džin", 240, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Tři kletby", DamageToAll = 32 }, new EnemyAction { Name = "Splněné přání smrti", DamageToAll = 40 }
            }) { Speed = 4.0f, AttackRange = 10.0f, AttackCooldown = 3500f },
            new EnemyTemplate("2_E_05", "Hlídač Podsvětí", 260, "Elite", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Trojitý skus", DamageToAll = 35 }, new EnemyAction { Name = "Vymazání naděje", DamageToAll = 38 }
            }) { Speed = 3.5f, AttackRange = 3.0f, AttackCooldown = 3000f },

            // ================== ACT 2 - BOSSOVÉ (3x) ==================
            new EnemyTemplate("2_B_01", "Faraon Zatracených", 600, "Boss", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Božský soud", DamageToAll = 40 }, new EnemyAction { Name = "Rozkaz k popravě", DamageToAll = 50 }
            }) { Speed = 2.5f, AttackRange = 15.0f, AttackCooldown = 4500f },
            new EnemyTemplate("2_B_02", "Písečný Leviatan", 700, "Boss", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Polknutí celého týmu", DamageToAll = 55 }, new EnemyAction { Name = "Absolutní destrukce", DamageToAll = 60 }
            }) { Speed = 4.0f, AttackRange = 20.0f, AttackCooldown = 5000f },
            new EnemyTemplate("2_B_03", "Archdémon Karmy", 650, "Boss", 2, new List<EnemyAction> {
                new EnemyAction { Name = "Pád do nicoty", DamageToAll = 55 }, new EnemyAction { Name = "Konečný soud", DamageToAll = 65 }
            }) { Speed = 3.8f, AttackRange = 12.0f, AttackCooldown = 4000f },

            // ================== ACT 3 - NORMÁLNÍ (10x) ==================
            new EnemyTemplate("3_N_01", "Stín Prázdnoty", 180, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Absolutní tma", DamageToAll = 22 }, new EnemyAction { Name = "Dotek propasti", DamageToAll = 28 }
            }) { Speed = 4.0f, AttackRange = 5.0f, AttackCooldown = 2800f },
            new EnemyTemplate("3_N_02", "Vymahač Karmy", 220, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Úder kladivem osudu", DamageToAll = 30 }, new EnemyAction { Name = "Trest za pýchu", DamageToAll = 35 }
            }) { Speed = 2.8f, AttackRange = 3.5f, AttackCooldown = 3200f },
            new EnemyTemplate("3_N_03", "Padlý Anděl", 200, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Odsouzení nebes", DamageToAll = 32 }, new EnemyAction { Name = "Božský hněv", DamageToAll = 35 }
            }) { Speed = 4.5f, AttackRange = 6.0f, AttackCooldown = 3000f },
            new EnemyTemplate("3_N_04", "Krystalický Golem", 250, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Drtivý krok", DamageToAll = 35 }, new EnemyAction { Name = "Geoda zkázy", DamageToAll = 30 }
            }) { Speed = 1.5f, AttackRange = 4.0f, AttackCooldown = 4500f },
            new EnemyTemplate("3_N_05", "Oko Propasti", 160, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Paprsek nicoty", DamageToAll = 40 }, new EnemyAction { Name = "Vypálení duše", DamageToAll = 35 }
            }) { Speed = 2.0f, AttackRange = 15.0f, AttackCooldown = 3800f },
            new EnemyTemplate("3_N_06", "Krvavá Valkýra", 190, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Kopí osudu", DamageToAll = 35 }, new EnemyAction { Name = "Poslední soud", DamageToAll = 38 }
            }) { Speed = 5.0f, AttackRange = 4.0f, AttackCooldown = 2500f },
            new EnemyTemplate("3_N_07", "Požírač Myšlenek", 175, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Ztráta paměti", DamageToAll = 32 }, new EnemyAction { Name = "Mozková mrtvice", DamageToAll = 38 }
            }) { Speed = 3.5f, AttackRange = 6.0f, AttackCooldown = 3500f },
            new EnemyTemplate("3_N_08", "Zkroucený Mnich", 210, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Úder tisíce dlaní", DamageToAll = 30 }, new EnemyAction { Name = "Zastavení srdce", DamageToAll = 40 }
            }) { Speed = 4.2f, AttackRange = 2.0f, AttackCooldown = 2200f },
            new EnemyTemplate("3_N_09", "Kosmický Horor", 230, "Normal", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Uchopení chapadly", DamageToAll = 32 }, new EnemyAction { Name = "Extinkce", DamageToAll = 42 }
            }) { Speed = 3.0f, AttackRange = 8.0f, AttackCooldown = 4000f },
            new EnemyTemplate("3_N_10", "Zvěstovatel Zkázy", 150, "Normal", 3, new List<EnemyAction> { 
                new EnemyAction { Name = "Apokalypsa hned", DamageToAll = 45 }, new EnemyAction { Name = "Pád meteoritu", DamageToAll = 50 }
            }) { Speed = 2.5f, AttackRange = 10.0f, AttackCooldown = 4500f },

            // ================== ACT 3 - ELITY (5x) ==================
            new EnemyTemplate("3_E_01", "Strážce Rovnováhy", 450, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Absolutní neutralita", DamageToAll = 45 }, new EnemyAction { Name = "Úder spravedlnosti", DamageToAll = 50 }
            }) { Speed = 3.5f, AttackRange = 5.0f, AttackCooldown = 3800f },
            new EnemyTemplate("3_E_02", "Avatar Temnoty", 400, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Černá díra", DamageToAll = 55 }, new EnemyAction { Name = "Zničení naděje", DamageToAll = 60 }
            }) { Speed = 3.0f, AttackRange = 12.0f, AttackCooldown = 4200f },
            new EnemyTemplate("3_E_03", "Nebešťan Hněvu", 420, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Zlaté kopí", DamageToAll = 55 }, new EnemyAction { Name = "Soudný blesk", DamageToAll = 50 }
            }) { Speed = 4.5f, AttackRange = 8.0f, AttackCooldown = 3500f },
            new EnemyTemplate("3_E_04", "Prázdný Drak", 500, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Dech antihmoty", DamageToAll = 60 }, new EnemyAction { Name = "Pád komety", DamageToAll = 65 }
            }) { Speed = 4.0f, AttackRange = 15.0f, AttackCooldown = 4500f },
            new EnemyTemplate("3_E_05", "Velekněz Konce", 380, "Elite", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Zakázané kouzlo", DamageToAll = 60 }, new EnemyAction { Name = "Slovo moci", DamageToAll = 50 }
            }) { Speed = 2.8f, AttackRange = 10.0f, AttackCooldown = 4000f },

            // ================== ACT 3 - BOSSOVÉ (3x) ==================
            new EnemyTemplate("3_B_01", "Ztělesnění Karmy", 1200, "Boss", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Zrcadlení hříchů", DamageToAll = 75 }, new EnemyAction { Name = "Smazání existence", DamageToAll = 85 }
            }) { Speed = 3.5f, AttackRange = 20.0f, AttackCooldown = 4000f },
            new EnemyTemplate("3_B_02", "Bůh Prázdnoty", 1500, "Boss", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Temná singularita", DamageToAll = 90 }, new EnemyAction { Name = "Návrat do nicoty", DamageToAll = 100 }
            }) { Speed = 4.0f, AttackRange = 25.0f, AttackCooldown = 4500f },
            new EnemyTemplate("3_B_03", "Královna Zlomených Duší", 1300, "Boss", 3, new List<EnemyAction> {
                new EnemyAction { Name = "Dotek agónie", DamageToAll = 85 }, new EnemyAction { Name = "Nekonečný žal", DamageToAll = 95 }
            }) { Speed = 4.5f, AttackRange = 15.0f, AttackCooldown = 3500f }
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