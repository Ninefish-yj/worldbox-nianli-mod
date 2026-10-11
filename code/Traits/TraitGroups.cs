using System.Collections.Generic;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Traits
{
    internal static class TraitGroups
    {
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            // ---- 心境组 ----
            var heartGroup = new ActorTraitGroupAsset();
            heartGroup.id = "Heart_Group";
            heartGroup.name = "trait_group_Heart";
            heartGroup.color = "#FFD700";
            AssetManager.trait_groups.add(heartGroup);

            // ---- 境界组 ----
            var realmGroup = new ActorTraitGroupAsset();
            realmGroup.id = "Realm_Group";
            realmGroup.name = "trait_group_Realm";
            realmGroup.color = "#00BFFF";
            AssetManager.trait_groups.add(realmGroup);

            // ---- 位格组 ----
            var weiGeGroup = new ActorTraitGroupAsset();
            weiGeGroup.id = "WeiGe_Group";
            weiGeGroup.name = "trait_group_WeiGe";
            weiGeGroup.color = "#FFD700";
            AssetManager.trait_groups.add(weiGeGroup);

            // ---- 法则组 ----
            var lawGroup = new ActorTraitGroupAsset();
            lawGroup.id = "Law_Group";
            lawGroup.name = "trait_group_Law";
            lawGroup.color = "#00CED1";
            AssetManager.trait_groups.add(lawGroup);

            // ---- 天赋组 ----
            var talentGroup = new ActorTraitGroupAsset();
            talentGroup.id = "Talent_Group";
            talentGroup.name = "trait_group_Talent";
            talentGroup.color = "#9B59B6";
            AssetManager.trait_groups.add(talentGroup);

            // ---- 前路断绝组 ----
            var noWayGroup = new ActorTraitGroupAsset();
            noWayGroup.id = "NoWay_Group";
            noWayGroup.name = "trait_group_NoWay";
            noWayGroup.color = "#8B0000";
            AssetManager.trait_groups.add(noWayGroup);

            // ★★★ 新增：小世界组 ★★★
            var smallWorldGroup = new ActorTraitGroupAsset();
            smallWorldGroup.id = "SmallWorld_Group";
            smallWorldGroup.name = LM.Get("nianli_small_world");
            smallWorldGroup.color = "#FF6B6B";
            AssetManager.trait_groups.add(smallWorldGroup);
        }
    }
}