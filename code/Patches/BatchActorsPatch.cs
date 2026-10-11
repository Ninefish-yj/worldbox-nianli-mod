using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Patches
{
    public static class BatchActorsPatch
    {
        private static int _lastProcessedYear = -1;
        private static int _frameCounter = 0;
        private const int FRAME_INTERVAL = 10;

        // ★★★ 方案一：分帧参数优化 ★★★
        private static List<Actor> _pendingActors = null;
        private static int _pendingIndex = 0;
        private static int _currentYear = -1;
        private const int ACTORS_PER_FRAME = 3;          // 原5，改为3
        private const float MAX_TIME_SLICE_MS = 0.003f;  // 原5ms，改为3ms

        // ★★★ 性能统计 ★★★
        private static float _totalProcessTime = 0f;
        private static int _totalProcessedCount = 0;
        private static float _lastStatsReport = 0f;

        public static void ProcessAllActors()
        {
            try
            {
                NianLiGarbageCollectorSystem.CheckAutoGC();

                if (Time.deltaTime > 0.5f) return;
                if (World.world == null || World.world.isPaused()) return;
                if (World.world.units == null) return;

                ActiveCultivatorCache.CleanupIfNeeded();

                _frameCounter++;
                if (_frameCounter < FRAME_INTERVAL) return;
                _frameCounter = 0;

                float elapsed = Time.deltaTime;
                int currentYear = World.world.map_stats?.year ?? -1;

                // 非年度逻辑
                NpcNianLiAI.ProcessAllNpcAI(elapsed);
                ProcessNoWayForwardRecovery(elapsed);

                // 年度逻辑
                if (currentYear != _lastProcessedYear && currentYear > 0)
                {
                    _lastProcessedYear = currentYear;
                    _currentYear = currentYear;
                    _pendingActors = World.world.units.getSimpleList();
                    _pendingIndex = 0;

                    ShuffleList(_pendingActors);
                    CultivationMethodSystem.TryGrantRandomMethodToRandomActor();

                    if (ConfigCache.EnableDebugLog)
                        Debug.Log($"[念力修行] 年度 {currentYear} 开始分帧处理，共 {_pendingActors?.Count ?? 0} 个角色");
                }

                // 分帧处理
                if (_pendingActors != null && _pendingIndex < _pendingActors.Count)
                {
                    int processed = 0;
                    float startTime = Time.realtimeSinceStartup;

                    while (_pendingIndex < _pendingActors.Count &&
                           processed < ACTORS_PER_FRAME &&
                           Time.realtimeSinceStartup - startTime < MAX_TIME_SLICE_MS)
                    {
                        Actor actor = _pendingActors[_pendingIndex];
                        _pendingIndex++;

                        if (actor != null && actor.isAlive() && actor.isSapient() && !actor.isRekt())
                        {
                            ProcessActorYearly(actor, currentYear);
                            processed++;
                        }
                    }

                    _totalProcessedCount += processed;
                    _totalProcessTime += Time.realtimeSinceStartup - startTime;

                    if (_pendingIndex >= _pendingActors.Count)
                    {
                        ActiveCultivatorCache.Rebuild();
                        CultivationMethodSystem.CleanupMethodCache();
                        MeditationSystem.CleanupInactiveActors();
                        ShieldSystem.CleanupInactiveActors();

                        _pendingActors = null;
                        _pendingIndex = 0;

                        if (ConfigCache.EnableDebugLog)
                            Debug.Log($"[念力修行] 年度 {_currentYear} 分帧处理完成，共处理 {_totalProcessedCount} 个角色");
                    }
                }

                ProcessCultivatorsPerFrame(elapsed);
                ReportPerformance();

                if (_pendingActors == null && Time.frameCount % 600 == 0)
                    CultivationMethodSystem.CleanupMethodCache();
            }
            catch (Exception e)
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.LogWarning($"[念力修行] ProcessAllActors 异常: {e.Message}");
            }
        }

        // ★★★ 修改：增加 currentYear 参数，用于频率优化 ★★★
        private static void ProcessActorYearly(Actor actor, int currentYear)
        {
            if (actor == null || !actor.isAlive() || !actor.isSapient() || actor.isRekt())
                return;

            if (actor.getAge() <= 1)
                TalentSystem.GrantTalentOnBirth(actor);

            int realm = actor.GetRealmLevel();

            if (realm >= 1)
            {
                GrowthSystem.ProcessYearlyGrowth(actor);
                MoodFluctuationSystem.ProcessMoodFluctuation(actor);
                KarmaSystem.ProcessYearlyKarmaDecay(actor);
                RandomEventsSystem.CheckRandomEvents(actor);

                if (realm >= 4)
                    CycleLawSystem.ProcessCycleLawGrowth(actor);
            }
            else
            {
                if (actor.getAge() >= 5)
                    GrowthSystem.ProcessYearlyGrowth(actor);
            }

            // ★★★ 方案三：功法补发改为每5年一次 ★★★
            if (realm >= 3 && currentYear % 5 == 0 && CultivationMethodSystem.GetMethods(actor).Count == 0)
            {
                if (CultivationMethodSystem.GrantRandomMethod(actor) && ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] 补发功法给 {actor.data.name}（境界{realm}）");
            }
        }

        private static void ProcessCultivatorsPerFrame(float elapsed)
        {
            var ids = ActiveCultivatorCache.GetAllIds().ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                long id = ids[i];
                Actor actor = World.world.units?.get(id);
                if (actor == null || !actor.isAlive() || actor.isRekt())
                {
                    ActiveCultivatorCache.RemoveCultivator(actor);
                    continue;
                }
                if (!actor.isSapient()) continue;
                if (actor.GetRealmLevel() < 1)
                {
                    ActiveCultivatorCache.RemoveCultivator(actor);
                    continue;
                }

                ManageHunger(actor, elapsed);

                if (actor.IsAscensionReady() && actor.getHealth() <= 0)
                {
                    actor.SetAscensionReady(false);
                    actor.SetAscensionStep(0);
                    continue;
                }

                MeditationSystem.ProcessMeditation(actor, elapsed);
                MeditationSystem.SpawnMeditationEffect(actor, elapsed);
                ShieldSystem.RegenShield(actor, elapsed);

                if (actor.IsHeartBroken(5))
                    actor.UpdateZenWuWeiTimer(elapsed);

                if (actor.IsAscensionReady())
                {
                    try
                    {
                        FlyingAscensionSystem.ProcessAscension(actor, elapsed);
                    }
                    catch (Exception ex)
                    {
                        if (ConfigCache.EnableDebugLog)
                            Debug.LogError($"[念力修行] 飞升异常: {ex.Message}");
                        actor.SetAscensionReady(false);
                        actor.SetAscensionStep(0);
                    }
                }
            }
        }

        private static void ProcessNoWayForwardRecovery(float elapsed)
        {
            var ids = ActiveCultivatorCache.GetAllIds().ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                Actor actor = World.world.units?.get(ids[i]);
                if (actor == null || !actor.isAlive()) continue;
                if (!actor.hasTrait(TraitIds.Trait_NoWayForward)) continue;
                NoWayForwardAI.ProcessRecovery(actor, elapsed);
            }
        }

        private static void ManageHunger(Actor actor, float elapsed)
        {
            if (actor == null || !actor.isAlive() || !actor.isSapient()) return;

            int realm = actor.GetRealmLevel();
            if (realm >= 4)
            {
                try
                {
                    if (actor.data.nutrition > 0)
                        actor.setNutrition(0, true);
                }
                catch { }
                return;
            }

            float nianLi = actor.GetNianLi();
            if (nianLi <= 0) return;

            try
            {
                int current = actor.data.nutrition;
                int max = actor.getMaxNutrition();
                if (current < max * 0.2f)
                {
                    int target = Math.Min(max, current + 20);
                    float cost = (target - current) * 0.5f;
                    if (nianLi >= cost)
                    {
                        actor.ChangeNianLi(-cost);
                        actor.setNutrition(target, true);
                    }
                }
            }
            catch (Exception e)
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.LogWarning($"[念力修行] 饥饿管理异常: {e.Message}");
            }
        }

        private static void ShuffleList(List<Actor> list)
        {
            if (list == null || list.Count <= 1) return;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static void ReportPerformance()
        {
            if (Time.time - _lastStatsReport < 60f) return;
            _lastStatsReport = Time.time;

            if (_totalProcessedCount > 0)
            {
                float avgTime = (_totalProcessTime / _totalProcessedCount) * 1000f;
                Debug.Log($"[念力修行] 年度处理统计: {_totalProcessedCount} 个角色, 平均 {avgTime:F2}ms/角色, 总耗时 {_totalProcessTime * 1000:F2}ms");
                _totalProcessedCount = 0;
                _totalProcessTime = 0f;
            }

            Debug.Log($"[念力修行] 功法缓存大小: {CultivationMethodSystem.GetCacheSize()}");
        }
    }
}