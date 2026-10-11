using UnityEngine;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Core
{
    /// <summary>
    /// 心灵天赋系统 - 角色出生时自动赋予随机天赋
    /// 12种心灵天赋：元觉、空纳、钝承、本源共情、洞因、孤衡、释执、溯情、镜观、重诠、御念、净忆
    /// </summary>
    public static class TalentSystem
    {
        // 12种天赋的ID和权重
        private static readonly (string id, float weight)[] TalentWeights = new (string, float)[]
        {
            ("Talent_YuanJue", 15f),   // 元觉
            ("Talent_KongNa", 15f),    // 空纳
            ("Talent_DunCheng", 15f),  // 钝承
            ("Talent_BenYuan", 8f),    // 本源共情（稀有）
            ("Talent_DongYin", 8f),    // 洞因（稀有）
            ("Talent_GuHeng", 12f),    // 孤衡
            ("Talent_ShiZhi", 10f),    // 释执
            ("Talent_SuQing", 10f),    // 溯情
            ("Talent_JingGuan", 10f),  // 镜观
            ("Talent_ChongQuan", 8f),  // 重诠（稀有）
            ("Talent_YuNian", 8f),     // 御念（稀有）
            ("Talent_JingYi", 5f),     // 净忆（最稀有）
        };

        /// <summary>
        /// 角色出生时赋予天赋
        /// </summary>
        public static void GrantTalentOnBirth(Actor actor)
        {
            if (actor == null || actor.data == null) return;
            if (actor.getAge() > 1) return;
            if (HasTalent(actor)) return;

            // 计算总权重
            float totalWeight = 0f;
            foreach (var t in TalentWeights)
                totalWeight += t.weight;

            // 随机选择
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            string selectedId = TalentWeights[0].id;

            foreach (var t in TalentWeights)
            {
                cumulative += t.weight;
                if (roll <= cumulative)
                {
                    selectedId = t.id;
                    break;
                }
            }

            actor.addTrait(selectedId);
	    NianLiInfoPanel.MarkDirty();
            // 天赋获得不再输出控制台日志（保持控制台干净）
        }

        /// <summary>
        /// 检查是否已有天赋
        /// </summary>
        public static bool HasTalent(Actor actor)
        {
            foreach (var t in TalentWeights)
            {
                if (actor.hasTrait(t.id))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取天赋名称
        /// </summary>
        public static string GetTalentName(string id)
        {
            switch (id)
            {
                case "Talent_YuanJue": return "元觉";
                case "Talent_KongNa": return "空纳";
                case "Talent_DunCheng": return "钝承";
                case "Talent_BenYuan": return "本源共情";
                case "Talent_DongYin": return "洞因";
                case "Talent_GuHeng": return "孤衡";
                case "Talent_ShiZhi": return "释执";
                case "Talent_SuQing": return "溯情";
                case "Talent_JingGuan": return "镜观";
                case "Talent_ChongQuan": return "重诠";
                case "Talent_YuNian": return "御念";
                case "Talent_JingYi": return "净忆";
                default: return id;
            }
        }
    }
}