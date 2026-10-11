using UnityEngine;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.UI
{
    public static class BreakthroughFeedback
    {
        private static readonly Color[] RealmColors = new Color[]
        {
            Color.white,
            new Color(0.5f, 0.8f, 1f),
            new Color(0.3f, 0.9f, 0.6f),
            new Color(1f, 0.7f, 0.2f),
            new Color(1f, 0.4f, 0.6f),
            new Color(1f, 0.84f, 0f)
        };

        private static readonly string[] RealmEmojis = new string[]
        {
            "🌱", "🌿", "🌳", "🌌", "🌀", "☀️"
        };

        public static void PlayBreakthroughEffect(Actor actor, int newRealm)
        {
            if (actor == null || actor.current_tile == null) return;

            Color color = RealmColors[Mathf.Clamp(newRealm, 0, 5)];

            SafeExecutor.Execute(() =>
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_sparkle", actor.current_tile, 0.6f);
                if (effect?.sprite_renderer != null)
                {
                    effect.sprite_renderer.color = color;
                    Object.Destroy(effect.gameObject, 1.2f);
                }
            }, "生成突破特效", false);
        }

        // ★★★ 心关突破：只有真我关（index==5）保留光圈 ★★★
        public static void PlayHeartBreakEffect(Actor actor, int heartIndex)
        {
            if (actor == null || actor.current_tile == null) return;

            string[] heartColors = {
                "#4FC3F7", "#81C784", "#FFB74D", "#F06292", "#CE93D8", "#FFD700"
            };

            if (heartIndex == 5)
            {
                SafeExecutor.Execute(() =>
                {
                    float scale = 3f;
                    BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", actor.current_tile, scale);
                    if (effect?.sprite_renderer != null)
                    {
                        ColorUtility.TryParseHtmlString(heartColors[heartIndex], out Color c);
                        effect.sprite_renderer.color = c;
                        Object.Destroy(effect.gameObject, 2.5f);
                    }
                }, $"生成真我关特效", false);
            }
            else
            {
                SafeExecutor.Execute(() =>
                {
                    float scale = 1.2f;
                    BaseEffect effect = EffectsLibrary.spawnAtTile("fx_sparkle", actor.current_tile, scale);
                    if (effect?.sprite_renderer != null)
                    {
                        ColorUtility.TryParseHtmlString(heartColors[heartIndex], out Color c);
                        effect.sprite_renderer.color = c;
                        Object.Destroy(effect.gameObject, 1.2f);
                    }
                }, $"生成心关突破特效 {heartIndex}", false);
            }
        }

        public static Color GetRealmColor(int realm)
        {
            return RealmColors[Mathf.Clamp(realm, 0, 5)];
        }

        public static string GetRealmEmoji(int realm)
        {
            return RealmEmojis[Mathf.Clamp(realm, 0, 5)];
        }
    }
}