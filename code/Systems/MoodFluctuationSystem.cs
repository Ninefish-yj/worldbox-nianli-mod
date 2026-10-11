using UnityEngine;
using NianLiXiuXing.code.Core;
using System.Collections.Generic;

namespace NianLiXiuXing.code.Systems
{
    public static class MoodFluctuationSystem
    {
        private static readonly string[] HeartNames = { "明心关", "澄心关", "定心关", "生灭关", "物我关", "真我关" };
        private static readonly float[] HeartBaseRegressChance = { 
            0.05f, 0.04f, 0.03f, 0.02f, 0.01f, 0.005f
        };
        private static readonly float[] RealmReduction = {
            1.0f, 1.0f, 0.8f, 0.5f, 0.3f, 0.0f
        };

        public static void ProcessMoodFluctuation(Actor actor)
        {
            if (actor == null) return;
            if (!actor.isSapient()) return;
            if (!actor.isAlive()) return;

            int realm = actor.GetRealmLevel();
            if (realm <= 0 || realm >= 5) return;

            // ★★★ 方案三：心关倒退改为每3年检查一次 ★★★
            int currentYear = World.world?.map_stats?.year ?? -1;
            if (currentYear < 0) return;
            if (currentYear % 3 != 0) return;  // ← 每3年一次

            TryHeartRegress(actor, realm);

            if (realm == 1)
                TryHeartStabilize(actor, 0.01f);
        }

        private static void TryHeartRegress(Actor actor, int realm)
        {
            List<int> brokenHearts = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                if (actor.IsHeartBroken(i))
                    brokenHearts.Add(i);
            }

            if (brokenHearts.Count == 0) return;

            int heartIndex = brokenHearts[Random.Range(0, brokenHearts.Count)];
            float regressChance = CalculateRegressChance(actor, realm, heartIndex);

            if (Random.Range(0f, 1f) < regressChance)
            {
                int mask = actor.GetHeartProgress();
                mask &= ~(1 << heartIndex);
                actor.SetHeartProgress(mask);

                if (actor.hasTrait($"Heart_{heartIndex}"))
                    actor.removeTrait($"Heart_{heartIndex}");

                float cost = 500f + heartIndex * 200f;
                actor.ChangeNianLi(-cost);

                int insight = actor.GetInsightLevel();
                if (insight > 0 && Random.Range(0f, 1f) < 0.2f)
                    actor.SetInsightLevel(insight - 1);

                BrokenHeartSystem.RecordHeartRegress(actor);

                if (realm >= 4)
                {
                    string msg = $"💔 {actor.data.name} 的 {HeartNames[heartIndex]} 出现裂痕，心性倒退！";
                    WorldTip.showNow(msg, true, "top");
                    WitchcraftWorldLog.AddLog("heart_regress", msg, actor, new Color(0.6f, 0.2f, 0.2f));
                }

                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] {actor.data.name} 心关倒退: {HeartNames[heartIndex]} (概率{regressChance*100:F2}%)");
            }
        }

        private static float CalculateRegressChance(Actor actor, int realm, int heartIndex)
        {
            float chance = HeartBaseRegressChance[heartIndex];
            chance *= RealmReduction[realm];

            int insight = actor.GetInsightLevel();
            chance *= (1f - insight * 0.08f);

            int lawCount = actor.GetLawCount();
            chance *= (1f - lawCount * 0.05f);

            if (actor.hasTrait(TraitIds.SmallWorld_4))
                chance *= 0.9f;

            if (actor.IsHeartBroken(5) && heartIndex == 0)
                chance *= 0.5f;

            if (heartIndex >= 4 && realm < 4)
                return 0f;

            return Mathf.Clamp(chance, 0.0001f, HeartBaseRegressChance[heartIndex] * 2f);
        }

        private static void TryHeartStabilize(Actor actor, float chance)
        {
            if (Random.Range(0f, 1f) > chance) return;

            for (int i = 0; i < 3; i++)
            {
                if (!actor.IsHeartBroken(i))
                {
                    bool canReBreak = false;
                    switch (i)
                    {
                        case 0: canReBreak = actor.getAge() >= 10 && actor.GetNianLi() > 1000f; break;
                        case 1: canReBreak = actor.getAge() >= 15 && actor.GetNianLi() > 1000f; break;
                        case 2: canReBreak = actor.getAge() >= 20 && actor.GetNianLi() > 1000f; break;
                    }
                    if (canReBreak)
                    {
                        actor.BreakHeart(i);
                        if (!actor.hasTrait($"Heart_{i}"))
                            actor.addTrait($"Heart_{i}");
                        if (ConfigCache.EnableDebugLog)
                            Debug.Log($"[念力修行] {actor.data.name} 心关稳固，重新获得 {HeartNames[i]}");
                        break;
                    }
                }
            }
        }
    }
}