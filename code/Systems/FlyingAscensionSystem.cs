using UnityEngine;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.Systems.Events;
using NianLiXiuXing.code.Utils;
using System.IO;
using FMODUnity;
using NeoModLoader.General;
using NianLiXiuXing.code.UI;

namespace NianLiXiuXing.code.Core
{
    public static class FlyingAscensionSystem
    {
        private static bool _isAscending = false;

        private const float MINOR_START_PERCENT = 0.10f;
        private const float MINOR_END_PERCENT = 0.30f;
        private const float MINOR_FLOAT_RANGE = 0.05f;

        private const float MAJOR_START_PERCENT = 0.20f;
        private const float MAJOR_END_PERCENT = 0.60f;
        private const float MAJOR_FLOAT_RANGE = 0.07f;

        // 颜色常量集中管理，避免散落字面量
        private static readonly Color MinorColor = new Color(0.3f, 0.6f, 1f);
        private static readonly Color MajorColor = new Color(1f, 0.84f, 0f);

        private static void PlayAscensionMusic()
        {
            SafeExecutor.Execute(() =>
            {
                string soundPath = Path.Combine(Application.streamingAssetsPath, "sounds/Sound_1.wav");
                if (File.Exists(soundPath))
                {
                    RuntimeManager.PlayOneShot(soundPath);
                }
                else if (ConfigCache.EnableDebugLog)
                {
                    Debug.LogWarning($"[念力修行] 音乐文件不存在: {soundPath}");
                }
            }, "播放飞升音乐", false);
        }

        private static void TriggerAscensionMiracle(Actor actor)
        {
            if (actor?.current_tile == null) return;
            for (int i = 0; i < 20; i++)
            {
                int x = actor.current_tile.x + Random.Range(-10, 10);
                int y = actor.current_tile.y + Random.Range(-10, 10);
                WorldTile tile = World.world.GetTileSimple(x, y);
                if (tile != null && tile.Type.ground)
                {
                    BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", tile, 1.5f);
                    if (effect?.sprite_renderer != null)
                    {
                        effect.sprite_renderer.color = MajorColor;
                        Object.Destroy(effect.gameObject, 2f);
                    }
                }
            }
        }

        public static bool TriggerAscension(Actor actor)
        {
            if (actor == null) return false;
            if (_isAscending) return false;

            AscensionType type = actor.GetAscensionType();

            if (type == AscensionType.Minor && actor.CanStartMinorAscension())
            {
                StartMinorAscension(actor);
                return true;
            }
            else if (type == AscensionType.Major && actor.CanStartMajorAscension())
            {
                float lifespan = actor.stats["lifespan"];
                float age = actor.getAge();

                actor.stats["lifespan"] = lifespan - 500f;
                actor.ChangeNianLi(-10000000f);

                if (actor.getAge() >= actor.stats["lifespan"])
                {
                    actor.data.health = -1;
                    actor.dieAndDestroy(AttackType.Other);
                    WorldTip.showNow($"💀 {actor.data.name} 寿元耗尽，未能引动大阳劫！", true, "top");
                    WorldMessageSystem.Broadcast($"💀 {actor.data.name} 寿元耗尽，未能引动大阳劫！");
                    return false;
                }

                StartMajorAscension(actor);
                return true;
            }

            return false;
        }

        private static void StartMinorAscension(Actor actor)
        {
            _isAscending = true;
            actor.SetAscensionStep(1);
            actor.SetAscensionReady(true);
            actor.data.set(DataKeys.LAST_ASCENSION_TYPE, "Minor");

            string message = $"☀️ {actor.data.name} 引动小阳劫！天地共鸣，五重阳劫降临！";
            WorldMessageSystem.Broadcast(message);

            SpawnEffect(actor, "fx_lightning_small", 6f, MinorColor, 2f);
        }

