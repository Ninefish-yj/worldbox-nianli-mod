using NeoModLoader.General;
namespace NianLiXiuXing.code.Core
{
    public static class Constants
    {
        public static float[] RealmThresholds => ConfigCache.RealmThresholds;
        public static float[] RealmMaxNianLi => ConfigCache.RealmMaxNianLi;
        public static float BASE_GROWTH => ConfigCache.BaseGrowth;
        public static float INSIGHT_CHANCE => ConfigCache.InsightChance;
        public static float[] BreakthroughChances => ConfigCache.BreakthroughChances;
        public static float[] BreakthroughDeathChances => ConfigCache.BreakthroughDeathChances;
        public static float[] BreakthroughNoWayChances => ConfigCache.BreakthroughNoWayChances;
        public static float MEDITATION_MULTIPLIER => ConfigCache.MeditationMultiplier;
        public static float MEDITATION_REQUIRE_INTERVAL => ConfigCache.MeditationRequireInterval;
        public static float STAYING_BONUS => ConfigCache.StayingBonus;
        public static float STAYING_DURATION_MIN => ConfigCache.StayingDurationMin;
        public static float STAYING_DURATION_MAX => ConfigCache.StayingDurationMax;
        public static int STAYING_MIN_REALM => ConfigCache.StayingMinRealm;
        public static float CYCLE_LAW_GROWTH_RATE => ConfigCache.CycleLawGrowthRate;
        public static float[] RealmSkillCostPercent => ConfigCache.RealmSkillCostPercent;
        public static float NIANLI_PER_ATTACK => ConfigCache.NianLiPerAttack;
        public static float NIANLI_PER_KILL => ConfigCache.NianLiPerKill;
        public static float SHIELD_REGEN_RATE => ConfigCache.ShieldRegenRate;
        public static float SHIELD_CONSUME_RATE => ConfigCache.ShieldConsumeRate;
        public static int MINOR_ASCENSION_TOTAL_STEPS => ConfigCache.MinorAscensionSteps;
        public static int MAJOR_ASCENSION_TOTAL_STEPS => ConfigCache.MajorAscensionSteps;
        public static float MINOR_ASCENSION_SUCCESS_RATE => ConfigCache.MinorAscensionSuccessRate;
        public static float MAJOR_ASCENSION_SUCCESS_RATE => ConfigCache.MajorAscensionSuccessRate;
        public static float MINOR_ASCENSION_FAIL_RATE => ConfigCache.MinorAscensionFailRate;
        public static float MAJOR_ASCENSION_FAIL_RATE => ConfigCache.MajorAscensionFailRate;

        public static readonly float[] RealmAttackBonus = new float[] { 0f, 1000f, 3000f, 6000f, 12000f, 25000f };
        public static readonly float[] RealmDefenseBonus = new float[] { 0f, 500f, 1500f, 3500f, 8000f, 20000f };
        public static readonly string[] CycleLaws = new string[] { LM.Get("nianli_law_reincarnation"), LM.Get("nianli_law_yinyang"), LM.Get("nianli_law_timespace"), LM.Get("nianli_law_causeeffect") };

        public const int CYCLE_LAW_MAX = 4;
        public const int BREAKTHROUGH_FAIL_BROADCAST_MIN_REALM = 3;
        public const float MOOD_REGRESS_CHANCE = 0.005f;
        public const float MOOD_REGRESS_AGE_MIN = 30f;
        public const float MOOD_STABLE_CHANCE = 0.01f;
        public const float STAYING_AUTO_CHANCE = 0.40f;
        public const float SHIELD_BASE_MAX = 1000f;
        public const float SKILL_COOLDOWN = 30f;
    }
}