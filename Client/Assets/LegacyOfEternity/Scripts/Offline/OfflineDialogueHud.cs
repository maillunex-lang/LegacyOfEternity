using System;
using LegacyOfEternity.World;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Modal talk panel: original quest/NPC dialogue (RU), then applies talk/turn-in.
    /// </summary>
    public sealed class OfflineDialogueHud : MonoBehaviour
    {
        static OfflineDialogueHud instance;

        string speakerName = "";
        string[] lines = Array.Empty<string>();
        int lineIndex;
        long npcId;
        bool pendingComplete;
        GUIStyle boxStyle;
        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle buttonStyle;
        bool stylesReady;

        public static bool IsOpen => instance != null && instance.pendingComplete;

        public static OfflineDialogueHud Ensure()
        {
            if (instance != null)
                return instance;
            var existing = FindFirstObjectByType<OfflineDialogueHud>();
            if (existing != null)
            {
                instance = existing;
                return existing;
            }
            var host = GameObject.Find("LOE_OriginalHaitan");
            if (host == null)
            {
                host = new GameObject("LOE_DialogueHud");
                DontDestroyOnLoad(host);
            }
            instance = host.GetComponent<OfflineDialogueHud>();
            if (instance == null)
                instance = host.AddComponent<OfflineDialogueHud>();
            return instance;
        }

        void Awake()
        {
            instance = this;
            OriginalDialogueCatalog.EnsureLoaded();
        }

        public void OpenQuestTalk(string speaker, long talkNpcId, int questId)
        {
            speakerName = string.IsNullOrEmpty(speaker) ? "NPC" : speaker;
            npcId = talkNpcId;
            lines = OriginalDialogueCatalog.LinesForQuestTalk(questId, talkNpcId);
            if (lines == null || lines.Length == 0)
                lines = new[] { OriginalDialogueCatalog.NpcGreeting(talkNpcId) };
            lineIndex = 0;
            pendingComplete = true;
        }

        void EnsureStyles()
        {
            if (stylesReady)
                return;
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            boxStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(26f * scale),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.82f, 0.40f, 1f) },
                wordWrap = true
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(22f * scale),
                normal = { textColor = Color.white },
                wordWrap = true
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(24f * scale),
                fontStyle = FontStyle.Bold,
                fixedHeight = 56f * scale
            };
            stylesReady = true;
        }

        void OnGUI()
        {
            if (!pendingComplete || lines == null || lines.Length == 0)
                return;
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            float width = Mathf.Min(Screen.width - 40f, 720f * scale);
            float height = Mathf.Min(Screen.height * 0.42f, 320f * scale);
            var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 24f * scale, width, height);

            GUI.Box(rect, GUIContent.none, boxStyle);
            GUILayout.BeginArea(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, rect.height - 24f));
            GUILayout.Label(speakerName, titleStyle);
            string text = lineIndex >= 0 && lineIndex < lines.Length ? lines[lineIndex] : "";
            GUILayout.Label(text, bodyStyle);
            GUILayout.FlexibleSpace();
            string btn = lineIndex < lines.Length - 1 ? "Далее" : "Готово";
            if (GUILayout.Button(btn, buttonStyle))
                Advance();
            GUILayout.EndArea();
        }

        void Advance()
        {
            if (lineIndex < lines.Length - 1)
            {
                lineIndex++;
                return;
            }
            Finish();
        }

        void Finish()
        {
            pendingComplete = false;
            var hero = OfflineSession.Hero;
            if (hero != null)
                OfflineDemoStore.ApplyQuestEvent(hero, "talk", npcId);
            lines = Array.Empty<string>();
            lineIndex = 0;
            npcId = 0;
        }
    }
}
