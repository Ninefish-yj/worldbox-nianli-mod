using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Systems
{
    public static class AscensionRecordSystem
    {
        private const string RECORD_KEY = "nianli_ascension_records";
        private static List<AscensionRecord> _records = new List<AscensionRecord>();
        private static bool _dirty = false;
        private static float _lastSaveTime = 0f;
        private const float SAVE_INTERVAL = 30f; // 30秒保存一次

        public class AscensionRecord
        {
            public long actorId;
            public string actorName;
            public int realmLevel;
            public bool isSuccess;
            public string timestamp;
        }

        public static void Load()
        {
            _records.Clear();
            var customData = World.world?.map_stats?.custom_data;
            if (customData == null) return;

            CleanupAllActorStates();

            SafeExecutor.Execute(() =>
            {
                customData.get(RECORD_KEY, out string json, "");
                if (string.IsNullOrEmpty(json)) return;
                _records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AscensionRecord>>(json);
                if (_records == null) _records = new List<AscensionRecord>();
            }, "加载飞升记录", false);
        }

        private static void SaveIfDirty()
        {
            if (!_dirty) return;
            if (Time.time - _lastSaveTime < SAVE_INTERVAL) return;
            SaveInternal();
        }

        private static void SaveInternal()
        {
            var customData = World.world?.map_stats?.custom_data;
            if (customData == null) return;

            SafeExecutor.Execute(() =>
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_records);
                customData.set(RECORD_KEY, json);
                _dirty = false;
                _lastSaveTime = Time.time;
            }, "保存飞升记录", false);
        }

        public static void AddRecord(Actor actor, bool success)
        {
            if (actor == null) return;
            if (!success) return;

            foreach (var r in _records)
            {
                if (r.actorId == actor.getID())
                    return;
            }

            var record = new AscensionRecord
            {
                actorId = actor.getID(),
                actorName = actor.data?.name ?? "未知",
                realmLevel = actor.GetRealmLevel(),
                isSuccess = true,
                timestamp = World.world?.map_stats?.year.ToString() ?? "?"
            };
            _records.Add(record);
            _dirty = true;
            Debug.Log($"[飞升记录] 添加 {record.actorName} (ID:{record.actorId})");
            SaveIfDirty();
        }

        public static void AddRecordManually(long actorId, string actorName, int realmLevel)
        {
            foreach (var r in _records)
            {
                if (r.actorId == actorId)
                    return;
            }

            var record = new AscensionRecord
            {
                actorId = actorId,
                actorName = actorName,
                realmLevel = realmLevel,
                isSuccess = true,
                timestamp = World.world?.map_stats?.year.ToString() ?? "?"
            };
            _records.Add(record);
            _dirty = true;
            Debug.Log($"[飞升记录] 手动添加 {actorName} (ID:{actorId})");
            SaveIfDirty();
        }

        public static AscensionRecord GetRecordById(long actorId)
        {
            foreach (var r in _records)
            {
                if (r.actorId == actorId)
                    return r;
            }
            return null;
        }

        public static AscensionRecord GetRecordByName(string actorName)
        {
            foreach (var r in _records)
            {
                if (r.actorName == actorName)
                    return r;
            }
            return null;
        }

        public static bool IsOnList(Actor actor)
        {
            if (actor == null) return false;
            foreach (var r in _records)
            {
                if (r.actorId == actor.getID() && r.isSuccess)
                    return true;
            }
            return false;
        }

        public static void RemoveRecord(Actor actor)
        {
            if (actor == null) return;
            int removed = _records.RemoveAll(r => r.actorId == actor.getID());
            if (removed > 0)
            {
                _dirty = true;
                Debug.Log($"[飞升记录] 移除 {actor.data?.name} 的记录");
                SaveIfDirty();
            }
        }

        public static void RemoveRecordById(long actorId)
        {
            int removed = _records.RemoveAll(r => r.actorId == actorId);
            if (removed > 0)
            {
                _dirty = true;
                Debug.Log($"[飞升记录] 移除 ID:{actorId} 的记录");
                SaveIfDirty();
            }
        }

        public static List<AscensionRecord> GetAllRecords()
        {
            return new List<AscensionRecord>(_records);
        }

        public static void ClearAll()
        {
            _records.Clear();
            _dirty = true;
            SaveInternal();
        }

        public static bool IsActorExists(long actorId)
        {
            if (World.world?.units == null) return false;
            return World.world.units.get(actorId) != null;
        }

        public static void CleanupAllActorStates()
        {
            if (World.world?.units == null) return;

            int cleanedCount = 0;
            foreach (var actor in World.world.units)
            {
                if (actor == null) continue;

                bool needsClean = false;

                if (actor.IsAscensionReady())
                {
                    actor.SetAscensionReady(false);
                    needsClean = true;
                }
                if (actor.GetAscensionStep() > 0)
                {
                    actor.SetAscensionStep(0);
                    needsClean = true;
                }
                if (actor.IsZenWuWei())
                {
                    actor.SetZenWuWei(false);
                    actor.SetZenWuWeiTimer(0f);
                    needsClean = true;
                }
                if (actor.IsMeditating())
                {
                    actor.SetMeditating(false);
                    needsClean = true;
                }
                if (actor.IsStaying())
                {
                    actor.SetStaying(false);
                    actor.SetStayingStartYear(-1);
                    actor.SetStayingDuration(0);
                    needsClean = true;
                }

                if (needsClean)
                {
                    cleanedCount++;
                    Debug.Log($"[念力修行] 加载清理：重置 {actor.data?.name ?? "未知"} 的危险状态");
                }

                ShieldSystem.CleanupTimer(actor.getID());
            }

            if (cleanedCount > 0)
            {
                Debug.Log($"[念力修行] 加载清理完成，共清理 {cleanedCount} 个角色");
            }
        }

        // 外部可调用强制保存
        public static void ForceSave()
        {
            if (_dirty) SaveInternal();
        }
    }
}