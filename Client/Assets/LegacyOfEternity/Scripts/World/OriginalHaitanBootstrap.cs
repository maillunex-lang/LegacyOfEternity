using System.Collections.Generic;
using LegacyOfEternity.Combat;
using LegacyOfEternity.Offline;
using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>
    /// Original Haitan start on the already-imported cs_haitan scene graphics:
    /// MapData birth/NPC/gather/monsters + QuestConfig-driven interactables.
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
            map = OriginalMapData.LoadHaitan();
            if (hero == null)
                hero = GameObject.Find("LOE Prototype Hero")?.transform;
            if (hero == null)
            {
                Debug.LogError("LOE_ORIGINAL_HAITAN missing hero");
                return;
            }

            GameObject visualTemplate = CaptureEnemyVisualTemplate();
            PlaceHero(hero);
            ClearDemoArena();
            SpawnMonsters(hero, visualTemplate);
            SpawnNpcsAndCollections(hero);

            var hudHost = gameObject.GetComponent<OfflineInteractHud>();
            if (hudHost == null)
                hudHost = gameObject.AddComponent<OfflineInteractHud>();
            hudHost.Bind(interactables.ToArray());

            // Scene already contains original dixing/walksurface/props under cs_haitan.
            var slabs = GameObject.Find("LOE_Haitan_Walksurface");
            if (slabs != null)
                Destroy(slabs);

            Debug.Log(
                "LOE_ORIGINAL_HAITAN map=" + map.mapId +
                " monsters=" + map.monsters.Length +
                " npcs=" + map.npcs.Length +
                " collections=" + map.collections.Length +
                " quests=loaded birth=" + map.heroBirth.Position);
        }

        static GameObject CaptureEnemyVisualTemplate()
        {
            var anyEnemy = FindFirstObjectByType<CombatEnemy>();
            if (anyEnemy == null)
                return null;
            foreach (Transform child in anyEnemy.transform)
            {
                if (child.GetComponentInChildren<Renderer>() != null)
                    return child.gameObject;
            }
            return null;
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
            Debug.Log("LOE_ORIGINAL_HAITAN cleared demo PvE arena");
        }

        void SpawnMonsters(Transform hero, GameObject visualTemplate)
        {
            var root = new GameObject("LOE_OriginalMonsters");
            root.transform.SetParent(transform, false);

            int spawned = 0;
            foreach (var spawn in map.monsters)
            {
                if (spawn.modelId == 0)
                    continue;

                var go = new GameObject("Monster_" + spawn.id + "_" + spawn.spawnIndex);
                go.transform.SetParent(root.transform, false);
                go.transform.SetPositionAndRotation(spawn.Position, spawn.Rotation);

                if (visualTemplate != null)
                {
                    var visual = Instantiate(visualTemplate, go.transform, false);
                    visual.name = "Visual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    visual.SetActive(true);
                }
                else
                {
                    var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    body.name = "Body";
                    body.transform.SetParent(go.transform, false);
                    body.transform.localPosition = Vector3.up;
                    Object.Destroy(body.GetComponent<Collider>());
                }

                var capsule = go.AddComponent<CharacterController>();
                capsule.height = 1.6f;
                capsule.center = new Vector3(0f, 0.8f, 0f);
                capsule.radius = 0.4f;

                var enemy = go.AddComponent<CombatEnemy>();
                var anim = go.GetComponentInChildren<Animator>();
                enemy.Configure(hero, anim, "МОНСТР " + spawn.id, 65, 7, spawn.id);
                go.AddComponent<CombatEnemyHud>();
                spawned++;
            }

            Debug.Log("LOE_ORIGINAL_HAITAN monsters_spawned=" + spawned);
        }

        void SpawnNpcsAndCollections(Transform hero)
        {
            var root = new GameObject("LOE_OriginalInteractables");
            root.transform.SetParent(transform, false);

            foreach (var npc in map.npcs)
            {
                string name = npc.id == 201001001 ? "Тиас" : ("NPC " + npc.id);
                var color = npc.id == 201001001
                    ? new Color(0.22f, 0.48f, 0.92f, 1f)
                    : new Color(0.55f, 0.55f, 0.62f, 1f);
                interactables.Add(CreateInteractable(
                    root.transform, name, npc.Position, npc.Rotation,
                    OfflineInteractKind.Guide, hero, color, npc.id));
            }

            foreach (var col in map.collections)
            {
                string name = col.id == 180 ? "Прибрежный хлам" : ("Сбор " + col.id);
                var color = new Color(0.72f, 0.55f, 0.28f, 1f);
                interactables.Add(CreateInteractable(
                    root.transform, name, col.Position, col.Rotation,
                    OfflineInteractKind.GatherScrap, hero, color, col.id));
            }
        }

        static OfflineWorldInteractable CreateInteractable(
            Transform parent, string name, Vector3 position, Quaternion rotation,
            OfflineInteractKind kind, Transform hero, Color color, long objectId)
        {
            var go = new GameObject("Original_" + kind + "_" + objectId);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            var interact = go.AddComponent<OfflineWorldInteractable>();
            interact.Setup(kind, name, hero, color, objectId);
            return interact;
        }
    }
}
