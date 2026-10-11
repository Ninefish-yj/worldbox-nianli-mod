using System.Collections.Generic;
using UnityEngine;
using NianLiXiuXing.code.Core;

namespace NianLiXiuXing.code.Systems
{
    public static class ReincarnationHelper
    {
        // ★ 新增：为飞升记录生成载体（不依赖源角色）
        public static Actor CreateReincarnationBody(string actorName = null)
        {
            Actor baby = FindFamilyBaby();
            if (baby == null)
            {
                baby = CreateRandomCreatureBaby(null);
            }

            if (baby == null) return null;

            if (!string.IsNullOrEmpty(actorName))
            {
                baby.setName($"{actorName}·归位", true);
            }

            if (!baby.hasTrait(TraitIds.Trait_Reincarnated))
                baby.addTrait(TraitIds.Trait_Reincarnated);

            if (!baby.hasTrait(TraitIds.WeiGe99))
                baby.addTrait(TraitIds.WeiGe99);

            return baby;
        }

        // ★ 兼容旧调用：TryReincarnate
        public static bool TryReincarnate(Actor actor)
        {
            if (actor == null) return false;
            if (!actor.isAlive()) return false;
            
            if (!CycleLawSystem.HasReincarnationLaw(actor)) return false;
            
            int reincarnationCount = actor.GetReincarnationCount();
            if (reincarnationCount >= 5) return false;

            Actor newBody = FindReincarnationTarget(actor);
            if (newBody == null) return false;

            TransferReincarnationData(actor, newBody);
            
            actor.AddReincarnationCount();
            newBody.AddReincarnationCount();
            
            if (!newBody.hasTrait(TraitIds.Trait_Reincarnated))
                newBody.addTrait(TraitIds.Trait_Reincarnated);
            
            string message = $"🔄 {actor.data.name} 启动轮回法则，转世为 {newBody.data.name}！";
            WorldMessageSystem.Broadcast(message);
            WitchcraftWorldLog.AddLog("reincarnation", message, newBody, new Color(0.5f, 0.3f, 1f));
            
            if (newBody.current_tile != null)
            {
                BaseEffect effect = EffectsLibrary.spawnAtTile("fx_spawn", newBody.current_tile, 3f);
                if (effect != null)
                {
                    effect.sprite_renderer.color = new Color(0.5f, 0.3f, 1f);
                    Object.Destroy(effect.gameObject, 3f);
                }
            }
            
            return true;
        }

        // 原有的方法
        public static Actor FindReincarnationTarget(Actor source = null)
        {
            Actor target = FindFamilyBaby();
            if (target != null) return target;
            target = CreateRandomCreatureBaby(source);
            return target;
        }

        public static Actor FindFamilyBaby()
        {
            if (World.world.families == null || World.world.families.Count == 0)
                return null;

            Family family = null;
            int attempts = 0;
            const int maxAttempts = 10;

            while (attempts < maxAttempts)
            {
                family = World.world.families.GetRandom();
                if (family != null && family.units != null && family.units.Count >= 2)
                    break;
                family = null;
                attempts++;
            }

            if (family == null) return null;

            Actor parent1 = null;
            Actor parent2 = null;

            foreach (Actor member in family.units)
            {
                if (member?.data == null || !member.isAlive()) continue;
                if (!member.isAdult()) continue;
                if (member.asset.id == "ghost") continue;

                if (parent1 == null)
                {
                    parent1 = member;
                }
                else if (parent2 == null && member.data.sex != parent1.data.sex)
                {
                    parent2 = member;
                    break;
                }
            }

            if (parent1 == null) return null;

            return BabyMaker.makeBaby(
                parent1,
                parent2,
                ActorSex.None,
                false,
                0,
                null,
                true,
                true
            );
        }

        public static Actor CreateRandomCreatureBaby(Actor source = null)
        {
            List<string> validCreatureTypes = GetValidCreatureTypes();
            if (validCreatureTypes.Count == 0) return null;

            string creatureType = validCreatureTypes[Random.Range(0, validCreatureTypes.Count)];

            WorldTile spawnTile = GetRandomSpawnTile();
            if (spawnTile == null) return null;

            Actor baby = World.world.units.createNewUnit(
                creatureType,
                spawnTile,
                false,
                0f,
                null,
                null,
                true,
                false,
                false,
                false
            );

            if (baby != null)
            {
                baby.data.age_overgrowth = 0;
            }

            return baby;
        }

        public static List<string> GetValidCreatureTypes()
        {
            List<string> types = new List<string>();

            foreach (ActorAsset asset in AssetManager.actor_library.list)
            {
                if (asset.canBecomeSapient() && !asset.unit_other)
                    types.Add(asset.id);
            }

            if (types.Count == 0)
            {
                foreach (ActorAsset asset in AssetManager.actor_library.list)
                {
                    if (asset.default_animal && !asset.unit_other)
                        types.Add(asset.id);
                }
            }

            return types;
        }

        public static WorldTile GetRandomSpawnTile()
        {
            for (int i = 0; i < 30; i++)
            {
                int x = Random.Range(10, MapBox.width - 10);
                int y = Random.Range(10, MapBox.height - 10);

                WorldTile tile = World.world.GetTileSimple(x, y);
                if (tile != null && tile.Type.ground && !tile.Type.block)
                    return tile;
            }
            return null;
        }

        private static void TransferReincarnationData(Actor oldActor, Actor newActor)
        {
            if (oldActor == null || newActor == null) return;

            float savedNianLi = oldActor.GetNianLi() * 0.5f;
            float maxNianLi = oldActor.GetCurrentMaxNianLi() * 0.3f;
            newActor.ChangeNianLi(Mathf.Min(savedNianLi, 5000f));
            newActor.SetCurrentMaxNianLi(Mathf.Min(maxNianLi, 10000f));

            int heartProgress = oldActor.GetHeartProgress();
            int preservedHearts = heartProgress & 0b000111;
            for (int i = 3; i <= 5; i++)
            {
                if (oldActor.IsHeartBroken(i) && Random.Range(0f, 1f) < 0.5f)
                {
                    preservedHearts |= (1 << i);
                }
            }
            newActor.SetHeartProgress(preservedHearts);
            
            for (int i = 0; i < 6; i++)
            {
                if (newActor.IsHeartBroken(i) && !newActor.hasTrait($"Heart_{i}"))
                    newActor.addTrait($"Heart_{i}");
            }

            int insight = oldActor.GetInsightLevel();
            newActor.SetInsightLevel(Mathf.Max(0, insight / 2));

            string[] laws = { 
                TraitIds.Law_Reincarnation, 
                TraitIds.Law_YinYang, 
                TraitIds.Law_TimeSpace, 
                TraitIds.Law_CauseEffect 
            };
            foreach (string law in laws)
            {
                if (oldActor.hasTrait(law) && !newActor.hasTrait(law))
                    newActor.addTrait(law);
            }

            int lawProgress = oldActor.GetCycleLawProgress();
            newActor.SetCycleLawProgress(lawProgress);

            string oldName = oldActor.getName();
            newActor.setName($"{oldName}·转世", true);

            if (!newActor.hasTrait(TraitIds.Trait_Reincarnated))
                newActor.addTrait(TraitIds.Trait_Reincarnated);

            DaoBeiSystem.AddRecord(
                newActor, 
                "轮回转世", 
                $"前身为 {oldName}，携带 {savedNianLi:F0} 念力转世",
                new Color(0.5f, 0.3f, 1f)
            );

            if (ConfigCache.EnableDebugLog)
                Debug.Log($"[念力修行] 转世数据转移完成: {oldName} → {newActor.getName()}");
        }
    }
}