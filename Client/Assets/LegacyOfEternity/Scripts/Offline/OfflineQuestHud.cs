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
        OfflineLootFeedback feedback;

        void Awake()
        {
            feedback = OfflineLootFeedback.Ensure(gameObject);
            var bag = GetComponent<OfflineBagHud>();
            if (bag == null)
                bag = gameObject.AddComponent<OfflineBagHud>();
            bag.BindFeedback(feedback);
        }

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
            var drops = OfflineDemoStore.RollKillLoot(OfflineSession.Hero);
            if (feedback != null)
                feedback.ShowLoot(drops);
            long templateId = enemy != null ? enemy.TemplateId : 0;
            OfflineDemoStore.ApplyQuestEvent(OfflineSession.Hero, "kill", templateId);
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
            float width = Mathf.Min(Screen.width - 190f * scale, 480f * scale);
            float height = 260f * scale;
            var hero = OfflineSession.Hero;
            OfflineDemoStore.EnsureInventory(hero);

            GUILayout.BeginArea(new Rect(16f, 16f, width, height), boxStyle);
            GUILayout.Label(hero.name + " · " + OfflineDemoStore.ClassLabels[OfflineDemoStore.ClassIndex(hero.heroClass)], titleStyle);
            GUILayout.Label("Опыт " + hero.experience + "   Монеты " + hero.money, bodyStyle);
            int slots = hero.inventory != null ? hero.inventory.Length : 0;
            GUILayout.Label("Сумка: " + slots + " вид(ов)", bodyStyle);
            if (GUILayout.Button(OfflineDemoStore.QuestTitle(hero.questId), titleStyle, GUILayout.MinHeight(36f * scale)))
                OfflineQuestNavigate.TryGo();
            GUILayout.Label(OfflineDemoStore.QuestObjective(hero.questId), bodyStyle);
            if (hero.questGoalCount > 0)
                GUILayout.Label("Прогресс " + hero.questProgress + "/" + hero.questGoalCount, bodyStyle);
            if (hero.questId == 0)
                GUILayout.Label("Нет активного задания Haitan", bodyStyle);
            else if (GUILayout.Button("Бежать к цели", GUILayout.Height(44f * scale)))
                OfflineQuestNavigate.TryGo();
            GUILayout.EndArea();
        }
    }
}
