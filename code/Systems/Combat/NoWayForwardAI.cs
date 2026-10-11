using UnityEngine;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Systems
{
    public static class NoWayForwardAI
    {
        public static bool HasNoWayForward(Actor actor) => actor != null && actor.hasTrait(TraitIds.Trait_NoWayForward);
        public static float GetSpeedMultiplier(Actor actor) => HasNoWayForward(actor) ? 0.4f : 1.0f;
        public static float GetAttackCooldownMultiplier(Actor actor) => HasNoWayForward(actor) ? 5.0f : 1.0f;

        public static bool ShouldAttack(Actor actor)
        {
            if (!HasNoWayForward(actor)) return true;
            if (actor.IsAttacked()) return Random.Range(0f, 1f) < 0.3f;
            return false;
        }

        public static bool ShouldChase(Actor actor)
        {
            if (!HasNoWayForward(actor)) return true;
            if (actor.beh_tile_target == null) return true;
            float dist = Vector2.Distance(
                new Vector2(actor.current_tile.x, actor.current_tile.y),
                new Vector2(actor.beh_tile_target.x, actor.beh_tile_target.y)
            );
            return dist <= 2f;
        }

        public static WorldTile GetTargetTile(Actor actor)
        {
            if (!HasNoWayForward(actor) || actor.current_tile == null) return null;
            Actor nearest = FindNearestEnemy(actor);
            if (nearest == null) return null;
            int dx = actor.current_tile.x - nearest.current_tile.x;
            int dy = actor.current_tile.y - nearest.current_tile.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist < 0.1f) return null;
            int step = 5;
            // ★★★ 增加有效性循环 ★★★
            while (step > 0)
            {
                int targetX = actor.current_tile.x + Mathf.RoundToInt(dx / dist * step);
                int targetY = actor.current_tile.y + Mathf.RoundToInt(dy / dist * step);
                targetX = Mathf.Clamp(targetX, 5, MapBox.width - 5);
                targetY = Mathf.Clamp(targetY, 5, MapBox.height - 5);
                WorldTile tile = World.world.GetTileSimple(targetX, targetY);
                if (tile != null && tile.Type.ground && !tile.Type.block)
                    return tile;
                step--;
            }
            return null;
        }

        private static Actor FindNearestEnemy(Actor actor)
        {
            if (actor?.current_tile == null) return null;
            Actor nearest = null;
            float nearestDist = float.MaxValue;
            Vector2 pos = new Vector2(actor.current_tile.x, actor.current_tile.y);
            var units = World.world.units.getSimpleList();
            if (units == null) return null;
            foreach (Actor other in units)
            {
                if (other == actor || other?.data == null || !other.isAlive()) continue;
                if (other.current_tile == null) continue;
                if (actor.kingdom == other.kingdom) continue;
                float dist = Vector2.Distance(pos, new Vector2(other.current_tile.x, other.current_tile.y));
                if (dist < nearestDist && dist < 10f) { nearestDist = dist; nearest = other; }
            }
            return nearest;
        }

        public static void ProcessRecovery(Actor actor, float elapsed)
        {
            if (!HasNoWayForward(actor)) return;
            actor.ChangeNianLi(elapsed * 60f);
            if (actor.getHealth() < actor.getMaxHealth())
            {
                int heal = Mathf.RoundToInt(actor.getMaxHealth() * 0.005f * elapsed);
                if (heal > 0) actor.restoreHealth(heal);
            }
        }
    }
}