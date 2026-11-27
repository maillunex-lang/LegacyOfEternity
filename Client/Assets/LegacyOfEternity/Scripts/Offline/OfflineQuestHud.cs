using UnityEngine;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Offline
{
    /// <summary>Large mobile quest progress HUD for offline Haitan chain on CoastalPrototype.</summary>
    public sealed class OfflineQuestHud : MonoBehaviour
    {
        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle boxStyle;
        bool stylesReady;

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

        void EnsureStyles()
        {
            if (stylesReady)
                return;
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
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
            boxStyle = new GUIStyle(GUI.skin.box);
            stylesReady = true;
        }

        void OnGUI()
        {
            if (!OfflineSession.Ready || OfflineSession.Hero == null)
                return;
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            float width = Mathf.Min(Screen.width - 24f, 520f * scale);
            float height = 200f * scale;
            var hero = OfflineSession.Hero;

            GUILayout.BeginArea(new Rect(16f, 16f, width, height), boxStyle);
            GUILayout.Label(hero.name + " · " + OfflineDemoStore.ClassLabels[OfflineDemoStore.ClassIndex(hero.heroClass)], titleStyle);
            GUILayout.Label("Опыт " + hero.experience + "   Монеты " + hero.money, bodyStyle);
            GUILayout.Label(OfflineDemoStore.QuestTitle(hero.questId), titleStyle);
            GUILayout.Label(OfflineDemoStore.QuestObjective(hero.questId), bodyStyle);
            if (hero.questGoalCount > 0)
                GUILayout.Label("Прогресс " + hero.questProgress + "/" + hero.questGoalCount, bodyStyle);
            if (hero.questId == 0)
                GUILayout.Label("Цепочка побережья завершена", bodyStyle);
            GUILayout.EndArea();
        }
    }
}
