using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.UI;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Systems
{
    public static class HeartDemonSystem
    {
        private const string DEMON_COOLDOWN_KEY = "nianli.demon_cooldown";

        // ★★★ 心魔劫特效：只有闪电，无光圈 ★★★
        private static void SpawnCalamityEffect(Actor actor)
        {
            if (actor?.current_tile == null) return;
            for (int i = 0; i < 3; i++)
            {
                int x = actor.current_tile.x + Random.Range(-3, 3);
                int y = actor.current_tile.y + Random.Range(-3, 3);
                WorldTile tile = World.world.GetTileSimple(x, y);
                if (tile != null && tile.Type.ground)
                {
                    BaseEffect effect = EffectsLibrary.spawnAtTile("fx_lightning_small", tile, 1.5f);
                    if (effect?.sprite_renderer != null)
                    {
                        effect.sprite_renderer.color = new Color(0.3f, 0f, 0.3f);
                        Object.Destroy(effect.gameObject, 1f);
                    }
                }
            }
        }

        public static bool CheckHeartDemon(Actor actor, int targetRealm)
        {
            if (actor == null) return false;
            if (targetRealm < 3) return false;

            actor.data.get(DEMON_COOLDOWN_KEY, out float lastDemonYear, 0f);
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;
            if (currentYear - lastDemonYear < 5f) return false;

            float karma = KarmaSystem.GetKarma(actor);
            float chance = 0.10f + karma / 100f * 0.20f;
            if (Random.Range(0f, 1f) > chance) return false;

            actor.data.set(DEMON_COOLDOWN_KEY, currentYear);
            
            int realm = actor.GetRealmLevel();
            // ★★★ 只对环界境及以上显示心魔劫提示 ★★★
            if (realm >= 4)
            {
                string message = string.Format(LM.Get("demon_trigger"), actor.data.name);
                WorldTip.showNow(message, true, "top");
            }

            float difficulty = 0.3f + targetRealm * 0.1f + karma / 100f * 0.3f;
            float successRate = 0.70f - difficulty;
            successRate = Mathf.Clamp(successRate, 0.20f, 0.85f);

            if (Random.Range(0f, 1f) < successRate)
            {
                if (realm >= 4)
                {
                    string msg = string.Format(LM.Get("demon_pass"), actor.data.name);
                    WorldTip.showNow(msg, true, "top");
                    WitchcraftWorldLog.AddLog("demon_pass", msg, actor, new Color(0.3f, 0.6f, 1f));
                }
                int insight = actor.GetInsightLevel();
                if (insight < 5) actor.SetInsightLevel(insight + 1);
                KarmaSystem.ReduceKarma(actor, 10f);
                return false;
            }
            else
            {
                SpawnCalamityEffect(actor);

                if (realm >= 4)
                {
                    string msg = string.Format(LM.Get("demon_fail"), actor.data.name);
                    WorldTip.showNow(msg, true, "top");
                    WitchcraftWorldLog.AddLog("demon_fail", msg, actor, new Color(0.8f, 0.2f, 0.2f));
                }
                actor.ChangeNianLi(-actor.GetNianLi() * 0.5f);
                for (int i = 5; i >= 3; i--)
                {
                    if (actor.IsHeartBroken(i))
                    {
                        int mask = actor.GetHeartProgress();
                        mask &= ~(1 << i);
                        actor.SetHeartProgress(mask);
                        if (actor.hasTrait($"Heart_{i}"))
                            actor.removeTrait($"Heart_{i}");
                        break;
                    }
                }
                KarmaSystem.AddKarma(actor, 5f);
                return true;
            }
        }
    }
}