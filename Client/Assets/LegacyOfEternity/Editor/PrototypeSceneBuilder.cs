#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LegacyOfEternity.Player;
using LegacyOfEternity.CameraSystem;

namespace LegacyOfEternity.Editor
{
    /// <summary>Builds a clean, runnable scene without changing the imported historical scene.</summary>
    public static class PrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/LegacyOfEternity/Scenes/Prototype.unity";

        [MenuItem("Legacy of Eternity/Create Prototype")]
        public static void CreateFromMenu() { Build(); }

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Legacy of Eternity Prototype";

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Prototype Ground";
            ground.transform.localScale = new Vector3(16f, 1f, 16f);

            GameObject hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = "Prototype Hero";
            hero.transform.position = new Vector3(0f, 1.3f, 0f);
            UnityEngine.Object.DestroyImmediate(hero.GetComponent<Collider>());
            hero.AddComponent<CharacterController>();
            hero.AddComponent<PlayerMotor>();

            GameObject camera = new GameObject("Legacy Follow Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<UnityEngine.Camera>();
            camera.AddComponent<AudioListener>();
            OrbitFollowCamera follow = camera.AddComponent<OrbitFollowCamera>();
            follow.SetTarget(hero.transform);
            camera.transform.position = new Vector3(0f, 7f, -9f);
            camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

            GameObject sunlight = new GameObject("Sun");
            Light light = sunlight.AddComponent<Light>();
            light.type = LightType.Directional;
            sunlight.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            if (!AssetDatabase.IsValidFolder("Assets/LegacyOfEternity/Scenes"))
                AssetDatabase.CreateFolder("Assets/LegacyOfEternity", "Scenes");

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save prototype scene");

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("LEGACY_OF_ETERNITY_PROTOTYPE_CREATED " + ScenePath);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
