using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.UI
{
    [HarmonyPatch]
    public static class UnitWindowNianLiUI
    {
        // ★ 改为 public，供业力条等其他UI组件引用
        public const string BAR_NAME = "_nianli_bar";
        private static readonly Color BAR_COLOR = new Color(0.6f, 0.3f, 1.0f);

        public static void Init() { }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitWindow), nameof(UnitWindow.OnEnable))]
        public static void UnitWindow_OnEnable_Postfix(UnitWindow __instance)
        {
            if (__instance?.actor == null || !__instance.actor.isAlive()) return;
            __instance.StartCoroutine(DelayedAddUI(__instance));
        }

        private static System.Collections.IEnumerator DelayedAddUI(UnitWindow window)
        {
            yield return null;
            yield return null;
            try
            {
                if (window == null || window.actor == null || !window.actor.isAlive()) yield break;
                if (!window.actor.isSapient()) yield break;
                AddOrUpdateNianLiBar(window);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[念力修行] UI异常: {e.Message}");
            }
        }

        private static void AddOrUpdateNianLiBar(UnitWindow window)
        {
            if (window?.actor == null) return;
            Actor actor = window.actor;

            float currentNianLi = actor.GetNianLi();
            float maxNianLi = actor.GetCurrentMaxNianLi();
            if (maxNianLi <= 0) maxNianLi = actor.GetRealmMaxNianLi();

            Transform background = window.transform.Find("Background");
            if (background == null) return;

            // ★ 全局递归查找红心血条（优先名称含 health）
            StatBar healthBar = FindHealthBarRecursive(background);
            if (healthBar == null)
            {
                Debug.LogWarning("[念力修行] 未找到生命值条，放弃创建念力条");
                return;
            }

            Transform healthParent = healthBar.transform.parent;
            if (healthParent == null)
            {
                Debug.LogWarning("[念力修行] 生命值条父容器为空，放弃创建");
                return;
            }

            // 念力为0或上限为0，隐藏已有念力条
            if (currentNianLi <= 0 || maxNianLi <= 0)
            {
                Transform existing = healthParent.Find(BAR_NAME);
                if (existing != null) existing.gameObject.SetActive(false);
                return;
            }

            // 确保父容器有 VerticalLayoutGroup（用于排列）
            VerticalLayoutGroup layout = healthParent.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = healthParent.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.spacing = 0; // 紧贴
            }

            // 查找是否已存在念力条（之前复制创建的）
            Transform existingBar = healthParent.Find(BAR_NAME);
            if (existingBar != null)
            {
                StatBar statBar = existingBar.GetComponent<StatBar>();
                if (statBar != null)
                {
                    UpdateStatBar(statBar, actor);
                    existingBar.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(healthParent as RectTransform);
                    return;
                }
            }

            // ★ 复制生命值条（完美继承所有显示属性）
            StatBar newBar = Object.Instantiate(healthBar, healthParent, false);
            newBar.name = BAR_NAME;

            // 复制 RectTransform 属性
            RectTransform templateRect = healthBar.GetComponent<RectTransform>();
            RectTransform newRect = newBar.GetComponent<RectTransform>();
            if (templateRect != null && newRect != null)
            {
                newRect.anchorMin = templateRect.anchorMin;
                newRect.anchorMax = templateRect.anchorMax;
                newRect.pivot = templateRect.pivot;
                newRect.sizeDelta = templateRect.sizeDelta;
                newRect.anchoredPosition = Vector2.zero;
            }

            // 修改颜色（填充条）
            if (newBar.bar != null)
            {
                Image barImage = newBar.bar.GetComponent<Image>();
                if (barImage != null) barImage.color = BAR_COLOR;
            }

            // 修改图标
            SetBarIcon(newBar);

            // 插入到血条之后
            int healthIndex = healthBar.transform.GetSiblingIndex();
            newBar.transform.SetSiblingIndex(healthIndex + 1);

            // 更新数值
            UpdateStatBar(newBar, actor);

            // 强制刷新
            LayoutRebuilder.ForceRebuildLayoutImmediate(healthParent as RectTransform);
        }

        // ─── 辅助方法 ───────────────────────────────────────────────

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
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        private static void SetBarIcon(StatBar bar)
        {
            Transform iconTransform = bar.transform.Find("Icon");
            if (iconTransform == null) return;
            Image iconImg = iconTransform.GetComponent<Image>();
            if (iconImg == null) return;
            iconTransform.gameObject.SetActive(true);
            Sprite sprite = SpriteTextureLoader.getSprite("ui/NianLi");
            if (sprite == null)
                sprite = Resources.Load<Sprite>("ui/icons/iconQuestionMark");
            if (sprite != null)
                iconImg.sprite = sprite;
        }

        private static void UpdateStatBar(StatBar bar, Actor actor)
        {
            float current = actor.GetNianLi();
            float max = actor.GetCurrentMaxNianLi();
            if (max <= 0) max = actor.GetRealmMaxNianLi();
            if (max <= 0)
            {
                bar.gameObject.SetActive(false);
                return;
            }
            bar.gameObject.SetActive(true);
            float percentage = Mathf.Clamp((current / max) * 100f, 0f, 100f);
            bar.setBar(percentage, 100f, "%", false, false, true, 0.25f);

            var tip = bar.GetComponent<TipButton>() ?? bar.gameObject.AddComponent<TipButton>();
            tip.textOnClick = LM.Get("nianli_nianli");
            tip.textOnClickDescription = $"{LM.Get("nianli_nianli")}: {current:F0} / {max:F0}";
        }
    }
}