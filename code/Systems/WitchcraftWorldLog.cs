using System;
using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Utils;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Systems
{
    public static class WitchcraftWorldLog
    {
        public const string LOG_GROUP = "NianLiXiuXing";
        private const string HISTORY_GROUP_ICON = "ui/NianLi";

        // ========== 日志资产字段 ==========
        public static WorldLogAsset breakthrough_realm_4;
        public static WorldLogAsset breakthrough_realm_5;
        public static WorldLogAsset heart_break_4;
        public static WorldLogAsset heart_break_5;
        public static WorldLogAsset ascension_success;
        public static WorldLogAsset ascension_fail;
        public static WorldLogAsset pioneer_realm_1;
        public static WorldLogAsset pioneer_realm_2;
        public static WorldLogAsset pioneer_realm_3;
        public static WorldLogAsset pioneer_realm_4;
        public static WorldLogAsset pioneer_realm_5;
        public static WorldLogAsset kill_realm_4;
        public static WorldLogAsset kill_realm_5;
        public static WorldLogAsset heart_fail;
        public static WorldLogAsset minor_ascension_success;
        public static WorldLogAsset breakthrough_fail_death_3;
        public static WorldLogAsset breakthrough_fail_death_4;
        public static WorldLogAsset breakthrough_fail_death_5;
        public static WorldLogAsset breakthrough_fail_noway_3;
        public static WorldLogAsset breakthrough_fail_noway_4;
        public static WorldLogAsset breakthrough_fail_noway_5;

        // ========== 初始化 ==========
        public static void Init()
        {
            RegisterHistoryGroup();
            RegisterLogAssets();
            Debug.Log("[念力修行] 历史系统初始化完成");
        }

        // ========== 注册历史分组 ==========
        private static void RegisterHistoryGroup()
        {
            if (AssetManager.history_groups == null) return;

            bool exists = false;
            foreach (HistoryGroupAsset group in AssetManager.history_groups.list)
            {
                if (group.id == LOG_GROUP)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var group = new HistoryGroupAsset
                {
                    id = LOG_GROUP,
                    icon_path = HISTORY_GROUP_ICON
                };
                AssetManager.history_groups.add(group);
                Debug.Log($"[念力修行] 历史分组 {LOG_GROUP} 已注册");
            }
        }

        // ========== 注册日志资产 ==========
        private static void RegisterLogAssets()
        {
            breakthrough_realm_4 = RegisterLogAsset(
                "nianli_breakthrough_realm_4",
                "ui/Realm4",
                new Color(0.3f, 0.6f, 1f)
            );

            breakthrough_realm_5 = RegisterLogAsset(
                "nianli_breakthrough_realm_5",
                "ui/Realm5",
                new Color(1f, 0.84f, 0f)
            );

            heart_break_4 = RegisterLogAsset(
                "nianli_heart_break_4",
                "ui/Heart4",
                new Color(1f, 0.84f, 0f)
            );

            heart_break_5 = RegisterLogAsset(
                "nianli_heart_break_5",
                "ui/Heart5",
                new Color(1f, 0.84f, 0f)
            );

            ascension_success = RegisterLogAsset(
                "nianli_ascension_success",
                "ui/NianLi",
                new Color(1f, 0.84f, 0f)
            );

            ascension_fail = RegisterLogAsset(
                "nianli_ascension_fail",
                "ui/NianLi",
                new Color(1f, 0.2f, 0.2f)
            );

            pioneer_realm_1 = RegisterLogAsset(
                "nianli_pioneer_realm_1",
                "ui/NianLi",
                new Color(1f, 0.84f, 0f)
            );

            pioneer_realm_2 = RegisterLogAsset(
                "nianli_pioneer_realm_2",
                "ui/NianLi",
                new Color(1f, 0.84f, 0f)
            );

            pioneer_realm_3 = RegisterLogAsset(
                "nianli_pioneer_realm_3",
                "ui/NianLi",
                new Color(1f, 0.84f, 0f)
            );

            pioneer_realm_4 = RegisterLogAsset(
                "nianli_pioneer_realm_4",
                "ui/NianLi",
                new Color(1f, 0.84f, 0f)
            );

            pioneer_realm_5 = RegisterLogAsset(
                "nianli_pioneer_realm_5",
                "ui/NianLi",
                new Color(1f, 0.84f, 0f)
            );

            kill_realm_4 = RegisterLogAsset(
                "nianli_kill_realm_4",
                "ui/NianLi",
                new Color(1f, 0.2f, 0.2f)
            );

            kill_realm_5 = RegisterLogAsset(
                "nianli_kill_realm_5",
                "ui/NianLi",
                new Color(1f, 0.2f, 0.2f)
            );

            heart_fail = RegisterLogAsset(
                "heart_fail",
                "ui/Heart5",
                new Color(0.5f, 0.5f, 0.5f)
            );

            minor_ascension_success = RegisterLogAsset(
                "minor_ascension_success",
                "ui/NianLi",
                new Color(0.3f, 0.6f, 1f)
            );

            breakthrough_fail_death_3 = RegisterLogAsset(
                "breakthrough_fail_death_3",
                "ui/NianLi",
                new Color(1f, 0.2f, 0.2f)
            );
            breakthrough_fail_death_4 = RegisterLogAsset(
                "breakthrough_fail_death_4",
                "ui/NianLi",
                new Color(1f, 0.2f, 0.2f)
            );
            breakthrough_fail_death_5 = RegisterLogAsset(
                "breakthrough_fail_death_5",
                "ui/NianLi",
                new Color(1f, 0.2f, 0.2f)
            );
            breakthrough_fail_noway_3 = RegisterLogAsset(
                "breakthrough_fail_noway_3",
                "ui/NianLi",
                new Color(0.5f, 0.5f, 0.5f)
            );
            breakthrough_fail_noway_4 = RegisterLogAsset(
                "breakthrough_fail_noway_4",
                "ui/NianLi",
                new Color(0.5f, 0.5f, 0.5f)
            );
            breakthrough_fail_noway_5 = RegisterLogAsset(
                "breakthrough_fail_noway_5",
                "ui/NianLi",
                new Color(0.5f, 0.5f, 0.5f)
            );
        }

        private static WorldLogAsset RegisterLogAsset(string id, string iconPath, Color color)
        {
            if (AssetManager.world_log_library.has(id))
                return AssetManager.world_log_library.get(id);

            var asset = new WorldLogAsset
            {
                id = id,
                group = LOG_GROUP,
                path_icon = iconPath,
                color = color,
                text_replacer = delegate (WorldLogMessage pMessage, ref string pText)
                {
                    string special = pMessage.getSpecial(1);
                    if (!string.IsNullOrEmpty(special))
                        pText = special;
                }
            };

            return AssetManager.world_log_library.add(asset);
        }

        // ========== 添加日志 ==========
        public static void AddLog(string logKey, string message, Actor actor, Color color, bool showWorldTip = true)
        {
            if (actor == null) return;

            // ★ 日志写入保持 logError: true（默认），便于发现关键问题
            SafeExecutor.Execute(() =>
            {
                WorldLogAsset asset = GetAssetByKey(logKey);
                if (asset == null)
                {
                    if (showWorldTip)
                        WorldTip.showNow(message, true, "top");
                    if (ConfigCache.EnableDebugLog)
                    {
                        Debug.Log($"[念力修行历史] {message}");
                    }
                    return;
                }

                var logMessage = new WorldLogMessage(asset, message, null, null);
                logMessage.unit = actor;
                logMessage.unit_id = actor.getID();

                if (actor.current_tile != null)
                {
                    logMessage.location = actor.current_tile.pos;
                }

                logMessage.color_special1 = color;

                logMessage.add();

                if (showWorldTip)
                    WorldTip.showNow(message, true, "top");

                if (ConfigCache.EnableDebugLog)
                {
                    Debug.Log($"[念力修行历史] {message}");
                }
            }, $"添加日志 {logKey}");  // ★ 默认 logError: true
        }

        // ========== 根据Key获取日志资产 ==========
        private static WorldLogAsset GetAssetByKey(string logKey)
        {
            switch (logKey)
            {
                case "breakthrough_realm_4": return breakthrough_realm_4;
                case "breakthrough_realm_5": return breakthrough_realm_5;
                case "heart_break_4": return heart_break_4;
                case "heart_break_5": return heart_break_5;
                case "ascension_success": return ascension_success;
                case "ascension_fail": return ascension_fail;
                case "pioneer_realm_1": return pioneer_realm_1;
                case "pioneer_realm_2": return pioneer_realm_2;
                case "pioneer_realm_3": return pioneer_realm_3;
                case "pioneer_realm_4": return pioneer_realm_4;
                case "pioneer_realm_5": return pioneer_realm_5;
                case "kill_realm_4": return kill_realm_4;
                case "kill_realm_5": return kill_realm_5;
                case "heart_fail": return heart_fail;
                case "minor_ascension_success": return minor_ascension_success;
                case "breakthrough_fail_death_3": return breakthrough_fail_death_3;
                case "breakthrough_fail_death_4": return breakthrough_fail_death_4;
                case "breakthrough_fail_death_5": return breakthrough_fail_death_5;
                case "breakthrough_fail_noway_3": return breakthrough_fail_noway_3;
                case "breakthrough_fail_noway_4": return breakthrough_fail_noway_4;
                case "breakthrough_fail_noway_5": return breakthrough_fail_noway_5;
                default: return null;
            }
        }

        // ==========================================
        // 以下为兼容旧版接口（已废弃）
        // ==========================================
        [Obsolete("日志已写入游戏原版历史系统，请使用 WitchcraftWorldLog.AddLog()")]
        public static List<WitchcraftLogEntry> GetAllLogs()
        {
            return new List<WitchcraftLogEntry>();
        }

        [Obsolete("日志已写入游戏原版历史系统，此方法已无实际作用")]
        public static void ClearLogs() { }

        [Obsolete("日志已写入游戏原版历史系统，此属性已无实际作用")]
        public static int Count => 0;
    }

    [Obsolete("日志已写入游戏原版历史系统，此类已废弃")]
    public class WitchcraftLogEntry
    {
        public string LogKey;
        public string Message;
        public string ActorName;
        public double Timestamp;
        public Color Color;
    }
}