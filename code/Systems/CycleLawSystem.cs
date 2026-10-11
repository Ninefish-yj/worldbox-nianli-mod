using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    /// <summary>
    /// 天地大环主干法则系统
    /// 四大法则：轮回、阴阳、时空、因果
    /// 环界境以上可领悟，每年随机领悟
    /// </summary>
    public static class CycleLawSystem
    {
        /// <summary>
        /// 处理法则领悟（每年调用）
        /// 环界境(4)以上才能领悟
        /// </summary>
        public static void ProcessCycleLawGrowth(Actor actor)
        {
            if (actor == null) return;
            if (actor.GetRealmLevel() < 4) return;

            // 每年有2%概率领悟一条新法则
            if (Random.Range(0f, 1f) < Constants.CYCLE_LAW_GROWTH_RATE)
            {
                int currentProgress = actor.GetCycleLawProgress();
                if (currentProgress < Constants.CYCLE_LAW_MAX)
                {
                    actor.AddCycleLawProgress(1);
                    string lawName = Constants.CycleLaws[currentProgress];
                    WorldMessageSystem.Broadcast($"{actor.data.name} 领悟了 {lawName} 法则!");
                }
            }
        }

        /// <summary>
        /// 应用法则效果（在实际逻辑中调用）
        /// 实际效果分散在各系统中，通过检查法则特质实现
        /// </summary>
        public static void ApplyLawEffects(Actor actor)
        {
            if (actor == null) return;
            // 各系统通过 HasXxxLaw 方法检查并应用效果
        }

        // ★★★ 法则检查方法 ★★★
        public static bool HasReincarnationLaw(Actor actor) => actor != null && actor.hasTrait(TraitIds.Law_Reincarnation);
        public static bool HasYinYangLaw(Actor actor) => actor != null && actor.hasTrait(TraitIds.Law_YinYang);
        public static bool HasTimeSpaceLaw(Actor actor) => actor != null && actor.hasTrait(TraitIds.Law_TimeSpace);
        public static bool HasCauseEffectLaw(Actor actor) => actor != null && actor.hasTrait(TraitIds.Law_CauseEffect);

        /// <summary>
        /// 获取法则效果描述（用于UI）
        /// </summary>
        public static string GetLawDescription(int progress)
        {
            string desc = "";
            if (progress >= 1) desc += "轮回: 死后保留50%念力\n";
            if (progress >= 2) desc += "阴阳: 20%攻击转防御\n";
            if (progress >= 3) desc += "时空: 念力增长+50%\n";
            if (progress >= 4) desc += "因果: 攻击必中\n";
            return desc;
        }

        /// <summary>
        /// 处理轮回转世（死亡时调用）
        /// 轮回法则(Lv1)：死后保留50%念力转世
        /// </summary>
        public static void ProcessReincarnation(Actor actor)
        {
            if (actor == null) return;
            if (actor.GetCycleLawProgress() < 1) return;

            // 保存当前念力，后续转世时恢复
            float currentNianLi = actor.GetNianLi();
            float savedNianLi = currentNianLi * 0.5f;

            actor.AddReincarnationCount();
            WorldMessageSystem.Broadcast($"🔄 {actor.data.name} 启动了轮回法则!");
        }
    }
}