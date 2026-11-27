using UnityEngine;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Spawns offline Haitan-style world quest props on CoastalPrototype:
    /// guide NPC (Тиас) and beach gather scraps.
    /// </summary>
    public sealed class OfflineWorldBootstrap : MonoBehaviour
    {
        OfflineWorldInteractable[] interactables = System.Array.Empty<OfflineWorldInteractable>();

        public static OfflineWorldBootstrap Ensure(Transform hero)
        {
            var existing = FindFirstObjectByType<OfflineWorldBootstrap>();
            if (existing != null)
                return existing;
            var go = new GameObject("LOE_OfflineWorld");
            DontDestroyOnLoad(go);
            var boot = go.AddComponent<OfflineWorldBootstrap>();
            boot.Build(hero);
            return boot;
        }

        void Build(Transform hero)
        {
            if (hero == null)
                hero = GameObject.Find("LOE Prototype Hero")?.transform;
            if (hero == null)
            {
                Debug.LogWarning("LOE_OFFLINE_WORLD no hero transform");
                return;
            }

            Vector3 forward = hero.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
                forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            var list = new System.Collections.Generic.List<OfflineWorldInteractable>();

            var guide = CreateInteractable(
                "Тиас",
                hero.position + forward * 4.5f,
                OfflineInteractKind.Guide,
                hero,
                new Color(0.22f, 0.48f, 0.92f, 1f));
            list.Add(guide);

            Vector3 beach = hero.position + forward * 9f + right * 3.5f;
            var enemies = FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
            if (enemies != null && enemies.Length > 0 && enemies[0] != null)
            {
                beach = Vector3.Lerp(hero.position, enemies[0].transform.position, 0.45f);
                beach.y = hero.position.y;
            }

            // One gather is enough for quest goal; extras give the beach a scavenger feel.
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = beach + right * ((i - 1) * 2.2f) + forward * (i == 1 ? 1.1f : 0f);
                var scrap = CreateInteractable(
                    "Прибрежный хлам",
                    pos,
                    OfflineInteractKind.GatherScrap,
                    hero,
                    new Color(0.72f, 0.55f, 0.28f, 1f));
                list.Add(scrap);
            }

            interactables = list.ToArray();
            var hud = gameObject.AddComponent<OfflineInteractHud>();
            hud.Bind(interactables);
            Debug.Log("LOE_OFFLINE_WORLD spawned interactables=" + interactables.Length);
        }

        static OfflineWorldInteractable CreateInteractable(
            string name, Vector3 position, OfflineInteractKind kind, Transform hero, Color color)
        {
            var go = new GameObject("Offline_" + kind + "_" + name);
            go.transform.position = position;
            var interact = go.AddComponent<OfflineWorldInteractable>();
            interact.Setup(kind, name, hero, color);
            return interact;
        }
    }
}
