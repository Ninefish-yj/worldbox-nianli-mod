using System;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Systems.Events
{
    public static class XiuXingEventCenter
    {
        public class BreakthroughEventArgs : EventArgs { public Actor Actor; public int OldRealm; public int NewRealm; public bool IsSuccess; }
        public class HeartBreakEventArgs : EventArgs { public Actor Actor; public int HeartIndex; }
        public class AscensionEventArgs : EventArgs { public Actor Actor; public AscensionType Type; public bool IsSuccess; }
        public class LawGainEventArgs : EventArgs { public Actor Actor; public string LawName; public int LawIndex; }
        public class DeathEventArgs : EventArgs { public Actor Actor; public Actor Killer; }

        public static Action<BreakthroughEventArgs> OnBreakthrough;
        public static Action<HeartBreakEventArgs> OnHeartBreak;
        public static Action<AscensionEventArgs> OnAscension;
        public static Action<LawGainEventArgs> OnLawGain;
        public static Action<DeathEventArgs> OnDeath;

        public static void TriggerBreakthrough(Actor actor, int oldRealm, int newRealm, bool success)
        {
            OnBreakthrough?.Invoke(new BreakthroughEventArgs
            {
                Actor = actor,
                OldRealm = oldRealm,
                NewRealm = newRealm,
                IsSuccess = success
            });
        }

        public static void TriggerHeartBreak(Actor actor, int heartIndex)
        {
            OnHeartBreak?.Invoke(new HeartBreakEventArgs
            {
                Actor = actor,
                HeartIndex = heartIndex
            });
        }

        public static void TriggerAscension(Actor actor, AscensionType type, bool success)
        {
            OnAscension?.Invoke(new AscensionEventArgs
            {
                Actor = actor,
                Type = type,
                IsSuccess = success
            });
        }

        public static void TriggerLawGain(Actor actor, string lawName, int lawIndex)
        {
            OnLawGain?.Invoke(new LawGainEventArgs
            {
                Actor = actor,
                LawName = lawName,
                LawIndex = lawIndex
            });
        }

        public static void TriggerDeath(Actor actor, Actor killer)
        {
            OnDeath?.Invoke(new DeathEventArgs
            {
                Actor = actor,
                Killer = killer
            });
        }
    }
}