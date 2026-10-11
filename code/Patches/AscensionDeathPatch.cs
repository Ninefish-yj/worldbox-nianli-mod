using HarmonyLib;
using UnityEngine;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.Core;
using System.Collections.Generic;

namespace NianLiXiuXing.code.Patches
{
    [HarmonyPatch]
    public static class AscensionDeathPatch
    {
        private static Dictionary<long, float> _resurrectionCooldown = new Dictionary<long, float>();
        private static Dictionary<long, float> _deathNianLi = new Dictionary<long, float>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Actor), "dieAndDestroy", new System.Type[] { typeof(AttackType) })]
        public static bool Prefix_dieAndDestroy(Actor __instance, AttackType pType)
        {
            if (__instance == null) return true;

            __instance.AddDeathCount();

            // ★ 九转功：死亡转生 ★
            if (CultivationMethodSystem.HasMethod(__instance, CultivationMethod.JiuZhuanGong))
            {
                double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
                float currentYear = (float)currentWorldTime;
                __instance.data.get("nianli_jiuzhuan_cooldown", out float lastYear, 0f);
                if (currentYear - lastYear >= 1f)
                {
                    __instance.restoreHealth(__instance.getMaxHealth());
                    __instance.data.health = __instance.getMaxHealth();
                    if (__instance.hasTrait(TraitIds.Trait_NoWayForward))
                        __instance.removeTrait(TraitIds.Trait_NoWayForward);
                    __instance.SetMeditating(false);
                    __instance.SetStaying(false);
                    __instance.SetStayingStartYear(-1);
                    __instance.SetStayingDuration(0);

                    __instance.data.set("nianli_jiuzhuan_cooldown", currentYear);

                    if (__instance.current_tile != null)
                    {
                        BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", __instance.current_tile, 5f);
                        if (effect != null)
                        {
                            effect.sprite_renderer.color = Color.green;
                            Object.Destroy(effect.gameObject, 3f);
                        }
                    }
                    WorldTip.showNow($"🔄 {__instance.data.name} 触发九转功，死而复生！", true, "top");
                    return false;
                }
            }

            // ★ 轮回法则：死亡时保存一半念力 ★
            if (CycleLawSystem.HasReincarnationLaw(__instance))
            {
                float currentNianLi = __instance.GetNianLi();
                float savedNianLi = currentNianLi * 0.5f;
                _deathNianLi[__instance.getID()] = savedNianLi;
                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] 轮回法则触发，{__instance.data.name} 保存了 {savedNianLi:F0} 念力");
            }

            // ★★★ 轮回法则转世 ★★★
            if (CycleLawSystem.HasReincarnationLaw(__instance))
            {
                bool willResurrect = false;
                
                if (AscensionRecordSystem.IsOnList(__instance))
                {
                    willResurrect = true;
                }
                else if (__instance.IsHeartBroken(0) && __instance.IsHeartBroken(1) && __instance.IsHeartBroken(2))
                {
                    float chance = __instance.GetResurrectionChance();
                    if (chance > 0f && Random.Range(0f, 1f) < chance)
                    {
                        willResurrect = true;
                    }
                }

                if (!willResurrect)
                {
                    if (ReincarnationHelper.TryReincarnate(__instance))
                    {
                        _deathNianLi.Remove(__instance.getID());
                        return true;
                    }
                }
            }

            // 飞升记录者：100%复活
            if (AscensionRecordSystem.IsOnList(__instance))
            {
                ClearEffectsAtActor(__instance);
                __instance.restoreHealth(__instance.getMaxHealth());
                __instance.data.health = __instance.getMaxHealth();

                if (_deathNianLi.TryGetValue(__instance.getID(), out float saved))
                {
                    __instance.ChangeNianLi(saved);
                    _deathNianLi.Remove(__instance.getID());
                }

                if (__instance.current_tile != null)
                {
                    BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", __instance.current_tile, 3f);
                    if (effect != null)
                        Object.Destroy(effect.gameObject, 2f);
                }

                if (!ConfigCache.InfiniteResurrection)
                {
                    AscensionRecordSystem.RemoveRecord(__instance);
                }

                return false;
            }

            // 心关概率复活
            if (TryHeartBasedResurrection(__instance))
            {
                if (_deathNianLi.TryGetValue(__instance.getID(), out float saved))
                {
                    __instance.ChangeNianLi(saved);
                    _deathNianLi.Remove(__instance.getID());
                }
                return false;
            }

            // ★★★ 彻底死亡，清理缓存 ★★★
            long id = __instance.getID();
            _resurrectionCooldown.Remove(id);
            _deathNianLi.Remove(id);

            return true;
        }

        private static bool TryHeartBasedResurrection(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return false;

            if (!actor.IsHeartBroken(0) || !actor.IsHeartBroken(1) || !actor.IsHeartBroken(2))
                return false;

            long id = actor.getID();
            double currentWorldTime = World.world?.map_stats?.world_time ?? 0.0;
            float currentYear = (float)currentWorldTime;

            if (_resurrectionCooldown.TryGetValue(id, out float lastResurrectionYear))
                if (currentYear - lastResurrectionYear < 1f) return false;

            float chance = actor.GetResurrectionChance();
            if (chance <= 0f) return false;

            if (Random.Range(0f, 1f) < chance)
            {
                ClearEffectsAtActor(actor);
                actor.restoreHealth(actor.getMaxHealth());
                actor.data.health = actor.getMaxHealth();

                actor.AddResurrectionCount();
                _resurrectionCooldown[id] = currentYear;

                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] {actor.data.name} 跨越了死亡 (概率{chance * 100:F0}%，剩余{actor.GetResurrectionChance() * 100:F0}%)");

                if (actor.current_tile != null)
                {
                    BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", actor.current_tile, 2f);
                    if (effect != null)
                        Object.Destroy(effect.gameObject, 2f);
                }

                return true;
            }

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {actor.data.name} 跨越死亡失败 (概率{chance * 100:F0}%)");
            return false;
        }

        private static void ClearEffectsAtActor(Actor actor)
        {
            if (actor == null || actor.current_tile == null) return;
            try
            {
                foreach (var effect in Object.FindObjectsOfType<BaseEffect>())
                {
                    if (effect == null) continue;
                    if (effect.transform != null && actor.current_tile != null)
                    {
                        Vector3 effectPos = effect.transform.position;
                        Vector3 actorPos = new Vector3(actor.current_tile.x, actor.current_tile.y, 0);
                        float dist = Vector3.Distance(effectPos, actorPos);
                        if (dist < 5f)
                            Object.Destroy(effect.gameObject);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[飞升记录] 清除特效时出错: {e.Message}");
            }
        }
    }
}