using UnityEngine;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public static class CombatExtensions
    {
        // ==========================================
        // 前路断绝百分比削弱
        // ==========================================

        public static float GetNoWayForwardPenalty(this Actor actor)
        {
            if (actor == null) return 1f;
            if (actor.hasTrait(TraitIds.Trait_NoWayForward))
            {
                return 0.5f;
            }
            return 1f;
        }

        public static bool HasNoWayForward(this Actor actor)
        {
            if (actor == null) return false;
            return actor.hasTrait(TraitIds.Trait_NoWayForward);
        }

        // ==================== 攻击力计算 ====================

        public static float GetAttackPower(this Actor actor)
        {
            if (actor == null || actor.data == null || !actor.isAlive()) return 0f;

            float baseAttack = Constants.RealmAttackBonus[actor.GetRealmLevel()];
            float nianLiBonus = actor.GetNianLi() / 50f;

            float heartBonus = 0f;
            if (actor.IsHeartBroken(0)) heartBonus += 5f;
            if (actor.IsHeartBroken(1)) heartBonus += 8f;
            if (actor.IsHeartBroken(2)) heartBonus += 10f;
            if (actor.IsHeartBroken(3)) heartBonus += 15f;
            if (actor.IsHeartBroken(4)) heartBonus += 20f;
            if (actor.IsHeartBroken(5)) heartBonus += 30f;

            float total = baseAttack + nianLiBonus + heartBonus;
            total *= actor.GetNoWayForwardPenalty();

            // ★ 小世界属性加成：初生+10% / 成长+25% / 融汇+50% / 完整+100%
            float swAtkBonus = GetSmallWorldCombatBonus(actor);
            total *= (1f + swAtkBonus);

            return total;
        }

        // ==================== 防御力计算 ====================

        public static float GetDefensePower(this Actor actor)
        {
            if (actor == null || actor.data == null || !actor.isAlive()) return 0f;

            float baseDefense = Constants.RealmDefenseBonus[actor.GetRealmLevel()];
            float heartBonus = 0f;
            if (actor.IsHeartBroken(2)) heartBonus += 20f;
            if (actor.IsHeartBroken(5)) heartBonus += 30f;

            float total = baseDefense + heartBonus;
            total *= actor.GetNoWayForwardPenalty();

            // ★ 小世界属性加成：初生+10% / 成长+25% / 融汇+50% / 完整+100%
            float swDefBonus = GetSmallWorldCombatBonus(actor);
            total *= (1f + swDefBonus);

            return total;
        }

        // ==================== 特殊伤害机制 ====================


        /// <summary>小世界提供的战斗属性加成倍率（按最高小世界等级）。</summary>
        public static float GetSmallWorldCombatBonus(this Actor actor)
        {
            if (actor == null) return 0f;
            if (actor.hasTrait(TraitIds.SmallWorld_4)) return 1.0f;
            if (actor.hasTrait(TraitIds.SmallWorld_3)) return 0.5f;
            if (actor.hasTrait(TraitIds.SmallWorld_2)) return 0.25f;
            if (actor.hasTrait(TraitIds.SmallWorld_1)) return 0.10f;
            return 0f;
        }

        public static float GetTrueDamageChance(this Actor actor)
        {
            if (actor == null || !actor.isAlive()) return 0f;
            if (actor.IsHeartBroken(5)) return 0.50f;
            return 0f;
        }

        public static bool HasLowHealthBonus(this Actor actor)
        {
            if (actor == null || !actor.isAlive() || !actor.IsHeartBroken(4)) return false;

            float healthPercent = actor.getHealth() / (float)actor.getMaxHealth();
            return healthPercent < 0.30f;
        }

        public static float GetSplashChance(this Actor actor)
        {
            if (actor == null || !actor.isAlive()) return 0f;
            if (actor.IsHeartBroken(3)) return 0.30f;
            return 0f;
        }

        public static float GetDamageReduction(this Actor actor)
        {
            if (actor == null || !actor.isAlive()) return 0f;
            if (actor.IsHeartBroken(2)) return 0.15f;
            return 0f;
        }

        public static float GetArmorPenetration(this Actor actor)
        {
            if (actor == null || !actor.isAlive()) return 0f;
            if (actor.IsHeartBroken(1)) return 0.10f;
            return 0f;
        }

        // ==================== ★ 完整伤害计算（含境界碾压） ====================

        public static float CalculateDamage(Actor attacker, Actor defender)
        {
            if (attacker == null || defender == null) return 0f;
            if (!attacker.isAlive() || !defender.isAlive()) return 0f;
            if (attacker.data == null || defender.data == null) return 0f;
            if (attacker.stats == null || defender.stats == null) return 0f;

            // ---- 基础攻防 ----
            float attack = attacker.GetAttackPower();
            float defense = defender.GetDefensePower();

            // ★ 阴阳法则：攻击者将自身 20% 防御转化为攻击
            if (CycleLawSystem.HasYinYangLaw(attacker))
            {
                float defenseToAttack = attacker.GetDefensePower() * 0.2f;
                attack += defenseToAttack;
            }

            // ★ 法宝加成
            float health = 0f;
            float speed = 0f;
            RefiningSystem.ApplyArtifactBonus(attacker, ref attack, ref defense, ref health, ref speed);

            // ---- 防御计算 ----
            float penetration = attacker.GetArmorPenetration();
            float effectiveDefense = defense * (1f - penetration);

            // ★ 因果法则：无视防御
            if (CycleLawSystem.HasCauseEffectLaw(attacker))
            {
                effectiveDefense = 0f;
            }

            float damage = Mathf.Max(1f, attack - effectiveDefense * 0.5f);

            // ---- 暴击 ----
            float critChance = attacker.GetCritChance();
            float critMult = attacker.GetCritDamageMultiplier();
            bool isCrit = Random.Range(0f, 1f) < critChance;
            attacker.data.set(DataKeys.LAST_CRIT, isCrit);
            if (isCrit) damage *= critMult;

            // ---- 真实伤害 ----
            if (Random.Range(0f, 1f) < attacker.GetTrueDamageChance())
            {
                damage = attack * 1.2f;
                attacker.data.set(DataKeys.LAST_TRUE_DAMAGE, true);
            }
            else
            {
                attacker.data.set(DataKeys.LAST_TRUE_DAMAGE, false);
            }

            // ---- 残血爆发 ----
            if (attacker.HasLowHealthBonus())
            {
                damage *= 2.0f;
                attacker.data.set(DataKeys.LAST_LOW_HEALTH_BONUS, true);
            }
            else
            {
                attacker.data.set(DataKeys.LAST_LOW_HEALTH_BONUS, false);
            }

            // ---- 防御减免 ----
            damage *= (1f - defender.GetDamageReduction());

            // ============================================================
            // ★★★ 境界碾压系统 ★★★
            // ============================================================

            int atkRealm = attacker.GetRealmLevel();
            int defRealm = defender.GetRealmLevel();
            int realmDiff = atkRealm - defRealm;

            // ─── 高境界打低境界：增伤 ───
            if (realmDiff > 0)
            {
                // 基础增伤：每级 +150%
                float realmBonus = 1f + realmDiff * 1.5f;
                damage *= realmBonus;

                // 额外碾压：境界差 ≥ 3 时，伤害再 ×3
                if (realmDiff >= 3)
                {
                    damage *= 3f;
                }

                // 境界差 ≥ 4 时，伤害再 ×10（秒杀级）
                if (realmDiff >= 4)
                {
                    damage *= 10f;
                }

                // 境界差 ≥ 2 时，低境界者护甲减半（破防效果）
                if (realmDiff >= 2)
                {
                    // 攻击方境界远高于防御方，防御方护甲减半
                    effectiveDefense *= 0.5f;
                }
            }

            // ─── 低境界打高境界：减伤 ───
            else if (realmDiff < 0)
            {
                int diff = Mathf.Abs(realmDiff);

                // 基础减伤：每级降低约 60% 伤害
                float realmPenalty = 1f / (1f + diff * 1.5f);
                damage *= Mathf.Max(realmPenalty, 0.05f);

                // 境界差 ≥ 2 时，低境界者攻击几乎无效（额外 -80%）
                if (diff >= 2)
                {
                    damage *= 0.2f;
                }

                // 境界差 ≥ 4 时，低境界者完全无法破防
                if (diff >= 4)
                {
                    damage *= 0.01f;  // 只有 1% 伤害
                }

                // 境界差 ≥ 3 时，附加“境界压制”状态（可选）
                if (diff >= 3)
                {
                    // 低境界者攻击命中率降低（模拟压制）
                    // 这里用伤害进一步降低体现
                    damage *= 0.5f;
                }
            }

            // ─── 额外：真我关无视部分碾压 ───
            if (attacker.IsHeartBroken(5) && realmDiff < 0)
            {
                // 真我关修士可以无视部分境界压制（减少一半惩罚）
                int diff = Mathf.Abs(realmDiff);
                float mitigation = Mathf.Min(diff * 0.2f, 0.5f);
                damage *= (1f + mitigation);
            }

            // ─── 额外：物我关对低境界者有额外压制 ───
            if (attacker.IsHeartBroken(4) && realmDiff > 0)
            {
                // 物我关修士对低境界者额外增加 50% 伤害
                damage *= 1.5f;
            }

            // 最终伤害保底（但不能为0，防止完全无伤害）
            damage = Mathf.Max(damage, 0.1f);

            // ---- 记录伤害 ----
            attacker.data.set(DataKeys.LAST_DAMAGE, damage);

            return damage;
        }
    }
}