using UnityEngine;
using NianLiXiuXing.code.Core;
using System.Collections.Generic;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Systems
{
    public static class MeditationSystem
    {
        private static Dictionary<long, float> _meditationTimers = new Dictionary<long, float>();
        private static Dictionary<long, float> _lastEffectTimes = new Dictionary<long, float>();

        private static float _enemyCheckTimer = 0f;
        private static bool _cachedHasEnemy = false;

        public static void ProcessMeditation(Actor actor, float elapsed)
        {
            if (actor == null || !actor.isAlive())
            {
                if (actor != null && actor.IsMeditating())
                    actor.SetMeditating(false);
                NianLiInfoPanel.MarkDirty();
                return;
            }

            long id = actor.getID();
            bool canMeditate = CanEnterMeditation(actor);

            if (canMeditate)
            {
                // ★ 优化字典操作
                if (!_meditationTimers.TryGetValue(id, out float currentTimer))
                    _meditationTimers[id] = 0f;
                else
                    _meditationTimers[id] = currentTimer + elapsed;

                if (_meditationTimers[id] >= Constants.MEDITATION_REQUIRE_INTERVAL)
                {
                    actor.SetMeditating(true);
                    float karma = KarmaSystem.GetKarma(actor);
                    if (karma > 0)
                    {
                        float decay = elapsed * 0.1f;
                        KarmaSystem.ReduceKarma(actor, decay);
                    }
                }
            }
            else
            {
                _meditationTimers[id] = 0f;
                if (actor.IsMeditating())
                    actor.SetMeditating(false);
            }
        }

        private static bool CanEnterMeditation(Actor actor)
        {
            if (actor.is_moving) return false;

            actor.data.get(DataKeys.LAST_ATTACK_TIME, out float lastAttack, 0f);
            if (Time.time - lastAttack < 3f) return false;

            if (HasNearbyEnemy(actor)) return false;

            if (actor.GetRealmLevel() >= 3) return true;
            return true;
        }

        private static bool HasNearbyEnemy(Actor actor)
        {
            if (actor.current_tile == null) return false;

            _enemyCheckTimer += Time.deltaTime;
            if (_enemyCheckTimer < 2f) return _cachedHasEnemy;
            _enemyCheckTimer = 0f;

            try
            {
                var nearby = Finder.getUnitsFromChunk(actor.current_tile, 2, 10f, false);
                if (nearby != null)
                {
                    foreach (Actor other in nearby)
                    {
                        if (other == actor || other?.data == null || !other.isAlive()) continue;
                        if (other.current_tile == null) continue;
                        if (actor.kingdom != other.kingdom && other.kingdom != null)
                        {
                            _cachedHasEnemy = true;
                            return true;
                        }
                    }
                }
            }
            catch { }

            _cachedHasEnemy = false;
            return false;
        }

        public static float GetMeditationMultiplier(Actor actor)
        {
            if (actor == null) return 1f;
            if (actor.IsMeditating())
            {
                if (actor.GetRealmLevel() >= 3)
                    return Constants.MEDITATION_MULTIPLIER * 1.5f;
                return Constants.MEDITATION_MULTIPLIER;
            }
            return 1f;
        }

        public static void SpawnMeditationEffect(Actor actor, float elapsed)
        {
            if (actor == null || !actor.IsMeditating()) return;
            if (actor.current_tile == null || !actor.isAlive()) return;

            long id = actor.getID();
            float currentWorldTime = (float)(World.world?.map_stats?.world_time ?? 0f);
            float interval = 3f / 365f;

            if (!_lastEffectTimes.TryGetValue(id, out float lastTime))
                _lastEffectTimes[id] = 0f;
            else if (currentWorldTime - lastTime < interval)
                return;

            _lastEffectTimes[id] = currentWorldTime;

            try
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_sparkle", actor.current_tile, 0.3f);
                if (effect?.sprite_renderer != null)
                {
                    effect.sprite_renderer.color = new Color(0.6f, 0.3f, 1f);
                    Object.Destroy(effect.gameObject, 2f);
                }
            }
            catch { }
        }

        public static void CleanupInactiveActors()
        {
            if (World.world?.units == null)
            {
                _meditationTimers.Clear();
                _lastEffectTimes.Clear();
                return;
            }

            List<long> toRemove = null;
            foreach (var id in _meditationTimers.Keys)
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
                {
                    _meditationTimers.Remove(id);
                    _lastEffectTimes.Remove(id);
                }
            }
        }

        public static void RemoveActor(long actorId)
        {
            _meditationTimers.Remove(actorId);
            _lastEffectTimes.Remove(actorId);
        }
    }
}