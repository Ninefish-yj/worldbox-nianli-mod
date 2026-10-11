using NeoModLoader.api;

namespace NianLiXiuXing.code.Core
{
    public static class Stats
    {
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            var nianLi = new BaseStatAsset();
            nianLi.id = "NianLi";
            nianLi.normalize = false;
            nianLi.used_only_for_civs = false;
            AssetManager.base_stats_library.add(nianLi);

            var attack = new BaseStatAsset();
            attack.id = "NianLiAttack";
            attack.normalize = false;
            attack.used_only_for_civs = false;
            AssetManager.base_stats_library.add(attack);

            var defense = new BaseStatAsset();
            defense.id = "NianLiDefense";
            defense.normalize = false;
            defense.used_only_for_civs = false;
            AssetManager.base_stats_library.add(defense);

            var critDamage = new BaseStatAsset();
            critDamage.id = "CriticalDamage";
            critDamage.normalize = false;
            critDamage.used_only_for_civs = false;
            AssetManager.base_stats_library.add(critDamage);

            var heartBonus = new BaseStatAsset();
            heartBonus.id = "HeartBonus";
            heartBonus.normalize = false;
            heartBonus.used_only_for_civs = false;
            AssetManager.base_stats_library.add(heartBonus);

            var deathCount = new BaseStatAsset();
            deathCount.id = "DeathCount";
            deathCount.normalize = false;
            deathCount.used_only_for_civs = false;
            AssetManager.base_stats_library.add(deathCount);
        }
    }
}