using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class CardDatabase
    {
        // 1. DATABÁZE VŠECH KARET VE HŘE
        public static Dictionary<string, Card> AllCards = new Dictionary<string, Card>
        {
            // --- ZÁKLADNÍ KARTY (Startovní pro většinu) ---
            { "Z_Uder", new Card("Z_Uder", "Základní úder", "Udělí 5 zranění.", 5, 0, 0, -1) },
            { "Z_Obrana", new Card("Z_Obrana", "Základní blok", "Přidá 5 bloku.", 0, 5, 0, 1) },

            // ==========================================
            // 1. PALADIN (SVĚTLONOŠ) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Základní a levné útoky (1-10) ---
            { "P_01", new Card("P_01", "Svatý úder", "Udělí 5 zranění. Karma +1.", cost: 1, damage: 5, karmaShift: 1) },
            { "P_02", new Card("P_02", "Zářící čepel", "Udělí 7 zranění.", cost: 1, damage: 7) },
            { "P_03", new Card("P_03", "Rychlý švih", "Udělí 3 zranění dvakrát. Karma +1.", cost: 1, damage: 3, hitCount: 2, karmaShift: 1) },
            { "P_04", new Card("P_04", "Úder štítem", "Udělí 4 zranění a přidá 4 blok.", cost: 1, damage: 4, block: 4) },
            { "P_05", new Card("P_05", "Drtivý dopad", "Udělí 10 zranění. Karma -1.", cost: 2, damage: 10, karmaShift: -1) },
            { "P_06", new Card("P_06", "Výpad", "Udělí 5 zranění, lízni 1 kartu.", cost: 1, damage: 5, drawCards: 1) },
            { "P_07", new Card("P_07", "Očišťující plamen", "Udělí 6 zranění. Karma +2.", cost: 1, damage: 6, karmaShift: 2) },
            { "P_08", new Card("P_08", "Těžká rána", "Udělí 12 zranění. Karma -2.", cost: 2, damage: 12, karmaShift: -2) },
            { "P_09", new Card("P_09", "Odplata", "Udělí 8 zranění a přidá 2 blok.", cost: 1, damage: 8, block: 2) },
            { "P_10", new Card("P_10", "Soudný den", "Udělí 15 zranění. Karma +1.", cost: 3, damage: 15, karmaShift: 1) },

            // --- Obrana a blokování (11-25) ---
            { "P_11", new Card("P_11", "Svatý štít", "Přidá 6 bloku. Karma +1.", cost: 1, block: 6, karmaShift: 1) },
            { "P_12", new Card("P_12", "Hradba víry", "Přidá 10 bloku. Karma +1.", cost: 2, block: 10, karmaShift: 1) },
            { "P_13", new Card("P_13", "Zlatá aura", "Přidá 15 bloku. Karma +2.", cost: 3, block: 15, karmaShift: 2) },
            { "P_14", new Card("P_14", "Ochrana slabých", "Přidá 8 bloku, vyléčí 2 HP.", cost: 2, block: 8, heal: 2) },
            { "P_15", new Card("P_15", "Karmická bariéra", "Přidá 5 bloku za 0 energie. Karma +1.", cost: 0, block: 5, karmaShift: 1) },
            { "P_16", new Card("P_16", "Odražení", "Přidá 4 bloku a udělí 4 zranění.", cost: 1, block: 4, damage: 4) },
            { "P_17", new Card("P_17", "Zdi kláštera", "Přidá 12 bloku. Karma 0.", cost: 2, block: 12) },
            { "P_18", new Card("P_18", "Nezlomnost", "Přidá 7 bloku, lízni 1 kartu.", cost: 1, block: 7, drawCards: 1) },
            { "P_19", new Card("P_19", "Štít úsvitu", "Přidá 9 bloku. Karma +2.", cost: 1, block: 9, karmaShift: 2) },
            { "P_20", new Card("P_20", "Bašta", "Přidá 20 bloku! Karma +3.", cost: 3, block: 20, karmaShift: 3) },
            { "P_21", new Card("P_21", "Požehnaný kov", "Přidá 6 bloku, vyléčí 3 HP.", cost: 1, block: 6, heal: 3) },
            { "P_22", new Card("P_22", "Krytí", "Přidá 5 bloku. Stojí 0.", cost: 0, block: 5) },
            { "P_23", new Card("P_23", "Pevný postoj", "Přidá 8 bloku, Karma +1.", cost: 1, block: 8, karmaShift: 1) },
            { "P_24", new Card("P_24", "Svatyně", "Přidá 14 bloku. Karma +1.", cost: 2, block: 14, karmaShift: 1) },
            { "P_25", new Card("P_25", "Andělská křídla", "Přidá 10 bloku, lízni 2 karty. Karma +2.", cost: 2, block: 10, drawCards: 2, karmaShift: 2) },

            // --- Léčení a Manipulace s Karmou (26-40) ---
            { "P_26", new Card("P_26", "Modlitba", "Vyléčí 5 HP. Karma +1.", cost: 1, heal: 5, karmaShift: 1) },
            { "P_27", new Card("P_27", "Hojivý dotek", "Vyléčí 8 HP. Karma +2.", cost: 2, heal: 8, karmaShift: 2) },
            { "P_28", new Card("P_28", "Světlo naděje", "Vyléčí 4 HP, lízni 1 kartu.", cost: 1, heal: 4, drawCards: 1) },
            { "P_29", new Card("P_29", "Očista", "Vyléčí 10 HP. Karma +3.", cost: 2, heal: 10, karmaShift: 3) },
            { "P_30", new Card("P_30", "Zázrak", "Vyléčí 20 HP! Karma +5.", cost: 3, heal: 20, karmaShift: 5) },
            { "P_31", new Card("P_31", "Malé požehnání", "Vyléčí 3 HP za 0 energie.", cost: 0, heal: 3) },
            { "P_32", new Card("P_32", "Vlna světla", "Vyléčí 6 HP a udělí 6 zranění.", cost: 2, heal: 6, damage: 6) },
            { "P_33", new Card("P_33", "Slza anděla", "Vyléčí 12 HP. Karma +2.", cost: 2, heal: 12, karmaShift: 2) },
            { "P_34", new Card("P_34", "Usmíření", "Vyléčí 5 HP. Karma +3.", cost: 1, heal: 5, karmaShift: 3) },
            { "P_35", new Card("P_35", "Zlatý prach", "Vyléčí 2 HP, přidá 2 blok. Stojí 0.", cost: 0, heal: 2, block: 2) },
            { "P_36", new Card("P_36", "Záře", "Vyléčí 7 HP, lízni 1 kartu. Karma +1.", cost: 2, heal: 7, drawCards: 1, karmaShift: 1) },
            { "P_37", new Card("P_37", "Božský klid", "Karma +4 za 0 energie.", cost: 0, karmaShift: 4) },
            { "P_38", new Card("P_38", "Svatá oběť", "Udělí ti zranění (později), vyléčí 15 HP. Karma +2.", cost: 1, heal: 15, karmaShift: 2) }, // Sebezranění nasimulujeme později
            { "P_39", new Card("P_39", "Znovuzrození", "Vyléčí 25 HP. Karma +4.", cost: 3, heal: 25, karmaShift: 4) },
            { "P_40", new Card("P_40", "Sjednocení", "Vyléčí 4 HP, přidá 4 blok, lízni 1 kartu.", cost: 2, heal: 4, block: 4, drawCards: 1) },

            // --- Podpora, Komba a Ultimátní karty (41-50) ---
            { "P_41", new Card("P_41", "Záblesk poznání", "Lízni 2 karty. Karma +1.", cost: 0, drawCards: 2, karmaShift: 1) },
            { "P_42", new Card("P_42", "Vedení", "Lízni 3 karty. Karma +2.", cost: 1, drawCards: 3, karmaShift: 2) },
            { "P_43", new Card("P_43", "Boží hněv", "Udělí 5 zranění třikrát! Karma +3.", cost: 3, damage: 5, hitCount: 3, karmaShift: 3) },
            { "P_44", new Card("P_44", "Svatá smršť", "Udělí 4 zranění dvakrát. Lízni 1 kartu.", cost: 2, damage: 4, hitCount: 2, drawCards: 1) },
            { "P_45", new Card("P_45", "Spravedlnost", "Udělí 20 zranění. Karma +2.", cost: 3, damage: 20, karmaShift: 2) },
            { "P_46", new Card("P_46", "Aura vykoupení", "Přidá 15 bloku, vyléčí 10 HP. Karma +4.", cost: 3, block: 15, heal: 10, karmaShift: 4) },
            { "P_47", new Card("P_47", "Kázání", "Lízni 2 karty. Přidá 5 bloku.", cost: 1, drawCards: 2, block: 5) },
            { "P_48", new Card("P_48", "Proroctví", "Lízni 4 karty! Karma 0.", cost: 2, drawCards: 4) },
            { "P_49", new Card("P_49", "Zásah shůry", "Udělí 30 zranění! Karma +5.", cost: 4, damage: 30, karmaShift: 5) }, // Vyžaduje extra energii
            { "P_50", new Card("P_50", "Absolutní světlo", "Vyléčí 15 HP, přidá 15 bloku, udělí 15 zranění. Karma +5.", cost: 5, heal: 15, block: 15, damage: 15, karmaShift: 5) },

            // ==========================================
            // 2. WARLOCK (STÍNOVÝ MÁG) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Temné útoky a kletby (1-20) ---
            { "W_01", new Card("W_01", "Stínový šíp", "Udělí 6 zranění. Karma -1.", cost: 1, damage: 6, karmaShift: -1) },
            { "W_02", new Card("W_02", "Dotek smrti", "Udělí 9 zranění. Karma -2.", cost: 1, damage: 9, karmaShift: -2) },
            { "W_03", new Card("W_03", "Krvavý dráp", "Udělí 12 zranění! Karma -3.", cost: 1, damage: 12, karmaShift: -3) },
            { "W_04", new Card("W_04", "Dvojitý sek", "Udělí 4 zranění dvakrát. Karma -1.", cost: 1, damage: 4, hitCount: 2, karmaShift: -1) },
            { "W_05", new Card("W_05", "Vysátí duše", "Udělí 5 zranění, vyléčí 2 HP. Karma -1.", cost: 1, damage: 5, heal: 2, karmaShift: -1) },
            { "W_06", new Card("W_06", "Plameny pekel", "Udělí 15 zranění. Karma -2.", cost: 2, damage: 15, karmaShift: -2) },
            { "W_07", new Card("W_07", "Hniloba", "Udělí 8 zranění, lízni 1 kartu. Karma -2.", cost: 1, damage: 8, drawCards: 1, karmaShift: -2) },
            { "W_08", new Card("W_08", "Bičování", "Udělí 6 zranění. Stojí 0 energie! Karma -2.", cost: 0, damage: 6, karmaShift: -2) },
            { "W_09", new Card("W_09", "Zkázonosný blesk", "Udělí 20 zranění! Karma -4.", cost: 2, damage: 20, karmaShift: -4) },
            { "W_10", new Card("W_10", "Temná exploze", "Udělí 10 zranění všem (zatím jednomu). Karma -3.", cost: 2, damage: 10, karmaShift: -3) },
            { "W_11", new Card("W_11", "Smrtící polibek", "Udělí 7 zranění, vyléčí 4 HP. Karma -2.", cost: 2, damage: 7, heal: 4, karmaShift: -2) },
            { "W_12", new Card("W_12", "Erupce stínů", "Udělí 5 zranění třikrát! Karma -3.", cost: 2, damage: 5, hitCount: 3, karmaShift: -3) },
            { "W_13", new Card("W_13", "Krutost", "Udělí 14 zranění. Karma -1.", cost: 2, damage: 14, karmaShift: -1) },
            { "W_14", new Card("W_14", "Zákeřný úder", "Udělí 8 zranění za 0 energie. Karma -3.", cost: 0, damage: 8, karmaShift: -3) },
            { "W_15", new Card("W_15", "Vysátí krve", "Udělí 10 zranění, vyléčí 10 HP. Karma -3.", cost: 3, damage: 10, heal: 10, karmaShift: -3) },
            { "W_16", new Card("W_16", "Nekróza", "Udělí 18 zranění. Karma -4.", cost: 2, damage: 18, karmaShift: -4) },
            { "W_17", new Card("W_17", "Ztracená duše", "Udělí 4 zranění a přidá 4 blok. Karma -1.", cost: 1, damage: 4, block: 4, karmaShift: -1) },
            { "W_18", new Card("W_18", "Šílenství", "Udělí 3 zranění čtyřikrát! Karma -4.", cost: 2, damage: 3, hitCount: 4, karmaShift: -4) },
            { "W_19", new Card("W_19", "Bolest", "Udělí 11 zranění. Karma -1.", cost: 1, damage: 11, karmaShift: -1) },
            { "W_20", new Card("W_20", "Pohlcení světla", "Udělí 15 zranění, Karma klesne o -5!", cost: 1, damage: 15, karmaShift: -5) },

            // --- Rituály, Lízání karet a Manipulace (21-35) ---
            { "W_21", new Card("W_21", "Temný pakt", "Lízni 3 karty. Karma -3.", cost: 1, drawCards: 3, karmaShift: -3) },
            { "W_22", new Card("W_22", "Krvavá oběť", "Lízni 2 karty za 0 energie. Karma -2.", cost: 0, drawCards: 2, karmaShift: -2) },
            { "W_23", new Card("W_23", "Pohled do propasti", "Lízni 4 karty! Karma -4.", cost: 2, drawCards: 4, karmaShift: -4) },
            { "W_24", new Card("W_24", "Rituál", "Přidá 1 energii (zatím lízne 1 kartu). Karma -1.", cost: 0, drawCards: 1, karmaShift: -1) },
            { "W_25", new Card("W_25", "Noční můra", "Lízni 2 karty, udělí 5 zranění. Karma -2.", cost: 1, drawCards: 2, damage: 5, karmaShift: -2) },
            { "W_26", new Card("W_26", "Studna temnoty", "Karma -4 za 0 energie.", cost: 0, karmaShift: -4) },
            { "W_27", new Card("W_27", "Hlad", "Lízni 1 kartu, vyléčí 3 HP. Karma -1.", cost: 1, drawCards: 1, heal: 3, karmaShift: -1) },
            { "W_28", new Card("W_28", "Znesvěcení", "Lízni 3 karty. Karma -2.", cost: 2, drawCards: 3, karmaShift: -2) },
            { "W_29", new Card("W_29", "Černá magie", "Lízni 2 karty, přidá 5 blok. Karma -2.", cost: 1, drawCards: 2, block: 5, karmaShift: -2) },
            { "W_30", new Card("W_30", "Kniha mrtvých", "Lízni 5 karet!!! Karma -5.", cost: 3, drawCards: 5, karmaShift: -5) },
            { "W_31", new Card("W_31", "Smlouva s démonem", "Vyléčí 15 HP, ale Karma -5.", cost: 1, heal: 15, karmaShift: -5) },
            { "W_32", new Card("W_32", "Zakázané vědění", "Lízni 2 karty. Stojí 0.", cost: 0, drawCards: 2) },
            { "W_33", new Card("W_33", "Ozvěny smrti", "Udělí 6 zranění, lízni 2 karty. Karma -3.", cost: 1, damage: 6, drawCards: 2, karmaShift: -3) },
            { "W_34", new Card("W_34", "Stínová forma", "Lízni 1 kartu, Karma -2.", cost: 0, drawCards: 1, karmaShift: -2) },
            { "W_35", new Card("W_35", "Zkáza", "Lízni 3 karty, udělí 10 zranění. Karma -4.", cost: 2, drawCards: 3, damage: 10, karmaShift: -4) },

            // --- Křehká obrana (Warlock se moc nebrání) (36-45) ---
            { "W_36", new Card("W_36", "Štít z kostí", "Přidá 5 bloku. Karma -1.", cost: 1, block: 5, karmaShift: -1) },
            { "W_37", new Card("W_37", "Brnění stínů", "Přidá 9 bloku. Karma -2.", cost: 1, block: 9, karmaShift: -2) },
            { "W_38", new Card("W_38", "Přízračná stěna", "Přidá 12 bloku. Karma -3.", cost: 2, block: 12, karmaShift: -3) },
            { "W_39", new Card("W_39", "Krvavá bariéra", "Přidá 8 bloku, vyléčí 2 HP. Karma -2.", cost: 1, block: 8, heal: 2, karmaShift: -2) },
            { "W_40", new Card("W_40", "Odhmotnění", "Přidá 15 bloku! Karma -4.", cost: 2, block: 15, karmaShift: -4) },
            { "W_41", new Card("W_41", "Zastření", "Přidá 4 bloku za 0 energie.", cost: 0, block: 4) },
            { "W_42", new Card("W_42", "Šepot prázdnoty", "Přidá 7 bloku, lízni 1 kartu. Karma -2.", cost: 1, block: 7, drawCards: 1, karmaShift: -2) },
            { "W_43", new Card("W_43", "Odmítnutí světla", "Přidá 10 bloku. Karma -3.", cost: 1, block: 10, karmaShift: -3) },
            { "W_44", new Card("W_44", "Temný kokon", "Přidá 20 bloku. Karma -5.", cost: 3, block: 20, karmaShift: -5) },
            { "W_45", new Card("W_45", "Upíří aura", "Přidá 5 bloku, vyléčí 5 HP. Karma -2.", cost: 2, block: 5, heal: 5, karmaShift: -2) },

            // --- Ultimátní zkáza (46-50) ---
            { "W_46", new Card("W_46", "Apokalypsa", "Udělí 25 zranění. Karma -4.", cost: 3, damage: 25, karmaShift: -4) },
            { "W_47", new Card("W_47", "Déšť meteorů", "Udělí 8 zranění třikrát! Karma -5.", cost: 3, damage: 8, hitCount: 3, karmaShift: -5) },
            { "W_48", new Card("W_48", "Pohlcovač duší", "Udělí 15 zranění, vyléčí 15 HP. Karma -4.", cost: 4, damage: 15, heal: 15, karmaShift: -4) },
            { "W_49", new Card("W_49", "Černá díra", "Lízni 5 karet, udělí 10 zranění. Karma -5.", cost: 3, drawCards: 5, damage: 10, karmaShift: -5) },
            { "W_50", new Card("W_50", "Absolutní zkáza", "Udělí 40 zranění!!! Karma -7.", cost: 5, damage: 40, karmaShift: -7) },

            // ==========================================
            // 3. MONK (VYROVNÁVAČ) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Bojová umění a rychlé údery (1-20) ---
            { "M_01", new Card("M_01", "Úder zenu", "Udělí 6 zranění. Karma 0.", cost: 1, damage: 6, karmaShift: 0) },
            { "M_02", new Card("M_02", "Dvojitý úder", "Udělí 4 zranění dvakrát. Karma 0.", cost: 1, damage: 4, hitCount: 2, karmaShift: 0) },
            { "M_03", new Card("M_03", "Kop z otočky", "Udělí 9 zranění. Karma 0.", cost: 1, damage: 9, karmaShift: 0) },
            { "M_04", new Card("M_04", "Tlakový bod", "Udělí 3 zranění, lízni 1 kartu. Karma 0.", cost: 0, damage: 3, drawCards: 1, karmaShift: 0) },
            { "M_05", new Card("M_05", "Pěst tygra", "Udělí 14 zranění. Karma 0.", cost: 2, damage: 14, karmaShift: 0) },
            { "M_06", new Card("M_06", "Smršť ran", "Udělí 2 zranění pětkrát! Karma 0.", cost: 2, damage: 2, hitCount: 5, karmaShift: 0) },
            { "M_07", new Card("M_07", "Úder otevřenou dlaní", "Udělí 5 zranění, přidá 5 blok. Karma 0.", cost: 1, damage: 5, block: 5, karmaShift: 0) },
            { "M_08", new Card("M_08", "Bodnutí pěstí", "Udělí 8 zranění. Karma +1.", cost: 1, damage: 8, karmaShift: 1) },
            { "M_09", new Card("M_09", "Tvrdý pád", "Udělí 8 zranění. Karma -1.", cost: 1, damage: 8, karmaShift: -1) },
            { "M_10", new Card("M_10", "Zářivý kop", "Udělí 12 zranění. Karma +1.", cost: 2, damage: 12, karmaShift: 1) },
            { "M_11", new Card("M_11", "Stínový kop", "Udělí 12 zranění. Karma -1.", cost: 2, damage: 12, karmaShift: -1) },
            { "M_12", new Card("M_12", "Série zenu", "Udělí 4 zranění třikrát. Karma 0.", cost: 2, damage: 4, hitCount: 3, karmaShift: 0) },
            { "M_13", new Card("M_13", "Průraz", "Udělí 15 zranění. Karma 0.", cost: 2, damage: 15, karmaShift: 0) },
            { "M_14", new Card("M_14", "Otevření gardu", "Udělí 7 zranění za 0 energie. Karma 0.", cost: 0, damage: 7, karmaShift: 0) },
            { "M_15", new Card("M_15", "Letící drak", "Udělí 18 zranění. Karma 0.", cost: 3, damage: 18, karmaShift: 0) },
            { "M_16", new Card("M_16", "Narušení čchi", "Udělí 5 zranění, lízni 2 karty. Karma 0.", cost: 1, damage: 5, drawCards: 2, karmaShift: 0) },
            { "M_17", new Card("M_17", "Bleskový výpad", "Udělí 6 zranění, stoji 0. Karma 0.", cost: 0, damage: 6, karmaShift: 0) },
            { "M_18", new Card("M_18", "Těžká dlaň", "Udělí 20 zranění. Karma 0.", cost: 3, damage: 20, karmaShift: 0) },
            { "M_19", new Card("M_19", "Úder jeřába", "Udělí 10 zranění. Karma +1.", cost: 1, damage: 10, karmaShift: 1) },
            { "M_20", new Card("M_20", "Úder hada", "Udělí 10 zranění. Karma -1.", cost: 1, damage: 10, karmaShift: -1) },

            // --- Klid, Blokování a Meditace (21-35) ---
            { "M_21", new Card("M_21", "Klidná mysl", "Přidá 7 bloku. Karma 0.", cost: 1, block: 7, karmaShift: 0) },
            { "M_22", new Card("M_22", "Ohyb rákosu", "Přidá 4 bloku, stojí 0. Karma 0.", cost: 0, block: 4, karmaShift: 0) },
            { "M_23", new Card("M_23", "Železná košile", "Přidá 12 bloku. Karma 0.", cost: 2, block: 12, karmaShift: 0) },
            { "M_24", new Card("M_24", "Bariéra čchi", "Přidá 15 bloku. Karma 0.", cost: 2, block: 15, karmaShift: 0) },
            { "M_25", new Card("M_25", "Vnitřní mír", "Přidá 5 bloku, vyléčí 3 HP. Karma 0.", cost: 1, block: 5, heal: 3, karmaShift: 0) },
            { "M_26", new Card("M_26", "Meditace", "Lízni 2 karty. Karma 0.", cost: 1, drawCards: 2, karmaShift: 0) },
            { "M_27", new Card("M_27", "Hluboká meditace", "Lízni 3 karty. Karma 0.", cost: 2, drawCards: 3, karmaShift: 0) },
            { "M_28", new Card("M_28", "Soustředění", "Lízni 1 kartu, přidá 3 blok. Stojí 0.", cost: 0, drawCards: 1, block: 3, karmaShift: 0) },
            { "M_29", new Card("M_29", "Zklidnění tepu", "Vyléčí 5 HP. Karma 0.", cost: 1, heal: 5, karmaShift: 0) },
            { "M_30", new Card("M_30", "Harmonie", "Vyléčí 10 HP. Karma 0.", cost: 2, heal: 10, karmaShift: 0) },
            { "M_31", new Card("M_31", "Odražení síly", "Přidá 8 bloku, udělí 4 zranění. Karma 0.", cost: 1, block: 8, damage: 4, karmaShift: 0) },
            { "M_32", new Card("M_32", "Tok vody", "Přidá 6 bloku, lízni 1 kartu. Karma 0.", cost: 1, block: 6, drawCards: 1, karmaShift: 0) },
            { "M_33", new Card("M_33", "Postoj hory", "Přidá 20 bloku! Karma 0.", cost: 3, block: 20, karmaShift: 0) },
            { "M_34", new Card("M_34", "Květ lotosu", "Vyléčí 4 HP, přidá 4 blok. Karma 0.", cost: 1, heal: 4, block: 4, karmaShift: 0) },
            { "M_35", new Card("M_35", "Prázdnota", "Lízni 4 karty. Karma 0.", cost: 2, drawCards: 4, karmaShift: 0) },

            // --- Manipulace s Karmou (Jemné korekce) (36-40) ---
            { "M_36", new Card("M_36", "Krok do Světla", "Přidá 5 bloku. Karma +2.", cost: 0, block: 5, karmaShift: 2) },
            { "M_37", new Card("M_37", "Krok do Stínu", "Udělí 5 zranění. Karma -2.", cost: 0, damage: 5, karmaShift: -2) },
            { "M_38", new Card("M_38", "Přesun vah (Světlo)", "Lízni 2 karty. Karma +3.", cost: 1, drawCards: 2, karmaShift: 3) },
            { "M_39", new Card("M_39", "Přesun vah (Temnota)", "Lízni 2 karty. Karma -3.", cost: 1, drawCards: 2, karmaShift: -3) },
            { "M_40", new Card("M_40", "Očištění mysli", "Vyléčí 2 HP, stojí 0. Karma +1.", cost: 0, heal: 2, karmaShift: 1) },

            // --- Ultimátní techniky a Mistrovství (41-50) ---
            { "M_41", new Card("M_41", "Zlatý drak", "Udělí 25 zranění! Karma 0.", cost: 3, damage: 25, karmaShift: 0) },
            { "M_42", new Card("M_42", "Tisíc dlaní", "Udělí 3 zranění šestkrát! Karma 0.", cost: 3, damage: 3, hitCount: 6, karmaShift: 0) },
            { "M_43", new Card("M_43", "Nesmrtelnost", "Přidá 30 bloku!!! Karma 0.", cost: 4, block: 30, karmaShift: 0) },
            { "M_44", new Card("M_44", "Absolutní rovnováha", "Vyléčí 8 HP, přidá 8 blok, udělí 8 zranění. Karma 0.", cost: 3, heal: 8, block: 8, damage: 8, karmaShift: 0) },
            { "M_45", new Card("M_45", "Mistrovské kombo", "Udělí 10 zranění, přidá 10 blok, lízni 2 karty. Karma 0.", cost: 3, damage: 10, block: 10, drawCards: 2, karmaShift: 0) },
            { "M_46", new Card("M_46", "Probuzení", "Lízni 5 karet. Karma 0.", cost: 2, drawCards: 5, karmaShift: 0) },
            { "M_47", new Card("M_47", "Úder prázdnoty", "Udělí 15 zranění za 1 energii (silné, ale vyžaduje přesně 0 Karmu v logice). Karma 0.", cost: 1, damage: 15, karmaShift: 0) },
            { "M_48", new Card("M_48", "Všudypřítomnost", "Přidá 15 bloku, lízni 3 karty. Karma 0.", cost: 3, block: 15, drawCards: 3, karmaShift: 0) },
            { "M_49", new Card("M_49", "Dokonalý úder", "Udělí 35 zranění! Karma 0.", cost: 4, damage: 35, karmaShift: 0) },
            { "M_50", new Card("M_50", "Osvícení", "Vyléčí 20 HP, přidá 20 bloku. Karma 0.", cost: 5, heal: 20, block: 20, karmaShift: 0) },

           // ==========================================
            // 4. BERSERKER (KRVAVÝ BERSERK) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Brutální a riskantní útoky (1-25) ---
            { "B_01", new Card("B_01", "Krvavý sek", "Udělí 8 zranění. Karma -1.", cost: 1, damage: 8, karmaShift: -1) },
            { "B_02", new Card("B_02", "Dvojitý zásek", "Udělí 5 zranění dvakrát. Karma -1.", cost: 1, damage: 5, hitCount: 2, karmaShift: -1) },
            { "B_03", new Card("B_03", "Bezhlavý výpad", "Udělí 14 zranění. Karma -2.", cost: 1, damage: 14, karmaShift: -2) },
            { "B_04", new Card("B_04", "Rozdrcení", "Udělí 18 zranění! Karma -3.", cost: 2, damage: 18, karmaShift: -3) },
            { "B_05", new Card("B_05", "Zvířecí zuřivost", "Udělí 4 zranění třikrát. Karma -2.", cost: 2, damage: 4, hitCount: 3, karmaShift: -2) },
            { "B_06", new Card("B_06", "Obětování", "Udělí 20 zranění za 1 energii! (Stojí tě HP). Karma -3.", cost: 1, damage: 20, karmaShift: -3) },
            { "B_07", new Card("B_07", "Rána naslepo", "Udělí 12 zranění náhodnému cíli. Karma -1.", cost: 1, damage: 12, karmaShift: -1) },
            { "B_08", new Card("B_08", "Vzteklý skus", "Udělí 6 zranění, vyléčí 2 HP. Karma -2.", cost: 1, damage: 6, heal: 2, karmaShift: -2) },
            { "B_09", new Card("B_09", "Proražení zbroje", "Udělí 10 zranění, přidá 2 blok. Karma -1.", cost: 1, damage: 10, block: 2, karmaShift: -1) },
            { "B_10", new Card("B_10", "Drtivý dopad", "Udělí 25 zranění! Karma -4.", cost: 3, damage: 25, karmaShift: -4) },
            { "B_11", new Card("B_11", "Tržná rána", "Udělí 8 zranění, lízni 1 kartu. Karma -1.", cost: 1, damage: 8, drawCards: 1, karmaShift: -1) },
            { "B_12", new Card("B_12", "Úder hlavou", "Udělí 15 zranění. Stojí tě 2 HP. Karma -2.", cost: 1, damage: 15, karmaShift: -2) },
            { "B_13", new Card("B_13", "Masakr", "Udělí 8 zranění všem nepřátelům. Karma -3.", cost: 2, damage: 8, karmaShift: -3) },
            { "B_14", new Card("B_14", "Poprava", "Udělí 30 zranění! Karma -4.", cost: 3, damage: 30, karmaShift: -4) },
            { "B_15", new Card("B_15", "Krvavá stopa", "Udělí 6 zranění. Karma 0.", cost: 0, damage: 6, karmaShift: 0) },
            { "B_16", new Card("B_16", "Nekonečný hněv", "Udělí 3 zranění pětkrát! Karma -4.", cost: 2, damage: 3, hitCount: 5, karmaShift: -4) },
            { "B_17", new Card("B_17", "Zlost", "Udělí 10 zranění. Stojí 0 energie! Karma -2.", cost: 0, damage: 10, karmaShift: -2) },
            { "B_18", new Card("B_18", "Odplata krve", "Udělí 12 zranění. Karma -1.", cost: 1, damage: 12, karmaShift: -1) },
            { "B_19", new Card("B_19", "Rozseknutí", "Udělí 22 zranění. Karma -3.", cost: 2, damage: 22, karmaShift: -3) },
            { "B_20", new Card("B_20", "Barbarství", "Udělí 16 zranění. Karma -2.", cost: 2, damage: 16, karmaShift: -2) },
            { "B_21", new Card("B_21", "Divoký švih", "Udělí 9 zranění. Karma -1.", cost: 1, damage: 9, karmaShift: -1) },
            { "B_22", new Card("B_22", "Kladivo zkázy", "Udělí 28 zranění. Karma -4.", cost: 3, damage: 28, karmaShift: -4) },
            { "B_23", new Card("B_23", "Bolestivý sek", "Udělí 15 zranění. Karma -2.", cost: 1, damage: 15, karmaShift: -2) },
            { "B_24", new Card("B_24", "Rozpárání", "Udělí 7 zranění dvakrát. Karma -3.", cost: 2, damage: 7, hitCount: 2, karmaShift: -3) },
            { "B_25", new Card("B_25", "Vír smrti", "Udělí 5 zranění čtyřikrát. Karma -4.", cost: 3, damage: 5, hitCount: 4, karmaShift: -4) },

            // --- Adrenalin, Lízání a chabá obrana (26-40) ---
            { "B_26", new Card("B_26", "Adrenalin", "Lízni 2 karty. Stojí 0. Karma -1.", cost: 0, drawCards: 2, karmaShift: -1) },
            { "B_27", new Card("B_27", "Vroucí krev", "Lízni 3 karty, stoji tě 3 HP. Karma -2.", cost: 1, drawCards: 3, karmaShift: -2) },
            { "B_28", new Card("B_28", "Bojový pokřik", "Lízni 1 kartu, přidá 4 blok. Karma 0.", cost: 0, drawCards: 1, block: 4, karmaShift: 0) },
            { "B_29", new Card("B_29", "Ignorování bolesti", "Přidá 10 bloku. Karma -1.", cost: 1, block: 10, karmaShift: -1) },
            { "B_30", new Card("B_30", "Zatnutí zubů", "Přidá 15 bloku. Karma -2.", cost: 2, block: 15, karmaShift: -2) },
            { "B_31", new Card("B_31", "Krvavý štít", "Přidá 8 bloku, lízni 1 kartu. Karma -1.", cost: 1, block: 8, drawCards: 1, karmaShift: -1) },
            { "B_32", new Card("B_32", "Šílenství", "Lízni 4 karty! Karma -3.", cost: 2, drawCards: 4, karmaShift: -3) },
            { "B_33", new Card("B_33", "Druhej dech", "Vyléčí 10 HP. Karma +1.", cost: 1, heal: 10, karmaShift: 1) }, // Výjimečná léčba
            { "B_34", new Card("B_34", "Řev", "Lízni 2 karty, Karma -2.", cost: 1, drawCards: 2, karmaShift: -2) },
            { "B_35", new Card("B_35", "Tlustá kůže", "Přidá 12 bloku. Karma 0.", cost: 2, block: 12, karmaShift: 0) },
            { "B_36", new Card("B_36", "Zběsilost", "Lízni 2 karty. Stojí 0.", cost: 0, drawCards: 2) },
            { "B_37", new Card("B_37", "Připravenost", "Lízni 3 karty, přidá 5 blok. Karma -1.", cost: 2, drawCards: 3, block: 5, karmaShift: -1) },
            { "B_38", new Card("B_38", "Poslední vzdor", "Přidá 20 bloku! Karma -2.", cost: 3, block: 20, karmaShift: -2) },
            { "B_39", new Card("B_39", "Zkrvavená pěst", "Udělí 5 zranění, přidá 5 blok. Karma -1.", cost: 1, damage: 5, block: 5, karmaShift: -1) },
            { "B_40", new Card("B_40", "Nabuzení", "Lízni 1 kartu, vyléčí 2 HP. Karma 0.", cost: 0, drawCards: 1, heal: 2, karmaShift: 0) },

            // --- Ultimátní zkáza (41-50) ---
            { "B_41", new Card("B_41", "Zkáza titánů", "Udělí 35 zranění! Karma -5.", cost: 4, damage: 35, karmaShift: -5) },
            { "B_42", new Card("B_42", "Ničivé tornádo", "Udělí 10 zranění třikrát! Karma -4.", cost: 3, damage: 10, hitCount: 3, karmaShift: -4) },
            { "B_43", new Card("B_43", "Apokalyptický úder", "Udělí 50 zranění!!! Karma -6.", cost: 5, damage: 50, karmaShift: -6) },
            { "B_44", new Card("B_44", "Krvavá lázeň", "Udělí 12 zranění, vyléčí 12 HP. Karma -4.", cost: 3, damage: 12, heal: 12, karmaShift: -4) },
            { "B_45", new Card("B_45", "Nezastavitelný", "Přidá 25 bloku, lízni 3 karty. Karma -3.", cost: 3, block: 25, drawCards: 3, karmaShift: -3) },
            { "B_46", new Card("B_46", "Ragnarok", "Udělí 15 zranění dvakrát. Karma -5.", cost: 3, damage: 15, hitCount: 2, karmaShift: -5) },
            { "B_47", new Card("B_47", "Pád do temnoty", "Lízni 5 karet, Karma -5.", cost: 2, drawCards: 5, karmaShift: -5) },
            { "B_48", new Card("B_48", "Totální destrukce", "Udělí 40 zranění. Karma -5.", cost: 4, damage: 40, karmaShift: -5) },
            { "B_49", new Card("B_49", "Krveprolití", "Udělí 8 zranění čtyřikrát! Karma -6.", cost: 4, damage: 8, hitCount: 4, karmaShift: -6) },
            { "B_50", new Card("B_50", "Poslední soud", "Udělí 60 zranění, ale zabije tě to (později nasimulujeme). Karma -7.", cost: 5, damage: 60, karmaShift: -7) },
            // ==========================================
            // 5. DRUID (OCHRÁNCE) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Útoky přírody a vampirismus (1-20) ---
            { "D_01", new Card("D_01", "Šlehnutí liánou", "Udělí 4 zranění a přidá 4 blok. Karma +1.", cost: 1, damage: 4, block: 4, karmaShift: 1) },
            { "D_02", new Card("D_02", "Vysátí života", "Udělí 3 zranění a vyléčí 3 HP. Karma 0.", cost: 1, damage: 3, heal: 3, karmaShift: 0) },
            { "D_03", new Card("D_03", "Hod kamenem", "Udělí 8 zranění. Karma 0.", cost: 1, damage: 8, karmaShift: 0) },
            { "D_04", new Card("D_04", "Roj včel", "Udělí 2 zranění čtyřikrát. Karma 0.", cost: 1, damage: 2, hitCount: 4, karmaShift: 0) },
            { "D_05", new Card("D_05", "Jedovatý trn", "Udělí 5 zranění, lízni 1 kartu. Karma -1.", cost: 1, damage: 5, drawCards: 1, karmaShift: -1) },
            { "D_06", new Card("D_06", "Drtivý kmen", "Udělí 14 zranění. Karma 0.", cost: 2, damage: 14, karmaShift: 0) },
            { "D_07", new Card("D_07", "Kousnutí vlka", "Udělí 10 zranění. Karma -1.", cost: 1, damage: 10, karmaShift: -1) },
            { "D_08", new Card("D_08", "Medvědí spár", "Udělí 12 zranění, přidá 5 blok. Karma 0.", cost: 2, damage: 12, block: 5, karmaShift: 0) },
            { "D_09", new Card("D_09", "Písečná bouře", "Udělí 6 zranění dvakrát. Karma 0.", cost: 2, damage: 6, hitCount: 2, karmaShift: 0) },
            { "D_10", new Card("D_10", "Zemětřesení", "Udělí 15 zranění. Karma -1.", cost: 3, damage: 15, karmaShift: -1) },
            { "D_11", new Card("D_11", "Kořeny", "Udělí 5 zranění. Stojí 0. Karma 0.", cost: 0, damage: 5, karmaShift: 0) },
            { "D_12", new Card("D_12", "Přírodní výběr", "Udělí 8 zranění, vyléčí 4 HP. Karma +1.", cost: 2, damage: 8, heal: 4, karmaShift: 1) },
            { "D_13", new Card("D_13", "Úder blesku", "Udělí 18 zranění! Karma +2.", cost: 2, damage: 18, karmaShift: 2) },
            { "D_14", new Card("D_14", "Spory", "Udělí 3 zranění všem. Karma 0.", cost: 1, damage: 3, karmaShift: 0) },
            { "D_15", new Card("D_15", "Ostrý list", "Udělí 7 zranění. Karma +1.", cost: 1, damage: 7, karmaShift: 1) },
            { "D_16", new Card("D_16", "Divoký útok", "Udělí 20 zranění. Karma -2.", cost: 3, damage: 20, karmaShift: -2) },
            { "D_17", new Card("D_17", "Mrazivý vítr", "Udělí 5 zranění, lízni 1 kartu. Karma +1.", cost: 1, damage: 5, drawCards: 1, karmaShift: 1) },
            { "D_18", new Card("D_18", "Výpad šelmy", "Udělí 9 zranění. Karma 0.", cost: 1, damage: 9, karmaShift: 0) },
            { "D_19", new Card("D_19", "Odveta lesa", "Udělí 11 zranění a přidá 3 blok. Karma 0.", cost: 2, damage: 11, block: 3, karmaShift: 0) },
            { "D_20", new Card("D_20", "Zatopení", "Udělí 12 zranění. Karma +1.", cost: 2, damage: 12, karmaShift: 1) },

            // --- Obrana, léčení a přírodní růst (21-40) ---
            { "D_21", new Card("D_21", "Dubová kůra", "Přidá 7 bloku. Karma +1.", cost: 1, block: 7, karmaShift: 1) },
            { "D_22", new Card("D_22", "Fotosyntéza", "Vyléčí 5 HP, přidá 5 blok. Karma +2.", cost: 2, heal: 5, block: 5, karmaShift: 2) },
            { "D_23", new Card("D_23", "Bylinky", "Vyléčí 4 HP, lízni 1 kartu. Karma +1.", cost: 1, heal: 4, drawCards: 1, karmaShift: 1) },
            { "D_24", new Card("D_24", "Růst", "Lízni 2 karty. Karma +1.", cost: 1, drawCards: 2, karmaShift: 1) },
            { "D_25", new Card("D_25", "Hustý porost", "Přidá 12 bloku. Karma +1.", cost: 2, block: 12, karmaShift: 1) },
            { "D_26", new Card("D_26", "Léčivý pramen", "Vyléčí 8 HP. Karma +2.", cost: 1, heal: 8, karmaShift: 2) },
            { "D_27", new Card("D_27", "Cyklus", "Lízni 2 karty, vyléčí 2 HP. Stojí 0. Karma 0.", cost: 0, drawCards: 2, heal: 2, karmaShift: 0) },
            { "D_28", new Card("D_28", "Kamenná zeď", "Přidá 15 bloku. Karma 0.", cost: 2, block: 15, karmaShift: 0) },
            { "D_29", new Card("D_29", "Hojnost", "Lízni 3 karty. Karma +2.", cost: 2, drawCards: 3, karmaShift: 2) },
            { "D_30", new Card("D_30", "Květ života", "Vyléčí 12 HP! Karma +3.", cost: 2, heal: 12, karmaShift: 3) },
            { "D_31", new Card("D_31", "Ranní rosa", "Vyléčí 3 HP za 0 energie. Karma +1.", cost: 0, heal: 3, karmaShift: 1) },
            { "D_32", new Card("D_32", "Pavučina", "Přidá 5 bloku, lízni 1 kartu. Karma 0.", cost: 1, block: 5, drawCards: 1, karmaShift: 0) },
            { "D_33", new Card("D_33", "Zátiší", "Přidá 8 bloku. Karma +2.", cost: 1, block: 8, karmaShift: 2) },
            { "D_34", new Card("D_34", "Houbové spory", "Vyléčí 5 HP, přidá 2 blok. Karma 0.", cost: 1, heal: 5, block: 2, karmaShift: 0) },
            { "D_35", new Card("D_35", "Zrození", "Lízni 4 karty. Karma +2.", cost: 2, drawCards: 4, karmaShift: 2) },
            { "D_36", new Card("D_36", "Ochranný mech", "Přidá 4 bloku za 0 energie. Karma 0.", cost: 0, block: 4, karmaShift: 0) },
            { "D_37", new Card("D_37", "Zvířecí instinkt", "Lízni 1 kartu, Karma +1.", cost: 0, drawCards: 1, karmaShift: 1) },
            { "D_38", new Card("D_38", "Medvědí spánek", "Vyléčí 15 HP, přidá 10 blok. Karma +3.", cost: 3, heal: 15, block: 10, karmaShift: 3) },
            { "D_39", new Card("D_39", "Zázrak přírody", "Lízni 2 karty, vyléčí 5 HP. Karma +1.", cost: 2, drawCards: 2, heal: 5, karmaShift: 1) },
            { "D_40", new Card("D_40", "Míza", "Přidá 10 bloku. Karma +1.", cost: 1, block: 10, karmaShift: 1) },

            // --- Ultimátní síly přírody (41-50) ---
            { "D_41", new Card("D_41", "Hněv lesa", "Udělí 25 zranění! Karma 0.", cost: 3, damage: 25, karmaShift: 0) },
            { "D_42", new Card("D_42", "Živoucí prales", "Přidá 25 bloku! Karma +3.", cost: 3, block: 25, karmaShift: 3) },
            { "D_43", new Card("D_43", "Strom života", "Vyléčí 25 HP!!! Karma +5.", cost: 4, heal: 25, karmaShift: 5) },
            { "D_44", new Card("D_44", "Sluneční erupce", "Udělí 15 zranění, vyléčí 10 HP. Karma +4.", cost: 4, damage: 15, heal: 10, karmaShift: 4) },
            { "D_45", new Card("D_45", "Volání divočiny", "Lízni 5 karet. Karma +2.", cost: 3, drawCards: 5, karmaShift: 2) },
            { "D_46", new Card("D_46", "Přírodní katastrofa", "Udělí 10 zranění třikrát! Karma -2.", cost: 4, damage: 10, hitCount: 3, karmaShift: -2) },
            { "D_47", new Card("D_47", "Splynutí s přírodou", "Vyléčí 15 HP, přidá 15 blok, lízni 2 karty. Karma +4.", cost: 5, heal: 15, block: 15, drawCards: 2, karmaShift: 4) },
            { "D_48", new Card("D_48", "Pohlcující liány", "Udělí 20 zranění, přidá 10 blok. Karma +1.", cost: 3, damage: 20, block: 10, karmaShift: 1) },
            { "D_49", new Card("D_49", "Znovuzrození lesa", "Vyléčí 20 HP, lízni 3 karty. Karma +3.", cost: 4, heal: 20, drawCards: 3, karmaShift: 3) },
            { "D_50", new Card("D_50", "Matka příroda", "Udělí 30 zranění, vyléčí 15 HP! Karma +5.", cost: 5, damage: 30, heal: 15, karmaShift: 5) },
            // ==========================================
            // 6. ROGUE (KARMICKÝ VRAH) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Rychlé dýky a levné útoky (1-20) ---
            { "R_01", new Card("R_01", "Rychlá dýka", "Udělí 4 zranění. Karma -1.", cost: 1, damage: 4, karmaShift: -1) },
            { "R_02", new Card("R_02", "Dvojitý sek", "Udělí 3 zranění dvakrát. Karma -1.", cost: 1, damage: 3, hitCount: 2, karmaShift: -1) },
            { "R_03", new Card("R_03", "Zákeřná rána", "Udělí 8 zranění. Karma -2.", cost: 1, damage: 8, karmaShift: -2) },
            { "R_04", new Card("R_04", "Jedovatá čepel", "Udělí 5 zranění, lízni 1 kartu. Karma -1.", cost: 1, damage: 5, drawCards: 1, karmaShift: -1) },
            { "R_05", new Card("R_05", "Bodnutí do zad", "Udělí 12 zranění! Karma -3.", cost: 1, damage: 12, karmaShift: -3) },
            { "R_06", new Card("R_06", "Vrhací nože", "Udělí 2 zranění třikrát. Karma 0.", cost: 1, damage: 2, hitCount: 3, karmaShift: 0) },
            { "R_07", new Card("R_07", "Přepadení", "Udělí 6 zranění za 0 energie! Karma -2.", cost: 0, damage: 6, karmaShift: -2) },
            { "R_08", new Card("R_08", "Skrytá čepel", "Udělí 4 zranění. Stojí 0. Karma -1.", cost: 0, damage: 4, karmaShift: -1) },
            { "R_09", new Card("R_09", "Přeseknutí žil", "Udělí 7 zranění. Karma -1.", cost: 1, damage: 7, karmaShift: -1) },
            { "R_10", new Card("R_10", "Zářivý klam", "Udělí 5 zranění. Karma +2.", cost: 1, damage: 5, karmaShift: 2) }, // Klamný útok do Světla
            { "R_11", new Card("R_11", "Smrtící pirueta", "Udělí 4 zranění všem. Karma -2.", cost: 2, damage: 4, karmaShift: -2) },
            { "R_12", new Card("R_12", "Podříznutí", "Udělí 15 zranění. Karma -3.", cost: 2, damage: 15, karmaShift: -3) },
            { "R_13", new Card("R_13", "Krvavé kombo", "Udělí 6 zranění dvakrát. Karma -2.", cost: 2, damage: 6, hitCount: 2, karmaShift: -2) },
            { "R_14", new Card("R_14", "Stínový úder", "Udělí 9 zranění, stoji 0 energie. Karma -3.", cost: 0, damage: 9, karmaShift: -3) },
            { "R_15", new Card("R_15", "Vykuchání", "Udělí 20 zranění! Karma -4.", cost: 2, damage: 20, karmaShift: -4) },
            { "R_16", new Card("R_16", "Nervový bod", "Udělí 3 zranění, přidá 5 blok. Karma 0.", cost: 1, damage: 3, block: 5, karmaShift: 0) },
            { "R_17", new Card("R_17", "Rychlý odhoz", "Udělí 5 zranění, lízni 2 karty. Karma -1.", cost: 1, damage: 5, drawCards: 2, karmaShift: -1) },
            { "R_18", new Card("R_18", "Odvedení pozornosti", "Udělí 4 zranění. Karma +3.", cost: 1, damage: 4, karmaShift: 3) },
            { "R_19", new Card("R_19", "Fantomový úder", "Udělí 10 zranění. Karma -1.", cost: 1, damage: 10, karmaShift: -1) },
            { "R_20", new Card("R_20", "Záblesk dýky", "Udělí 8 zranění. Karma +1.", cost: 1, damage: 8, karmaShift: 1) },

            // --- Úhyby, Lízání a Extrémní výkyvy Karmy (21-40) ---
            { "R_21", new Card("R_21", "Úhyb", "Přidá 6 bloku. Karma +3.", cost: 1, block: 6, karmaShift: 3) },
            { "R_22", new Card("R_22", "Kouřová clona", "Přidá 8 bloku, lízni 1 kartu. Karma +2.", cost: 1, block: 8, drawCards: 1, karmaShift: 2) },
            { "R_23", new Card("R_23", "Příprava", "Lízni 3 karty! Stojí 0! Karma 0.", cost: 0, drawCards: 3, karmaShift: 0) }, // Extrémně silná karta pro komba
            { "R_24", new Card("R_24", "Akrobacie", "Přidá 4 bloku, lízni 2 karty. Karma +1.", cost: 1, block: 4, drawCards: 2, karmaShift: 1) },
            { "R_25", new Card("R_25", "Stíny", "Přidá 10 bloku. Karma -2.", cost: 1, block: 10, karmaShift: -2) },
            { "R_26", new Card("R_26", "Salto vzad", "Přidá 5 bloku, Karma +4.", cost: 1, block: 5, karmaShift: 4) },
            { "R_27", new Card("R_27", "Adrenalinový skok", "Lízni 2 karty. Karma +2.", cost: 1, drawCards: 2, karmaShift: 2) },
            { "R_28", new Card("R_28", "Bleskové reflexy", "Přidá 15 bloku! Karma +3.", cost: 2, block: 15, karmaShift: 3) },
            { "R_29", new Card("R_29", "Splynutí se stínem", "Karma -4 za 0 energie.", cost: 0, karmaShift: -4) }, // Čistá manipulace
            { "R_30", new Card("R_30", "Výstup na světlo", "Karma +4 za 0 energie.", cost: 0, karmaShift: 4) }, // Čistá manipulace
            { "R_31", new Card("R_31", "Soustředění", "Lízni 2 karty, vyléčí 2 HP. Karma 0.", cost: 1, drawCards: 2, heal: 2, karmaShift: 0) },
            { "R_32", new Card("R_32", "Útěk", "Přidá 12 bloku. Karma +2.", cost: 2, block: 12, karmaShift: 2) },
            { "R_33", new Card("R_33", "Hra se smrtí", "Lízni 4 karty. Karma -3.", cost: 1, drawCards: 4, karmaShift: -3) },
            { "R_34", new Card("R_34", "Přežití", "Vyléčí 5 HP, přidá 5 blok. Karma +1.", cost: 1, heal: 5, block: 5, karmaShift: 1) },
            { "R_35", new Card("R_35", "Falešný krok", "Přidá 4 bloku za 0 energie. Karma -1.", cost: 0, block: 4, karmaShift: -1) },
            { "R_36", new Card("R_36", "Ošálení", "Lízni 1 kartu, přidá 3 blok. Stojí 0.", cost: 0, drawCards: 1, block: 3, karmaShift: 0) },
            { "R_37", new Card("R_37", "Zásoby", "Vyléčí 8 HP. Karma +2.", cost: 1, heal: 8, karmaShift: 2) },
            { "R_38", new Card("R_38", "Mistr úniků", "Přidá 20 bloku! Karma +4.", cost: 3, block: 20, karmaShift: 4) },
            { "R_39", new Card("R_39", "Předtucha", "Lízni 2 karty, přidá 5 blok. Karma +1.", cost: 1, drawCards: 2, block: 5, karmaShift: 1) },
            { "R_40", new Card("R_40", "Odhalení slabiny", "Lízni 3 karty. Karma +2.", cost: 2, drawCards: 3, karmaShift: 2) },

            // --- Ultimátní asasínské techniky (41-50) ---
            { "R_41", new Card("R_41", "Tanec stínů", "Udělí 5 zranění pětkrát! Karma -5.", cost: 3, damage: 5, hitCount: 5, karmaShift: -5) },
            { "R_42", new Card("R_42", "Nekonečné kombo", "Lízni 5 karet! Karma 0.", cost: 2, drawCards: 5, karmaShift: 0) },
            { "R_43", new Card("R_43", "Atentát", "Udělí 40 zranění! Karma -4.", cost: 3, damage: 40, karmaShift: -4) },
            { "R_44", new Card("R_44", "Přízrak", "Přidá 30 bloku!!! Karma +5.", cost: 3, block: 30, karmaShift: 5) },
            { "R_45", new Card("R_45", "Krvavá stopa", "Udělí 15 zranění dvakrát, lízni 2 karty. Karma -3.", cost: 3, damage: 15, hitCount: 2, drawCards: 2, karmaShift: -3) },
            { "R_46", new Card("R_46", "Dokonalý plán", "Lízni 6 karet! Karma +2.", cost: 3, drawCards: 6, karmaShift: 2) },
            { "R_47", new Card("R_47", "Skrytý drak", "Udělí 25 zranění za 1 energii (vyžaduje kombo). Karma -2.", cost: 1, damage: 25, karmaShift: -2) },
            { "R_48", new Card("R_48", "Očištění krve", "Vyléčí 20 HP. Karma +5.", cost: 3, heal: 20, karmaShift: 5) },
            { "R_49", new Card("R_49", "Bouře dýk", "Udělí 3 zranění desetkrát!!! Karma -6.", cost: 4, damage: 3, hitCount: 10, karmaShift: -6) },
            { "R_50", new Card("R_50", "Dotek smrtky", "Udělí 60 zranění. Karma -8.", cost: 4, damage: 60, karmaShift: -8) },

            // ==========================================
            // 7. BARD (SUPPORT) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Lehké útoky a oslabující melodie (1-15) ---
            { "Bd_01", new Card("Bd_01", "Falešný tón", "Udělí 3 zranění. Karma -1.", cost: 1, damage: 3, karmaShift: -1) },
            { "Bd_02", new Card("Bd_02", "Ostrý akord", "Udělí 5 zranění. Karma +1.", cost: 1, damage: 5, karmaShift: 1) },
            { "Bd_03", new Card("Bd_03", "Rytmický úder", "Udělí 2 zranění dvakrát. Karma 0.", cost: 1, damage: 2, hitCount: 2, karmaShift: 0) },
            { "Bd_04", new Card("Bd_04", "Rušivá píseň", "Udělí 4 zranění, lízni 1 kartu. Karma -1.", cost: 1, damage: 4, drawCards: 1, karmaShift: -1) },
            { "Bd_05", new Card("Bd_05", "Bojový marš", "Udělí 6 zranění, přidá 3 blok. Karma +1.", cost: 1, damage: 6, block: 3, karmaShift: 1) },
            { "Bd_06", new Card("Bd_06", "Výsměch", "Udělí 8 zranění. Karma -2.", cost: 1, damage: 8, karmaShift: -2) },
            { "Bd_07", new Card("Bd_07", "Crescendo", "Udělí 12 zranění. Karma +2.", cost: 2, damage: 12, karmaShift: 2) },
            { "Bd_08", new Card("Bd_08", "Bodnutí dýkou", "Udělí 6 zranění. Stojí 0. Karma -1.", cost: 0, damage: 6, karmaShift: -1) },
            { "Bd_09", new Card("Bd_09", "Uši rvoucí ryk", "Udělí 4 zranění všem. Karma -2.", cost: 2, damage: 4, karmaShift: -2) },
            { "Bd_10", new Card("Bd_10", "Disonance", "Udělí 10 zranění. Karma -3.", cost: 1, damage: 10, karmaShift: -3) },
            { "Bd_11", new Card("Bd_11", "Hrdinský epos", "Udělí 8 zranění, vyléčí 2 HP. Karma +1.", cost: 2, damage: 8, heal: 2, karmaShift: 1) },
            { "Bd_12", new Card("Bd_12", "Ozvena", "Udělí 3 zranění třikrát. Karma 0.", cost: 2, damage: 3, hitCount: 3, karmaShift: 0) },
            { "Bd_13", new Card("Bd_13", "Rána loutnou", "Udělí 15 zranění. Karma -1.", cost: 2, damage: 15, karmaShift: -1) },
            { "Bd_14", new Card("Bd_14", "Sólo na bubny", "Udělí 5 zranění za 0 energie. Karma 0.", cost: 0, damage: 5, karmaShift: 0) },
            { "Bd_15", new Card("Bd_15", "Závěrečný tón", "Udělí 20 zranění. Karma -2.", cost: 3, damage: 20, karmaShift: -2) },

            // --- Masivní podpora, štíty a léčení (16-35) ---
            { "Bd_16", new Card("Bd_16", "Píseň naděje", "Vyléčí 4 HP, přidá 4 blok. Karma +3.", cost: 1, heal: 4, block: 4, karmaShift: 3) },
            { "Bd_17", new Card("Bd_17", "Zklidňující melodie", "Přidá 8 bloku. Karma +2.", cost: 1, block: 8, karmaShift: 2) },
            { "Bd_18", new Card("Bd_18", "Hymna ochrany", "Přidá 15 bloku (rozdělí se). Karma +3.", cost: 2, block: 15, karmaShift: 3) },
            { "Bd_19", new Card("Bd_19", "Píseň světla", "Vyléčí 8 HP. Karma +2.", cost: 1, heal: 8, karmaShift: 2) },
            { "Bd_20", new Card("Bd_20", "Povzbuzení", "Lízni 2 karty. Karma +1.", cost: 1, drawCards: 2, karmaShift: 1) },
            { "Bd_21", new Card("Bd_21", "Ukolébavka", "Přidá 10 bloku, lízni 1 kartu. Karma +2.", cost: 2, block: 10, drawCards: 1, karmaShift: 2) },
            { "Bd_22", new Card("Bd_22", "Léčivý sbor", "Vyléčí 12 HP. Karma +3.", cost: 2, heal: 12, karmaShift: 3) },
            { "Bd_23", new Card("Bd_23", "Inspirace", "Lízni 3 karty. Karma 0.", cost: 1, drawCards: 3, karmaShift: 0) },
            { "Bd_24", new Card("Bd_24", "Rychlé akordy", "Přidá 5 bloku, lízni 2 karty. Karma +1.", cost: 1, block: 5, drawCards: 2, karmaShift: 1) },
            { "Bd_25", new Card("Bd_25", "Štít z hudby", "Přidá 20 bloku! Karma +4.", cost: 3, block: 20, karmaShift: 4) },
            { "Bd_26", new Card("Bd_26", "Láskyplná serenáda", "Vyléčí 5 HP za 0 energie. Karma +1.", cost: 0, heal: 5, karmaShift: 1) },
            { "Bd_27", new Card("Bd_27", "Radostná zpráva", "Vyléčí 3 HP, přidá 5 blok. Karma +2.", cost: 1, heal: 3, block: 5, karmaShift: 2) },
            { "Bd_28", new Card("Bd_28", "Melodie života", "Vyléčí 15 HP! Karma +4.", cost: 3, heal: 15, karmaShift: 4) },
            { "Bd_29", new Card("Bd_29", "Krátká pauza", "Přidá 4 bloku. Stojí 0. Karma 0.", cost: 0, block: 4, karmaShift: 0) },
            { "Bd_30", new Card("Bd_30", "Sborový zpěv", "Vyléčí 8 HP, přidá 8 blok. Karma +3.", cost: 2, heal: 8, block: 8, karmaShift: 3) },
            { "Bd_31", new Card("Bd_31", "Aura múzy", "Lízni 2 karty, vyléčí 2 HP. Karma +1.", cost: 1, drawCards: 2, heal: 2, karmaShift: 1) },
            { "Bd_32", new Card("Bd_32", "Nezlomný rytmus", "Přidá 12 bloku. Karma +1.", cost: 1, block: 12, karmaShift: 1) },
            { "Bd_33", new Card("Bd_33", "Balada o štítu", "Přidá 8 bloku za 0 energie! Karma +2.", cost: 0, block: 8, karmaShift: 2) },
            { "Bd_34", new Card("Bd_34", "Píseň znovuzrození", "Vyléčí 10 HP, lízni 1 kartu. Karma +3.", cost: 2, heal: 10, drawCards: 1, karmaShift: 3) },
            { "Bd_35", new Card("Bd_35", "Harmonie duší", "Vyléčí 6 HP, přidá 6 blok. Karma +2.", cost: 1, heal: 6, block: 6, karmaShift: 2) },

            // --- Drastická manipulace s Karmou (36-45) ---
            { "Bd_36", new Card("Bd_36", "Píseň zmaru", "Udělí 3 zranění. Karma -4! (Pro pomoc Warlockovi).", cost: 1, damage: 3, karmaShift: -4) },
            { "Bd_37", new Card("Bd_37", "Zpěv zoufalství", "Lízni 2 karty. Karma -3.", cost: 1, drawCards: 2, karmaShift: -3) },
            { "Bd_38", new Card("Bd_38", "Svatý chorál", "Přidá 5 bloku. Karma +5! (Pro pomoc Paladinovi).", cost: 1, block: 5, karmaShift: 5) },
            { "Bd_39", new Card("Bd_39", "Pád do temnot", "Karma -5 za 0 energie.", cost: 0, karmaShift: -5) },
            { "Bd_40", new Card("Bd_40", "Vzestup ke světlu", "Karma +5 za 0 energie.", cost: 0, karmaShift: 5) },
            { "Bd_41", new Card("Bd_41", "Smutná melodie", "Vyléčí 4 HP. Karma -2.", cost: 1, heal: 4, karmaShift: -2) },
            { "Bd_42", new Card("Bd_42", "Veselá melodie", "Vyléčí 4 HP. Karma +2.", cost: 1, heal: 4, karmaShift: 2) },
            { "Bd_43", new Card("Bd_43", "Rozladění", "Udělí 5 zranění. Karma -3.", cost: 1, damage: 5, karmaShift: -3) },
            { "Bd_44", new Card("Bd_44", "Dokonalé naladění", "Lízni 3 karty. Karma +3.", cost: 2, drawCards: 3, karmaShift: 3) },
            { "Bd_45", new Card("Bd_45", "Karmický obrat", "Udělí 2 zranění, přidá 2 blok. Karma 0.", cost: 0, damage: 2, block: 2, karmaShift: 0) },

            // --- Ultimátní orchestr (46-50) ---
            { "Bd_46", new Card("Bd_46", "Óda na radost", "Vyléčí 25 HP! Karma +6.", cost: 4, heal: 25, karmaShift: 6) },
            { "Bd_47", new Card("Bd_47", "Requiem", "Udělí 30 zranění! Karma -6.", cost: 4, damage: 30, karmaShift: -6) },
            { "Bd_48", new Card("Bd_48", "Symfonie štítů", "Přidá 40 bloku!!! Karma +4.", cost: 4, block: 40, karmaShift: 4) },
            { "Bd_49", new Card("Bd_49", "Mistrovské dílo", "Vyléčí 15 HP, přidá 15 blok, lízni 3 karty. Karma +5.", cost: 5, heal: 15, block: 15, drawCards: 3, karmaShift: 5) },
            { "Bd_50", new Card("Bd_50", "Poslední přídavek", "Lízni 7 karet! Karma 0.", cost: 3, drawCards: 7, karmaShift: 0) },
            // ==========================================
            // 8. PYROMANCER (PYROMANT) - 50 UNIKÁTNÍCH KARET
            // ==========================================

            // --- Rychlé plameny a jiskry (1-20) ---
            { "Py_01", new Card("Py_01", "Ohnivá koule", "Udělí 12 zranění. Karma -1.", cost: 2, damage: 12, karmaShift: -1) },
            { "Py_02", new Card("Py_02", "Jiskra", "Udělí 4 zranění za 0 energie. Karma 0.", cost: 0, damage: 4, karmaShift: 0) },
            { "Py_03", new Card("Py_03", "Zápal", "Udělí 7 zranění. Karma -1.", cost: 1, damage: 7, karmaShift: -1) },
            { "Py_04", new Card("Py_04", "Plamínek", "Udělí 3 zranění dvakrát. Karma 0.", cost: 1, damage: 3, hitCount: 2, karmaShift: 0) },
            { "Py_05", new Card("Py_05", "Ohnivý bič", "Udělí 9 zranění. Karma -2.", cost: 1, damage: 9, karmaShift: -2) },
            { "Py_06", new Card("Py_06", "Vzplanutí", "Udělí 5 zranění, lízni 1 kartu. Karma -1.", cost: 1, damage: 5, drawCards: 1, karmaShift: -1) },
            { "Py_07", new Card("Py_07", "Sežehnutí", "Udělí 14 zranění. Karma -2.", cost: 2, damage: 14, karmaShift: -2) },
            { "Py_08", new Card("Py_08", "Ohnivý proud", "Udělí 4 zranění třikrát. Karma -1.", cost: 2, damage: 4, hitCount: 3, karmaShift: -1) },
            { "Py_09", new Card("Py_09", "Exploze", "Udělí 18 zranění! Karma -3.", cost: 2, damage: 18, karmaShift: -3) },
            { "Py_10", new Card("Py_10", "Dračí dech", "Udělí 10 zranění všem. Karma -2.", cost: 2, damage: 10, karmaShift: -2) },
            { "Py_11", new Card("Py_11", "Spáleniště", "Udělí 6 zranění, přidá 4 blok. Karma -1.", cost: 1, damage: 6, block: 4, karmaShift: -1) },
            { "Py_12", new Card("Py_12", "Popel", "Udělí 5 zranění. Stojí 0. Karma -1.", cost: 0, damage: 5, karmaShift: -1) },
            { "Py_13", new Card("Py_13", "Vulkanická rána", "Udělí 20 zranění! Karma -4.", cost: 3, damage: 20, karmaShift: -4) },
            { "Py_14", new Card("Py_14", "Ohnivý déšť", "Udělí 6 zranění dvakrát. Karma -2.", cost: 2, damage: 6, hitCount: 2, karmaShift: -2) },
            { "Py_15", new Card("Py_15", "Tepelný šok", "Udělí 15 zranění. Karma -2.", cost: 2, damage: 15, karmaShift: -2) },
            { "Py_16", new Card("Py_16", "Roztavení", "Udělí 8 zranění, lízni 2 karty. Karma -3.", cost: 2, damage: 8, drawCards: 2, karmaShift: -3) },
            { "Py_17", new Card("Py_17", "Žhavé uhlíky", "Udělí 2 zranění čtyřikrát. Karma -1.", cost: 1, damage: 2, hitCount: 4, karmaShift: -1) },
            { "Py_18", new Card("Py_18", "Výbuch hněvu", "Udělí 12 zranění. Karma -3.", cost: 1, damage: 12, karmaShift: -3) },
            { "Py_19", new Card("Py_19", "Pekelný šleh", "Udělí 25 zranění. Karma -4.", cost: 3, damage: 25, karmaShift: -4) },
            { "Py_20", new Card("Py_20", "Zpopelnění", "Udělí 30 zranění! Karma -5.", cost: 4, damage: 30, karmaShift: -5) },

            // --- Ohnivé štíty, Lízání a Manipulace hořením (21-40) ---
            { "Py_21", new Card("Py_21", "Ohnivý štít", "Přidá 8 bloku, udělí 3 zranění. Karma -1.", cost: 1, block: 8, damage: 3, karmaShift: -1) },
            { "Py_22", new Card("Py_22", "Zeď plamenů", "Přidá 12 bloku. Karma -2.", cost: 2, block: 12, karmaShift: -2) },
            { "Py_23", new Card("Py_23", "Kauterizace", "Vyléčí 6 HP, ale Karma -2.", cost: 1, heal: 6, karmaShift: -2) }, // Ohnivé léčení bolí Karmu
            { "Py_24", new Card("Py_24", "Rozdmýchání", "Lízni 2 karty. Karma -1.", cost: 1, drawCards: 2, karmaShift: -1) },
            { "Py_25", new Card("Py_25", "Krmení ohně", "Lízni 3 karty! Karma -2.", cost: 1, drawCards: 3, karmaShift: -2) },
            { "Py_26", new Card("Py_26", "Teplo domova", "Vyléčí 4 HP, přidá 4 blok. Karma +1.", cost: 1, heal: 4, block: 4, karmaShift: 1) }, // Výjimečné světlo
            { "Py_27", new Card("Py_27", "Dým", "Přidá 6 bloku za 0 energie. Karma -1.", cost: 0, block: 6, karmaShift: -1) },
            { "Py_28", new Card("Py_28", "Uhašení", "Přidá 10 bloku. Karma 0.", cost: 1, block: 10, karmaShift: 0) },
            { "Py_29", new Card("Py_29", "Přísun kyslíku", "Lízni 2 karty. Stojí 0. Karma -1.", cost: 0, drawCards: 2, karmaShift: -1) },
            { "Py_30", new Card("Py_30", "Žár", "Udělí 7 zranění, přidá 7 blok. Karma -2.", cost: 2, damage: 7, block: 7, karmaShift: -2) },
            { "Py_31", new Card("Py_31", "Magma", "Přidá 15 bloku. Karma -3.", cost: 2, block: 15, karmaShift: -3) },
            { "Py_32", new Card("Py_32", "Soustředění plamene", "Lízni 1 kartu, Karma -2.", cost: 0, drawCards: 1, karmaShift: -2) },
            { "Py_33", new Card("Py_33", "Fénixovo pírko", "Vyléčí 15 HP! Karma +2.", cost: 3, heal: 15, karmaShift: 2) },
            { "Py_34", new Card("Py_34", "Hořící krev", "Vyléčí 5 HP, lízni 1 kartu. Karma -1.", cost: 1, heal: 5, drawCards: 1, karmaShift: -1) },
            { "Py_35", new Card("Py_35", "Zatmění", "Karma -4 za 0 energie. (Pro zesílení relikvie)", cost: 0, karmaShift: -4) },
            { "Py_36", new Card("Py_36", "Světlice", "Lízni 2 karty, Karma +2.", cost: 1, drawCards: 2, karmaShift: 2) },
            { "Py_37", new Card("Py_37", "Lávový štít", "Přidá 20 bloku! Karma -4.", cost: 3, block: 20, karmaShift: -4) },
            { "Py_38", new Card("Py_38", "Sopka", "Udělí 10 zranění, přidá 10 blok. Karma -3.", cost: 3, damage: 10, block: 10, karmaShift: -3) },
            { "Py_39", new Card("Py_39", "Zrychlené hoření", "Lízni 4 karty. Karma -3.", cost: 2, drawCards: 4, karmaShift: -3) },
            { "Py_40", new Card("Py_40", "Plamenná aura", "Přidá 5 bloku. Stojí 0. Karma 0.", cost: 0, block: 5, karmaShift: 0) },

            // --- Ultimátní ohnivé peklo (41-50) ---
            { "Py_41", new Card("Py_41", "Inferno", "Udělí 10 zranění třikrát!!! Karma -4.", cost: 3, damage: 10, hitCount: 3, karmaShift: -4) },
            { "Py_42", new Card("Py_42", "Supernova", "Udělí 40 zranění všem (zatím cíli)! Karma -6.", cost: 4, damage: 40, karmaShift: -6) },
            { "Py_43", new Card("Py_43", "Meteor", "Udělí 50 zranění! Karma -5.", cost: 4, damage: 50, karmaShift: -5) },
            { "Py_44", new Card("Py_44", "Ohnivá bouře", "Udělí 5 zranění osmkrát!!! Karma -7.", cost: 5, damage: 5, hitCount: 8, karmaShift: -7) },
            { "Py_45", new Card("Py_45", "Spálení na prach", "Udělí 35 zranění, lízni 2 karty. Karma -4.", cost: 4, damage: 35, drawCards: 2, karmaShift: -4) },
            { "Py_46", new Card("Py_46", "Sluneční záře", "Vyléčí 20 HP, přidá 20 blok! Karma +5.", cost: 4, heal: 20, block: 20, karmaShift: 5) },
            { "Py_47", new Card("Py_47", "Srdce sopky", "Lízni 6 karet! Karma -4.", cost: 3, drawCards: 6, karmaShift: -4) },
            { "Py_48", new Card("Py_48", "Plamenný kruh", "Přidá 30 bloku, udělí 15 zranění. Karma -3.", cost: 4, block: 30, damage: 15, karmaShift: -3) },
            { "Py_49", new Card("Py_49", "Armagedon", "Udělí 60 zranění! Karma -8.", cost: 5, damage: 60, karmaShift: -8) },
            { "Py_50", new Card("Py_50", "Konečný plamen", "Udělí 100 zranění, ale stojí veškerou energii a Karmu! Karma -10.", cost: 5, damage: 100, karmaShift: -10) },
        };

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
                    new List<string> { "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Uder", "Z_Obrana", "Z_Obrana", "H4_Zurit", "H4_Rana" }, 
                    new List<string> { "H4_Zurit", "H4_Rana" }
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