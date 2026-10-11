using UnityEngine;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Systems
{
    public static class NianLiAILimiter
    {
        public static float DangerThreshold { get; set; } = 1000f;
        public static float ConservationThreshold { get; set; } = 5000f;

        public enum NianLiState { Normal, Conserve, Critical }

        public static NianLiState GetState(Actor actor)
        {
            if (actor == null) return NianLiState.Normal;
            float nianLi = actor.GetNianLi();
            float max = actor.GetCurrentMaxNianLi();
            if (max <= 0) return NianLiState.Normal;
            float ratio = nianLi / max;
            if (ratio < 0.10f) return NianLiState.Critical;
            if (ratio < 0.30f) return NianLiState.Conserve;
            return NianLiState.Normal;
        }

        public static bool ShouldAvoidCombat(Actor actor)
        {
            if (actor == null) return false;
            if (!ConfigCache.EnableNianLiAILimiter) return false;
            NianLiState state = GetState(actor);
            if (state == NianLiState.Critical) return true;
            if (state == NianLiState.Conserve)
            {
                if (actor.beh_tile_target == null) return false;
                Actor target = GetTargetEnemy(actor);
                if (target == null) return false;
                return target.GetRealmLevel() > actor.GetRealmLevel();
            }
            return false;
        }

        private static Actor GetTargetEnemy(Actor actor)
        {
            if (actor?.beh_tile_target == null) return null;
            var units = World.world.units.getSimpleList();
            if (units == null) return null;
            foreach (Actor other in units)
            {
                if (other == actor || other?.data == null || !other.isAlive()) continue;
                if (other.current_tile == null) continue;
                if (actor.kingdom == other.kingdom) continue;
                if (other.current_tile.x == actor.beh_tile_target.x && other.current_tile.y == actor.beh_tile_target.y)
                    return other;
            }
            return null;
        }

        public static float GetSkillDiscount(Actor actor)
        {
            NianLiState state = GetState(actor);
            switch (state)
            {
                case NianLiState.Critical: return 0.1f;
                case NianLiState.Conserve: return 0.5f;
                default: return 1.0f;
            }
        }

        public static float GetSpeedModifier(Actor actor)
        {
            return GetState(actor) == NianLiState.Critical ? 1.3f : 1.0f;
        }

        public static bool ShouldMeditate(Actor actor)
        {
            if (actor == null) return false;
            NianLiState state = GetState(actor);
            if (state == NianLiState.Normal) return false;
            if (HasNearbyEnemy(actor)) return false;
            if (state == NianLiState.Critical) return true;
            if (state == NianLiState.Conserve && !actor.IsAttacked())
                return Random.Range(0f, 1f) < 0.1f;
            return false;
        }

        private static bool HasNearbyEnemy(Actor actor)
        {
            if (actor?.current_tile == null) return false;
            Vector2 pos = new Vector2(actor.current_tile.x, actor.current_tile.y);
            var units = World.world.units.getSimpleList();
            if (units == null) return false;
            foreach (Actor other in units)
            {
                if (other == actor || other?.data == null || !other.isAlive()) continue;
                if (other.current_tile == null) continue;
                if (actor.kingdom == other.kingdom) continue;
                if (Vector2.Distance(pos, new Vector2(other.current_tile.x, other.current_tile.y)) < 5f)
                    return true;
            }
            return false;
        }
    }
}