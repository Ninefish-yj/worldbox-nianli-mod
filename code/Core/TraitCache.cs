using System;
using System.Collections.Generic;
using UnityEngine;

namespace NianLiXiuXing.code.Core
{
    public static class TraitCache
    {
        private const string MASK_KEY = "nianli.trait_mask";

        // 特质ID到位掩码的映射
        private static readonly Dictionary<string, TraitMask> _traitToMask = new Dictionary<string, TraitMask>
        {
            // 心关
            { TraitIds.Heart_0, TraitMask.Heart_0 },
            { TraitIds.Heart_1, TraitMask.Heart_1 },
            { TraitIds.Heart_2, TraitMask.Heart_2 },
            { TraitIds.Heart_3, TraitMask.Heart_3 },
            { TraitIds.Heart_4, TraitMask.Heart_4 },
            { TraitIds.Heart_5, TraitMask.Heart_5 },
            // 境界
            { TraitIds.Realm_1, TraitMask.Realm_1 },
            { TraitIds.Realm_2, TraitMask.Realm_2 },
            { TraitIds.Realm_3, TraitMask.Realm_3 },
            { TraitIds.Realm_4, TraitMask.Realm_4 },
            { TraitIds.Realm_5, TraitMask.Realm_5 },
            // 法则
            { TraitIds.Law_Reincarnation, TraitMask.Law_Reincarnation },
            { TraitIds.Law_YinYang, TraitMask.Law_YinYang },
            { TraitIds.Law_TimeSpace, TraitMask.Law_TimeSpace },
            { TraitIds.Law_CauseEffect, TraitMask.Law_CauseEffect },
            // 小世界
            { TraitIds.SmallWorld_1, TraitMask.SmallWorld_1 },
            { TraitIds.SmallWorld_2, TraitMask.SmallWorld_2 },
            { TraitIds.SmallWorld_3, TraitMask.SmallWorld_3 },
            { TraitIds.SmallWorld_4, TraitMask.SmallWorld_4 },
            // 特殊
            { TraitIds.Trait_NoWayForward, TraitMask.NoWayForward },
            { TraitIds.WeiGe99, TraitMask.WeiGe99 },
            { TraitIds.MinorTranscendence, TraitMask.MinorTranscendence },
            { TraitIds.Trait_Reincarnated, TraitMask.Reincarnated },
            { TraitIds.Trait_BrokenHeart, TraitMask.BrokenHeart },
            // 天赋
            { TraitIds.Talent_YuanJue, TraitMask.Talent_YuanJue },
            { TraitIds.Talent_KongNa, TraitMask.Talent_KongNa },
            { TraitIds.Talent_DunCheng, TraitMask.Talent_DunCheng },
            { TraitIds.Talent_BenYuan, TraitMask.Talent_BenYuan },
            { TraitIds.Talent_DongYin, TraitMask.Talent_DongYin },
            { TraitIds.Talent_GuHeng, TraitMask.Talent_GuHeng },
            { TraitIds.Talent_ShiZhi, TraitMask.Talent_ShiZhi },
            { TraitIds.Talent_SuQing, TraitMask.Talent_SuQing },
            { TraitIds.Talent_JingGuan, TraitMask.Talent_JingGuan },
            { TraitIds.Talent_ChongQuan, TraitMask.Talent_ChongQuan },
            { TraitIds.Talent_YuNian, TraitMask.Talent_YuNian },
            { TraitIds.Talent_JingYi, TraitMask.Talent_JingYi },
        };

        // ============================================================
        // ★★★ 核心修改：GetMask 每次重建，保证正确性 ★★★
        // 原因：TraitCachePatch 补丁因游戏版本差异无法稳定工作，
        // 因此放弃缓存机制，每次直接重建掩码。
        // 性能影响：每个角色每次调用 HasTrait 都会遍历其特质列表
        // （通常 < 30 个），开销完全可接受。
        // ============================================================

        public static TraitMask GetMask(Actor actor)
        {
            if (actor == null || actor.data == null) return TraitMask.None;
            // ★ 不再读取 data 中的缓存，直接重建
            return RebuildMask(actor);
        }

        /// <summary>
        /// 手动重建掩码（用于外部调用，如加载存档后强制刷新）
        /// </summary>
        public static void Rebuild(Actor actor)
        {
            if (actor == null) return;
            TraitMask mask = RebuildMask(actor);
            // 可选存储到 data，但 GetMask 不会读取它
            actor.data.set(MASK_KEY, (long)mask);
        }

        private static TraitMask RebuildMask(Actor actor)
        {
            TraitMask mask = TraitMask.None;
            // actor.getTraits() 返回 IEnumerable<ActorTrait>
            foreach (var trait in actor.getTraits())
            {
                if (_traitToMask.TryGetValue(trait.id, out TraitMask bit))
                    mask |= bit;
            }
            return mask;
        }

        // ============================================================
        // ★ 对外查询接口 ★
        // ============================================================

        public static bool HasTrait(Actor actor, TraitMask flag)
        {
            if (actor == null) return false;
            return (GetMask(actor) & flag) != 0;
        }

        public static bool HasAll(Actor actor, TraitMask flags)
        {
            if (actor == null) return false;
            return (GetMask(actor) & flags) == flags;
        }

        public static bool HasAny(Actor actor, TraitMask flags)
        {
            if (actor == null) return false;
            return (GetMask(actor) & flags) != 0;
        }

        // ============================================================
        // ★ 特质变化回调（保留，但不再被补丁调用，可用于手动更新） ★
        // ============================================================

        public static void OnTraitChanged(Actor actor, string traitId, bool added)
        {
            if (actor == null) return;
            // 由于 GetMask 每次都重建，这里不需要做任何事
            // 但保留此方法以防将来需要手动触发重建
        }
    }
}