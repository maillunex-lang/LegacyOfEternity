using System.Collections.Generic;
using LegacyOfEternity.Combat;
using LegacyOfEternity.Offline;
using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>
    /// Original Haitan start: MapData spawns + QuestConfig + t_model/t_npc visuals.
    /// </summary>
    public sealed class OriginalHaitanBootstrap : MonoBehaviour
    {
        const string RootName = "LOE_OriginalHaitan";

        OriginalMapData map;
        readonly List<OfflineWorldInteractable> interactables = new List<OfflineWorldInteractable>();

        public static OriginalHaitanBootstrap Ensure(Transform hero)
        {
            var existing = FindFirstObjectByType<OriginalHaitanBootstrap>();
            if (existing != null)
                return existing;

            var go = new GameObject(RootName);
            DontDestroyOnLoad(go);
            var boot = go.AddComponent<OriginalHaitanBootstrap>();
            boot.Build(hero);
            return boot;
        }

        void Build(Transform hero)
        {
            OriginalQuestCatalog.EnsureLoaded();
            OriginalModelCatalog.EnsureLoaded();
            map = OriginalMapData.LoadHaitan();
            if (hero == null)
                hero = GameObject.Find("LOE Prototype Hero")?.transform;
            if (hero == null)
            {
                Debug.LogError("LOE_ORIGINAL_HAITAN missing hero");
                return;
            }

            PlaceHero(hero);
            ClearDemoArena();
            int monsters = SpawnMonsters(hero);
            SpawnNpcsAndCollections(hero);

            OriginalDialogueCatalog.EnsureLoaded();
            OfflineDialogueHud.Ensure();
            OfflineQuestDungeon.Ensure();

            var hudHost = gameObject.GetComponent<OfflineInteractHud>();
            if (hudHost == null)
                hudHost = gameObject.AddComponent<OfflineInteractHud>();
            hudHost.Bind(interactables.ToArray());
            if (gameObject.GetComponent<OfflineDialogueHud>() == null)
                gameObject.AddComponent<OfflineDialogueHud>();


            var slabs = GameObject.Find("LOE_Haitan_Walksurface");
            if (slabs != null)
                Destroy(slabs);

            Debug.Log(
                "LOE_ORIGINAL_HAITAN map=" + map.mapId +
                " monsters_spawned=" + monsters +
                " npcs=" + map.npcs.Length +
                " collections=" + map.collections.Length +
                " birth=" + map.heroBirth.Position);
        }

        void PlaceHero(Transform hero)
        {
            var birth = map.heroBirth.Position;
            var cc = hero.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
            hero.SetPositionAndRotation(birth, map.heroBirth.Rotation);
            if (cc != null)
                cc.enabled = true;

            var cam = GameObject.Find("LOE Follow Camera");
            if (cam != null)
            {
                Vector3 offset = Quaternion.Euler(0f, 50f, 0f) * new Vector3(0f, 10f, -5f);
                cam.transform.position = birth + offset;
                cam.transform.LookAt(birth + Vector3.up * 1.2f);
            }
        }

        static void ClearDemoArena()
        {
            var arena = GameObject.Find("LOE PvE Arena");
            if (arena == null)
                return;
            foreach (var enemy in arena.GetComponentsInChildren<CombatEnemy>(true))
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
            arena.SetActive(false);
        }

        int SpawnMonsters(Transform hero)
        {
            var root = new GameObject("LOE_OriginalMonsters");
            root.transform.SetParent(transform, false);
            int spawned = 0;
            int withVisual = 0;

            foreach (var spawn in map.monsters)
            {
                if (spawn.modelId == 0)
                    continue;

                var go = new GameObject("Monster_" + spawn.id + "_" + spawn.spawnIndex);
                go.transform.SetParent(root.transform, false);
                go.transform.SetPositionAndRotation(spawn.Position, spawn.Rotation);

                var visual = OriginalModelCatalog.AttachVisual(go.transform, spawn.modelId);
                if (visual != null)
                    withVisual++;
                else
                    CreateCapsuleFallback(go.transform, new Color(0.55f, 0.2f, 0.2f, 1f));

                var capsule = go.AddComponent<CharacterController>();
                capsule.height = 1.6f;
                capsule.center = new Vector3(0f, 0.8f, 0f);
                capsule.radius = 0.4f;

                var stats = OriginalModelCatalog.MonsterStats(spawn.id);
                string title = stats != null ? stats.name : ("МОНСТР " + spawn.id);
                int hp = stats != null ? Mathf.Max(1, stats.hp) : 65;
                int dmg = stats != null ? Mathf.Max(1, stats.att) : 7;

                var enemy = go.AddComponent<CombatEnemy>();
                var anim = go.GetComponentInChildren<Animator>();
                enemy.Configure(hero, anim, title, hp, dmg, spawn.id);
                go.AddComponent<CombatEnemyHud>();
                spawned++;
            }

            Debug.Log("LOE_ORIGINAL_HAITAN monsters=" + spawned + " with_model=" + withVisual);
            return spawned;
        }

        void SpawnNpcsAndCollections(Transform hero)
        {
            var root = new GameObject("LOE_OriginalInteractables");
            root.transform.SetParent(transform, false);
            int npcVisual = 0;

            foreach (var npc in map.npcs)
            {
                string name = OriginalModelCatalog.NpcName(npc.id);
                long look = OriginalModelCatalog.NpcLookModelId(npc.id);
                if (look == 0)
                    look = npc.modelId;

                var color = npc.id == 201001001
                    ? new Color(0.22f, 0.48f, 0.92f, 1f)
                    : new Color(0.55f, 0.55f, 0.62f, 1f);

                var interact = CreateInteractable(
                    root.transform, name, npc.Position, npc.Rotation,
                    OfflineInteractKind.Guide, hero, color, npc.id, look, ref npcVisual);
                interactables.Add(interact);
            }

            foreach (var col in map.collections)
            {
                string name = OriginalModelCatalog.CollectionName(col.id);
                var color = new Color(0.72f, 0.55f, 0.28f, 1f);
                int ignore = 0;
                var interact = CreateInteractable(
                    root.transform, name, col.Position, col.Rotation,
                    OfflineInteractKind.GatherScrap, hero, color, col.id, col.modelId, ref ignore);
                interactables.Add(interact);
            }

            Debug.Log("LOE_ORIGINAL_HAITAN npc_visuals=" + npcVisual);
        }

        static OfflineWorldInteractable CreateInteractable(
            Transform parent, string name, Vector3 position, Quaternion rotation,
            OfflineInteractKind kind, Transform hero, Color color, long objectId, long modelId,
            ref int visualCount)
        {
            var go = new GameObject("Original_" + kind + "_" + objectId);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);

            var visual = OriginalModelCatalog.AttachVisual(go.transform, modelId);
            if (visual != null)
                visualCount++;
            // Always keep marker/label from Setup; hide capsule body if real visual exists.
            var interact = go.AddComponent<OfflineWorldInteractable>();
            interact.Setup(kind, name, hero, color, objectId);
            if (visual != null)
                interact.HidePrimitiveBody();
            return interact;
        }

        static void CreateCapsuleFallback(Transform parent, Color color)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "BodyFallback";
            body.transform.SetParent(parent, false);
            body.transform.localPosition = Vector3.up;
            Object.Destroy(body.GetComponent<Collider>());
            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (shader != null)
                body.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };
        }
    }
}
