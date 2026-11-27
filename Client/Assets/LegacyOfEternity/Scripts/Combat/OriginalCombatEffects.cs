using System.Collections;
using UnityEngine;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Combat
{
    /// <summary>
    /// Original source VFX/audio, cleaned from incompatible legacy scripts by the Editor import tool.
    /// This is a local PvE preview: gameplay damage is still owned by CombatEncounter.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class OriginalCombatEffects : MonoBehaviour
    {
        [SerializeField] private CharacterClassSelector selector;
        [SerializeField] private GameObject[] classEffects = new GameObject[20];
        [SerializeField] private AudioClip[] classSounds = new AudioClip[20];
        private AudioSource player;
        // Source: Resources/Lua/Data/t_skill_action.lua.txt, normal/local-player FX.
        // Source has no configured FS_W attack001 FX; use the short preview fallback.
        private static readonly float[,] EffectSeconds =
        {
            { 0.6f, 1.4f, 2.0f, 1.9f, 3.0f },
            { 1.0f, 10.0f, 3.0f, 2.8f, 2.5f },
            { 0.8f, 3.0f, 2.0f, 2.5f, 5.0f },
            { 0.8f, 2.1f, 1.6f, 2.1f, 2.8f }
        };
        private static readonly bool[,] SourceBind =
        {
            { false, false, true, true, false },
            { false, false, false, false, false },
            { false, false, false, false, false },
            { false, false, false, false, false }
        };

        private void Awake()
        {
            if (selector == null) selector = GetComponent<CharacterClassSelector>();
            player = GetComponent<AudioSource>();
            player.playOnAwake = false;
            player.spatialBlend = 0f;
            player.volume = 0.58f;
        }

        public void PlayAction(int slot, CombatEnemy target, float firstImpactSeconds)
        {
            int cl = selector != null ? (int)selector.CurrentClass : 0;
            if (slot < 0 || slot > 4 || cl < 0 || cl > 3) return;
            int index = cl * 5 + slot;

            if (classSounds != null && index < classSounds.Length && classSounds[index] != null)
                player.PlayOneShot(classSounds[index]);

            if (classEffects == null || index >= classEffects.Length || classEffects[index] == null)
                return;
            StartCoroutine(SpawnActionEffect(classEffects[index], target, cl, slot, firstImpactSeconds));
        }

        private IEnumerator SpawnActionEffect(GameObject prefab, CombatEnemy target, int classIndex,
                                               int slot, float impactSeconds)
        {
            bool ranged = classIndex >= 2;
            float fxLifetime = EffectSeconds[classIndex, Mathf.Clamp(slot, 0, 4)];
            // The source skill table requests attachment only for warrior skills 002/003.
            Transform visual = selector != null && selector.ActiveAnimator != null
                ? selector.ActiveAnimator.transform : transform;
            Transform muzzle = null;
            if (ranged)
            {
                foreach (var marker in visual.GetComponentsInChildren<Transform>(true))
                    if (marker.name == "E_fire" || marker.name == "DM_R_Hand")
                    {
                        muzzle = marker;
                        break;
                    }
            }

            Vector3 spawn = ranged
                ? (muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.25f)
                : transform.position;
            Quaternion facing = transform.rotation;
            GameObject instance = Instantiate(prefab, spawn, facing);
            if (SourceBind[classIndex, slot])
            {
                instance.transform.SetParent(transform, true);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
            instance.name = "Original FX " + prefab.name;
            SanitizeMagentaMaterials(instance);
            // The imported heroes are normalized to 1.8 world units high by FourClassesBuilder.
            // Original FX authored for the original character rig must preserve their scale
            // relative to that same rig. The visual's original scale factor varies by class.
            // Never modify the source particle size/shape curves or original prefabs.
            float originalRigScale = visual != null ? visual.lossyScale.x : 1f;
            if (originalRigScale <= 0.001f || originalRigScale > 10f)
                originalRigScale = 1f;
            instance.transform.localScale = SourceBind[classIndex, slot]
                ? prefab.transform.localScale : prefab.transform.localScale * originalRigScale;
            // Original particle prefabs already encode Play On Awake and active-state
            // per emitter. Forcing Play(true) on every nested system was bypassing
            // their authored timing and triggering inactive ice/debris prematurely.
            // Let Unity start only the emitters enabled by the original prefab.

            if (ranged && target != null && !target.IsDead)
            {
                float duration = Mathf.Clamp(impactSeconds, 0.15f, 2.0f);
                float elapsed = 0f;
                var origin = instance.transform.position;
                var destination = target.AimPoint;
                while (instance != null && elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    if (target != null && !target.IsDead) destination = target.AimPoint;
                    float fraction = Mathf.Clamp01(elapsed / duration);
                    instance.transform.position = Vector3.Lerp(origin, destination, fraction);
                    yield return null;
                }
            }
            else
            {
                // Original melee FX follow the hero's facing during a short action window.
                float elapsed = 0f;
                float duration = fxLifetime;
                while (instance != null && elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    if (!SourceBind[classIndex, slot])
                    {
                        instance.transform.position = transform.position;
                        instance.transform.rotation = transform.rotation;
                    }
                    yield return null;
                }
            }
            if (instance != null) Destroy(instance, 0.15f);
        }

        /// <summary>
        /// Replace null / error-shader materials that show as magenta on Android GLES.
        /// </summary>
        private static void SanitizeMagentaMaterials(GameObject root)
        {
            var fallback = GetFallbackParticleMaterial();
            if (fallback == null)
                return;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                if (mats == null || mats.Length == 0)
                    continue;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null ||
                        mat.shader == null ||
                        mat.shader.name == "Hidden/InternalErrorShader" ||
                        !mat.shader.isSupported)
                    {
                        mats[i] = fallback;
                        changed = true;
                    }
                }
                if (changed)
                    renderer.sharedMaterials = mats;
            }
        }

        private static Material fallbackParticle;

        private static Material GetFallbackParticleMaterial()
        {
            if (fallbackParticle != null)
                return fallbackParticle;
            var shader = Shader.Find("Mobile/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null)
                return null;
            fallbackParticle = new Material(shader)
            {
                name = "LOE_RuntimeParticleFallback",
                hideFlags = HideFlags.HideAndDontSave
            };
            if (fallbackParticle.HasProperty("_TintColor"))
                fallbackParticle.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.55f));
            return fallbackParticle;
        }
    }
}