        private static void StartMajorAscension(Actor actor)
        {
            _isAscending = true;
            actor.SetAscensionStep(1);
            actor.SetAscensionReady(true);
            actor.data.set(DataKeys.LAST_ASCENSION_TYPE, "Major");

            string message = $"☀️☀️☀️ {actor.data.name} 引动大阳劫！诸天震颤，九重天劫降临！";
            WorldMessageSystem.Broadcast(message);
            WitchcraftWorldLog.AddLog("major_ascension_trigger", message, actor, MajorColor);

            SpawnEffect(actor, "fx_lightning_small", 10f, MajorColor, 2f);
            PlayAscensionMusic();
        }

        public static void ProcessAscension(Actor actor, float elapsed)
        {
            if (actor == null) return;
            if (!actor.IsAscensionReady()) return;

            if (!actor.isAlive())
            {
                actor.SetAscensionReady(false);
                actor.SetAscensionStep(0);
                _isAscending = false;
                return;
            }

            actor.data.get(DataKeys.LAST_ASCENSION_TYPE, out string typeStr, "Minor");
            bool isMajor = typeStr == "Major";

            int totalSteps = isMajor ? Constants.MAJOR_ASCENSION_TOTAL_STEPS : Constants.MINOR_ASCENSION_TOTAL_STEPS;
            int step = actor.GetAscensionStep();

            if (step > totalSteps)
            {
                CompleteAscension(actor, isMajor);
                return;
            }

            actor.data.get(DataKeys.LAST_ASCENSION_TIMER, out float timer, 0f);
            timer += elapsed;

            float interval = isMajor ? 5f : 4f;
            if (timer >= interval)
            {
                timer = 0f;
                ProcessAscensionStep(actor, step, isMajor, totalSteps);
                if (actor.IsAscensionReady() && actor.isAlive())
                {
                    actor.AddAscensionStep();
                    if (step < totalSteps)
                    {
                        string msg = isMajor
                            ? $"⚡ {actor.data.name} 第{step + 1}重天劫！（{step + 1}/{totalSteps}）"
                            : $"⚡ {actor.data.name} 第{step + 1}阳劫！（{step + 1}/{totalSteps}）";
                        WorldMessageSystem.Broadcast(msg);
                    }
                }
                else
                {
                    _isAscending = false;
                }
            }

            actor.data.set(DataKeys.LAST_ASCENSION_TIMER, timer);

            if (actor.isAlive() && actor.current_tile != null && Random.Range(0f, 1f) < 0.05f)
            {
                SpawnEffect(actor, "fx_lightning_small", 4f, isMajor ? MajorColor : MinorColor, 2f);
            }
        }

