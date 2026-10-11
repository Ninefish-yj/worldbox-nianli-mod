using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.UI
{
    public static class NianLiPerformanceUI
    {
        private static ScrollWindow _window;
        private static Text _contentText;
        private static bool _initialized;
        private static float _lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 3f;

        private static bool _isPanelVisible = false;

        // 世界业力缓存
        private static float _cachedTotalKarma = 0f;
        private static float _cachedAvgKarma = 0f;
        private static int _cachedHighKarmaCount = 0;
        private static int _cachedTotalCultivators = 0;
        private static int[] _cachedRealmCounts = new int[6];

        private const float WINDOW_WIDTH = 240f;
        private const float WINDOW_HEIGHT = 320f;

        public static void Init()
        {
            if (_initialized) return;

            _window = WindowCreator.CreateEmptyWindow(
                "NianLiPerformanceUI",
                LM.Get("nianli_world_info_title"),
                "ui/NianLiWorldInfo"
            );

            if (_window == null)
            {
                Debug.LogWarning("[念力修行] 创建世界概况窗口失败");
                return;
            }

            Transform background = _window.transform.Find("Background");
            if (background == null) return;

            RectTransform bgRect = background.GetComponent<RectTransform>();
            if (bgRect != null)
            {
                bgRect.sizeDelta = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            }

            BuildUI(background);

            var callback = _window.gameObject.AddComponent<WindowOpenCallback>();
            callback.onWindowOpen = OnWindowOpened;
            callback.onWindowClose = OnWindowClosed;

            _initialized = true;
            Debug.Log("[念力修行] 世界概况窗口初始化完成");
        }

        private static void BuildUI(Transform parent)
        {
            GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.15f);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(8, 8);
            scrollRect.offsetMax = new Vector2(-8, -8);
            scrollObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);

            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);

            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(contentObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 6);
            textRect.offsetMax = new Vector2(-6, -6);

            Text text = textObj.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 9;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;

            ContentSizeFitter fitter = contentObj.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect scroll = scrollObj.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            _contentText = text;

            // 底部按钮
            BuildBottomPanel(parent);
        }

        private static void BuildBottomPanel(Transform parent)
        {
            GameObject panelObj = new GameObject("ButtonPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            panelObj.transform.SetParent(parent, false);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, 30);
            panelRect.offsetMin = new Vector2(6, 4);
            panelRect.offsetMax = new Vector2(-6, 4);

            HorizontalLayoutGroup layout = panelObj.GetComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4;
            layout.padding = new RectOffset(4, 4, 4, 4);

            CreateButton(panelObj, "🔄 刷新", new Color(0.2f, 0.4f, 0.7f), () =>
            {
                Refresh();
                WorldTip.showNow("🔄 数据已刷新", true, "top");
            });

            CreateButton(panelObj, "🧹 清理", new Color(0.5f, 0.2f, 0.2f), () =>
            {
                NianLiGarbageCollectorSystem.ForceCleanup();
                WorldTip.showNow("🧹 缓存已清理", true, "top");
                Refresh();
            });

            CreateButton(panelObj, "💚 净化", new Color(0.2f, 0.5f, 0.2f), () =>
            {
                WorldTip.showNow("💚 选中角色按 Ctrl+Shift+K", true, "top");
            });
        }

        private static Button CreateButton(GameObject parent, string text, Color color, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject("Button", typeof(RectTransform), typeof(Button), typeof(Image));
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(70, 22);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = color;

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            Text txt = txtObj.GetComponent<Text>();
            txt.font = LocalizedTextManager.current_font;
            txt.fontSize = 9;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = text;

            return btn;
        }

        private static void OnWindowOpened() { _isPanelVisible = true; Refresh(); }
        private static void OnWindowClosed() { _isPanelVisible = false; }

        public static void ShowWindow()
        {
            if (!_initialized) Init();
            ScrollWindow.showWindow("NianLiPerformanceUI");
        }

        public static void Refresh()
        {
            if (_contentText == null)
            {
                Debug.LogWarning("[念力修行] Refresh 失败：_contentText 为空");
                return;
            }
            CollectWorldStats();
            var sb = new StringBuilder();
            BuildWorldInfoText(sb);
            _contentText.text = sb.ToString();

            // 调试日志：确认数据已更新
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] 世界概况已刷新，修士总数: {_cachedTotalCultivators}");
        }

        // ============================================================
        // ★★★ 核心修改：使用 getSimpleList() 遍历 ★★★
        // ============================================================
        private static void CollectWorldStats()
        {
            _cachedTotalKarma = 0f;
            _cachedHighKarmaCount = 0;
            _cachedTotalCultivators = 0;
            for (int i = 0; i < 6; i++) _cachedRealmCounts[i] = 0;

            if (World.world?.units == null)
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.LogWarning("[念力修行] World.world.units 为空，无法采集数据");
                return;
            }

            // ★★★ 修复：使用 getSimpleList() 获取单位列表 ★★★
            var units = World.world.units.getSimpleList();
            if (units == null || units.Count == 0)
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.LogWarning("[念力修行] 单位列表为空，无法采集数据");
                return;
            }

            int totalActors = 0;
            float totalKarma = 0f;

            // ★★★ 使用 for 循环遍历 ★★★
            for (int i = 0; i < units.Count; i++)
            {
                Actor actor = units[i];
                if (actor == null || !actor.isAlive() || !actor.isSapient()) continue;
                totalActors++;

                // 统计业力
                float karma = KarmaSystem.GetKarma(actor);
                if (karma > 0)
                {
                    totalKarma += karma;
                    if (karma > 50f) _cachedHighKarmaCount++;
                }

                // 统计修士（境界 >= 1）
                int realm = actor.GetRealmLevel();
                if (realm >= 1 && realm <= 5)
                {
                    _cachedRealmCounts[realm]++;
                    _cachedTotalCultivators++;
                }
            }

            _cachedTotalKarma = totalKarma;
            _cachedAvgKarma = totalActors > 0 ? totalKarma / totalActors : 0f;

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] 数据采集完成: {totalActors} 个智慧单位, {_cachedTotalCultivators} 个修士");
        }

        private static void BuildWorldInfoText(StringBuilder sb)
        {
            sb.AppendLine($"<color=#FFD700>══════ 世界概况 ══════</color>");

            int currentYear = World.world?.map_stats?.year ?? 0;
            sb.AppendLine($"  <color=#88CCFF>年份:</color> <color=#FFFFFF>{currentYear}</color>");

            sb.AppendLine();
            sb.AppendLine($"<color=#FF6B6B>──── 业力统计 ────</color>");
            sb.AppendLine($"  <color=#88CCFF>总业力:</color> <color=#FFFFFF>{_cachedTotalKarma:F0}</color>");
            sb.AppendLine($"  <color=#88CCFF>平均:</color> <color=#FFFFFF>{_cachedAvgKarma:F1}</color>");
            sb.AppendLine($"  <color=#88CCFF>高危(>50):</color> <color=#FF6B6B>{_cachedHighKarmaCount}</color>");

            sb.AppendLine();
            sb.AppendLine($"<color=#4CAF50>──── 修士统计 ────</color>");
            sb.AppendLine($"  <color=#88CCFF>总修士:</color> <color=#FFFFFF>{_cachedTotalCultivators}</color>");

            string[] realmNames = { "", "养念", "复观", "大通", "环界", "托举" };
            string[] realmColors = { "", "#88CCFF", "#4CAF50", "#FFA500", "#FF6B6B", "#FFD700" };
            for (int i = 1; i <= 5; i++)
            {
                if (_cachedRealmCounts[i] > 0)
                {
                    sb.AppendLine($"    <color={realmColors[i]}>●</color> {realmNames[i]}: <color=#FFFFFF>{_cachedRealmCounts[i]}</color>");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"<color=#FFA500>──── 上四境限制 ────</color>");
            sb.AppendLine($"  <color=#88CCFF>大通:</color> <color=#FFFFFF>{(ConfigCache.MaxGreatRealm > 0 ? ConfigCache.MaxGreatRealm.ToString() : "∞")}</color>");
            sb.AppendLine($"  <color=#88CCFF>环界:</color> <color=#FFFFFF>{(ConfigCache.MaxRingRealm > 0 ? ConfigCache.MaxRingRealm.ToString() : "∞")}</color>");
            sb.AppendLine($"  <color=#88CCFF>托举:</color> <color=#FFFFFF>{(ConfigCache.MaxUpholdRealm > 0 ? ConfigCache.MaxUpholdRealm.ToString() : "∞")}</color>");
            sb.AppendLine($"  <color=#88CCFF>超脱:</color> <color=#FFFFFF>{(ConfigCache.MaxTranscendent > 0 ? ConfigCache.MaxTranscendent.ToString() : "∞")}</color>");

            sb.AppendLine();
            sb.AppendLine($"<color=#FFD700>────────────────────</color>");
            string karmaStatus, karmaColor;
            if (_cachedTotalKarma < 10f)
            { karmaStatus = "✅ 世界清净"; karmaColor = "#4CAF50"; }
            else if (_cachedTotalKarma < 100f)
            { karmaStatus = "⚠️ 业力滋生"; karmaColor = "#FFA500"; }
            else if (_cachedTotalKarma < 500f)
            { karmaStatus = "⚠️ 业力弥漫"; karmaColor = "#FF6B6B"; }
            else
            { karmaStatus = "🔥 业力滔天！"; karmaColor = "#FF0000"; }
            sb.AppendLine($"  <color={karmaColor}>【{karmaStatus}】</color>");

            sb.AppendLine();
            sb.AppendLine($"  <color=#888888>更新: {DateTime.Now:HH:mm:ss}</color>");
        }

        public static void Update()
        {
            if (!_initialized || !_isPanelVisible) return;
            if (Time.time - _lastUpdateTime < UPDATE_INTERVAL) return;
            _lastUpdateTime = Time.time;
            Refresh();
        }

        private class WindowOpenCallback : MonoBehaviour
        {
            public Action onWindowOpen;
            public Action onWindowClose;
            private void OnEnable() => onWindowOpen?.Invoke();
            private void OnDisable() => onWindowClose?.Invoke();
        }
    }
}