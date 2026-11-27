using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LegacyOfEternity.Editor
{
    /// <summary>
    /// Phone acceptance hotfix: heroes readable on Android (Unlit) + soft particle
    /// fallback so empty/broken VFX slots are not solid purple/white quads.
    /// </summary>
    public static class M0511AndroidVisibilityFix
    {
        private const string EffectsRoot = "Assets/LegacyOfEternity/Effects";
        private const string SoftTexPath = "Assets/LegacyOfEternity/Effects/SoftParticle.png";
        private const string FallbackPath = "Assets/LegacyOfEternity/Effects/AndroidParticleFallback.mat";
        private static readonly string[] HeroMats =
        {
            "Assets/LegacyOfEternity/WarriorPrototype.mat",
            "Assets/LegacyOfEternity/AssassinPrototype.mat",
            "Assets/LegacyOfEternity/MagePrototype.mat",
            "Assets/LegacyOfEternity/ArcherPrototype.mat"
        };

        [MenuItem("Legacy of Eternity/M05.11 Fix Android Hero+VFX Visibility")]
        public static void Run()
        {
            int heroes = FixHeroMaterials();
            var soft = AssetDatabase.LoadAssetAtPath<Texture2D>(SoftTexPath);
            if (soft == null)
                throw new Exception("Missing SoftParticle.png at " + SoftTexPath);

            var fallback = EnsureSoftFallback(soft);
            int prefabs = 0, remapped = 0, filled = 0, disabled = 0;

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
                            if (mats[i] == null)
                            {
                                mats[i] = fallback;
                                filled++;
                                changed = true;
                                continue;
                            }
                            if (NeedsSoftFallback(mats[i]))
                            {
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

                        // Particle quads that still have no useful albedo → hide renderer
                        // rather than show a full-screen purple sheet.
                        var psr = renderer as ParticleSystemRenderer;
                        if (psr != null && AllFallbackOrEmpty(psr.sharedMaterials, fallback))
                        {
                            // Keep enabled but with soft texture fallback already assigned.
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
            Debug.Log("LOE_M0511_VISIBILITY_FIX heroes=" + heroes +
                      " prefabs=" + prefabs + " filledNull=" + filled +
                      " remapped=" + remapped + " disabled=" + disabled);
        }

        private static int FixHeroMaterials()
        {
            var unlit = Shader.Find("Unlit/Texture");
            if (unlit == null)
                unlit = Shader.Find("Mobile/Unlit (Supports Lightmap)");
            if (unlit == null)
                throw new Exception("No Unlit shader for hero visibility fix");

            int count = 0;
            foreach (var path in HeroMats)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                    continue;
                var tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                mat.shader = unlit;
                if (tex != null && mat.HasProperty("_MainTex"))
                    mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", Color.white);
                EditorUtility.SetDirty(mat);
                count++;
            }
            return count;
        }

        private static Material EnsureSoftFallback(Texture2D soft)
        {
            var shader = Shader.Find("Mobile/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null)
                throw new Exception("No particle additive shader");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(FallbackPath);
            if (mat == null)
            {
                mat = new Material(shader) { name = "AndroidParticleFallback" };
                AssetDatabase.CreateAsset(mat, FallbackPath);
            }
            mat.shader = shader;
            if (mat.HasProperty("_MainTex"))
                mat.SetTexture("_MainTex", soft);
            if (mat.HasProperty("_TintColor"))
                mat.SetColor("_TintColor", new Color(1f, 0.95f, 0.75f, 0.45f));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static bool NeedsSoftFallback(Material mat)
        {
            if (mat == null || mat.shader == null)
                return true;
            string name = mat.shader.name ?? "";
            if (name == "Hidden/InternalErrorShader" || !mat.shader.isSupported)
                return true;
            // These ShaderForge shaders often sample as flat tinted sheets on mobile GLES.
            if (name == "LD/LD_glow" || name == "LD/LD_uv_blend_depth" || name == "LD/LD_water")
                return true;
            if (!HasAnyTexture(mat))
                return true;
            return false;
        }

        private static bool HasAnyTexture(Material mat)
        {
            foreach (var key in new[] { "_MainTex", "_Texture", "_Tex", "_wenli" })
            {
                if (mat.HasProperty(key) && mat.GetTexture(key) != null)
                    return true;
            }
            return false;
        }

        private static bool AllFallbackOrEmpty(Material[] mats, Material fallback)
        {
            if (mats == null || mats.Length == 0)
                return true;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] != null && mats[i] != fallback)
                    return false;
            return true;
        }
    }
}
