using LegacyOfEternity.Rendering;
using UnityEngine;

namespace LegacyOfEternity.Player
{
    /// <summary>
    /// Keeps class mesh materials on original Custom/Character (PORT-01).
    /// Falls back to Unlit/Texture only if Custom/Character is missing (pink/error).
    /// </summary>
    public sealed class HeroAndroidVisibility : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "CoastalPrototype")
                return;

            CustomLight.ApplyDefaultCharacterLight();

            var hero = GameObject.Find("LOE Prototype Hero");
            if (hero == null)
                return;
            if (hero.GetComponent<HeroAndroidVisibility>() == null)
                hero.AddComponent<HeroAndroidVisibility>();

            EnsureSceneCharacterLight();
        }

        static void EnsureSceneCharacterLight()
        {
            if (Object.FindFirstObjectByType<CustomLight>() != null)
                return;

            var go = new GameObject("LOE Character Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f, 1f);
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var bridge = go.AddComponent<CustomLight>();
            bridge.characterLight = light;

            var existing = Object.FindFirstObjectByType<Light>();
            if (existing != null && existing != light)
                bridge.mainLight = existing;
        }

        void Start()
        {
            Apply();
        }

        public void Apply()
        {
            CustomLight.ApplyDefaultCharacterLight();

            var character = Shader.Find("Custom/Character");
            var unlit = Shader.Find("Unlit/Texture");
            if (unlit == null)
                unlit = Shader.Find("Mobile/Unlit (Supports Lightmap)");

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;
                if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer)
                    continue;

                var mats = renderer.materials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null)
                        continue;

                    // Prefer original character shader when available.
                    if (character != null)
                    {
                        if (mat.shader != character && IsCharacterLikeMaterial(mat))
                        {
                            var tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : mat.mainTexture;
                            var copy = new Material(character);
                            if (tex != null)
                                copy.SetTexture("_MainTex", tex);
                            if (mat.HasProperty("_SpecTex") && copy.HasProperty("_SpecTex"))
                            {
                                var spec = mat.GetTexture("_SpecTex");
                                if (spec != null)
                                    copy.SetTexture("_SpecTex", spec);
                            }
                            CopyFloat(mat, copy, "_SpecPower");
                            CopyFloat(mat, copy, "_SpecIntensity");
                            CopyFloat(mat, copy, "_SpecPower2");
                            CopyFloat(mat, copy, "_SpecIntensity2");
                            mats[i] = copy;
                            changed = true;
                        }
                        continue;
                    }

                    // Emergency fallback only — not parity.
                    if (unlit == null)
                        continue;
                    if (mat.shader == unlit)
                        continue;
                    var fallbackTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : mat.mainTexture;
                    var fallback = new Material(unlit);
                    if (fallbackTex != null)
                        fallback.mainTexture = fallbackTex;
                    if (fallback.HasProperty("_Color"))
                        fallback.SetColor("_Color", Color.white);
                    mats[i] = fallback;
                    changed = true;
                }

                if (changed)
                    renderer.materials = mats;
            }
        }

        static bool IsCharacterLikeMaterial(Material mat)
        {
            if (mat.shader == null)
                return true;
            var name = mat.shader.name ?? string.Empty;
            if (name.Contains("Character"))
                return false; // already character family
            if (name.StartsWith("Hidden/") || name.Contains("Particle") || name.Contains("UI"))
                return false;
            // Standard / Unlit / error / missing → restore Custom/Character
            return name.Contains("Standard")
                   || name.Contains("Unlit")
                   || name.Contains("InternalError")
                   || name.Contains("Hidden/InternalErrorShader")
                   || name == "Mobile/Diffuse"
                   || mat.HasProperty("_MainTex");
        }

        static void CopyFloat(Material src, Material dst, string prop)
        {
            if (src.HasProperty(prop) && dst.HasProperty(prop))
                dst.SetFloat(prop, src.GetFloat(prop));
        }
    }
}
