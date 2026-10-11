using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.UI;     // ★ 新增
using NeoModLoader.General;      // ★ 新增

namespace NianLiXiuXing.code.Systems
{
    public enum ArtifactRarity { Common, Uncommon, Rare, Epic, Legendary }

    public class Artifact
    {
        public string Name;
        public ArtifactRarity Rarity;
        public float AttackBonus;
        public float DefenseBonus;
        public float HealthBonus;
        public float SpeedBonus;
        public string SpecialEffect;

        public Artifact(string name, ArtifactRarity rarity, float atk, float def, float hp, float spd, string effect = "")
        {
            Name = name;
            Rarity = rarity;
            AttackBonus = atk;
            DefenseBonus = def;
            HealthBonus = hp;
            SpeedBonus = spd;
            SpecialEffect = effect;
        }
    }

    public static class RefiningSystem
    {
        private const string ARTIFACT_KEY = "nianli.artifact";

        private static readonly string[] Prefixes = {
            "青木", "玄铁", "紫金", "灵玉", "混沌", "乾坤", "星辰", "血魔", "太虚", "无相",
            "九转", "万象", "雷霆", "寒冰", "烈火", "疾风", "厚土", "明月", "旭日", "龙鳞",
            "凤羽", "虎魄", "龟甲", "麒麟", "天罡", "地煞", "阴阳", "五行", "八卦", "太极"
        };

        private static readonly string[] Suffixes = {
            "剑", "盾", "佩", "葫芦", "环", "钟", "刃", "尺", "镜", "珠", "印", "令",
            "幡", "幢", "杖", "钺", "钩", "叉", "鞭", "锏", "锤", "抓", "拐", "戟", "斧"
        };

        private static string GenerateRandomName()
        {
            string prefix = Prefixes[Random.Range(0, Prefixes.Length)];
            string suffix = Suffixes[Random.Range(0, Suffixes.Length)];
            return prefix + suffix;
        }

        private static (float atk, float def, float hp, float spd) GetStatsForRarity(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common:   return (0.10f, 0.08f, 0.05f, 0.02f);
                case ArtifactRarity.Uncommon: return (0.15f, 0.12f, 0.08f, 0.05f);
                case ArtifactRarity.Rare:     return (0.20f, 0.18f, 0.12f, 0.08f);
                case ArtifactRarity.Epic:     return (0.28f, 0.25f, 0.18f, 0.12f);
                case ArtifactRarity.Legendary:return (0.40f, 0.35f, 0.25f, 0.20f);
                default: return (0f, 0f, 0f, 0f);
            }
        }

