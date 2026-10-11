using UnityEngine;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public static class HeartRealmEffects
    {
        public static void ProcessAllHeartEffects(Actor attacker, Actor defender)
        {
            if (attacker == null || defender == null) return;

            ProcessMingXinEffect(attacker);
            ProcessChengXinEffect(defender);
            ProcessDingXinEffect(defender);
            ProcessShengMieEffect(attacker, defender);
            ProcessWuWoEffect(attacker);
        }

        private static void ProcessMingXinEffect(Actor actor)
        {
            if (!actor.IsHeartBroken(0)) return;
            actor.ChangeNianLi(5f);
        }

        private static void ProcessChengXinEffect(Actor actor)
        {
            if (!actor.IsHeartBroken(1)) return;
        }

        private static void ProcessDingXinEffect(Actor actor)
        {
            if (!actor.IsHeartBroken(2)) return;
        }

        private static void ProcessShengMieEffect(Actor attacker, Actor defender)
        {
            if (!attacker.IsHeartBroken(3)) return;

            attacker.data.get(DataKeys.LAST_DAMAGE, out float damage, 0f);
            float stealAmount = damage * 0.1f;

            float targetNianLi = defender.GetNianLi();
            float actualSteal = Mathf.Min(stealAmount, targetNianLi);
            defender.ChangeNianLi(-actualSteal);
            attacker.ChangeNianLi(actualSteal * 0.5f);
        }

        private static void ProcessWuWoEffect(Actor actor)
        {
            if (!actor.IsHeartBroken(4)) return;
        }

        public static bool IsImmuneToKnockback(Actor actor)
        {
            return actor.IsHeartBroken(2);
        }

        public static bool CanIgnoreTerrain(Actor actor)
        {
            return actor.IsHeartBroken(4);
        }

        public static string GetHeartEffectDescription(int heartIndex)
        {
            switch (heartIndex)
            {
                case 0: return "明心关: 念力凝聚 - 攻击获得念力";
                case 1: return "澄心关: 念力纯净 - 免疫负面状态";
                case 2: return "定心关: 心念不动 - 免疫击退眩晕";
                case 3: return "生灭关: 借天地之力 - 减少攻击念力消耗";
                case 4: return "物我关: 脱离束缚 - 可穿越障碍";
                case 5: return "真我关: 自在不灭 - 致命伤时以念力抵挡，3秒无敌";
                default: return "";
            }
        }
    }
}