using System;
using System.Collections.Generic;
using NeoModLoader.api;
using UnityEngine;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public static class ConfigCache
    {
        // ============================================================
        // ★ 公共配置属性 ★
        // ============================================================
        public static bool AutoFavoriteRealm3 { get; private set; } = true;
        public static bool AutoFavoriteRealm4 { get; private set; } = true;
        public static bool AutoFavoriteRealm5 { get; private set; } = true;
        public static bool BlockBreakthroughRealm3 { get; private set; } = false;
        public static bool BlockBreakthroughRealm4 { get; private set; } = false;
        public static bool BlockBreakthroughRealm5 { get; private set; } = false;
        public static bool EnableDebugLog { get; private set; } = false;
        public static bool InfiniteResurrection { get; private set; } = true;
        public static bool EnableManaConvert { get; private set; } = true;
        public static bool UseHardMode { get; private set; } = false;
        public static bool EnableAutoGC { get; private set; } = true;

        public static float[] RealmThresholds { get; private set; } = new float[] { 0f, 1000f, 5000f, 20000f, 100000f, 500000f };
        public static float[] RealmMaxNianLi { get; private set; } = new float[] { 1100f, 10000f, 100000f, 1000000f, 5000000f, float.MaxValue };
        public static float BaseGrowth { get; private set; } = 100f;
        public static float InsightChance { get; private set; } = 0.02f;
        public static float[] BreakthroughChances { get; private set; } = new float[] { 0.70f, 0.55f, 0.40f, 0.25f, 0.15f };
        public static float[] BreakthroughDeathChances { get; private set; } = new float[] { 0.08f, 0.12f, 0.18f, 0.25f, 0.35f };
        public static float[] BreakthroughNoWayChances { get; private set; } = new float[] { 0.08f, 0.12f, 0.18f, 0.25f, 1.00f };
        public static float MeditationMultiplier { get; private set; } = 3.0f;
        public static float MeditationRequireInterval { get; private set; } = 5f;
        public static float StayingBonus { get; private set; } = 1.5f;
        public static float StayingDurationMin { get; private set; } = 20f;
        public static float StayingDurationMax { get; private set; } = 50f;
        public static int StayingMinRealm { get; private set; } = 3;
        public static float CycleLawGrowthRate { get; private set; } = 0.02f;
        public static float[] RealmSkillCostPercent { get; private set; } = new float[] { 0f, 0.05f, 0.08f, 0.10f, 0.12f, 0.15f };
        public static float NianLiPerAttack { get; private set; } = 50f;
        public static float NianLiPerKill { get; private set; } = 1000f;
        public static float ShieldRegenRate { get; private set; } = 50f;
        public static float ShieldConsumeRate { get; private set; } = 0.1f;
        public static int MinorAscensionSteps { get; private set; } = 5;
        public static int MajorAscensionSteps { get; private set; } = 9;
        public static float MinorAscensionSuccessRate { get; private set; } = 0.75f;
        public static float MajorAscensionSuccessRate { get; private set; } = 0.60f;
        public static float MinorAscensionFailRate { get; private set; } = 0.20f;
        public static float MajorAscensionFailRate { get; private set; } = 0.30f;

        public static float NianLiDangerThreshold { get; private set; } = 1000f;
        public static float NianLiConservationThreshold { get; private set; } = 5000f;
        public static bool EnableNianLiAILimiter { get; private set; } = true;
        public static float KarmaCleanseCostPerPoint { get; private set; } = 500f;
        public static float LawLoopholeMultiplier { get; private set; } = 1.0f;

        // ★★★ 上四境限制（新增大通境） ★★★
        public static int MaxGreatRealm { get; private set; } = 0;
        public static int MaxRingRealm { get; private set; } = 0;
        public static int MaxUpholdRealm { get; private set; } = 0;
        public static int MaxTranscendent { get; private set; } = 0;

        // ============================================================
        // ★ 配置项注册表 ★
        // ============================================================
        private class ConfigEntry
        {
            public string Category;
            public string Key;
            public Action<float> OnFloat;
            public Action<int> OnInt;
            public Action<bool> OnBool;
            public string DefaultValue;
        }

        private static Dictionary<string, ConfigEntry> _entries = new Dictionary<string, ConfigEntry>();
        private static bool _isLoaded = false;

        // ============================================================
        // ★ 静态构造函数 ★
        // ============================================================
        static ConfigCache()
        {
            // 境界阈值
            RegisterFloat("境界阈值", "RealmThreshold1", v => RealmThresholds[1] = v, "1000");
            RegisterFloat("境界阈值", "RealmThreshold2", v => RealmThresholds[2] = v, "5000");
            RegisterFloat("境界阈值", "RealmThreshold3", v => RealmThresholds[3] = v, "20000");
            RegisterFloat("境界阈值", "RealmThreshold4", v => RealmThresholds[4] = v, "100000");
            RegisterFloat("境界阈值", "RealmThreshold5", v => RealmThresholds[5] = v, "500000");

            // 境界念力上限
            RegisterFloat("境界念力上限", "RealmMaxNianLi1", v => RealmMaxNianLi[1] = v, "10000");
            RegisterFloat("境界念力上限", "RealmMaxNianLi2", v => RealmMaxNianLi[2] = v, "100000");
            RegisterFloat("境界念力上限", "RealmMaxNianLi3", v => RealmMaxNianLi[3] = v, "1000000");
            RegisterFloat("境界念力上限", "RealmMaxNianLi4", v => RealmMaxNianLi[4] = v, "5000000");
            RegisterFloat("境界念力上限", "RealmMaxNianLi5", v => RealmMaxNianLi[5] = v, "10000000");

            // 基础设定
            RegisterFloat("基础设定", "BaseGrowth", v => { if (v > 0) BaseGrowth = v; }, "100");
            RegisterFloat("基础设定", "InsightChance", v => InsightChance = Mathf.Clamp01(v), "0.02");

            // 突破概率
            RegisterFloat("突破概率", "BreakthroughChance_1", v => BreakthroughChances[0] = Mathf.Clamp01(v), "0.70");
            RegisterFloat("突破概率", "BreakthroughChance_2", v => BreakthroughChances[1] = Mathf.Clamp01(v), "0.55");
            RegisterFloat("突破概率", "BreakthroughChance_3", v => BreakthroughChances[2] = Mathf.Clamp01(v), "0.40");
            RegisterFloat("突破概率", "BreakthroughChance_4", v => BreakthroughChances[3] = Mathf.Clamp01(v), "0.25");
            RegisterFloat("突破概率", "BreakthroughChance_5", v => BreakthroughChances[4] = Mathf.Clamp01(v), "0.15");

            // 突破死亡概率
            RegisterFloat("突破死亡概率", "BreakthroughDeathChance_1", v => BreakthroughDeathChances[0] = Mathf.Clamp01(v), "0.08");
            RegisterFloat("突破死亡概率", "BreakthroughDeathChance_2", v => BreakthroughDeathChances[1] = Mathf.Clamp01(v), "0.12");
            RegisterFloat("突破死亡概率", "BreakthroughDeathChance_3", v => BreakthroughDeathChances[2] = Mathf.Clamp01(v), "0.18");
            RegisterFloat("突破死亡概率", "BreakthroughDeathChance_4", v => BreakthroughDeathChances[3] = Mathf.Clamp01(v), "0.25");
            RegisterFloat("突破死亡概率", "BreakthroughDeathChance_5", v => BreakthroughDeathChances[4] = Mathf.Clamp01(v), "0.35");

            // 突破前路断绝概率
            RegisterFloat("突破前路断绝概率", "BreakthroughNoWayChance_1", v => BreakthroughNoWayChances[0] = Mathf.Clamp01(v), "0.08");
            RegisterFloat("突破前路断绝概率", "BreakthroughNoWayChance_2", v => BreakthroughNoWayChances[1] = Mathf.Clamp01(v), "0.12");
            RegisterFloat("突破前路断绝概率", "BreakthroughNoWayChance_3", v => BreakthroughNoWayChances[2] = Mathf.Clamp01(v), "0.18");
            RegisterFloat("突破前路断绝概率", "BreakthroughNoWayChance_4", v => BreakthroughNoWayChances[3] = Mathf.Clamp01(v), "0.25");
            RegisterFloat("突破前路断绝概率", "BreakthroughNoWayChance_5", v => BreakthroughNoWayChances[4] = Mathf.Clamp01(v), "1.00");

            // 冥想
            RegisterFloat("冥想", "MeditationMultiplier", v => { if (v >= 1f) MeditationMultiplier = v; }, "3.0");
            RegisterFloat("冥想", "MeditationRequireInterval", v => { if (v >= 0.5f) MeditationRequireInterval = v; }, "5");

            // 驻足
            RegisterFloat("驻足", "StayingBonus", v => { if (v >= 1f) StayingBonus = v; }, "1.5");
            RegisterFloat("驻足", "StayingDurationMin", v => { if (v >= 1f) StayingDurationMin = v; }, "20");
            RegisterFloat("驻足", "StayingDurationMax", v => { if (v >= StayingDurationMin) StayingDurationMax = v; }, "50");
            RegisterInt("驻足", "StayingMinRealm", v => StayingMinRealm = Mathf.Clamp(v, 1, 5), "3");

            // 法则
            RegisterFloat("法则", "CycleLawGrowthRate", v => CycleLawGrowthRate = Mathf.Clamp01(v), "0.02");

            // 境界技能消耗
            RegisterFloat("境界技能消耗", "SkillCost_0", v => RealmSkillCostPercent[0] = Mathf.Clamp01(v), "0");
            RegisterFloat("境界技能消耗", "SkillCost_1", v => RealmSkillCostPercent[1] = Mathf.Clamp01(v), "0.05");
            RegisterFloat("境界技能消耗", "SkillCost_2", v => RealmSkillCostPercent[2] = Mathf.Clamp01(v), "0.08");
            RegisterFloat("境界技能消耗", "SkillCost_3", v => RealmSkillCostPercent[3] = Mathf.Clamp01(v), "0.10");
            RegisterFloat("境界技能消耗", "SkillCost_4", v => RealmSkillCostPercent[4] = Mathf.Clamp01(v), "0.12");
            RegisterFloat("境界技能消耗", "SkillCost_5", v => RealmSkillCostPercent[5] = Mathf.Clamp01(v), "0.15");

            // 战斗
            RegisterFloat("战斗", "NianLiPerAttack", v => { if (v >= 0) NianLiPerAttack = v; }, "50");
            RegisterFloat("战斗", "NianLiPerKill", v => { if (v >= 0) NianLiPerKill = v; }, "1000");

            // 护盾
            RegisterFloat("护盾", "ShieldRegenRate", v => { if (v >= 0) ShieldRegenRate = v; }, "50");
            RegisterFloat("护盾", "ShieldConsumeRate", v => ShieldConsumeRate = Mathf.Clamp01(v), "0.1");

            // 阳劫
            RegisterInt("阳劫", "MinorAscensionSteps", v => { if (v >= 1) MinorAscensionSteps = v; }, "5");
            RegisterInt("阳劫", "MajorAscensionSteps", v => { if (v >= 1) MajorAscensionSteps = v; }, "9");
            RegisterFloat("阳劫", "MinorAscensionSuccessRate", v => MinorAscensionSuccessRate = Mathf.Clamp01(v), "0.75");
            RegisterFloat("阳劫", "MajorAscensionSuccessRate", v => MajorAscensionSuccessRate = Mathf.Clamp01(v), "0.60");
            RegisterFloat("阳劫", "MinorAscensionFailRate", v => MinorAscensionFailRate = Mathf.Clamp01(v), "0.20");
            RegisterFloat("阳劫", "MajorAscensionFailRate", v => MajorAscensionFailRate = Mathf.Clamp01(v), "0.30");

            // AI行为限制
            RegisterFloat("AI行为限制", "DangerThreshold", v => { if (v >= 0) NianLiDangerThreshold = v; }, "1000");
            RegisterFloat("AI行为限制", "ConservationThreshold", v => { if (v >= 0) NianLiConservationThreshold = v; }, "5000");

            // 业力
            RegisterFloat("业力", "KarmaCleanseCostPerPoint", v => { if (v >= 1) KarmaCleanseCostPerPoint = v; }, "500");

            // 念力修行设置
            RegisterFloat("念力修行设置", "LawLoopholeMultiplier", v => { if (v >= 0) LawLoopholeMultiplier = v; }, "1.0");

            // ★★★ 上四境限制 ★★★
            RegisterInt("上四境限制", "MaxGreatRealm", v => MaxGreatRealm = Mathf.Max(0, v), "0");
            RegisterInt("上四境限制", "MaxRingRealm", v => MaxRingRealm = Mathf.Max(0, v), "0");
            RegisterInt("上四境限制", "MaxUpholdRealm", v => MaxUpholdRealm = Mathf.Max(0, v), "0");
            RegisterInt("上四境限制", "MaxTranscendent", v => MaxTranscendent = Mathf.Max(0, v), "0");

            // 开关类
            RegisterBool("开关", "AutoFavoriteRealm3", v => AutoFavoriteRealm3 = v, "true");
            RegisterBool("开关", "AutoFavoriteRealm4", v => AutoFavoriteRealm4 = v, "true");
            RegisterBool("开关", "AutoFavoriteRealm5", v => AutoFavoriteRealm5 = v, "true");
            RegisterBool("开关", "BlockBreakthroughRealm3", v => BlockBreakthroughRealm3 = v, "false");
            RegisterBool("开关", "BlockBreakthroughRealm4", v => BlockBreakthroughRealm4 = v, "false");
            RegisterBool("开关", "BlockBreakthroughRealm5", v => BlockBreakthroughRealm5 = v, "false");
            RegisterBool("开关", "EnableDebugLog", v => EnableDebugLog = v, "false");
            RegisterBool("开关", "InfiniteResurrection", v => InfiniteResurrection = v, "true");
            RegisterBool("开关", "EnableManaConvert", v => EnableManaConvert = v, "true");
            RegisterBool("开关", "UseHardMode", v => UseHardMode = v, "false");
            RegisterBool("开关", "EnableAutoGC", v => EnableAutoGC = v, "true");
            RegisterBool("开关", "EnableNianLiAILimiter", v => EnableNianLiAILimiter = v, "true");
        }

        // ============================================================
        // ★ 注册辅助 ★
        // ============================================================
        private static void RegisterFloat(string category, string key, Action<float> callback, string defaultValue)
        {
            _entries[$"{category}.{key}"] = new ConfigEntry
            {
                Category = category,
                Key = key,
                OnFloat = callback,
                DefaultValue = defaultValue
            };
        }

        private static void RegisterInt(string category, string key, Action<int> callback, string defaultValue)
        {
            _entries[$"{category}.{key}"] = new ConfigEntry
            {
                Category = category,
                Key = key,
                OnInt = callback,
                DefaultValue = defaultValue
            };
        }

        private static void RegisterBool(string category, string key, Action<bool> callback, string defaultValue)
        {
            _entries[$"{category}.{key}"] = new ConfigEntry
            {
                Category = category,
                Key = key,
                OnBool = callback,
                DefaultValue = defaultValue
            };
        }

        // ============================================================
        // ★ 安全加载 ★
        // ============================================================
        public static void LoadFromConfig(ModConfig config)
        {
            if (config == null)
            {
                Debug.Log("[念力修行] ModConfig 为空，使用默认配置");
                return;
            }

            int loadedCount = 0;
            int missingCount = 0;
            int errorCount = 0;

            foreach (var entry in _entries.Values)
            {
                try
                {
                    Dictionary<string, ModConfigItem> categoryDict = null;
                    try
                    {
                        categoryDict = config[entry.Category];
                    }
                    catch (KeyNotFoundException)
                    {
                        if (EnableDebugLog)
                            Debug.Log($"[念力修行] 配置分类 \"{entry.Category}\" 不存在，使用默认值 ({entry.DefaultValue})");
                        missingCount++;
                        continue;
                    }

                    if (categoryDict == null)
                    {
                        if (EnableDebugLog)
                            Debug.Log($"[念力修行] 配置分类 \"{entry.Category}\" 为空，使用默认值 ({entry.DefaultValue})");
                        missingCount++;
                        continue;
                    }

                    if (!categoryDict.TryGetValue(entry.Key, out var item))
                    {
                        if (EnableDebugLog)
                            Debug.Log($"[念力修行] 配置键 \"{entry.Category}.{entry.Key}\" 不存在，使用默认值 ({entry.DefaultValue})");
                        missingCount++;
                        continue;
                    }

                    string typeStr = item.Type.ToString();

                    if (entry.OnFloat != null && (typeStr == "SLIDER" || typeStr == "TEXT"))
                    {
                        float val = typeStr == "SLIDER" ? item.FloatVal : float.Parse(item.TextVal);
                        entry.OnFloat(val);
                        loadedCount++;
                    }
                    else if (entry.OnInt != null && typeStr == "TEXT")
                    {
                        if (int.TryParse(item.TextVal, out int i))
                        {
                            entry.OnInt(i);
                            loadedCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"[念力修行] 配置项 \"{entry.Category}.{entry.Key}\" 的 TextVal \"{item.TextVal}\" 无法解析为整数，使用默认值 ({entry.DefaultValue})");
                        }
                    }
                    else if (entry.OnBool != null && typeStr == "SWITCH")
                    {
                        entry.OnBool(item.BoolVal);
                        loadedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[念力修行] 配置项 \"{entry.Category}.{entry.Key}\" 类型 \"{typeStr}\" 无法匹配，使用默认值 ({entry.DefaultValue})");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Debug.LogError($"[念力修行] 加载配置项 \"{entry.Category}.{entry.Key}\" 异常: {ex.Message}");
                }
            }

            // 同步 AI 限流器
            NianLiAILimiter.DangerThreshold = NianLiDangerThreshold;
            NianLiAILimiter.ConservationThreshold = NianLiConservationThreshold;

            _isLoaded = true;
            Debug.Log($"[念力修行] 配置加载完成：成功 {loadedCount} 项，缺失 {missingCount} 项，错误 {errorCount} 项");
        }

        // ============================================================
        // ★ 其他方法 ★
        // ============================================================
        public static void ReloadConfig()
        {
            var config = NianLiXiuXingClass.Instance?.GetConfig();
            if (config != null)
            {
                LoadFromConfig(config);
                Debug.Log("[念力修行] 配置热重载完成");
            }
            else
            {
                Debug.LogWarning("[念力修行] 热重载失败：无法获取配置");
            }
        }

        public static bool IsAutoFavoriteRealm(int realm)
        {
            switch (realm)
            {
                case 3: return AutoFavoriteRealm3;
                case 4: return AutoFavoriteRealm4;
                case 5: return AutoFavoriteRealm5;
                default: return true;
            }
        }

        public static bool IsBlockBreakthroughRealm(int realm)
        {
            switch (realm)
            {
                case 3: return BlockBreakthroughRealm3;
                case 4: return BlockBreakthroughRealm4;
                case 5: return BlockBreakthroughRealm5;
                default: return false;
            }
        }

        public static bool IsLoaded() => _isLoaded;

        // ============================================================
        // ★ 回调方法（供 ModConfig UI 使用） ★
        // ============================================================
        public static void OnAutoFavoriteRealm3Changed(bool v) => AutoFavoriteRealm3 = v;
        public static void OnAutoFavoriteRealm4Changed(bool v) => AutoFavoriteRealm4 = v;
        public static void OnAutoFavoriteRealm5Changed(bool v) => AutoFavoriteRealm5 = v;
        public static void OnBlockBreakthroughRealm3Changed(bool v) => BlockBreakthroughRealm3 = v;
        public static void OnBlockBreakthroughRealm4Changed(bool v) => BlockBreakthroughRealm4 = v;
        public static void OnBlockBreakthroughRealm5Changed(bool v) => BlockBreakthroughRealm5 = v;
        public static void OnDebugLogChanged(bool v) => EnableDebugLog = v;
        public static void OnInfiniteResurrectionChanged(bool v) => InfiniteResurrection = v;
        public static void OnManaConvertChanged(bool v) => EnableManaConvert = v;
        public static void OnDifficultyModeChanged(bool v) => UseHardMode = v;
        public static void OnLawLoopholeMultiplierChanged(float v) => LawLoopholeMultiplier = Mathf.Max(0f, v);
        public static void OnEnableAutoGCChanged(bool value) => EnableAutoGC = value;
        public static void OnEnableNianLiAILimiterChanged(bool value) => EnableNianLiAILimiter = value;

        // ★★★ 上四境限制回调（float 版本，供 LoadFromConfig 内部使用） ★★★
        public static void OnMaxGreatRealmChanged(float value) => MaxGreatRealm = Mathf.RoundToInt(Mathf.Max(0, value));
        public static void OnMaxRingRealmChanged(float value) => MaxRingRealm = Mathf.RoundToInt(Mathf.Max(0, value));
        public static void OnMaxUpholdRealmChanged(float value) => MaxUpholdRealm = Mathf.RoundToInt(Mathf.Max(0, value));
        public static void OnMaxTranscendentChanged(float value) => MaxTranscendent = Mathf.RoundToInt(Mathf.Max(0, value));

        // ★★★ 上四境限制回调（string 版本，供 ModConfig UI 调用 TEXT 类型） ★★★
        public static void OnMaxGreatRealmChanged(string value)
        {
            if (int.TryParse(value, out int result))
                MaxGreatRealm = Mathf.Max(0, result);
            else
                Debug.LogWarning($"[念力修行] MaxGreatRealm 值 \"{value}\" 无效，保持当前值 {MaxGreatRealm}");
        }

        public static void OnMaxRingRealmChanged(string value)
        {
            if (int.TryParse(value, out int result))
                MaxRingRealm = Mathf.Max(0, result);
            else
                Debug.LogWarning($"[念力修行] MaxRingRealm 值 \"{value}\" 无效，保持当前值 {MaxRingRealm}");
        }

        public static void OnMaxUpholdRealmChanged(string value)
        {
            if (int.TryParse(value, out int result))
                MaxUpholdRealm = Mathf.Max(0, result);
            else
                Debug.LogWarning($"[念力修行] MaxUpholdRealm 值 \"{value}\" 无效，保持当前值 {MaxUpholdRealm}");
        }

        public static void OnMaxTranscendentChanged(string value)
        {
            if (int.TryParse(value, out int result))
                MaxTranscendent = Mathf.Max(0, result);
            else
                Debug.LogWarning($"[念力修行] MaxTranscendent 值 \"{value}\" 无效，保持当前值 {MaxTranscendent}");
        }
    }
}