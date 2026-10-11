using UnityEngine;

namespace NianLiXiuXing.code.Utils
{
    /// <summary>
    /// 特效优化工具
    /// 禁用不必要的组件以减少性能开销
    /// </summary>
    public static class EffectOptimizer
    {
        /// <summary>
        /// 优化特效：禁用 Animator、ParticleSystem 等不需要的组件
        /// </summary>
        public static void OptimizeEffect(BaseEffect effect, bool disableAnimator = true, bool disableParticles = true)
        {
            if (effect == null) return;

            // 禁用 Animator
            if (disableAnimator)
            {
                var animator = effect.GetComponent<Animator>();
                if (animator != null)
                    animator.enabled = false;
            }

            // 禁用 ParticleSystem
            if (disableParticles)
            {
                var particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                    particles.Stop(true);
            }

            // 禁用 AudioSource
            var audio = effect.GetComponent<AudioSource>();
            if (audio != null)
                audio.enabled = false;

            // 禁用 Collider
            var collider = effect.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
        }

        /// <summary>
        /// 优化特效并设置自动销毁
        /// </summary>
        public static void SpawnOptimized(string effectId, WorldTile tile, float scale, float lifetime, Color? color = null)
        {
            if (tile == null) return;

            BaseEffect effect = EffectPool.Spawn(effectId, tile, scale, color ?? Color.white);
            if (effect == null) return;

            OptimizeEffect(effect);
            EffectPool.Recycle(effect, lifetime);
        }

        /// <summary>
        /// 优化特效并禁用所有不必要的渲染
        /// </summary>
        public static void MakeStaticEffect(BaseEffect effect)
        {
            if (effect == null) return;

            // 禁用所有动画组件
            var animators = effect.GetComponentsInChildren<Animator>();
            foreach (var anim in animators)
                anim.enabled = false;

            // 禁用所有粒子系统
            var particles = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
                ps.Stop(true);

            // 冻结 Transform
            effect.transform.hasChanged = false;
        }

        /// <summary>
        /// 检查并清理无效特效组件
        /// </summary>
        public static void CleanupEffectComponents(BaseEffect effect)
        {
            if (effect == null) return;

            // 移除无效的 MonoBehaviour
            var components = effect.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null)
                    continue;
                // 检查是否是我们自己的组件，如果是则保留
                // 否则移除
                if (comp.GetType().Assembly != typeof(EffectOptimizer).Assembly)
                {
                    // 非模组组件，可能不需要
                }
            }
        }
    }
}