        private static void ProcessAscensionStep(Actor actor, int step, bool isMajor, int totalSteps)
        {
            if (actor == null) return;
            if (!actor.isAlive()) return;

            float startPercent = isMajor ? MAJOR_START_PERCENT : MINOR_START_PERCENT;
            float endPercent = isMajor ? MAJOR_END_PERCENT : MINOR_END_PERCENT;
            float floatRange = isMajor ? MAJOR_FLOAT_RANGE : MINOR_FLOAT_RANGE;

            float t = (step - 1) / (float)(totalSteps - 1);
            float basePercent = Mathf.Lerp(startPercent, endPercent, t);
            float actualPercent = basePercent * (1f + Random.Range(-floatRange, floatRange));
            actualPercent = Mathf.Clamp(actualPercent, 0.01f, 0.95f);

            int currentHealth = actor.getHealth();
            int damage = Mathf.Max(1, Mathf.RoundToInt(currentHealth * actualPercent));

            float successRate = isMajor ? Constants.MAJOR_ASCENSION_SUCCESS_RATE : Constants.MINOR_ASCENSION_SUCCESS_RATE;
            float roll = Random.Range(0f, 1f);

            if (roll < successRate)
            {
                actor.changeHealth(-damage);
                WorldTip.showNow($"✅ 第{step}劫通过! (-{damage}HP, {actualPercent * 100:F1}%)", true, "top");
                SpawnEffect(actor, "fx_explosion_wave", 3f, isMajor ? MajorColor : MinorColor, 1.5f);
                NianLiInfoPanel.MarkDirty();
                return;
            }

            int doubledDamage = Mathf.Min(damage * 2, currentHealth);
            actor.changeHealth(-doubledDamage);

            if (!actor.isAlive())
            {
                double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
                float currentYear = (float)currentWorldTime;
                actor.data.set(DataKeys.LAST_ASCENSION_TIMER, currentYear);

                string title = isMajor ? LM.Get("nianli_major_ascension") : LM.Get("nianli_minor_ascension");
                WorldTip.showNow($"💀 {actor.data.name} {title}失败，身死道消！", true, "top");
                WorldMessageSystem.Broadcast($"💀 {actor.data.name} {title}失败，身死道消！");

                actor.SetAscensionReady(false);
                actor.SetAscensionStep(0);
                _isAscending = false;
                ClearEffectsAtActor(actor);
                NianLiInfoPanel.MarkDirty();
                return;
            }

            string title2 = isMajor ? LM.Get("nianli_major_ascension") : LM.Get("nianli_minor_ascension");
            WorldTip.showNow($"⛔ {actor.data.name} {title2}失败，前路断绝！", true, "top");

            double currentWorldTime2 = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear2 = (float)currentWorldTime2;
            actor.data.set(DataKeys.LAST_ASCENSION_TIMER, currentYear2);

            if (!actor.hasTrait(TraitIds.Trait_NoWayForward))
                actor.addTrait(TraitIds.Trait_NoWayForward);
            actor.SetNianLi(0);
            actor.SetShieldHP(0);
            actor.SetShieldMax(0);
            actor.SetMeditating(false);
            actor.SetStaying(false);
            actor.SetStayingStartYear(-1);
            actor.SetStayingDuration(0);
            actor.SetAscensionReady(false);
            actor.SetAscensionStep(0);
            _isAscending = false;
            ClearEffectsAtActor(actor);
            NianLiInfoPanel.MarkDirty();

            if (isMajor)
            {
                int newHealth = Mathf.Max(1, Mathf.RoundToInt(actor.getMaxHealth() * 0.1f));
                actor.setHealth(newHealth);
                actor.data.health = newHealth;

                if (actor.hasTrait(TraitIds.MinorTranscendence))
                    actor.removeTrait(TraitIds.MinorTranscendence);

                WorldMessageSystem.Broadcast($"💀 {actor.data.name} 大阳劫失败，位格破碎，仅存一丝生机！");
                WorldTip.showNow($"💀 {actor.data.name} 大阳劫失败，位格破碎，仅存一丝生机！", true, "top");
                WorldMessageSystem.BroadcastBreakthroughFail(actor, 5, false);
            }
            else
            {
                actor.setHealth(Mathf.Max(1, actor.getMaxHealth() / 2));
                actor.data.health = Mathf.Max(1, actor.getMaxHealth() / 2);
            }
        }

        /// <summary>消耗法力折算为额外念力（大/小阳劫成功时共用）。</summary>
        private static void ConsumeManaForNianLi(Actor actor, float ratio)
        {
            if (!ConfigCache.EnableManaConvert) return;
            int mana = actor.getMana();
            if (mana <= 0) return;

            float extraNianLi = mana / 100f * ratio;
            actor.ChangeNianLi(extraNianLi);
            actor.spendMana(mana);
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {actor.data.name} 法力 {mana} → 额外念力 {extraNianLi:F1}");
        }

