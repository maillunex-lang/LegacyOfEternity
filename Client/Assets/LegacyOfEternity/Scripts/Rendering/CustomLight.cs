using UnityEngine;

namespace LegacyOfEternity.Rendering
{
    /// <summary>
    /// Ports Darklord CustomLight: feeds original Custom/Character globals
    /// (_SLightColor / _SLightDir / _SLightIntensity).
    /// </summary>
    [ExecuteAlways]
    public sealed class CustomLight : MonoBehaviour
    {
        public Light mainLight;
        public Light characterLight;

        void Update()
        {
            if (mainLight != null)
                SetMainLight();
            if (characterLight != null)
                SetCharacterLight();
        }

        void SetCharacterLight()
        {
            Shader.SetGlobalColor("_SLightColor", characterLight.color);
            Vector3 direction = characterLight.transform.rotation * Vector3.back;
            Shader.SetGlobalVector("_SLightDir", direction);
            Shader.SetGlobalFloat("_SLightIntensity", characterLight.intensity);
        }

        void SetMainLight()
        {
            Shader.SetGlobalColor("_MLightColor", mainLight.color);
            Vector3 direction = mainLight.transform.rotation * Vector3.back;
            Shader.SetGlobalVector("_MLightDir", direction);
            Shader.SetGlobalFloat("_MLightIntensity", mainLight.intensity);
        }

        /// <summary>Safe defaults when no scene light is wired yet.</summary>
        public static void ApplyDefaultCharacterLight()
        {
            Shader.SetGlobalColor("_SLightColor", new Color(1f, 0.95f, 0.9f, 1f));
            Shader.SetGlobalVector("_SLightDir", new Vector4(0.35f, 0.75f, -0.45f, 0f));
            Shader.SetGlobalFloat("_SLightIntensity", 1.1f);
        }
    }
}
