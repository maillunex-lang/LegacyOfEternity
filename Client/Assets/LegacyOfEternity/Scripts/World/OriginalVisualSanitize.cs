using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>
    /// Fix pink/missing materials on ported Darklord prefabs.
    /// Legacy scripts are not ported; missing-script components are stripped from YAML offline.
    /// </summary>
    public static class OriginalVisualSanitize
    {
        static Shader fallbackShader;

        public static void Sanitize(GameObject root)
        {
            if (root == null)
                return;
#if UNITY_EDITOR
            StripMissingScriptsEditor(root);
#endif
            FixMissingMaterials(root);
        }

#if UNITY_EDITOR
        static void StripMissingScriptsEditor(GameObject root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transforms[i].gameObject);
        }
#endif

        static void FixMissingMaterials(GameObject root)
        {
            if (fallbackShader == null)
            {
                fallbackShader = Shader.Find("Custom/Character")
                                 ?? Shader.Find("Legacy Shaders/Diffuse")
                                 ?? Shader.Find("Standard")
                                 ?? Shader.Find("Mobile/Diffuse");
            }
            if (fallbackShader == null)
                return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                var rend = renderers[r];
                if (rend == null)
                    continue;
                var mats = rend.sharedMaterials;
                if (mats == null || mats.Length == 0)
                    continue;
                bool changed = false;
                for (int m = 0; m < mats.Length; m++)
                {
                    var mat = mats[m];
                    bool broken = mat == null
                                  || mat.shader == null
                                  || mat.shader.name == "Hidden/InternalErrorShader"
                                  || mat.shader.name.Contains("Error");
                    if (!broken && mat != null && mat.HasProperty("_MainTex") && mat.mainTexture == null)
                    {
                        // Missing texture often renders magenta on mobile; keep shader if possible.
                        broken = mat.shader.name.StartsWith("Custom/", StringComparison.Ordinal)
                                 || mat.shader.name.StartsWith("LD_", StringComparison.Ordinal);
                    }
                    if (broken)
                    {
                        mats[m] = new Material(fallbackShader)
                        {
                            name = "LOE_FallbackMat",
                            color = new Color(0.55f, 0.52f, 0.48f, 1f)
                        };
                        changed = true;
                    }
                }
                if (changed)
                    rend.sharedMaterials = mats;
            }
        }
    }
}