        private static void CompleteAscension(Actor actor, bool isMajor)
        {
            if (actor == null) return;
            if (!actor.isAlive()) return;

            actor.SetAscensionReady(false);
            actor.SetAscensionStep(0);
            _isAscending = false;

            if (isMajor)
            {
                int maxTrans = ConfigCache.MaxTranscendent;
                if (maxTrans > 0)
                {
                    int currentTrans = 0;
                    if (World.world?.units != null)
                    {
                        foreach (Actor a in World.world.units)
                        {
                            if (a != null && a.isAlive() && a.hasTrait(TraitIds.WeiGe99))
                                currentTrans++;
                        }
                    }
                    if (currentTrans >= maxTrans)
                    {
                        actor.stats["lifespan"] += 500f;
                        actor.ChangeNianLi(10000000f);
                        actor.SetAscensionReady(false);
                        actor.SetAscensionStep(0);
                        _isAscending = false;
                        WorldTip.showNow($"⚠️ 超脱位格已达上限（{maxTrans}），无法继续飞升", true, "top");
                        ClearEffectsAtActor(actor);
                        return;
                    }
                }

                actor.restoreHealth(actor.getMaxHealth());
                actor.ChangeNianLi(100000f);
                actor.stats["NianLi"] += 100000f;

                ConsumeManaForNianLi(actor, 2f);

                if (!actor.hasTrait(TraitIds.WeiGe99))
                    actor.addTrait(TraitIds.WeiGe99);

                actor.data.set(DataKeys.LAST_ASCENSION_TIMER, 0f);

                WorldMessageSystem.BroadcastAscension(actor, true);
                PlayAscensionMusic();

                SpawnEffect(actor, "fx_spawn", 15f, MajorColor, 5f);

                // ★★★ 大阳劫：保留光圈 ★★★
                for (int i = 0; i < 9; i++)
                {
                    int index = i;
                    float scale = 2f + i * 2f;
                    SafeExecutor.Execute(() =>
                    {
                        if (actor?.current_tile != null)
                        {
                            BaseEffect ring = EffectsLibrary.spawnAtTile("fx_explosion_wave", actor.current_tile, scale);
                            if (ring?.sprite_renderer != null)
                            {
                                ring.sprite_renderer.color = MajorColor;
                                Object.Destroy(ring.gameObject, 2f);
                            }
                        }
                    }, $"生成大阳劫环状特效 {index}", false);
                }

                WorldTip.showNow("🌟✨🌟 大阳劫成！超脱天地！ 🌟✨🌟", true, "top");
                TriggerAscensionMiracle(actor);
                XiuXingEventCenter.TriggerAscension(actor, AscensionType.Major, true);
                NianLiInfoPanel.MarkDirty();
            }
            else
            {
                // ★★★ 小阳劫：只生成粒子，不生成光圈 ★★★
                actor.restoreHealth(actor.getMaxHealth());
                actor.ChangeNianLi(10000f);
                actor.SetMinorAscensionComplete(true);

                ConsumeManaForNianLi(actor, 1.2f);

                if (!actor.hasTrait(TraitIds.MinorTranscendence))
                    actor.addTrait(TraitIds.MinorTranscendence);

                WorldMessageSystem.BroadcastMinorAscension(actor);
                PlayAscensionMusic();

                SpawnEffect(actor, "fx_sparkle", 3f, MinorColor, 2f);

                WorldTip.showNow("🌊 小阳劫成！准超脱之身！", true, "top");
                TriggerAscensionMiracle(actor);
                XiuXingEventCenter.TriggerAscension(actor, AscensionType.Minor, true);
                NianLiInfoPanel.MarkDirty();
            }

            ClearEffectsAtActor(actor);
        }

        public static bool IsAscending(Actor actor)
        {
            if (actor == null) return false;
            return actor.IsAscensionReady();
        }

        /// <summary>统一的特效生成入口（替代原 SpawnLightningEffect / SpawnWaveEffect）。</summary>
        private static void SpawnEffect(Actor actor, string effectId, float scale, Color color, float duration)
        {
            if (actor?.current_tile == null) return;
            SafeExecutor.Execute(() =>
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile(effectId, actor.current_tile, scale);
                if (effect?.sprite_renderer != null)
                {
                    effect.sprite_renderer.color = color;
                    Object.Destroy(effect.gameObject, duration);
                }
            }, $"生成特效 {effectId}", false);
        }

        private static void ClearEffectsAtActor(Actor actor)
        {
            if (actor == null || actor.current_tile == null) return;
            SafeExecutor.Execute(() =>
            {
                foreach (var effect in Object.FindObjectsOfType<BaseEffect>())
                {
                    if (effect == null) continue;
                    if (effect.transform != null && actor.current_tile != null)
                    {
                        Vector3 effectPos = effect.transform.position;
                        Vector3 actorPos = new Vector3(actor.current_tile.x, actor.current_tile.y, 0);
                        float dist = Vector3.Distance(effectPos, actorPos);
                        if (dist < 3f)
                        {
                            Object.Destroy(effect.gameObject);
                        }
                    }
                }
            }, "清除飞升特效", false);
        }
    }
}
