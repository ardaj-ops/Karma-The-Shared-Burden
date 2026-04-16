using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    public static class RelicManager
    {
        // Pomocná funkce pro zjištění, jestli je hráč "první" v místnosti 
        // (aby se týmové efekty jako Karma nebo zranění všem nepřátelům neaplikovaly 5x, když hraje 5 hráčů)
        private static bool IsFirstPlayer(Player p, GameRoom room)
        {
            return room.Players.IndexOf(p) == 0;
        }

        // Pomocná funkce pro zjištění, zda jde o boj s Bossem
        private static bool IsBossNode(GameRoom room)
        {
            var node = room.Map.FirstOrDefault(n => n.Id == room.CurrentNodeId);
            return node != null && node.Type == NodeType.Boss;
        }

        // ==========================================
        // 1. EFEKTY NA ZAČÁTKU BOJE
        // ==========================================
        public static void ApplyCombatStartRelics(Player p, GameRoom room, List<ActiveEnemy> enemies)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            bool isFirst = IsFirstPlayer(p, room);

            // --- HERO RELIKVIE (Startovní) ---
            if (rIds.Contains("Hero_01")) { p.Block += 5; if (isFirst) room.CurrentKarma += 2; }
            if (rIds.Contains("Hero_02")) { p.Hp -= 2; p.Mana += 1; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Hero_03")) p.AddEffect(EffectType.Dexterity, 1);
            if (rIds.Contains("Hero_04")) p.AddEffect(EffectType.Strength, 1);
            if (rIds.Contains("Hero_05")) p.AddEffect(EffectType.Regen, 1);
            if (rIds.Contains("Hero_06") && isFirst) foreach (var e in enemies) e.AddEffect(EffectType.Weak, 1);
            if (rIds.Contains("Hero_07")) p.DrawCards(1);
            if (rIds.Contains("Hero_08") && isFirst) foreach (var e in enemies) e.AddEffect(EffectType.Flame, 1);

            // --- LEGENDÁRNÍ (L) RELIKVIE ---
            if (rIds.Contains("L_04")) { p.Hp -= 5; p.Mana += 1; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("L_06") && isFirst) foreach (var e in enemies) e.Hp = Math.Max(1, (int)(e.Hp * 0.85));

            // --- BĚŽNÉ RELIKVIE (1 - 100) ---
            if (rIds.Contains("Relic_003")) p.Hp = Math.Min(p.MaxHp, p.Hp + 5);
            if (rIds.Contains("Relic_014")) { p.Hp -= 5; p.AddEffect(EffectType.Strength, 3); if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Relic_016")) p.Block += 10;
            if (rIds.Contains("Relic_042")) p.AddEffect(EffectType.Dexterity, -1);
            if (rIds.Contains("Relic_061")) p.DrawCards(2);
            if (rIds.Contains("Relic_076") && isFirst) room.CurrentKarma += 3;

            // --- SILNÉ A BOSS RELIKVIE (101 - 200) ---
            if (rIds.Contains("Relic_106")) { p.Hp -= Math.Max(1, p.MaxHp / 10); p.AddEffect(EffectType.Strength, 3); p.AddEffect(EffectType.Dexterity, 3); if(p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Relic_111") && IsBossNode(room)) p.Hp = p.MaxHp;
            if (rIds.Contains("Relic_130")) p.MaxHp += 20; // Simulace smlouvy s peklem (dá HP, ale blok je 0 - viz ModifyBlock)
            if (rIds.Contains("Relic_147") && isFirst) foreach (var e in enemies) e.AddEffect(EffectType.Weak, 2);
            if (rIds.Contains("Relic_148") && isFirst) foreach (var e in enemies) e.AddEffect(EffectType.Vulnerable, 2);

            // --- ŠÍLENÉ A ENDGAME RELIKVIE (201 - 300) ---
            if (rIds.Contains("Relic_203")) { p.Hp = 1; p.Block += 100; } // Křišťálové srdce (1 HP, masivní blok)
            if (rIds.Contains("Relic_241")) p.Block += 20; // Simulace imunity na první ránu
            if (rIds.Contains("Relic_285")) p.AddEffect(EffectType.Dexterity, 2);
        }

        // ==========================================
        // 2. EFEKTY NA ZAČÁTKU BOJE (Dříve "Na začátku tahu")
        // Ve 3D bojích už nemáme tahy, takže relikvie dávající
        // věci "každý tah" zde dají masivní jednorázový bonus do startu!
        // ==========================================
        public static void ApplyTurnStartRelics(Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            bool isFirst = IsFirstPlayer(p, room);

            if (rIds.Contains("Relic_010") && p.Hp < (p.MaxHp / 2)) p.Hp = Math.Min(p.MaxHp, p.Hp + 10);
            if (rIds.Contains("Relic_045")) { p.Mana += 1; p.DrawCards(1); }
            if (rIds.Contains("Relic_062")) p.DrawCards(3); // Hadí oko - rovnou ti naláduje ruku
            if (rIds.Contains("Relic_101")) p.Mana += 2;
            if (rIds.Contains("Relic_119") && isFirst) foreach (var e in room.ActiveEnemies) e.AddEffect(EffectType.Flame, 2);
            if (rIds.Contains("Relic_121")) p.Mana += 3;
            if (rIds.Contains("Relic_142") && isFirst) foreach (var e in room.ActiveEnemies) e.AddEffect(EffectType.Poison, 3);
            if (rIds.Contains("Relic_160")) p.Block += 15;
            if (rIds.Contains("Relic_200")) { p.Mana += 3; p.DrawCards(2); p.AddEffect(EffectType.Regen, 1); }
            if (rIds.Contains("Relic_202") && isFirst) foreach (var e in room.ActiveEnemies) e.AddEffect(EffectType.Flame, 3);
            if (rIds.Contains("Relic_230")) { p.Mana += 5; if (p.Hand.Count > 0) p.Hand.RemoveAt(0); }
            if (rIds.Contains("Relic_253") && room.CurrentKarma < -5 && isFirst) foreach (var e in room.ActiveEnemies) e.Hp -= 30;
            if (rIds.Contains("Relic_282")) { p.AddEffect(EffectType.Strength, 1); p.AddEffect(EffectType.Dexterity, 1); p.AddEffect(EffectType.Regen, 1); }
            if (rIds.Contains("Relic_300")) { p.Mana += 3; p.AddEffect(EffectType.Strength, 3); p.AddEffect(EffectType.Dexterity, 3); p.AddEffect(EffectType.Regen, 3); p.DrawCards(3); }
        }

        // ==========================================
        // 3. ÚPRAVA POŠKOZENÍ (ÚTOK)
        // ==========================================
        public static int ModifyDamage(int baseDamage, Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            int dmg = baseDamage;

            // Zrcadlo karmy (L_05)
            if (rIds.Contains("L_05"))
            {
                if (room.CurrentKarma >= 10) dmg = (int)(dmg * 0.7);
                if (room.CurrentKarma <= -10) dmg = (int)(dmg * 1.5);
            }

            if (rIds.Contains("Relic_031") && p.CardsPlayedThisTurn == 0) dmg += 8; // Brousek - obrovská rána na první útok
            if (rIds.Contains("Relic_042")) dmg += 2;
            if (rIds.Contains("Relic_173")) dmg += 5; // Simulace "Ignoruje Blok" hrubým bonusem
            if (rIds.Contains("Relic_219")) dmg += 3;
            if (rIds.Contains("Relic_251") && room.CurrentKarma == 0) dmg *= 2; // Misky osudu

            return dmg;
        }

        // ==========================================
        // 4. ÚPRAVA BLOKU (OBRANA)
        // ==========================================
        public static int ModifyBlock(int baseBlock, Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            int blk = baseBlock;

            // Zlatý štít a Zkamenělý strom
            if (rIds.Contains("Relic_117")) blk = (int)(blk * 1.3);
            if (rIds.Contains("Relic_265")) blk = (int)(blk * 1.25);
            
            // Smlouva s peklem
            if (rIds.Contains("Relic_130")) blk = 0; 

            return blk;
        }

        // ==========================================
        // 5. KONEC BOJE A TÁBORÁK
        // ==========================================
        public static void ApplyCombatEndRelics(Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            
            if (rIds.Contains("Relic_001")) p.Hp = Math.Min(p.MaxHp, p.Hp + 2);
            if (rIds.Contains("Relic_008")) p.MaxHp += 2;
            if (rIds.Contains("Relic_133")) p.Hp = Math.Min(p.MaxHp, p.Hp + 20);
            if (rIds.Contains("L_02")) p.Gold += 15; // Zlatý skarabeus bonus
            if (rIds.Contains("Relic_207")) p.Hp = Math.Min(p.MaxHp, p.Hp + 10); // Andělská krev simulace
            if (rIds.Contains("Relic_211")) p.MaxHp += 5;
            if (rIds.Contains("Relic_247")) p.Gold += Math.Max(0, p.MaxHp - p.Hp); // Krvavá mince
        }

        public static int ApplyCampfireRelics(Player p, GameRoom room, int baseHeal)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            
            // Relikvie blokující léčení
            if (rIds.Contains("Relic_123") || rIds.Contains("Relic_057")) return 0;
            
            // Bonusové léčení
            int finalHeal = baseHeal;
            if (rIds.Contains("Relic_011")) finalHeal += 15;
            if (rIds.Contains("Relic_207")) finalHeal *= 2; // Andělská krev - dvojité léčení u ohně
            
            return finalHeal;
        }

        // ==========================================
        // 6. DYNAMICKÉ UDÁLOSTI V BOJI
        // ==========================================
        public static void OnEnemyKilled(Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            if (rIds.Contains("Relic_005")) p.Hp = Math.Min(p.MaxHp, p.Hp + 4);
            if (rIds.Contains("Relic_015") && p.Hp < 5) p.Hp += (int)(p.MaxHp * 0.3); // Andělské pírko (záchrana)
        }

        public static void ApplyCardPlayedRelics(Player p, GameRoom room, CardTemplate card)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            
            if (rIds.Contains("L_03") && card.Damage > 0) p.Block += 1;
            if (rIds.Contains("Relic_185") && card.KarmaShift == 0) p.Mana += 1;
            if (rIds.Contains("Relic_254") && card.KarmaShift != 0) p.Block += 2;
        }
    }
}