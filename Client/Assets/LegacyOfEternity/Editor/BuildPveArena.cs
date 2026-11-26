using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Player;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Editor
{
    public static class BuildPveArena
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const string Root = "Assets/LegacyOfEternity/";
        private const string Source = "Assets/LegacyAssets/AssetsRaw/Animation/monster/";
        private static readonly string[] Monsters = { "monster_001", "monster_002", "monster_003" };
        private static readonly string[] Names = { "ЛЕСНОЙ ЗВЕРЬ", "ПЕЩЕРНЫЙ СТРАЖ", "ДИКИЙ ХИЩНИК" };

        private static AnimationClip GetClip(string name, string stem, bool loop)
        {
            string path = Source + name + "/" + name + "@" + stem + ".FBX";
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new Exception("Missing monster clip: " + path);
            bool changed = false;
            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic; changed = true;
            }
            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0) throw new Exception("No monster animations: " + path);
            foreach (var clip in clips)
                if (clip.loopTime != loop || clip.loopPose != loop)
                { clip.loopTime = loop; clip.loopPose = loop; changed = true; }
            if (changed) { importer.clipAnimations = clips; importer.SaveAndReimport(); }
            var loaded = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
            if (loaded == null) throw new Exception("No usable clip: " + path);
            if (loaded.isLooping != loop) throw new Exception("Wrong animation loop state: " + path);
            return loaded;
        }

        private static AnimatorController MakeController(string id)
        {
            string path = Root + id + "PvE.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            if (controller.layers == null || controller.layers.Length == 0) controller.AddLayer("Base Layer");
            var sm = controller.layers[0].stateMachine;
            AnimatorState State(string name, AnimationClip clip)
            {
                var s = sm.states.Select(e => e.state).FirstOrDefault(e => e.name == name) ?? sm.AddState(name);
                s.motion = clip;
                s.writeDefaultValues = true;
                return s;
            }

            var idle = State("Idle", GetClip(id, "stand", true));
            var run = State("Run", GetClip(id, "run", true));
            var attack = State("Attack", GetClip(id, "attack001", false));
            var hit = State("Hit", GetClip(id, "hit014", false));
            var death = State("Death", GetClip(id, "die001", false));
            sm.defaultState = idle;
            void Float(string n) { if (!controller.parameters.Any(p => p.name == n)) controller.AddParameter(n, AnimatorControllerParameterType.Float); }
            void Bool(string n) { if (!controller.parameters.Any(p => p.name == n)) controller.AddParameter(n, AnimatorControllerParameterType.Bool); }
            void Trigger(string n) { if (!controller.parameters.Any(p => p.name == n)) controller.AddParameter(n, AnimatorControllerParameterType.Trigger); }
            Float("Speed"); Bool("Dead"); Trigger("Attack"); Trigger("Hit"); Trigger("Reset");
            void Move(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
            {
                if (from.transitions.Any(t => t.destinationState == to)) return;
                var t = from.AddTransition(to);
                t.hasExitTime = false; t.duration = .15f;
                t.AddCondition(mode, .15f, "Speed");
            }
            Move(idle, run, AnimatorConditionMode.Greater);
            Move(run, idle, AnimatorConditionMode.Less);
            void Action(AnimatorState state, string trigger)
            {
                if (!sm.anyStateTransitions.Any(t => t.destinationState == state))
                {
                    var transition = sm.AddAnyStateTransition(state);
                    transition.hasExitTime = false; transition.duration = .08f; transition.canTransitionToSelf = false;
                    transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
                    transition.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
                }
                if (!state.transitions.Any(t => t.destinationState == idle))
                {
                    var back = state.AddTransition(idle);
                    back.hasExitTime = true; back.exitTime = .9f; back.duration = .12f;
                    back.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
                }
            }
            Action(attack, "Attack");
            Action(hit, "Hit");
            if (!sm.anyStateTransitions.Any(t => t.destinationState == death))
            {
                var toDeath = sm.AddAnyStateTransition(death);
                toDeath.hasExitTime = false; toDeath.duration = .08f; toDeath.canTransitionToSelf = false;
                toDeath.AddCondition(AnimatorConditionMode.If, 0, "Dead");
            }
            if (!death.transitions.Any(t => t.destinationState == idle))
            {
                var reset = death.AddTransition(idle);
                reset.hasExitTime = false; reset.duration = .1f;
                reset.AddCondition(AnimatorConditionMode.If, 0, "Reset");
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static Vector3 FindSpawn(Vector3 hero, IReadOnlyList<Vector3> existing, int index)
        {
            var baseRadius = new [] { 3f, 4.2f, 5.4f };
            for (int ring = 0; ring < 7; ring++)
            {
                float radius = baseRadius[index] + ring * .9f;
                for (int step = 0; step < 24; step++)
                {
                    float theta = (step + index * 7) * Mathf.PI / 12f;
                    var point = hero + new Vector3(Mathf.Cos(theta) * radius, 15f, Mathf.Sin(theta) * radius);
                    var hits = Physics.RaycastAll(point, Vector3.down, 35f);
                    foreach (var hit in hits.OrderByDescending(h => h.point.y))
                    {
                        if (hit.normal.y < .72f) continue;
                        if (hit.collider.GetComponentInParent<PrototypeCombat>() != null || hit.collider.GetComponentInParent<CombatEnemy>() != null) continue;
                        if (Mathf.Abs(hit.point.y - (hero.y - 1f)) > 2.6f) continue;
                        Vector3 pos = hit.point + Vector3.up * .05f;
                        if (existing.All(p => Vector3.Distance(p, pos) > 2.2f))
                            return pos;
                    }
                }
            }
            throw new Exception("No safe nearby ground for monster " + index + "; scene spawn must be adjusted.");
        }

        private static CombatEnemy MakeMonster(string id, string title, int number, Transform parent, Transform hero, Vector3 spawn)
        {
            var root = new GameObject(id + " Enemy");
            root.transform.SetParent(parent, false);
            root.transform.position = spawn;
            var facing = hero.position - spawn; facing.y = 0;
            root.transform.rotation = facing.sqrMagnitude > .001f ? Quaternion.LookRotation(facing.normalized, Vector3.up) : Quaternion.identity;

            string assetPath = Source + id + "/" + id + ".FBX";
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null) throw new Exception("Missing monster model " + assetPath);
            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.SaveAndReimport();
            }
            var original = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (original == null) throw new Exception("Missing monster prefab " + assetPath);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(original);
            visual.name = id + " Visual";
            visual.transform.SetParent(root.transform, false);
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new Exception("Monster has no mesh: " + id);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            if (b.size.y < .01f) throw new Exception("Invalid monster bounds " + id);
            visual.transform.localScale *= 1.6f / b.size.y;
            b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            visual.transform.position += root.transform.position - new Vector3(b.center.x, b.min.y, b.center.z);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Source + id + "/" + id + ".tga");
            if (texture == null) throw new Exception("Missing monster texture " + id);
            string matPath = Root + id + "PvE.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard")) { mainTexture = texture };
                AssetDatabase.CreateAsset(mat, matPath);
            }
            foreach (var r in renderers) r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
            var animator = visual.GetComponent<Animator>();
            if (animator == null) animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = MakeController(id);
            animator.applyRootMotion = false;
            var capsule = root.AddComponent<CharacterController>();
            capsule.height = 1.6f; capsule.center = new Vector3(0, .8f, 0); capsule.radius = .40f;
            capsule.stepOffset = .2f; capsule.slopeLimit = 50f;
            var enemy = root.AddComponent<CombatEnemy>();
            enemy.Configure(hero, animator, title, 65 + number * 25, 3 + number);
            Debug.Log("LOE_PVE_MONSTER " + id + " pos=" + spawn + " height=" + b.size.y + " renderers=" + renderers.Length);
            return enemy;
        }

        private static Text Label(Transform parent, string name, string content, int fontSize, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var label = go.GetComponent<Text>();
            label.text = content; label.fontSize = fontSize; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.color = Color.white; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false;
            return label;
        }

        [MenuItem("Legacy of Eternity/Build PvE Arena")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero = all.FirstOrDefault(t => t.name == "LOE Prototype Hero");
            var hud = all.FirstOrDefault(t => t.name == "LOE Mobile HUD");
            if (hero == null || hud == null) throw new Exception("Missing player or HUD");
            var arena = all.FirstOrDefault(t => t.name == "LOE PvE Arena");
            if (arena == null)
            {
                arena = new GameObject("LOE PvE Arena").transform;
                Physics.SyncTransforms();
            }
            var monsters = new CombatEnemy[Monsters.Length];
            var spots = new List<Vector3>();
            for (int i = 0; i < Monsters.Length; i++)
            {
                var existing = arena.Find(Monsters[i] + " Enemy");
                if (existing != null) monsters[i] = existing.GetComponent<CombatEnemy>();
                else
                {
                    Vector3 spawn = FindSpawn(hero.position, spots, i);
                    monsters[i] = MakeMonster(Monsters[i], Names[i], i, arena, hero, spawn);
                }
                if (monsters[i] == null) throw new Exception("Missing PvE component " + Monsters[i]);
                spots.Add(monsters[i].transform.position);
            }

            var encounter = hero.GetComponent<CombatEncounter>() ?? hero.gameObject.AddComponent<CombatEncounter>();
            var panel = hud.Find("PvE Target HUD");
            if (panel == null)
            {
                var go = new GameObject("PvE Target HUD", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(hud, false); panel = go.transform;
                var r = go.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(1, 1);
                r.anchoredPosition = new Vector2(-25, -230); r.sizeDelta = new Vector2(480, 118);
                go.GetComponent<Image>().color = new Color(.03f, .10f, .15f, .75f);
                Label(panel, "Target Name", "ЦЕЛЬ: НЕТ", 25, new Vector2(0, .45f), Vector2.one);
                Label(panel, "Target Message", "АТАКА / НАВЫКИ / СМЕНА ЦЕЛИ", 20, Vector2.zero, new Vector2(1, .45f));
            }
            var selected = panel.Find("Target Name")?.GetComponent<Text>();
            var message = panel.Find("Target Message")?.GetComponent<Text>();
            if (selected == null || message == null) throw new Exception("PvE target HUD incomplete");
            encounter.Configure(monsters, selected, message);
            var button = hud.Find("Select Next Target");
            if (button == null)
            {
                var go = new GameObject("Select Next Target", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(hud, false);
                button = go.transform;
                var r = go.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(1, 0);
                r.pivot = new Vector2(1, 0);
                r.sizeDelta = new Vector2(185, 70);
                r.anchoredPosition = new Vector2(-28, 275);
                go.GetComponent<Image>().color = new Color(.06f, .26f, .35f, .9f);
                Label(button, "Caption", "ВЫБРАТЬ ЦЕЛЬ", 21, Vector2.zero, Vector2.one);
                UnityEventTools.AddVoidPersistentListener(go.GetComponent<Button>().onClick, encounter.TargetNext);
            }
            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Failed saving arena scene");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_PVE_BUILDER_OK monsters=" + monsters.Length + " targetHUD=1 targetButton=1 scene=" + ScenePath);
        }
    }
}
