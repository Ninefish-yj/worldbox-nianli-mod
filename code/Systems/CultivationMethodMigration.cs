using UnityEngine;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.Core
{
    /// <summary>
    /// 旧存档数据修复与迁移（单一职责：只负责历史存档兼容）。
    /// </summary>
    public static partial class CultivationMethodSystem
    {
        private static bool _hasFixedOldSaves = false;

        /// <summary>为旧存档中达到大通境但仍无功法的角色补发功法。</summary>
        public static void FixMissingMethodsForOldSaves()
        {
            if (_hasFixedOldSaves) return;
            _hasFixedOldSaves = true;

            if (World.world?.units == null)
            {
                Debug.LogWarning("[念力修行] 世界未加载，无法补发功法");
                return;
            }

            int fixedCount = 0;
            int checkedCount = 0;
            var allUnits = World.world.units.getSimpleList();
            foreach (Actor actor in allUnits)
            {
                if (actor == null || actor.data == null || !actor.isAlive()) continue;
                if (!actor.isSapient()) continue;
                int realm = actor.GetRealmLevel();
                if (realm < 3) continue;
                checkedCount++;
                if (GetMethods(actor).Count > 0) continue;

                if (GrantRandomMethod(actor))
                    fixedCount++;
            }

            if (fixedCount > 0)
                Debug.Log($"[念力修行] 旧存档功法修复：为 {fixedCount} 个角色补发了功法（共检查 {checkedCount} 个大通境及以上角色）");
            else if (checkedCount > 0)
                Debug.Log($"[念力修行] 旧存档功法修复：检查了 {checkedCount} 个大通境及以上角色，均已有功法，无需补发");
            else
                Debug.Log("[念力修行] 旧存档功法修复：未检查到任何大通境及以上角色");
        }

        /// <summary>把旧版单功法字段迁移到新的多功法列表。</summary>
        public static void MigrateOldMethod(Actor actor)
        {
            actor.data.get("nianli.CultivationMethod", out string oldVal, "None");
            if (oldVal != "None" && System.Enum.TryParse(oldVal, out CultivationMethod oldMethod))
            {
                var list = GetMethods(actor);
                if (!list.Contains(oldMethod))
                {
                    list.Add(oldMethod);
                    SetMethods(actor, list);
                }
                actor.data.set("nianli.CultivationMethod", "");
            }
        }

        public static void MigrateAllOldSaves()
        {
            if (World.world?.units == null) return;
            foreach (Actor actor in World.world.units)
                if (actor != null && actor.data != null)
                    MigrateOldMethod(actor);
        }
    }
}
