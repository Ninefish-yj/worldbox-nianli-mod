using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using NeoModLoader.General;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Systems;

namespace NianLiXiuXing.code.UI
{
    [HarmonyPatch]
    public static class NianLiInfoPanel
    {
        #region 常量与静态字段

        private const string LEFT_PANEL_NAME = "NianLiInfoPanel_Left";
        private const string RIGHT_PANEL_NAME = "NianLiInfoPanel_Right";
        private const string INDENT = "  ";

        private static Text _leftText;
        private static Text _rightText;
        private static bool _initialized = false;
        private static Actor _currentActor;

        private static float _lastNianLi = -1f;
        private static int _lastRealm = -1;
        private static int _lastHeartMask = -1;
        private static bool _uiDirty = true;

        private static readonly string COLOR_GOLD = "#FFD700";
        private static readonly string COLOR_CYAN = "#00D4FF";
        private static readonly string COLOR_RED = "#FF6B6B";
        private static readonly string COLOR_ORANGE = "#FFA500";
        private static readonly string COLOR_GREEN = "#4CAF50";
        private static readonly string COLOR_PURPLE = "#9B59B6";
        private static readonly string COLOR_GRAY = "#888888";
        private static readonly string COLOR_WHITE = "#FFFFFF";

        #endregion

        #region 初始化与UI创建

