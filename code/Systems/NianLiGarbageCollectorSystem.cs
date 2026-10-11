using System;
using UnityEngine;
using UnityEngine.Profiling;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Systems
{
    /// <summary>
    /// 念力修行模组垃圾回收系统
    /// 负责内存缓存清理、GC触发及性能维护
    /// </summary>
    internal static class NianLiGarbageCollectorSystem
    {
        // ============================================================
        // ★ 配置常量 ★
        // ============================================================
        private const float COOLDOWN_SECONDS = 50f;
        private const float MEMORY_CHECK_INTERVAL = 5f;
        private const int DEFAULT_THRESHOLD_MB = 512;
        private const long BYTES_PER_MB = 1048576L;

        // ============================================================
        // ★ 状态字段 ★
        // ============================================================
        private static float _lastGCTime = -COOLDOWN_SECONDS;
        private static float _lastCheckTime = 0f;
        private static bool _isRunning = false;
        public static bool EnableAutoGC = true;

        // ============================================================
        // ★ 主入口：自动GC检查（由 BatchActorsPatch 每帧调用） ★
        // ============================================================
        public static void CheckAutoGC()
        {
            if (!EnableAutoGC) return;
            if (_isRunning) return;

            float currentTime = Time.unscaledTime;
            if (currentTime - _lastGCTime < COOLDOWN_SECONDS) return;
            if (currentTime - _lastCheckTime < MEMORY_CHECK_INTERVAL) return;
            _lastCheckTime = currentTime;

            long usedBytes = Profiler.GetMonoUsedSizeLong();
            long thresholdBytes = (long)DEFAULT_THRESHOLD_MB * BYTES_PER_MB;

            if (usedBytes >= thresholdBytes)
            {
                Debug.Log($"[念力修行-GC] 内存阈值触发: {usedBytes / BYTES_PER_MB}MB >= {DEFAULT_THRESHOLD_MB}MB，开始GC");
                RunLightGC();
            }
        }

        // ============================================================
        // ★ 轻量GC ★
        // ============================================================
        public static void RunLightGC()
        {
            if (_isRunning) return;

            long beforeMB = GetUsedMB();
            _isRunning = true;

            try
            {
                GC.Collect(1, GCCollectionMode.Optimized, false, false);
                CleanupModuleCaches();
                _lastGCTime = Time.unscaledTime;
                _lastCheckTime = _lastGCTime;

                long afterMB = GetUsedMB();
                if (beforeMB != afterMB)
                {
                    Debug.Log($"[念力修行-GC] 轻量GC完成: {beforeMB}MB -> {afterMB}MB (释放 {beforeMB - afterMB}MB)");
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        // ============================================================
        // ★ 完整GC（手动调用） ★
        // ============================================================
        public static void RunFullGC()
        {
            if (_isRunning) return;

            long beforeMB = GetUsedMB();
            _isRunning = true;

            try
            {
                Resources.UnloadUnusedAssets();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(0, GCCollectionMode.Forced, true, false);
                CleanupModuleCaches();
                _lastGCTime = Time.unscaledTime;
                _lastCheckTime = _lastGCTime;

                long afterMB = GetUsedMB();
                Debug.Log($"[念力修行-GC] 完整GC完成: {beforeMB}MB -> {afterMB}MB (释放 {beforeMB - afterMB}MB)");
            }
            finally
            {
                _isRunning = false;
            }
        }

        // ============================================================
        // ★ 辅助方法 ★
        // ============================================================
        private static long GetUsedMB()
        {
            return Profiler.GetMonoUsedSizeLong() / BYTES_PER_MB;
        }

        // ============================================================
        // ★★★ 核心：清理所有模块缓存（扩展集成） ★★★
        // ============================================================
        private static void CleanupModuleCaches()
        {
            // ---- 原有清理 ----
            ActiveCultivatorCache.CleanupIfNeeded();
            MeditationSystem.CleanupInactiveActors();
            ShieldSystem.CleanupInactiveActors();

            // ============================================================
            // ★★★ 新增：功法缓存清理 ★★★
            // ============================================================
            CultivationMethodSystem.CleanupMethodCache();

            // ============================================================
            // ★★★ 新增：特效池清理 ★★★
            // ============================================================
            EffectPool.Cleanup();

            // ============================================================
            // ★★★ 新增：AI近邻缓存清理 ★★★
            // ============================================================
            NpcNianLiAI.CleanupNearbyCache();

            // ============================================================
            // ★★★ 新增：StringBuilder池清理（可选，通常不需要） ★★★
            // ============================================================
            // StringBuilderPool.Clear(); // 谨慎使用，可能影响性能
        }

        // ============================================================
        // ★ 外部开关 ★
        // ============================================================
        public static void OnEnableAutoGCChanged(bool value)
        {
            EnableAutoGC = value;
            Debug.Log($"[念力修行-GC] 自动GC已{(value ? "启用" : "禁用")}");
        }

        // ============================================================
        // ★ 手动触发（调试用） ★
        // ============================================================
        public static void ForceCleanup()
        {
            RunLightGC();
            CleanupModuleCaches();
            Debug.Log("[念力修行-GC] 已强制清理所有缓存");
        }
    }
}