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
                    if (mat == null || mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                    {
                        mats[m] = new Material(fallbackShader) { name = "LOE_FallbackMat" };
                        changed = true;
                    }
                }
                if (changed)
                    rend.sharedMaterials = mats;
            }
        }
    }
}
