using System;
using System.Collections.Generic;
using LegacyOfEternity.Combat;
using LegacyOfEternity.World;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Offline quest dungeon (kind 14/17): local arena from Dungeon_questConfig monsters.
    /// Clears when all spawned enemies die, then ApplyQuestEvent("dungeon").
    /// </summary>
    public sealed class OfflineQuestDungeon : MonoBehaviour
    {
        [Serializable]
        class MonsterSpawn
        {
            public long templateId;
            public int count;
            public float x;
            public float z;
        }

        [Serializable]
        class DungeonDef
        {
            public int id;
            public long mapId;
            public int timeSec;
            public int deathLimit;
            public float doorX;
            public float doorZ;
            public MonsterSpawn[] monsters;
            public string title;
        }

        [Serializable]
        class File
        {
            public string source;
            public DungeonDef[] dungeons;
        }

        static OfflineQuestDungeon instance;
        static File data;
        static bool loaded;

        readonly List<CombatEnemy> alive = new List<CombatEnemy>();
        Transform hero;
        Vector3 returnPos;
        Quaternion returnRot;
        int activeDungeonId;
        bool running;
        GameObject arenaRoot;
        float timeLeft;
        GUIStyle boxStyle;
        GUIStyle labelStyle;
        GUIStyle buttonStyle;
        bool stylesReady;

        public static bool IsRunning => instance != null && instance.running;

        public static OfflineQuestDungeon Ensure()
        {
            if (instance != null)
                return instance;
            var host = GameObject.Find("LOE_OriginalHaitan");
            if (host == null)
            {
                host = new GameObject("LOE_QuestDungeon");
                DontDestroyOnLoad(host);
            }
            instance = host.GetComponent<OfflineQuestDungeon>();
            if (instance == null)
                instance = host.AddComponent<OfflineQuestDungeon>();
            return instance;
        }

        void Awake()
        {
            instance = this;
            EnsureCatalog();
        }

        static void EnsureCatalog()
        {
            if (loaded)
                return;
            var asset = Resources.Load<TextAsset>("OriginalMap/HaitanDungeons");
            if (asset == null)
            {
                data = new File { dungeons = Array.Empty<DungeonDef>() };
                loaded = true;
                Debug.LogWarning("LOE_DUNGEON missing HaitanDungeons.json");
                return;
            }
            data = JsonUtility.FromJson<File>(asset.text);
            data.dungeons ??= Array.Empty<DungeonDef>();
            loaded = true;
            Debug.Log("LOE_DUNGEON loaded=" + data.dungeons.Length);
        }

        public static bool TryEnter(int dungeonId)
        {
            return Ensure().Enter(dungeonId);
        }

        bool Enter(int dungeonId)
        {
            EnsureCatalog();
            if (running)
                return false;
            DungeonDef def = null;
            for (int i = 0; i < data.dungeons.Length; i++)
            {
                if (data.dungeons[i] != null && data.dungeons[i].id == dungeonId)
                {
                    def = data.dungeons[i];
                    break;
                }
            }
            if (def == null)
            {
                Debug.LogWarning("LOE_DUNGEON unknown id=" + dungeonId);
                return false;
            }

            hero = GameObject.Find("LOE Prototype Hero")?.transform;
            if (hero == null)
                return false;

            returnPos = hero.position;
            returnRot = hero.rotation;
            activeDungeonId = dungeonId;
            timeLeft = Mathf.Max(30f, def.timeSec);
            alive.Clear();

            Vector3 arenaOrigin = returnPos + hero.forward * 12f + Vector3.up * 0.2f;
            arenaRoot = new GameObject("LOE_QuestDungeon_" + dungeonId);
            arenaRoot.transform.position = arenaOrigin;

            // Floor marker
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "ArenaFloor";
            floor.transform.SetParent(arenaRoot.transform, false);
            floor.transform.localScale = new Vector3(14f, 0.15f, 14f);
            Object.Destroy(floor.GetComponent<Collider>());
            var shader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            if (shader != null)
                floor.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = new Color(0.18f, 0.2f, 0.28f, 1f) };

            var cc = hero.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            hero.SetPositionAndRotation(arenaOrigin + Vector3.forward * -4f + Vector3.up, Quaternion.identity);
            if (cc != null) cc.enabled = true;

            if (def.monsters != null)
            {
                for (int i = 0; i < def.monsters.Length; i++)
                {
                    var m = def.monsters[i];
                    if (m == null) continue;
                    int n = Mathf.Max(1, m.count);
                    for (int k = 0; k < n; k++)
                    {
                        Vector3 local = new Vector3((m.x % 17f) * 0.05f + k * 1.6f, 0f, 5f + (m.z % 13f) * 0.05f);
                        SpawnEnemy(def, m.templateId, arenaOrigin + local);
                    }
                }
            }

            CombatEnemy.Killed += OnEnemyKilled;
            running = true;
            Debug.Log("LOE_DUNGEON enter id=" + dungeonId + " enemies=" + alive.Count + " title=" + def.title);
            return true;
        }

        void SpawnEnemy(DungeonDef def, long templateId, Vector3 pos)
        {
            var go = new GameObject("DungeonMob_" + templateId);
            go.transform.SetParent(arenaRoot.transform, false);
            go.transform.position = pos;

            long modelId = LookModelFor(templateId);
            var visual = OriginalModelCatalog.AttachVisual(go.transform, modelId);
            if (visual == null)
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "BodyFallback";
                body.transform.SetParent(go.transform, false);
                body.transform.localPosition = Vector3.up;
                Object.Destroy(body.GetComponent<Collider>());
            }

            var capsule = go.AddComponent<CharacterController>();
            capsule.height = 1.8f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.radius = 0.45f;

            var stats = OriginalModelCatalog.MonsterStats(templateId);
            string title = stats != null ? stats.name : ("Босс " + templateId);
            int hp = stats != null ? Mathf.Max(200, stats.hp) : 2000;
            int dmg = stats != null ? Mathf.Max(5, stats.att) : 12;

            var enemy = go.AddComponent<CombatEnemy>();
            var anim = go.GetComponentInChildren<Animator>();
            enemy.Configure(hero, anim, title, hp, dmg, templateId);
            go.AddComponent<CombatEnemyHud>();
            alive.Add(enemy);
        }

        static long LookModelFor(long templateId)
        {
            // From t_monster look model ids for Haitan quest dungeon bosses.
            if (templateId == 100101201) return 880211004;
            if (templateId == 100101303) return 880211008;
            if (templateId == 100102203) return 880211004;
            if (templateId == 100103201) return 880211008;
            return 880210108;
        }

        void OnEnemyKilled(CombatEnemy enemy)
        {
            if (!running || enemy == null)
                return;
            alive.RemoveAll(e => e == null || e == enemy);
            if (alive.Count > 0)
                return;
            CompleteAndExit(true);
        }

        void Update()
        {
            if (!running)
                return;
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
                CompleteAndExit(false);
        }

        void CompleteAndExit(bool success)
        {
            if (!running)
                return;
            running = false;
            CombatEnemy.Killed -= OnEnemyKilled;

            if (arenaRoot != null)
                Destroy(arenaRoot);
            arenaRoot = null;
            alive.Clear();

            if (hero != null)
            {
                var cc = hero.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                hero.SetPositionAndRotation(returnPos, returnRot);
                if (cc != null) cc.enabled = true;
            }

            int dungeonId = activeDungeonId;
            activeDungeonId = 0;
            if (success && OfflineSession.Hero != null)
            {
                OfflineDemoStore.ApplyQuestEvent(OfflineSession.Hero, "dungeon", dungeonId);
                Debug.Log("LOE_DUNGEON cleared id=" + dungeonId);
            }
            else
                Debug.Log("LOE_DUNGEON failed/timeout id=" + dungeonId);
        }

        void OnDestroy()
        {
            if (running)
                CombatEnemy.Killed -= OnEnemyKilled;
            if (instance == this)
                instance = null;
        }

        void EnsureStyles()
        {
            if (stylesReady) return;
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            boxStyle = new GUIStyle(GUI.skin.box);
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(22f * scale),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(22f * scale),
                fixedHeight = 48f * scale
            };
            stylesReady = true;
        }

        void OnGUI()
        {
            if (!running)
                return;
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            var rect = new Rect(Screen.width * 0.5f - 220f * scale, 20f * scale, 440f * scale, 90f * scale);
            GUI.Box(rect, GUIContent.none, boxStyle);
            GUILayout.BeginArea(rect);
            GUILayout.Label("Подземелье · осталось " + Mathf.CeilToInt(timeLeft) + "с · врагов " + alive.Count, labelStyle);
            if (GUILayout.Button("Покинуть", buttonStyle))
                CompleteAndExit(false);
            GUILayout.EndArea();
        }
    }
}
