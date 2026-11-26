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
        private static readonly float[] Seconds = { 1.4f, 1.8f, 2.0f, 2.7f, 3.0f };

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
                : transform.position + Vector3.up * 0.85f + transform.forward * 0.8f;
            Quaternion facing = transform.rotation;
            GameObject instance = Instantiate(prefab, spawn, facing);
            instance.name = "Original FX " + prefab.name;
            // All imported prefab assets were cleaned of missing legacy MonoBehaviours.
            foreach (var system in instance.GetComponentsInChildren<ParticleSystem>(true))
                system.Play(true);

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
                float duration = Seconds[Mathf.Clamp(slot, 0, 4)];
                while (instance != null && elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    instance.transform.position = transform.position + Vector3.up * 0.85f +
                        transform.forward * 0.8f;
                    instance.transform.rotation = transform.rotation;
                    yield return null;
                }
            }
            if (instance != null) Destroy(instance, 0.15f);
        }
    }
}
