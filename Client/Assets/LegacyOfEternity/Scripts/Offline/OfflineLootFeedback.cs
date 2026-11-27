using System.Collections.Generic;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>Short mobile toast lines for loot / item use.</summary>
    public sealed class OfflineLootFeedback : MonoBehaviour
    {
        struct Line
        {
            public string text;
            public float until;
        }

        readonly List<Line> lines = new List<Line>();
        GUIStyle style;
        bool ready;

        public static OfflineLootFeedback Ensure(GameObject host)
        {
            var existing = host.GetComponent<OfflineLootFeedback>();
            if (existing != null)
                return existing;
            return host.AddComponent<OfflineLootFeedback>();
        }

        public void ShowLoot(OfflineDemoStore.LootLine[] drops)
        {
            if (drops == null)
                return;
            for (int i = 0; i < drops.Length; i++)
            {
                var d = drops[i];
                if (d == null || d.count <= 0)
                    continue;
                string label = d.itemId == "money"
                    ? "+" + d.count + " монет"
                    : "+" + OfflineDemoStore.ItemDisplayName(d.itemId) + " ×" + d.count;
                Push(label);
            }
        }

        public void Push(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            lines.Add(new Line { text = text, until = Time.unscaledTime + 2.8f });
            while (lines.Count > 5)
                lines.RemoveAt(0);
        }

        void EnsureStyle()
        {
            if (ready)
                return;
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(26f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.92f, 0.45f, 1f) }
            };
            ready = true;
        }

        void OnGUI()
        {
            if (!OfflineSession.Ready || lines.Count == 0)
                return;
            EnsureStyle();
            float now = Time.unscaledTime;
            for (int i = lines.Count - 1; i >= 0; i--)
                if (lines[i].until <= now)
                    lines.RemoveAt(i);

            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            float y = Screen.height * 0.42f;
            for (int i = 0; i < lines.Count; i++)
            {
                var rect = new Rect(20f, y + i * 34f * scale, Screen.width - 40f, 34f * scale);
                GUI.Label(rect, lines[i].text, style);
            }
        }
    }
}
