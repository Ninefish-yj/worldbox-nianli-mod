using System;
using System.Collections.Generic;
using UnityEngine;

namespace NianLiXiuXing.code.Utils
{
    /// <summary>
    /// 集合操作辅助工具
    /// 减少 foreach 分配，提供高效的遍历方式
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// 使用 for 循环遍历 List（避免 foreach 迭代器分配）
        /// </summary>
        public static void ForEach<T>(this List<T> list, Action<T> action)
        {
            if (list == null || action == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i]);
            }
        }

        /// <summary>
        /// 使用 for 循环遍历 List（带索引）
        /// </summary>
        public static void ForEachWithIndex<T>(this List<T> list, Action<T, int> action)
        {
            if (list == null || action == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }

        /// <summary>
        /// 使用 for 循环遍历数组
        /// </summary>
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null || action == null) return;
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }

        /// <summary>
        /// 遍历 Dictionary 的 Keys（避免 KeyValuePair 分配）
        /// </summary>
        public static void ForEachKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey> action)
        {
            if (dict == null || action == null) return;
            var keys = dict.Keys;
            foreach (var key in keys)
            {
                action(key);
            }
        }

        /// <summary>
        /// 遍历 Dictionary 的 Values
        /// </summary>
        public static void ForEachValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TValue> action)
        {
            if (dict == null || action == null) return;
            var values = dict.Values;
            foreach (var value in values)
            {
                action(value);
            }
        }

        /// <summary>
        /// 获取 List 的只读引用（避免复制）
        /// </summary>
        public static IReadOnlyList<T> AsReadOnly<T>(this List<T> list)
        {
            return list;
        }

        /// <summary>
        /// 安全获取 List 元素（带空检查）
        /// </summary>
        public static T GetSafe<T>(this List<T> list, int index, T defaultValue = default)
        {
            if (list == null || index < 0 || index >= list.Count)
                return defaultValue;
            return list[index];
        }

        /// <summary>
        /// 安全获取 Dictionary 值
        /// </summary>
        public static TValue GetSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null) return defaultValue;
            if (dict.TryGetValue(key, out var value))
                return value;
            return defaultValue;
        }

        /// <summary>
        /// 高效移除符合条件的元素（原地操作，不分配新 List）
        /// </summary>
        public static int RemoveWhere<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null) return 0;
            int removeCount = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                    removeCount++;
                }
            }
            return removeCount;
        }

        /// <summary>
        /// 高效查找第一个匹配元素
        /// </summary>
        public static T FindFirst<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null || match == null) return default;
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                    return list[i];
            }
            return default;
        }

        /// <summary>
        /// 获取数组的快速枚举器（避免迭代器分配）
        /// </summary>
        public static ArrayEnumerator<T> GetFastEnumerator<T>(this T[] array)
        {
            return new ArrayEnumerator<T>(array);
        }

        /// <summary>
        /// 快速数组枚举器结构体（零分配）
        /// </summary>
        public struct ArrayEnumerator<T>
        {
            private readonly T[] _array;
            private int _index;

            public ArrayEnumerator(T[] array)
            {
                _array = array;
                _index = -1;
            }

            public bool MoveNext()
            {
                _index++;
                return _array != null && _index < _array.Length;
            }

            public T Current => _array[_index];

            public void Reset()
            {
                _index = -1;
            }
        }

        /// <summary>
        /// 获取 List 的快速枚举器（避免迭代器分配）
        /// </summary>
        public static ListEnumerator<T> GetFastEnumerator<T>(this List<T> list)
        {
            return new ListEnumerator<T>(list);
        }

        /// <summary>
        /// 快速 List 枚举器结构体（零分配）
        /// </summary>
        public struct ListEnumerator<T>
        {
            private readonly List<T> _list;
            private int _index;

            public ListEnumerator(List<T> list)
            {
                _list = list;
                _index = -1;
            }

            public bool MoveNext()
            {
                _index++;
                return _list != null && _index < _list.Count;
            }

            public T Current => _list[_index];

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}