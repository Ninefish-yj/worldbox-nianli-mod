using System.Collections.Generic;
using HarmonyLib;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Systems
{
    /// <summary>
    /// 国家修士统计系统
    /// 统计国家内各境界修士数量
    /// </summary>
    public static class NationStatisticsSystem
    {
        /// <summary>
        /// 获取国家的修士统计
        /// </summary>
        /// <returns>Dictionary<境界等级, 人数></returns>
        public static Dictionary<int, int> GetNationCultivatorStats(Kingdom kingdom)
        {
            var stats = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
                stats[i] = 0;

            if (kingdom?.units == null) return stats;

            foreach (Actor actor in kingdom.units)
            {
                if (actor?.data == null || !actor.isAlive()) continue;
                int realm = actor.GetRealmLevel();
                if (realm >= 1 && realm <= 5)
                    stats[realm]++;
            }

            return stats;
        }

        /// <summary>
        /// 获取国家总修士数
        /// </summary>
        public static int GetTotalCultivators(Kingdom kingdom)
        {
            int total = 0;
            var stats = GetNationCultivatorStats(kingdom);
            foreach (var count in stats.Values)
                total += count;
            return total;
        }

        /// <summary>
        /// 获取国家最高境界
        /// </summary>
        public static int GetHighestRealm(Kingdom kingdom)
        {
            int highest = 0;
            if (kingdom?.units == null) return highest;

            foreach (Actor actor in kingdom.units)
            {
                if (actor?.data == null || !actor.isAlive()) continue;
                int realm = actor.GetRealmLevel();
                if (realm > highest)
                    highest = realm;
            }

            return highest;
        }
    }
}