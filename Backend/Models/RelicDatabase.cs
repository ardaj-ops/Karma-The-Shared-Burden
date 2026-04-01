using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    

   public static class RelicDatabase
    {
        public static List<Relic> LootRelics = InitializeRelics();

        private static List<Relic> InitializeRelics()
        {
            var allRelics = new List<Relic>();
            allRelics.AddRange(GetHeroRelics());
            allRelics.AddRange(GetFirstHundred());
            allRelics.AddRange(GetSecondHundred());
            allRelics.AddRange(GetThirdHundred());
            return allRelics;
        }

        public static Relic GetRandomRelic()
        {
            Random rng = new Random();
            return LootRelics[rng.Next(LootRelics.Count)];
        }

        // --- STARTOVNÍ A SPECIÁLNÍ ---
        private static List<Relic> GetHeroRelics() => new List<Relic> {
            new Relic("Hero_01", "Svatý kodex", "Na začátku boje získáš 5 Bloku a tým získá +2 Karmu. (Paladin)"),
            new Relic("Hero_02", "Zkrvavený grimoár", "Na začátku boje ztratíš 2 HP, ale získáš 1 Manu. (Warlock)"),
            new Relic("Hero_03", "Mnišské korále", "Na začátku boje získáš 1 Obratnost. (Monk)"),
            new Relic("Hero_04", "Zubatá sekyra", "Na začátku boje získáš 1 Sílu. (Berserker)"),
            new Relic("Hero_05", "Zlatý žalud", "Na začátku boje získáš 1 Regeneraci. (Druid)"),
            new Relic("Hero_06", "Stínový plášť", "Na začátku boje aplikuje 1 Oslabení na všechny nepřátele. (Rogue)"),
            new Relic("Hero_07", "Rozladěná loutna", "Na začátku boje si lízni 1 kartu navíc. (Bard)"),
            new Relic("Hero_08", "Věčná pochodeň", "Na začátku boje aplikuje 1 Hoření na všechny nepřátele. (Pyromancer)"),
            new Relic("L_01", "Krvavý rubín", "Zvyšuje tvé Max HP o 10."),
            new Relic("L_02", "Zlatý skarabeus", "Získáš o 30 % více zlaťáků z bitev."),
            new Relic("L_03", "Kámen rovnováhy", "Každý tvůj útok generuje +1 Blok."),
            new Relic("L_04", "Prokletý pergamen", "Tvé karty stojí o 1 méně Many, ale začínáš boj s -5 HP."),
            new Relic("L_05", "Karmické zrcadlo", "Zdvojnásobuje efekt tvých změn Karmy."),
            new Relic("L_06", "Oko stínů", "Na začátku boje ubere nepřátelům 15 % jejich Max HP.")
        };

        // --- PRVNÍ STOVKA (001 - 100) ---
        private static List<Relic> GetFirstHundred() => new List<Relic> {
            new Relic("Relic_001", "Krvavá čutora", "Na konci každého boje vyléčí 2 HP."),
            new Relic("Relic_002", "Zlaté jablko", "Zvyšuje tvé Max HP o 10."),
            new Relic("Relic_003", "Trolí krev", "Na začátku každého boje vyléčí 5 HP."),
            new Relic("Relic_004", "Zázračný obvaz", "Pokaždé, když získáš blok z prázdné ruky, vyléčí 1 HP."),
            new Relic("Relic_005", "Upíří tesák", "Pokaždé, když zabiješ nepřítele, vyléčí 4 HP."),
            new Relic("Relic_006", "Srdce golema", "Na konci každého tahu vyléčí 1 HP, pokud jsi nedostal poškození."),
            new Relic("Relic_007", "Požehnaný amulet", "Léčivé karty tě léčí o 2 HP více."),
            new Relic("Relic_008", "Míza prastromu", "Zvyšuje Max HP o 2 za každý vyhraný boj."),
            new Relic("Relic_009", "Zkamenělé vejce", "Na začátku boje získáš 1 Regeneraci."),
            new Relic("Relic_010", "Dračí srdce", "Pokud klesneš pod 50% HP, vyléčíš 10 HP (jednou za boj)."),
            new Relic("Relic_011", "Kosti svatého", "Při odpočinku u táboráku se vyléčíš o 15 HP navíc."),
            new Relic("Relic_012", "Rybí šupina", "Na začátku každého tahu vyléčí 1 HP."),
            new Relic("Relic_013", "Maska šamana", "Pokaždé, když zahraješ kartu s cenou 0, vyléčí 1 HP."),
            new Relic("Relic_014", "Démonický roh", "Na začátku boje ztratíš 5 HP, ale získáš 3 Síly."),
            new Relic("Relic_015", "Andělské pírko", "Zabrání tvé první smrti v boji a vyléčí tě na 30% HP (1x za hru)."),
            new Relic("Relic_016", "Krunýř želvy", "Na začátku každého boje získáš 10 Bloku."),
            new Relic("Relic_017", "Mosazný štít", "Na začátku každého tahu získáš 2 Bloku."),
            new Relic("Relic_021", "Měsíční kámen", "Na začátku boje získáš 1 Obratnost."),
            new Relic("Relic_022", "Kostěná helma", "Karty dávající blok dávají o 2 bloku více."),
            new Relic("Relic_025", "Mithrilová spona", "Získáš 2 Obratnosti, ale nemůžeš se léčit v táboráku."),
            new Relic("Relic_026", "Aegis", "Na konci tahu si ponecháš 3 Bloku do dalšího tahu."),
            new Relic("Relic_030", "Ochranná runa", "Každá 5. zahraná karta dá 10 Bloku."),
            new Relic("Relic_031", "Brousek", "Tvůj první útok v boji udělí o 8 zranění více."),
            new Relic("Relic_032", "Krvavý náramek", "Každý útok udělí o 1 zranění více."),
            new Relic("Relic_033", "Tesák mantikory", "Na začátku každého boje získáš 1 Sílu."),
            new Relic("Relic_042", "Těžká kovadlina", "Všechny útoky dávají +2 poškození, ale -1 Obratnost."),
            new Relic("Relic_045", "Křišťálová lebka", "První útočná karta v tahu se zahraje dvakrát."),
            new Relic("Relic_046", "Zářící lucerna", "Na začátku boje +1 Manu v prvním tahu."),
            new Relic("Relic_048", "Prokletý rubín", "+1 Manu každý tah, ale na začátku boje -4 HP."),
            new Relic("Relic_061", "Taška triků", "Na začátku boje si lízneš 2 karty navíc."),
            new Relic("Relic_062", "Hadí oko", "Na začátku každého tahu lízneš o 1 kartu navíc."),
            new Relic("Relic_076", "Závaží spravedlnosti", "Na začátku boje Týmová Karma +3."),
            new Relic("Relic_093", "Členská karta", "Ceny v obchodech jsou o 30% nižší."),
            new Relic("Relic_100", "Zlaté rouno", "Za každých 100 Zlaťáků získáš na začátku boje 1 Blok.")
        };

        // --- DRUHÁ STOVKA (101 - 200) ---
        private static List<Relic> GetSecondHundred() => new List<Relic> {
            new Relic("Relic_101", "Zamrzlé oko", "První karta zahraná v boji stojí 0 Many."),
            new Relic("Relic_103", "Zlaté hodinky", "Pokud ukončíš tah s 3+ kartami v ruce, získáš 10 Bloku."),
            new Relic("Relic_106", "Temný grál", "Na začátku boje -10% HP, ale +3 Síla a +3 Obratnost."),
            new Relic("Relic_108", "Těžké okovy", "Jsi imunní vůči Oslabení (Weak)."),
            new Relic("Relic_111", "Kouzelná fazole", "Na začátku souboje s Bossem vyléčí tým do plna."),
            new Relic("Relic_117", "Zkamenělý strom", "Karty dávající Blok ho dávají o 30% více."),
            new Relic("Relic_118", "Otrávená dýka", "Nezablokované poškození aplikuje 1 Jed."),
            new Relic("Relic_119", "Žhavý uhlík", "Při obdržení poškození aplikuješ na útočníka 2 Hoření."),
            new Relic("Relic_121", "Koruna králů", "[Boss] +1 Manu každý tah, ale odměny nabízejí o 1 kartu méně."),
            new Relic("Relic_123", "Fylaktérium", "[Boss] +1 Manu každý tah, ale u táboráku se nelze léčit."),
            new Relic("Relic_124", "Prsten bolesti", "[Boss] +1 Manu každý tah, ale na začátku tahu ztratíš 2 HP."),
            new Relic("Relic_130", "Smlouva s peklem", "[Boss] Max HP x2, ale nemůžeš získat žádný Blok."),
            new Relic("Relic_132", "Oko bouře", "[Boss] Na začátku každého boje udělí všem nepřátelům 20 zranění."),
            new Relic("Relic_133", "Nekonečný lektvar", "[Boss] Po každém vyhraném boji vyléčí celý tým o 20 HP."),
            new Relic("Relic_139", "Krvavá maska", "[Boss] +1 Manu každý tah, ale zabití nepřítele tě zraní za 5 HP."),
            new Relic("Relic_142", "Toxická krev", "Jed na nepřátelích na konci jejich tahu neklesá."),
            new Relic("Relic_144", "Sopečný kámen", "Hoření dává o 50% větší poškození."),
            new Relic("Relic_145", "Srdce obra", "Síla (Strength) přidává +2 poškození za stack (místo 1)."),
            new Relic("Relic_146", "Rukavice zloděje", "Obratnost (Dexterity) přidává +2 bloku za stack (místo 1)."),
            new Relic("Relic_147", "Slzavý plyn", "Na začátku boje aplikuje 2 Oslabení na všechny nepřátele."),
            new Relic("Relic_148", "Ostré hroty", "Na začátku boje aplikuje 2 Zranitelnost na všechny nepřátele."),
            new Relic("Relic_155", "Kamenný kalendář", "V 7. tahu udělí 50 zranění všem nepřátelům."),
            new Relic("Relic_160", "Ledová stěna", "50% nevyužitého Bloku se přenáší do dalšího tahu."),
            new Relic("Relic_163", "Zkažená krev", "Tvé karty způsobují o 5 větší poškození, ale zraní i tebe za 1 HP."),
            new Relic("Relic_173", "Oko draka", "Tvé útoky ignorují Blok nepřítele."),
            new Relic("Relic_178", "Kamenný démon", "Při smrti ztratíš 50 Zlaťáků a oživíš se s 10 HP."),
            new Relic("Relic_185", "Stříbrná váha", "Každá karta s Karmou 0 ti po zahrání vrátí 1 Manu."),
            new Relic("Relic_193", "Lávový prsten", "Na začátku tvého tahu udělí 3 poškození všem nepřátelům."),
            new Relic("Relic_200", "Srdce Spire", "+1 Mana, +1 Karta a +1 HP na začátku každého tahu.")
        };

        // --- TŘETÍ STOVKA (201 - 300) ---
        private static List<Relic> GetThirdHundred() => new List<Relic> {
            new Relic("Relic_201", "Kámen šílenství", "Na začátku boje získáš 5 Síly, ale ztratíš 10 HP."),
            new Relic("Relic_202", "Zrcadlo zkázy", "Kdykoliv dostaneš poškození, útočník dostane to samé poškození zpět."),
            new Relic("Relic_203", "Křišťálové srdce", "Tvé Max HP je sníženo na 1, ale získáváš 50 Bloku každý tah."),
            new Relic("Relic_204", "Okovy bolesti", "Nemůžeš získat Obratnost, ale máš trvale +3 Síly."),
            new Relic("Relic_206", "Temný grimoár", "Pokaždé, když zahraješ kartu, ztratíš 1 HP a udělíš 3 zranění všem."),
            new Relic("Relic_207", "Andělská krev", "Všechna léčení v boji jsou o 100 % silnější."),
            new Relic("Relic_208", "Maska smrti", "Nepřátelé začínají boj s 5 Jedem a 5 Hořením."),
            new Relic("Relic_210", "Démonická smlouva", "Získáš +2 Manu každý tah, ale na konci tahu dostaneš 5 poškození."),
            new Relic("Relic_211", "Krev titánů", "Tvé Max HP se zvýší o 50."),
            new Relic("Relic_213", "Slza bohyně", "Na začátku boje získáš 3 Regenerace."),
            new Relic("Relic_215", "Oko cyklopa", "Každý tvůj útok ignoruje nepřátelský Blok."),
            new Relic("Relic_216", "Srdce lesa", "Pokud máš na konci tahu plné HP, získáš 1 Max HP."),
            new Relic("Relic_219", "Sluneční kámen", "Karty typu Útok ti dávají o 3 Poškození více."),
            new Relic("Relic_223", "Krev draka", "Hoření aplikované na nepřátele nikdy nevyprchá."),
            new Relic("Relic_224", "Jedovatá žláza", "Jed aplikovaný na nepřátele dává dvojnásobné poškození."),
            new Relic("Relic_229", "Svatý talisman", "Získáš +1 Manu každý tah."),
            new Relic("Relic_230", "Oltář oběti", "Získáš +2 Manu každý tah, ale první karta v ruce je každý tah zničena."),
            new Relic("Relic_232", "Zlatý prsten", "Pokaždé, když v obchodě koupíš kartu, dostaneš ji rovnou vylepšenou (+)."),
            new Relic("Relic_236", "Zlodějský paklíč", "Z elitních nepřátel získáš 2 relikvie místo jedné."),
            new Relic("Relic_237", "Lapač snů", "Kdykoliv bys měl dostat Oslabení (Weak), získáš místo toho 1 Sílu."),
            new Relic("Relic_241", "Mithrilová zbroj", "Jsi imunní vůči prvnímu poškození v každém tahu."),
            new Relic("Relic_244", "Kámen oživení", "Při smrti se oživíš s 1 HP a 99 Bloky (1x za hru)."),
            new Relic("Relic_246", "Rukavice nekromanta", "Každý mrtvý nepřítel ti dá do konce boje 1 Sílu."),
            new Relic("Relic_247", "Krvavá mince", "Za každé zranění, které utržíš do HP, získáš 2 Zlaťáky."),
            new Relic("Relic_251", "Misky osudu", "Pokud je Karma přesně 0, všechny tvé karty mají dvojnásobný efekt."),
            new Relic("Relic_252", "Andělská koruna", "Pokud je Karma nad +5, na konci tahu vyléčíš 5 HP."),
            new Relic("Relic_253", "Démonická koruna", "Pokud je Karma pod -5, na konci tahu udělíš 10 zranění všem."),
            new Relic("Relic_254", "Karmický zvon", "Pokaždé, když se Karma změní, získáš 2 Bloku."),
            new Relic("Relic_257", "Zlaté pírko", "Na začátku boje lízneš tolik karet, kolik je aktuální kladná Karma."),
            new Relic("Relic_259", "Prázdná nádoba", "Karmu nelze změnit. Zůstává na 0. Staty rostou o +2 Síly a Obratnosti."),
            new Relic("Relic_260", "Nekonečný cyklus", "Na konci boje se tvá Karma obrátí (z +5 na -5 a naopak)."),
            new Relic("Relic_264", "Těžká sekera", "Tvé útoky dávají +10 poškození, ale ztratíš 1 Manu každý tah."),
            new Relic("Relic_265", "Zlatý štít EX", "Každý tvůj Blok je navýšen o 25 %."),
            new Relic("Relic_268", "Železná játra", "Jsi imunní vůči Jedu (Poison)."),
            new Relic("Relic_269", "Azbestová kůže", "Jsi imunní vůči Hoření (Flame)."),
            new Relic("Relic_270", "Mithrilová kůže", "Jsi imunní vůči Oslabení a Zranitelnosti."),
            new Relic("Relic_277", "Slza draka", "Pokud jsi v týmu poslední naživu, tvé staty se ztrojnásobí."),
            new Relic("Relic_282", "Magická houba", "Na začátku každého tahu dostaneš náhodný pozitivní buff."),
            new Relic("Relic_283", "Cínový rytíř", "Na začátku boje přidá 20 Bloku všem spoluhráčům."),
            new Relic("Relic_284", "Zlatý meč", "Na začátku boje dá všem spoluhráčům 2 Síly."),
            new Relic("Relic_285", "Svatý kříž", "Na začátku boje dá všem spoluhráčům 2 Obratnosti."),
            new Relic("Relic_287", "Krvavá oběť týmu", "Vezme všem 5 HP, ale dá všem +1 Manu každý tah."),
            new Relic("Relic_290", "Záhadný lektvar", "Pokaždé, když použiješ táborák, celý tým získá náhodnou relikvii."),
            new Relic("Relic_294", "Dračí tesák", "Tvé útoky ignorují veškerý Blok."),
            new Relic("Relic_295", "Zkamenělý drak", "Nepřátelé v tomto Actu nemohou získávat Blok."),
            new Relic("Relic_296", "Křídla anděla", "Při smrti tě oživí s plným HP (1x za hru)."),
            new Relic("Relic_297", "Oko stvořitele", "Získáš +3 Manu, ale nepřátelé mají dvojnásobek HP."),
            new Relic("Relic_300", "Srdce Vesmíru", "Získáš vše: +1 Mana, +2 Síla, +2 Obratnost, +2 Regen a +2 karty každý tah.")
        };
    }
}