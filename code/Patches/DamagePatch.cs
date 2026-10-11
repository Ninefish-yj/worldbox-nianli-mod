using UnityEngine;
using HarmonyLib;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Patches
{
    [HarmonyPatch]
    public static class DamagePatch
    {
        // ★★★ 修正：使用完整的参数签名（7个参数，匹配游戏实际方法） ★★★
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Actor), "getHit", new System.Type[] {
            typeof(float),          // pDamage
            typeof(bool),           // pFlash
            typeof(AttackType),     // pAttackType
            typeof(BaseSimObject),  // pAttacker
            typeof(bool),           // pSkipIfShake
            typeof(bool),           // pMetallicWeapon
            typeof(bool)            // pCheckDamageReduction
        })]
        public static bool Actor_GetHit_Prefix(
            Actor __instance,
            ref float pDamage,
            bool pFlash,
            AttackType pAttackType,
            BaseSimObject pAttacker,
            bool pSkipIfShake,
            bool pMetallicWeapon,
            bool pCheckDamageReduction)
        {
            // ★ 核心逻辑保持不变 ★
            if (__instance == null || __instance.data == null) return true;
            if (__instance.data.health <= 0 || float.IsNaN(__instance.data.health))
            {
                __instance.data.health = -1;
                __instance.dieAndDestroy(AttackType.Other);
                return false;
            }
            if (!__instance.isAlive()) return true;

            // 只处理武器攻击和普通攻击
            if (pAttackType != AttackType.Weapon && pAttackType != AttackType.Other) return true;

            bool defenderHasCultivation = __instance.GetShieldMax() > 0 || __instance.GetHeartProgress() > 0;
            Actor attacker = pAttacker as Actor;
            bool attackerIsCultivator = attacker != null && attacker.GetRealmLevel() > 0;
            if (!defenderHasCultivation && !attackerIsCultivator) return true;

            // 清理异常护盾
            float shieldHP = __instance.GetShieldHP();
            float shieldMax = __instance.GetShieldMax();
            if (shieldHP > 100000f || float.IsNaN(shieldHP) || float.IsInfinity(shieldHP))
            {
                __instance.SetShieldHP(0);
                __instance.SetShieldMax(0);
            }
            if (shieldMax > 100000f || float.IsNaN(shieldMax) || float.IsInfinity(shieldMax))
            {
                __instance.SetShieldMax(0);
            }
            if (__instance.IsZenWuWei()) return false;

            // 计算伤害
            float damage;
            if (attacker != null && attackerIsCultivator)
                damage = CombatExtensions.CalculateDamage(attacker, __instance);
            else
                damage = pDamage;

            // 前路断绝惩罚
            if (attacker != null && attackerIsCultivator)
            {
                if (attacker.hasTrait(TraitIds.Trait_NoWayForward)) damage *= 0.5f;
                if (ConfigCache.EnableNianLiAILimiter)
                {
                    float nianLi = attacker.GetNianLi();
                    float max = attacker.GetCurrentMaxNianLi();
                    if (max > 0)
                    {
                        float ratio = nianLi / max;
                        if (ratio < 0.3f)
                        {
                            float penalty = Mathf.Lerp(0.3f, 1f, ratio / 0.3f);
                            damage *= penalty;
                        }
                    }
                }
            }

            if (damage <= 0) return false;

            float damageAfterShield = ShieldSystem.AbsorbDamage(__instance, damage);

            // 真我关 自在不灭
            if (__instance.IsHeartBroken(5) && __instance.CanUseZenWuWei())
            {
                float currentHealth = __instance.getHealth();
                if (damageAfterShield >= currentHealth)
                {
                    __instance.TriggerZenWuWei();
                    __instance.ChangeNianLi(-__instance.GetNianLi());
                    __instance.setHealth(1);
                    if (__instance.current_tile != null)
                    {
                        BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", __instance.current_tile, 3f);
                        if (effect?.sprite_renderer != null)
                        {
                            effect.sprite_renderer.color = new Color(1f, 0.84f, 0f);
                            Object.Destroy(effect.gameObject, 3f);
                        }
                    }
                    WorldTip.showNow($"✦ {__instance.data.name} 自在不灭!", true, "top");
                    return false;
                }
            }

            int finalDamage = Mathf.RoundToInt(damageAfterShield);
            if (finalDamage > 0)
            {
                __instance.changeHealth(-finalDamage);
                __instance.startColorEffect(ActorColorEffect.Red);
            }

            if (attacker != null && attackerIsCultivator)
            {
                HeartRealmEffects.ProcessAllHeartEffects(attacker, __instance);
                RealmSkills.ProcessRealmSkills(attacker, __instance);
                if (finalDamage > 0) attacker.ChangeNianLi(Constants.NIANLI_PER_ATTACK);
                CombatSystem.ProcessCombatEffects(attacker, __instance);
            }

            if (!__instance.hasHealth())
            {
                __instance.attackedBy = attacker;
                __instance.batch.c_check_deaths.Add(__instance);
                if (attacker != null)
                {
                    attacker.AddKill();
                    attacker.ChangeNianLi(Constants.NIANLI_PER_KILL);
                }
                CycleLawSystem.ProcessReincarnation(__instance);
                foreach (var trait in __instance.getTraits())
                    trait.action_death?.Invoke(__instance, __instance.current_tile);

                int defRealm = __instance.GetRealmLevel();
                if (defRealm >= 4 && attacker != null)
                    WorldMessageSystem.BroadcastWithHistory(defRealm == 4 ?
                        $"💀 环界强者 {__instance.data.name} 被 {attacker.data.name} 击杀！" :
                        $"💀 超越者 {__instance.data.name} 陨落于 {attacker.data.name} 之手！");
            }
            return false;
        }
    }
}