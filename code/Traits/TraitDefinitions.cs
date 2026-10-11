using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Traits
{
    internal static class TraitDefinitions
    {
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            // ==========================================
            // 心关特质 (6个)
            // ==========================================
            for (int i = 0; i < 6; i++)
            {
                var trait = new ActorTrait();
                trait.id = $"Heart_{i}";
                trait.path_icon = $"ui/Heart{i}";
                trait.group_id = "Heart_Group";
                trait.can_be_given = true;
                trait.needs_to_be_explored = false;
                trait.base_stats = new BaseStats();

                trait.base_stats["HeartBonus"] = (0.3f + i * 0.5f) * 1.2f;

                switch (i)
                {
                    case 0:
                        trait.base_stats["damage"] = 5000f;
                        trait.base_stats["health"] = 50000f;
                        trait.base_stats["attack_speed"] = 500f;
                        trait.base_stats["NianLiAttack"] = 500f;
                        trait.base_stats["NianLiDefense"] = 200f;
                        break;
                    case 1:
                        trait.base_stats["damage"] = 15000f;
                        trait.base_stats["health"] = 150000f;
                        trait.base_stats["armor"] = 1000f;
                        trait.base_stats["attack_speed"] = 1000f;
                        trait.base_stats["NianLiAttack"] = 1500f;
                        trait.base_stats["NianLiDefense"] = 600f;
                        break;
                    case 2:
                        trait.base_stats["damage"] = 50000f;
                        trait.base_stats["health"] = 500000f;
                        trait.base_stats["armor"] = 3000f;
                        trait.base_stats["attack_speed"] = 2000f;
                        trait.base_stats["NianLiAttack"] = 5000f;
                        trait.base_stats["NianLiDefense"] = 2000f;
                        break;
                    case 3:
                        trait.base_stats["damage"] = 200000f;
                        trait.base_stats["health"] = 2000000f;
                        trait.base_stats["armor"] = 10000f;
                        trait.base_stats["attack_speed"] = 4000f;
                        trait.base_stats["critical_chance"] = 0.10f;
                        trait.base_stats["CriticalDamage"] = 0.5f;
                        trait.base_stats["NianLiAttack"] = 20000f;
                        trait.base_stats["NianLiDefense"] = 8000f;
                        break;
                    case 4:
                        trait.base_stats["damage"] = 1000000f;
                        trait.base_stats["health"] = 10000000f;
                        trait.base_stats["armor"] = 50000f;
                        trait.base_stats["attack_speed"] = 8000f;
                        trait.base_stats["speed"] = 1000f;
                        trait.base_stats["critical_chance"] = 0.20f;
                        trait.base_stats["CriticalDamage"] = 1.0f;
                        trait.base_stats["NianLiAttack"] = 100000f;
                        trait.base_stats["NianLiDefense"] = 40000f;
                        break;
                    case 5:
                        trait.base_stats["damage"] = 5000000f;
                        trait.base_stats["health"] = 50000000f;
                        trait.base_stats["armor"] = 250000f;
                        trait.base_stats["attack_speed"] = 15000f;
                        trait.base_stats["speed"] = 3000f;
                        trait.base_stats["mana"] = 100000f;
                        trait.base_stats["critical_chance"] = 0.35f;
                        trait.base_stats["CriticalDamage"] = 2.0f;
                        trait.base_stats["NianLiAttack"] = 500000f;
                        trait.base_stats["NianLiDefense"] = 200000f;
                        break;
                }

                AssetManager.traits.add(trait);
            }

            // ==========================================
            // 境界特质 (5个) — 生命值压缩到安全范围
            // ==========================================
            for (int i = 1; i <= 5; i++)
            {
                var trait = new ActorTrait();
                trait.id = $"Realm_{i}";
                trait.path_icon = $"ui/Realm{i}";
                trait.group_id = "Realm_Group";
                trait.can_be_given = true;
                trait.needs_to_be_explored = false;
                trait.base_stats = new BaseStats();

                trait.base_stats["HeartBonus"] = (0.5f + i * 0.8f) * 1.2f;

                float nianLiAtk = 1000f * Mathf.Pow(10f, i - 1);
                float nianLiDef = 500f * Mathf.Pow(10f, i - 1);
                trait.base_stats["NianLiAttack"] = nianLiAtk;
                trait.base_stats["NianLiDefense"] = nianLiDef;
                trait.base_stats["CriticalDamage"] = 0.1f + i * 0.15f;

                float[] lifespan = { 150f, 300f, 2000f, 10000f, 100000f };
                trait.base_stats["lifespan"] = lifespan[i - 1];

                // ---- ★ 生命值：控制在 int 安全范围内 ----
                float healthBase = 10000f * Mathf.Pow(10f, i - 1); // 10,000 → 100,000 → 1,000,000 → 10,000,000 → 100,000,000
                float damageBase = 1000f * Mathf.Pow(100f, i - 1);
                float armorBase = 200f * Mathf.Pow(100f, i - 1);
                float speedBase = 100f * Mathf.Pow(2f, i - 1);
                float atkSpeedBase = 500f * Mathf.Pow(2f, i - 1);
                float manaBase = 5000f * Mathf.Pow(10f, i - 1);
                float rangeBase = 50f * Mathf.Pow(2f, i - 1);
                float aoeBase = 50f * Mathf.Pow(2f, i - 1);
                float targetsBase = 10f * Mathf.Pow(2f, i - 1);
                float critChanceBase = 0.02f + i * 0.03f;

                trait.base_stats["health"] = healthBase;
                trait.base_stats["damage"] = damageBase;
                trait.base_stats["armor"] = armorBase;
                trait.base_stats["speed"] = speedBase;
                trait.base_stats["attack_speed"] = atkSpeedBase;
                trait.base_stats["mana"] = manaBase;
                trait.base_stats["range"] = rangeBase;
                trait.base_stats["area_of_effect"] = aoeBase;
                trait.base_stats["targets"] = targetsBase;
                trait.base_stats["critical_chance"] = critChanceBase;

                AssetManager.traits.add(trait);
            }

            // ==========================================
            // 超脱位格（托举境之上）
            // ==========================================
            var weiGe99 = new ActorTrait();
            weiGe99.id = TraitIds.WeiGe99;
            weiGe99.path_icon = "ui/WeiGe99";
            weiGe99.group_id = "WeiGe_Group";
            weiGe99.can_be_given = true;
            weiGe99.needs_to_be_explored = false;
            weiGe99.base_stats = new BaseStats();

            weiGe99.base_stats["HeartBonus"] = 20f;
            weiGe99.base_stats["NianLiAttack"] = 1000000000f;
            weiGe99.base_stats["NianLiDefense"] = 500000000f;
            weiGe99.base_stats["CriticalDamage"] = 5.0f;
            weiGe99.base_stats["health"] = 1000000000f;    // 10亿（安全）
            weiGe99.base_stats["damage"] = 100000000f;
            weiGe99.base_stats["armor"] = 10000000f;
            weiGe99.base_stats["range"] = 5000f;
            weiGe99.base_stats["area_of_effect"] = 5000f;
            weiGe99.base_stats["targets"] = 5000f;
            weiGe99.base_stats["speed"] = 5000f;
            weiGe99.base_stats["attack_speed"] = 5000f;
            weiGe99.base_stats["mana"] = 50000000f;
            weiGe99.base_stats["critical_chance"] = 0.8f;
            weiGe99.base_stats["lifespan"] = float.MaxValue;

            AssetManager.traits.add(weiGe99);

            // ==========================================
            // 准超脱（小阳劫成功标记）
            // ==========================================
            var minorTranscendence = new ActorTrait();
            minorTranscendence.id = TraitIds.MinorTranscendence;
            minorTranscendence.path_icon = "ui/MinorTranscendence";
            minorTranscendence.group_id = "WeiGe_Group";
            minorTranscendence.can_be_given = true;
            minorTranscendence.needs_to_be_explored = false;
            minorTranscendence.base_stats = new BaseStats();

            minorTranscendence.base_stats["HeartBonus"] = 3f;
            minorTranscendence.base_stats["NianLiAttack"] = 10000000f;
            minorTranscendence.base_stats["NianLiDefense"] = 5000000f;
            minorTranscendence.base_stats["CriticalDamage"] = 1.5f;
            minorTranscendence.base_stats["health"] = 100000000f;    // 1亿（安全）
            minorTranscendence.base_stats["damage"] = 10000000f;
            minorTranscendence.base_stats["armor"] = 1000000f;
            minorTranscendence.base_stats["speed"] = 1000f;
            minorTranscendence.base_stats["attack_speed"] = 2000f;
            minorTranscendence.base_stats["mana"] = 10000000f;
            minorTranscendence.base_stats["critical_chance"] = 0.3f;
            minorTranscendence.base_stats["lifespan"] = float.MaxValue;

            AssetManager.traits.add(minorTranscendence);

            // ==========================================
            // 前路断绝
            // ==========================================
            var noWayForward = new ActorTrait();
            noWayForward.id = TraitIds.Trait_NoWayForward;
            noWayForward.path_icon = "ui/Trait_NoWayForward";
            noWayForward.group_id = "NoWay_Group";
            noWayForward.can_be_given = true;
            noWayForward.needs_to_be_explored = false;
            noWayForward.base_stats = new BaseStats();
            noWayForward.base_stats["damage"] = -0.5f;
            AssetManager.traits.add(noWayForward);

            // ==========================================
            // 四大法则（保持不变）
            // ==========================================
            var lawReincarnation = new ActorTrait();
            lawReincarnation.id = TraitIds.Law_Reincarnation;
            lawReincarnation.path_icon = "ui/Law_Reincarnation";
            lawReincarnation.group_id = "Law_Group";
            lawReincarnation.can_be_given = true;
            lawReincarnation.needs_to_be_explored = false;
            lawReincarnation.base_stats = new BaseStats();
            lawReincarnation.base_stats["lifespan"] = 200f;
            lawReincarnation.base_stats["health"] = 5000f;
            AssetManager.traits.add(lawReincarnation);

            var lawYinYang = new ActorTrait();
            lawYinYang.id = TraitIds.Law_YinYang;
            lawYinYang.path_icon = "ui/Law_YinYang";
            lawYinYang.group_id = "Law_Group";
            lawYinYang.can_be_given = true;
            lawYinYang.needs_to_be_explored = false;
            lawYinYang.base_stats = new BaseStats();
            lawYinYang.base_stats["damage"] = 2500f;
            lawYinYang.base_stats["armor"] = 3000f;
            AssetManager.traits.add(lawYinYang);

            var lawTimeSpace = new ActorTrait();
            lawTimeSpace.id = TraitIds.Law_TimeSpace;
            lawTimeSpace.path_icon = "ui/Law_TimeSpace";
            lawTimeSpace.group_id = "Law_Group";
            lawTimeSpace.can_be_given = true;
            lawTimeSpace.needs_to_be_explored = false;
            lawTimeSpace.base_stats = new BaseStats();
            lawTimeSpace.base_stats["speed"] = 1000f;
            lawTimeSpace.base_stats["attack_speed"] = 2000f;
            AssetManager.traits.add(lawTimeSpace);

            var lawCauseEffect = new ActorTrait();
            lawCauseEffect.id = TraitIds.Law_CauseEffect;
            lawCauseEffect.path_icon = "ui/Law_CauseEffect";
            lawCauseEffect.group_id = "Law_Group";
            lawCauseEffect.can_be_given = true;
            lawCauseEffect.needs_to_be_explored = false;
            lawCauseEffect.base_stats = new BaseStats();
            lawCauseEffect.base_stats["critical_chance"] = 0.20f;
            lawCauseEffect.base_stats["damage"] = 2000f;
            lawCauseEffect.base_stats["CriticalDamage"] = 0.2f;
            AssetManager.traits.add(lawCauseEffect);

            // ==========================================
            // 小世界特质（四重）
            // ==========================================
            var smallWorld1 = new ActorTrait();
            smallWorld1.id = TraitIds.SmallWorld_1;
            smallWorld1.path_icon = "ui/SmallWorld_1";
            smallWorld1.group_id = "SmallWorld_Group";
            smallWorld1.can_be_given = true;
            smallWorld1.needs_to_be_explored = false;
            smallWorld1.base_stats = new BaseStats();
            smallWorld1.base_stats["health"] = 50000f;
            smallWorld1.base_stats["damage"] = 5000f;
            smallWorld1.base_stats["armor"] = 2000f;
            smallWorld1.base_stats["lifespan"] = 500f;
            AssetManager.traits.add(smallWorld1);

            var smallWorld2 = new ActorTrait();
            smallWorld2.id = TraitIds.SmallWorld_2;
            smallWorld2.path_icon = "ui/SmallWorld_2";
            smallWorld2.group_id = "SmallWorld_Group";
            smallWorld2.can_be_given = true;
            smallWorld2.needs_to_be_explored = false;
            smallWorld2.base_stats = new BaseStats();
            smallWorld2.base_stats["health"] = 200000f;
            smallWorld2.base_stats["damage"] = 20000f;
            smallWorld2.base_stats["armor"] = 8000f;
            smallWorld2.base_stats["lifespan"] = 1000f;
            smallWorld2.base_stats["attack_speed"] = 2000f;
            AssetManager.traits.add(smallWorld2);

            var smallWorld3 = new ActorTrait();
            smallWorld3.id = TraitIds.SmallWorld_3;
            smallWorld3.path_icon = "ui/SmallWorld_3";
            smallWorld3.group_id = "SmallWorld_Group";
            smallWorld3.can_be_given = true;
            smallWorld3.needs_to_be_explored = false;
            smallWorld3.base_stats = new BaseStats();
            smallWorld3.base_stats["health"] = 1000000f;
            smallWorld3.base_stats["damage"] = 100000f;
            smallWorld3.base_stats["armor"] = 40000f;
            smallWorld3.base_stats["lifespan"] = 2000f;
            smallWorld3.base_stats["attack_speed"] = 4000f;
            smallWorld3.base_stats["speed"] = 2000f;
            AssetManager.traits.add(smallWorld3);

            var smallWorld4 = new ActorTrait();
            smallWorld4.id = TraitIds.SmallWorld_4;
            smallWorld4.path_icon = "ui/SmallWorld_4";
            smallWorld4.group_id = "SmallWorld_Group";
            smallWorld4.can_be_given = true;
            smallWorld4.needs_to_be_explored = false;
            smallWorld4.base_stats = new BaseStats();
            smallWorld4.base_stats["health"] = 5000000f;
            smallWorld4.base_stats["damage"] = 500000f;
            smallWorld4.base_stats["armor"] = 200000f;
            smallWorld4.base_stats["lifespan"] = 4000f;
            smallWorld4.base_stats["attack_speed"] = 8000f;
            smallWorld4.base_stats["speed"] = 4000f;
            smallWorld4.base_stats["critical_chance"] = 0.1f;
            smallWorld4.base_stats["CriticalDamage"] = 0.5f;
            AssetManager.traits.add(smallWorld4);

            // ==========================================
            // 12种心灵天赋（health加成很小，无需修改）
            // ==========================================
// 道心破碎
// ==========================================
var brokenHeart = new ActorTrait();
brokenHeart.id = TraitIds.Trait_BrokenHeart;
brokenHeart.path_icon = "ui/Trait_BrokenHeart";  // 需要准备图标
brokenHeart.group_id = "NoWay_Group";  // 与前路断绝同组
brokenHeart.can_be_given = true;
brokenHeart.needs_to_be_explored = false;
brokenHeart.base_stats = new BaseStats();
brokenHeart.base_stats["HeartBonus"] = -0.5f;  // 修行速度 -50%

AssetManager.traits.add(brokenHeart);

            Debug.Log("[念力修行] 特质加载完成");
        }
    }
}