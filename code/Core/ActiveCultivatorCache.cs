using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Core
{
    public static class ActiveCultivatorCache
    {
        private static HashSet<long> _cultivatorIds = new HashSet<long>();
        private static float _lastCleanupTime = 0f;
        private const float CLEANUP_INTERVAL = 5f;

        public static bool IsCultivator(Actor actor) => actor != null && _cultivatorIds.Contains(actor.getID());
        public static void AddCultivator(Actor actor) { if (actor != null) _cultivatorIds.Add(actor.getID()); }
        public static void RemoveCultivator(Actor actor) { if (actor != null) _cultivatorIds.Remove(actor.getID()); }
        public static void RemoveCultivator(long id) { _cultivatorIds.Remove(id); }

        public static void CleanupIfNeeded()
        {
            if (Time.unscaledTime - _lastCleanupTime < CLEANUP_INTERVAL) return;
            _lastCleanupTime = Time.unscaledTime;

            // ★ HashSet.RemoveWhere 内部使用枚举器，但不会分配额外集合，是高效的 ★
            _cultivatorIds.RemoveWhere(id =>
            {
                Actor actor = World.world?.units?.get(id);
                return actor == null || !actor.isAlive() || actor.GetRealmLevel() < 1;
            });
        }

        public static void Rebuild()
        {
            _cultivatorIds.Clear();
            if (World.world?.units == null)
            {
                Debug.Log("[念力修行] 修士缓存重建：世界未加载");
                return;
            }

            int count = 0;

            // ★ 使用 getSimpleList() 获取 List<Actor>，然后使用 for 循环代替 foreach ★
            var units = World.world.units.getSimpleList();
            for (int i = 0; i < units.Count; i++)
            {
                Actor actor = units[i];
                if (actor?.data == null || !actor.isAlive()) continue;

                int realm = actor.GetRealmLevel();
                if (realm >= 1)
                {
                    _cultivatorIds.Add(actor.getID());
                    count++;
                }
                else
                {
                    // 兼容旧存档：从特质推断境界
                    for (int j = 1; j <= 5; j++)
                    {
                        if (actor.hasTrait($"Realm_{j}"))
                        {
                            _cultivatorIds.Add(actor.getID());
                            count++;
                            actor.SetRealmLevel(j);
                            break;
                        }
                    }
                }
            }

            Debug.Log($"[念力修行] 修士缓存重建完成，共 {count} 人");
        }

        public static int Count => _cultivatorIds.Count;
        public static IEnumerable<long> GetAllIds() => _cultivatorIds;
    }
}