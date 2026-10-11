using System;
using UnityEngine;

namespace NianLiXiuXing.code.Utils
{
    public static class SafeExecutor
    {
        public static void Execute(Action action, string context = "", bool logError = true)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                if (logError)
                    Debug.LogError($"[念力修行] {context} 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static T Execute<T>(Func<T> func, T defaultValue = default, string context = "", bool logError = true)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (logError)
                    Debug.LogError($"[念力修行] {context} 异常: {ex.Message}\n{ex.StackTrace}");
                return defaultValue;
            }
        }
    }
}