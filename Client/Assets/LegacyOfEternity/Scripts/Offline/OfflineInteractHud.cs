using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>Large mobile interact button when near a relevant offline quest object.</summary>
    public sealed class OfflineInteractHud : MonoBehaviour
    {
        OfflineWorldInteractable[] targets = System.Array.Empty<OfflineWorldInteractable>();
        GUIStyle buttonStyle;
        GUIStyle hintStyle;
        bool stylesReady;

        public void Bind(OfflineWorldInteractable[] interactables)
        {
            targets = interactables ?? System.Array.Empty<OfflineWorldInteractable>();
        }

        void EnsureStyles()
        {
            if (stylesReady)
                return;
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(28f * scale),
                fontStyle = FontStyle.Bold,
                fixedHeight = 72f * scale,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(22f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.9f, 0.45f, 1f) }
            };
            stylesReady = true;
        }

        OfflineWorldInteractable FindActive()
        {
            OfflineWorldInteractable best = null;
            for (int i = 0; i < targets.Length; i++)
            {
                var t = targets[i];
                if (t != null && t.InRange())
                {
                    best = t;
                    break;
                }
            }
            return best;
        }

        void OnGUI()
        {
            if (!OfflineSession.Ready || OfflineSession.Hero == null)
                return;
            EnsureStyles();
            var active = FindActive();
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            float width = Mathf.Min(Screen.width - 40f, 460f * scale);
            float height = 84f * scale;
            var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 28f * scale, width, height);

            if (OfflineDialogueHud.IsOpen || OfflineQuestDungeon.IsRunning)
                return;

            if (active != null)
            {
                if (GUI.Button(rect, active.ActionLabel() + " · " + active.DisplayName, buttonStyle))
                    active.TryInteract();
                return;
            }

            var q = LegacyOfEternity.World.OriginalQuestCatalog.Get(OfflineSession.Hero.questId);
            if (q != null && LegacyOfEternity.World.OriginalQuestCatalog.NeedsActionButton(q)
                && !LegacyOfEternity.World.OriginalQuestCatalog.GoalComplete(OfflineSession.Hero, q))
            {
                string label = ActionLabel(q);
                if (GUI.Button(rect, label, buttonStyle))
                    RunQuestAction(q);
                return;
            }

            string hint = HintForQuest(OfflineSession.Hero.questId);
            if (!string.IsNullOrEmpty(hint))
            {
                var hintRect = new Rect(20f, Screen.height - 56f * scale, Screen.width - 40f, 40f * scale);
                GUI.Label(hintRect, hint, hintStyle);
            }
        }

        static string HintForQuest(int questId)
        {
            if (questId <= 0)
                return "";
            return LegacyOfEternity.World.OriginalQuestCatalog.Objective(questId);
        }

        static string ActionLabel(LegacyOfEternity.World.OriginalQuestDef q)
        {
            switch (q.kind)
            {
                case LegacyOfEternity.World.OriginalQuestKind.UseItem:
                    return "Использовать " + OfflineDemoStore.ItemDisplayName(
                        (q.useItemId != 0 ? q.useItemId : q.goalId).ToString());
                case LegacyOfEternity.World.OriginalQuestKind.Reach:
                    return "Дойти до цели";
                case LegacyOfEternity.World.OriginalQuestKind.Dungeon:
                    return "Войти в подземелье " + q.dungeonId;
                case LegacyOfEternity.World.OriginalQuestKind.MountDungeon:
                    return "Подземелье скакуна " + q.dungeonId;
                default:
                    return "Продолжить задание";
            }
        }

        static void RunQuestAction(LegacyOfEternity.World.OriginalQuestDef q)
        {
            var hero = OfflineSession.Hero;
            if (hero == null || q == null)
                return;
            switch (q.kind)
            {
                case LegacyOfEternity.World.OriginalQuestKind.UseItem:
                    OfflineDemoStore.ApplyQuestEvent(hero, "use",
                        q.useItemId != 0 ? q.useItemId : q.goalId);
                    break;
                case LegacyOfEternity.World.OriginalQuestKind.Reach:
                    OfflineDemoStore.ApplyQuestEvent(hero, "reach", q.positionId);
                    break;
                case LegacyOfEternity.World.OriginalQuestKind.Dungeon:
                case LegacyOfEternity.World.OriginalQuestKind.MountDungeon:
                    if (OfflineQuestDungeon.IsRunning)
                        return;
                    OfflineQuestDungeon.TryEnter((int)q.dungeonId);
                    break;
            }
        }
    }
}
