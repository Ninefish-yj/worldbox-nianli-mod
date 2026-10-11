using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems.Events;
using NeoModLoader.General;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Systems
{
    public static partial class GrowthSystem
    {
        // ============================================================
        // ★★★ 法则与小世界逻辑（每五年一次判定） ★★★
        // ============================================================

        private static float GetSmallWorldLawBonus(Actor actor)
        {
            if (actor.GetRealmLevel() < 4) return 0f;
            // 按小世界等级递加法则领悟率：初生2% / 成长4% / 融汇6% / 完整10%
            if (actor.hasTrait(TraitIds.SmallWorld_4)) return 0.10f;
            if (actor.hasTrait(TraitIds.SmallWorld_3)) return 0.06f;
            if (actor.hasTrait(TraitIds.SmallWorld_2)) return 0.04f;
            if (actor.hasTrait(TraitIds.SmallWorld_1)) return 0.02f;
            return 0f;
        }

        private static string GetSmallWorldName(int lawCount)
        {
            switch (lawCount)
            {
                case 1: return "小世界·初生";
                case 2: return "小世界·成长";
                case 3: return "小世界·融汇";
                case 4: return "小世界·完整";
                default: return "";
            }
        }

        private static void UpdateSmallWorldTrait(Actor actor)
        {
            if (actor == null) return;
            
            string[] smallWorldTraits = { TraitIds.SmallWorld_1, TraitIds.SmallWorld_2, TraitIds.SmallWorld_3, TraitIds.SmallWorld_4 };
            foreach (string traitId in smallWorldTraits)
                if (actor.hasTrait(traitId)) actor.removeTrait(traitId);

            int lawCount = actor.GetLawCount();
            string newTrait = null;
            switch (lawCount)
            {
                case 1: newTrait = TraitIds.SmallWorld_1; break;
                case 2: newTrait = TraitIds.SmallWorld_2; break;
                case 3: newTrait = TraitIds.SmallWorld_3; break;
                case 4: newTrait = TraitIds.SmallWorld_4; break;
                default: return;
            }
            
            if (!actor.hasTrait(newTrait))
            {
                actor.addTrait(newTrait);
                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] {actor.data.name} 获得 {GetSmallWorldName(lawCount)}（{lawCount}道法则）");
            }
        }

        // ★★★ 修改后的 TryGainLaw：每五年判定一次 ★★★
        private static void TryGainLaw(Actor actor)
        {
            if (actor == null || !actor.isSapient()) return;
            if (actor.GetRealmLevel() < Constants.STAYING_MIN_REALM) return;
            if (actor.HasAllLaws()) return;

            // ---- 五年冷却检查 ----
            int currentYear = World.world?.map_stats?.year ?? -1;
            if (currentYear < 0) return;

            actor.data.get(DataKeys.LAST_LAW_CHECK_YEAR, out int lastCheckYear, 0);
            if (currentYear - lastCheckYear < 5) return; // 未满5年，不进行判定

            // 更新检查年份（无论是否领悟，都更新）
            actor.data.set(DataKeys.LAST_LAW_CHECK_YEAR, currentYear);

            // ---- 概率判定 ----
            float baseChance = 0.02f; // 每五年基础2%
            float smallWorldBonus = GetSmallWorldLawBonus(actor); // 环界境以上+5%
            float totalChance = baseChance + smallWorldBonus;

            if (Random.Range(0f, 1f) > totalChance) return; // 未触发

            // ---- 随机选择一条未领悟的法则 ----
            string[] laws = { TraitIds.Law_Reincarnation, TraitIds.Law_YinYang, TraitIds.Law_TimeSpace, TraitIds.Law_CauseEffect };
            string[] lawNames = { 
                LM.Get("nianli_law_reincarnation"), 
                LM.Get("nianli_law_yinyang"), 
                LM.Get("nianli_law_timespace"), 
                LM.Get("nianli_law_causeeffect") 
            };
            Color[] lawColors = {
                new Color(0.5f, 0.3f, 1f),
                new Color(0.5f, 0.5f, 0.5f),
                new Color(0.2f, 0.8f, 1f),
                new Color(1f, 0.8f, 0.2f)
            };
            
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < laws.Length; i++)
                if (!actor.hasTrait(laws[i])) availableIndices.Add(i);
            if (availableIndices.Count == 0) return;

            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            string lawTrait = laws[randomIndex];
            string lawName = lawNames[randomIndex];
            actor.addTrait(lawTrait);
            NianLiInfoPanel.MarkDirty();
            actor.AddCycleLawProgress(1);

            // ★★★ 特效（单粒子） ★★★
            SpawnLawEffect(actor, lawColors[randomIndex]);

            if (actor.GetRealmLevel() >= 4) UpdateSmallWorldTrait(actor);
            
            WorldMessageSystem.Broadcast($"{actor.data.name} 领悟了 {lawName} 法则！");
            XiuXingEventCenter.TriggerLawGain(actor, lawName, randomIndex);
            
            if (actor.HasAllLaws())
                WorldMessageSystem.Broadcast($"「轮回·阴阳·时空·因果」\n{actor.data.name} 已集齐四大法则！");
        }

        // ★★★ 特效生成（无延迟） ★★★
        private static void SpawnLawEffect(Actor actor, Color color)
        {
            if (actor?.current_tile == null) return;
            try
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_sparkle", actor.current_tile, 0.8f);
                if (effect?.sprite_renderer != null)
                {
                    effect.sprite_renderer.color = color;
                    Object.Destroy(effect.gameObject, 1.5f);
                }
            }
            catch (System.Exception e)
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.LogWarning($"[念力修行] 法则特效生成失败: {e.Message}");
            }
        }

        private static void TryEnterStaying(Actor actor)
        {
            if (actor == null || !actor.isSapient()) return;
            if (actor.GetRealmLevel() < Constants.STAYING_MIN_REALM) return;
            if (actor.IsStaying()) return;
            
            int lawCount = actor.GetLawCount();
            float stayingChance = lawCount == 0 ? 1.0f : (lawCount >= 4 ? 0f : 1.0f - lawCount * 0.10f);
            if (Random.Range(0f, 1f) > stayingChance) return;
            
            float duration = Random.Range(Constants.STAYING_DURATION_MIN, Constants.STAYING_DURATION_MAX);
            actor.EnterStaying(duration);
        }
    }
}