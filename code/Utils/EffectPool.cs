using System.Collections.Generic;
using UnityEngine;

namespace NianLiXiuXing.code.Utils
{
    public static class EffectPool
    {
        private static Dictionary<string, Queue<BaseEffect>> _pools = new Dictionary<string, Queue<BaseEffect>>();
        private const int MAX_POOL_SIZE = 30;
        private static float _lastCleanup = 0f;
        private const float CLEANUP_INTERVAL = 60f;

        public static BaseEffect Spawn(string effectId, WorldTile tile, float scale)
        {
            return Spawn(effectId, tile, scale, Color.white);
        }

        public static BaseEffect Spawn(string effectId, WorldTile tile, float scale, Color color)
        {
            if (string.IsNullOrEmpty(effectId) || tile == null)
                return null;

            BaseEffect effect = null;

            if (_pools.TryGetValue(effectId, out var pool) && pool.Count > 0)
            {
                effect = pool.Dequeue();
                if (effect != null)
                {
                    effect.gameObject.SetActive(true);
                    effect.transform.position = new Vector3(tile.pos.x, tile.pos.y, 0f);
                    effect.transform.localScale = Vector3.one * scale;
                    if (effect.sprite_renderer != null)
                        effect.sprite_renderer.color = color;
                    return effect;
                }
            }

            effect = EffectsLibrary.spawnAtTile(effectId, tile, scale);
            if (effect != null && effect.sprite_renderer != null)
                effect.sprite_renderer.color = color;

            return effect;
        }

        public static void Recycle(BaseEffect effect, float delay = 0f)
        {
            if (effect == null) return;

            if (delay > 0f)
            {
                // ★ 使用 Unity 的 Destroy 延迟销毁，不再依赖 DelayedActionsManager ★
                Object.Destroy(effect.gameObject, delay);
            }
            else
            {
                RecycleImmediate(effect);
            }
        }

        private static void RecycleImmediate(BaseEffect effect)
        {
            if (effect == null) return;

            string effectId = effect.name;

            effect.gameObject.SetActive(false);
            effect.transform.SetParent(null);
            effect.transform.localScale = Vector3.one;

            if (effect.sprite_renderer != null)
            {
                effect.sprite_renderer.color = Color.white;
                effect.sprite_renderer.sprite = null;
            }

            if (!_pools.TryGetValue(effectId, out var pool))
            {
                pool = new Queue<BaseEffect>();
                _pools[effectId] = pool;
            }

            if (pool.Count < MAX_POOL_SIZE)
                pool.Enqueue(effect);
            else
                Object.Destroy(effect.gameObject);
        }

        public static void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                foreach (var effect in pool)
                {
                    if (effect != null)
                        Object.Destroy(effect.gameObject);
                }
            }
            _pools.Clear();
        }

        public static void Cleanup()
        {
            if (Time.time - _lastCleanup < CLEANUP_INTERVAL) return;
            _lastCleanup = Time.time;

            List<string> toRemove = new List<string>();
            foreach (var kvp in _pools)
            {
                if (kvp.Value == null || kvp.Value.Count == 0)
                    toRemove.Add(kvp.Key);
            }
            foreach (var key in toRemove)
                _pools.Remove(key);
        }

        public static string GetStats()
        {
            int total = 0;
            foreach (var pool in _pools.Values)
                total += pool.Count;
            return $"特效池: {_pools.Count} 种类型, 共 {total} 个对象";
        }
    }
}