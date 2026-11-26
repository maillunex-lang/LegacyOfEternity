using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Mobile
{
    /// <summary>Original UIMain auto-fight icon toggles with actual AUTO state.</summary>
    public sealed class OriginalAutoButtonArt : MonoBehaviour
    {
        [SerializeField] private AutoCombatController controller;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite autoOn;
        [SerializeField] private Sprite autoOff;
        private int previous = -1;

        public void Configure(AutoCombatController value, Image image, Sprite on, Sprite off)
        {
            controller = value;
            icon = image;
            autoOn = on;
            autoOff = off;
            previous = -1;
            Refresh();
        }

        private void OnEnable() { previous = -1; Refresh(); }

        private void LateUpdate()
        {
            if (controller != null && previous != (controller.IsEnabled ? 1 : 0))
                Refresh();
        }

        private void Refresh()
        {
            if (controller == null || icon == null || autoOn == null || autoOff == null) return;
            bool enabled = controller.IsEnabled;
            icon.sprite = enabled ? autoOn : autoOff;
            icon.preserveAspect = true;
            icon.color = Color.white;
            previous = enabled ? 1 : 0;
        }
    }
}
