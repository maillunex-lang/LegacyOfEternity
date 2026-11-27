using System;
using UnityEngine;

namespace LegacyOfEternity.World
{
    [Serializable]
    public sealed class OriginalQuestDoc
    {
        public int id;
        public string name;
        public string summary;
        public string goalFormat;
        public string goalTarget;
        public int acceptStoryId;
    }

    [Serializable]
    public sealed class OriginalStoryDef
    {
        public int storyId;
        public long npcId;
        public string[] lines;
    }

    [Serializable]
    public sealed class OriginalNpcGreeting
    {
        public int dialogueId;
        public long npcId;
        public string text;
    }

    [Serializable]
    public sealed class OriginalDialogueCatalogFile
    {
        public string source;
        public string language;
        public long mapId;
        public OriginalQuestDoc[] quests;
        public OriginalStoryDef[] stories;
        public OriginalNpcGreeting[] npcGreetings;
    }

    /// <summary>
    /// Haitan quest docs + NPC/story dialogue lines (ported from original, translated to RU).
    /// </summary>
    public static class OriginalDialogueCatalog
    {
        static OriginalDialogueCatalogFile data;
        static bool loaded;

        public static void EnsureLoaded()
        {
            if (loaded)
                return;
            var asset = Resources.Load<TextAsset>("OriginalMap/HaitanDialogues");
            if (asset == null)
                throw new Exception("Missing Resources/OriginalMap/HaitanDialogues.json");
            data = JsonUtility.FromJson<OriginalDialogueCatalogFile>(asset.text);
            data.quests ??= Array.Empty<OriginalQuestDoc>();
            data.stories ??= Array.Empty<OriginalStoryDef>();
            data.npcGreetings ??= Array.Empty<OriginalNpcGreeting>();
            loaded = true;
            Debug.Log("LOE_ORIGINAL_DIALOGUES loaded quests=" + data.quests.Length +
                      " stories=" + data.stories.Length +
                      " greetings=" + data.npcGreetings.Length +
                      " lang=" + data.language);
        }

        public static OriginalQuestDoc QuestDoc(int questId)
        {
            EnsureLoaded();
            for (int i = 0; i < data.quests.Length; i++)
                if (data.quests[i] != null && data.quests[i].id == questId)
                    return data.quests[i];
            return null;
        }

        public static string QuestName(int questId)
        {
            var d = QuestDoc(questId);
            return d != null && !string.IsNullOrEmpty(d.name) ? d.name : null;
        }

        public static string QuestSummary(int questId)
        {
            var d = QuestDoc(questId);
            return d != null ? d.summary ?? "" : "";
        }

        public static string QuestObjectiveText(int questId)
        {
            var d = QuestDoc(questId);
            if (d == null)
                return null;
            string target = string.IsNullOrEmpty(d.goalTarget) ? "" : d.goalTarget;
            if (!string.IsNullOrEmpty(d.goalFormat) && d.goalFormat.Contains("%s"))
                return d.goalFormat.Replace("%s", target);
            if (!string.IsNullOrEmpty(d.summary))
                return d.summary;
            return target;
        }

        public static int AcceptStoryId(int questId)
        {
            var d = QuestDoc(questId);
            return d != null ? d.acceptStoryId : 0;
        }

        public static string[] StoryLines(int storyId)
        {
            EnsureLoaded();
            if (storyId <= 0)
                return Array.Empty<string>();
            for (int i = 0; i < data.stories.Length; i++)
            {
                var s = data.stories[i];
                if (s != null && s.storyId == storyId && s.lines != null && s.lines.Length > 0)
                    return s.lines;
            }
            return Array.Empty<string>();
        }

        public static string NpcGreeting(long npcId)
        {
            EnsureLoaded();
            for (int i = 0; i < data.npcGreetings.Length; i++)
            {
                var g = data.npcGreetings[i];
                if (g != null && g.npcId == npcId && !string.IsNullOrEmpty(g.text))
                    return g.text;
            }
            return "Приветствую, странник! Чем могу помочь?";
        }

        /// <summary>
        /// Lines shown when talking to finishNpc for the active quest.
        /// Prefer acceptStory dialogue; fall back to summary + greeting.
        /// </summary>
        public static string[] LinesForQuestTalk(int questId, long npcId)
        {
            int storyId = AcceptStoryId(questId);
            var lines = StoryLines(storyId);
            if (lines.Length > 0)
                return lines;

            var summary = QuestSummary(questId);
            if (!string.IsNullOrEmpty(summary))
                return new[] { summary };

            return new[] { NpcGreeting(npcId) };
        }
    }
}
