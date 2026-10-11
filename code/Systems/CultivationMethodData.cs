using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Core
{
    public class MethodData
    {
        public CultivationMethod Method { get; set; }
        public string NameKey { get; set; }
        public string DescKey { get; set; }
        public string ColorHex { get; set; }
        public MethodRarity Rarity { get; set; }
        public int UnlockRealm { get; set; }
        public float SpeedBonus { get; set; }
        public float AttackMod { get; set; } = 1f;
        public float DefenseMod { get; set; } = 1f;
        public float HealthMod { get; set; } = 1f;
        public string SpecialEffectKey { get; set; }
    }

    /// <summary>
    /// 功法静态数据定义与查表（单一职责：只负责数据与元数据映射，不含运行时缓存/业务逻辑）。
    /// </summary>
    public static partial class CultivationMethodSystem
    {
        // ============================================================
        // ★★★ 功法完整列表 ★★★
        // ============================================================
        public static readonly List<MethodData> AllMethods = new List<MethodData>
        {
            new MethodData { Method = CultivationMethod.None,        NameKey = "method_name_none",   DescKey = "method_desc_none",   ColorHex = "#888888", Rarity = MethodRarity.Common,     UnlockRealm = 0, SpeedBonus = 0f,    AttackMod = 1f, DefenseMod = 1f, HealthMod = 1f, SpecialEffectKey = "method_effect_none"  },
            new MethodData { Method = CultivationMethod.FenTianJue,  NameKey = "method_name_fentian",DescKey = "method_desc_fentian",ColorHex = "#FF6B6B", Rarity = MethodRarity.Common,     UnlockRealm = 3, SpeedBonus = 0.3f,  AttackMod = 1.5f,DefenseMod = 0.8f,HealthMod = 1f, SpecialEffectKey = "method_effect_fentian" },
            new MethodData { Method = CultivationMethod.BuMieTi,     NameKey = "method_name_bumie",  DescKey = "method_desc_bumie",  ColorHex = "#4CAF50", Rarity = MethodRarity.Common,     UnlockRealm = 3, SpeedBonus = 0f,    AttackMod = 0.8f,DefenseMod = 1.5f,HealthMod = 1.5f,SpecialEffectKey = "method_effect_bumie"  },
            new MethodData { Method = CultivationMethod.WanXiangJing,  NameKey = "method_name_wanxiang",DescKey= "method_desc_wanxiang",ColorHex="#FFD700", Rarity = MethodRarity.Common,     UnlockRealm = 3, SpeedBonus = 0.2f,  AttackMod = 1.15f,DefenseMod= 1.15f,HealthMod= 1.15f,SpecialEffectKey="method_effect_wanxiang" },
            new MethodData { Method = CultivationMethod.YiNianJue,    NameKey = "method_name_yinian", DescKey = "method_desc_yinian", ColorHex = "#9B59B6", Rarity = MethodRarity.Common,     UnlockRealm = 3, SpeedBonus = 1.0f,  AttackMod = 0.7f,DefenseMod = 0.7f,HealthMod = 0.7f,SpecialEffectKey = "method_effect_yinian" },
            new MethodData { Method = CultivationMethod.JiuZhuanGong,  NameKey = "method_name_jiuzhuan",DescKey="method_desc_jiuzhuan",ColorHex = "#00BFFF", Rarity = MethodRarity.Common,     UnlockRealm = 3, SpeedBonus = -0.2f, AttackMod = 1f,DefenseMod = 1f,HealthMod = 1f, SpecialEffectKey = "method_effect_jiuzhuan" },
            new MethodData { Method = CultivationMethod.LeiTingJue,    NameKey = "method_name_leiting",DescKey = "method_desc_leiting",ColorHex = "#7B68EE", Rarity = MethodRarity.Uncommon,   UnlockRealm = 3, SpeedBonus = 0.1f,  AttackMod = 1.3f,DefenseMod = 1f,HealthMod = 1f, SpecialEffectKey = "method_effect_leiting" },
            new MethodData { Method = CultivationMethod.XuanBingJue,   NameKey = "method_name_xuanbing",DescKey="method_desc_xuanbing",ColorHex="#87CEEB", Rarity = MethodRarity.Uncommon,   UnlockRealm = 3, SpeedBonus = 0.05f, AttackMod = 1f, DefenseMod = 1.2f,HealthMod = 1.1f,SpecialEffectKey="method_effect_xuanbing" },
            new MethodData { Method = CultivationMethod.XingChenJue,  NameKey = "method_name_xingchen",DescKey="method_desc_xingchen",ColorHex="#FFD700", Rarity = MethodRarity.Uncommon,   UnlockRealm = 3, SpeedBonus = 0.15f, AttackMod = 1f, DefenseMod = 1f,HealthMod = 1f, SpecialEffectKey="method_effect_xingchen" },
            new MethodData { Method = CultivationMethod.XueMoJue,      NameKey = "method_name_xuemo",  DescKey = "method_desc_xuemo",  ColorHex = "#DC143C", Rarity = MethodRarity.Rare,       UnlockRealm = 4, SpeedBonus = 0.5f,  AttackMod = 1.4f,DefenseMod = 1f,HealthMod = 0.8f,SpecialEffectKey = "method_effect_xuemo"  },
            new MethodData { Method = CultivationMethod.TaiXuJue,      NameKey = "method_name_taixu",  DescKey = "method_desc_taixu", ColorHex = "#DA70D6", Rarity = MethodRarity.Rare,       UnlockRealm = 4, SpeedBonus = -0.1f, AttackMod = 1f, DefenseMod = 1f,HealthMod = 1f, SpecialEffectKey = "method_effect_taixu"  },
            new MethodData { Method = CultivationMethod.WuXiangJue,   NameKey = "method_name_wuxiang",DescKey= "method_desc_wuxiang",ColorHex="#F0E68C", Rarity = MethodRarity.Rare,       UnlockRealm = 4, SpeedBonus = 0.25f,  AttackMod = 1.1f,DefenseMod= 1.1f,HealthMod= 1.1f,SpecialEffectKey="method_effect_wuxiang"  },
            new MethodData { Method = CultivationMethod.HunDunJue,      NameKey = "method_name_hundun", DescKey = "method_desc_hundun",ColorHex = "#8B008B", Rarity = MethodRarity.Legendary, UnlockRealm = 5, SpeedBonus = 0.8f,   AttackMod = 1.2f,DefenseMod = 1.2f,HealthMod= 1.2f,SpecialEffectKey="method_effect_hundun"  }
        };

        // 字典索引：O(1) 按枚举值查表，替代原线性遍历
        private static readonly Dictionary<CultivationMethod, MethodData> _methodDataLookup = BuildMethodLookup();

        private static Dictionary<CultivationMethod, MethodData> BuildMethodLookup()
        {
            var dict = new Dictionary<CultivationMethod, MethodData>(AllMethods.Count);
            foreach (var data in AllMethods)
            {
                if (!dict.ContainsKey(data.Method))
                    dict[data.Method] = data;
            }
            return dict;
        }

        public static MethodData GetMethodData(CultivationMethod method)
        {
            return _methodDataLookup.TryGetValue(method, out var data) ? data : AllMethods[0];
        }

        // ============================================================
        // ★★★ 稀有度 / 境界元数据 ★★★
        // ============================================================
        public static string GetRarityName(MethodRarity rarity)
        {
            switch (rarity)
            {
                case MethodRarity.Common:    return LocalizedTextManager.getText("rarity_common");
                case MethodRarity.Uncommon:  return LocalizedTextManager.getText("rarity_uncommon");
                case MethodRarity.Rare:       return LocalizedTextManager.getText("rarity_rare");
                case MethodRarity.Epic:       return LocalizedTextManager.getText("rarity_epic");
                case MethodRarity.Legendary: return LocalizedTextManager.getText("rarity_legendary");
                default: return "Unknown";
            }
        }

        public static Color GetRarityColor(MethodRarity rarity)
        {
            switch (rarity)
            {
                case MethodRarity.Common:    return Color.white;
                case MethodRarity.Uncommon:  return new Color(0.2f, 0.8f, 0.2f);
                case MethodRarity.Rare:     return new Color(0.2f, 0.5f, 1f);
                case MethodRarity.Epic:     return new Color(0.7f, 0.2f, 0.8f);
                case MethodRarity.Legendary: return new Color(1f, 0.7f, 0f);
                default: return Color.gray;
            }
        }

        public static string GetRealmName(int realm)
        {
            string[] keys = { "realm_mortal", "realm_nurture", "realm_reflect", "realm_great", "realm_ring", "realm_uphold" };
            if (realm >= 0 && realm <= 5)
                return LocalizedTextManager.getText(keys[realm]);
            return "Unknown";
        }
    }
}
