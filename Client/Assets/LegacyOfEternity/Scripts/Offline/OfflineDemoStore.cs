using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Fully offline account/hero/quest store for Android demo (no network).
    /// Ported from LunexUfa prototype and adapted to four LoE classes.
    /// </summary>
    public static class OfflineDemoStore
    {
        public static readonly string[] ClassIds = { "warrior", "assassin", "mage", "archer" };
        public static readonly string[] ClassLabels = { "Воин", "Ассасин", "Маг", "Лучник" };

        [Serializable]
        public class Account
        {
            public string id;
            public string username;
            public string password;
        }

        [Serializable]
        public class Hero
        {
            public string id;
            public string name;
            public string worldId;
            public string heroClass;
            public int experience;
            public int money;
            public int questId;
            public int questProgress;
            public int questGoalCount;
        }

        [Serializable]
        public class WorldInfo
        {
            public string id;
            public string name;
        }

        [Serializable]
        class Db
        {
            public Account[] accounts = new Account[0];
            public Hero[] heroes = new Hero[0];
        }

        static readonly WorldInfo[] Worlds =
        {
            new WorldInfo { id = "world-coastal-1", name = "Побережье Хайтен (оффлайн)" }
        };

        static string PathFile => Path.Combine(Application.persistentDataPath, "loe_offline_demo.json");

        static Db Load()
        {
            if (!File.Exists(PathFile))
                return new Db();
            try
            {
                return JsonUtility.FromJson<Db>(File.ReadAllText(PathFile, Encoding.UTF8)) ?? new Db();
            }
            catch
            {
                return new Db();
            }
        }

        static void Save(Db db) => File.WriteAllText(PathFile, JsonUtility.ToJson(db, true), Encoding.UTF8);

        public static bool Register(string username, string password, out string error)
        {
            error = null;
            username = (username ?? "").Trim();
            if (username.Length < 2 || (password ?? "").Length < 4)
            {
                error = "invalid_credentials";
                return false;
            }
            var db = Load();
            for (int i = 0; i < db.accounts.Length; i++)
            {
                if (string.Equals(db.accounts[i].username, username, StringComparison.OrdinalIgnoreCase))
                {
                    error = "account_exists";
                    return false;
                }
            }
            var list = new List<Account>(db.accounts)
            {
                new Account
                {
                    id = Guid.NewGuid().ToString("N"),
                    username = username,
                    password = password
                }
            };
            db.accounts = list.ToArray();
            Save(db);
            return true;
        }

        public static string Login(string username, string password, out string error)
        {
            error = null;
            var db = Load();
            for (int i = 0; i < db.accounts.Length; i++)
            {
                var a = db.accounts[i];
                if (string.Equals(a.username, username, StringComparison.OrdinalIgnoreCase) && a.password == password)
                    return "offline-" + a.id;
            }
            error = "invalid_credentials";
            return null;
        }

        public static WorldInfo[] ListWorlds() => Worlds;

        public static Hero[] ListHeroes(string accountToken, string worldId)
        {
            string accountId = AccountIdFromToken(accountToken);
            var db = Load();
            var result = new List<Hero>();
            for (int i = 0; i < db.heroes.Length; i++)
            {
                var h = db.heroes[i];
                if (h.worldId == worldId && h.id != null && h.id.StartsWith(accountId + ":", StringComparison.Ordinal))
                    result.Add(h);
            }
            return result.ToArray();
        }

        public static Hero CreateHero(string accountToken, string worldId, string name, string heroClass, out string error)
        {
            error = null;
            name = (name ?? "").Trim();
            if (name.Length < 2 || name.Length > 24)
            {
                error = "invalid_hero";
                return null;
            }
            if (Array.IndexOf(ClassIds, heroClass) < 0)
                heroClass = ClassIds[0];
            string accountId = AccountIdFromToken(accountToken);
            var db = Load();
            for (int i = 0; i < db.heroes.Length; i++)
            {
                if (db.heroes[i].worldId == worldId &&
                    string.Equals(db.heroes[i].name, name, StringComparison.OrdinalIgnoreCase))
                {
                    error = "hero_exists";
                    return null;
                }
            }
            var hero = new Hero
            {
                id = accountId + ":" + Guid.NewGuid().ToString("N"),
                name = name,
                worldId = worldId,
                heroClass = heroClass,
                experience = 0,
                money = 0,
                questId = 1001701,
                questProgress = 0,
                questGoalCount = 0
            };
            var heroes = new List<Hero>(db.heroes) { hero };
            db.heroes = heroes.ToArray();
            Save(db);
            return Clone(hero);
        }

        public static Hero Enter(string accountToken, string worldId, string heroId, out string error)
        {
            error = null;
            string accountId = AccountIdFromToken(accountToken);
            var db = Load();
            for (int i = 0; i < db.heroes.Length; i++)
            {
                var h = db.heroes[i];
                if (h.id == heroId && h.worldId == worldId &&
                    h.id.StartsWith(accountId + ":", StringComparison.Ordinal))
                    return Clone(h);
            }
            error = "invalid_hero";
            return null;
        }

        public static void SaveHero(Hero hero)
        {
            if (hero == null || string.IsNullOrEmpty(hero.id))
                return;
            var db = Load();
            for (int i = 0; i < db.heroes.Length; i++)
            {
                if (db.heroes[i].id == hero.id)
                {
                    db.heroes[i] = Clone(hero);
                    Save(db);
                    return;
                }
            }
        }

        /// <summary>
        /// Haitan-style offline chain on CoastalPrototype:
        /// talk (Tias) → gather scrap → kill x3 → turn in to Tias.
        /// </summary>
        public static void ApplyQuestEvent(Hero hero, string type)
        {
            if (hero == null)
                return;
            if (hero.questId == 1001701 && type == "talk")
            {
                hero.experience += 1040;
                hero.money += 1000;
                hero.questId = 1001702;
                hero.questProgress = 0;
                hero.questGoalCount = 1;
            }
            else if (hero.questId == 1001702 && type == "gather")
            {
                hero.questProgress = 1;
                hero.experience += 550;
                hero.money += 1050;
                hero.questId = 1001703;
                hero.questProgress = 0;
                hero.questGoalCount = 3;
            }
            else if (hero.questId == 1001703 && type == "kill")
            {
                hero.questProgress = Mathf.Min(hero.questProgress + 1, 3);
                if (hero.questProgress >= 3)
                {
                    hero.experience += 550;
                    hero.money += 1050;
                    hero.questId = 1001704;
                    hero.questProgress = 0;
                    hero.questGoalCount = 0;
                }
            }
            else if (hero.questId == 1001704 && type == "turnin")
            {
                hero.experience += 800;
                hero.money += 1200;
                hero.questId = 0;
                hero.questProgress = 0;
                hero.questGoalCount = 0;
            }
            SaveHero(hero);
        }

        public static string QuestTitle(int questId)
        {
            switch (questId)
            {
                case 1001701: return "Близкая опасность (1)";
                case 1001702: return "Близкая опасность (2)";
                case 1001703: return "Близкая опасность (3)";
                case 1001704: return "Близкая опасность (итог)";
                default: return "Нет активного задания";
            }
        }

        public static string QuestObjective(int questId)
        {
            switch (questId)
            {
                case 1001701: return "Поговорите с Тиасом";
                case 1001702: return "Соберите прибрежный хлам";
                case 1001703: return "Победите трёх противников";
                case 1001704: return "Вернитесь к Тиасу и сдайте задание";
                default: return "Исследуйте побережье";
            }
        }

        public static int ClassIndex(string heroClass)
        {
            int idx = Array.IndexOf(ClassIds, heroClass);
            return idx < 0 ? 0 : idx;
        }

        static string AccountIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return "";
            return token.StartsWith("offline-", StringComparison.Ordinal)
                ? token.Substring("offline-".Length)
                : token;
        }

        static Hero Clone(Hero src) => JsonUtility.FromJson<Hero>(JsonUtility.ToJson(src));
    }
}
