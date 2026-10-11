using System.IO;
using UnityEngine;
using FMODUnity;
using NianLiXiuXing.code.Core;
using NianLiXiuXing.code.Utils;
using NeoModLoader.General;

namespace NianLiXiuXing.code.Systems
{
    public static class WorldMessageSystem
    {
        private static void PlayAscensionMusic()
        {
            SafeExecutor.Execute(() =>
            {
                string soundPath = Path.Combine(Application.streamingAssetsPath, "sounds/Sound_1.wav");
                if (File.Exists(soundPath))
                {
                    RuntimeManager.PlayOneShot(soundPath);
                }
                else if (ConfigCache.EnableDebugLog)
                {
                    Debug.LogWarning($"[念力修行] 音乐文件不存在: {soundPath}");
                }
            }, "播放飞升音乐", false);
        }

        public static void Broadcast(string message)
        {
            WorldTip.showNow(message, true, "top");
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {message}");
        }

        public static void BroadcastWithHistory(string message)
        {
            WorldTip.showNow(message, true, "top");
            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {message}");
        }

        public static void BroadcastHistoryOnly(string logKey, string message, Actor actor, Color color)
        {
            WitchcraftWorldLog.AddLog(logKey, message, actor, color, false);
        }

        public static void BroadcastBreakthrough(Actor actor, int realmLevel)
        {
            if (realmLevel < 4) return;

            string message;
            string logKey = $"breakthrough_realm_{realmLevel}";

            if (realmLevel == 4)
            {
                message = $"🌌 {actor.data.name} 突破至环界境！\n" +
                          "「以天地大环主干法则，搭建自身天地大环。」\n" +
                          "从此正式走向创造世界之路，\n" +
                          "一念一世界，一息一宇宙。";
            }
            else
            {
                message = $"☀️☀️☀️ {actor.data.name} 突破至托举境！ ☀️☀️☀️\n" +
                          "「托举内环天地，飞升虚无之巅，\n" +
                          " 自身即宇宙，宇宙即自身。」\n" +
                          "新世界历由此开启，\n" +
                          "万界俯首，时空臣服。";
            }

            string[] realmNames = { "", LM.Get("realm_nurture"), LM.Get("realm_reflect"), LM.Get("realm_great"), LM.Get("realm_ring"), LM.Get("realm_uphold") };
            if (realmLevel >= 3)
            {
                DaoBeiSystem.AddRecord(actor, "境界突破", $"突破至 {realmNames[realmLevel]}", new Color(0.3f, 0.6f, 1f));
            }

            Color color = new Color(0.3f, 0.6f, 1f);
            WitchcraftWorldLog.AddLog(logKey, message, actor, color);
        }

        public static void BroadcastHeartBreak(Actor actor, int heartIndex)
        {
            if (heartIndex < 5) return;

            PlayAscensionMusic();

            string message;
            string logKey = $"heart_break_{heartIndex}";

            message = $"✦✦✦ {actor.data.name} 破尽万法，证得真我！ ✦✦✦\n" +
                      "「我即唯一，唯一即我。\n" +
                      " 破尽我执、物我两忘，见独无古今，不死不生。」\n" +
                      "自此抵达心灵的终极圆满，永恒自在，\n" +
                      "天地万物，皆为其念力所化。";

            string[] heartNames = { LM.Get("heart_mingxin"), LM.Get("heart_chengxin"), LM.Get("heart_dingxin"), 
                                    LM.Get("heart_shengmie"), LM.Get("heart_wuwo"), LM.Get("heart_zhenwo") };
            DaoBeiSystem.AddRecord(actor, "心关突破", $"突破 {heartNames[heartIndex]}", new Color(1f, 0.84f, 0f));

            Color color = new Color(1f, 0.84f, 0f);
            WitchcraftWorldLog.AddLog(logKey, message, actor, color);
        }

        public static void BroadcastMinorAscension(Actor actor)
        {
            if (actor == null) return;

            string message = $"🌊🌊🌊 {actor.data.name} 渡过小阳劫！ 🌊🌊🌊\n" +
                              "「五重阳劫尽，一念通天地。\n" +
                              " 虽未登极境，已窥大道门。」\n" +
                              "准超脱之身，天地认可，\n" +
                              "只待大阳劫临，便可破茧成蝶。";

            DaoBeiSystem.AddRecord(actor, LM.Get("nianli_minor_ascension"), "渡过小阳劫，获得准超脱之身", new Color(0.3f, 0.6f, 1f));

            Color color = new Color(0.3f, 0.6f, 1f);
            WitchcraftWorldLog.AddLog("minor_ascension_success", message, actor, color);
        }

