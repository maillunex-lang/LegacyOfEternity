using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Editor
{
    public static class M055CombatHudBuilder
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private static readonly string[] Names = { "Attack", "Skill 1", "Skill 2", "Skill 3", "Skill 4" };

        [MenuItem("Legacy of Eternity/M05.5 Install Combat HUD")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var all = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero = all.FirstOrDefault(x => x.name == "LOE Prototype Hero");
            var hud = all.FirstOrDefault(x => x.name == "LOE Mobile HUD");
            var arena = all.FirstOrDefault(x => x.name == "LOE PvE Arena");
            if (hero == null || hud == null || arena == null)
                throw new Exception("Missing hero, HUD or monster arena");
            var encounter = hero.GetComponent<CombatEncounter>();
            if (encounter == null) throw new Exception("CombatEncounter missing");
            var panel = hud.Find("Combat Actions");
            if (panel == null) throw new Exception("Combat Actions missing");

            var labels = new Text[5];
            for (int i = 0; i < 5; ++i)
            {
                var parent = panel.Find(Names[i]);
                if (parent == null || parent.GetComponent<Button>() == null)
                    throw new Exception("Missing combat button " + Names[i]);
                var overlay = parent.Find("Cooldown Overlay");
                if (overlay == null)
                {
                    var obj = new GameObject("Cooldown Overlay", typeof(RectTransform), typeof(Text));
                    obj.transform.SetParent(parent, false);
                    overlay = obj.transform;
                    var rect = obj.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(.48f, .52f);
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var text = obj.GetComponent<Text>();
                    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    text.fontSize = 24;
                    text.alignment = TextAnchor.MiddleRight;
                    text.color = new Color(1f, .87f, .45f, 1f);
                    text.raycastTarget = false;
                }
                labels[i] = overlay.GetComponent<Text>();
                if (labels[i] == null) throw new Exception("Missing cooldown Text on " + Names[i]);
            }

            var cooldown = hero.GetComponent<CombatCooldownHud>();
            if (cooldown == null) cooldown = hero.gameObject.AddComponent<CombatCooldownHud>();
            cooldown.Configure(encounter, labels);
            var monsters = arena.GetComponentsInChildren<CombatEnemy>(true);
            if (monsters.Length != 3) throw new Exception("Expected three PvE monsters");
            foreach (var enemy in monsters)
            {
                var visual = enemy.GetComponent<CombatEnemyHud>();
                if (visual == null) visual = enemy.gameObject.AddComponent<CombatEnemyHud>();
                visual.Configure(encounter);
                EditorUtility.SetDirty(visual);
            }
            EditorUtility.SetDirty(cooldown);
            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save HUD changes");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M055_HUD_COOLDOWNS=5 ENEMY_HEALTH_BARS=3 DAMAGE_NUMBERS=3 TARGET_HIGHLIGHT=3");
        }

        [MenuItem("Legacy of Eternity/M05.5 Audit Combat HUD")]
        public static void Audit()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var all = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero = all.First(x => x.name == "LOE Prototype Hero");
            var hud = all.First(x => x.name == "LOE Mobile HUD");
            var arena = all.First(x => x.name == "LOE PvE Arena");
            var panel = hud.Find("Combat Actions");
            if (hero.GetComponent<CombatCooldownHud>() == null ||
                Names.Any(n => panel.Find(n)?.Find("Cooldown Overlay")?.GetComponent<Text>() == null) ||
                arena.GetComponentsInChildren<CombatEnemyHud>(true).Length != 3)
                throw new Exception("LOE_M055_HUD_AUDIT failed");
            Debug.Log("LOE_M055_HUD_AUDIT passed cooldowns=5 worldBars=3");
        }
    }
}
