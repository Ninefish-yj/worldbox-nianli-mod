using UnityEngine;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Core
{
    public static class RealmSkills
    {
        // 技能冷却改为游戏天数（30天）
        private const float SKILL_COOLDOWN_YEARS = 30f / 365f;

        public static void ProcessRealmSkills(Actor actor, Actor defender)
        {
            if (actor == null) return;

            int realm = actor.GetRealmLevel();
            if (realm <= 0) return;

            float currentWorldTime = (float)(World.world?.map_stats?.world_time ?? 0f);
            actor.data.get(DataKeys.LAST_SKILL_WORLD_TIME, out float lastSkillTime, 0f);

            if (currentWorldTime - lastSkillTime < SKILL_COOLDOWN_YEARS) return;

            if (Random.Range(0f, 1f) > 0.08f) return;

            // 百分比消耗
            float costPercent = Constants.RealmSkillCostPercent[realm];
            float baseCost = actor.GetNianLi() * costPercent;

            // 生灭关：消耗减半
            float costMultiplier = 1f;
            if (actor.IsHeartBroken(3))
            {
                costMultiplier = 0.5f;
            }

            float actualCost = baseCost * costMultiplier;
            actualCost = Mathf.Max(actualCost, 100f);

            if (actor.GetNianLi() < actualCost) return;

            bool skillTriggered = false;

            switch (realm)
            {
                case 1: skillTriggered = SkillNianLiImpact(actor, defender); break;
                case 2: skillTriggered = SkillNianLiPerception(actor); break;
                case 3: skillTriggered = SkillTianDiResonance(actor); break;
                case 4: skillTriggered = SkillTianDiDaHuan(actor, defender); break;
                case 5: skillTriggered = SkillAscension(actor); break;
            }

            if (skillTriggered)
            {
                actor.ChangeNianLi(-actualCost);
		NianLiInfoPanel.MarkDirty();
                actor.data.set(DataKeys.LAST_SKILL_WORLD_TIME, currentWorldTime);

                if (costMultiplier < 1f)
                {
                    Debug.Log($"[念力修行] {actor.data.name} 生灭关触发，技能消耗减半 ({baseCost:F0}→{actualCost:F0})");
                }
            }
        }

        private static bool SkillNianLiImpact(Actor actor, Actor defender)
        {
            if (defender == null) return false;

            float extraDamage = actor.GetNianLi() * 0.02f;
            int damage = Mathf.RoundToInt(extraDamage);
            defender.changeHealth(-damage);
            defender.startColorEffect(ActorColorEffect.Red);

            if (defender.current_tile != null)
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_sparkle", defender.current_tile, 0.6f);
                if (effect != null) Object.Destroy(effect.gameObject, 1f);
            }

            return true;
        }

        private static bool SkillNianLiPerception(Actor actor)
        {
            float aoeBonus = 5f;
            actor.stats["area_of_effect"] += aoeBonus;

            if (actor.current_tile != null)
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_sparkle", actor.current_tile, 2f);
                if (effect != null) Object.Destroy(effect.gameObject, 1.5f);
            }

            return true;
        }

        private static bool SkillTianDiResonance(Actor actor)
        {
            if (actor.current_tile == null) return false;

            Vector2 center = new Vector2(actor.current_tile.x, actor.current_tile.y);
            float radius = 5f;

            foreach (Actor target in World.world.units)
            {
                if (target == actor) continue;
                if (target?.data == null || !target.isAlive()) continue;
                if (target.current_tile == null) continue;

                float dist = Vector2.Distance(center, new Vector2(target.current_tile.x, target.current_tile.y));
                if (dist > radius) continue;

                if (target.kingdom == actor.kingdom)
                {
                    target.stats["NianLiAttack"] += 10f;
                }
            }

            BaseEffect effect = EffectsLibrary.spawnAtTile("fx_explosion_wave", actor.current_tile, 3f);
            if (effect != null) Object.Destroy(effect.gameObject, 2f);

            return true;
        }

        private static bool SkillTianDiDaHuan(Actor actor, Actor defender)
        {
            if (defender == null || defender.current_tile == null) return false;

            float damage = actor.GetNianLi() * 0.1f;
            int finalDamage = Mathf.RoundToInt(damage);

            Vector2 center = new Vector2(defender.current_tile.x, defender.current_tile.y);
            float radius = 3f;

            foreach (Actor target in World.world.units)
            {
                if (target == actor) continue;
                if (target?.data == null || !target.isAlive()) continue;
                if (target.current_tile == null) continue;

                float dist = Vector2.Distance(center, new Vector2(target.current_tile.x, target.current_tile.y));
                if (dist > radius) continue;

                target.changeHealth(-finalDamage);
                target.startColorEffect(ActorColorEffect.Red);
            }

            BaseEffect effect = EffectsLibrary.spawnAtTile("fx_explosion_wave", defender.current_tile, 4f);
            if (effect != null) Object.Destroy(effect.gameObject, 2.5f);

            return true;
        }

        // ★★★ 修改后的飞升技能 ★★★
        private static bool SkillAscension(Actor actor)
        {
            // 检查是否能触发大阳劫（托举境 + 已完成小阳劫 + 其他条件）
            if (actor.CanStartMajorAscension())
            {
                FlyingAscensionSystem.TriggerAscension(actor);
                return true;
            }
            
            // 如果当前是环界境且满足小阳劫条件，也可以触发（但通常由 GrowthSystem 触发）
            // 这里仅触发大阳劫
            return false;
        }

        public static string GetSkillDescription(int realmLevel)
        {
            switch (realmLevel)
            {
                case 1: return "养念: 念力冲击 - 造成额外念力伤害";
                case 2: return "复观: 念力感知 - 增加攻击范围";
                case 3: return "大通: 天地共鸣 - 友军获得攻击加成";
                case 4: return "环界: 天地大环 - 范围念力爆发";
                case 5: return "托举: 飞升 - 开启大阳劫";
                default: return "";
            }
        }
    }
}