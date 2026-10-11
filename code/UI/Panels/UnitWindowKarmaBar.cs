using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.UI
{
    [HarmonyPatch]
    public static class UnitWindowKarmaBar
    {
        public const string BAR_NAME = "_nianli_karma_bar";
        private const string PANEL_NAME = "_nianli_karma_panel";
        private static readonly Color KARMA_COLOR_LOW = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color KARMA_COLOR_MID = new Color(0.8f, 0.8f, 0.2f);
        private static readonly Color KARMA_COLOR_HIGH = new Color(0.8f, 0.2f, 0.2f);

        public static void Init() { }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitWindow), nameof(UnitWindow.OnEnable))]
        public static void UnitWindow_OnEnable_Postfix(UnitWindow __instance)
        {
            if (__instance?.actor == null || !__instance.actor.isAlive()) return;
            __instance.StartCoroutine(DelayedAddKarmaBar(__instance));
        }

        private static System.Collections.IEnumerator DelayedAddKarmaBar(UnitWindow window)
        {
            yield return null;
            yield return null;
            try
            {
                if (window == null || window.actor == null || !window.actor.isAlive()) yield break;
                if (!window.actor.isSapient()) yield break;
                AddOrUpdateKarmaUI(window);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[念力修行] 业力UI异常: {e.Message}");
            }
        }

        private static void AddOrUpdateKarmaUI(UnitWindow window)
        {
            if (window?.actor == null) return;
            Actor actor = window.actor;

            float karma = KarmaSystem.GetKarma(actor);
            float virtue = KarmaSystem.GetVirtue(actor);

            Transform background = window.transform.Find("Background");
            if (background == null) return;

            // ---- 业力条 ----
            StatBar templateBar = FindTemplateBar(background);
            if (templateBar == null)
            {
                Debug.LogWarning("[念力修行] 未找到模板条，放弃创建业力条");
                return;
            }

            Transform parent = templateBar.transform.parent;
            if (parent == null) return;

            // 查找或创建业力条
            Transform existingBar = parent.Find(BAR_NAME);
            StatBar bar = null;
            if (existingBar != null)
            {
                bar = existingBar.GetComponent<StatBar>();
                if (bar != null)
                {
                    UpdateKarmaBar(bar, actor);
                    existingBar.gameObject.SetActive(karma > 0);
                }
            }
            else if (karma > 0)
            {
                bar = CreateKarmaBar(templateBar, parent);
                UpdateKarmaBar(bar, actor);
            }

            // ---- 业力管理面板（功德 + 净心按钮） ----
            Transform existingPanel = parent.Find(PANEL_NAME);
            GameObject panelObj = null;
            if (existingPanel != null)
            {
                panelObj = existingPanel.gameObject;
            }
            else
            {
                panelObj = CreateKarmaPanel(parent);
            }

            // 更新面板内容
            if (panelObj != null)
            {
                UpdateKarmaPanel(panelObj, actor);
                // 仅当业力或功德 > 0 时显示
                panelObj.SetActive(karma > 0 || virtue > 0);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
        }

        // ---- 查找模板条（优先念力条） ----
        private static StatBar FindTemplateBar(Transform parent)
        {
            Transform nianLiBar = parent.Find(UnitWindowNianLiUI.BAR_NAME);
            if (nianLiBar != null)
            {
                StatBar sb = nianLiBar.GetComponent<StatBar>();
                if (sb != null) return sb;
            }
            return FindHealthBarRecursive(parent);
        }

        private static StatBar FindHealthBarRecursive(Transform parent)
        {
            foreach (Transform child in parent)
            {
                StatBar sb = child.GetComponent<StatBar>();
                if (sb != null && child.name.ToLower().Contains("health"))
                    return sb;
                if (child.childCount > 0)
                {
                    StatBar found = FindHealthBarRecursive(child);
                    if (found != null) return found;
                }
            }
            return null;
        }

        // ---- 创建业力条 ----
        private static StatBar CreateKarmaBar(StatBar template, Transform parent)
        {
            StatBar newBar = Object.Instantiate(template, parent, false);
            newBar.name = BAR_NAME;

            RectTransform templateRect = template.GetComponent<RectTransform>();
            RectTransform newRect = newBar.GetComponent<RectTransform>();
            if (templateRect != null && newRect != null)
            {
                newRect.anchorMin = templateRect.anchorMin;
                newRect.anchorMax = templateRect.anchorMax;
                newRect.pivot = templateRect.pivot;
                newRect.sizeDelta = templateRect.sizeDelta;
                newRect.anchoredPosition = Vector2.zero;
            }

            // 设置图标
            Transform iconTransform = newBar.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImg = iconTransform.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconTransform.gameObject.SetActive(true);
                    Sprite sprite = SpriteTextureLoader.getSprite("ui/Karma");
                    if (sprite == null) sprite = Resources.Load<Sprite>("ui/icons/iconQuestionMark");
                    if (sprite != null) iconImg.sprite = sprite;
                }
            }

            // 插入到念力条之后
            int insertIndex = template.transform.GetSiblingIndex() + 1;
            Transform nianLiBar = parent.Find(UnitWindowNianLiUI.BAR_NAME);
            if (nianLiBar != null)
                insertIndex = nianLiBar.GetSiblingIndex() + 1;
            newBar.transform.SetSiblingIndex(insertIndex);

            return newBar;
        }

        // ---- 更新业力条数值 ----
        private static void UpdateKarmaBar(StatBar bar, Actor actor)
        {
            float karma = KarmaSystem.GetKarma(actor);
            float maxKarma = 100f;

            if (karma <= 0)
            {
                bar.gameObject.SetActive(false);
                return;
            }

            bar.gameObject.SetActive(true);
            float percentage = Mathf.Clamp((karma / maxKarma) * 100f, 0f, 100f);
            bar.setBar(percentage, 100f, "%", false, false, true, 0.25f);

            Image barImage = bar.bar?.GetComponent<Image>();
            if (barImage != null)
                barImage.color = GetKarmaColor(karma);

            var tip = bar.GetComponent<TipButton>() ?? bar.gameObject.AddComponent<TipButton>();
            tip.textOnClick = LM.Get("Karma");
            tip.textOnClickDescription = $"{LM.Get("Karma")}: {karma:F0}/100\n{GetKarmaDescription(karma)}";
        }

        // ---- 创建业力面板（功德 + 按钮） ----
        private static GameObject CreateKarmaPanel(Transform parent)
        {
            GameObject panel = new GameObject(PANEL_NAME, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            panel.transform.SetParent(parent, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, 24);
            panelRect.offsetMin = new Vector2(0, 0);
            panelRect.offsetMax = new Vector2(0, 0);

            HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4;
            layout.padding = new RectOffset(4, 4, 2, 2);

            // ---- 功德文本 ----
            GameObject virtueObj = new GameObject("VirtueText", typeof(RectTransform), typeof(Text));
            virtueObj.transform.SetParent(panel.transform, false);
            RectTransform virtueRect = virtueObj.GetComponent<RectTransform>();
            virtueRect.sizeDelta = new Vector2(100, 20);
            Text virtueText = virtueObj.GetComponent<Text>();
            virtueText.font = LocalizedTextManager.current_font;
            virtueText.fontSize = 10;
            virtueText.alignment = TextAnchor.MiddleLeft;
            virtueText.color = new Color(0.8f, 0.8f, 0.8f);
            virtueText.supportRichText = true;
            virtueText.text = "";

            // ---- 净心按钮 ----
            GameObject btnObj = new GameObject("CleanseButton", typeof(RectTransform), typeof(Button), typeof(Image));
            btnObj.transform.SetParent(panel.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(50, 20);
            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.6f);
            Button btn = btnObj.GetComponent<Button>();

            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            Text btnText = btnTextObj.GetComponent<Text>();
            btnText.font = LocalizedTextManager.current_font;
            btnText.fontSize = 10;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.text = LM.Get("karma_cleanse_button");

            btn.onClick.AddListener(() =>
            {
                UnitWindow window = Object.FindObjectOfType<UnitWindow>();
                if (window?.actor != null && window.actor.isAlive())
                {
                    KarmaSystem.PerformCleanse(window.actor);
                    // 刷新UI
                    AddOrUpdateKarmaUI(window);
                }
                else
                {
                    WorldTip.showNow("请先打开一个单位的详情窗口", true, "top");
                }
            });

            // 存储按钮引用以便更新状态
            btnObj.name = "CleanseButton"; // 便于查找

            // 插入到业力条之后
            int insertIndex = parent.Find(BAR_NAME)?.GetSiblingIndex() ?? parent.childCount;
            panel.transform.SetSiblingIndex(insertIndex + 1);

            return panel;
        }

        // ---- 更新业力面板内容 ----
        private static void UpdateKarmaPanel(GameObject panel, Actor actor)
        {
            float karma = KarmaSystem.GetKarma(actor);
            float virtue = KarmaSystem.GetVirtue(actor);

            // 更新功德文本
            Transform virtueTransform = panel.transform.Find("VirtueText");
            if (virtueTransform != null)
            {
                Text virtueText = virtueTransform.GetComponent<Text>();
                if (virtueText != null)
                {
                    virtueText.text = $"<color=#4CAF50>功德：{virtue:F0}/100</color>";
                }
            }

            // 更新净心按钮状态
            Transform btnTransform = panel.transform.Find("CleanseButton");
            if (btnTransform != null)
            {
                Button btn = btnTransform.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = (karma > 0);
                    Image img = btnTransform.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = btn.interactable ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                    }
                }
                // 更新按钮文本（显示消耗）
                Transform textTransform = btnTransform.Find("Text");
                if (textTransform != null)
                {
                    Text btnText = textTransform.GetComponent<Text>();
                    if (btnText != null)
                    {
                        if (karma > 0)
                        {
                            float cost = karma * ConfigCache.KarmaCleanseCostPerPoint;
                            btnText.text = $"净心（{cost:F0}）";
                        }
                        else
                        {
                            btnText.text = LM.Get("karma_cleanse_button");
                        }
                    }
                }
            }
        }

        // ---- 辅助方法 ----
        private static Color GetKarmaColor(float karma)
        {
            if (karma <= 30f) return KARMA_COLOR_LOW;
            if (karma <= 60f) return KARMA_COLOR_MID;
            return KARMA_COLOR_HIGH;
        }

        private static string GetKarmaDescription(float karma)
        {
            if (karma <= 10f) return "业力轻微，无负面影响";
            if (karma <= 30f) return "业力积累，心关略微不稳";
            if (karma <= 50f) return "业力缠身，心关倒退风险增加";
            if (karma <= 70f) return "业力深重，修行受阻";
            return "业力滔天，心魔缠身！";
        }
    }
}