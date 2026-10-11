using UnityEngine;
using NianLiXiuXing.code.Core;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Systems
{
    public static class RandomEventsSystem
    {
        private static float _lastEventTime = 0f;
        private const float EVENT_INTERVAL = 60f;

        public static void CheckRandomEvents(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;

            if (Time.time - _lastEventTime < EVENT_INTERVAL) return;
            _lastEventTime = Time.time;

            float roll = Random.Range(0f, 1f);

            if (roll < 0.02f)
                TriggerFortuitousEvent(actor);
            else if (roll < 0.05f)
                TriggerInsightMoment(actor);
        }

        private static void TriggerFortuitousEvent(Actor actor)
        {
            int eventType = Random.Range(0, 4);
            string message = "";

            switch (eventType)
            {
                case 0:
                    if (CultivationMethodSystem.GetMethods(actor).Count == 0)
                    {
                        CultivationMethodSystem.GrantRandomMethod(actor);
                        message = string.Format(LM.Get("event_fortuitous_method"), actor.data.name);
                    }
                    else
                    {
                        actor.ChangeNianLi(2000f);
                        message = string.Format(LM.Get("event_fortuitous_nianli"), actor.data.name, 2000f);
                    }
                    break;

                case 1:
                    float boost = Random.Range(500f, 3000f);
                    actor.ChangeNianLi(boost);
                    message = string.Format(LM.Get("event_fortuitous_nianli"), actor.data.name, boost);
                    break;

                case 2:
                    if (RefiningSystem.GetArtifact(actor) == null)
                    {
                        var list = RefiningSystem.GenerateRandomArtifactList(1);
                        if (list.Count > 0)
                        {
                            var selected = list[0];
                            RefiningSystem.SetArtifact(actor, selected);
                            message = string.Format(LM.Get("event_fortuitous_artifact"), actor.data.name, selected.Name, LM.Get("ArtifactRarity" + selected.Rarity.ToString()));
                        }
                        else
                        {
                            actor.ChangeNianLi(1500f);
                            message = string.Format(LM.Get("event_fortuitous_stone"), actor.data.name, 1500f);
                        }
                    }
                    else
                    {
                        actor.ChangeNianLi(1500f);
                        message = string.Format(LM.Get("event_fortuitous_stone"), actor.data.name, 1500f);
                    }
                    break;

                case 3: // ★ 行善积德
                    float karma = KarmaSystem.GetKarma(actor);
                    if (karma > 0)
                    {
                        float reduction = Random.Range(3f, 10f);
                        KarmaSystem.ReduceKarma(actor, reduction);
                        KarmaSystem.AddVirtue(actor, reduction * 0.5f);
                        message = string.Format(LM.Get("event_fortuitous_virtue"), actor.data.name, reduction, reduction * 0.5f);
                    }
                    else
                    {
                        float nianLiBoost = Random.Range(500f, 2000f);
                        actor.ChangeNianLi(nianLiBoost);
                        message = string.Format(LM.Get("event_fortuitous_nianli"), actor.data.name, nianLiBoost);
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(message))
            {
                WorldTip.showNow(message, true, "top");
                WitchcraftWorldLog.AddLog("fortuitous_event", message, actor, new Color(1f, 0.84f, 0f));
                DaoBeiSystem.AddRecord(actor, LM.Get("dao_fortuitous"), message, new Color(1f, 0.84f, 0f));
            }
        }

        private static void TriggerInsightMoment(Actor actor)
        {
            int insight = actor.GetInsightLevel();
            if (insight >= 5)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (!actor.IsHeartBroken(i))
                    {
                        actor.BreakHeart(i);
                        if (!actor.hasTrait($"Heart_{i}"))
                            actor.addTrait($"Heart_{i}");
                        string msg = string.Format(LM.Get("event_insight_break"), actor.data.name, GetHeartName(i));
                        WorldTip.showNow(msg, true, "top");
                        WitchcraftWorldLog.AddLog("insight_break", msg, actor, new Color(0.5f, 0.3f, 1f));
                        return;
                    }
                }
                actor.ChangeNianLi(3000f);
                WorldTip.showNow(string.Format(LM.Get("event_insight_nianli"), actor.data.name, 3000f), true, "top");
            }
            else
            {
                actor.SetInsightLevel(insight + 1);
                string msg = string.Format(LM.Get("event_insight_up"), actor.data.name, insight + 1);
                WorldTip.showNow(msg, true, "top");
                WitchcraftWorldLog.AddLog("insight_up", msg, actor, new Color(0.5f, 0.3f, 1f));
            }
            DaoBeiSystem.AddRecord(actor, LM.Get("dao_insight"), LM.Get("insight_daobei"), new Color(0.5f, 0.3f, 1f));
        }

        private static string GetHeartName(int index)
        {
            string[] names = { "明心关", "澄心关", "定心关", "生灭关", "物我关", "真我关" };
            return (index >= 0 && index <= 5) ? names[index] : "未知心关";
        }
    }
}