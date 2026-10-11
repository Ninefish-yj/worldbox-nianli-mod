using UnityEngine;
using System.Collections.Generic;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Core
{
    public static partial class ActorExtensions
    {
        private const string CYCLE_LAW_PROGRESS_KEY = DataKeys.CYCLE_LAW_PROGRESS;

        // ---- 法则 ----
        public static int GetCycleLawProgress(this Actor actor)
        {
            actor.data.get(CYCLE_LAW_PROGRESS_KEY, out int val, 0);
            return val;
        }
        public static void SetCycleLawProgress(this Actor actor, int val)
        {
            actor.data.set(CYCLE_LAW_PROGRESS_KEY, Mathf.Clamp(val, 0, 4));
        }
        public static void AddCycleLawProgress(this Actor actor, int val)
        {
            int current = actor.GetCycleLawProgress();
            actor.SetCycleLawProgress(current + val);
        }

        public static bool HasAllLaws(this Actor actor)
        {
            return actor.hasTrait(TraitIds.Law_Reincarnation) &&
                   actor.hasTrait(TraitIds.Law_YinYang) &&
                   actor.hasTrait(TraitIds.Law_TimeSpace) &&
                   actor.hasTrait(TraitIds.Law_CauseEffect);
        }

        public static int GetLawCount(this Actor actor)
        {
            int count = 0;
            if (actor.hasTrait(TraitIds.Law_Reincarnation)) count++;
            if (actor.hasTrait(TraitIds.Law_YinYang)) count++;
            if (actor.hasTrait(TraitIds.Law_TimeSpace)) count++;
            if (actor.hasTrait(TraitIds.Law_CauseEffect)) count++;
            return count;
        }

        public static List<string> GetLawNames(this Actor actor)
        {
            var laws = new List<string>();
            if (actor.hasTrait(TraitIds.Law_Reincarnation)) laws.Add(LM.Get("nianli_law_reincarnation"));
            if (actor.hasTrait(TraitIds.Law_YinYang)) laws.Add(LM.Get("nianli_law_yinyang"));
            if (actor.hasTrait(TraitIds.Law_TimeSpace)) laws.Add(LM.Get("nianli_law_timespace"));
            if (actor.hasTrait(TraitIds.Law_CauseEffect)) laws.Add(LM.Get("nianli_law_causeeffect"));
            return laws;
        }
    }
}