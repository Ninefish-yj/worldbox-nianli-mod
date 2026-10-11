using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.UI
{
    public static class NianLiDaoBeiUI
    {
        private static ScrollWindow _window;
        private static Text _text;
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;

            _window = WindowCreator.CreateEmptyWindow(
                "NianLiDaoBeiUI",
                LocalizedTextManager.getText("world_dao_bei"),
                "ui/NianLiDaoBei"
            );
            if (_window == null) return;

            Transform background = _window.transform.Find("Background");
            if (background == null)
            {
                GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(_window.transform, false);
                background = bgObj.transform;
                RectTransform bgRect = background.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                bgRect.anchoredPosition = Vector2.zero;
                Image bgImg = background.GetComponent<Image>();
                bgImg.color = new Color(0, 0, 0, 0.85f);
            }
            RectTransform bgRectFinal = background.GetComponent<RectTransform>();
            bgRectFinal.sizeDelta = new Vector2(240, 320);

            GameObject scrollViewObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollViewObj.transform.SetParent(background, false);
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -10);
            scrollViewObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);

            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask));
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
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
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            Text text = textObj.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 9;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;

            ContentSizeFitter fitter = contentObj.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect scroll = scrollViewObj.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            _text = text;
            _initialized = true;
            Debug.Log("[念力修行] 世界道碑窗口初始化完成");
        }

        public static void ShowWindow()
        {
            if (!_initialized) Init();
            Refresh();
            ScrollWindow.showWindow("NianLiDaoBeiUI");
        }

        public static void Refresh()
        {
            if (_text == null) return;

            var records = DaoBeiSystem.GetAllRecords();
            if (records.Count == 0)
            {
                _text.text = "<color=#888888>暂无修行记录，你的道碑尚为空白。</color>";
                return;
            }

            var sb = new StringBuilder();
            foreach (var r in records)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(r.Color);
                sb.AppendLine($"<color=#{colorHex}>[{r.Time}] {r.ActorName} · {r.EventType}</color>");
                sb.AppendLine($"  {r.Description}");
                sb.AppendLine();
            }
            _text.text = sb.ToString();
        }
    }
}