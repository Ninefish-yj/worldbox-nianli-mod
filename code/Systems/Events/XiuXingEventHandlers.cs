using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.UI;
using UnityEngine;

namespace NianLiXiuXing.code.Systems.Events
{
    public static class XiuXingEventHandlers
    {
        private static bool _subscribed = false;

        public static void SubscribeAll()
        {
            if (_subscribed) return;
            _subscribed = true;

            XiuXingEventCenter.OnBreakthrough += e =>
            {
                BreakthroughFeedback.PlayBreakthroughEffect(e.Actor, e.NewRealm);
                WorldMessageSystem.BroadcastBreakthrough(e.Actor, e.NewRealm);
            };

            XiuXingEventCenter.OnHeartBreak += e =>
            {
                BreakthroughFeedback.PlayHeartBreakEffect(e.Actor, e.HeartIndex);
                WorldMessageSystem.BroadcastHeartBreak(e.Actor, e.HeartIndex);
            };

            XiuXingEventCenter.OnAscension += e =>
            {
                if (e.IsSuccess)
                {
                    WorldMessageSystem.BroadcastAscension(e.Actor, true);
                }
            };

            XiuXingEventCenter.OnLawGain += e =>
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] {e.Actor.data.name} 领悟了 {e.LawName} 法则");
            };

            XiuXingEventCenter.OnDeath += e =>
            {
                if (e.Killer != null && e.Actor.GetRealmLevel() >= 4)
                {
                    WorldMessageSystem.BroadcastKill(e.Actor, e.Killer);
                }
            };
        }

        public static void UnsubscribeAll()
        {
            _subscribed = false;
            XiuXingEventCenter.OnBreakthrough = null;
            XiuXingEventCenter.OnHeartBreak = null;
            XiuXingEventCenter.OnAscension = null;
            XiuXingEventCenter.OnLawGain = null;
            XiuXingEventCenter.OnDeath = null;
        }
    }
}