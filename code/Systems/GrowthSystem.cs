using UnityEngine;
using NianLiXiuXing.code.Core;
using System.Collections.Generic;

namespace NianLiXiuXing.code.Systems
{
    public static partial class GrowthSystem
    {
        // ============================================================
        // ★★★ 凡人念力增长配置 ★★★
        // ============================================================
        private const float MORTAL_BASE_GROWTH = 50f;
        private const float MORTAL_MAX_NIAN_LI = 1100f;

        // ============================================================
        // ★★★ 开拓者记录 ★★★
        // ============================================================
        private static readonly string[] PIONEER_KEYS = {
            "",
            "nianli_pioneer_realm_1",
            "nianli_pioneer_realm_2",
            "nianli_pioneer_realm_3",
            "nianli_pioneer_realm_4",
            "nianli_pioneer_realm_5"
        };
        private static HashSet<int> _recordedPioneerRealms = new HashSet<int>();
        private static HashSet<int> _pioneerProcessing = new HashSet<int>();

        private static void CheckAndRecordPioneer(Actor actor, int realmLevel)
        {
            if (actor == null || realmLevel < 1 || realmLevel > 5) return;
            if (_recordedPioneerRealms.Contains(realmLevel)) return;
            if (_pioneerProcessing.Contains(realmLevel)) return;

            var customData = World.world?.map_stats?.custom_data;
            if (customData == null) return;

            customData.get(PIONEER_KEYS[realmLevel], out string existingName, "");
            if (!string.IsNullOrEmpty(existingName)) return;

            _pioneerProcessing.Add(realmLevel);
            customData.set(PIONEER_KEYS[realmLevel], actor.getName());
            _recordedPioneerRealms.Add(realmLevel);
            _pioneerProcessing.Remove(realmLevel);

            WorldMessageSystem.BroadcastPioneer(actor, realmLevel);

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] 开拓者记录：{actor.getName()} 成为境界{realmLevel}的开拓者");
        }

        public static void MarkPioneerRecorded(int realmLevel)
        {
            if (realmLevel >= 1 && realmLevel <= 5)
                _recordedPioneerRealms.Add(realmLevel);
        }

        // ============================================================
        // ★★★ 年度增长主入口 ★★★
        // ============================================================
        public static void ProcessYearlyGrowth(Actor actor)
        {
            if (actor == null || !actor.isAlive() || !actor.isSapient()) return;
            
            int currentYear = World.world?.map_stats?.year ?? -1;
            if (currentYear < 0) return;
            if (currentYear <= actor.GetLastGrowthYear()) return;

            int realm = actor.GetRealmLevel();

            if (realm == 0)
            {
                ProcessMortalGrowth(actor);
                // ★★★ 凡人不再提前 return，继续执行后面的心关尝试 ★★★
            }
            else
            {
                // 修士年度增长
                try
                {
                    float growth = CalculateYearlyGrowth(actor);
                    float currentMax = actor.GetCurrentMaxNianLi();
                    if (currentMax <= 0) currentMax = actor.GetRealmMaxNianLi();
                    actor.SetCurrentMaxNianLi(currentMax + growth);
                    actor.ChangeNianLi(growth);

                    if (ConfigCache.EnableDebugLog)
                        Debug.Log($"[念力修行] {actor.getName()} 念力增长 {growth:F0}，当前 {actor.GetNianLi():F0}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[念力修行] 修士增长异常: {e.Message}");
                }
            }

            // ★★★ 所有角色（包括凡人）都尝试突破心关 ★★★
            for (int i = 0; i < 6; i++)
            {
                // 凡人只尝试前三关（明心、澄心、定心）
                if (realm == 0 && i >= 3) break;
                TryBreakHeart(actor, i);
            }

            // 顿悟判定（所有角色）
            if (Random.Range(0f, 1f) < Constants.INSIGHT_CHANCE)
            {
                int insight = actor.GetInsightLevel();
                if (insight < 5) actor.SetInsightLevel(insight + 1);
            }

            // 法则领悟（修士）
            if (realm >= 1)
            {
                TryGainLaw(actor);
                TryBreakthrough(actor);
            }

            actor.SetLastGrowthYear(currentYear);
        }

