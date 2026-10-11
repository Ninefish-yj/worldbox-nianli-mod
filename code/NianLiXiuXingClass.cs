using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Traits;
using NianLiXiuXing.code.UI;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.Patches;
using NianLiXiuXing.code.Systems.Events;

namespace NianLiXiuXing
{
    internal class NianLiXiuXingClass : BasicMod<NianLiXiuXingClass>
    {
        public static string id = "nianlixiuxing";
        private static NianLiXiuXingClass _instance;
        public static NianLiXiuXingClass Instance => _instance;

        private static bool _hasPerformedStartupCleanup = false;
        private static float _lastUpdateTime = 0f;
        private static bool _hasDelayedFixStarted = false;

        protected override void OnModLoad()
        {
            _instance = this;
            gameObject.SetActive(true);

            Stats.Init();
            TraitGroups.Init();
            TraitDefinitions.Init();

            // ★★★ 手动逐个应用补丁（避免单个补丁失败导致全部失效） ★★★
            try
            {
                var harmony = new Harmony(id);
                var assembly = Assembly.GetExecutingAssembly();

                // 获取所有带 [HarmonyPatch] 的类
                var patchClasses = new List<Type>();
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<HarmonyPatch>(false) != null)
                        patchClasses.Add(type);
                }

                Debug.Log($"[念力修行] 发现 {patchClasses.Count} 个补丁类");

                int successCount = 0;
                int failCount = 0;

                // ★★★ 关键修复：不需要声明变量，直接调用 Patch() ★★★
                foreach (var patchClass in patchClasses)
                {
                    try
                    {
                        // ★ 对 DamagePatch 特殊处理：即使失败也不影响其他补丁 ★
                        if (patchClass.Name == "DamagePatch")
                        {
                            Debug.Log("[念力修行] 尝试应用 DamagePatch...");
                            try
                            {
                                // ★ 直接调用，不存储返回值 ★
                                harmony.CreateClassProcessor(patchClass).Patch();
                                successCount++;
                                Debug.Log($"[念力修行] 补丁 {patchClass.Name} 应用成功");
                            }
                            catch (Exception ex)
                            {
                                failCount++;
                                Debug.LogWarning($"[念力修行] 补丁 {patchClass.Name} 应用失败（已跳过）: {ex.Message}");
                            }
                            continue;
                        }

                        // ★ 其他补丁同样直接调用 ★
                        harmony.CreateClassProcessor(patchClass).Patch();
                        successCount++;
                        Debug.Log($"[念力修行] 补丁 {patchClass.Name} 应用成功");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Debug.LogWarning($"[念力修行] 补丁 {patchClass.Name} 应用失败: {ex.Message}");
                    }
                }

                Debug.Log($"[念力修行] Harmony 补丁应用完成：成功 {successCount}，失败 {failCount}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[念力修行] Harmony 初始化异常: {e.Message}");
            }

            WitchcraftWorldLog.Init();
            AscensionRecordSystem.Load();

            NianLiAscensionRecordWindow.Init();
            UnitWindowNianLiUI.Init();
            UnitWindowKarmaBar.Init();
            NianLiRankingWindow.Init();
            NianLiStatsIcon.Init();
            NianLiInfoPanel.Init();
            NianLiTab.Init();

            DaoBeiSystem.Init();
            NianLiCultivationMethodUI.Init();
            NianLiDaoBeiUI.Init();

            NianLiPerformanceUI.Init();

            ActiveCultivatorCache.Rebuild();

            LoadExistingPioneers();
            StartCoroutine(DelayedStartupCleanup());

            try
            {
                ActorExtensions.FixAllOverMaxNianLi();
                CultivationMethodSystem.FixMissingMethodsForOldSaves();
                FixAllActorValues();
                Debug.Log("[念力修行] 启动修复完成");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[念力修行] 启动修复异常: {e.Message}");
            }

            // ============================================================
            // ★★★ 配置加载（直接使用 ModConfig） ★★★
            // ============================================================
            try
            {
                var config = GetConfig();
                if (config != null)
                {
                    ConfigCache.LoadFromConfig(config);
                    Debug.Log("[念力修行] 配置加载完成");
                }
                else
                {
                    Debug.LogWarning("[念力修行] 无法获取 ModConfig，使用默认配置");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[念力修行] 配置加载失败: {e.Message}，使用默认配置");
            }

            try
            {
                XiuXingEventHandlers.SubscribeAll();
                Debug.Log("[念力修行] 事件订阅完成");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[念力修行] 事件订阅失败: {e.Message}");
            }

            if (!_hasDelayedFixStarted)
            {
                _hasDelayedFixStarted = true;
                StartCoroutine(DelayedFixMethods());
            }

            Debug.Log("[念力修行] 模组加载完成");
        }

        private IEnumerator DelayedFixMethods()
        {
            float waitTime = 0f;
            while (World.world?.units == null && waitTime < 5f)
            {
                yield return null;
                waitTime += Time.deltaTime;
            }
            yield return null;

            Debug.Log("[念力修行] 开始执行延迟功法补发...");
            CultivationMethodSystem.FixMissingMethodsForOldSaves();
            Debug.Log("[念力修行] 延迟功法补发执行完毕");
        }

