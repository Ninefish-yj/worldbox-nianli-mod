using System;

namespace NianLiXiuXing.code.Core
{
    [Flags]
    public enum TraitMask : long
    {
        None = 0,
        Heart_0 = 1L << 0,
        Heart_1 = 1L << 1,
        Heart_2 = 1L << 2,
        Heart_3 = 1L << 3,
        Heart_4 = 1L << 4,
        Heart_5 = 1L << 5,
        Realm_1 = 1L << 6,
        Realm_2 = 1L << 7,
        Realm_3 = 1L << 8,
        Realm_4 = 1L << 9,
        Realm_5 = 1L << 10,
        Law_Reincarnation = 1L << 11,
        Law_YinYang = 1L << 12,
        Law_TimeSpace = 1L << 13,
        Law_CauseEffect = 1L << 14,
        SmallWorld_1 = 1L << 15,
        SmallWorld_2 = 1L << 16,
        SmallWorld_3 = 1L << 17,
        SmallWorld_4 = 1L << 18,
        NoWayForward = 1L << 19,
        WeiGe99 = 1L << 20,
        MinorTranscendence = 1L << 21,
        Reincarnated = 1L << 22,
        BrokenHeart = 1L << 23,
        Talent_YuanJue = 1L << 24,
        Talent_KongNa = 1L << 25,
        Talent_DunCheng = 1L << 26,
        Talent_BenYuan = 1L << 27,
        Talent_DongYin = 1L << 28,
        Talent_GuHeng = 1L << 29,
        Talent_ShiZhi = 1L << 30,
        Talent_SuQing = 1L << 31,
        Talent_JingGuan = 1L << 32,
        Talent_ChongQuan = 1L << 33,
        Talent_YuNian = 1L << 34,
        Talent_JingYi = 1L << 35,
        AllHearts = Heart_0 | Heart_1 | Heart_2 | Heart_3 | Heart_4 | Heart_5,
        AllLaws = Law_Reincarnation | Law_YinYang | Law_TimeSpace | Law_CauseEffect,
        AnyTalent = Talent_YuanJue | Talent_KongNa | Talent_DunCheng | Talent_BenYuan |
                    Talent_DongYin | Talent_GuHeng | Talent_ShiZhi | Talent_SuQing |
                    Talent_JingGuan | Talent_ChongQuan | Talent_YuNian | Talent_JingYi,
    }
}