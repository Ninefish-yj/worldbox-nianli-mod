using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using System.Text;

namespace NianLiXiuXing.code.UI
{
    public static class NianLiCultivationMethodUI
    {
        private static ScrollWindow _window;
        private static bool _initialized;
        private static Transform _listContent;
        private static Text _detailText;
        private static Button _grantButton;
        private static CultivationMethod _selectedMethod = CultivationMethod.None;

        public static void Init()
        {
            if (_initialized) return;

            // ★★★ 直接使用中文标题，避免本地化缺失 ★★★
            _window = WindowCreator.CreateEmptyWindow(
                "NianLiCultivationMethodUI",
                "功法百科",
                "ui/NianLiMethod"
            );
            if (_window == null)
            {
                Debug.LogError("[念力修行] 创建功法百科窗口失败！");
                return;
            }

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
                background.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);
            }
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 400);

            BuildUI(background);
            _initialized = true;
            Debug.Log("[念力修行] 功法百科窗口初始化完成");
        }

        private static void BuildUI(Transform background)
        {
            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(background, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(8, 8);
            panelRect.offsetMax = new Vector2(-8, -8);

            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4;
            layout.padding = new RectOffset(5, 5, 5, 5);

            // ---- 标题（硬编码） ----
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 28);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.font = LocalizedTextManager.current_font;
            titleText.fontSize = 14;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.84f, 0f);
            titleText.supportRichText = true;
            titleText.text = "📖 功法百科";

            // ---- 功法列表（ScrollView） ----
            GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollObj.transform.SetParent(panel.transform, false);
            RectTransform scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.sizeDelta = new Vector2(0, 160);
            scrollRectTransform.anchorMin = new Vector2(0, 0.5f);
            scrollRectTransform.anchorMax = new Vector2(1, 0.5f);
            scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
            scrollObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);

            // Viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);

            // Content
            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup listLayout = contentObj.GetComponent<VerticalLayoutGroup>();
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = false;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            listLayout.spacing = 2;
            listLayout.padding = new RectOffset(3, 3, 3, 3);

            ContentSizeFitter fitter = contentObj.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 25f;

            // 滚动条
            GameObject scrollbarObj = new GameObject("VerticalScrollbar", typeof(RectTransform), typeof(Scrollbar));
            scrollbarObj.transform.SetParent(scrollObj.transform, false);
            RectTransform scrollbarRect = scrollbarObj.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1, 0);
            scrollbarRect.anchorMax = new Vector2(1, 1);
            scrollbarRect.pivot = new Vector2(1, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(14, 0);
            Scrollbar scrollbar = scrollbarObj.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.TopToBottom;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            _listContent = contentObj.transform;

            // ---- 详情 ----
            GameObject detailObj = new GameObject("Detail", typeof(RectTransform), typeof(Text));
            detailObj.transform.SetParent(panel.transform, false);
            RectTransform detailRect = detailObj.GetComponent<RectTransform>();
            detailRect.sizeDelta = new Vector2(0, 100);
            _detailText = detailObj.GetComponent<Text>();
            _detailText.font = LocalizedTextManager.current_font;
            _detailText.fontSize = 10;
            _detailText.alignment = TextAnchor.UpperLeft;
            _detailText.supportRichText = true;
            _detailText.text = "请从上方选择功法查看详情";

            // ---- 补发按钮 ----
            GameObject btnObj = new GameObject("GrantButton", typeof(RectTransform), typeof(Button), typeof(Image));
            btnObj.transform.SetParent(panel.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0, 32);
            Button btn = btnObj.GetComponent<Button>();
            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 0.2f);

            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            Text btnText = btnTextObj.GetComponent<Text>();
            btnText.font = LocalizedTextManager.current_font;
            btnText.fontSize = 13;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.text = "补发功法";

            _grantButton = btn;
            btn.onClick.AddListener(GrantMethodsToAll);

            PopulateMethodList();
        }

        private static void PopulateMethodList()
        {
            if (_listContent == null) return;

            // 清除旧条目
            foreach (Transform child in _listContent)
                Object.Destroy(child.gameObject);

            // 添加新条目
            foreach (var methodData in CultivationMethodSystem.AllMethods)
            {
                if (methodData.Method == CultivationMethod.None) continue;

                GameObject entry = new GameObject($"Entry_{methodData.Method}", typeof(RectTransform), typeof(Image));
                entry.transform.SetParent(_listContent, false);
                RectTransform entryRect = entry.GetComponent<RectTransform>();
                entryRect.sizeDelta = new Vector2(0, 24);

                Image entryImg = entry.GetComponent<Image>();
                entryImg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

                Button entryBtn = entry.AddComponent<Button>();
                entryBtn.targetGraphic = entryImg;
                CultivationMethod method = methodData.Method;
                entryBtn.onClick.AddListener(() =>
                {
                    _selectedMethod = method;
                    ShowDetail(method);
                });

                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                textObj.transform.SetParent(entry.transform, false);
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8, 0);
                textRect.offsetMax = new Vector2(-8, 0);

                Text text = textObj.GetComponent<Text>();
                text.font = LocalizedTextManager.current_font;
                text.fontSize = 11;
                text.alignment = TextAnchor.MiddleLeft;
                text.supportRichText = true;

                // ★ 硬编码名称（或使用本地化后备）
                string displayName = LocalizedTextManager.getText(methodData.NameKey);
                if (string.IsNullOrEmpty(displayName)) displayName = methodData.Method.ToString();
                string color = methodData.ColorHex;
                string rarityName = CultivationMethodSystem.GetRarityName(methodData.Rarity);
                text.text = $"<color={color}>[{rarityName}] {displayName}</color>";

                TipButton tip = entry.AddComponent<TipButton>();
                tip.textOnClick = displayName;
                tip.textOnClickDescription = LocalizedTextManager.getText(methodData.DescKey);
            }

            // 强制刷新布局
            RectTransform contentRect = _listContent as RectTransform;
            if (contentRect != null)
            {
                int count = _listContent.childCount;
                float height = count * 26f + 6f;
                contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_listContent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_listContent.parent as RectTransform);

            // 滚动到顶部
            ScrollRect scrollRect = _listContent.parent?.parent?.GetComponent<ScrollRect>();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }

        private static void ShowDetail(CultivationMethod method)
        {
            if (_detailText == null) return;

            MethodData data = CultivationMethodSystem.GetMethodData(method);
            if (data == null || data.Method == CultivationMethod.None)
            {
                _detailText.text = "请从上方选择功法查看详情";
                return;
            }

            StringBuilder sb = new StringBuilder();
            string name = LocalizedTextManager.getText(data.NameKey);
            if (string.IsNullOrEmpty(name)) name = data.Method.ToString();
            string desc = LocalizedTextManager.getText(data.DescKey);
            if (string.IsNullOrEmpty(desc)) desc = "无描述";
            string effect = LocalizedTextManager.getText(data.SpecialEffectKey);
            if (string.IsNullOrEmpty(effect)) effect = "无特殊效果";

            sb.AppendLine($"<color={data.ColorHex}><b>{name}</b></color>");
            sb.AppendLine($"<color=#AAAAAA>{desc}</color>");
            sb.AppendLine($"<color=#888888>稀有度:</color> <color={GetRarityColorHex(data.Rarity)}>{CultivationMethodSystem.GetRarityName(data.Rarity)}</color>");
            sb.AppendLine($"<color=#888888>解锁境界:</color> {CultivationMethodSystem.GetRealmName(data.UnlockRealm)}");
            sb.AppendLine($"<color=#888888>修炼加成:</color> <color=#FFD700>+{(data.SpeedBonus * 100):F0}%</color>");
            sb.AppendLine($"<color=#888888>攻击:</color> {(data.AttackMod * 100):F0}%  <color=#888888>防御:</color> {(data.DefenseMod * 100):F0}%");
            sb.AppendLine($"<color=#888888>效果:</color> {effect}");

            _detailText.text = sb.ToString();
        }

        private static string GetRarityColorHex(MethodRarity rarity)
        {
            Color c = CultivationMethodSystem.GetRarityColor(rarity);
            return $"#{ColorUtility.ToHtmlStringRGB(c)}";
        }

        // ---- 补发功法 ----
        private static void GrantMethodsToAll()
        {
            if (World.world?.units == null)
            {
                WorldTip.showNow("世界未加载", true, "top");
                return;
            }

            int count = 0;
            var allUnits = World.world.units.getSimpleList();
            foreach (Actor actor in allUnits)
            {
                if (actor == null || !actor.isAlive() || !actor.isSapient()) continue;
                if (actor.GetRealmLevel() < 3) continue;
                if (CultivationMethodSystem.GetMethods(actor).Count > 0) continue;

                if (CultivationMethodSystem.GrantRandomMethod(actor))
                    count++;
            }

            if (count > 0)
            {
                WorldTip.showNow($"✅ 已为 {count} 个角色补发功法", true, "top");
                Debug.Log($"[念力修行] 补发功法给 {count} 个大通境及以上角色");
            }
            else
            {
                WorldTip.showNow("所有大通境及以上角色已有功法", true, "top");
            }
        }

        public static void ShowWindow()
        {
            if (!_initialized) Init();
            PopulateMethodList();
            if (_selectedMethod != CultivationMethod.None)
                ShowDetail(_selectedMethod);
            else
                _detailText.text = "请从上方选择功法查看详情";
            ScrollWindow.showWindow("NianLiCultivationMethodUI");
        }
    }
}