        public static void BroadcastAscension(Actor actor, bool success)
        {
            string message;
            string logKey = success ? "ascension_success" : "ascension_fail";

            if (success)
            {
                message = $"🌟✨🌟 {actor.data.name} 超脱飞升！ 🌟✨🌟\n" +
                          "「托举内环天地，成就超脱之位，\n" +
                          " 从此跳出三界外，不在五行中。」\n" +
                          "万界震动，诸天共庆，\n" +
                          "大道之巅，又多一位永恒行者。";
                AscensionRecordSystem.RemoveRecord(actor);
                AscensionRecordSystem.AddRecord(actor, true);
                DaoBeiSystem.AddRecord(actor, "飞升超脱", "成就超脱，跳出三界外，不在五行中", new Color(1f, 0.84f, 0f));
            }
            else
            {
                message = $"💀 {actor.data.name} 飞升失败，陨落于阳劫！\n" +
                          "[倒在求道的道路上，他无疑是幸运的，\n" +
                          "  但他也是不幸的——\n" +
                          "  只差一步，便是永恒。]";

                DaoBeiSystem.AddRecord(actor, "飞升失败", "陨落于阳劫", new Color(1f, 0.2f, 0.2f));
                
                actor.data.get(DataKeys.ASCENSION_FAIL_COUNT, out int failCount, 0);
                actor.data.set(DataKeys.ASCENSION_FAIL_COUNT, failCount + 1);
                
                Color failColor = new Color(1f, 0.2f, 0.2f);
                WitchcraftWorldLog.AddLog("ascension_fail", message, actor, failColor);
                
                if (failCount >= 3)
                {
                    WorldTip.showNow($"⚠️ {actor.data.name} 已连续失败 {failCount} 次，道基受损！", true, "top");
                }
                return;
            }

            Color color = success ? new Color(1f, 0.84f, 0f) : new Color(1f, 0.2f, 0.2f);
            WitchcraftWorldLog.AddLog(logKey, message, actor, color);
        }

        public static void BroadcastPioneer(Actor actor, int realmLevel)
        {
            string[] realmNames = { "", LM.Get("realm_nurture"), LM.Get("realm_reflect"), LM.Get("realm_great"), LM.Get("realm_ring"), LM.Get("realm_uphold") };
            if (realmLevel < 1 || realmLevel > 5) return;

            string logKey = $"pioneer_realm_{realmLevel}";
            string name = actor?.getName() ?? "未知修士";
            string message = GetPioneerMessage(name, realmLevel);

            Color color = new Color(1f, 0.84f, 0f);
            WitchcraftWorldLog.AddLog(logKey, message, actor, color);

            if (realmLevel >= 4)
            {
                DaoBeiSystem.AddRecord(actor, "开拓者", $"成为 {realmNames[realmLevel]} 的开拓者", new Color(1f, 0.84f, 0f));
            }
        }

        private static string GetPioneerMessage(string name, int realmLevel)
        {
            if (string.IsNullOrEmpty(name)) name = "未知修士";

            switch (realmLevel)
            {
                case 1: return $"🌟 {name} 于混沌中点燃第一缕念火，成为了道路的开拓者";
                case 2: return $"🌟 {name} 在无人踏足的荒原上刻下印记，成为了道路的开拓者";
                case 3: return $"🌟 {name} 叩开了天地玄门，成为了道路的开拓者";
                case 4: return $"🌟 {name} 以法则为墨，画下属于自己第一道天地大环，成为了道路的开拓者";
                case 5: return $"🌟 {name} 第一个走上了超脱之路，成为了道路的开拓者";
                default: return $"🌟 {name} 成为了道路的开拓者";
            }
        }

        public static void BroadcastKill(Actor victim, Actor killer)
        {
            if (victim == null || killer == null) return;

            int realm = victim.GetRealmLevel();
            if (realm < 4) return;

            string logKey = $"kill_realm_{realm}";
            string message;

            if (realm == 4)
            {
                message = $"💀 环界强者 {victim.data.name} 被 {killer.data.name} 击杀！";
            }
            else
            {
                message = $"💀 超越者 {victim.data.name} 陨落于 {killer.data.name} 之手！\n【天地同悲！】";
            }

            Color color = new Color(1f, 0.2f, 0.2f);
            WitchcraftWorldLog.AddLog(logKey, message, victim, color);
        }

        // ★★★ 环界境失败静默，托举境失败正常显示 ★★★
        public static void BroadcastBreakthroughFail(Actor actor, int realmLevel, bool isDeath)
        {
            string[] realmNames = { "", LM.Get("realm_nurture"), LM.Get("realm_reflect"), LM.Get("realm_great"), LM.Get("realm_ring"), LM.Get("realm_uphold") };
            if (realmLevel < 1 || realmLevel > 5) return;
            if (realmLevel < 3) return;

            if (realmLevel == 4)
            {
                if (ConfigCache.EnableDebugLog)
                    Debug.Log($"[念力修行] {actor.data.name} 突破环界境失败（静默）");
                return;
            }

            if (realmLevel == 5)
            {
                string logKey = isDeath ? "breakthrough_fail_death_5" : "breakthrough_fail_noway_5";
                string message = isDeath
                    ? $"💀 {actor.data.name} 突破 {realmNames[realmLevel]} 失败，道消身陨！"
                    : $"⛔ {actor.data.name} 突破 {realmNames[realmLevel]} 失败，前路断绝！";
                Color color = isDeath ? new Color(1f, 0.2f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
                WorldTip.showNow(message, true, "top");
                WitchcraftWorldLog.AddLog(logKey, message, actor, color);
                return;
            }

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] {actor.data.name} 突破失败（未处理）");
        }
    }
}