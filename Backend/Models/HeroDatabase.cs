using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class HeroDatabase
    {
         // 2. DATABÁZE HRDINŮ (S jejich unikátními mechanikami, relikviemi a balíčky)
        public static Dictionary<string, HeroTemplate> Heroes = new Dictionary<string, HeroTemplate>
        {
            { 
                "Paladin", 
                new HeroTemplate(
                    "Paladin", "Světlonoš", 80, 
                    "Aura světla: Tvoje léčení za hranice maximálních životů se přemění na Blok.",
                    new Relic("R_Kalich", "Svatý kalich", "Na konci tahu..."),
                    
                    // POZOR ZMĚNA: Tady musíme Paladinovi dát do začátku jeho nové karty!
                    // Například 4x úder, 4x štít a 2 silnější karty:
                    new List<string> { "P_01", "P_01", "P_01", "P_01", "P_11", "P_11", "P_11", "P_11", "P_04", "P_26" }, 
                    new List<string> { "P_10", "P_29" } // Karty k objevení
                )
            },
            { 
                "Warlock", 
                new HeroTemplate(
                    "Warlock", "Stínový mág", 60, 
                    "Krvavá oběť: Karty, které zraňují tebe samotného, ti trvale v tomto souboji zvyšují poškození.",
                    new Relic("R_Lebka", "Prokletá lebka", "Kdykoliv zahraješ kartu posouvající Karmu do Temnoty, udělíš nepříteli 1 dodatečné zranění."),
                    
                    // ZMĚNĚNO: Startovní balíček Warlocka na nové karty (více útoků, méně obrany, pakt na lízání)
                    new List<string> { "W_01", "W_01", "W_01", "W_01", "W_01", "W_36", "W_36", "W_36", "W_22", "W_03" }, 
                    new List<string> { "W_06", "W_21" } // Karty k objevení
                ) 
            },
            { 
                "Monk", 
                new HeroTemplate(
                    "Monk", "Vyrovnávač", 70, 
                    "Stav Zenu: Karty s neutrální Karmou (0) se každým zahráním v boji stávají silnějšími.",
                    new Relic("R_Koralek", "Korálky rovnováhy", "Pokud je na konci tahu Karma přesně 0, celý tým získá 3 Blok."),
                    
                    // ZMĚNĚNO: Startovní balíček (Neutrální údery, bloky a dvě karty na kroky stranou)
                    new List<string> { "M_01", "M_01", "M_01", "M_01", "M_21", "M_21", "M_21", "M_21", "M_36", "M_37" }, 
                    new List<string> { "M_02", "M_26" }
                ) 
            },
            { 
                "Berserker", 
                new HeroTemplate(
                    "Berserker", "Krvavý berserk", 85, 
                    "Zuřivost: Čím méně máš životů, tím větší zranění udělují tvé základní útoky.",
                    new Relic("R_Sekera", "Zrezivělá sekera", "Pokud máš pod 50% HP, všechny tvé útoky zraňují za +3."),
                    
                    // ZMĚNĚNO: Startovní balíček (Téměř čistý útok a troška sebevražedného adrenalinu)
                    new List<string> { "B_01", "B_01", "B_01", "B_01", "B_02", "B_03", "B_29", "B_29", "B_26", "B_06" }, 
                    new List<string> { "B_04", "B_27" }
                ) 
            },
            { 
                "Druid", 
                new HeroTemplate(
                    "Druid", "Ochránce", 75, 
                    "Přírodní cyklus: Zahrání útoku ihned posílí tvou další obrannou kartu a naopak.",
                    new Relic("R_Seminko", "Živé semínko", "Při tvém prvním léčení v každém tahu získáš automaticky 2 Blok."),
                    
                    // ZMĚNĚNO: Startovní balíček (Vyvážený mix s důrazem na blok a vampirismus)
                    new List<string> { "D_01", "D_01", "D_01", "D_03", "D_21", "D_21", "D_21", "D_23", "D_02", "D_24" }, 
                    new List<string> { "D_06", "D_22" }
                ) 
            },
            { 
                "Rogue", 
                new HeroTemplate(
                    "Rogue", "Karmický vrah", 65, 
                    "Kombo: Pokud zahraješ dvě karty stejného typu po sobě, spustí se jejich dodatečný efekt.",
                    new Relic("R_Plast", "Stínový plášť", "Pokud ve svém tahu pohneš Karmou o více než 3 body libovolným směrem, ignoruješ první zranění v dalším kole."),
                    
                    // ZMĚNĚNO: Startovní balíček (Rychlé útoky, úhyby a silný posun karmy pro Relikvii)
                    new List<string> { "R_01", "R_01", "R_01", "R_06", "R_21", "R_21", "R_21", "R_24", "R_23", "R_29" }, 
                    new List<string> { "R_05", "R_22" }
                ) 
            },
            { 
                "Bard", 
                new HeroTemplate(
                    "Bard", "Bard", 70, 
                    "Rezonance: Tvé zahrané karty neposkytují blok tobě, ale rozdělují ho rovnoměrně zbytku týmu.",
                    new Relic("R_Loutna", "Loutna ozvěn", "Na začátku boje ihned posuneš Karmu o +2 do Světla."),
                    
                    // ZMĚNĚNO: Startovní balíček (Extrémní podpora, málo útoku, silné tahání Karmy)
                    new List<string> { "Bd_01", "Bd_02", "Bd_17", "Bd_17", "Bd_17", "Bd_17", "Bd_16", "Bd_20", "Bd_36", "Bd_38" }, 
                    new List<string> { "Bd_18", "Bd_23" }
                ) 
            },
            { 
                "Pyromancer", 
                new HeroTemplate(
                    "Pyromancer", "Pyromant", 60, 
                    "Vznícení: Tvé útoky aplikují 'Hoření', které nepříteli ubližuje na konci tahu.",
                    new Relic("R_Uhlak", "Věčně žhnoucí uhlík", "Za každý bod Karmy v Temnotě dostává Boss na konci tahu 1 poškození."),
                    
                    // ZMĚNĚNO: Startovní balíček (Smršť základních plamenů a trocha rychlého lízání)
                    new List<string> { "Py_02", "Py_02", "Py_03", "Py_03", "Py_03", "Py_21", "Py_21", "Py_21", "Py_01", "Py_24" }, 
                    new List<string> { "Py_07", "Py_22" }
                ) 
            }
        };
    }
}