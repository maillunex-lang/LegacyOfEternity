using System;
using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>Quest kinds from Darklord QuestConfig / TitanQuest.</summary>
    public static class OriginalQuestKind
    {
        public const int Gossip = 1;
        public const int KillMonster = 2;
        public const int Gather = 4;
    }

    [Serializable]
    public sealed class OriginalQuestDef
    {
        public int id;
        public string name;
        public int kind;
        public int minLevel;
        public int afterQuestId;
        public long finishNpcId;
        public string questGoals;
        public long goalId;
        public int goalCount;
        public bool autoAccept;
        public int expReward;
        public int moneyReward;
        public string prof1Reward;
        public string prof2Reward;
        public string prof3Reward;
        public string prof4Reward;
        public string otherReward;
    }

    [Serializable]
    public sealed class OriginalQuestCatalogFile
    {
        public string source;
        public long mapId;
        public OriginalQuestDef[] quests;
    }

    public static class OriginalQuestCatalog
    {
        static OriginalQuestDef[] quests = Array.Empty<OriginalQuestDef>();
        static bool loaded;

        public static void EnsureLoaded()
        {
            if (loaded)
                return;
            var asset = Resources.Load<TextAsset>("OriginalMap/HaitanQuests");
            if (asset == null)
                throw new Exception("Missing Resources/OriginalMap/HaitanQuests.json");
            var file = JsonUtility.FromJson<OriginalQuestCatalogFile>(asset.text);
            quests = file?.quests ?? Array.Empty<OriginalQuestDef>();
            loaded = true;
            Debug.Log("LOE_ORIGINAL_QUESTS loaded=" + quests.Length);
        }

        public static OriginalQuestDef Get(int questId)
        {
            EnsureLoaded();
            for (int i = 0; i < quests.Length; i++)
                if (quests[i] != null && quests[i].id == questId)
                    return quests[i];
            return null;
        }

        public static string Title(int questId)
        {
            var docName = OriginalDialogueCatalog.QuestName(questId);
            if (!string.IsNullOrEmpty(docName))
                return docName;
            var q = Get(questId);
            return q != null ? q.name : "Нет активного задания";
        }

        public static string Objective(int questId)
        {
            var docObj = OriginalDialogueCatalog.QuestObjectiveText(questId);
            if (!string.IsNullOrEmpty(docObj))
                return docObj;
            var q = Get(questId);
            if (q == null)
                return "Исследуйте побережье";
            switch (q.kind)
            {
                case OriginalQuestKind.Gossip:
                    return q.finishNpcId != 0
                        ? "Поговорите с " + OriginalModelCatalog.NpcName(q.finishNpcId)
                        : "Продолжайте задание";
                case OriginalQuestKind.Gather:
                    return "Соберите объект " + q.goalId + " ×" + q.goalCount;
                case OriginalQuestKind.KillMonster:
                    return "Убейте монстров " + q.goalId + " ×" + q.goalCount
                           + (q.finishNpcId != 0 ? " и сдайте " + OriginalModelCatalog.NpcName(q.finishNpcId) : "");
                default:
                    return q.name;
            }
        }

        public static bool NeedsNpc(OriginalQuestDef q, long npcId)
        {
            if (q == null || npcId == 0 || q.finishNpcId != npcId)
                return false;
            if (q.kind == OriginalQuestKind.Gossip)
                return true;
            // Kill/gather with finishNpc: talk only after goal progress is met.
            return false;
        }

        public static bool GoalComplete(Offline.OfflineDemoStore.Hero hero, OriginalQuestDef q)
        {
            if (hero == null || q == null)
                return false;
            if (q.kind == OriginalQuestKind.Gossip)
                return true;
            return q.goalCount <= 0 || hero.questProgress >= q.goalCount;
        }
    }
}
