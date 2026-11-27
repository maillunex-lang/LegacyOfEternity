using LegacyOfEternity.World;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Entry point after login. No demo placement — delegates to original MapData bootstrap.
    /// </summary>
    public sealed class OfflineWorldBootstrap : MonoBehaviour
    {
        public static OfflineWorldBootstrap Ensure(Transform hero)
        {
            OriginalHaitanBootstrap.Ensure(hero);

            var existing = FindFirstObjectByType<OfflineWorldBootstrap>();
            if (existing != null)
                return existing;
            var go = new GameObject("LOE_OfflineWorld");
            DontDestroyOnLoad(go);
            return go.AddComponent<OfflineWorldBootstrap>();
        }
    }
}
