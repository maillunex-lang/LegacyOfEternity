using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using LegacyOfEternity.Player;
using LoeCamera = LegacyOfEternity.CameraSystem.OrbitFollowCamera;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Blocks CoastalPrototype until offline register → world → hero → enter.
    /// Injected at runtime so the verified combat scene asset stays untouched.
    /// </summary>
    public sealed class OfflineLoginGate : MonoBehaviour
    {
        string username = "";
        string password = "";
        string heroName = "";
        string status = "";
        string token;
        OfflineDemoStore.WorldInfo[] worlds;
        OfflineDemoStore.WorldInfo selectedWorld;
        OfflineDemoStore.Hero[] heroes;
        int selectedClass;
        Vector2 scroll;
        bool gameplayUnlocked;
        Behaviour[] disabledBehaviours = Array.Empty<Behaviour>();
        Canvas[] disabledCanvases = Array.Empty<Canvas>();
        bool eventSystemWasEnabled;

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
            FreezeGameplay(true);
        }

        void OnDestroy()
        {
            if (!gameplayUnlocked)
                FreezeGameplay(false);
        }

        void FreezeGameplay(bool freeze)
        {
            if (freeze)
            {
                var list = new System.Collections.Generic.List<Behaviour>();
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
                    if (disabledCanvases[i] != null)
                        disabledCanvases[i].enabled = false;

                var es = FindFirstObjectByType<EventSystem>();
                if (es != null)
                {
                    eventSystemWasEnabled = es.enabled;
                    es.enabled = false;
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

        void UnlockGameplay(OfflineDemoStore.Hero hero)
        {
            OfflineSession.Enter(token, hero);
            var selector = FindFirstObjectByType<CharacterClassSelector>();
            if (selector != null)
                selector.SelectClass(OfflineDemoStore.ClassIndex(hero.heroClass));
            gameplayUnlocked = true;
            FreezeGameplay(false);
            if (GetComponent<OfflineQuestHud>() == null)
                gameObject.AddComponent<OfflineQuestHud>();
            enabled = false;
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

        void OnGUI()
        {
            if (gameplayUnlocked)
                return;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            float width = Mathf.Min(520f, Screen.width - 24f);
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2f, 16f, width, Screen.height - 32f), GUI.skin.box);
            GUILayout.Label("Legacy of Eternity · оффлайн вход");
            if (!string.IsNullOrEmpty(status))
                GUILayout.Label(status);
            scroll = GUILayout.BeginScrollView(scroll);

            if (string.IsNullOrEmpty(token))
            {
                GUILayout.Label("Логин");
                username = GUILayout.TextField(username, 32);
                GUILayout.Label("Пароль");
                password = GUILayout.PasswordField(password, '*', 128);
                if (GUILayout.Button("Зарегистрироваться", GUILayout.Height(44)))
                {
                    string err;
                    status = OfflineDemoStore.Register(username, password, out err)
                        ? "Аккаунт создан. Войдите."
                        : ErrorLabel(err);
                }
                if (GUILayout.Button("Войти", GUILayout.Height(44)))
                {
                    string err;
                    token = OfflineDemoStore.Login(username, password, out err);
                    if (token == null)
                        status = ErrorLabel(err);
                    else
                    {
                        password = "";
                        status = "";
                        worlds = OfflineDemoStore.ListWorlds();
                    }
                }
            }
            else if (selectedWorld == null)
            {
                GUILayout.Label("Выберите мир");
                if (worlds == null)
                    worlds = OfflineDemoStore.ListWorlds();
                for (int i = 0; i < worlds.Length; i++)
                {
                    var world = worlds[i];
                    if (GUILayout.Button(world.name, GUILayout.Height(44)))
                    {
                        selectedWorld = world;
                        heroes = OfflineDemoStore.ListHeroes(token, world.id);
                    }
                }
                if (GUILayout.Button("Выйти из аккаунта"))
                {
                    token = null;
                    worlds = null;
                }
            }
            else
            {
                GUILayout.Label(selectedWorld.name + " · Герои");
                if (heroes != null)
                {
                    for (int i = 0; i < heroes.Length; i++)
                    {
                        var hero = heroes[i];
                        int idx = OfflineDemoStore.ClassIndex(hero.heroClass);
                        string label = hero.name + " · " + OfflineDemoStore.ClassLabels[idx];
                        if (GUILayout.Button(label, GUILayout.Height(44)))
                        {
                            string err;
                            var entered = OfflineDemoStore.Enter(token, selectedWorld.id, hero.id, out err);
                            if (entered == null)
                                status = ErrorLabel(err);
                            else
                                UnlockGameplay(entered);
                        }
                    }
                }
                GUILayout.Label("Имя нового героя");
                heroName = GUILayout.TextField(heroName, 24);
                selectedClass = GUILayout.SelectionGrid(selectedClass, OfflineDemoStore.ClassLabels, 2);
                if (GUILayout.Button("Создать героя", GUILayout.Height(44)))
                {
                    string err;
                    var created = OfflineDemoStore.CreateHero(
                        token, selectedWorld.id, heroName, OfflineDemoStore.ClassIds[selectedClass], out err);
                    if (created == null)
                        status = ErrorLabel(err);
                    else
                    {
                        heroName = "";
                        heroes = OfflineDemoStore.ListHeroes(token, selectedWorld.id);
                        status = "Герой создан. Войдите им в мир.";
                    }
                }
                if (GUILayout.Button("К списку миров"))
                    selectedWorld = null;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
