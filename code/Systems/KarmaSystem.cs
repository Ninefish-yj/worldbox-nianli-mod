using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.UI;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Systems
{
    public static class KarmaSystem
    {
        private const string KARMA_KEY = "nianli.karma";
        private const string VIRTUE_KEY = "nianli.virtue";
        private const string LAST_PUNISHMENT_KEY = "nianli.last_karma_punishment";
        private const float MAX_KARMA = 100f;
        private const float MAX_VIRTUE = 100f;

        public static float GetKarma(Actor actor)
        {
            if (actor == null) return 0f;
            actor.data.get(KARMA_KEY, out float val, 0f);
            return Mathf.Clamp(val, 0f, MAX_KARMA);
        }

        public static void SetKarma(Actor actor, float val)
        {
            if (actor == null) return;
            actor.data.set(KARMA_KEY, Mathf.Clamp(val, 0f, MAX_KARMA));
            NianLiInfoPanel.MarkDirty();
        }

        public static float GetVirtue(Actor actor)
        {
            if (actor == null) return 0f;
            actor.data.get(VIRTUE_KEY, out float val, 0f);
            return Mathf.Clamp(val, 0f, MAX_VIRTUE);
        }

        public static void SetVirtue(Actor actor, float val)
        {
            if (actor == null) return;
            actor.data.set(VIRTUE_KEY, Mathf.Clamp(val, 0f, MAX_VIRTUE));
            NianLiInfoPanel.MarkDirty();
        }

        public static void AddVirtue(Actor actor, float amount)
        {
            if (actor == null) return;
            float current = GetVirtue(actor);
            SetVirtue(actor, current + amount);
        }

        public static void AddKarma(Actor actor, float amount)
        {
            if (actor == null) return;
            float current = GetKarma(actor);
            SetKarma(actor, current + amount);
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {actor.data.name} 业力 +{amount}，当前 {current + amount:F0}");
            CheckKarmaPunishment(actor);
        }

        public static void ReduceKarma(Actor actor, float amount)
        {
            if (actor == null) return;
            float current = GetKarma(actor);
            SetKarma(actor, current - amount);
        }

        // ★★★ 业力天谴：只有闪电，无光圈 ★★★
        private static void CheckKarmaPunishment(Actor actor)
        {
            if (actor == null) return;
            float karma = GetKarma(actor);
            if (karma < MAX_KARMA) return;

            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            actor.data.get(LAST_PUNISHMENT_KEY, out float lastYear, -100f);
            if (currentYear - lastYear < 1f) return;

            actor.data.set(LAST_PUNISHMENT_KEY, currentYear);

            if (actor.current_tile != null)
            {
                BaseEffect lightning = EffectsLibrary.spawnAtTile("fx_lightning_small", actor.current_tile, 2f);
                if (lightning?.sprite_renderer != null)
                {
                    lightning.sprite_renderer.color = Color.white;
                    Object.Destroy(lightning.gameObject, 1f);
                }
                // ★★★ 移除 fx_explosion_wave ★★★
            }

            int damage = Mathf.RoundToInt(actor.getMaxHealth() * 0.3f);
            actor.changeHealth(-damage);

            string msg = $"⚡ {actor.data.name} 业力滔天，遭天谴！";
            WorldTip.showNow(msg, true, "top");
            WitchcraftWorldLog.AddLog("karma_punishment", msg, actor, new Color(1f, 0.5f, 0f));

            SetKarma(actor, karma - 20f);
        }

        public static void ProcessYearlyKarmaDecay(Actor actor)
        {
            if (actor == null) return;
            float karma = GetKarma(actor);
            if (karma <= 0) return;

            float virtue = GetVirtue(actor);
            float insight = actor.GetInsightLevel();
            float decay = (1f + insight * 0.5f) * (1f + virtue / 100f);
            SetKarma(actor, Mathf.Max(0, karma - decay));
        }

        public static float GetKarmaPenalty(Actor actor)
        {
            float karma = GetKarma(actor);
            if (karma <= 0) return 0f;
            return (karma / 10f) * 0.05f;
        }

        public static bool PerformCleanse(Actor actor)
        {
            if (actor == null) return false;
            float karma = GetKarma(actor);
            if (karma <= 0)
            {
                WorldTip.showNow("业力已清零，无需净化", true, "top");
                return false;
            }

            float cost = karma * ConfigCache.KarmaCleanseCostPerPoint;
            if (actor.GetNianLi() < cost)
            {
                WorldTip.showNow($"念力不足（需要 {cost:F0}）", true, "top");
                return false;
            }

            actor.ChangeNianLi(-cost);
            ReduceKarma(actor, karma);
            AddVirtue(actor, karma * 0.1f);
            WorldTip.showNow($"🧘 净心仪式完成，业力清零！消耗 {cost:F0} 念力", true, "top");
            return true;
        }
    }
}