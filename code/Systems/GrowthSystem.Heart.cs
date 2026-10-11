using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems.Events;
using NeoModLoader.General;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Systems
{
    public static partial class GrowthSystem
    {
        private static string GetHeartName(int index)
        {
            string[] names = { "明心关", "澄心关", "定心关", "生灭关", "物我关", "真我关" };
            return (index >= 0 && index <= 5) ? names[index] : "未知心关";
        }

        public static float CalculateHeartBreakChance(Actor actor, int heartIndex)
        {
            int age = actor.getAge();
            int insight = actor.GetInsightLevel();
            int lawCount = actor.GetLawCount();
            float nianLi = actor.GetNianLi();

            float baseChance = 0f;
            float maxChance = 1f;

            switch (heartIndex)
            {
                case 0:
                    baseChance = 0.65f + insight * 0.05f + Mathf.Min(nianLi / 10000f * 0.02f, 0.20f);
                    maxChance = 1f;
                    break;
                case 1:
                    baseChance = 0.40f + insight * 0.05f + Mathf.Min(nianLi / 10000f * 0.02f, 0.20f);
                    maxChance = 0.80f;
                    break;
                case 2:
                    baseChance = 0.30f + insight * 0.05f + Mathf.Min(nianLi / 10000f * 0.02f, 0.20f);
                    maxChance = 0.75f;
                    break;
                case 3:
                    baseChance = 0.25f + insight * 0.04f;
                    if (actor.HasDiedBefore()) baseChance += 0.15f;
                    maxChance = 0.70f;
                    break;
                case 4:
                    baseChance = 0.30f + insight * 0.05f;
                    if (age > 80) baseChance += ((age - 80) / 10f) * 0.03f;
                    maxChance = 0.80f;
                    break;
                case 5:
                    baseChance = 0.15f + insight * 0.02f + lawCount * 0.02f;
                    if (age > 150) baseChance += ((age - 150) / 20f) * 0.03f;
                    maxChance = 0.50f;
                    break;
                default:
                    return 0f;
            }

            return Mathf.Min(baseChance, maxChance);
        }

        private static void TryBreakHeart(Actor actor, int index)
        {
            if (!actor.isSapient()) return;
            if (actor.IsHeartBroken(index)) return;

            bool canAttempt = false;

            switch (index)
            {
                case 0: canAttempt = actor.getAge() >= 10 && actor.GetNianLi() > 1000f; break;
                case 1: canAttempt = actor.getAge() >= 15 && actor.GetNianLi() > 1000f; break;
                case 2: canAttempt = actor.getAge() >= 20 && actor.GetNianLi() > 1000f; break;
                case 3: canAttempt = actor.getAge() >= 50 && actor.GetNianLi() > 50000f; break;
                case 4: canAttempt = actor.GetRealmLevel() >= 3 && actor.getAge() >= 80 && actor.GetNianLi() > 100000f; break;
                case 5: canAttempt = actor.GetRealmLevel() >= 4 && actor.getAge() >= 120 && actor.GetNianLi() > 500000f && actor.GetInsightLevel() >= 5; break;
                default: return;
            }

            if (!canAttempt) return;

            float chance = CalculateHeartBreakChance(actor, index);
            float roll = Random.Range(0f, 1f);

            if (roll >= chance)
            {
                if (index == 5)
                {
                    float cost = actor.GetNianLi() * 0.1f;
                    actor.ChangeNianLi(-cost);
                }
                else if (index >= 3)
                {
                    actor.ChangeNianLi(-500f);
                }
                return;
            }

            actor.BreakHeart(index);
            NianLiInfoPanel.MarkDirty();
            if (!actor.hasTrait($"Heart_{index}")) actor.addTrait($"Heart_{index}");

            XiuXingEventCenter.TriggerHeartBreak(actor, index);

            if (index == 4 && !actor.isFavorite())
            {
                int currentRealm = actor.GetRealmLevel();
                if (ConfigCache.IsAutoFavoriteRealm(currentRealm))
                {
                    actor.data.favorite = true;
                }
            }

            WorldMessageSystem.BroadcastHeartBreak(actor, index);
        }
    }
}