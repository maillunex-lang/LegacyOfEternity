using UnityEngine;
using UnityEngine.UI;

namespace LegacyOfEternity.Combat
{
    /// <summary>Shows the same cooldown timers used by the shared manual/automatic battle engine.</summary>
    public sealed class CombatCooldownHud : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        [SerializeField] private Text[] countdowns = new Text[5];

        public void Configure(CombatEncounter combat, Text[] labels)
        {
            encounter = combat;
            countdowns = labels;
        }

        private void Update()
        {
            if (encounter == null || countdowns == null) return;
            for (int slot = 0; slot < countdowns.Length && slot < 5; slot++)
            {
                var label = countdowns[slot];
                if (label == null) continue;
                float remaining = encounter.CooldownRemaining(slot);
                label.text = remaining > .05f ? remaining.ToString("F1") + "с" : "";
            }
        }
    }
}