        // ============================================================
        // ★★★ 凡人念力增长 ★★★
        // ============================================================
        private static void ProcessMortalGrowth(Actor actor)
        {
            if (actor == null) return;
            
            int age = actor.getAge();
            if (age < 5) return;

            float growth = MORTAL_BASE_GROWTH;
            
            // 天赋加成
            float talentBonus = GetTalentSpeedBonus(actor);
            growth *= (1f + talentBonus);

            float currentNianLi = actor.GetNianLi();
            float maxNianLi = MORTAL_MAX_NIAN_LI;
            float newNianLi = Mathf.Min(currentNianLi + growth, maxNianLi);
            actor.SetNianLi(newNianLi);

            // ★★★ 凡人突破至养念境（需要任意心关 + 50%概率） ★★★
            if (newNianLi >= MORTAL_MAX_NIAN_LI && age >= 10)
            {
                if (actor.IsHeartBroken(0) || actor.IsHeartBroken(1) || actor.IsHeartBroken(2))
                {
                    if (Random.Range(0f, 1f) < 0.5f)  // 50% 概率突破
                    {
                        DoBreakthrough(actor, 0, 1, maxNianLi);
                      }
                }
            }

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] 凡人 {actor.getName()} 念力增长 {growth:F0}，当前 {newNianLi:F0}/{maxNianLi:F0}");
        }

        // ============================================================
        // ★★★ 天赋速度加成（public：供 UI 展示各项加成明细） ★★★
        // ============================================================
        public static float GetTalentSpeedBonus(Actor actor)
        {
            if (actor.hasTrait("Talent_JingYi")) return 2.0f;
            if (actor.hasTrait("Talent_BenYuan")) return 1.0f;
            if (actor.hasTrait("Talent_DongYin")) return 1.0f;
            if (actor.hasTrait("Talent_ChongQuan")) return 1.0f;
            if (actor.hasTrait("Talent_YuNian")) return 1.0f;
            if (actor.hasTrait("Talent_GuHeng")) return 0.5f;
            if (actor.hasTrait("Talent_ShiZhi")) return 0.5f;
            if (actor.hasTrait("Talent_SuQing")) return 0.5f;
            if (actor.hasTrait("Talent_JingGuan")) return 0.5f;
            if (actor.hasTrait("Talent_YuanJue")) return 0.2f;
            if (actor.hasTrait("Talent_KongNa")) return 0.2f;
            if (actor.hasTrait("Talent_DunCheng")) return 0.2f;
            return 0f;
        }

        // ============================================================
        // ★★★ 总修行速度倍率（public：供 UI 展示与实际增长共用） ★★★
        // ============================================================
        public static float GetTotalCultivationSpeed(Actor actor)
        {
            float heartBonus = actor.stats["HeartBonus"];
            float talentBonus = GetTalentSpeedBonus(actor);
            float manaBonus = actor.GetManaSpeedBonus();
            float methodBonus = CultivationMethodSystem.GetTotalSpeedBonus(actor);
            float lawBonus = CycleLawSystem.HasTimeSpaceLaw(actor) ? 0.5f : 0f;
            float meditationMultiplier = MeditationSystem.GetMeditationMultiplier(actor);
            float stayingMultiplier = actor.IsStaying() ? Constants.STAYING_BONUS : 1f;
            float brokenPenalty = BrokenHeartSystem.GetBrokenHeartPenalty(actor);

            return (1f + heartBonus + talentBonus + manaBonus + methodBonus + lawBonus)
                   * meditationMultiplier * stayingMultiplier * (1f - brokenPenalty);
        }

        // ============================================================
        // ★★★ 修士年度念力增长计算 ★★★
        // ============================================================
        private static float CalculateYearlyGrowth(Actor actor)
        {
            return Constants.BASE_GROWTH * GetTotalCultivationSpeed(actor);
        }
    }
}