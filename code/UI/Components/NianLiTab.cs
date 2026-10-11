using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.UI;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Utils;

namespace NianLiXiuXing.code.UI
{
    public static class NianLiTab
    {
        private static PowersTab _tabInstance;
        private static bool _initialized;
        private static GameObject _statsPanel;
        private static Text _statsText;

        public static void Init()
        {
            if (_initialized) return;

            Sprite icon = SpriteTextureLoader.getSprite("ui/NianLi");
            if (icon == null)
                icon = Resources.Load<Sprite>("ui/icons/iconQuestionMark");

            _tabInstance = TabManager.CreateTab(
                "nianli_tab",
                LM.Get("nianli_tab_title"),
                LM.Get("nianli_tab_subtitle"),
                icon
            );

            if (_tabInstance != null)
            {
                BuildContent();
                _initialized = true;
                Debug.Log("[念力修行] Tab 创建成功！");
            }
            else
            {
                Debug.LogError("[念力修行] Tab 创建失败！");
            }
        }

        private static void BuildContent()
        {
            if (_tabInstance == null) return;

            var panel = new GameObject("NianLiPanel");
            panel.transform.SetParent(_tabInstance.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = new Vector2(10, 10);
            panelRect.offsetMax = new Vector2(-10, -10);

            // ---- 说明文字 ----
            GameObject textObj = new GameObject("InfoText");
            textObj.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 1);
            textRect.anchoredPosition = new Vector2(0, -10);
            textRect.sizeDelta = new Vector2(0, 50);
            Text text = textObj.AddComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = LM.Get("nianli_tab_description");

            // ============================================================
            // ★ 第一行按钮：全局功能 ★
            // ============================================================

            // ---- 1. 排行榜 ----
            CreateTabButton("NianLiRanking_Button", "NianLiRankingWindow", "ui/NianLiRanking",
                LM.Get("nianli_ranking_title"), LM.Get("nianli_ranking_description"));

            // ---- 2. 飞升记录 ----
            CreateTabButton("NianLiAscension_Button", "NianLiAscensionRecordWindow", "ui/NianLiAscension",
                LM.Get("nianli_ascension_record_title"), LM.Get("nianli_ascension_record_description"));

            // ---- 3. 世界道碑 ----
            CreateTabButton("NianLiDaoBei_Button", "NianLiDaoBeiUI", "ui/NianLiDaoBei",
                LM.Get("world_dao_bei"), LM.Get("nianli_daobei_description"));

            // ---- 4. 功法百科 ----
            CreateTabButton("NianLiMethod_Button", "NianLiCultivationMethodUI", "ui/NianLiMethod",
                LM.Get("nianli_method_bible_title"), LM.Get("nianli_method_bible_description"));

            // ============================================================
            // ★★★ 5. 性能监控（新增） ★★★
            // ============================================================
         CreateTabButton("NianLiPerformance_Button", "NianLiPerformanceUI", "ui/NianLiWorldInfo",
    LM.Get("nianli_world_info_button_title"),
    LM.Get("nianli_world_info_button_desc"));
            // ============================================================
            // ★ 第二行：全局统计面板 ★
            // ============================================================

            CreateStatsPanel(panel);

            Debug.Log("[念力修行] Tab 按钮创建完成");
        }

        /// <summary>
        /// 辅助方法：创建 Tab 按钮，包含空值保护
        /// </summary>
        private static void CreateTabButton(string buttonId, string windowId, string iconPath, string title, string desc)
        {
            if (_tabInstance == null) return;

            Sprite icon = SpriteTextureLoader.getSprite(iconPath);
            if (icon == null)
                icon = Resources.Load<Sprite>("ui/icons/iconQuestionMark");

            PowerButton button = PowerButtonCreator.CreateWindowButton(buttonId, windowId, icon);
            if (button == null)
            {
                Debug.LogWarning($"[念力修行] 创建按钮 {buttonId} 失败，跳过");
                return;
            }

            // 设置按钮显示文本
            Text btnText = button.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = title;

            // 设置提示
            TipButton tip = button.GetComponent<TipButton>();
            if (tip != null)
            {
                tip.textOnClick = title;
                tip.textOnClickDescription = desc;
            }

            PowerButtonCreator.AddButtonToTab(button, _tabInstance);
        }

        // ---- 创建统计面板 ----
        private static void CreateStatsPanel(GameObject parentPanel)
        {
            if (parentPanel == null) return;

            GameObject statsObj = new GameObject("StatsPanel", typeof(RectTransform));
            statsObj.transform.SetParent(parentPanel.transform, false);
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0);
            statsRect.anchorMax = new Vector2(1, 0.3f);
            statsRect.offsetMin = new Vector2(10, 10);
            statsRect.offsetMax = new Vector2(-10, -5);

            // 添加 VerticalLayoutGroup
            VerticalLayoutGroup layout = statsObj.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 2;
            layout.padding = new RectOffset(5, 5, 5, 5);

            // 标题
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(statsObj.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 20);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.font = LocalizedTextManager.current_font;
            titleText.fontSize = 12;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.text = "📊 " + LM.Get("world_stats_title");

            // 统计内容
            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(Text));
            contentObj.transform.SetParent(statsObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0, 60);
            Text contentText = contentObj.GetComponent<Text>();
            contentText.font = LocalizedTextManager.current_font;
            contentText.fontSize = 10;
            contentText.alignment = TextAnchor.UpperLeft;
            contentText.color = new Color(0.8f, 0.8f, 0.8f);
            contentText.supportRichText = true;
            _statsText = contentText;

            // 启动自动更新
            var updater = statsObj.AddComponent<StatsPanelUpdater>();
            updater.Initialize(contentText);
        }

        // ---- 自动更新统计面板 ----
        private class StatsPanelUpdater : MonoBehaviour
        {
            private Text _text;
            private float _updateTimer = 0f;
            private const float UPDATE_INTERVAL = 5f;

            public void Initialize(Text text)
            {
                _text = text;
                UpdateStats();
            }

            private void Update()
            {
                _updateTimer += Time.deltaTime;
                if (_updateTimer >= UPDATE_INTERVAL)
                {
                    _updateTimer = 0f;
                    UpdateStats();
                }
            }

            private void UpdateStats()
            {
                if (_text == null || World.world?.units == null) return;

                int total = 0;
                int[] realmCounts = new int[6];
                int totalKarma = 0;
                int highKarmaCount = 0;

                // ★ 使用 for 循环代替 foreach
                var units = World.world.units.getSimpleList();
                for (int i = 0; i < units.Count; i++)
                {
                    Actor actor = units[i];
                    if (actor == null || !actor.isAlive() || !actor.isSapient()) continue;
                    int realm = actor.GetRealmLevel();
                    if (realm >= 1 && realm <= 5)
                    {
                        realmCounts[realm]++;
                        total++;
                    }
                    float karma = KarmaSystem.GetKarma(actor);
                    if (karma > 0)
                    {
                        totalKarma += Mathf.RoundToInt(karma);
                        if (karma > 50f) highKarmaCount++;
                    }
                }

                string[] realmNames = { "", "养念", "复观", "大通", "环界", "托举" };
                string realmLine = "";
                for (int i = 1; i <= 5; i++)
                {
                    if (realmCounts[i] > 0)
                        realmLine += $"{realmNames[i]} {realmCounts[i]}  ";
                }

                _text.text = $"👥 总修士：{total}\n" +
                             $"📈 {realmLine}\n" +
                             $"☯ 世界业力：{totalKarma}，业力高危（>50）：{highKarmaCount} 人";
            }
        }
    }
}