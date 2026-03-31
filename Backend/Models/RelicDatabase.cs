using System;
using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class RelicDatabase
    {
        public static List<Relic> LootRelics = new List<Relic>
        {
            new Relic("L_01", "Krvavý rubín", "Zvyšuje tvé Max HP o 10."),
            new Relic("L_02", "Zlatý skarabeus", "Získáš o 30 % více zlaťáků z bitev."),
            new Relic("L_03", "Kámen rovnováhy", "Každý tvůj útok generuje +1 Blok."),
            new Relic("L_04", "Prokletý pergamen", "Tvé karty stojí o 1 méně Many, ale začínáš boj s -5 HP."),
            new Relic("L_05", "Karmické zrcadlo", "Zdvojnásobuje efekt tvých změn Karmy."),
            new Relic("L_06", "Oko stínů", "Na začátku boje ubere nepřátelům 15 % jejich Max HP.")
        };

        public static Relic GetRandomRelic()
        {
            Random rng = new Random();
            return LootRelics[rng.Next(LootRelics.Count)];
        }
    }
}