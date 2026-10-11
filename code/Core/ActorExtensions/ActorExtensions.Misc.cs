using UnityEngine;

namespace NianLiXiuXing.code.Core
{
    public static partial class ActorExtensions
    {
        // ---- 死亡 ----
        private const string DEATH_COUNT_KEY = DataKeys.DEATH_COUNT;
        private const string RESURRECTION_COUNT_KEY = DataKeys.RESURRECTION_COUNT;
        private const string REINCARNATION_COUNT_KEY = DataKeys.REINCARNATION_COUNT;

        public static int GetDeathCount(this Actor actor)
        {
            actor.data.get(DEATH_COUNT_KEY, out int val, 0);
            return val;
        }
        public static void SetDeathCount(this Actor actor, int val)
        {
            actor.data.set(DEATH_COUNT_KEY, Mathf.Max(0, val));
            actor.stats["DeathCount"] = Mathf.Max(0, val);
        }
        public static void AddDeathCount(this Actor actor)
        {
            int current = actor.GetDeathCount();
            actor.SetDeathCount(current + 1);
        }
        public static bool HasDiedBefore(this Actor actor)
        {
            return actor.GetDeathCount() > 0;
        }

        // ---- 复活 ----
        public static int GetResurrectionCount(this Actor actor)
        {
            actor.data.get(RESURRECTION_COUNT_KEY, out int val, 0);
            return val;
        }
        public static void SetResurrectionCount(this Actor actor, int val)
        {
            actor.data.set(RESURRECTION_COUNT_KEY, Mathf.Max(0, val));
        }
        public static void AddResurrectionCount(this Actor actor)
        {
            int current = actor.GetResurrectionCount();
            actor.SetResurrectionCount(current + 1);
        }

        public static float GetResurrectionChance(this Actor actor)
        {
            int count = actor.GetResurrectionCount();
            return Mathf.Max(0f, 0.90f - count * 0.10f);
        }
        public static bool CanResurrect(this Actor actor)
        {
            return actor.GetResurrectionChance() > 0f;
        }

        // ---- 轮回 ----
        public static int GetReincarnationCount(this Actor actor)
        {
            actor.data.get(REINCARNATION_COUNT_KEY, out int val, 0);
            return val;
        }
        public static void AddReincarnationCount(this Actor actor)
        {
            int current = actor.GetReincarnationCount();
            actor.data.set(REINCARNATION_COUNT_KEY, current + 1);
        }

        // ---- 冥想 ----
        private const string IS_MEDITATING_KEY = DataKeys.IS_MEDITATING;

        public static bool IsMeditating(this Actor actor)
        {
            actor.data.get(IS_MEDITATING_KEY, out bool val, false);
            return val;
        }
        public static void SetMeditating(this Actor actor, bool val)
        {
            actor.data.set(IS_MEDITATING_KEY, val);
        }

        // ---- 驻足 ----
        private const string IS_STAYING_KEY = DataKeys.IS_STAYING;
        private const string STAYING_START_YEAR_KEY = DataKeys.STAYING_START_YEAR;
        private const string STAYING_DURATION_KEY =  DataKeys.STAYING_DURATION;

        public static void SetStaying(this Actor actor, bool val)
        {
            actor.data.set(IS_STAYING_KEY, val);
        }
        public static float GetStayingStartYear(this Actor actor)
        {
            actor.data.get(STAYING_START_YEAR_KEY, out float val, -1f);
            return val;
        }
        public static void SetStayingStartYear(this Actor actor, float year)
        {
            actor.data.set(STAYING_START_YEAR_KEY, year);
        }
        public static float GetStayingDuration(this Actor actor)
        {
            actor.data.get(STAYING_DURATION_KEY, out float val, 0f);
            return val;
        }
        public static void SetStayingDuration(this Actor actor, float duration)
        {
            actor.data.set(STAYING_DURATION_KEY, duration);
        }