        private IEnumerator DelayedStartupCleanup()
        {
            yield return null;
            yield return null;
            yield return null;

            if (_hasPerformedStartupCleanup) yield break;
            _hasPerformedStartupCleanup = true;

            if (World.world == null) yield break;

            int waitCount = 0;
            while (World.world.map_stats == null && waitCount < 30)
            {
                yield return null;
                waitCount++;
            }

            if (World.world.map_stats == null) yield break;

            try
            {
                AscensionRecordSystem.CleanupAllActorStates();
                Debug.Log("[念力修行] 启动清理完成");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[念力修行] 启动清理时出错: {e.Message}");
            }
        }

        private void LoadExistingPioneers()
        {
            string[] keys = {
                "nianli_pioneer_realm_1",
                "nianli_pioneer_realm_2",
                "nianli_pioneer_realm_3",
                "nianli_pioneer_realm_4",
                "nianli_pioneer_realm_5"
            };

            for (int i = 0; i < keys.Length; i++)
            {
                string pioneer = "";
                World.world?.map_stats?.custom_data.get(keys[i], out pioneer, "");
                if (!string.IsNullOrEmpty(pioneer))
                {
                    GrowthSystem.MarkPioneerRecorded(i + 1);
                }
            }
        }

        private void FixAllActorValues()
        {
            if (World.world?.units == null) return;
            int fixedCount = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || actor.data == null) continue;
                bool fixedSomething = false;

                float nianLi = actor.GetNianLi();
                if (float.IsNaN(nianLi) || float.IsInfinity(nianLi) || nianLi < 0)
                {
                    actor.SetNianLi(0);
                    fixedSomething = true;
                }

                float maxNianLi = actor.GetCurrentMaxNianLi();
                if (float.IsNaN(maxNianLi) || float.IsInfinity(maxNianLi) || maxNianLi < 0)
                {
                    actor.SetCurrentMaxNianLi(actor.GetRealmMaxNianLi());
                    fixedSomething = true;
                }

                float shieldHP = 0f;
                try { shieldHP = actor.GetShieldHP(); } catch { }
                if (float.IsNaN(shieldHP) || float.IsInfinity(shieldHP) || shieldHP < 0)
                {
                    try { actor.SetShieldHP(0); } catch { }
                    fixedSomething = true;
                }

                float shieldMax = 0f;
                try { shieldMax = actor.GetShieldMax(); } catch { }
                if (float.IsNaN(shieldMax) || float.IsInfinity(shieldMax) || shieldMax < 0)
                {
                    try { actor.SetShieldMax(0); } catch { }
                    fixedSomething = true;
                }

                if (fixedSomething) fixedCount++;
            }
            if (fixedCount > 0)
            {
                Debug.Log($"[念力修行] 修复了 {fixedCount} 个角色的异常数值");
            }
        }

        private void Update()
        {
            try
            {
                if (Time.deltaTime > 0.5f) return;
                if (Time.time - _lastUpdateTime < 0.01f) return;
                _lastUpdateTime = Time.time;

                BatchActorsPatch.ProcessAllActors();

                NianLiPerformanceUI.Update();

                // ---- 快捷键 ----
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
                {
                    ConfigCache.ReloadConfig();
                    WorldTip.showNow("🔄 念力修行配置已热重载！", true, "top");
                    if (ConfigCache.EnableDebugLog)
                        Debug.Log("[念力修行] 配置热重载完成");
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
                {
                    FixAllActorValues();
                    WorldTip.showNow("🔧 已修复所有角色异常数值！", true, "top");
                    if (ConfigCache.EnableDebugLog)
                        Debug.Log("[念力修行] 数值修复完成");
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
                {
                    NianLiRankingWindow.ShowWindow();
                    if (ConfigCache.EnableDebugLog)
                        Debug.Log("[念力修行] 快捷键 Ctrl+Shift+L 触发，打开排行榜");
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
                {
                    if (SelectedUnit.isSet())
                    {
                        Actor actor = SelectedUnit.unit;
                        if (actor != null && actor.isAlive())
                            KarmaSystem.PerformCleanse(actor);
                    }
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.G))
                {
                    int count = 0;
                    if (World.world?.units != null)
                    {
                        var allUnits = World.world.units.getSimpleList();
                        for (int i = 0; i < allUnits.Count; i++)
                        {
                            Actor actor = allUnits[i];
                            if (actor == null || !actor.isAlive() || !actor.isSapient()) continue;
                            if (actor.GetRealmLevel() < 3) continue;
                            if (CultivationMethodSystem.GetMethods(actor).Count > 0) continue;
                            if (CultivationMethodSystem.GrantRandomMethod(actor))
                                count++;
                        }
                    }
                    WorldTip.showNow($"✅ 补发功法给 {count} 个角色", true, "top");
                    Debug.Log($"[念力修行] 手动补发功法给 {count} 个大通境及以上角色");
                }

                if (SelectedUnit.isSet() && Input.GetKeyDown(KeyCode.G))
                {
                    Actor actor = SelectedUnit.unit;
                    if (actor != null && actor.isAlive())
                    {
                        GrowthSystem.ProcessYearlyGrowth(actor);
                        WorldTip.showNow($"🧪 已触发 {actor.getName()} 的年度增长", true, "top");
                    }
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
                {
                    NianLiPerformanceUI.ShowWindow();
                    if (ConfigCache.EnableDebugLog)
                        Debug.Log("[念力修行] 快捷键 Ctrl+Shift+P 触发，打开性能监控窗口");
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
                {
                    NianLiGarbageCollectorSystem.ForceCleanup();
                    WorldTip.showNow("🧹 已强制清理所有缓存并触发GC", true, "top");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[念力修行] Update 异常: {e.Message}");
            }
        }
    }
}