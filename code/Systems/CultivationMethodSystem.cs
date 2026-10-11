using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.General;
using Newtonsoft.Json;
using NianLiXiuXing.code.UI;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Core
{
    /// <summary>
    /// 功法系统核心：运行时缓存、读写、切换与数值加成汇总。
    /// 数据定义见 <see cref="CultivationMethodData"/>，随机传播见 <see cref="CultivationMethodRandom"/>，
    /// 旧存档迁移见 <see cref="CultivationMethodMigration"/>。
    /// </summary>
    public static partial class CultivationMethodSystem
    {
        private const string METHODS_KEY = "nianli.CultivationMethods";

        // ============================================================
        // ★★★ 功法缓存（内存缓存，避免频繁 JSON 反序列化） ★★★
        // ============================================================
        private static readonly Dictionary<long, List<CultivationMethod>> _methodCache = new Dictionary<long, List<CultivationMethod>>();
        private static bool _cacheDirty = false;
        private static float _cacheLastCleanup = 0f;
        private const float CACHE_CLEANUP_INTERVAL = 30f;

        // ============================================================
        // ★★★ 缓存读写 ★★★
        // ============================================================

        public static List<CultivationMethod> GetMethods(Actor actor)
        {
            if (actor == null) return new List<CultivationMethod>();

            long id = actor.getID();
            if (_methodCache.TryGetValue(id, out var cached) && cached != null)
                return cached;

            actor.data.get(METHODS_KEY, out string json, "[]");
            try
            {
                var list = JsonConvert.DeserializeObject<List<CultivationMethod>>(json) ?? new List<CultivationMethod>();
                _methodCache[id] = list;
                return list;
            }
            catch
            {
                var fallback = new List<CultivationMethod>();
                _methodCache[id] = fallback;
                return fallback;
            }
        }

        public static void SetMethods(Actor actor, List<CultivationMethod> methods)
        {
            if (actor == null) return;

            long id = actor.getID();
            string json = JsonConvert.SerializeObject(methods ?? new List<CultivationMethod>());
            actor.data.set(METHODS_KEY, json);

            _methodCache[id] = new List<CultivationMethod>(methods ?? new List<CultivationMethod>());
            _cacheDirty = true;
        }

        public static void AddMethod(Actor actor, CultivationMethod method)
        {
            if (actor == null || method == CultivationMethod.None) return;

            var list = GetMethods(actor);
            if (!list.Contains(method))
            {
                list.Add(method);
                SetMethods(actor, list);
            }
        }

        public static void RemoveMethod(Actor actor, CultivationMethod method)
        {
            if (actor == null) return;

            var list = GetMethods(actor);
            if (list.Remove(method))
                SetMethods(actor, list);
        }

        public static bool HasMethod(Actor actor, CultivationMethod method)
        {
            return GetMethods(actor).Contains(method);
        }

        public static CultivationMethod GetMethod(Actor actor)
        {
            var list = GetMethods(actor);
            return list.Count > 0 ? list[list.Count - 1] : CultivationMethod.None;
        }

        public static void ClearMethodCache(long actorId)
        {
            _methodCache.Remove(actorId);
            _cacheDirty = true;
        }

        public static void ClearMethodCache(Actor actor)
        {
            if (actor != null)
                ClearMethodCache(actor.getID());
        }

        public static void CleanupMethodCache()
        {
            if (Time.time - _cacheLastCleanup < CACHE_CLEANUP_INTERVAL)
                return;
            _cacheLastCleanup = Time.time;

            if (World.world?.units == null)
            {
                _methodCache.Clear();
                _cacheDirty = true;
                return;
            }

            List<long> toRemove = new List<long>();
            foreach (var kvp in _methodCache)
            {
                long id = kvp.Key;
                Actor actor = World.world.units.get(id);
                if (actor == null || !actor.isAlive())
                    toRemove.Add(id);
            }

            if (toRemove.Count > 0)
            {
                foreach (long id in toRemove)
                    _methodCache.Remove(id);
                _cacheDirty = true;

                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] 功法缓存清理：移除 {toRemove.Count} 个已失效角色");
            }
        }

        public static void RefreshAllCache()
        {
            _methodCache.Clear();
            _cacheDirty = true;
            Debug.Log("[念力修行] 功法缓存已全部刷新");
        }

        public static int GetCacheSize()
        {
            return _methodCache.Count;
        }

        // ============================================================
        // ★★★ 切换与数值加成汇总 ★★★
        // ============================================================

        public static bool SwitchMethod(Actor actor, CultivationMethod newMethod)
        {
            if (newMethod == CultivationMethod.None) return false;

            var list = GetMethods(actor);
            if (list.Contains(newMethod))
            {
                list.Remove(newMethod);
                list.Add(newMethod);
            }
            else
            {
                list.Add(newMethod);
            }
            SetMethods(actor, list);
            NianLiInfoPanel.MarkDirty();

            var data = GetMethodData(newMethod);
            DaoBeiSystem.AddRecord(actor,
                LocalizedTextManager.getText("dao_event_switch"),
                string.Format(LocalizedTextManager.getText("method_switched"), LocalizedTextManager.getText(data.NameKey)),
                Color.white);
            return true;
        }

        public static float GetTotalSpeedBonus(Actor actor)
        {
            float total = 0f;
            foreach (var m in GetMethods(actor))
            {
                var data = GetMethodData(m);
                if (data != null) total += data.SpeedBonus;
            }
            return total;
        }

        public static float GetTotalAttackMod(Actor actor)
        {
            float total = 1f;
            foreach (var m in GetMethods(actor))
            {
                var data = GetMethodData(m);
                if (data != null) total *= data.AttackMod;
            }
            return total;
        }

        public static float GetTotalDefenseMod(Actor actor)
        {
            float total = 1f;
            foreach (var m in GetMethods(actor))
            {
                var data = GetMethodData(m);
                if (data != null) total *= data.DefenseMod;
            }
            return total;
        }

        public static float GetTotalHealthMod(Actor actor)
        {
            float total = 1f;
            foreach (var m in GetMethods(actor))
            {
                var data = GetMethodData(m);
                if (data != null) total *= data.HealthMod;
            }
            return total;
        }

        public static bool CanSwitchMethod(Actor actor)
        {
            return true;
        }
    }
}
