using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Core;
using System.Linq;
using NeoModLoader.General;
using NianLiXiuXing.code.Systems.Events;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Systems
{
    public static class NpcNianLiAI
    {
        private static float _lastProcessTime = 0f;
        private const float PROCESS_INTERVAL = 10f;

        // ============================================================
        // ★★★ 近邻缓存 ★★★
        // ============================================================
        private static Dictionary<long, List<Actor>> _nearbyCache = new Dictionary<long, List<Actor>>();
        private static Dictionary<long, float> _nearbyCacheTime = new Dictionary<long, float>();
        private const float CACHE_TTL = 2f;
        private static float _lastCacheCleanup = 0f;
        private const float CACHE_CLEANUP_INTERVAL = 30f;

        public static void ProcessAllNpcAI(float elapsed)
        {
            if (World.world?.units == null) return;

            _lastProcessTime += elapsed;
            if (_lastProcessTime < PROCESS_INTERVAL) return;
            _lastProcessTime = 0f;

            CleanupNearbyCache();

            var ids = ActiveCultivatorCache.GetAllIds().ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                Actor actor = World.world.units?.get(ids[i]);
                if (actor?.data == null || !actor.isAlive()) continue;
                if (!actor.isSapient()) continue;
                if (actor.GetRealmLevel() < 1) continue;
                if (actor.isFavorite()) continue;
                ProcessSingleNpcAI(actor);
            }
        }

        // ============================================================
        // ★ 带缓存的近邻查询 ★
        // ============================================================
        private static List<Actor> GetCachedNearby(Actor actor, int chunkLevel, float radius)
        {
            if (actor == null || actor.current_tile == null)
                return new List<Actor>();

            long id = actor.getID();
            if (_nearbyCache.TryGetValue(id, out var cached) &&
                _nearbyCacheTime.TryGetValue(id, out float timestamp) &&
                Time.time - timestamp < CACHE_TTL)
            {
                return cached ?? new List<Actor>();
            }

            // ★★★ 修复：显式转换为 List<Actor> ★★★
            var result = Finder.getUnitsFromChunk(actor.current_tile, chunkLevel, radius, false);
            _nearbyCache[id] = (result as List<Actor>) ?? new List<Actor>();
            _nearbyCacheTime[id] = Time.time;
            return _nearbyCache[id];
        }

        // ============================================================
        // ★ 缓存清理 ★
        // ============================================================
        public static void CleanupNearbyCache()
        {
            if (Time.time - _lastCacheCleanup < CACHE_CLEANUP_INTERVAL) return;
            _lastCacheCleanup = Time.time;

            if (World.world?.units == null)
            {
                _nearbyCache.Clear();
                _nearbyCacheTime.Clear();
                return;
            }

            List<long> toRemove = new List<long>();
            foreach (var id in _nearbyCache.Keys)
            {
                Actor actor = World.world.units.get(id);
                if (actor == null || !actor.isAlive())
                    toRemove.Add(id);
            }
            foreach (var id in toRemove)
            {
                _nearbyCache.Remove(id);
                _nearbyCacheTime.Remove(id);
            }
        }

        public static void ClearNearbyCache(long actorId)
        {
            _nearbyCache.Remove(actorId);
            _nearbyCacheTime.Remove(actorId);
        }

        public static void ClearNearbyCache(Actor actor)
        {
            if (actor != null)
                ClearNearbyCache(actor.getID());
        }

        // ============================================================
        // ★ 单个 NPC AI 处理 ★
        // ============================================================
        private static void ProcessSingleNpcAI(Actor actor)
        {
            float nianLi = actor.GetNianLi();
            float maxNianLi = actor.GetCurrentMaxNianLi();
            if (maxNianLi <= 0) maxNianLi = actor.GetRealmMaxNianLi();
            if (maxNianLi <= 0) return;

            float ratio = nianLi / maxNianLi;

            if (actor.hasTrait(TraitIds.Trait_NoWayForward))
            {
                ProcessNoWayForwardNPC(actor);
                return;
            }

            if (ratio < 0.15f && !actor.IsAttacked() && !actor.is_moving)
            {
                TryForceMeditate(actor);
                return;
            }

            if (ratio > 0.90f)
            {
                TryNpcBreakthrough(actor);
                return;
            }

            if (actor.GetRealmLevel() >= 4 && Random.Range(0f, 1f) < 0.005f)
            {
                TryLearnLaw(actor);
            }

            if (actor.GetRealmLevel() >= 3 && Random.Range(0f, 1f) < 0.002f)
            {
                TryUpgradeMethod(actor);
            }
        }

        // ============================================================
        // ★ 前路断绝 NPC 行为 ★
        // ============================================================
        private static void ProcessNoWayForwardNPC(Actor actor)
        {
            if (actor.current_tile == null || actor.is_moving) return;

            bool hasEnemy = false;
            var nearby = GetCachedNearby(actor, 1, 5f);
            for (int i = 0; i < nearby.Count; i++)
            {
                Actor other = nearby[i];
                if (other == actor || other?.data == null || !other.isAlive()) continue;
                if (actor.kingdom != other.kingdom)
                {
                    hasEnemy = true;
                    break;
                }
            }

            if (hasEnemy)
            {
                WorldTile escapeTile = NoWayForwardAI.GetTargetTile(actor);
                if (escapeTile != null)
                    actor.goTo(escapeTile);
            }
            else if (!actor.IsMeditating())
                actor.SetMeditating(true);
        }

        // ============================================================
        // ★ 强制冥想 ★
        // ============================================================
        private static void TryForceMeditate(Actor actor)
        {
            if (actor.IsMeditating() || actor.is_moving) return;

            var nearby = GetCachedNearby(actor, 1, 5f);
            for (int i = 0; i < nearby.Count; i++)
            {
                Actor other = nearby[i];
                if (other == actor || other?.data == null || !other.isAlive()) continue;
                if (actor.kingdom != other.kingdom) return;
            }
            actor.SetMeditating(true);
        }

        // ============================================================
        // ★ NPC 突破 ★
        // ============================================================
        private static void TryNpcBreakthrough(Actor actor)
        {
            int current = actor.GetRealmLevel();
            if (current >= 5) return;
            if (actor.hasTrait(TraitIds.Trait_NoWayForward)) return;

            int next = current + 1;
            if (ConfigCache.IsBlockBreakthroughRealm(next)) return;

            float threshold = Constants.RealmThresholds[next];
            float nianLi = actor.GetNianLi();
            if (nianLi < threshold * 1.3f) return;

            if (current > 0 && !actor.HasAllRequiredHearts(current)) return;

            if (next == 5 && current == 4)
            {
                if (actor.IsMinorAscensionComplete())
                    XiuXingEventCenter.TriggerBreakthrough(actor, current, next, true);
                else if (actor.CanStartMinorAscension())
                    FlyingAscensionSystem.TriggerAscension(actor);
                return;
            }

            float chance = (next == 4) ? actor.GetLawCount() * 0.10f : Constants.BreakthroughChances[current];
            if (Random.Range(0f, 1f) <= chance)
                XiuXingEventCenter.TriggerBreakthrough(actor, current, next, true);
        }

        // ============================================================
        // ★ 学习法则 ★
        // ============================================================
        private static void TryLearnLaw(Actor actor)
        {
            if (actor.HasAllLaws()) return;

            string[] laws = { TraitIds.Law_Reincarnation, TraitIds.Law_YinYang, TraitIds.Law_TimeSpace, TraitIds.Law_CauseEffect };
            string[] lawNames = {
                LM.Get("nianli_law_reincarnation"),
                LM.Get("nianli_law_yinyang"),
                LM.Get("nianli_law_timespace"),
                LM.Get("nianli_law_causeeffect")
            };

            for (int i = 0; i < laws.Length; i++)
            {
                if (!actor.hasTrait(laws[i]))
                {
                    actor.addTrait(laws[i]);
                    actor.AddCycleLawProgress(1);
                    XiuXingEventCenter.TriggerLawGain(actor, lawNames[i], i);
                    if (actor.HasAllLaws())
                        WorldTip.showNow($"「轮回·阴阳·时空·因果」\n{actor.data.name} 已集齐四大法则！", true, "top");
                    break;
                }
            }
        }

        // ============================================================
        // ★ 升级功法 ★
        // ============================================================
        private static void TryUpgradeMethod(Actor actor)
        {
            var methods = CultivationMethodSystem.GetMethods(actor);
            if (methods.Count == 0)
            {
                CultivationMethodSystem.GrantRandomMethod(actor);
                return;
            }

            int realm = actor.GetRealmLevel();
            var available = new List<MethodData>();
            foreach (var data in CultivationMethodSystem.AllMethods)
            {
                if (data.Method != CultivationMethod.None &&
                    realm >= data.UnlockRealm &&
                    !methods.Contains(data.Method))
                {
                    float weight = GetMethodWeightForAI(data.Rarity);
                    for (int i = 0; i < (int)weight; i++)
                        available.Add(data);
                }
            }
            if (available.Count == 0) return;

            MethodData selected = available[Random.Range(0, available.Count)];
            CultivationMethod currentMethod = CultivationMethodSystem.GetMethod(actor);
            MethodData currentData = CultivationMethodSystem.GetMethodData(currentMethod);

            bool shouldSwitch = false;
            if (selected.Rarity > currentData.Rarity)
                shouldSwitch = true;
            else if (selected.Rarity == currentData.Rarity && selected.SpeedBonus > currentData.SpeedBonus)
                shouldSwitch = true;

            if (!shouldSwitch) return;

            CultivationMethodSystem.SwitchMethod(actor, selected.Method);
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行AI] {actor.data.name} 切换功法至 {LocalizedTextManager.getText(selected.NameKey)} (稀有度: {selected.Rarity})");
        }

        private static float GetMethodWeightForAI(MethodRarity rarity)
        {
            switch (rarity)
            {
                case MethodRarity.Common: return 1f;
                case MethodRarity.Uncommon: return 2f;
                case MethodRarity.Rare: return 4f;
                case MethodRarity.Epic: return 8f;
                case MethodRarity.Legendary: return 16f;
                default: return 1f;
            }
        }
    }
}