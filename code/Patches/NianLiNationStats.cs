using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using NianLiXiuXing.code.Core;
using NeoModLoader.General;

namespace NianLiXiuXing.code.UI
{
    /// <summary>
    /// 国家修士统计
    /// 
    /// 【功能】
    /// 在国家窗口的统计行中显示该国念力修士的数量和境界分布
    /// 
    /// 【显示位置】
    /// 国家窗口 → 统计行（StatsRowsContainer）
    /// 
    /// 【显示效果】
    /// ┌─────────────────────────────────────────────┐
    /// │  念力修士  15  (悬停显示详细分布)          │
    /// │              ├── 托举：0人                  │
    /// │              ├── 环界：1人                  │
    /// │              ├── 大通：3人                  │
    /// │              ├── 复观：5人                  │
    /// │              └── 养念：6人                  │
    /// │              最高境界：环界境               │
    /// └─────────────────────────────────────────────┘
    /// 
    /// 【技术实现】
    /// 通过 Harmony 补丁，在 KingdomWindow.showStatsRows 时注入
    /// 使用原版 KeyValueField 组件，与游戏原生风格一致
    /// </summary>
    [HarmonyPatch]
    public static class NianLiNationStats
    {
        // ==================== 常量 ====================
        private static readonly string[] REALM_NAMES = { "", LM.Get("realm_nurture"), LM.Get("realm_reflect"), LM.Get("realm_great"), LM.Get("realm_ring"), LM.Get("realm_uphold") };

        // ==================== Harmony补丁 ====================

        /// <summary>
        /// Harmony补丁：在国家窗口显示修士统计
        /// 
        /// 原版方法：KingdomWindow.showStatsRows
        /// 执行时机：国家窗口打开或切换国家时
        /// 
        /// 实现流程：
        /// 1. 获取当前选中的国家
        /// 2. 统计该国的修士境界分布
        /// 3. 创建或获取统计行
        /// 4. 设置显示文字和数值
        /// 5. 绑定Tooltip（显示详细分布）
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(KingdomWindow), nameof(KingdomWindow.showStatsRows))]
        public static void Post_showStatsRows(KingdomWindow __instance)
        {
            // ---- 第1步：获取国家 ----
            Kingdom kingdom = SelectedMetas.selected_kingdom;
            if (kingdom == null || __instance == null) return;

            var container = __instance.stats_rows_container;
            if (container == null) return;

            // ---- 第2步：统计数据 ----
            var stats = GetRealmStats(kingdom);
            int total = 0;
            foreach (var count in stats.Values) total += count;

            // ---- 第3步：创建或获取统计行 ----
            // 使用唯一ID "nianli_kingdom_cultivators" 避免冲突
            KeyValueField row = container.getStatRow("nianli_kingdom_cultivators");
            if (row == null) return;

            // ---- 第4步：设置显示 ----
            row.name_text.text = "念力修士";
            row.value.text = total.ToString();
            row.icon.gameObject.SetActive(false);  // 隐藏图标

            // ---- 第5步：绑定Tooltip ----
            TooltipDataGetter tooltipGetter = delegate ()
            {
                var sb = new StringBuilder();
                bool hasAny = false;

                // 从高到低显示各境界人数
                for (int i = 5; i >= 1; i--)
                {
                    if (stats[i] > 0)
                    {
                        sb.AppendLine($"{REALM_NAMES[i]}：{stats[i]}人");
                        hasAny = true;
                    }
                }

                if (!hasAny) sb.AppendLine("暂无");

                // 显示最高境界
                int highest = GetHighestRealm(kingdom);
                if (highest > 0)
                {
                    sb.AppendLine($"\n最高境界：{REALM_NAMES[highest]}");
                }

                return new TooltipData
                {
                    _tip_name = "念力修士境界分布",
                    _tip_description = sb.ToString().TrimEnd()
                };
            };

            row.setMetaForTooltip(0, -1L, "nianli_kingdom_cultivators_info", tooltipGetter);
            row.gameObject.SetActive(true);
        }

        // ==================== 数据统计方法 ====================

        /// <summary>
        /// 统计国家的修士境界分布
        /// 
        /// 遍历国家的所有单位，统计各境界人数
        /// </summary>
        /// <returns>Dictionary<境界等级, 人数></returns>
        private static Dictionary<int, int> GetRealmStats(Kingdom kingdom)
        {
            var stats = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++) stats[i] = 0;

            if (kingdom?.units == null) return stats;

            foreach (Actor actor in kingdom.units)
            {
                if (actor?.data == null || !actor.isAlive()) continue;
                int realm = actor.GetRealmLevel();
                if (realm >= 1 && realm <= 5) stats[realm]++;
            }

            return stats;
        }

        /// <summary>
        /// 获取国家的最高境界
        /// </summary>
        private static int GetHighestRealm(Kingdom kingdom)
        {
            int highest = 0;
            if (kingdom?.units == null) return highest;

            foreach (Actor actor in kingdom.units)
            {
                if (actor?.data == null || !actor.isAlive()) continue;
                int realm = actor.GetRealmLevel();
                if (realm > highest) highest = realm;
            }

            return highest;
        }
    }
}