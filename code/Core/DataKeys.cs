namespace NianLiXiuXing.code.Core
{
    public static class DataKeys
    {
        // ==================== 境界系统 ====================
        public const string REALM_LEVEL = "nianli.RealmLevel";
        public const string NIAN_LI = "nianli.NianLi";
        public const string CURRENT_MAX_NIAN_LI = "nianli.CurrentMaxNianLi";
        public const string LAST_GROWTH_YEAR = "nianli.LastGrowthYear";

        // ==================== 心关系统 ====================
        public const string HEART_PROGRESS = "nianli.HeartProgress";
        public const string INSIGHT_LEVEL = "nianli.InsightLevel";

        // ==================== 战斗系统 ====================
        public const string TOTAL_KILLS = "nianli.TotalKills";
        public const string LAST_ATTACK_TIME = "nianli.LastAttackTime";
        public const string SHIELD_HP = "nianli.ShieldHP";
        public const string SHIELD_MAX = "nianli.ShieldMax";

        // ==================== 飞升系统 ====================
        public const string ASCENSION_READY = "nianli.AscensionReady";
        public const string ASCENSION_STEP = "nianli.AscensionStep";
        public const string MINOR_ASCENSION_COMPLETE = "nianli.MinorAscensionComplete";
        public const string ASCENSION_FAIL_COUNT = "nianli.AscensionFailCount";

        // ==================== 飞升计时器/类型 ====================
        public const string LAST_ASCENSION_TIMER = "nianli.AscensionTimer";
        public const string LAST_ASCENSION_TYPE = "nianli.AscensionType";

        // ==================== 战斗临时标记 ====================
        public const string LAST_CRIT = "nianli.LastCrit";
        public const string LAST_TRUE_DAMAGE = "nianli.LastTrueDamage";
        public const string LAST_LOW_HEALTH_BONUS = "nianli.LastLowHealthBonus";
        public const string LAST_DAMAGE = "nianli.LastDamage";
        public const string LAST_SKILL_WORLD_TIME = "nianli.LastSkillWorldTime";

        // ==================== 死亡/复活 ====================
        public const string DEATH_COUNT = "nianli.DeathCount";
        public const string RESURRECTION_COUNT = "nianli.ResurrectionCount";
        public const string REINCARNATION_COUNT = "nianli.ReincarnationCount";

        // ==================== 冥想 ====================
        public const string IS_MEDITATING = "nianli.IsMeditating";

        // ==================== 驻足 ====================
        public const string IS_STAYING = "nianli.IsStaying";
        public const string STAYING_START_YEAR = "nianli.StayingStartYear";
        public const string STAYING_DURATION = "nianli.StayingDuration";

        // ==================== 自在不灭 ====================
        public const string ZEN_WU_WEI = "nianli.ZenWuWei";
        public const string ZEN_WU_WEI_TIMER = "nianli.ZenWuWeiTimer";
        public const string ZEN_WU_WEI_COOLDOWN = "nianli.ZenWuWeiCooldown";

        // ==================== 法力联动 ====================
        public const string LAST_MANA_CONVERT = "nianli.LastManaConvert";

        // ==================== 法则 ====================
        public const string CYCLE_LAW_PROGRESS = "nianli.CycleLawProgress";
        // ★★★ 新增：上次法则检查年份 ★★★
        public const string LAST_LAW_CHECK_YEAR = "nianli.LastLawCheckYear";

        // ==================== 功法 ====================
        public const string CULTIVATION_METHODS = "nianli.CultivationMethods";
        public const string METHOD_SWITCH_COOLDOWN = "nianli.MethodSwitchCooldown";
    }
}