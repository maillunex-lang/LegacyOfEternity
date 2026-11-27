using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>Mobile bag panel: list stacks, optional potion use.</summary>
    public sealed class OfflineBagHud : MonoBehaviour
    {
        bool open;
        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle buttonStyle;
        GUIStyle boxStyle;
        bool stylesReady;
        Vector2 scroll;
        OfflineLootFeedback feedback;

        public void BindFeedback(OfflineLootFeedback lootFeedback)
        {
            feedback = lootFeedback;
        }

        void EnsureStyles()
        {
            if (stylesReady)
                return;
            float scale = Mathf.Clamp(Screen.height / 640f, 1.1f, 2.4f);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(28f * scale),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.82f, 0.40f, 1f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(24f * scale),
                normal = { textColor = Color.white },
                wordWrap = true
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(24f * scale),
                fontStyle = FontStyle.Bold,
                fixedHeight = 56f * scale
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
            var hero = OfflineSession.Hero;
            OfflineDemoStore.EnsureInventory(hero);

            float bagBtnW = 150f * scale;
            float bagBtnH = 56f * scale;
            var bagBtn = new Rect(Screen.width - bagBtnW - 16f, 16f, bagBtnW, bagBtnH);
            if (GUI.Button(bagBtn, open ? "Закрыть" : "Сумка", buttonStyle))
                open = !open;

            if (!open)
                return;

            float panelW = Mathf.Min(Screen.width - 40f, 520f * scale);
            float panelH = Mathf.Min(Screen.height - 120f * scale, 480f * scale);
            var panel = new Rect((Screen.width - panelW) * 0.5f, 90f * scale, panelW, panelH);
            GUI.Box(panel, GUIContent.none, boxStyle);
            GUILayout.BeginArea(new Rect(panel.x + 12f, panel.y + 12f, panel.width - 24f, panel.height - 24f));
            GUILayout.Label("Сумка", titleStyle);
            GUILayout.Label("Монеты: " + hero.money, bodyStyle);

            var inv = hero.inventory;
            if (inv == null || inv.Length == 0)
            {
                GUILayout.Label("Пусто — убейте монстров или соберите хлам.", bodyStyle);
            }
            else
            {
                scroll = GUILayout.BeginScrollView(scroll);
                for (int i = 0; i < inv.Length; i++)
                {
                    var stack = inv[i];
                    if (stack == null || stack.count <= 0)
                        continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(OfflineDemoStore.ItemDisplayName(stack.id) + "  ×" + stack.count, bodyStyle, GUILayout.ExpandWidth(true));
                    if (stack.id == OfflineDemoStore.ItemPotion &&
                        GUILayout.Button("Выпить", buttonStyle, GUILayout.Width(140f * scale)))
                    {
                        if (OfflineDemoStore.TryConsumeItem(hero, OfflineDemoStore.ItemPotion, 1))
                        {
                            if (feedback != null)
                                feedback.Push("Зелье выпито (+фиктивное лечение)");
                        }
                    }
                    else if (IsQuestUseItem(hero, stack.id) &&
                             GUILayout.Button("Использовать", buttonStyle, GUILayout.Width(160f * scale)))
                    {
                        long itemId;
                        if (long.TryParse(stack.id, out itemId))
                        {
                            OfflineDemoStore.ApplyQuestEvent(hero, "use", itemId);
                            if (feedback != null)
                                feedback.Push("Использовано: " + OfflineDemoStore.ItemDisplayName(stack.id));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }

            if (GUILayout.Button("Закрыть сумку", buttonStyle))
                open = false;
            GUILayout.EndArea();
        }

        static bool IsQuestUseItem(OfflineDemoStore.Hero hero, string itemId)
        {
            if (hero == null || string.IsNullOrEmpty(itemId))
                return false;
            var q = LegacyOfEternity.World.OriginalQuestCatalog.Get(hero.questId);
            if (q == null || q.kind != LegacyOfEternity.World.OriginalQuestKind.UseItem)
                return false;
            long need = q.useItemId != 0 ? q.useItemId : q.goalId;
            long parsed;
            return need != 0 && long.TryParse(itemId, out parsed) && parsed == need;
        }
    }
}