        private static string GetSpecialEffect(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common:   return LM.Get("artifact_effect_common");
                case ArtifactRarity.Uncommon: return LM.Get("artifact_effect_uncommon");
                case ArtifactRarity.Rare:     return LM.Get("artifact_effect_rare");
                case ArtifactRarity.Epic:     return LM.Get("artifact_effect_epic");
                case ArtifactRarity.Legendary:return LM.Get("artifact_effect_legendary");
                default: return "";
            }
        }

        // ★★★ 供 RandomEventsSystem 使用 ★★★
        public static List<Artifact> GenerateRandomArtifactList(int count)
        {
            var list = new List<Artifact>();
            for (int i = 0; i < count; i++)
            {
                // 随机稀有度（偏向低稀有度）
                ArtifactRarity rarity;
                float roll = Random.Range(0f, 1f);
                if (roll < 0.40f) rarity = ArtifactRarity.Common;
                else if (roll < 0.70f) rarity = ArtifactRarity.Uncommon;
                else if (roll < 0.90f) rarity = ArtifactRarity.Rare;
                else if (roll < 0.98f) rarity = ArtifactRarity.Epic;
                else rarity = ArtifactRarity.Legendary;

                var stats = GetStatsForRarity(rarity);
                string name = GenerateRandomName();
                string effect = GetSpecialEffect(rarity);
                list.Add(new Artifact(name, rarity, stats.atk, stats.def, stats.hp, stats.spd, effect));
            }
            return list;
        }

        public static Artifact GetArtifact(Actor actor)
        {
            if (actor == null) return null;
            actor.data.get(ARTIFACT_KEY, out string json, "");
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var parts = json.Split('|');
                if (parts.Length < 6) return null;
                var rarity = (ArtifactRarity)int.Parse(parts[1]);
                float atk = float.Parse(parts[2]);
                float def = float.Parse(parts[3]);
                float hp = float.Parse(parts[4]);
                float spd = float.Parse(parts[5]);
                string effect = parts.Length > 6 ? parts[6] : "";
                return new Artifact(parts[0], rarity, atk, def, hp, spd, effect);
            }
            catch { return null; }
        }

        public static void SetArtifact(Actor actor, Artifact artifact)
        {
            if (actor == null) return;
            string data = $"{artifact.Name}|{(int)artifact.Rarity}|{artifact.AttackBonus}|{artifact.DefenseBonus}|{artifact.HealthBonus}|{artifact.SpeedBonus}|{artifact.SpecialEffect}";
            actor.data.set(ARTIFACT_KEY, data);
            NianLiInfoPanel.MarkDirty();
        }

        public static void RemoveArtifact(Actor actor)
        {
            if (actor == null) return;
            actor.data.set(ARTIFACT_KEY, "");
            NianLiInfoPanel.MarkDirty();
        }

        public static bool RefineArtifact(Actor actor)
        {
            if (actor == null) return false;
            if (GetArtifact(actor) != null)
            {
                WorldTip.showNow(LM.Get("refining_already_has"), true, "top");
                return false;
            }

            int realm = actor.GetRealmLevel();
            float cost = 5000f + realm * 5000f;
            if (actor.GetNianLi() < cost)
            {
                WorldTip.showNow(string.Format(LM.Get("refining_not_enough_nianli"), cost), true, "top");
                return false;
            }
            actor.ChangeNianLi(-cost);

            float successRate = 0.40f + actor.GetInsightLevel() * 0.06f + realm * 0.05f;
            successRate = Mathf.Min(successRate, 0.85f);

            if (Random.Range(0f, 1f) > successRate)
            {
                WorldTip.showNow(LM.Get("refining_fail"), true, "top");
                return false;
            }

            ArtifactRarity rarity;
            float roll = Random.Range(0f, 1f);
            if (roll < 0.10f && realm >= 4)       rarity = ArtifactRarity.Legendary;
            else if (roll < 0.25f && realm >= 3)  rarity = ArtifactRarity.Epic;
            else if (roll < 0.50f && realm >= 2)  rarity = ArtifactRarity.Rare;
            else if (roll < 0.75f)                rarity = ArtifactRarity.Uncommon;
            else                                  rarity = ArtifactRarity.Common;

            var stats = GetStatsForRarity(rarity);
            string name = GenerateRandomName();
            string effect = GetSpecialEffect(rarity);

            Artifact newArtifact = new Artifact(name, rarity, stats.atk, stats.def, stats.hp, stats.spd, effect);
            SetArtifact(actor, newArtifact);

            WorldTip.showNow(string.Format(LM.Get("refining_success"), name, LM.Get("ArtifactRarity" + rarity.ToString())), true, "top");
            DaoBeiSystem.AddRecord(actor, "炼器", string.Format(LM.Get("refining_daobei"), name), new Color(0.5f, 0.8f, 1f));
            return true;
        }

        public static void ApplyArtifactBonus(Actor actor, ref float attack, ref float defense, ref float health, ref float speed)
        {
            var artifact = GetArtifact(actor);
            if (artifact == null) return;
            attack *= (1f + artifact.AttackBonus);
            defense *= (1f + artifact.DefenseBonus);
            health *= (1f + artifact.HealthBonus);
            speed *= (1f + artifact.SpeedBonus);
        }
    }
}