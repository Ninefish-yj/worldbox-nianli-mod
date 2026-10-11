using System;
using UnityEngine;

namespace NianLiXiuXing.code.Utils
{
    /// <summary>
    /// 性能验证工具
    /// 用于测量优化前后的性能差异
    /// </summary>
    public static class PerformanceValidator
    {
        private static float _frameTimeBaseline = 0f;
        private static float _frameTimeAfter = 0f;
        private static int _sampleCount = 0;
        private const int SAMPLE_SIZE = 60;

        /// <summary>
        /// 开始基准测试（优化前）
        /// </summary>
        public static void StartBaseline()
        {
            _frameTimeBaseline = 0f;
            _frameTimeAfter = 0f;
            _sampleCount = 0;
            Debug.Log("[性能验证] 开始采集基准数据...");
        }

        /// <summary>
        /// 采样帧时间
        /// </summary>
        public static void SampleFrameTime()
        {
            if (_sampleCount < SAMPLE_SIZE)
            {
                _frameTimeBaseline += Time.deltaTime;
                _sampleCount++;
                if (_sampleCount >= SAMPLE_SIZE)
                {
                    _frameTimeBaseline /= SAMPLE_SIZE;
                    Debug.Log($"[性能验证] 基准采集完成，平均帧时间: {_frameTimeBaseline * 1000:F2}ms ({1f / _frameTimeBaseline:F1} FPS)");
                }
            }
        }

        /// <summary>
        /// 开始优化后测试
        /// </summary>
        public static void StartOptimizedTest()
        {
            _sampleCount = 0;
            _frameTimeAfter = 0f;
            Debug.Log("[性能验证] 开始采集优化后数据...");
        }

        /// <summary>
        /// 采样优化后帧时间
        /// </summary>
        public static void SampleOptimizedFrameTime()
        {
            if (_sampleCount < SAMPLE_SIZE)
            {
                _frameTimeAfter += Time.deltaTime;
                _sampleCount++;
                if (_sampleCount >= SAMPLE_SIZE)
                {
                    _frameTimeAfter /= SAMPLE_SIZE;
                    float improvement = (1f - _frameTimeAfter / _frameTimeBaseline) * 100f;
                    Debug.Log($"[性能验证] 优化后采集完成，平均帧时间: {_frameTimeAfter * 1000:F2}ms ({1f / _frameTimeAfter:F1} FPS)");
                    Debug.Log($"[性能验证] 🎯 性能提升: {improvement:F1}%");
                }
            }
        }

        /// <summary>
        /// 显示完整性能报告
        /// </summary>
        public static void ShowReport()
        {
            string report = $@"
╔══════════════════════════════════════════════════════════╗
║                    性能验证报告                          ║
╠══════════════════════════════════════════════════════════╣
║  基准平均帧时间: {_frameTimeBaseline * 1000:F2}ms ({1f / _frameTimeBaseline:F1} FPS)
║  优化后平均帧时间: {_frameTimeAfter * 1000:F2}ms ({1f / _frameTimeAfter:F1} FPS)
║  性能提升: {(1f - _frameTimeAfter / _frameTimeBaseline) * 100f:F1}%
╚══════════════════════════════════════════════════════════╝";
            Debug.Log(report);
        }
    }
}