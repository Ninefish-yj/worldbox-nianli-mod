using System;
using UnityEngine;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public static partial class ActorExtensions
    {
        // ============================================================
        // ★ 内部安全辅助方法（替代 NianLiSafeHelper） ★
        // ============================================================
        private static float SafeClamp(float value, float min = 0f, float max = float.MaxValue)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return min;
            return Mathf.Clamp(value, min, max);
        }

        private static int SafeClamp(int value, int min = 0, int max = int.MaxValue)
        {
            return Mathf.Clamp(value, min, max);
        }

        // ============================================================
        // ★ 境界与念力核心方法 ★
        // ============================================================

        private const string REALM_LEVEL_KEY = "nianli.RealmLevel";
        private const string NIAN_LI_KEY = "nianli.NianLi";
        private const string CURRENT_MAX_NIAN_LI_KEY = "nianli.CurrentMaxNianLi";
        private const string LAST_GROWTH_YEAR_KEY = "nianli.LastGrowthYear";

        public static int GetRealmLevel(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0;

            actor.data.get(REALM_LEVEL_KEY, out int val, 0);
            if (val > 0) return SafeClamp(val, 0, 5);

            // 从特质推断
            for (int i = 5; i >= 1; i--)
            {
                if (actor.hasTrait($"Realm_{i}"))
                {
                    actor.data.set(REALM_LEVEL_KEY, i);
                    return i;
                }
            }
            return 0;
        }

        public static void SetRealmLevel(this Actor actor, int level)
        {
            if (actor == null || actor.data == null) return;
            int clamped = SafeClamp(level, 0, 5);
            actor.data.set(REALM_LEVEL_KEY, clamped);
        }

        public static float GetNianLi(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0f;
            actor.data.get(NIAN_LI_KEY, out float val, 0f);
            return SafeClamp(val);
        }

        public static void SetNianLi(this Actor actor, float val)
        {
            if (actor == null || actor.data == null) return;
            float safeVal = float.IsNaN(val) || float.IsInfinity(val) ? 0f : val;
            float max = actor.GetCurrentMaxNianLi();
            if (max <= 0) max = actor.GetRealmMaxNianLi();
            if (max <= 0) max = 1000f;
            actor.data.set(NIAN_LI_KEY, Mathf.Clamp(safeVal, 0f, max));
        }

        public static void ChangeNianLi(this Actor actor, float delta)
        {
            if (actor == null || actor.data == null) return;
            if (float.IsNaN(delta) || float.IsInfinity(delta)) return;
            float current = actor.GetNianLi();
            float max = actor.GetCurrentMaxNianLi();
            if (max <= 0) max = actor.GetRealmMaxNianLi();
            if (max <= 0) max = 1000f;
            actor.SetNianLi(Mathf.Clamp(current + delta, 0f, max));
        }

        public static float GetRealmMaxNianLi(this Actor actor)
        {
            if (actor == null) return 0f;
            int realm = actor.GetRealmLevel();
            if (realm < 0 || realm >= ConfigCache.RealmMaxNianLi.Length)
                return float.MaxValue;
            return ConfigCache.RealmMaxNianLi[realm];
        }

        public static float GetCurrentMaxNianLi(this Actor actor)
        {
            if (actor == null || actor.data == null) return 0f;
            actor.data.get(CURRENT_MAX_NIAN_LI_KEY, out float val, 0f);
            float clamped = SafeClamp(val);
            float realmMax = actor.GetRealmMaxNianLi();
            return clamped > realmMax ? realmMax : clamped;
        }

        public static void SetCurrentMaxNianLi(this Actor actor, float val)
        {
            if (actor == null || actor.data == null) return;
            float safeVal = float.IsNaN(val) || float.IsInfinity(val) ? 0f : val;
            float realmMax = actor.GetRealmMaxNianLi();
            actor.data.set(CURRENT_MAX_NIAN_LI_KEY, Mathf.Clamp(safeVal, 0f, realmMax));
        }

        public static int GetLastGrowthYear(this Actor actor)
        {
            if (actor == null || actor.data == null) return -1;
            actor.data.get(LAST_GROWTH_YEAR_KEY, out int val, -1);
            return val;
        }

        public static void SetLastGrowthYear(this Actor actor, int year)
        {
            if (actor == null || actor.data == null) return;
            actor.data.set(LAST_GROWTH_YEAR_KEY, year);
        }

        public static bool HasAllRequiredHearts(this Actor actor, int realmLevel)
        {
            if (actor == null) return false;
            switch (realmLevel)
            {
                case 1: return actor.IsHeartBroken(0) && actor.IsHeartBroken(1) && actor.IsHeartBroken(2);
                case 2: return actor.IsHeartBroken(3);
                case 3: return true;
                case 4: return actor.IsHeartBroken(4);
                case 5: return actor.IsHeartBroken(5);
                default: return false;
            }
        }

        public static void FixAllOverMaxNianLi()
        {
            if (World.world?.units == null) return;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || actor.data == null) continue;

                int realm = actor.GetRealmLevel();
                if (realm > 0 && !actor.hasTrait($"Realm_{realm}"))
                {
                    actor.addTrait($"Realm_{realm}");
                }

                float realmMax = actor.GetRealmMaxNianLi();
                float currentMax = actor.GetCurrentMaxNianLi();
                if (currentMax > realmMax)
                {
                    actor.SetCurrentMaxNianLi(realmMax);
                    float currentNianLi = actor.GetNianLi();
                    if (currentNianLi > realmMax)
                        actor.SetNianLi(realmMax);
                }
            }
        }
    }
}