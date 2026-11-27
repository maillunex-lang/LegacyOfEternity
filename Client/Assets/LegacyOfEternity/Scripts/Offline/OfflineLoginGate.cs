using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LegacyOfEternity.Player;
using LegacyOfEternity.Mobile;
using LoeCamera = LegacyOfEternity.CameraSystem.OrbitFollowCamera;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Mobile login gate with large original-scale UI (1136x640 reference).
    /// Built as runtime uGUI so Android touch targets stay readable.
    /// </summary>
    public sealed class OfflineLoginGate : MonoBehaviour
    {
        enum Step { Account, Worlds, Heroes }

        string username = "";
        string password = "";
        string heroName = "";
        string status = "";
        string token;
        OfflineDemoStore.WorldInfo[] worlds;
        OfflineDemoStore.WorldInfo selectedWorld;
        OfflineDemoStore.Hero[] heroes;
        int selectedClass;
        Step step = Step.Account;
        bool gameplayUnlocked;

        Behaviour[] disabledBehaviours = Array.Empty<Behaviour>();
        Canvas[] disabledCanvases = Array.Empty<Canvas>();
        bool eventSystemWasEnabled;

        Canvas uiCanvas;
        RectTransform panel;
        Text titleLabel;
        Text statusLabel;
        readonly List<GameObject> dynamicRows = new List<GameObject>();
        InputField userField;
        InputField passField;
        InputField heroField;
        Font uiFont;

        static readonly Color PanelColor = new Color(0.06f, 0.10f, 0.16f, 0.94f);
        static readonly Color Accent = new Color(0.85f, 0.70f, 0.32f, 1f);
        static readonly Color ButtonColor = new Color(0.12f, 0.22f, 0.34f, 1f);
        static readonly Color ButtonHot = new Color(0.18f, 0.34f, 0.48f, 1f);
        static readonly Color FieldColor = new Color(0.04f, 0.07f, 0.12f, 0.96f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "CoastalPrototype")
                return;
            if (FindFirstObjectByType<OfflineLoginGate>() != null)
                return;
            var go = new GameObject("LOE_OfflineLoginGate");
            DontDestroyOnLoad(go);
            go.AddComponent<OfflineLoginGate>();
        }

        void Awake()
        {
            OfflineSession.Clear();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FreezeGameplay(true);
            BuildUi();
            ShowAccount();
        }

        void OnDestroy()
        {
            if (!gameplayUnlocked)
                FreezeGameplay(false);
            if (uiCanvas != null)
                Destroy(uiCanvas.gameObject);
        }

        void FreezeGameplay(bool freeze)
        {
            if (freeze)
            {
                var list = new List<Behaviour>();
                foreach (var motor in FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None))
                    list.Add(motor);
                foreach (var combat in FindObjectsByType<PrototypeCombat>(FindObjectsSortMode.None))
                    list.Add(combat);
                foreach (var auto in FindObjectsByType<AutoCombatController>(FindObjectsSortMode.None))
                    list.Add(auto);
                foreach (var joy in FindObjectsByType<MobileJoystick>(FindObjectsSortMode.None))
                    list.Add(joy);
                foreach (var cam in FindObjectsByType<LoeCamera>(FindObjectsSortMode.None))
                    list.Add(cam);
                disabledBehaviours = list.ToArray();
                for (int i = 0; i < disabledBehaviours.Length; i++)
                    if (disabledBehaviours[i] != null)
                        disabledBehaviours[i].enabled = false;

                disabledCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                for (int i = 0; i < disabledCanvases.Length; i++)
                    if (disabledCanvases[i] != null && disabledCanvases[i] != uiCanvas)
                        disabledCanvases[i].enabled = false;

                var es = FindFirstObjectByType<EventSystem>();
                if (es != null)
                {
                    eventSystemWasEnabled = es.enabled;
                    es.enabled = true;
                }
            }
            else
            {
                for (int i = 0; i < disabledBehaviours.Length; i++)
                    if (disabledBehaviours[i] != null)
                        disabledBehaviours[i].enabled = true;
                for (int i = 0; i < disabledCanvases.Length; i++)
                    if (disabledCanvases[i] != null)
                        disabledCanvases[i].enabled = true;
                var es = FindFirstObjectByType<EventSystem>();
                if (es != null)
                    es.enabled = eventSystemWasEnabled || true;
            }
        }

        void BuildUi()
        {
            EnsureEventSystem();

            var root = new GameObject("LOE_OfflineLoginCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            uiCanvas = root.GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 5000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1136f, 640f);
            scaler.matchWidthOrHeight = 0.5f;
            DontDestroyOnLoad(root);

            var dim = CreateImage(root.transform, "Dim", new Color(0f, 0f, 0f, 0.55f));
            Stretch(dim.rectTransform);

            var panelGo = CreateImage(root.transform, "Panel", PanelColor).gameObject;
            panel = panelGo.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(620f, 560f);

            var border = CreateImage(panel, "Border", new Color(Accent.r, Accent.g, Accent.b, 0.35f));
            Stretch(border.rectTransform, -4f);
            var inner = CreateImage(panel, "Inner", PanelColor);
            Stretch(inner.rectTransform, -8f);

            titleLabel = CreateText(panel, "Title", "LEGACY OF ETERNITY", 34, FontStyle.Bold, Accent);
            Place(titleLabel.rectTransform, 0f, -28f, 560f, 44f);

            var subtitle = CreateText(panel, "Subtitle", "Оффлайн вход", 22, FontStyle.Normal, Color.white);
            Place(subtitle.rectTransform, 0f, -66f, 560f, 30f);

            statusLabel = CreateText(panel, "Status", "", 20, FontStyle.Normal, new Color(1f, 0.82f, 0.45f, 1f));
            Place(statusLabel.rectTransform, 0f, -98f, 560f, 36f);
        }

        void ClearDynamic()
        {
            for (int i = 0; i < dynamicRows.Count; i++)
                if (dynamicRows[i] != null)
                    Destroy(dynamicRows[i]);
            dynamicRows.Clear();
            userField = null;
            passField = null;
            heroField = null;
        }

        void ShowAccount()
        {
            step = Step.Account;
            ClearDynamic();
            titleLabel.text = "LEGACY OF ETERNITY";
            SetStatus(status);

            float y = -150f;
            dynamicRows.Add(CreateLabel("Логин", y));
            y -= 36f;
            userField = CreateInput("User", y, username, false);
            y -= 78f;
            dynamicRows.Add(CreateLabel("Пароль", y));
            y -= 36f;
            passField = CreateInput("Pass", y, password, true);
            y -= 92f;
            dynamicRows.Add(CreateButton("Зарегистрироваться", y, OnRegister).gameObject);
            y -= 78f;
            dynamicRows.Add(CreateButton("Войти", y, OnLogin).gameObject);
        }

        void ShowWorlds()
        {
            step = Step.Worlds;
            ClearDynamic();
            titleLabel.text = "Выбор мира";
            SetStatus("");
            if (worlds == null)
                worlds = OfflineDemoStore.ListWorlds();

            float y = -150f;
            for (int i = 0; i < worlds.Length; i++)
            {
                var world = worlds[i];
                dynamicRows.Add(CreateButton(world.name, y, () =>
                {
                    selectedWorld = world;
                    heroes = OfflineDemoStore.ListHeroes(token, world.id);
                    ShowHeroes();
                }).gameObject);
                y -= 78f;
            }
            dynamicRows.Add(CreateButton("Выйти из аккаунта", y, () =>
            {
                token = null;
                worlds = null;
                selectedWorld = null;
                heroes = null;
                status = "";
                ShowAccount();
            }).gameObject);
        }

        void ShowHeroes()
        {
            step = Step.Heroes;
            ClearDynamic();
            titleLabel.text = selectedWorld.name;
            SetStatus(status);

            float y = -140f;
            if (heroes != null)
            {
                for (int i = 0; i < heroes.Length; i++)
                {
                    var hero = heroes[i];
                    int idx = OfflineDemoStore.ClassIndex(hero.heroClass);
                    string label = hero.name + "  ·  " + OfflineDemoStore.ClassLabels[idx];
                    dynamicRows.Add(CreateButton(label, y, () => EnterHero(hero.id)).gameObject);
                    y -= 72f;
                }
            }

            dynamicRows.Add(CreateLabel("Новый герой", y));
            y -= 34f;
            heroField = CreateInput("HeroName", y, heroName, false);
            y -= 78f;

            dynamicRows.Add(CreateClassRow(y).gameObject);
            y -= 78f;
            dynamicRows.Add(CreateButton("Создать героя", y, OnCreateHero).gameObject);
            y -= 78f;
            dynamicRows.Add(CreateButton("К списку миров", y, () =>
            {
                selectedWorld = null;
                status = "";
                ShowWorlds();
            }).gameObject);
        }

        void OnRegister()
        {
            PullFields();
            string err;
            status = OfflineDemoStore.Register(username, password, out err)
                ? "Аккаунт создан. Войдите."
                : ErrorLabel(err);
            SetStatus(status);
        }

        void OnLogin()
        {
            PullFields();
            string err;
            token = OfflineDemoStore.Login(username, password, out err);
            if (token == null)
            {
                status = ErrorLabel(err);
                SetStatus(status);
                return;
            }
            password = "";
            status = "";
            worlds = OfflineDemoStore.ListWorlds();
            ShowWorlds();
        }

        void OnCreateHero()
        {
            PullFields();
            string err;
            var created = OfflineDemoStore.CreateHero(
                token, selectedWorld.id, heroName, OfflineDemoStore.ClassIds[selectedClass], out err);
            if (created == null)
            {
                status = ErrorLabel(err);
                SetStatus(status);
                return;
            }
            heroName = "";
            heroes = OfflineDemoStore.ListHeroes(token, selectedWorld.id);
            status = "Герой создан — нажмите на него, чтобы войти.";
            ShowHeroes();
        }

        void EnterHero(string heroId)
        {
            string err;
            var entered = OfflineDemoStore.Enter(token, selectedWorld.id, heroId, out err);
            if (entered == null)
            {
                status = ErrorLabel(err);
                SetStatus(status);
                return;
            }
            UnlockGameplay(entered);
        }

        void PullFields()
        {
            if (userField != null) username = userField.text ?? "";
            if (passField != null) password = passField.text ?? "";
            if (heroField != null) heroName = heroField.text ?? "";
        }

        void UnlockGameplay(OfflineDemoStore.Hero hero)
        {
            OfflineSession.Enter(token, hero);
            var selector = FindFirstObjectByType<CharacterClassSelector>();
            if (selector != null)
                selector.SelectClass(OfflineDemoStore.ClassIndex(hero.heroClass));
            gameplayUnlocked = true;
            if (uiCanvas != null)
                Destroy(uiCanvas.gameObject);
            uiCanvas = null;
            FreezeGameplay(false);
            if (GetComponent<OfflineQuestHud>() == null)
                gameObject.AddComponent<OfflineQuestHud>();
            enabled = false;
        }

        void SetStatus(string text)
        {
            if (statusLabel != null)
                statusLabel.text = text ?? "";
        }

        static string ErrorLabel(string error)
        {
            switch (error)
            {
                case "invalid_credentials": return "Проверьте логин и пароль (пароль от 4 символов).";
                case "account_exists": return "Аккаунт с таким логином уже есть.";
                case "invalid_hero": return "Имя героя: 2–24 буквы или цифры.";
                case "hero_exists": return "Это имя уже занято.";
                default: return "Не удалось завершить запрос. " + error;
            }
        }

        GameObject CreateLabel(string text, float y)
        {
            var t = CreateText(panel, "Label", text, 22, FontStyle.Bold, new Color(0.85f, 0.9f, 1f, 1f));
            Place(t.rectTransform, 0f, y, 520f, 30f);
            t.alignment = TextAnchor.MiddleLeft;
            return t.gameObject;
        }

        InputField CreateInput(string name, float y, string value, bool passwordMode)
        {
            var go = CreateImage(panel, name, FieldColor).gameObject;
            var rt = go.GetComponent<RectTransform>();
            Place(rt, 0f, y, 520f, 64f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(Accent.r, Accent.g, Accent.b, 0.45f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = uiFont;
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            Stretch(text.rectTransform, -18f, -10f);

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderGo.transform.SetParent(go.transform, false);
            var placeholder = placeholderGo.GetComponent<Text>();
            placeholder.font = uiFont;
            placeholder.fontSize = 26;
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = new Color(1f, 1f, 1f, 0.35f);
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.text = passwordMode ? "••••••••" : "";
            Stretch(placeholder.rectTransform, -18f, -10f);

            var input = go.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholder;
            input.text = value ?? "";
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = passwordMode ? 64 : 32;
            if (passwordMode)
                input.contentType = InputField.ContentType.Password;
            dynamicRows.Add(go);
            return input;
        }

        Button CreateButton(string caption, float y, Action onClick)
        {
            var go = CreateImage(panel, "Btn", ButtonColor).gameObject;
            var rt = go.GetComponent<RectTransform>();
            Place(rt, 0f, y, 520f, 68f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            var label = CreateText(go.transform, "Label", caption, 28, FontStyle.Bold, Color.white);
            Stretch(label.rectTransform);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHot;
            colors.pressedColor = new Color(0.25f, 0.42f, 0.55f, 1f);
            colors.selectedColor = ButtonHot;
            button.colors = colors;
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(() => onClick());
            return button;
        }

        RectTransform CreateClassRow(float y)
        {
            var row = new GameObject("Classes", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(panel, false);
            Place(row, 0f, y, 520f, 64f);

            float w = 520f / OfflineDemoStore.ClassLabels.Length;
            for (int i = 0; i < OfflineDemoStore.ClassLabels.Length; i++)
            {
                int index = i;
                bool selected = selectedClass == index;
                var go = CreateImage(row, "Class" + i, selected ? ButtonHot : ButtonColor).gameObject;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(i * w + 2f, 0f);
                rt.sizeDelta = new Vector2(w - 4f, 0f);

                var label = CreateText(go.transform, "L", OfflineDemoStore.ClassLabels[i], 20, FontStyle.Bold, selected ? Accent : Color.white);
                Stretch(label.rectTransform);

                var button = go.AddComponent<Button>();
                button.targetGraphic = go.GetComponent<Image>();
                button.onClick.AddListener(() =>
                {
                    selectedClass = index;
                    PullFields();
                    ShowHeroes();
                });
            }
            return row;
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
        }

        Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        Text CreateText(Transform parent, string name, string value, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = uiFont;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        static void Stretch(RectTransform rt, float inset = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        static void Stretch(RectTransform rt, float xInset, float yInset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(xInset, yInset);
            rt.offsetMax = new Vector2(-xInset, -yInset);
        }
    }
}
