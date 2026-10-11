using UnityEngine;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public static partial class ActorExtensions
    {
        // ============================================================
        // ★ 战斗统计 ★
        // ============================================================
        private const string TOTAL_KILLS_KEY = DataKeys.TOTAL_KILLS;
        private const string LAST_ATTACK_TIME_KEY = DataKeys.LAST_ATTACK_TIME;

        public static int GetTotalKills(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0;
            actor.data.get(TOTAL_KILLS_KEY, out int val, 0);
            return val;
        }

        public static void AddKill(this Actor actor)
        {
            if (actor == null) return;
            int current = actor.GetTotalKills();
            actor.data.set(TOTAL_KILLS_KEY, current + 1);
        }

        public static float GetLastAttackTime(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0f;
            actor.data.get(LAST_ATTACK_TIME_KEY, out float val, 0f);
            return val;
        }

        public static void SetLastAttackTime(this Actor actor, float val)
        {
            if (actor == null) return;
            actor.data.set(LAST_ATTACK_TIME_KEY, val);
        }

        // ============================================================
        // ★ 暴击率 ★
        // ============================================================
        public static float GetCritChance(this Actor actor)
        {
            if (actor == null || !actor.isAlive()) return 0f;
            
            float crit = actor.stats["critical_chance"];

            // 心关加成
            if (actor.IsHeartBroken(3)) crit += 0.10f;
            if (actor.IsHeartBroken(5)) crit += 0.15f;

            // 境界加成
            int realm = actor.GetRealmLevel();
            switch (realm)
            {
                case 2: crit += 0.05f; break;
                case 3: crit += 0.10f; break;
                case 4: crit += 0.15f; break;
                case 5: crit += 0.25f; break;
            }

            // 前路断绝惩罚
            if (actor.hasTrait(TraitIds.Trait_NoWayForward))
                crit *= 0.5f;

            return Mathf.Min(crit, 0.80f);
        }

        // ============================================================
        // ★ 暴击伤害 ★
        // ============================================================
        public static float GetCritDamageMultiplier(this Actor actor)
        {
            if (actor == null || !actor.isAlive()) return 1.5f;
            
            float critDamage = actor.stats["CriticalDamage"];
            if (critDamage <= 0f) critDamage = 1.5f;

            // 心关加成
            if (actor.IsHeartBroken(3)) critDamage += 0.5f;
            if (actor.IsHeartBroken(5)) critDamage += 0.5f;

            // 前路断绝惩罚
            if (actor.hasTrait(TraitIds.Trait_NoWayForward))
                critDamage *= 0.5f;

            return Mathf.Max(critDamage, 1.0f);
        }

        // ============================================================
        // ★ 护盾系统 ★
        // ============================================================
        private const string SHIELD_HP_KEY = DataKeys.SHIELD_HP;
        private const string SHIELD_MAX_KEY = DataKeys.SHIELD_MAX;

        public static float GetShieldHP(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0f;
            actor.data.get(SHIELD_HP_KEY, out float val, 0f);
            return Mathf.Max(0, val);
        }

        public static void SetShieldHP(this Actor actor, float val)
        {
            if (actor == null) return;
            actor.data.set(SHIELD_HP_KEY, Mathf.Max(0, val));
        }

        public static float GetShieldMax(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0f;
            actor.data.get(SHIELD_MAX_KEY, out float val, 0f);
            return Mathf.Max(0, val);
        }

        public static void SetShieldMax(this Actor actor, float val)
        {
            if (actor == null) return;
            actor.data.set(SHIELD_MAX_KEY, Mathf.Max(0, val));
        }

        // ============================================================
        // ★ 护盾恢复（由 ShieldSystem 调用） ★
        // ============================================================
        public static void AddShieldHP(this Actor actor, float amount)
        {
            if (actor == null || amount <= 0) return;
            float current = actor.GetShieldHP();
            float max = actor.GetShieldMax();
            if (max <= 0) return;
            actor.SetShieldHP(Mathf.Min(current + amount, max));
        }

        public static float GetShieldPercent(this Actor actor)
        {
            float max = actor.GetShieldMax();
            if (max <= 0) return 0f;
            return actor.GetShieldHP() / max;
        }
    }
}