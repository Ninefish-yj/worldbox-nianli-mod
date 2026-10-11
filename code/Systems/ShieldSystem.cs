using UnityEngine;
using NianLiXiuXing.code.Core;
using System.Collections.Generic;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Systems
{
    public static class ShieldSystem
    {
        private static Dictionary<long, float> _shieldRegenTimers = new Dictionary<long, float>();

        public static bool ActivateShield(Actor actor)
        {
            if (actor == null) return false;

            if (actor.GetNianLi() < 1000f) return false;

            float shieldMax = actor.GetNianLi() * 0.1f;
            shieldMax = Mathf.Min(shieldMax, 10000f);

            actor.SetShieldMax(shieldMax);
            actor.SetShieldHP(shieldMax);

            actor.ChangeNianLi(-1000f);

            if (actor.current_tile != null)
                EffectsLibrary.spawnAtTile("fx_sparkle", actor.current_tile, 2f);

            return true;
        }

        public static float AbsorbDamage(Actor actor, float damage)
        {
            if (actor == null) return damage;

            float shieldHP = actor.GetShieldHP();
            if (shieldHP <= 0) return damage;

            float absorbed = Mathf.Min(shieldHP, damage);
            actor.SetShieldHP(shieldHP - absorbed);
            NianLiInfoPanel.MarkDirty();
            float nianLiCost = absorbed * Constants.SHIELD_CONSUME_RATE;
            actor.ChangeNianLi(-nianLiCost);

            if (actor.GetShieldHP() <= 0 && actor.current_tile != null)
            {
                EffectsLibrary.spawnAtTile("fx_explosion_wave", actor.current_tile, 1f);
            }

            return damage - absorbed;
        }

        public static void RegenShield(Actor actor, float elapsed)
        {
            if (actor == null) return;

            long id = actor.getID();
            float currentWorldTime = (float)(World.world?.map_stats?.world_time ?? 0f);

            if (!_shieldRegenTimers.TryGetValue(id, out float lastRegenTime))
                lastRegenTime = 0f;

            float interval = 0.5f / 365f;
            if (currentWorldTime - lastRegenTime < interval)
            {
                // 更新时间到当前，避免每帧判断
                _shieldRegenTimers[id] = lastRegenTime;
                return;
            }
            _shieldRegenTimers[id] = currentWorldTime;

            float shieldHP = actor.GetShieldHP();
            float shieldMax = actor.GetShieldMax();

            if (shieldHP >= shieldMax || shieldMax <= 0) return;

            float regen = Constants.SHIELD_REGEN_RATE * 43200f;
            actor.SetShieldHP(Mathf.Min(shieldHP + regen, shieldMax));
        }

        public static string GetShieldStatus(Actor actor)
        {
            if (actor == null) return "无护盾";
            float hp = actor.GetShieldHP();
            float max = actor.GetShieldMax();
            if (max <= 0) return "无护盾";
            return $"护盾: {hp:F0}/{max:F0}";
        }

        public static void CleanupInactiveActors()
        {
            if (World.world?.units == null)
            {
                _shieldRegenTimers.Clear();
                return;
            }

            List<long> toRemove = null;
            foreach (var id in _shieldRegenTimers.Keys)
            {
                var actor = World.world.units.get(id);
                if (actor == null || !actor.isAlive())
                {
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                }
            }
            if (toRemove != null)
            {
                foreach (var id in toRemove)
                    _shieldRegenTimers.Remove(id);
            }
        }

        public static void CleanupTimer(long actorId)
        {
            _shieldRegenTimers.Remove(actorId);
        }
    }
}