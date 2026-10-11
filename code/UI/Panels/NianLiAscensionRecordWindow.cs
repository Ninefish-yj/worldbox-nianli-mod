using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoModLoader.General;
using NianLiXiuXing.code.Systems;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.UI
{
    public static class NianLiAscensionRecordWindow
    {
        private static ScrollWindow _window;
        private static Transform _content;
        private static GameObject _entryPrefab;
        private static bool _initialized;

        private const float WINDOW_WIDTH = 240f;
        private const float WINDOW_HEIGHT = 320f;

        private static Font _cachedFont;

        private static Font GetUIFont()
        {
            if (_cachedFont != null) return _cachedFont;
            Font font = LocalizedTextManager.current_font;
            if (font != null)
            {
                _cachedFont = font;
                return _cachedFont;
            }
            _cachedFont = new Font();
            return _cachedFont;
        }

        public static void Init()
        {
            if (_initialized) return;

            _window = WindowCreator.CreateEmptyWindow(
                "NianLiAscensionRecordWindow",
                LM.Get("nianli_ascension_record_title"),
                "ui/NianLiAscension"
            );

            if (_window == null)
            {
                Debug.LogError("[飞升记录] 创建窗口失败！");
                return;
            }

            var bg = _window.transform.Find("Background");
            if (bg != null)
                bg.GetComponent<RectTransform>().sizeDelta = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

            CreateScrollView();

            var callback = _window.gameObject.AddComponent<WindowOpenCallback>();
            callback.onWindowOpen = Refresh;

            _initialized = true;
            Debug.Log("[飞升记录] 窗口初始化完成");
        }

        public static void ShowWindow()
        {
            if (!_initialized) Init();

            if (_entryPrefab == null)
            {
                _entryPrefab = CreateEntryPrefab();
                _entryPrefab.SetActive(false);
            }

            LoadRecords();
            Refresh();
            ScrollWindow.showWindow("NianLiAscensionRecordWindow");
        }

        public static void ShowWindowFromSwitch(bool value)
        {
            ShowWindow();
        }

        private static void LoadRecords()
        {
            AscensionRecordSystem.Load();
        }

        private static void CreateScrollView()
        {
            var bg = _window.transform.Find("Background");
            if (bg == null) return;

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
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 2;
            layout.padding = new RectOffset(5, 5, 5, 5);

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            _content = content.transform;
        }

        private static GameObject CreateEntryPrefab()
        {
            var entry = new GameObject("RecordEntry");
            entry.AddComponent<RectTransform>().sizeDelta = new Vector2(220, 28);

            var bg = entry.AddComponent<Image>();
            bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");

            var nameText = CreateText("Name", entry.transform, 68);
            nameText.anchorMin = new Vector2(0, 0.5f);
            nameText.anchorMax = new Vector2(0, 0.5f);
            nameText.pivot = new Vector2(0, 0.5f);
            nameText.anchoredPosition = new Vector2(5, 0);
            nameText.sizeDelta = new Vector2(68, 24);

            var statusText = CreateText("Status", entry.transform, 42);
            statusText.anchorMin = new Vector2(0, 0.5f);
            statusText.anchorMax = new Vector2(0, 0.5f);
            statusText.pivot = new Vector2(0, 0.5f);
            statusText.anchoredPosition = new Vector2(76, 0);
            statusText.sizeDelta = new Vector2(42, 24);

            var timeText = CreateText("Time", entry.transform, 48);
            timeText.anchorMin = new Vector2(0, 0.5f);
            timeText.anchorMax = new Vector2(0, 0.5f);
            timeText.pivot = new Vector2(0, 0.5f);
            timeText.anchoredPosition = new Vector2(121, 0);
            timeText.sizeDelta = new Vector2(48, 24);

            // 放置按钮
            var btnObj = new GameObject("PlaceButton");
            btnObj.transform.SetParent(entry.transform, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 0.5f);
            btnRect.anchorMax = new Vector2(1, 0.5f);
            btnRect.pivot = new Vector2(1, 0.5f);
            btnRect.anchoredPosition = new Vector2(-5, 0);
            btnRect.sizeDelta = new Vector2(34, 20);

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.8f);

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnText = btnObj.AddComponent<Text>();
            btnText.font = GetUIFont();
            btnText.fontSize = 9;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.text = "放置";

            btnObj.SetActive(false);

            return entry;
        }

        private static RectTransform CreateText(string name, Transform parent, float width)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 24);

            var text = obj.AddComponent<Text>();
            text.font = GetUIFont();
            text.fontSize = 9;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;

            return rect;
        }

        public static void Refresh()
        {
            if (_content == null) return;

            foreach (Transform child in _content)
                Object.Destroy(child.gameObject);

            var records = AscensionRecordSystem.GetAllRecords();
            if (records.Count == 0)
            {
                var empty = new GameObject("Empty");
                empty.transform.SetParent(_content, false);
                var rect = empty.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(190, 40);
                var text = empty.AddComponent<Text>();
                text.font = GetUIFont();
                text.fontSize = 12;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.gray;
                text.text = "暂无飞升记录";
                return;
            }

            foreach (var record in records)
            {
                var entry = Object.Instantiate(_entryPrefab, _content);
                entry.SetActive(true);

                var nameText = entry.transform.Find("Name")?.GetComponent<Text>();
                if (nameText != null)
                    nameText.text = record.actorName;

                var statusText = entry.transform.Find("Status")?.GetComponent<Text>();
                if (statusText != null)
                {
                    statusText.text = record.isSuccess ? "✅" : "💀";
                    statusText.color = record.isSuccess ? Color.green : Color.red;
                }

                var timeText = entry.transform.Find("Time")?.GetComponent<Text>();
                if (timeText != null)
                    timeText.text = record.timestamp + "年";

                Actor existingActor = null;
                if (World.world != null && World.world.units != null)
                {
                    foreach (var u in World.world.units)
                    {
                        if (u != null && u.getID() == record.actorId)
                        {
                            existingActor = u;
                            break;
                        }
                    }
                }

                bool isDead = existingActor == null || !existingActor.isAlive();

                var btnObj = entry.transform.Find("PlaceButton")?.gameObject;
                if (btnObj != null)
                {
                    var btn = btnObj.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();

                    // ★ 获取按钮组件
                    Image btnImg = btnObj.GetComponent<Image>();
                    Text btnText = btnObj.GetComponentInChildren<Text>();

                    // ★ 情况1：记录有效且角色存在但死亡 → 显示“复活”按钮
                    if (record.isSuccess && existingActor != null && !existingActor.isAlive())
                    {
                        btnObj.SetActive(true);
                        if (btnImg != null) btnImg.color = new Color(0.2f, 0.6f, 0.2f);
                        if (btnText != null) btnText.text = "复活";
                        long id = record.actorId;
                        btn.onClick.AddListener(() => { ReviveActor(id); });
                    }
                    // ★ 情况2：记录有效且角色完全不存在 → 显示“放置”按钮
                    else if (record.isSuccess && existingActor == null)
                    {
                        btnObj.SetActive(true);
                        if (btnImg != null) btnImg.color = new Color(0.2f, 0.4f, 0.8f);
                        if (btnText != null) btnText.text = "放置";
                        long id = record.actorId;
                        btn.onClick.AddListener(() => { PlaceActorFromRecord(id); });
                    }
                    // ★ 情况3：角色存在且存活 → 隐藏按钮
                    else
                    {
                        btnObj.SetActive(false);
                    }
                }
            }
        }

        private static void PlaceActorFromRecord(long actorId)
        {
            var record = AscensionRecordSystem.GetRecordById(actorId);
            if (record == null)
            {
                WorldTip.showNow(LM.Get("RecordNotExist"), true, "top");
                return;
            }

            if (AscensionRecordSystem.IsActorExists(actorId))
            {
                WorldTip.showNow(string.Format(LM.Get("ActorAlreadyExists"), record.actorName), true, "top");
                return;
            }

            Actor newActor = ReincarnationHelper.CreateReincarnationBody(record.actorName);
            if (newActor == null)
            {
                WorldTip.showNow(LM.Get("PlaceActorFail"), true, "top");
                return;
            }

            if (record.realmLevel > 0 && record.realmLevel <= 5)
            {
                newActor.SetRealmLevel(record.realmLevel);
                if (!newActor.hasTrait($"Realm_{record.realmLevel}"))
                    newActor.addTrait($"Realm_{record.realmLevel}");
            }

            if (!newActor.hasTrait(TraitIds.WeiGe99))
                newActor.addTrait(TraitIds.WeiGe99);

            AscensionRecordSystem.RemoveRecordById(actorId);
            AscensionRecordSystem.AddRecord(newActor, true);

            if (newActor.current_tile != null)
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", newActor.current_tile, 3f);
                if (effect != null)
                {
                    effect.sprite_renderer.color = new Color(1f, 0.84f, 0f);
                    Object.Destroy(effect.gameObject, 3f);
                }
            }

            WorldTip.showNow(string.Format(LM.Get("PlaceActorSuccess"), record.actorName), true, "top");
            Debug.Log($"[飞升记录] 放置 {record.actorName} (新ID:{newActor.getID()})");

            Refresh();
        }

        private static void ReviveActor(long actorId)
        {
            Actor actor = null;
            if (World.world != null && World.world.units != null)
            {
                foreach (var u in World.world.units)
                {
                    if (u != null && u.getID() == actorId)
                    {
                        actor = u;
                        break;
                    }
                }
            }

            if (actor == null)
            {
                WorldTip.showNow("❌ 该角色已彻底消失，无法复活", true, "top");
                return;
            }

            actor.restoreHealth(actor.getMaxHealth());
            actor.data.health = actor.getMaxHealth();

            AscensionRecordSystem.RemoveRecord(actor);

            WorldTip.showNow($"✨ {actor.data.name} 已从天地核心复活！", true, "top");
            Debug.Log($"[飞升记录] 复活 {actor.data.name} (ID:{actorId})");

            Refresh();
        }

        private class WindowOpenCallback : MonoBehaviour
        {
            public System.Action onWindowOpen;
            private void OnEnable() => onWindowOpen?.Invoke();
        }
    }
}