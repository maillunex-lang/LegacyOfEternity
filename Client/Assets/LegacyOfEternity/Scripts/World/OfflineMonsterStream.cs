using System.Collections.Generic;
using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>Activate Haitan monsters only near the hero so birth isn't a mob wall.</summary>
    public sealed class OfflineMonsterStream : MonoBehaviour
    {
        const float ActivateRange = 42f;
        const float DeactivateRange = 55f;

        Transform hero;
        readonly List<GameObject> monsters = new List<GameObject>();
        float nextTick;

        public void Configure(Transform heroTransform, List<GameObject> spawned)
        {
            hero = heroTransform;
            monsters.Clear();
            if (spawned != null)
                monsters.AddRange(spawned);
            Apply(force: true);
        }

        void Update()
        {
            if (hero == null || monsters.Count == 0)
                return;
            if (Time.time < nextTick)
                return;
            nextTick = Time.time + 0.45f;
            Apply(force: false);
        }

        void Apply(bool force)
        {
            Vector3 hp = hero.position;
            for (int i = 0; i < monsters.Count; i++)
            {
                var go = monsters[i];
                if (go == null)
                    continue;
                Vector3 p = go.transform.position;
                float dx = p.x - hp.x;
                float dz = p.z - hp.z;
                float d2 = dx * dx + dz * dz;
                bool on = go.activeSelf;
                if (!on && (force || d2 <= ActivateRange * ActivateRange))
                    go.SetActive(true);
                else if (on && d2 > DeactivateRange * DeactivateRange)
                    go.SetActive(false);
            }
        }
    }
}
