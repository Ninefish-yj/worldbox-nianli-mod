using UnityEngine;

namespace NianLiXiuXing.code.Core
{
    public static class ActorExtensions_Compatibility
    {
        public static bool IsAttacked(this Actor actor)
        {
            if (actor == null) return false;
            // 如果游戏版本有 is_attacked 字段，可以使用：
            // return actor.is_attacked;
            // 否则返回 false 作为临时兼容
            return false;
        }
    }
}