using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Editor
{
    /// <summary>
    /// Creates cleaned copies of ORIGINAL class FX prefabs. The historical effect scripts
    /// have no compatible Unity 6 implementation, so missing MonoBehaviours are stripped
    /// only from new working prefabs (the original assets are never modified).
    /// </summary>
    public static class M053OriginalVfxBuilder
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const string Original = "Assets/LegacyAssets/Resources/Effect/skill/player/";
        private const string DestRoot = "Assets/LegacyOfEternity/Effects";
        private const string AudioRoot = "Assets/LegacyAssets/Resources/Sound/skill/";
        private static readonly string[] ClassFolders = {"ZS_M", "CK_W", "FS_W", "GJS_M"};
        private static readonly string[] Clips = {"attack001", "skill001", "skill002", "skill003", "skill004"};
        private static readonly string[][] OriginalSounds =
        {
            new[] {"ZS_Atk_01.mp3", "ZS_Skill_01.mp3", "ZS_Skill_02.mp3", "ZS_Skill_03.mp3", "ZS_Skill_04.mp3"},
            new[] {"assassinatorattack01.mp3", "assassinatorskill01.mp3", "assassinatorskill02.mp3", "assassinatorskill03.mp3", "assassinatorskill04.mp3"},
            new[] {"FS_Atk_01.mp3", "FS_Skill_01.mp3", "FS_Skill_02.mp3", "FS_Skill_03.mp3", "FS_Skill_04.mp3"},
            // Available archer/shooter assets. Original 4-skill audio mapping not confirmed.
            new[] {"SS_Atk_01.wav", "SS_Atk_02.wav", "SS_Atk_03.wav", "SS_Atk_02.wav", "SS_Skill_04.wav"}
        };

        private static void EnsureDirectory(string path)
        {
            var parts = path.Split('/');
            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        [MenuItem("Legacy of Eternity/M05.3 Install Original FX and Sounds")]
        public static void Run()
        {
            EnsureDirectory(DestRoot);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var hero = scene.GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "LOE Prototype Hero");
            if (hero == null) throw new Exception("LOE prototype hero not present");
            var controller = hero.GetComponent<OriginalCombatEffects>();
            if (controller == null) controller = hero.gameObject.AddComponent<OriginalCombatEffects>();
            var assets = new GameObject[20];
            var sounds = new AudioClip[20];
            int missingComponentsRemoved = 0;
            for (int cl = 0; cl < ClassFolders.Length; cl++)
            {
                string cls = ClassFolders[cl];
                EnsureDirectory(DestRoot + "/" + cls);
                for (int slot = 0; slot < 5; slot++)
                {
                    int index = cl*5 + slot;
                    // Local player uses the unsuffixed prefab. The original client
                    // appends _S only for AT_REMOTE_PLAYER, never local PvE.
                    string name = cls + "_" + Clips[slot];
                    string originalPath = Original + cls + "/" + name + ".prefab";
                    string targetPath = DestRoot + "/" + cls + "/" + name + "_Unity6.prefab";
                    var source = AssetDatabase.LoadAssetAtPath<GameObject>(originalPath);
                    if (source == null) throw new Exception("Missing original FX " + originalPath);

                    var effect = (GameObject)PrefabUtility.InstantiatePrefab(source);
                    if (effect == null) throw new Exception("Cannot instantiate original FX " + originalPath);
                    effect.name = name + "_Unity6";
                    if (PrefabUtility.IsPartOfPrefabInstance(effect))
                        PrefabUtility.UnpackPrefabInstance(effect, PrefabUnpackMode.Completely,
                            InteractionMode.AutomatedAction);
                    foreach (var t in effect.GetComponentsInChildren<Transform>(true))
                    {
                        missingComponentsRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                        t.gameObject.layer = 0; // Old custom FX layer 18 is no longer defined.
                    }

                    var particleCount = effect.GetComponentsInChildren<ParticleSystem>(true).Length;
                    var rendererCount = effect.GetComponentsInChildren<Renderer>(true).Length;
                    if (particleCount == 0 && rendererCount == 0)
                    {
                        UnityEngine.Object.DestroyImmediate(effect);
                        throw new Exception("Original FX became invisible " + originalPath);
                    }
                    assets[index] = PrefabUtility.SaveAsPrefabAsset(effect, targetPath);
                    UnityEngine.Object.DestroyImmediate(effect);
                    if (assets[index] == null) throw new Exception("FX prefab save failed " + targetPath);

                    sounds[index] = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioRoot + OriginalSounds[cl][slot]);
                    if (sounds[index] == null)
                        throw new Exception("Missing original sound " + AudioRoot + OriginalSounds[cl][slot]);
                    Debug.Log("LOE_M053_INSTALL class=" + cls + " action=" + Clips[slot] +
                        " originalParticles=" + particleCount + " originalRenderers=" + rendererCount +
                        " sound=" + OriginalSounds[cl][slot]);
                }
            }
            var serialized = new SerializedObject(controller);
            var effectArray = serialized.FindProperty("classEffects");
            var audioArray = serialized.FindProperty("classSounds");
            effectArray.arraySize = audioArray.arraySize = 20;
            for (int i = 0; i < 20; i++)
            {
                effectArray.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
                audioArray.GetArrayElementAtIndex(i).objectReferenceValue = sounds[i];
            }
            serialized.FindProperty("selector").objectReferenceValue =
                hero.GetComponent<LegacyOfEternity.Player.CharacterClassSelector>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("M05.3 scene save failed");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M053_INSTALLED classActions=20 originalSounds=20 removedIncompatibleLegacyScripts=" +
                missingComponentsRemoved);
        }

        [MenuItem("Legacy of Eternity/M05.3 Audit Installed Original FX")]
        public static void Audit()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var hero = scene.GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .First(t => t.name == "LOE Prototype Hero");
            var effectComponent = hero.GetComponent<OriginalCombatEffects>();
            if (effectComponent == null) throw new Exception("OriginalCombatEffects missing on hero");
            var serialized = new SerializedObject(effectComponent);
            var effects = serialized.FindProperty("classEffects");
            var sounds = serialized.FindProperty("classSounds");
            if (effects.arraySize != 20 || sounds.arraySize != 20)
                throw new Exception("Original combat FX and audio arrays must contain 20 entries");
            int missing = 0, particles = 0;
            for (int i = 0; i < 20; i++)
            {
                var prefab = effects.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                var audio = sounds.GetArrayElementAtIndex(i).objectReferenceValue as AudioClip;
                if (prefab == null || audio == null) throw new Exception("FX or sound missing at index " + i);
                var scriptsMissing = prefab.GetComponentsInChildren<Transform>(true)
                    .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                missing += scriptsMissing;
                particles += prefab.GetComponentsInChildren<ParticleSystem>(true).Length;
            }
            Debug.Log("LOE_M053_AUDIT originalEffects=20 originalSounds=20 particles=" + particles +
                " missingScripts=" + missing);
            if (missing > 0) throw new Exception("Cleaned original VFX still contain " + missing + " scripts");
        }
    }
}
