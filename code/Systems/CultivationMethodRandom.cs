using System.Collections.Generic;
using UnityEngine;
using NeoModLoader.General;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Core
{
    /// <summary>
    /// 功法随机赐予与世界传播（单一职责：只负责随机发放逻辑）。
    /// </summary>
    public static partial class CultivationMethodSystem
    {
        private static readonly Dictionary<string, float> _lastGrantTimes = new Dictionary<string, float>();

        /// <summary>给单个角色随机一门功法（仅当其尚未拥有任何功法时）。</summary>
        public static bool GrantRandomMethod(Actor actor)
        {
            if (actor == null) return false;
            if (GetMethods(actor).Count > 0) return false;

            MethodData selected = PickRandomMethod();
            if (selected == null) return false;

            AddMethod(actor, selected.Method);

            Color grantColor = ParseColor(selected.ColorHex);
            DaoBeiSystem.AddRecord(actor,
                LocalizedTextManager.getText("dao_event_grant"),
                string.Format(LocalizedTextManager.getText("method_granted_desc"), LocalizedTextManager.getText(selected.NameKey)),
                grantColor);

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] 赐予 {actor.data.name} 功法：{LocalizedTextManager.getText(selected.NameKey)}");
            return true;
        }

        /// <summary>按稀有度权重在世界范围内随机传播一门功法给一个随机智者。</summary>
        public static void TryGrantRandomMethodToRandomActor()
        {
            if (World.world?.units == null) return;

            var customData = World.world.map_stats.custom_data;
            const string checkKey = "nianli_last_method_grant_check";
            customData.get(checkKey, out float lastCheckTime, 0f);
            float currentTime = (float)World.world.map_stats.world_time;
            if (currentTime - lastCheckTime < 10f) return;
            customData.set(checkKey, currentTime);

            if (Random.Range(0f, 1f) > 0.12f) return;

            List<Actor> candidates = new List<Actor>();
            foreach (Actor actor in World.world.units)
            {
                if (actor != null && actor.isAlive() && actor.isSapient() && actor.data != null)
                    candidates.Add(actor);
            }
            if (candidates.Count == 0) return;

            MethodData selected = PickWeightedRandomMethod();
            if (selected == null) return;

            float interval = GetMethodInterval(selected.Rarity);
            string methodKey = selected.Method.ToString();
            if (!_lastGrantTimes.ContainsKey(methodKey))
                _lastGrantTimes[methodKey] = 0f;
            if (currentTime - _lastGrantTimes[methodKey] < interval) return;

            Actor target = candidates[Random.Range(0, candidates.Count)];
            AddMethod(target, selected.Method);
            _lastGrantTimes[methodKey] = currentTime;

            string msg = string.Format(LM.Get("method_granted_worldtip"), target.data.name, LocalizedTextManager.getText(selected.NameKey));
            WorldTip.showNow(msg, true, "top");

            Color grantColor = ParseColor(selected.ColorHex);
            DaoBeiSystem.AddRecord(target, "功法传承", $"获得 {LocalizedTextManager.getText(selected.NameKey)}", grantColor);
        }

        private static MethodData PickRandomMethod()
        {
            List<MethodData> available = CollectNonNoneMethods();
            return available.Count == 0 ? null : available[Random.Range(0, available.Count)];
        }

        private static MethodData PickWeightedRandomMethod()
        {
            List<MethodData> available = CollectNonNoneMethods();
            if (available.Count == 0) return null;

            float totalWeight = 0f;
            foreach (var data in available)
                totalWeight += GetMethodWeight(data.Rarity);

            float roll = Random.Range(0f, totalWeight);
            float cum = 0f;
            foreach (var data in available)
            {
                cum += GetMethodWeight(data.Rarity);
                if (roll <= cum) return data;
            }
            return available[available.Count - 1];
        }

        private static List<MethodData> CollectNonNoneMethods()
        {
            var list = new List<MethodData>();
            foreach (var data in AllMethods)
                if (data.Method != CultivationMethod.None)
                    list.Add(data);
            return list;
        }

        private static Color ParseColor(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out Color c))
                c = Color.white;
            return c;
        }

        private static float GetMethodWeight(MethodRarity rarity)
        {
            switch (rarity)
            {
                case MethodRarity.Common:    return 5f;
                case MethodRarity.Uncommon:   return 4f;
                case MethodRarity.Rare:       return 3f;
                case MethodRarity.Epic:      return 2f;
                case MethodRarity.Legendary:  return 1f;
                default: return 1f;
            }
        }

        private static float GetMethodInterval(MethodRarity rarity)
        {
            switch (rarity)
            {
                case MethodRarity.Common:    return 10f;
                case MethodRarity.Uncommon:   return 13f;
                case MethodRarity.Rare:       return 16f;
                case MethodRarity.Epic:      return 20f;
                case MethodRarity.Legendary:  return 25f;
                default: return 10f;
            }
        }
    }
}
