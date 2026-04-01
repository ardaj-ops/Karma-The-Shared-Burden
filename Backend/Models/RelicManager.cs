using System.Collections.Generic;
using System.Linq;
using System;
using RoguelikeCardGame.Hubs;
namespace RoguelikeCardGame.Models
{
    public static class RelicManager
    {
        // ==========================================
        // 1. EFEKTY NA ZAČÁTKU BOJE
        // ==========================================
        public static void ApplyCombatStartRelics(Player p, GameRoom room, List<ActiveEnemy> enemies)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();

            // --- HERO RELIKVIE (Startovní) ---
            if (rIds.Contains("Hero_01")) { p.Block += 5; if (room.Players.IndexOf(p) == 0) room.CurrentKarma += 2; }
            if (rIds.Contains("Hero_02")) { p.Hp -= 2; p.Mana += 1; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Hero_03")) p.AddEffect(EffectType.Dexterity, 1);
            if (rIds.Contains("Hero_04")) p.AddEffect(EffectType.Strength, 1);
            if (rIds.Contains("Hero_05")) p.AddEffect(EffectType.Regen, 1);
            if (rIds.Contains("Hero_06")) foreach (var e in enemies) e.AddEffect(EffectType.Weak, 1);
            if (rIds.Contains("Hero_07")) p.DrawCards(1);
            if (rIds.Contains("Hero_08")) foreach (var e in enemies) e.AddEffect(EffectType.Flame, 1);

            // --- LEGENDÁRNÍ (L) RELIKVIE ---
            if (rIds.Contains("L_01")) p.MaxHp += 10; // Pozn: MaxHP se obvykle mění v ClaimReward, ale pro jistotu
            if (rIds.Contains("L_04")) { p.Hp -= 5; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("L_06")) foreach (var e in enemies) e.Hp = (int)(e.Hp * 0.85);

            // Spuštění logiky rozdělené po stovkách
            ApplyFirstHundredStart(p, room, enemies, rIds);
            ApplySecondHundredStart(p, room, enemies, rIds);
            ApplyThirdHundredStart(p, room, enemies, rIds);
        }

        private static void ApplyFirstHundredStart(Player p, GameRoom room, List<ActiveEnemy> enemies, HashSet<string> rIds)
        {
            if (rIds.Contains("Relic_003")) p.Hp = Math.Min(p.MaxHp, p.Hp + 5);
            if (rIds.Contains("Relic_009")) p.AddEffect(EffectType.Regen, 1);
            if (rIds.Contains("Relic_016")) p.Block += 10;
            if (rIds.Contains("Relic_021")) p.AddEffect(EffectType.Dexterity, 1);
            if (rIds.Contains("Relic_023")) p.Block += 50; // Přízračný plášť
            if (rIds.Contains("Relic_033")) p.AddEffect(EffectType.Strength, 1);
            if (rIds.Contains("Relic_046")) p.Mana += 1;
            if (rIds.Contains("Relic_061")) p.DrawCards(2);
            if (rIds.Contains("Relic_076")) room.CurrentKarma += 3;
            if (rIds.Contains("Relic_077")) room.CurrentKarma -= 3;
            if (rIds.Contains("Relic_081")) foreach (var e in enemies.Take(1)) e.AddEffect(EffectType.Poison, 3);
            if (rIds.Contains("Relic_082")) foreach (var e in enemies.Take(1)) e.AddEffect(EffectType.Flame, 3);
        }

        private static void ApplySecondHundredStart(Player p, GameRoom room, List<ActiveEnemy> enemies, HashSet<string> rIds)
        {
            if (rIds.Contains("Relic_106")) { p.Hp -= (p.MaxHp / 10); p.AddEffect(EffectType.Strength, 3); p.AddEffect(EffectType.Dexterity, 3); }
            if (rIds.Contains("Relic_132")) foreach (var e in enemies) e.Hp -= 20;
            if (rIds.Contains("Relic_147")) foreach (var e in enemies) e.AddEffect(EffectType.Weak, 2);
            if (rIds.Contains("Relic_148")) foreach (var e in enemies) e.AddEffect(EffectType.Vulnerable, 2);
            if (rIds.Contains("Relic_171")) { room.CurrentKarma -= 5; p.AddEffect(EffectType.Strength, 3); }
            if (rIds.Contains("Relic_172")) { room.CurrentKarma += 5; p.AddEffect(EffectType.Dexterity, 3); }
        }

        private static void ApplyThirdHundredStart(Player p, GameRoom room, List<ActiveEnemy> enemies, HashSet<string> rIds)
        {
            if (rIds.Contains("Relic_201")) { p.Hp -= 10; p.AddEffect(EffectType.Strength, 5); }
            if (rIds.Contains("Relic_208")) foreach (var e in enemies) { e.AddEffect(EffectType.Poison, 5); e.AddEffect(EffectType.Flame, 5); }
            if (rIds.Contains("Relic_213")) p.AddEffect(EffectType.Regen, 3);
            if (rIds.Contains("Relic_257") && room.CurrentKarma > 0) p.DrawCards(Math.Min(room.CurrentKarma, 5));
            if (rIds.Contains("Relic_283")) p.Block += 20;
            if (rIds.Contains("Relic_284")) p.AddEffect(EffectType.Strength, 2);
            if (rIds.Contains("Relic_285")) p.AddEffect(EffectType.Dexterity, 2);
            if (rIds.Contains("Relic_286")) p.AddEffect(EffectType.Regen, 2);
        }

