using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LegacyOfEternity.Player;
using LegacyOfEternity.CameraSystem;

namespace LegacyOfEternity.Editor
{
    public static class CoastalPrototypeBuilder
    {
        const string Source = "Assets/LegacyAssets/Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity";
        const string Output = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        [MenuItem("Legacy of Eternity/Build Coastal Prototype")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(Source, OpenSceneMode.Single);
            var objects = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToArray();
            int removed = 0;
            foreach (var obj in objects)
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            var renderers = objects.SelectMany(o => o.GetComponents<Renderer>()).Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
            if (renderers.Length == 0) throw new Exception("No active renderers in coast");
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            foreach (var obj in objects)
            {
                foreach (var cam in obj.GetComponents<Camera>()) cam.enabled = false;
                foreach (var listener in obj.GetComponents<AudioListener>()) listener.enabled = false;
                if (obj.CompareTag("MainCamera")) obj.tag = "Untagged";
            }
            Physics.SyncTransforms();
            Vector3 spawn = bounds.center;
            bool found = false;
            for (int ring = 0; ring < 10 && !found; ring++)
            for (int i = 0; i < 16 && !found; i++)
            {
                float a = i * Mathf.PI / 8;
                var point = bounds.center + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * ring * Mathf.Min(bounds.size.x, bounds.size.z) / 24;
                point.y = bounds.max.y + 20;
                if (Physics.Raycast(point, Vector3.down, out var hit, bounds.size.y + 100) && hit.normal.y > .7f)
                { spawn = hit.point + Vector3.up * 1.1f; found = true; }
            }
            if (!found) throw new Exception("No walkable collider found; manual spawn needed");
            var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = "LOE Prototype Hero";
            hero.transform.position = spawn;
            UnityEngine.Object.DestroyImmediate(hero.GetComponent<Collider>());
            hero.AddComponent<CharacterController>();
            hero.AddComponent<PlayerMotor>();
            var camera = new GameObject("LOE Follow Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>().farClipPlane = 1000;
            camera.AddComponent<AudioListener>();
            camera.AddComponent<OrbitFollowCamera>().SetTarget(hero.transform);
            camera.transform.rotation = Quaternion.Euler(35,0,0);
            camera.transform.position = spawn + Vector3.up * 1.5f - camera.transform.forward * 9;
            if (!EditorSceneManager.SaveScene(scene, Output, true)) throw new Exception("Save copy failed");
            var copy = EditorSceneManager.OpenScene(Output, OpenSceneMode.Single);
            int missing = copy.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            if (missing != 0) throw new Exception("Missing scripts remain: " + missing);
            Debug.Log($"LOE_COAST_OK renderers={renderers.Length} removed={removed} spawn={spawn} missing={missing}");
        }
    }
}
