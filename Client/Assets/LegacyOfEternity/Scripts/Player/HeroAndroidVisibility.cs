using UnityEngine;

namespace LegacyOfEternity.Player
{
    /// <summary>
    /// Ensures class mesh materials stay readable on Android (baked lights + Standard = near-black).
    /// </summary>
    public sealed class HeroAndroidVisibility : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "CoastalPrototype")
                return;
            var hero = GameObject.Find("LOE Prototype Hero");
            if (hero == null)
                return;
            if (hero.GetComponent<HeroAndroidVisibility>() == null)
                hero.AddComponent<HeroAndroidVisibility>();
        }

        void Start()
        {
            Apply();
        }

        public void Apply()
        {
            var unlit = Shader.Find("Unlit/Texture");
            if (unlit == null)
                unlit = Shader.Find("Mobile/Unlit (Supports Lightmap)");
            if (unlit == null)
                return;

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;
                // Skip UI / particles under hero if any.
                if (renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer)
                    continue;
                var mats = renderer.materials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null)
                        continue;
                    var tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    if (mat.shader == unlit && tex != null)
                        continue;
                    var copy = new Material(unlit);
                    if (tex != null)
                        copy.mainTexture = tex;
                    else if (mat.HasProperty("_MainTex") && mat.mainTexture != null)
                        copy.mainTexture = mat.mainTexture;
                    if (copy.HasProperty("_Color"))
                        copy.SetColor("_Color", Color.white);
                    mats[i] = copy;
                    changed = true;
                }
                if (changed)
                    renderer.materials = mats;
            }
        }
    }
}
