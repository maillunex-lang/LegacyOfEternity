using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    public static class M054AutoCombatBuilder
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        [MenuItem("Legacy of Eternity/M05.4 Install AUTO Combat")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var objects = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero = objects.FirstOrDefault(t => t.name == "LOE Prototype Hero");
            var hud = objects.FirstOrDefault(t => t.name == "LOE Mobile HUD");
            if (hero == null || hud == null) throw new Exception("Hero or mobile HUD not found");

            var auto = hero.GetComponent<AutoCombatController>();
            if (auto == null) auto = hero.gameObject.AddComponent<AutoCombatController>();
            var toggle = hud.Find("Auto Combat Toggle");
            if (toggle == null)
            {
                var buttonObject = new GameObject("Auto Combat Toggle",
                    typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(hud, false);
                toggle = buttonObject.transform;
                var rect = buttonObject.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                rect.sizeDelta = new Vector2(190, 70);
                rect.anchoredPosition = new Vector2(-240, 275);
                buttonObject.GetComponent<Image>().color = new Color(.05f, .23f, .22f, .92f);

                var labelObject = new GameObject("AUTO Indicator", typeof(RectTransform), typeof(Text));
                labelObject.transform.SetParent(toggle, false);
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
                var text = labelObject.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 21;
                text.raycastTarget = false;
                text.color = Color.white;
                text.text = "АВТО: ВЫКЛ";
            }
            var button = toggle.GetComponent<Button>();
            var indicator = toggle.Find("AUTO Indicator")?.GetComponent<Text>();
            if (button == null || indicator == null)
                throw new Exception("AUTO button setup incomplete");

            if (button.onClick.GetPersistentEventCount() == 0)
                UnityEventTools.AddVoidPersistentListener(button.onClick, auto.Toggle);
            else if (button.onClick.GetPersistentEventCount() != 1 ||
                     button.onClick.GetPersistentMethodName(0) != "Toggle")
                throw new Exception("Unexpected AUTO listener configuration");

            auto.Configure(indicator);
            var so = new SerializedObject(auto);
            so.FindProperty("autoEnabled").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(auto);
            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("AUTO scene save failed");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M054_AUTO_INSTALLED button=1 default=off sharedCombat=1");
        }

        [MenuItem("Legacy of Eternity/M05.4 Audit AUTO Combat")]
        public static void Audit()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var objects = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero = objects.First(t => t.name == "LOE Prototype Hero");
            var hud = objects.First(t => t.name == "LOE Mobile HUD");
            var auto = hero.GetComponent<AutoCombatController>();
            var toggle = hud.Find("Auto Combat Toggle");
            var button = toggle != null ? toggle.GetComponent<Button>() : null;
            if (auto == null || hero.GetComponent<PlayerMotor>() == null ||
                hero.GetComponent<PrototypeCombat>() == null ||
                button == null || toggle.Find("AUTO Indicator")?.GetComponent<Text>() == null ||
                button.onClick.GetPersistentEventCount() != 1 ||
                button.onClick.GetPersistentMethodName(0) != "Toggle")
                throw new Exception("LOE_M054_AUDIT failed");
            var so = new SerializedObject(auto);
            if (so.FindProperty("autoEnabled").boolValue)
                throw new Exception("LOE_M054_AUDIT auto is enabled by default");
            Debug.Log("LOE_M054_AUDIT passed button=1 defaultOff=1");
        }
    }
}
