using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.Core
{
    public struct DaoBeiRecord
    {
        public string Time;
        public string ActorName;
        public string EventType;
        public string Description;
        public string ColorHex;
        [JsonIgnore]
        public Color Color;

        public DaoBeiRecord(string time, string actorName, string eventType, string description, Color color)
        {
            Time = time;
            ActorName = actorName;
            EventType = eventType;
            Description = description;
            ColorHex = ColorUtility.ToHtmlStringRGB(color);
            Color = color;
        }
    }

    public static class DaoBeiSystem
    {
        private const string RECORDS_KEY = "nianli.daobei_records";
        private static List<DaoBeiRecord> _records = new List<DaoBeiRecord>();
        private const int MAX_RECORDS = 100;

        public static void Init()
        {
            Load();
        }

        public static void Load()
        {
            _records.Clear();
            var customData = World.world?.map_stats?.custom_data;
            if (customData == null) return;

            customData.get(RECORDS_KEY, out string json, "");
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var data = JsonConvert.DeserializeObject<List<DaoBeiRecord>>(json);
                if (data != null)
                {
                    _records = data;
                    foreach (var record in _records)
                    {
                        if (!string.IsNullOrEmpty(record.ColorHex))
                            ColorUtility.TryParseHtmlString("#" + record.ColorHex, out Color c);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[道碑] 加载记录失败: {e.Message}");
                _records = new List<DaoBeiRecord>();
            }
        }

        public static void Save()
        {
            var customData = World.world?.map_stats?.custom_data;
            if (customData == null) return;

            string json = JsonConvert.SerializeObject(_records);
            customData.set(RECORDS_KEY, json);
        }

        public static void AddRecord(Actor actor, string eventType, string description, Color? color = null)
        {
            if (actor == null)
            {
                Debug.LogWarning("[道碑] 添加记录失败：actor 为空");
                return;
            }

            string time = World.world?.map_stats?.year.ToString() ?? "?";
            string actorName = actor.getName();
            Color clr = color ?? new Color(0.8f, 0.8f, 0.8f);

            var record = new DaoBeiRecord(time, actorName, eventType, description, clr);
            _records.Add(record);

            if (_records.Count > MAX_RECORDS)
                _records.RemoveRange(0, _records.Count - MAX_RECORDS);

            Save();

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[道碑] 添加记录: {time} {actorName} · {eventType} - {description}");
        }

        public static List<DaoBeiRecord> GetAllRecords()
        {
            return new List<DaoBeiRecord>(_records);
        }

        public static List<DaoBeiRecord> GetRecordsForActor(Actor actor)
        {
            if (actor == null) return new List<DaoBeiRecord>();
            var list = new List<DaoBeiRecord>();
            string name = actor.getName();
            foreach (var r in _records)
            {
                if (r.ActorName == name)
                    list.Add(r);
            }
            return list;
        }

        public static void Clear()
        {
            _records.Clear();
            Save();
        }
    }
}