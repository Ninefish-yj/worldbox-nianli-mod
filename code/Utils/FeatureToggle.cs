using UnityEngine;

namespace NianLiXiuXing.code.Utils
{
    /// <summary>
    /// 功能开关
    /// 用于在运行时启用/禁用特定优化功能，便于回滚
    /// </summary>
    public static class FeatureToggle
    {
        // ============================================================
        // ★ 功能开关 ★
        // ============================================================
        public static bool EnableTraitCache { get; set; } = true;
        public static bool EnableCombatCache { get; set; } = true;
        public static bool EnableMethodCache { get; set; } = true;
        public static bool EnableStringBuilderPool { get; set; } = true;
        public static bool EnableEffectPool { get; set; } = true;
        public static bool EnableNearbyCache { get; set; } = true;
        public static bool EnablePerformanceMonitor { get; set; } = true;

        // ============================================================
        // ★ 配置文件支持 ★
        // ============================================================
        private const string TOGGLE_PREFIX = "nianli.feature.";

        public static void LoadFromActorData(Actor actor)
        {
            if (actor == null) return;

            actor.data.get(TOGGLE_PREFIX + "TraitCache", out bool tc, true);
            EnableTraitCache = tc;

            actor.data.get(TOGGLE_PREFIX + "CombatCache", out bool cc, true);
            EnableCombatCache = cc;

            actor.data.get(TOGGLE_PREFIX + "MethodCache", out bool mc, true);
            EnableMethodCache = mc;

            actor.data.get(TOGGLE_PREFIX + "StringBuilderPool", out bool sbp, true);
            EnableStringBuilderPool = sbp;

            actor.data.get(TOGGLE_PREFIX + "EffectPool", out bool ep, true);
            EnableEffectPool = ep;

            actor.data.get(TOGGLE_PREFIX + "NearbyCache", out bool nc, true);
            EnableNearbyCache = nc;

            actor.data.get(TOGGLE_PREFIX + "PerformanceMonitor", out bool pm, true);
            EnablePerformanceMonitor = pm;
        }

        // ============================================================
        // ★ 状态查询 ★
        // ============================================================
        public static string GetStatus()
        {
            return $@"
功能开关状态:
  特质缓存: {(EnableTraitCache ? "✓" : "✗")}
  战斗缓存: {(EnableCombatCache ? "✓" : "✗")}
  功法缓存: {(EnableMethodCache ? "✓" : "✗")}
  StringBuilder池: {(EnableStringBuilderPool ? "✓" : "✗")}
  特效池: {(EnableEffectPool ? "✓" : "✗")}
  AI近邻缓存: {(EnableNearbyCache ? "✓" : "✗")}
  性能监控: {(EnablePerformanceMonitor ? "✓" : "✗")}
";
        }

        /// <summary>
        /// 检查是否应该使用缓存（带开关控制）
        /// </summary>
        public static bool ShouldUseTraitCache() => EnableTraitCache;
        public static bool ShouldUseCombatCache() => EnableCombatCache;
        public static bool ShouldUseMethodCache() => EnableMethodCache;
        public static bool ShouldUseStringBuilderPool() => EnableStringBuilderPool;
        public static bool ShouldUseEffectPool() => EnableEffectPool;
        public static bool ShouldUseNearbyCache() => EnableNearbyCache;
        public static bool ShouldUsePerformanceMonitor() => EnablePerformanceMonitor;
    }
}