using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.UI;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Systems
{
    public static class BrokenHeartSystem
    {
        private const string BROKEN_COUNT_KEY = "nianli.broken_count";
        private const string BROKEN_STATE_KEY = "nianli.broken_state";

        public static void RecordHeartRegress(Actor actor)
        {
            if (actor == null) return;
            actor.data.get(BROKEN_COUNT_KEY, out int count, 0);
            count++;
            actor.data.set(BROKEN_COUNT_KEY, count);

            if (count >= 3)
            {
                SetBrokenHeartState(actor, true);
                string msg = string.Format(LM.Get("heart_broken_trigger"), actor.data.name);
                WorldTip.showNow(msg, true, "top");
                WitchcraftWorldLog.AddLog("heart_broken", msg, actor, new Color(0.5f, 0.2f, 0.2f));
                DaoBeiSystem.AddRecord(actor, LM.Get("dao_broken_heart"), LM.Get("broken_heart_daobei"), new Color(0.5f, 0.2f, 0.2f));
            }
            else if (IsBrokenHeart(actor))
            {
                // ★★★ 已破碎时再倒退，叠加额外惩罚（速度-10%） ★★★
                actor.stats["speed"] *= 0.9f;
                string msg = $"💔 {actor.data.name} 道心再受重创，速度永久下降10%！";
                WorldTip.showNow(msg, true, "top");
                WitchcraftWorldLog.AddLog("heart_regress_extra", msg, actor, new Color(0.5f, 0.2f, 0.2f));
            }
        }

        public static bool IsBrokenHeart(Actor actor)
        {
            if (actor == null) return false;
            actor.data.get(BROKEN_STATE_KEY, out bool val, false);
            return val;
        }

        public static void SetBrokenHeartState(Actor actor, bool broken)
        {
            if (actor == null) return;
            actor.data.set(BROKEN_STATE_KEY, broken);

            if (broken)
            {
                if (!actor.hasTrait(TraitIds.Trait_BrokenHeart))
                    actor.addTrait(TraitIds.Trait_BrokenHeart);
            }
            else
            {
                if (actor.hasTrait(TraitIds.Trait_BrokenHeart))
                    actor.removeTrait(TraitIds.Trait_BrokenHeart);
                actor.data.set(BROKEN_COUNT_KEY, 0);
            }
            NianLiInfoPanel.MarkDirty();
        }

        public static float GetBrokenHeartPenalty(Actor actor)
        {
            if (actor != null && actor.hasTrait(TraitIds.Trait_BrokenHeart))
                return 0.5f;
            if (IsBrokenHeart(actor)) return 0.5f;
            return 0f;
        }

        public static void RepairBrokenHeart(Actor actor)
        {
            if (actor == null || !IsBrokenHeart(actor)) return;
            SetBrokenHeartState(actor, false);
            string msg = string.Format(LM.Get("heart_repair"), actor.data.name);
            WorldTip.showNow(msg, true, "top");
            WitchcraftWorldLog.AddLog("heart_repair", msg, actor, new Color(0.3f, 0.8f, 0.3f));
            DaoBeiSystem.AddRecord(actor, LM.Get("dao_repair_heart"), LM.Get("repair_heart_daobei"), new Color(0.3f, 0.8f, 0.3f));
        }
    }
}