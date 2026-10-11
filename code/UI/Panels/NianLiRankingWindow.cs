using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using NeoModLoader.General;

namespace NianLiXiuXing.code.UI
{
    public static class NianLiRankingWindow
    {
        private static ScrollWindow _window;
        private static Transform _content;
        private static GameObject _entryPrefab;
        private static bool _initialized;

        private const float WINDOW_WIDTH = 240f;
        private const float WINDOW_HEIGHT = 320f;

        public static void Init()
        {
            if (_initialized) return;

            _window = WindowCreator.CreateEmptyWindow(
                "NianLiRankingWindow",
                LM.Get("nianli_ranking_title"),
                "ui/NianLiRanking"
            );

            var bg = _window.transform.Find("Background");
            if (bg != null)
            {
                bg.GetComponent<RectTransform>().sizeDelta = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            }

            CreateScrollView();
            _entryPrefab = CreateEntryPrefab();
            _entryPrefab.SetActive(false);

            var callback = _window.gameObject.AddComponent<WindowOpenCallback>();
            callback.onWindowOpen = Refresh;

            _initialized = true;
        }

        public static void ShowWindow()
        {
            if (!_initialized) Init();
            ScrollWindow.showWindow("NianLiRankingWindow");
            Refresh();
        }

        public static void ShowWindowFromSwitch(bool value)
        {
            ShowWindow();
        }

        private static void CreateScrollView()
        {
            var bg = _window.transform.Find("Background");

            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(bg, false);
            var scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(8, 8);
            scrollRect.offsetMax = new Vector2(-8, -35);

            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            scrollObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
            scrollObj.AddComponent<Mask>();

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 2;
            layout.padding = new RectOffset(3, 3, 3, 3);

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            _content = content.transform;
        }

        private static GameObject CreateEntryPrefab()
        {
            var entry = new GameObject("RankEntry");
            entry.AddComponent<RectTransform>().sizeDelta = new Vector2(220, 24);

            var bg = entry.AddComponent<Image>();
            bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");

            var rank = CreateText("Rank", entry.transform, 28);
            rank.anchorMin = new Vector2(0, 0.5f);
            rank.anchorMax = new Vector2(0, 0.5f);
            rank.pivot = new Vector2(0, 0.5f);
            rank.anchoredPosition = new Vector2(5, 0);
            rank.sizeDelta = new Vector2(28, 24);

            var name = CreateText("Name", entry.transform, 70);
            name.anchorMin = new Vector2(0, 0.5f);
            name.anchorMax = new Vector2(0, 0.5f);
            name.pivot = new Vector2(0, 0.5f);
            name.anchoredPosition = new Vector2(36, 0);
            name.sizeDelta = new Vector2(70, 24);

            var realm = CreateText("Realm", entry.transform, 34);
            realm.anchorMin = new Vector2(0, 0.5f);
            realm.anchorMax = new Vector2(0, 0.5f);
            realm.pivot = new Vector2(0, 0.5f);
            realm.anchoredPosition = new Vector2(109, 0);
            realm.sizeDelta = new Vector2(34, 24);

            var power = CreateText("Power", entry.transform, 50);
            power.anchorMin = new Vector2(0, 0.5f);
            power.anchorMax = new Vector2(0, 0.5f);
            power.pivot = new Vector2(0, 0.5f);
            power.anchoredPosition = new Vector2(146, 0);
            power.sizeDelta = new Vector2(50, 24);

            var hearts = CreateText("Hearts", entry.transform, 25);
            hearts.anchorMin = new Vector2(0, 0.5f);
            hearts.anchorMax = new Vector2(0, 0.5f);
            hearts.pivot = new Vector2(0, 0.5f);
            hearts.anchoredPosition = new Vector2(199, 0);
            hearts.sizeDelta = new Vector2(25, 24);

            return entry;
        }

        private static RectTransform CreateText(string name, Transform parent, float width)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 24);

            var text = obj.AddComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 8;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;

