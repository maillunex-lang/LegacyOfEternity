using System.Collections.Generic;
using LegacyOfEternity.Combat;
using LegacyOfEternity.Offline;
using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>
    /// Restores original Haitan start (map 110200011): birth, NPC/gather, all monster
    /// spawn points from MapData. Replaces the demo PvE-arena cluster next to the hero.
    /// </summary>
    public sealed class OriginalHaitanBootstrap : MonoBehaviour
    {
        const string RootName = "LOE_OriginalHaitan";
        const long GuideNpcId = 201001001;
        const long QuestGatherId = 180;

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
            map = OriginalMapData.LoadHaitan();
            if (hero == null)
                hero = GameObject.Find("LOE Prototype Hero")?.transform;
            if (hero == null)
            {
                Debug.LogError("LOE_ORIGINAL_HAITAN missing hero");
                return;
            }

            // Capture a visual template before clearing the demo arena.
            GameObject visualTemplate = CaptureEnemyVisualTemplate();

            EnsureWalkSurface();
            PlaceHero(hero);
            ClearDemoArena();
            SpawnMonsters(hero, visualTemplate);
            SpawnNpcsAndCollections(hero);

            var hudHost = gameObject.GetComponent<OfflineInteractHud>();
            if (hudHost == null)
                hudHost = gameObject.AddComponent<OfflineInteractHud>();
            hudHost.Bind(interactables.ToArray());

            Debug.Log(
                "LOE_ORIGINAL_HAITAN map=" + map.mapId +
                " monsters=" + map.monsters.Length +
                " npcs=" + map.npcs.Length +
                " collections=" + map.collections.Length +
                " birth=" + map.heroBirth.Position);
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

        static void EnsureWalkSurface()
        {
            if (GameObject.Find("LOE_Haitan_Walksurface") != null)
                return;

            // Invisible ground covering original map extents so birth/spawns are walkable
            // until full cs_yw_haitan01 mesh import is wired into the playable scene.
            var root = new GameObject("LOE_Haitan_Walksurface");
            CreateSlab(root.transform, new Vector3(105f, 49.9f, 115f), new Vector3(220f, 1f, 240f));
            CreateSlab(root.transform, new Vector3(90f, 54.3f, 80f), new Vector3(80f, 1f, 40f));
            CreateSlab(root.transform, new Vector3(110f, 58.0f, 130f), new Vector3(80f, 1f, 80f));
            CreateSlab(root.transform, new Vector3(110f, 62.0f, 165f), new Vector3(60f, 1f, 60f));
        }

        static void CreateSlab(Transform parent, Vector3 center, Vector3 size)
        {
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "WalkSlab";
            slab.transform.SetParent(parent, false);
            slab.transform.position = center;
            slab.transform.localScale = size;
            var renderer = slab.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
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
                enemy.Configure(hero, anim, "МОНСТР " + spawn.id, 65, 7);
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
                bool questGuide = npc.id == GuideNpcId;
                string name = questGuide ? "Тиас" : ("NPC " + npc.id);
                var kind = questGuide ? OfflineInteractKind.Guide : OfflineInteractKind.WorldProp;
                var color = questGuide
                    ? new Color(0.22f, 0.48f, 0.92f, 1f)
                    : new Color(0.55f, 0.55f, 0.62f, 1f);
                interactables.Add(CreateInteractable(
                    root.transform, name, npc.Position, npc.Rotation, kind, hero, color));
            }

            foreach (var col in map.collections)
            {
                bool questGather = col.id == QuestGatherId;
                string name = questGather ? "Прибрежный хлам" : ("Сбор " + col.id);
                var kind = questGather ? OfflineInteractKind.GatherScrap : OfflineInteractKind.WorldProp;
                var color = new Color(0.72f, 0.55f, 0.28f, 1f);
                interactables.Add(CreateInteractable(
                    root.transform, name, col.Position, col.Rotation, kind, hero, color));
            }
        }

        static OfflineWorldInteractable CreateInteractable(
            Transform parent, string name, Vector3 position, Quaternion rotation,
            OfflineInteractKind kind, Transform hero, Color color)
        {
            var go = new GameObject("Original_" + kind + "_" + name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            var interact = go.AddComponent<OfflineWorldInteractable>();
            interact.Setup(kind, name, hero, color);
            return interact;
        }
    }
}
