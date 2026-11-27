using UnityEngine;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Offline
{
    /// <summary>Large mobile quest HUD for offline Haitan-style chain on CoastalPrototype.</summary>
    public sealed class OfflineQuestHud : MonoBehaviour
    {
        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle buttonStyle;
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
            int titleSize = Mathf.RoundToInt(26f * scale);
            int bodySize = Mathf.RoundToInt(22f * scale);
            int buttonSize = Mathf.RoundToInt(24f * scale);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = titleSize,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.82f, 0.40f, 1f) },
                wordWrap = true
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = bodySize,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = buttonSize,
                fontStyle = FontStyle.Bold,
                fixedHeight = 56f * scale,
                alignment = TextAnchor.MiddleCenter
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
            float height = 250f * scale;
            var hero = OfflineSession.Hero;

            GUILayout.BeginArea(new Rect(16f, 16f, width, height), boxStyle);
            GUILayout.Label(hero.name + " · " + OfflineDemoStore.ClassLabels[OfflineDemoStore.ClassIndex(hero.heroClass)], titleStyle);
            GUILayout.Label("Опыт " + hero.experience + "   Монеты " + hero.money, bodyStyle);
            GUILayout.Label(OfflineDemoStore.QuestTitle(hero.questId) + " · " + OfflineDemoStore.QuestObjective(hero.questId), bodyStyle);
            if (hero.questGoalCount > 0)
                GUILayout.Label("Прогресс " + hero.questProgress + "/" + hero.questGoalCount, bodyStyle);
            if (hero.questId == 1001701 && GUILayout.Button("Поговорить с проводником", buttonStyle))
                OfflineDemoStore.ApplyQuestEvent(hero, "talk");
            if (hero.questId == 1001702 && GUILayout.Button("Собрать на берегу", buttonStyle))
                OfflineDemoStore.ApplyQuestEvent(hero, "gather");
            GUILayout.EndArea();
        }
    }
}
