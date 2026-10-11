using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.UI
{
    [HarmonyPatch]
    public static class NianLiStatsIcon
    {
        private const string ICON_GROUP_NAME = "NianLiIconGroup";

        public static void Init() { }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitWindow), nameof(UnitWindow.OnEnable))]
        public static void UnitWindow_OnEnable_Postfix(UnitWindow __instance)
        {
            if (__instance?.actor == null || !__instance.actor.isAlive()) return;
            __instance.StartCoroutine(DelayedInit(__instance));
        }

        private static IEnumerator DelayedInit(UnitWindow window)
        {
            yield return null;

            try
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.Log("[念力修行] ★ NianLiStatsIcon 延迟初始化开始");

                EnsureIconsExist(window);
                RefreshValues(window, window.actor);

                if (ConfigCache.EnableDebugLog)
                    Debug.Log("[念力修行] ★ NianLiStatsIcon 初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[念力修行] 延迟初始化异常: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void EnsureIconsExist(UnitWindow window)
        {
            try
            {
                Transform content = window.transform.Find("Background/Scroll View/Viewport/Content/content_more_icons");
                if (content == null)
                {
                    Debug.LogWarning("[念力修行] 未找到 content_more_icons，图标无法创建");
                    return;
                }

                Transform existingGroup = content.Find(ICON_GROUP_NAME);
                if (existingGroup != null)
                {
                    if (ConfigCache.EnableDebugLog)
                        Debug.Log("[念力修行] 图标组已存在，跳过创建");
                    return;
                }

                if (ConfigCache.EnableDebugLog)
                    Debug.Log("[念力修行] 图标组不存在，开始创建...");

                InitializeIcons(window, content);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[念力修行] EnsureIconsExist 失败: {e.Message}");
            }
        }

        private static void InitializeIcons(UnitWindow window, Transform content)
        {
            try
            {
                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    if (content.GetChild(i).name == ICON_GROUP_NAME)
                        Object.Destroy(content.GetChild(i).gameObject);
                }

                Transform scrollView = window.transform.Find("Background/Scroll View");
                if (scrollView != null)
                {
                    ScrollRect sr = scrollView.GetComponent<ScrollRect>();
                    if (sr != null) sr.enabled = true;
                }

                content.gameObject.AddOrGetComponent<StatsIconContainer>();

                if (content.childCount < 5)
                {
                    Debug.LogWarning($"[念力修行] 原版行数量不足 ({content.childCount})，无法创建图标组");
                    return;
                }

                Transform originalParent = content.GetChild(4);
                if (originalParent == null)
                {
                    Debug.LogWarning("[念力修行] 未找到第4行作为模板");
                    return;
                }

                Transform i_kills = originalParent.Find("i_kills");
                if (i_kills == null)
                {
                    Debug.LogWarning("[念力修行] 模板行中未找到 i_kills 节点");
                    return;
                }

                Transform newGroup = Object.Instantiate(originalParent);
                newGroup.name = ICON_GROUP_NAME;
                newGroup.SetParent(content, false);
                newGroup.localScale = Vector3.one;
                newGroup.SetSiblingIndex(content.childCount - 1);

                RectTransform rt = newGroup.GetComponent<RectTransform>();
                if (rt != null)
                {
                    RectTransform originalRt = originalParent.GetComponent<RectTransform>();
                    if (originalRt != null)
                    {
                        rt.anchorMin = originalRt.anchorMin;
                        rt.anchorMax = originalRt.anchorMax;
                        rt.offsetMin = originalRt.offsetMin;
                        rt.offsetMax = originalRt.offsetMax;
                        rt.sizeDelta = originalRt.sizeDelta;
                        rt.pivot = originalRt.pivot;
                    }
                }

                for (int i = newGroup.childCount - 1; i >= 0; i--)
                {
                    Transform child = newGroup.GetChild(i);
                    if (child != null && child.name != "i_kills")
                        Object.Destroy(child.gameObject);
                }

                Transform template = newGroup.Find("i_kills");
                if (template == null) return;

                // ★ 图标配置：ID 必须与 setIconValue 中的键一致
                var iconConfigs = new[]
                {
                    new { id = "NianLi", icon = "ui/NianLi", nameKey = "statsIcon_NianLi", descKey = "nianli_nianli" },
                    new { id = "HeartBonus", icon = "ui/HeartBonus", nameKey = "statsIcon_HeartBonus", descKey = "statsIcon_HeartBonus" },
                    new { id = "NianLiAttack", icon = "ui/NianLiAttack", nameKey = "statsIcon_NianLiAttack", descKey = "statsIcon_NianLiAttack" },
                    new { id = "NianLiDefense", icon = "ui/NianLiDefense", nameKey = "statsIcon_NianLiDefense", descKey = "statsIcon_NianLiDefense" },
                    new { id = "CriticalDamage", icon = "ui/NianLiCrit", nameKey = "statsIcon_CriticalDamage", descKey = "statsIcon_CriticalDamage" },
                    new { id = "DeathCount", icon = "ui/DeathCount", nameKey = "statsIcon_DeathCount", descKey = "statsIcon_DeathCount" },
                };

                foreach (var cfg in iconConfigs)
                {
                    Transform newIcon = Object.Instantiate(template, newGroup);
                    StatsIcon icon = newIcon.GetComponent<StatsIcon>();
                    if (icon != null)
                    {
                        // ★ 设置图标
                        Sprite sprite = SpriteTextureLoader.getSprite(cfg.icon);
                        if (sprite == null)
                            sprite = Resources.Load<Sprite>("ui/icons/iconQuestionMark");
                        if (sprite != null)
                            icon.getIcon().sprite = sprite;

                        // ★★★ 关键：保持 icon.name 为英文 ID，以便 setIconValue 能查找 ★★★
                        icon.name = cfg.id;
                        newIcon.gameObject.name = cfg.id;

                        // 获取本地化文本
                        string displayName = LM.Get(cfg.nameKey);
                        string displayDesc = LM.Get(cfg.descKey);

                        // ★ 设置子 Text 组件的显示文本（标签）
                        Text labelText = newIcon.GetComponentInChildren<Text>();
                        if (labelText != null)
                        {
                            labelText.text = displayName;
                        }
                        else
                        {
                            // 如果没找到，尝试遍历所有子 Text
                            Text[] texts = newIcon.GetComponentsInChildren<Text>();
                            foreach (Text t in texts)
                            {
                                // 通常标签是第一个或名称包含 "name"/"label"
                                if (t.gameObject.name.ToLower().Contains("name") || t.gameObject.name.ToLower().Contains("label"))
                                {
                                    t.text = displayName;
                                    break;
                                }
                            }
                        }

                        // ★ 设置 TipButton（悬停提示）
                        TipButton tipButton = newIcon.GetComponent<TipButton>();
                        if (tipButton != null)
                        {
                            tipButton.textOnClick = displayName;
                            tipButton.textOnClickDescription = displayDesc;
                        }

                        if (ConfigCache.EnableDebugLog)
                            Debug.Log($"[念力修行] 创建图标: {cfg.id} -> 标签: {displayName}, 描述: {displayDesc}");
                    }
                }

                Object.DestroyImmediate(template.gameObject);

                VerticalLayoutGroup layoutGroup = content.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);

                if (ConfigCache.EnableDebugLog)
                    Debug.Log("[念力修行] ✅ 成功创建属性图标组");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[念力修行] 初始化图标失败: {e.Message}");
            }
        }

        private static void RefreshValues(UnitWindow window, Actor actor)
        {
            try
            {
                // ★ 使用与创建时相同的 ID
                window.setIconValue("NianLi", actor.GetNianLi());
                window.setIconValue("HeartBonus", actor.stats["HeartBonus"]);
                window.setIconValue("NianLiAttack", actor.stats["NianLiAttack"]);
                window.setIconValue("NianLiDefense", actor.stats["NianLiDefense"]);
                window.setIconValue("CriticalDamage", actor.stats["CriticalDamage"] * 100f);
                window.setIconValue("DeathCount", actor.GetDeathCount());

                if (ConfigCache.EnableDebugLog)
                    Debug.Log("[念力修行] ✅ 属性值已刷新");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[念力修行] 刷新属性值失败: {e.Message}");
            }
        }
    }

    public class StatsIconData
    {
        public string Name { get; }
        public string IconPath { get; }
        public bool IsShow { get; }
        public string Description { get; }

        public StatsIconData(string name, string iconPath, bool isShow = false, string description = "")
        {
            Name = name;
            IconPath = iconPath;
            IsShow = isShow;
            Description = string.IsNullOrEmpty(description) ? name : description;
        }
    }
}