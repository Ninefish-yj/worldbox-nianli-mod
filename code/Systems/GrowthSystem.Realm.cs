using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems.Events;
using System.Collections.Generic;
using NeoModLoader.General;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Systems
{
    public static partial class GrowthSystem
    {
        private static void ForceKillActor(Actor actor)
        {
            if (actor == null || actor.isRekt() || !actor.isAlive()) return;
            try
            {
                ActiveCultivatorCache.RemoveCultivator(actor);
                actor.SetShieldHP(0);
                actor.SetShieldMax(0);
                actor.data.health = -1;
                actor.dieAndDestroy(AttackType.Other);
            }
            catch (System.Exception e)
            {
                if (ConfigCache.EnableDebugLog) 
                    Debug.LogWarning($"[念力修行] 强制杀死单位失败: {e.Message}");
            }
        }

        private static float GetBreakthroughToRealm4Chance(Actor actor)
        {
            return actor.GetLawCount() * 0.10f;
        }

        private static float GetBreakthroughChance(int currentRealm)
        {
            if (currentRealm < 0 || currentRealm >= Constants.BreakthroughChances.Length) return 0.5f;
            if (ConfigCache.UseHardMode)
            {
                float[] hardChances = { 0.50f, 0.35f, 0.20f, 0.12f, 0.06f };
                return hardChances[currentRealm];
            }
            return Constants.BreakthroughChances[currentRealm];
        }

        private static float GetBreakthroughDeathChance(int currentRealm)
        {
            if (currentRealm < 0 || currentRealm >= Constants.BreakthroughDeathChances.Length) return 0.1f;
            if (ConfigCache.UseHardMode)
            {
                float[] hardDeathChances = { 0.15f, 0.20f, 0.30f, 0.40f, 0.50f };
                return hardDeathChances[currentRealm];
            }
            return Constants.BreakthroughDeathChances[currentRealm];
        }

        private static float GetBreakthroughNoWayChance(int currentRealm)
        {
            if (currentRealm < 0 || currentRealm >= Constants.BreakthroughNoWayChances.Length) return 0.1f;
            if (ConfigCache.UseHardMode)
            {
                float[] hardNoWayChances = { 0.15f, 0.20f, 0.30f, 0.40f, 1.00f };
                return hardNoWayChances[currentRealm];
            }
            return Constants.BreakthroughNoWayChances[currentRealm];
        }

        private static string GetRealmName(int realmLevel)
        {
            string[] names = { 
                LM.Get("realm_mortal"), 
                LM.Get("realm_nurture"), 
                LM.Get("realm_reflect"), 
                LM.Get("realm_great"), 
                LM.Get("realm_ring"), 
                LM.Get("realm_uphold") 
            };
            return (realmLevel >= 0 && realmLevel <= 5) ? names[realmLevel] : "未知";
        }

        private static string GenerateTitle(Actor actor)
        {
            if (actor == null) return "无名";
            int realm = actor.GetRealmLevel();
            int heartCount = 0;
            for (int i = 0; i < 6; i++) if (actor.IsHeartBroken(i)) heartCount++;
            int lawCount = actor.GetLawCount();
            bool hasSmallWorld4 = actor.hasTrait(TraitIds.SmallWorld_4);
            if (realm < 3) return null;
            string[] realmTitles = { "", "", "", "通玄", "创界", "超脱" };
            string[] heartSuffixes = { "初成", "小成", "大成", "圆满" };
            int heartIndex = Mathf.Clamp(heartCount / 2, 0, 3);
            string title = realmTitles[realm] + heartSuffixes[heartIndex];
            if (lawCount == 4) title = "万象" + title;
            if (hasSmallWorld4) title = "九转" + title;
            if (realm == 5 && actor.IsHeartBroken(5)) title = "至圣" + title;
            if (heartCount == 6 && realm >= 3) title = "无极" + title;
            if (actor.GetInsightLevel() >= 5) title += "·慧";
            return title.Length > 16 ? title.Substring(0, 16) : title;
        }

        private static void UpdateActorNameWithRealm(Actor actor)
        {
            if (actor == null || actor.data == null) return;
            int realm = actor.GetRealmLevel();
            string realmName = GetRealmName(realm);
            if (realmName == LM.Get("realm_mortal") || realmName == "未知") return;
            
            string currentName = actor.getName();
            string baseName = ExtractBaseName(currentName);
            if (string.IsNullOrEmpty(baseName)) baseName = "修士";
            
            string newName;
            if (realm >= 3)
            {
                string title = GenerateTitle(actor);
                if (string.IsNullOrEmpty(title)) title = "修士";
                newName = title + "·" + baseName + "·" + realmName;
            }
            else
            {
                newName = baseName + "·" + realmName;
            }
            actor.setName(newName, true);
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {baseName} 突破至 {realmName}，新名称：{newName}");
        }

        private static string ExtractBaseName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "";
            if (!fullName.Contains("·")) return fullName;
            
            string[] parts = fullName.Split('·');
            if (parts.Length >= 2)
            {
                string lastPart = parts[parts.Length - 1];
                string[] realmNames = { 
                    LM.Get("realm_nurture"), 
                    LM.Get("realm_reflect"), 
                    LM.Get("realm_great"), 
                    LM.Get("realm_ring"), 
                    LM.Get("realm_uphold") 
                };
                foreach (string rn in realmNames)
                {
                    if (lastPart == rn) 
                        return parts[parts.Length - 2];
                }
                return parts[0];
            }
            return parts[0];
        }

        private static int CountRealmActors(int realmLevel)
        {
            if (World.world?.units == null) return 0;
            int count = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || !actor.isAlive()) continue;
                if (actor.GetRealmLevel() == realmLevel)
                    count++;
            }
            return count;
        }

       private static bool CanBreakthroughToHighRealm(Actor actor, int targetRealm)
{
    // ★ 现在大通境(3)也开始受限制
    if (targetRealm < 3) return true;

    int maxAllowed = 0;
    string realmName = "";

    switch (targetRealm)
    {
        case 3:
            maxAllowed = ConfigCache.MaxGreatRealm;
            realmName = "大通境";
            break;
        case 4:
            maxAllowed = ConfigCache.MaxRingRealm;
            realmName = "环界境";
            break;
        case 5:
            maxAllowed = ConfigCache.MaxUpholdRealm;
            realmName = "托举境";
            break;
        default:
            return true;
    }

    if (maxAllowed <= 0) return true;  // 0 = 无限制

    int currentCount = CountRealmActors(targetRealm);
    if (currentCount >= maxAllowed)
    {
        if (actor.isFavorite()) return true;  // 收藏角色例外
        WorldTip.showNow($"⚠️ {realmName} 角色已达上限（{maxAllowed}），无法继续突破", true, "top");
        return false;
    }
    return true;
}
        private static void TryBreakthrough(Actor actor)
        {
            if (actor == null || !actor.isSapient() || !actor.isAlive()) return;
            if (actor.hasTrait(TraitIds.Trait_NoWayForward)) return;
            if (actor.getAge() < 5) return;
            if (actor.getHealth() <= 0) return;

            int current = actor.GetRealmLevel();
            if (current >= 5) return;
            int next = current + 1;
            if (ConfigCache.IsBlockBreakthroughRealm(next)) return;

            float threshold = Constants.RealmThresholds[next];
            if (actor.GetNianLi() < threshold) return;
            if (current > 0 && !actor.HasAllRequiredHearts(current)) return;

            if (!CanBreakthroughToHighRealm(actor, next))
                return;

            if (next == 5 && current == 4)
            {
                if (actor.IsMinorAscensionComplete())
                {
                    DoBreakthrough(actor, current, next, threshold);
                    return;
                }
                if (actor.CanStartMinorAscension())
                    FlyingAscensionSystem.TriggerAscension(actor);
                return;
            }

            float chance = (next == 4) ? GetBreakthroughToRealm4Chance(actor) : GetBreakthroughChance(current);
            if (Random.Range(0f, 1f) > chance)
            {
                HandleBreakthroughFailure(actor, current, next, threshold);
                return;
            }
            DoBreakthrough(actor, current, next, threshold);
        }

        private static void HandleBreakthroughFailure(Actor actor, int current, int next, float threshold)
        {
            if (next < 4)
            {
                if (Random.Range(0f, 1f) < 0.01f) ForceKillActor(actor);
                return;
            }

            float failCost = threshold * 0.05f;
            actor.ChangeNianLi(-failCost);

            if (Random.Range(0f, 1f) < GetBreakthroughDeathChance(current))
            {
                WorldMessageSystem.BroadcastBreakthroughFail(actor, next, true);
                ForceKillActor(actor);
                return;
            }

            if (Random.Range(0f, 1f) < GetBreakthroughNoWayChance(current))
            {
                if (!actor.hasTrait(TraitIds.Trait_NoWayForward))
                {
                    actor.addTrait(TraitIds.Trait_NoWayForward);
                    actor.SetNianLi(0);
                    actor.SetShieldHP(0);
                    actor.SetShieldMax(0);
                    actor.setHealth(Mathf.Max(1, actor.getMaxHealth() / 2));
                    actor.data.health = Mathf.Max(1, actor.getMaxHealth() / 2);
                    actor.SetMeditating(false);
                    actor.SetStaying(false);
                    actor.SetStayingStartYear(-1);
                    actor.SetStayingDuration(0);
                    if (next >= 4) WorldMessageSystem.BroadcastBreakthroughFail(actor, next, false);
                }
            }
        }

        private static void DoBreakthrough(Actor actor, int current, int next, float threshold)
        {
            // ★ 心魔劫检查（目标境界>=3时触发）
            if (next >= 3 && HeartDemonSystem.CheckHeartDemon(actor, next))
            {
                return;
            }

            actor.SetRealmLevel(next);
            NianLiInfoPanel.MarkDirty();
            actor.ChangeNianLi(-threshold * 0.1f);

            ActiveCultivatorCache.AddCultivator(actor);

            if (!actor.hasTrait($"Realm_{next}")) actor.addTrait($"Realm_{next}");
            if (current > 0 && actor.hasTrait($"Realm_{current}")) actor.removeTrait($"Realm_{current}");
            UpdateActorNameWithRealm(actor);

            XiuXingEventCenter.TriggerBreakthrough(actor, current, next, true);

            if (next >= 3 && CultivationMethodSystem.GetMethods(actor).Count == 0)
            {
                CultivationMethodSystem.GrantRandomMethod(actor);
            }

            if (next == 4)
            {
                int lawCount = actor.GetLawCount();
                if (lawCount >= 1 && lawCount <= 4)
                {
                    string smallWorldTrait = $"SmallWorld_{lawCount}";
                    if (!actor.hasTrait(smallWorldTrait)) actor.addTrait(smallWorldTrait);
                }
                else if (!actor.hasTrait(TraitIds.SmallWorld_1)) actor.addTrait(TraitIds.SmallWorld_1);
                if (!actor.isFavorite() && ConfigCache.IsAutoFavoriteRealm(next)) actor.data.favorite = true;
            }
            if (next >= 4 && !actor.isFavorite() && ConfigCache.IsAutoFavoriteRealm(next)) 
                actor.data.favorite = true;

            WorldMessageSystem.BroadcastBreakthrough(actor, next);
            CheckAndRecordPioneer(actor, next);

            if (next == 5 && actor.IsMinorAscensionComplete() && actor.CanStartMajorAscension())
                FlyingAscensionSystem.TriggerAscension(actor);

            if (next == Constants.STAYING_MIN_REALM)
            {
                WorldMessageSystem.Broadcast($"{actor.data.name} 到达大通境，开始感应天地大环法则！");
                TryEnterStaying(actor);
            }
        }
    }
}