using UnityEngine;

namespace NianLiXiuXing.code.Core
{
    public static partial class ActorExtensions
    {
        private const string HEART_PROGRESS_KEY = DataKeys.HEART_PROGRESS;
        private const string INSIGHT_LEVEL_KEY = DataKeys.INSIGHT_LEVEL;

        // ---- 心关数据存取 ----
        public static int GetHeartProgress(this Actor actor)
        {
            actor.data.get(HEART_PROGRESS_KEY, out int val, 0);
            return val;
        }
        public static void SetHeartProgress(this Actor actor, int mask)
        {
            actor.data.set(HEART_PROGRESS_KEY, mask);
        }
        public static bool IsHeartBroken(this Actor actor, int heartIndex)
        {
            return (actor.GetHeartProgress() & (1 << heartIndex)) != 0;
        }
        public static void BreakHeart(this Actor actor, int heartIndex)
        {
            int mask = actor.GetHeartProgress();
            mask |= (1 << heartIndex);
            actor.SetHeartProgress(mask);
        }

        // 养念三关无序突破（念力需求 1000）
        public static bool CanBreakHeartUnordered(this Actor actor, int heartIndex)
        {
            if (heartIndex < 0 || heartIndex > 2) return false;
            if (actor.IsHeartBroken(heartIndex)) return false;
            switch (heartIndex)
            {
                case 0: return actor.getAge() >= 10 && actor.GetNianLi() > 1000f;
                case 1: return actor.getAge() >= 15 && actor.GetNianLi() > 1000f;
                case 2: return actor.getAge() >= 20 && actor.GetNianLi() > 1000f;
                default: return false;
            }
        }

        // ---- 顿悟 ----
        public static int GetInsightLevel(this Actor actor)
        {
            actor.data.get(INSIGHT_LEVEL_KEY, out int val, 0);
            return val;
        }
        public static void SetInsightLevel(this Actor actor, int level)
        {
            actor.data.set(INSIGHT_LEVEL_KEY, Mathf.Clamp(level, 0, 5));
        }

        // ---- 心关倒退检查 ----
        public static bool CanHeartRegress(this Actor actor, int heartIndex)
        {
            if (heartIndex == 5) return false;
            if (!actor.IsHeartBroken(heartIndex)) return false;
            return true;
        }
    }
}