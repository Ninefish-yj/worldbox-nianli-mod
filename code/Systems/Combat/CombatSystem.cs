using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Systems
{
    public static class CombatSystem
    {
        public static void ProcessCombatEffects(Actor attacker, Actor defender)
        {
            if (attacker == null || defender == null) return;
            if (!attacker.isAlive() || !defender.isAlive()) return;
            if (defender.current_tile == null) return;
            if (attacker.data == null || defender.data == null) return;

            // ★★★ 生灭关溅射：小粒子 ★★★
            if (attacker.IsHeartBroken(3) && Random.Range(0f, 1f) < 0.30f)
            {
                float splashDamage = attacker.GetAttackPower() * 0.5f;
                Vector2 center = new Vector2(defender.current_tile.x, defender.current_tile.y);
                float radius = 2f;

                var units = World.world.units.getSimpleList();
                if (units != null)
                {
                    foreach (Actor target in units)
                    {
                        if (target == defender || target == attacker) continue;
                        if (target?.data == null || !target.isAlive()) continue;
                        if (target.current_tile == null) continue;

                        float dist = Vector2.Distance(center, new Vector2(target.current_tile.x, target.current_tile.y));
                        if (dist > radius) continue;

                        int finalDmg = Mathf.RoundToInt(splashDamage * 0.5f);
                        target.changeHealth(-finalDmg);
                        target.startColorEffect(ActorColorEffect.Red);
                    }
                }

                SpawnEffectSafely("fx_sparkle", defender.current_tile, 0.6f, 1f);
            }

            // ★★★ 残血爆发：小粒子 ★★★
            if (attacker.HasLowHealthBonus())
            {
                SpawnEffectSafely("fx_sparkle", attacker.current_tile, 0.8f, 1.2f);
            }

            // ★★★ 真实伤害：小粒子 ★★★
            attacker.data.get(DataKeys.LAST_TRUE_DAMAGE, out bool isTrue, false);
            if (isTrue)
            {
                SpawnEffectSafely("fx_sparkle", defender.current_tile, 0.6f, 1f);
            }

            // ★★★ 境界攻击光环：小粒子 ★★★
            if (attacker.current_tile != null && attacker.GetRealmLevel() >= 2)
            {
                SpawnEffectSafely("fx_sparkle", attacker.current_tile, 0.5f, 0.8f, (effect) =>
                {
                    if (effect?.sprite_renderer != null)
                    {
                        int realm = attacker.GetRealmLevel();
                        switch (realm)
                        {
                            case 2: effect.sprite_renderer.color = Color.cyan; break;
                            case 3: effect.sprite_renderer.color = Color.green; break;
                            case 4: effect.sprite_renderer.color = new Color(1f, 0.5f, 0f); break;
                            case 5: effect.sprite_renderer.color = Color.magenta; break;
                        }
                    }
                });
            }
        }

        private static void SpawnEffectSafely(string effectId, WorldTile tile, float scale, float destroyDelay, System.Action<BaseEffect> onCreated = null)
        {
            if (tile == null) return;
            SafeExecutor.Execute(() =>
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile(effectId, tile, scale);
                if (effect != null)
                {
                    onCreated?.Invoke(effect);
                    Object.Destroy(effect.gameObject, destroyDelay);
                }
            }, $"生成战斗特效 {effectId}", false);
        }

        public static void ShowDamageNumber(Actor attacker, float damage)
        {
            if (attacker?.current_tile == null) return;

            attacker.data.get(DataKeys.LAST_CRIT, out bool isCrit, false);
            attacker.data.get(DataKeys.LAST_TRUE_DAMAGE, out bool isTrue, false);
            attacker.data.get(DataKeys.LAST_LOW_HEALTH_BONUS, out bool isLow, false);

            string msg = $"💥 {damage:F1}";
            if (isCrit) msg += " 暴击!";
            if (isTrue) msg += " 真实!";
            if (isLow) msg += " 残血爆发!";

            WorldTip.showNow(msg, true, "top");
        }
    }
}