        public static bool IsStaying(this Actor actor)
        {
            actor.data.get(IS_STAYING_KEY, out bool flag, false);
            if (!flag) return false;
            float startYear = actor.GetStayingStartYear();
            float duration = actor.GetStayingDuration();
            if (startYear < 0 || duration <= 0) return false;
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            float yearFloat = (float)Date.getYear(currentYear);
            if (yearFloat - startYear >= duration)
            {
                actor.SetStaying(false);
                actor.SetStayingStartYear(-1);
                actor.SetStayingDuration(0);
                return false;
            }
            return true;
        }

        public static void EnterStaying(this Actor actor, float duration)
        {
            if (duration <= 0) return;
            if (actor.GetRealmLevel() < ConfigCache.StayingMinRealm) return;
            if (actor.IsStaying()) return;
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float yearFloat = (float)Date.getYear((float)currentWorldTime);
            actor.SetStaying(true);
            actor.SetStayingStartYear(yearFloat);
            actor.SetStayingDuration(duration);
        }

        // ---- 自在不灭 ----
        private const string ZEN_WU_WEI_KEY = DataKeys.ZEN_WU_WEI;
        private const string ZEN_WU_WEI_TIMER_KEY = DataKeys.ZEN_WU_WEI_TIMER;
        private const string ZEN_WU_WEI_COOLDOWN_KEY = DataKeys.ZEN_WU_WEI_COOLDOWN;

        public static bool IsZenWuWei(this Actor actor)
        {
            actor.data.get(ZEN_WU_WEI_KEY, out bool val, false);
            return val;
        }
        public static void SetZenWuWei(this Actor actor, bool val)
        {
            actor.data.set(ZEN_WU_WEI_KEY, val);
        }
        public static float GetZenWuWeiTimer(this Actor actor)
        {
            actor.data.get(ZEN_WU_WEI_TIMER_KEY, out float val, 0f);
            return val;
        }
        public static void SetZenWuWeiTimer(this Actor actor, float val)
        {
            actor.data.set(ZEN_WU_WEI_TIMER_KEY, Mathf.Max(0f, val));
        }
        public static float GetZenWuWeiCooldown(this Actor actor)
        {
            actor.data.get(ZEN_WU_WEI_COOLDOWN_KEY, out float val, 0f);
            return val;
        }
        public static void SetZenWuWeiCooldown(this Actor actor, float val)
        {
            actor.data.set(ZEN_WU_WEI_COOLDOWN_KEY, Mathf.Max(0f, val));
        }

        public static bool CanUseZenWuWei(this Actor actor)
        {
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            return currentYear >= actor.GetZenWuWeiCooldown();
        }

        public static void TriggerZenWuWei(this Actor actor)
        {
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            actor.SetZenWuWei(true);
            actor.SetZenWuWeiTimer(1f / 365f);
            actor.SetZenWuWeiCooldown(currentYear + 30f);
        }

        public static void UpdateZenWuWeiTimer(this Actor actor, float elapsed)
        {
            if (!actor.IsZenWuWei()) return;
            float timer = actor.GetZenWuWeiTimer();
            timer -= elapsed;
            if (timer <= 0f)
            {
                timer = 0f;
                actor.SetZenWuWei(false);
            }
            actor.SetZenWuWeiTimer(timer);
        }

        // ---- 法力联动 ----
        private const string LAST_MANA_CONVERT_KEY = "nianli.LastManaConvert";

        public static float GetManaConvertCooldown(this Actor actor)
        {
            actor.data.get(LAST_MANA_CONVERT_KEY, out float val, 0f);
            return val;
        }
        public static void SetManaConvertCooldown(this Actor actor, float year)
        {
            actor.data.set(LAST_MANA_CONVERT_KEY, year);
        }

        public static float GetManaSpeedBonus(this Actor actor)
        {
            if (actor == null) return 0f;
            int mana = actor.getMana();
            return Mathf.Clamp((mana / 1000f) * 0.05f, 0f, 0.30f);
        }

        public static float GetManaConvertMultiplier(this Actor actor)
        {
            if (actor == null) return 1f;
            int realm = actor.GetRealmLevel();
            float[] multipliers = { 1f, 1f, 1.2f, 1.5f, 2f, 3f };
            return realm < multipliers.Length ? multipliers[realm] : multipliers[multipliers.Length - 1];
        }
    }
}