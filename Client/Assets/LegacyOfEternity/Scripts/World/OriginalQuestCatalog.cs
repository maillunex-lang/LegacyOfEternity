using System;
using UnityEngine;

namespace LegacyOfEternity.World
{
    /// <summary>Quest kinds from Darklord QuestConfig / TitanQuest.</summary>
    public static class OriginalQuestKind
    {
        public const int Gossip = 1;
        public const int KillMonster = 2;
        public const int KillDrop = 3;      // kill monster → quest item drop
        public const int Gather = 4;
        public const int UseItem = 7;       // use item from bag
        public const int Deliver = 8;       // talk / deliver to finishNpc
        public const int Reach = 9;         // reach position id
        public const int Dungeon = 14;      // clear quest dungeon
        public const int MountDungeon = 17; // clear mount dungeon
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
        public long dropItemId;
        public int dropRate;          // /10000
        public long useItemId;
        public long positionId;
        public long dungeonId;
        public string finishRemoveItem;
        public long acceptGetItem;
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
                case OriginalQuestKind.Deliver:
                    return q.finishNpcId != 0
                        ? "Поговорите с " + OriginalModelCatalog.NpcName(q.finishNpcId)
                        : "Продолжайте задание";
                case OriginalQuestKind.Gather:
                    return "Соберите объект " + q.goalId + " ×" + q.goalCount;
                case OriginalQuestKind.KillMonster:
                    return "Убейте монстров " + q.goalId + " ×" + q.goalCount
                           + (q.finishNpcId != 0 ? " и сдайте " + OriginalModelCatalog.NpcName(q.finishNpcId) : "");
                case OriginalQuestKind.KillDrop:
                    return "Добудьте предмет " + q.dropItemId + " ×" + q.goalCount
                           + " (с монстра " + q.goalId + ")";
                case OriginalQuestKind.UseItem:
                    return "Используйте предмет " + q.useItemId;
                case OriginalQuestKind.Reach:
                    return "Дойдите до точки " + q.positionId;
                case OriginalQuestKind.Dungeon:
                    return "Пройдите подземелье " + q.dungeonId;
                case OriginalQuestKind.MountDungeon:
                    return "Пройдите подземелье скакуна " + q.dungeonId;
                default:
                    return q.name;
            }
        }

        public static bool NeedsNpc(OriginalQuestDef q, long npcId)
        {
            if (q == null || npcId == 0 || q.finishNpcId != npcId)
                return false;
            if (q.kind == OriginalQuestKind.Gossip || q.kind == OriginalQuestKind.Deliver)
                return true;
            return false;
        }

        public static bool IsTalkKind(OriginalQuestDef q)
        {
            if (q == null)
                return false;
            return q.kind == OriginalQuestKind.Gossip || q.kind == OriginalQuestKind.Deliver;
        }

        public static bool NeedsActionButton(OriginalQuestDef q)
        {
            if (q == null)
                return false;
            switch (q.kind)
            {
                case OriginalQuestKind.UseItem:
                case OriginalQuestKind.Reach:
                case OriginalQuestKind.Dungeon:
                case OriginalQuestKind.MountDungeon:
                    return true;
                default:
                    return false;
            }
        }

        public static bool GoalComplete(Offline.OfflineDemoStore.Hero hero, OriginalQuestDef q)
        {
            if (hero == null || q == null)
                return false;
            if (IsTalkKind(q))
                return true;
            if (q.kind == OriginalQuestKind.UseItem)
                return hero.questProgress >= Mathf.Max(1, q.goalCount);
            return q.goalCount <= 0 || hero.questProgress >= q.goalCount;
        }
    }
}
