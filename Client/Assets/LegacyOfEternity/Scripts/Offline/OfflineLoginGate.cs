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
    /// Offline login gate styled like original Darklord LoginUI:
    /// full-bleed splash, large fields, ornate start button (1136x640).
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
        RectTransform contentRoot;
        Text titleLabel;
        Text statusLabel;
        readonly List<GameObject> dynamicRows = new List<GameObject>();
        InputField userField;
        InputField passField;
        InputField heroField;
        Font uiFont;

        Sprite sprBg;
        Sprite sprPanel;
        Sprite sprField;
        Sprite sprBtn;
        Sprite sprTitleBar;
        Sprite sprDivider;
        Sprite[] sprClasses = Array.Empty<Sprite>();

        static readonly Color Gold = new Color(1f, 0.86f, 0.45f, 1f);
        static readonly Color SoftWhite = new Color(0.96f, 0.96f, 0.98f, 1f);
        static readonly Color DimPanel = new Color(0.02f, 0.03f, 0.05f, 0.55f);

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
            LoadArt();
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

        void LoadArt()
        {
            sprBg = Resources.Load<Sprite>("OfflineUI/bg_login");
            sprPanel = Resources.Load<Sprite>("OfflineUI/panel_login");
            sprField = Resources.Load<Sprite>("OfflineUI/field_bg");
            sprBtn = Resources.Load<Sprite>("OfflineUI/btn_start");
            sprTitleBar = Resources.Load<Sprite>("OfflineUI/title_bar");
            sprDivider = Resources.Load<Sprite>("OfflineUI/divider");
            sprClasses = new[]
            {
                Resources.Load<Sprite>("OfflineUI/class_1"),
                Resources.Load<Sprite>("OfflineUI/class_2"),
                Resources.Load<Sprite>("OfflineUI/class_3"),
                Resources.Load<Sprite>("OfflineUI/class_4")
            };
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

            // Full-bleed original splash
            var bg = CreateImage(root.transform, "Splash", Color.white);
            Stretch(bg.rectTransform);
            if (sprBg != null)
            {
                bg.sprite = sprBg;
                bg.type = Image.Type.Simple;
                bg.preserveAspect = false;
            }
            else
                bg.color = new Color(0.05f, 0.07f, 0.1f, 1f);

            // Soft vignette so fields stay readable over bright armor
            var veil = CreateImage(root.transform, "Veil", new Color(0f, 0f, 0f, 0.28f));
            Stretch(veil.rectTransform);

            contentRoot = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            contentRoot.SetParent(root.transform, false);
            Stretch(contentRoot);

            // Version + brand
            var version = CreateText(contentRoot, "Version", "Версия 0.6.2", 22, FontStyle.Normal, SoftWhite);
            PlaceTopLeft(version.rectTransform, 24f, -18f, 280f, 32f);

            titleLabel = CreateText(contentRoot, "Title", "LEGACY OF ETERNITY", 30, FontStyle.Bold, Gold);
            PlaceTop(titleLabel.rectTransform, 0f, -16f, 700f, 40f);

            if (sprTitleBar != null)
            {
                var bar = CreateImage(contentRoot, "TitleBar", Color.white);
                bar.sprite = sprTitleBar;
                bar.type = Image.Type.Sliced;
                PlaceTop(bar.rectTransform, 0f, -58f, 520f, 22f);
            }

            statusLabel = CreateText(contentRoot, "Status", "", 22, FontStyle.Normal, Gold);
            PlaceTop(statusLabel.rectTransform, 0f, -86f, 720f, 34f);
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

            // Centered login plate over splash (original proportions, enlarged for touch)
            var plate = CreateImage(contentRoot, "Plate", Color.white);
            if (sprPanel != null)
            {
                plate.sprite = sprPanel;
                plate.type = Image.Type.Sliced;
                plate.color = Color.white;
            }
            else
                plate.color = DimPanel;
            PlaceCenter(plate.rectTransform, 0f, -36f, 560f, 360f);
            dynamicRows.Add(plate.gameObject);

            float y = -54f;
            dynamicRows.Add(CreateLabel(plate.rectTransform, "Логин", y, 480f));
            y -= 40f;
            userField = CreateInput(plate.rectTransform, "User", y, username, false, 480f, 58f);
            y -= 78f;
            dynamicRows.Add(CreateLabel(plate.rectTransform, "Пароль", y, 480f));
            y -= 40f;
            passField = CreateInput(plate.rectTransform, "Pass", y, password, true, 480f, 58f);
            y -= 88f;

            dynamicRows.Add(CreateOrnateButton(plate.rectTransform, "Войти", y, 420f, 72f, OnLogin).gameObject);
            y -= 82f;
            dynamicRows.Add(CreateSecondaryButton(plate.rectTransform, "Зарегистрироваться", y, 420f, 56f, OnRegister).gameObject);
        }

        void ShowWorlds()
        {
            step = Step.Worlds;
            ClearDynamic();
            titleLabel.text = "Выбор мира";
            SetStatus("");
            if (worlds == null)
                worlds = OfflineDemoStore.ListWorlds();

            var plate = CreateImage(contentRoot, "Plate", DimPanel);
            if (sprPanel != null)
            {
                plate.sprite = sprPanel;
                plate.type = Image.Type.Sliced;
                plate.color = Color.white;
            }
            PlaceCenter(plate.rectTransform, 0f, -20f, 560f, 380f);
            dynamicRows.Add(plate.gameObject);

            float y = -50f;
            for (int i = 0; i < worlds.Length; i++)
            {
                var world = worlds[i];
                dynamicRows.Add(CreateOrnateButton(plate.rectTransform, world.name, y, 440f, 68f, () =>
                {
                    selectedWorld = world;
                    heroes = OfflineDemoStore.ListHeroes(token, world.id);
                    ShowHeroes();
                }).gameObject);
                y -= 78f;
            }
            dynamicRows.Add(CreateSecondaryButton(plate.rectTransform, "Выйти из аккаунта", y, 440f, 54f, () =>
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
            titleLabel.text = selectedWorld != null ? selectedWorld.name : "Герои";
            SetStatus(status);

            var plate = CreateImage(contentRoot, "Plate", DimPanel);
            if (sprPanel != null)
            {
                plate.sprite = sprPanel;
                plate.type = Image.Type.Sliced;
                plate.color = Color.white;
            }
            PlaceCenter(plate.rectTransform, 0f, -10f, 620f, 460f);
            dynamicRows.Add(plate.gameObject);

            float y = -40f;
            if (heroes != null)
            {
                for (int i = 0; i < heroes.Length; i++)
                {
                    var hero = heroes[i];
                    int idx = OfflineDemoStore.ClassIndex(hero.heroClass);
                    string label = hero.name + "  ·  " + OfflineDemoStore.ClassLabels[idx];
                    dynamicRows.Add(CreateOrnateButton(plate.rectTransform, label, y, 520f, 64f, () => EnterHero(hero.id)).gameObject);
                    y -= 72f;
                }
            }

            if (sprDivider != null)
            {
                var div = CreateImage(plate.rectTransform, "Div", Color.white);
                div.sprite = sprDivider;
                PlaceTop(div.rectTransform, 0f, y, 420f, 10f);
                dynamicRows.Add(div.gameObject);
                y -= 24f;
            }

            dynamicRows.Add(CreateLabel(plate.rectTransform, "Новый герой", y, 520f));
            y -= 36f;
            heroField = CreateInput(plate.rectTransform, "HeroName", y, heroName, false, 520f, 56f);
            y -= 72f;
            dynamicRows.Add(CreateClassRow(plate.rectTransform, y).gameObject);
            y -= 100f;
            dynamicRows.Add(CreateOrnateButton(plate.rectTransform, "Создать героя", y, 420f, 68f, OnCreateHero).gameObject);
            y -= 76f;
            dynamicRows.Add(CreateSecondaryButton(plate.rectTransform, "К списку миров", y, 420f, 52f, () =>
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
            string tok = OfflineDemoStore.Login(username, password, out err);
            if (string.IsNullOrEmpty(tok))
            {
                status = ErrorLabel(err);
                SetStatus(status);
                return;
            }
            token = tok;
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
            EnterHero(created.id);
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
            Transform heroTransform = selector != null ? selector.transform : null;
            if (heroTransform == null)
                heroTransform = GameObject.Find("LOE Prototype Hero")?.transform;
            OfflineWorldBootstrap.Ensure(heroTransform);
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

        GameObject CreateLabel(Transform parent, string text, float y, float width)
        {
            var t = CreateText(parent, "Label", text, 24, FontStyle.Bold, SoftWhite);
            PlaceTop(t.rectTransform, 0f, y, width, 32f);
            t.alignment = TextAnchor.MiddleLeft;
            return t.gameObject;
        }

        InputField CreateInput(Transform parent, string name, float y, string value, bool passwordMode, float width, float height)
        {
            var go = CreateImage(parent, name, new Color(0.05f, 0.07f, 0.1f, 0.92f)).gameObject;
            var rt = go.GetComponent<RectTransform>();
            PlaceTop(rt, 0f, y, width, height);
            var image = go.GetComponent<Image>();
            if (sprField != null)
            {
                image.sprite = sprField;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = uiFont;
            text.fontSize = 28;
            text.color = SoftWhite;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            Stretch(text.rectTransform, 18f, 8f);

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderGo.transform.SetParent(go.transform, false);
            var placeholder = placeholderGo.GetComponent<Text>();
            placeholder.font = uiFont;
            placeholder.fontSize = 26;
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = new Color(1f, 1f, 1f, 0.35f);
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.text = passwordMode ? "••••••••" : "";
            Stretch(placeholder.rectTransform, 18f, 8f);

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

        Button CreateOrnateButton(Transform parent, string caption, float y, float width, float height, Action onClick)
        {
            var go = CreateImage(parent, "Btn", Color.white).gameObject;
            var rt = go.GetComponent<RectTransform>();
            PlaceTop(rt, 0f, y, width, height);
            var image = go.GetComponent<Image>();
            if (sprBtn != null)
            {
                image.sprite = sprBtn;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.color = Color.white;
            }
            else
                image.color = new Color(0.55f, 0.32f, 0.08f, 1f);

            var label = CreateText(go.transform, "Label", caption, 28, FontStyle.Bold, SoftWhite);
            Stretch(label.rectTransform);
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick());
            return button;
        }

        Button CreateSecondaryButton(Transform parent, string caption, float y, float width, float height, Action onClick)
        {
            var go = CreateImage(parent, "Btn2", new Color(0.08f, 0.1f, 0.14f, 0.88f)).gameObject;
            var rt = go.GetComponent<RectTransform>();
            PlaceTop(rt, 0f, y, width, height);
            if (sprField != null)
            {
                var image = go.GetComponent<Image>();
                image.sprite = sprField;
                image.type = Image.Type.Sliced;
                image.color = new Color(1f, 1f, 1f, 0.85f);
            }

            var label = CreateText(go.transform, "Label", caption, 24, FontStyle.Bold, Gold);
            Stretch(label.rectTransform);

            var button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(() => onClick());
            return button;
        }

        RectTransform CreateClassRow(Transform parent, float y)
        {
            var row = new GameObject("Classes", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(parent, false);
            PlaceTop(row, 0f, y, 520f, 90f);

            int count = OfflineDemoStore.ClassLabels.Length;
            float w = 520f / count;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                bool selected = selectedClass == index;
                var go = CreateImage(row, "Class" + i, selected ? Color.white : new Color(1f, 1f, 1f, 0.55f)).gameObject;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(i * w + 4f, 0f);
                rt.sizeDelta = new Vector2(w - 8f, 0f);

                var image = go.GetComponent<Image>();
                if (sprClasses != null && i < sprClasses.Length && sprClasses[i] != null)
                {
                    image.sprite = sprClasses[i];
                    image.preserveAspect = true;
                    image.color = selected ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                }

                var label = CreateText(go.transform, "L", OfflineDemoStore.ClassLabels[i], 16, FontStyle.Bold, selected ? Gold : SoftWhite);
                label.alignment = TextAnchor.LowerCenter;
                Stretch(label.rectTransform, 2f, 2f);

                var button = go.AddComponent<Button>();
                button.targetGraphic = image;
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
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        Text CreateText(Transform parent, string name, string value, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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

        static void PlaceTop(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        static void PlaceTopLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        static void PlaceCenter(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
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
