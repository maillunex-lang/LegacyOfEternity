using UnityEngine;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Offline
{
    /// <summary>Shows offline quest progress and wires talk/gather/kill on CoastalPrototype.</summary>
    public sealed class OfflineQuestHud : MonoBehaviour
    {
        void OnEnable()
        {
            CombatEnemy.Killed += OnEnemyKilled;
        }

        void OnDisable()
        {
            CombatEnemy.Killed -= OnEnemyKilled;
        }

        void OnEnemyKilled(CombatEnemy enemy)
        {
            if (!OfflineSession.Ready || OfflineSession.Hero == null)
                return;
            OfflineDemoStore.ApplyQuestEvent(OfflineSession.Hero, "kill");
        }

        void OnGUI()
        {
            if (!OfflineSession.Ready || OfflineSession.Hero == null)
                return;
            var hero = OfflineSession.Hero;
            GUILayout.BeginArea(new Rect(12, 12, 420, 170), GUI.skin.box);
            GUILayout.Label(hero.name + " · " + OfflineDemoStore.ClassLabels[OfflineDemoStore.ClassIndex(hero.heroClass)]);
            GUILayout.Label("Опыт " + hero.experience + "  Монеты " + hero.money);
            GUILayout.Label(OfflineDemoStore.QuestTitle(hero.questId) + " · " + OfflineDemoStore.QuestObjective(hero.questId));
            if (hero.questGoalCount > 0)
                GUILayout.Label("Прогресс " + hero.questProgress + "/" + hero.questGoalCount);
            if (hero.questId == 1001701 && GUILayout.Button("Поговорить с проводником", GUILayout.Height(36)))
                OfflineDemoStore.ApplyQuestEvent(hero, "talk");
            if (hero.questId == 1001702 && GUILayout.Button("Собрать на берегу", GUILayout.Height(36)))
                OfflineDemoStore.ApplyQuestEvent(hero, "gather");
            GUILayout.EndArea();
        }
    }
}
