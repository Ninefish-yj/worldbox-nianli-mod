using UnityEngine;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public static partial class ActorExtensions
    {
        private const string ASCENSION_READY_KEY = DataKeys.ASCENSION_READY;
        private const string ASCENSION_STEP_KEY = DataKeys.ASCENSION_STEP;
        private const string MINOR_ASCENSION_COMPLETE_KEY = DataKeys.MINOR_ASCENSION_COMPLETE;

        // ---- 飞升状态 ----
        public static bool IsAscensionReady(this Actor actor)
        {
            actor.data.get(ASCENSION_READY_KEY, out bool val, false);
            return val;
        }
        public static void SetAscensionReady(this Actor actor, bool val)
        {
            actor.data.set(ASCENSION_READY_KEY, val);
        }

        public static int GetAscensionStep(this Actor actor)
        {
            actor.data.get(ASCENSION_STEP_KEY, out int val, 0);
            return val;
        }
        public static void SetAscensionStep(this Actor actor, int val)
        {
            actor.data.set(ASCENSION_STEP_KEY, Mathf.Clamp(val, 0, 9));
        }
        public static void AddAscensionStep(this Actor actor)
        {
            int current = actor.GetAscensionStep();
            actor.SetAscensionStep(current + 1);
        }

        public static bool IsMinorAscensionComplete(this Actor actor)
        {
            actor.data.get(MINOR_ASCENSION_COMPLETE_KEY, out bool val, false);
            return val;
        }
        public static void SetMinorAscensionComplete(this Actor actor, bool val)
        {
            actor.data.set(MINOR_ASCENSION_COMPLETE_KEY, val);
        }

        // ---- 飞升类型 ----
        public static AscensionType GetAscensionType(this Actor actor)
        {
            if (actor.GetRealmLevel() == 4 && !actor.IsMinorAscensionComplete())
                return AscensionType.Minor;
            if (actor.GetRealmLevel() == 5 && actor.IsMinorAscensionComplete())
                return AscensionType.Major;
            return AscensionType.None;
        }

        // ---- 小阳劫条件 ----
        public static bool CanStartMinorAscension(this Actor actor)
        {
            if (actor.GetRealmLevel() != 4) return false;
            if (actor.IsMinorAscensionComplete()) return false;
            if (!actor.IsHeartBroken(5)) return false;
            if (actor.GetNianLi() < 1000000f) return false;
            if (!actor.HasAllLaws()) return false;
            if (!actor.hasTrait(TraitIds.SmallWorld_4)) return false;
            return true;
        }

        // ============================================================
        // ★★★ 大阳劫条件（史诗级） ★★★
        // ============================================================

        public static bool CanStartMajorAscension(this Actor actor)
        {
            if (actor == null) return false;

            // ---- 基础条件 ----
            if (actor.GetRealmLevel() != 5) return false;
            if (!actor.IsMinorAscensionComplete()) return false;
            if (!actor.IsHeartBroken(5)) return false;

            // ---- 六大心关全部突破 ----
            for (int i = 0; i < 6; i++)
            {
                if (!actor.IsHeartBroken(i)) return false;
            }

            // ---- 四大法则全部集齐 ----
            if (!actor.HasAllLaws()) return false;

            // ---- 小世界四重完整 ----
            if (!actor.hasTrait(TraitIds.SmallWorld_4)) return false;

            // ---- 顿悟 ≥ 5 ----
            if (actor.GetInsightLevel() < 5) return false;

            // ---- 念力 ≥ 5000万 ----
            if (actor.GetNianLi() < 50000000f) return false;

            // ---- 业力 ≤ 10（清净之身） ----
            if (KarmaSystem.GetKarma(actor) > 10f) return false;

            // ---- 检查冷却（失败后50年冷却） ----
            actor.data.get(DataKeys.LAST_ASCENSION_TIMER, out float lastFailTime, -100f);
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            if (currentYear - lastFailTime < 50f) return false;

            // ---- 寿元消耗检查（必须≥500年剩余寿命） ----
            float lifespan = actor.stats["lifespan"];
            float age = actor.getAge();
            if (lifespan - age < 500f) return false;

            return true;
        }

        // ---- 获取大阳劫阻挡原因 ----
        public static string GetMajorAscensionBlockReason(this Actor actor)
        {
            if (actor == null) return "未知错误";

            if (actor.GetRealmLevel() != 5) return "需达到托举境";
            if (!actor.IsMinorAscensionComplete()) return "需先完成小阳劫";
            if (!actor.IsHeartBroken(5)) return "需突破真我关";

            // 六大心关
            string[] heartNames = { "明心关", "澄心关", "定心关", "生灭关", "物我关", "真我关" };
            for (int i = 0; i < 6; i++)
            {
                if (!actor.IsHeartBroken(i))
                    return $"需突破 {heartNames[i]}";
            }

            if (!actor.HasAllLaws()) return "需集齐四大法则";
            if (!actor.hasTrait(TraitIds.SmallWorld_4)) return "需将小世界修至四重";
            if (actor.GetInsightLevel() < 5) return $"需顿悟达到5级（当前{actor.GetInsightLevel()}级）";
            if (actor.GetNianLi() < 50000000f) return $"念力需达到5000万（当前{actor.GetNianLi():F0}）";
            if (KarmaSystem.GetKarma(actor) > 10f) return $"业力需≤10（当前{KarmaSystem.GetKarma(actor):F0}）";

            float lifespan = actor.stats["lifespan"];
            float age = actor.getAge();
            if (lifespan - age < 500f) return $"剩余寿命需≥500年（当前{lifespan - age:F0}年）";

            actor.data.get(DataKeys.LAST_ASCENSION_TIMER, out float lastFailTime, -100f);
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            if (currentYear - lastFailTime < 50f) return $"需等待{50 - (currentYear - lastFailTime):F0}年冷却";

            return "";
        }

        // ---- 旧版小阳劫阻挡原因（保持不变） ----
        public static string GetAscensionBlockReason(this Actor actor)
        {
            if (actor.GetRealmLevel() == 4)
            {
                if (actor.IsMinorAscensionComplete()) return "已完成小阳劫";
                if (!actor.IsHeartBroken(5)) return "需突破真我关";
                if (actor.GetNianLi() < 1000000f) return "念力需达到100万";
                if (!actor.HasAllLaws()) return "需集齐四大法则";
                if (!actor.hasTrait(TraitIds.SmallWorld_4)) return "需将小世界修至四重";
                return "";
            }
            if (actor.GetRealmLevel() == 5)
            {
                if (!actor.IsMinorAscensionComplete()) return "需先完成小阳劫";
                // ★ 使用新的大阳劫条件
                return actor.GetMajorAscensionBlockReason();
            }
            return "需达到环界境或托举境";
        }
    }
}