        // ==========================================
        // 2. EFEKTY NA ZAČÁTKU TAHU
        // ==========================================
        public static void ApplyTurnStartRelics(Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();

            // Mana generace (Všech 300)
            string[] manaRelics = { 
                "Relic_048", "Relic_049", "Relic_052", "Relic_053", "Relic_054", "Relic_056", 
                "Relic_057", "Relic_059", "Relic_121", "Relic_122", "Relic_123", "Relic_124", 
                "Relic_125", "Relic_126", "Relic_127", "Relic_131", "Relic_136", "Relic_139", 
                "Relic_210", "Relic_229", "Relic_230", "Relic_287", "Relic_297", "Relic_300" 
            };
            if (manaRelics.Any(rIds.Contains)) p.Mana += 1;
            if (rIds.Contains("Relic_055") && p.Hp == p.MaxHp) p.Mana += 1;
            if (rIds.Contains("Relic_138") && p.Hp == p.MaxHp) p.Mana += 2;

            // Pasivní efekty tahů
            if (rIds.Contains("Relic_012") || rIds.Contains("Relic_300")) p.Hp = Math.Min(p.MaxHp, p.Hp + 1);
            if (rIds.Contains("Relic_017")) p.Block += 2;
            if (rIds.Contains("Relic_271")) p.Block += 5;
            if (rIds.Contains("Relic_062") || rIds.Contains("Relic_200")) p.DrawCards(1);
            if (rIds.Contains("Relic_124")) { p.Hp -= 2; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Relic_210")) { p.Hp -= 5; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Relic_230") && p.Hand.Count > 0) p.Hand.RemoveAt(0);
            if (rIds.Contains("Relic_282")) {
                string[] buffs = { EffectType.Strength, EffectType.Dexterity, EffectType.Regen };
                p.AddEffect(buffs[new Random().Next(buffs.Length)], 1);
            }
        }

        // ==========================================
        // 3. ÚPRAVA POŠKOZENÍ (ÚTOK)
        // ==========================================
        public static int ModifyDamage(int baseDamage, Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            int dmg = baseDamage;

            // Flat bonusy
            if (rIds.Contains("Relic_032")) dmg += 1;
            if (rIds.Contains("Relic_042")) dmg += 2;
            if (rIds.Contains("Relic_163")) { dmg += 5; p.Hp -= 1; }
            if (rIds.Contains("Relic_219")) dmg += 3;
            if (rIds.Contains("Relic_264")) { dmg += 10; p.Mana = Math.Max(0, p.Mana - 1); }
            if (rIds.Contains("Relic_087")) dmg += 2;

            // Zrcadlo karmy (L_05)
            if (rIds.Contains("L_05"))
            {
                if (room.CurrentKarma >= 10) dmg = (int)(dmg * 0.7);
                if (room.CurrentKarma <= -10) dmg = (int)(dmg * 1.5);
            }

            // Kostky osudu a Alchymista
            if (rIds.Contains("Relic_137") || rIds.Contains("Relic_291"))
            {
                dmg = new Random().Next(0, 2) == 0 ? (int)(dmg * 1.5) : (int)(dmg * 0.5);
            }

            return dmg;
        }

        // ==========================================
        // 4. ÚPRAVA BLOKU (OBRANA)
        // ==========================================
        public static int ModifyBlock(int baseBlock, Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            int blk = baseBlock;

            if (rIds.Contains("Relic_022") || rIds.Contains("L_03")) blk += 2;
            if (rIds.Contains("Relic_117")) blk = (int)(blk * 1.3);
            if (rIds.Contains("Relic_218")) blk += 4;
            if (rIds.Contains("Relic_265")) blk = (int)(blk * 1.25);
            
            // Smlouva s peklem
            if (rIds.Contains("Relic_130")) blk = 0; 

            return blk;
        }

        // ==========================================
        // 5. KONEC BOJE, TÁBORÁK A SMRT
        // ==========================================
        public static void ApplyCombatEndRelics(Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            if (rIds.Contains("Relic_001")) p.Hp = Math.Min(p.MaxHp, p.Hp + 2);
            if (rIds.Contains("Relic_008")) p.MaxHp += 2;
            if (rIds.Contains("Relic_133")) p.Hp = Math.Min(p.MaxHp, p.Hp + 20);
            if (rIds.Contains("Relic_187")) p.Gold += (p.MaxHp - p.Hp);
            if (rIds.Contains("Relic_260")) room.CurrentKarma *= -1;
        }

        public static int ApplyCampfireRelics(Player p, GameRoom room, int baseHeal)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            if (rIds.Contains("Relic_011")) return baseHeal + 15;
            if (rIds.Contains("Relic_123") || rIds.Contains("Relic_057")) return 0;
            if (rIds.Contains("Relic_184")) p.MaxHp += 5;
            if (rIds.Contains("Relic_214")) p.Gold += 100;
            return baseHeal;
        }

        public static void OnEnemyKilled(Player p, GameRoom room)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            if (rIds.Contains("Relic_005")) p.Hp = Math.Min(p.MaxHp, p.Hp + 4);
            if (rIds.Contains("Relic_246") || rIds.Contains("Relic_276")) p.AddEffect(EffectType.Strength, 1);
            if (rIds.Contains("Relic_139")) { p.Hp -= 5; if (p.Hp < 1) p.Hp = 1; }
        }

        public static void ApplyCardPlayedRelics(Player p, GameRoom room, CardTemplate card)
        {
            var rIds = room.TeamRelics.Select(r => r.Id).ToHashSet();
            if (rIds.Contains("Relic_065") && card.Cost == 0) p.DrawCards(1);
            if (rIds.Contains("Relic_206")) { p.Hp -= 1; if (p.Hp < 1) p.Hp = 1; }
            if (rIds.Contains("Relic_254")) p.Block += 2;
        }
    }
}