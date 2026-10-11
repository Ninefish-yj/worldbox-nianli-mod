using UnityEngine;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Utils
{
    /// <summary>
    /// 统一日志工具
    /// 支持日志级别控制
    /// </summary>
    public static class NianLiLogger
    {
        public enum LogLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Debug = 4
        }

        public static LogLevel CurrentLevel { get; set; } = LogLevel.Info;

        private const string PREFIX = "[念力修行] ";

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (level > CurrentLevel) return;
            Debug.Log(PREFIX + message);
        }

        public static void LogWarning(string message, LogLevel level = LogLevel.Warning)
        {
            if (level > CurrentLevel) return;
            Debug.LogWarning(PREFIX + message);
        }

        public static void LogError(string message, LogLevel level = LogLevel.Error)
        {
            if (level > CurrentLevel) return;
            Debug.LogError(PREFIX + message);
        }

        public static void LogException(System.Exception ex, LogLevel level = LogLevel.Error)
        {
            if (level > CurrentLevel) return;
            Debug.LogException(ex);
        }

        public static void LogDebug(string message)
        {
            if (CurrentLevel < LogLevel.Debug) return;
            if (!ConfigCache.EnableDebugLog) return;
            Debug.Log(PREFIX + "[DEBUG] " + message);
        }
    }
}