        public static void Init() { }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UnitWindow), nameof(UnitWindow.OnEnable))]
        public static void UnitWindow_OnEnable_Postfix(UnitWindow __instance)
        {
            if (__instance?.actor == null || !__instance.actor.isAlive()) return;

            if (!_initialized)
            {
                _leftText = CreatePanel(__instance, LEFT_PANEL_NAME, new Vector3(-230, 0, 0));
                _rightText = CreatePanel(__instance, RIGHT_PANEL_NAME, new Vector3(250, 0, 0));
                _initialized = true;

                if (_leftText != null)
                {
                    var updater = _leftText.gameObject.GetComponent<NianLiPanelUpdater>();
                    if (updater == null)
                        updater = _leftText.gameObject.AddComponent<NianLiPanelUpdater>();
                    updater.Initialize(_leftText, _rightText);
                }
            }

            _currentActor = __instance.actor;
            _uiDirty = true;
            UpdateText(_currentActor);
        }

        private static Text CreatePanel(UnitWindow window, string panelName, Vector3 position)
        {
            var obj = new GameObject(panelName, typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter));
            obj.transform.SetParent(window.transform.Find("Background"));
            obj.transform.localPosition = position;
            obj.transform.localScale = Vector3.one;

            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(170, 0);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            var fitter = obj.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var text = obj.GetComponent<Text>();
            text.font = LocalizedTextManager.current_font;
            text.fontSize = 9;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;

            return text;
        }

        #endregion

        #region 外部刷新接口

        public static void MarkDirty() => _uiDirty = true;
        public static void ClearDirty() => _uiDirty = false;
        public static bool IsDirty() => _uiDirty;
        public static void Refresh(Actor actor) => UpdateText(actor);

        #endregion

        #region 核心更新逻辑（使用 new StringBuilder()，不使用池）

        public static void UpdateText(Actor actor)
        {
            if (_leftText == null || _rightText == null) return;
            if (actor == null) return;

            float currentNianLi = actor.GetNianLi();
            int currentRealm = actor.GetRealmLevel();
            int currentHeartMask = actor.GetHeartProgress();

            if (!_uiDirty &&
                Mathf.Approximately(currentNianLi, _lastNianLi) &&
                currentRealm == _lastRealm &&
                currentHeartMask == _lastHeartMask)
                return;

            _lastNianLi = currentNianLi;
            _lastRealm = currentRealm;
            _lastHeartMask = currentHeartMask;
            _uiDirty = false;

            // ★ 直接使用 new StringBuilder()，不使用池
            var left = new StringBuilder();
            var right = new StringBuilder();

            BuildLeftPanel(actor, left);
            BuildRightPanel(actor, right);

            _leftText.text = left.ToString();
            _rightText.text = right.ToString();

            // 无需 Return
        }

        // ---- 左栏构建 ----
        private static void BuildLeftPanel(Actor actor, StringBuilder sb)
        {
            int realm = actor.GetRealmLevel();
            string[] realmNames = {
                LM.Get("realm_mortal"),
                LM.Get("realm_nurture"),
                LM.Get("realm_reflect"),
                LM.Get("realm_great"),
                LM.Get("realm_ring"),
                LM.Get("realm_uphold")
            };
            string realmColor = realm >= 4 ? COLOR_GOLD : realm >= 2 ? COLOR_CYAN : COLOR_GRAY;

            sb.Append("<b><color=").Append(COLOR_GOLD).Append(">✦ 念力修行</color></b>  <color=")
              .Append(realmColor).Append("><b>").Append(realmNames[realm]).Append("</b></color>\n");

            float nianLi = actor.GetNianLi();
            float maxNianLi = actor.GetCurrentMaxNianLi();
            if (maxNianLi <= 0) maxNianLi = actor.GetRealmMaxNianLi();
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">念力</color> <color=")
              .Append(COLOR_GOLD).Append(">").Append(nianLi.ToString("F0")).Append("</color>  <color=")
              .Append(COLOR_GRAY).Append(">/ ").Append(maxNianLi.ToString("F0")).Append("</color>\n");

            float health = actor.data.health;
            float maxHealth = actor.stats["health"];
            if (maxHealth <= 0) maxHealth = 1f;
            string healthColor = health / maxHealth > 0.5f ? COLOR_GREEN : (health / maxHealth > 0.2f ? COLOR_ORANGE : COLOR_RED);
            string healthBar = BuildHealthBar(health, maxHealth, 14);
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">生命</color> ").Append(healthBar)
              .Append(" <color=").Append(healthColor).Append(">").Append(health.ToString("F0")).Append("</color>  <color=")
              .Append(COLOR_GRAY).Append(">/ ").Append(maxHealth.ToString("F0")).Append("</color>\n");

            if (realm < 5)
            {
                float threshold = Constants.RealmThresholds[realm + 1];
                float progress = Mathf.Clamp01(nianLi / threshold) * 100f;
                string bar = BuildProgressBar(progress, 14);
                string color = progress >= 100f ? COLOR_GREEN : COLOR_GOLD;
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">突破进度</color> ").Append(bar)
                  .Append(" <color=").Append(color).Append(">").Append(progress.ToString("F0")).Append("%</color>\n");
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">下一境</color> <color=")
                  .Append(COLOR_CYAN).Append(">").Append(realmNames[realm + 1]).Append("</color>  <color=")
                  .Append(COLOR_GRAY).Append(">需 ").Append(threshold.ToString("F0")).Append("</color>\n");
            }
            else sb.Append(INDENT).Append("<color=").Append(COLOR_GOLD).Append(">★ 已臻圆满</color>\n");
            sb.Append('\n');

            sb.Append("<color=").Append(COLOR_GOLD).Append(">──── 心关 ────</color>\n");
            sb.Append(INDENT).Append(GetHeartStatusLine(actor)).Append('\n');
            sb.Append('\n');

            sb.Append("<color=").Append(COLOR_GOLD).Append(">──── 修行速度 ────</color>\n");
            float totalSpeed = GrowthSystem.GetTotalCultivationSpeed(actor);
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">总速度</color> <color=")
              .Append(COLOR_GOLD).Append(">×").Append(totalSpeed.ToString("F1")).Append("</color>\n");

            float heartBonus = actor.stats["HeartBonus"];
            float talentBonus = GrowthSystem.GetTalentSpeedBonus(actor);
            float manaBonus = actor.GetManaSpeedBonus();
            float methodBonus = CultivationMethodSystem.GetTotalSpeedBonus(actor);
            float lawBonus = CycleLawSystem.HasTimeSpaceLaw(actor) ? 0.5f : 0f;
            float meditationMultiplier = MeditationSystem.GetMeditationMultiplier(actor);
            float stayingMultiplier = actor.IsStaying() ? Constants.STAYING_BONUS : 1f;

            if (heartBonus > 0) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">心境</color> <color=").Append(COLOR_GREEN).Append(">+").Append((heartBonus * 100).ToString("F0")).Append("%</color>\n");
            if (talentBonus > 0) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">天赋</color> <color=").Append(COLOR_PURPLE).Append(">+").Append((talentBonus * 100).ToString("F0")).Append("%</color>\n");
            if (methodBonus > 0) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">功法</color> <color=").Append(COLOR_ORANGE).Append(">+").Append((methodBonus * 100).ToString("F0")).Append("%</color>\n");
            if (manaBonus > 0) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">法力</color> <color=").Append(COLOR_CYAN).Append(">+").Append((manaBonus * 100).ToString("F0")).Append("%</color>\n");
            if (lawBonus > 0) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">时空法则</color> <color=").Append(COLOR_GOLD).Append(">+50%</color>\n");
            if (meditationMultiplier > 1f) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_PURPLE).Append(">冥想</color> <color=").Append(COLOR_WHITE).Append(">×").Append(meditationMultiplier.ToString("F1")).Append("</color>\n");
            if (stayingMultiplier > 1f) sb.Append(INDENT).Append(INDENT).Append("<color=").Append(COLOR_ORANGE).Append(">驻足</color> <color=").Append(COLOR_WHITE).Append(">×").Append(stayingMultiplier.ToString("F1")).Append("</color>\n");

            if (BrokenHeartSystem.IsBrokenHeart(actor) || actor.hasTrait(TraitIds.Trait_BrokenHeart))
                sb.Append(INDENT).Append("<color=").Append(COLOR_RED).Append(">💔 道心破碎 -50%</color>\n");
            sb.Append('\n');

            int insight = actor.GetInsightLevel();
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">顿悟</color> <color=").Append(COLOR_GOLD).Append(">");
            for (int i = 0; i < 5; i++) sb.Append(i < insight ? "★" : "☆");
            sb.Append("</color>  <color=").Append(COLOR_GRAY).Append(">(").Append(insight).Append("/5)</color>\n");
        }

        // ---- 右栏构建 ----
        private static void BuildRightPanel(Actor actor, StringBuilder sb)
        {
            int realm = actor.GetRealmLevel();

            sb.Append("<color=").Append(COLOR_CYAN).Append(">──── 天地大环 ────</color>\n");
            sb.Append(INDENT).Append(GetLawStatusLine(actor)).Append('\n');
            int lawCount = actor.GetLawCount();
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">进度</color> <color=").Append(COLOR_GOLD).Append(">").Append(lawCount).Append("/4</color>\n");
            sb.Append('\n');

            if (realm >= 4)
            {
                string smallWorld = GetSmallWorldTrait(actor);
                string swColor = smallWorld == LM.Get("nianli_small_world_complete") ? COLOR_GOLD : COLOR_RED;
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">小世界</color> <color=").Append(swColor).Append(">").Append(smallWorld).Append("</color>\n");
                sb.Append('\n');
            }

            sb.Append("<color=").Append(COLOR_ORANGE).Append(">──── 功法 ────</color>\n");
            var methods = CultivationMethodSystem.GetMethods(actor);
            if (methods.Count > 0)
            {
                var mainMethod = CultivationMethodSystem.GetMethod(actor);
                var data = CultivationMethodSystem.GetMethodData(mainMethod);
                sb.Append(INDENT).Append("<color=").Append(data.ColorHex).Append(">").Append(LocalizedTextManager.getText(data.NameKey)).Append("</color>\n");
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">修炼+").Append((data.SpeedBonus * 100).ToString("F0")).Append("%</color>\n");
                if (methods.Count > 1) sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">已学 ").Append(methods.Count).Append(" 种</color>\n");
            }
            else sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">未修炼</color>\n");
            sb.Append('\n');

            sb.Append("<color=").Append(COLOR_GOLD).Append(">──── 业力 ────</color>\n");
            float karma = KarmaSystem.GetKarma(actor);
            float virtue = KarmaSystem.GetVirtue(actor);
            string karmaColor = karma > 50f ? COLOR_RED : (karma > 20f ? COLOR_ORANGE : COLOR_GREEN);
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">业力</color> <color=").Append(karmaColor).Append(">").Append(karma.ToString("F0")).Append("/100</color>\n");
            sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">功德</color> <color=").Append(COLOR_GREEN).Append(">").Append(virtue.ToString("F0")).Append("/100</color>\n");
            float karmaPenalty = KarmaSystem.GetKarmaPenalty(actor) * 100f;
            if (karmaPenalty > 0) sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">业力惩罚</color> <color=").Append(COLOR_RED).Append(">-").Append(karmaPenalty.ToString("F0")).Append("%</color>\n");
            sb.Append('\n');

            sb.Append("<color=").Append(COLOR_ORANGE).Append(">──── 本命物 ────</color>\n");
            var artifact = RefiningSystem.GetArtifact(actor);
            if (artifact != null)
            {
                string rarityColor = GetRarityColor(artifact.Rarity);
                sb.Append(INDENT).Append("<color=").Append(rarityColor).Append(">").Append(artifact.Name).Append("</color>\n");
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">攻击+").Append((artifact.AttackBonus * 100).ToString("F0")).Append("% 防御+").Append((artifact.DefenseBonus * 100).ToString("F0")).Append("%</color>\n");
            }
            else sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">未炼制</color>\n");
            sb.Append('\n');

            sb.Append("<color=").Append(COLOR_ORANGE).Append(">──── 飞升 ────</color>\n");
            if (actor.IsAscensionReady())
            {
                int step = actor.GetAscensionStep();
                actor.data.get(DataKeys.LAST_ASCENSION_TYPE, out string typeStr, "Minor");
                string typeName = typeStr == "Major" ? LM.Get("nianli_major_ascension") : LM.Get("nianli_minor_ascension");
                int totalSteps = typeStr == "Major" ? Constants.MAJOR_ASCENSION_TOTAL_STEPS : Constants.MINOR_ASCENSION_TOTAL_STEPS;
                sb.Append(INDENT).Append("<color=").Append(COLOR_RED).Append(">⏳ ").Append(typeName).Append(" ").Append(step).Append("/").Append(totalSteps).Append("</color>\n");
            }
            else if (realm == 4 && !actor.IsMinorAscensionComplete())
            {
                string reason = actor.GetAscensionBlockReason();
                if (string.IsNullOrEmpty(reason)) sb.Append(INDENT).Append("<color=").Append(COLOR_CYAN).Append(">⚡ 可小阳劫</color>\n");
                else sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">⏸ ").Append(reason).Append("</color>\n");
            }
            else if (realm == 5 && actor.IsMinorAscensionComplete())
            {
                string reason = actor.GetMajorAscensionBlockReason();
                if (string.IsNullOrEmpty(reason)) sb.Append(INDENT).Append("<color=").Append(COLOR_GOLD).Append(">⚡ 可大阳劫</color>\n");
                else sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">⏸ ").Append(reason).Append("</color>\n");
            }
            else sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">⏸ 未达条件</color>\n");
            sb.Append('\n');

            int age = actor.getAge();
            float lifespan = actor.stats["lifespan"];
            if (lifespan >= float.MaxValue / 2) sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">寿命</color> <color=").Append(COLOR_GOLD).Append(">∞</color>\n");
            else
            {
                float ratio = age / lifespan;
                string color = ratio > 0.8f ? COLOR_RED : COLOR_GREEN;
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">寿命</color> <color=").Append(color).Append(">").Append(age).Append("/").Append(lifespan.ToString("F0")).Append("</color>  <color=").Append(COLOR_GRAY).Append(">(").Append((ratio * 100).ToString("F0")).Append("%)</color>\n");
            }

            int deathCount = actor.GetDeathCount();
            int resCount = actor.GetResurrectionCount();
            int reincCount = actor.GetReincarnationCount();
            if (deathCount > 0 || resCount > 0 || reincCount > 0)
                sb.Append(INDENT).Append("<color=").Append(COLOR_GRAY).Append(">死亡</color> ").Append(deathCount)
                  .Append("  <color=").Append(COLOR_GRAY).Append(">复活</color> ").Append(resCount)
                  .Append("  <color=").Append(COLOR_GRAY).Append(">转世</color> ").Append(reincCount).Append('\n');

            if (actor.hasTrait(TraitIds.WeiGe99)) sb.Append(INDENT).Append("<color=").Append(COLOR_GOLD).Append(">✦ 真超脱</color>\n");
            else if (actor.hasTrait(TraitIds.MinorTranscendence)) sb.Append(INDENT).Append("<color=").Append(COLOR_CYAN).Append(">✦ 准超脱</color>\n");
            if (actor.hasTrait(TraitIds.Trait_NoWayForward)) sb.Append(INDENT).Append("<color=").Append(COLOR_RED).Append(">⛔ 前路断绝</color>\n");
            if (actor.hasTrait(TraitIds.Trait_Reincarnated)) sb.Append(INDENT).Append("<color=").Append(COLOR_PURPLE).Append(">🔄 转世者</color>\n");
        }

        #endregion

        #region 辅助方法

        private static string BuildProgressBar(float percent, int length)
        {
            int filled = Mathf.Clamp(Mathf.RoundToInt(percent / 100f * length), 0, length);
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
                sb.Append(i < filled ? $"<color={COLOR_GOLD}>█</color>" : "░");
            return sb.ToString();
        }

        private static string BuildHealthBar(float current, float max, int length)
        {
            if (max <= 0) return new string('░', length);
            float percent = Mathf.Clamp01(current / max);
            int filled = Mathf.RoundToInt(percent * length);
            string color = percent > 0.5f ? COLOR_GREEN : (percent > 0.2f ? COLOR_ORANGE : COLOR_RED);
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
                sb.Append(i < filled ? $"<color={color}>█</color>" : "░");
            return sb.ToString();
        }

        private static string GetHeartStatusLine(Actor actor)
        {
            string[] names = { LM.Get("heart_mingxin"), LM.Get("heart_chengxin"), LM.Get("heart_dingxin"), LM.Get("heart_shengmie"), LM.Get("heart_wuwo"), LM.Get("heart_zhenwo") };
            var sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                bool broken = actor.IsHeartBroken(i);
                string color = broken ? COLOR_GREEN : COLOR_GRAY;
                string icon = broken ? "◆" : "◇";
                sb.Append("<color=").Append(color).Append(">").Append(icon).Append(names[i]).Append("</color> ");
                if ((i + 1) % 3 == 0 && i < 5) sb.Append("\n  ");
            }
            return sb.ToString().TrimEnd();
        }

        private static string GetLawStatusLine(Actor actor)
        {
            string[] lawKeys = { TraitIds.Law_Reincarnation, TraitIds.Law_YinYang, TraitIds.Law_TimeSpace, TraitIds.Law_CauseEffect };
            string[] lawNames = { LM.Get("nianli_law_reincarnation"), LM.Get("nianli_law_yinyang"), LM.Get("nianli_law_timespace"), LM.Get("nianli_law_causeeffect") };
            var sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                bool has = actor.hasTrait(lawKeys[i]);
                string color = has ? COLOR_GREEN : COLOR_GRAY;
                string icon = has ? "◆" : "◇";
                sb.Append("<color=").Append(color).Append(">").Append(icon).Append(lawNames[i]).Append("</color> ");
            }
            return sb.ToString().TrimEnd();
        }


        private static string GetSmallWorldTrait(Actor actor)
        {
            if (actor.hasTrait(TraitIds.SmallWorld_4)) return LM.Get("nianli_small_world_complete");
            if (actor.hasTrait(TraitIds.SmallWorld_3)) return LM.Get("nianli_small_world_merge");
            if (actor.hasTrait(TraitIds.SmallWorld_2)) return LM.Get("nianli_small_world_growth");
            if (actor.hasTrait(TraitIds.SmallWorld_1)) return LM.Get("nianli_small_world_birth");
            return "";
        }

        private static string GetRarityColor(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common: return "#888888";
                case ArtifactRarity.Uncommon: return "#4CAF50";
                case ArtifactRarity.Rare: return "#2196F3";
                case ArtifactRarity.Epic: return "#9C27B0";
                case ArtifactRarity.Legendary: return "#FFD700";
                default: return COLOR_WHITE;
            }
        }

        #endregion
    }

    // ---- 自动更新组件 ----
    internal class NianLiPanelUpdater : MonoBehaviour
    {
        private Text _leftText, _rightText;
        private Actor _lastActor;
        private float _lastNianLi;
        private int _lastRealm, _lastHeartMask;
        private float _updateTimer = 0f;
        private const float UPDATE_INTERVAL = 0.5f;

        public void Initialize(Text left, Text right) { _leftText = left; _rightText = right; }
        private void OnEnable() { NianLiInfoPanel.MarkDirty(); ForceUpdate(); }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer < UPDATE_INTERVAL) return;
            _updateTimer = 0f;

            Actor actor = GetCurrentActor();
            if (actor == null) { if (_lastActor != null) { _lastActor = null; ClearTexts(); } return; }

            float nianLi = actor.GetNianLi();
            int realm = actor.GetRealmLevel();
            int heartMask = actor.GetHeartProgress();

            if (actor != _lastActor || !Mathf.Approximately(nianLi, _lastNianLi) || realm != _lastRealm || heartMask != _lastHeartMask || NianLiInfoPanel.IsDirty())
            {
                _lastActor = actor; _lastNianLi = nianLi; _lastRealm = realm; _lastHeartMask = heartMask;
                NianLiInfoPanel.UpdateText(actor);
                NianLiInfoPanel.ClearDirty();
            }
        }

        private void ForceUpdate()
        {
            Actor actor = GetCurrentActor();
            if (actor != null) { _lastActor = actor; _lastNianLi = actor.GetNianLi(); _lastRealm = actor.GetRealmLevel(); _lastHeartMask = actor.GetHeartProgress(); NianLiInfoPanel.UpdateText(actor); NianLiInfoPanel.ClearDirty(); }
            else ClearTexts();
        }

        private Actor GetCurrentActor()
        {
            UnitWindow unitWindow = FindObjectOfType<UnitWindow>();
            if (unitWindow != null && unitWindow.actor != null && unitWindow.actor.isAlive()) return unitWindow.actor;
            if (SelectedUnit.isSet()) return SelectedUnit.unit;
            return null;
        }

        private void ClearTexts() { if (_leftText != null) _leftText.text = ""; if (_rightText != null) _rightText.text = ""; }
    }
}