            return rect;
        }

        public static void Refresh()
        {
            if (_content == null) return;

            foreach (Transform child in _content)
                UnityEngine.Object.Destroy(child.gameObject);

            var top10 = RankingSystem.GetTop10();
            if (top10.Count == 0)
            {
                var empty = new GameObject("Empty");
                empty.transform.SetParent(_content, false);
                var rect = empty.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(190, 40);
                var text = empty.AddComponent<Text>();
                text.font = LocalizedTextManager.current_font;
                text.fontSize = 12;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(0.6f, 0.6f, 0.6f);
                text.text =  LM.Get("nianli_ranking_empty");
                return;
            }

            string[] realmNames = { "", LM.Get("realm_nurture"), LM.Get("realm_reflect"), LM.Get("realm_great"), LM.Get("realm_ring"), LM.Get("realm_uphold") };
            int rank = 1;

            foreach (var entryData in top10)
            {
                var entry = UnityEngine.Object.Instantiate(_entryPrefab, _content);
                entry.SetActive(true);

                var rankText = entry.transform.Find("Rank")?.GetComponent<Text>();
                if (rankText != null)
                {
                    rankText.text = $"#{rank}";
                    rankText.color = rank <= 3 ? new Color(1f, 0.84f, 0f) : Color.white;
                }

                var nameText = entry.transform.Find("Name")?.GetComponent<Text>();
                if (nameText != null)
                {
                    nameText.text = entryData.actor.data?.name ?? "未知";
                    nameText.color = entryData.realm >= 4 ? new Color(1f, 0.5f, 0f) : Color.white;
                }

                var realmText = entry.transform.Find("Realm")?.GetComponent<Text>();
                if (realmText != null)
                {
                    if (entryData.realm > 0 && entryData.realm <= 5)
                        realmText.text = realmNames[entryData.realm];
                    else
                        realmText.text = LM.Get("realm_mortal");
                }

                var powerText = entry.transform.Find("Power")?.GetComponent<Text>();
                if (powerText != null)
                {
                    float power = entryData.power;
                    powerText.text = power >= 10000 ? $"{power / 10000f:F1}万" : $"{power:F0}";
                    powerText.color = power >= 50000 ? new Color(1f, 0.84f, 0f) : Color.white;
                }

                var heartsText = entry.transform.Find("Hearts")?.GetComponent<Text>();
                if (heartsText != null)
                {
                    heartsText.text = $"♥{entryData.heartCount}";
                    heartsText.color = entryData.heartCount >= 6 ? new Color(1f, 0.84f, 0f) : Color.white;
                }

                var btn = entry.GetComponent<Button>() ?? entry.AddComponent<Button>();
                long actorId = entryData.actor?.getID() ?? 0;
                btn.onClick.AddListener(() =>
                {
                    if (entryData.actor != null && entryData.actor.isAlive())
                        ActionLibrary.openUnitWindow(entryData.actor);
                });

                rank++;
            }
        }

        private class WindowOpenCallback : MonoBehaviour
        {
            public Action onWindowOpen;
            private void OnEnable() => onWindowOpen?.Invoke();
        }
    }

    public static class RankingSystem
    {
        public static List<(Actor actor, float power, int realm, int heartCount)> GetTop10()
        {
            var list = new List<(Actor actor, float power, int realm, int heartCount)>();

            if (World.world?.units == null) return list;

            foreach (Actor actor in World.world.units)
            {
                if (actor?.data == null || !actor.isAlive()) continue;
                if (actor.GetNianLi() <= 0) continue;

                int realm = actor.GetRealmLevel();
                if (realm < 0) realm = 0;

                int heartCount = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (actor.IsHeartBroken(i)) heartCount++;
                }

                float realmPower = realm * 1000f;
                float heartPower = heartCount * 200f;
                float nianLiPower = actor.GetNianLi() / 10f;
                float attackPower = actor.stats["NianLiAttack"];
                float defensePower = actor.stats["NianLiDefense"] * 0.5f;
                // ★ 修复：使用正确的键 "CriticalDamage"
                float critPower = actor.stats["CriticalDamage"] * 100f;
                float totalPower = realmPower + heartPower + nianLiPower + attackPower + defensePower + critPower;

                list.Add((actor, totalPower, realm, heartCount));
            }

            list.Sort((a, b) => b.power.CompareTo(a.power));

            if (list.Count > 10)
                list = list.GetRange(0, 10);

            return list;
        }
    }
}