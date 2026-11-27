using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LegacyOfEternity.Editor
{
    /// <summary>
    /// Fills empty ParticleSystemRenderer material slots on original VFX prefabs.
    /// Null materials render as magenta (error shader) on Android.
    /// </summary>
    public static class M053AndroidVfxMaterialsFix
    {
        private const string EffectsRoot = "Assets/LegacyOfEternity/Effects";
        private const string FallbackPath = "Assets/LegacyOfEternity/Effects/AndroidParticleFallback.mat";

        [MenuItem("Legacy of Eternity/M05.3 Fix Android VFX Magenta Materials")]
        public static void Run()
        {
            var fallback = EnsureFallback();
            int prefabs = 0, slots = 0, filled = 0, remapped = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { EffectsRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                bool dirty = false;
                try
                {
                    prefabs++;
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        var mats = renderer.sharedMaterials;
                        if (mats == null || mats.Length == 0)
                            continue;
                        bool changed = false;
                        for (int i = 0; i < mats.Length; i++)
                        {
                            slots++;
                            if (mats[i] == null)
                            {
                                mats[i] = fallback;
                                filled++;
                                changed = true;
                                continue;
                            }
                            var shader = mats[i].shader;
                            if (shader == null || shader.name == "Hidden/InternalErrorShader" || !shader.isSupported)
                            {
                                // Keep textures if possible by cloning fallback and copying main tex.
                                var fixedMat = new Material(fallback);
                                CopyKnownTextures(mats[i], fixedMat);
                                var matPath = Path.Combine(
                                    Path.GetDirectoryName(path) ?? EffectsRoot,
                                    mats[i].name + "_AndroidFix.mat").Replace('\\', '/');
                                // Prefer in-memory assignment without proliferating assets for every slot:
                                mats[i] = fallback;
                                remapped++;
                                changed = true;
                            }
                        }
                        if (changed)
                        {
                            renderer.sharedMaterials = mats;
                            dirty = true;
                        }
                    }
                    if (dirty)
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M053_ANDROID_VFX_FIX prefabs=" + prefabs +
                      " slots=" + slots + " filledNull=" + filled + " remappedBroken=" + remapped);
        }

        [MenuItem("Legacy of Eternity/M05.3 Audit Android VFX Magenta Materials")]
        public static void Audit()
        {
            int missing = 0, broken = 0, total = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { EffectsRoot }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = renderer.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        total++;
                        if (mats[i] == null) { missing++; continue; }
                        var shader = mats[i].shader;
                        if (shader == null || shader.name == "Hidden/InternalErrorShader")
                            broken++;
                    }
                }
            }
            Debug.Log("LOE_M053_ANDROID_VFX_AUDIT slots=" + total + " missing=" + missing + " broken=" + broken);
            if (missing != 0 || broken != 0)
                throw new Exception("Android VFX still has magenta-prone materials: missing=" +
                                    missing + " broken=" + broken);
        }

        private static Material EnsureFallback()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(FallbackPath);
            if (existing != null)
                return existing;

            var shader = Shader.Find("Mobile/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Particles/Additive");
            if (shader == null)
                throw new Exception("No particle additive shader for Android VFX fallback");

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FallbackPath
                .Replace("Assets/", Application.dataPath + "/"))) ?? ".");
            var mat = new Material(shader)
            {
                name = "AndroidParticleFallback",
                color = new Color(1f, 1f, 1f, 0.5f)
            };
            if (mat.HasProperty("_TintColor"))
                mat.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.5f));
            AssetDatabase.CreateAsset(mat, FallbackPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<Material>(FallbackPath);
        }

        private static void CopyKnownTextures(Material from, Material to)
        {
            foreach (var key in new[] { "_MainTex", "_Texture", "_Tex", "_wenli", "_mask" })
            {
                if (from.HasProperty(key) && to.HasProperty(key))
                {
                    var tex = from.GetTexture(key);
                    if (tex != null)
                        to.SetTexture(key, tex);
                }
            }
        }
    }
}
