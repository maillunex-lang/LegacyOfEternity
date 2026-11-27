using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>
    /// Fully offline account/hero/quest/inventory store for Android demo (no network).
    /// </summary>
    public static class OfflineDemoStore
    {
        public static readonly string[] ClassIds = { "warrior", "assassin", "mage", "archer" };
        public static readonly string[] ClassLabels = { "Воин", "Ассасин", "Маг", "Лучник" };

        public const string ItemScrap = "offline_scrap";
        public const string ItemFang = "offline_fang";
        public const string ItemHide = "offline_hide";
        public const string ItemBone = "offline_bone";
        public const string ItemPotion = "offline_potion";
        public const string ItemOre = "offline_ore";
        public const string ItemCloth = "offline_cloth";
        public const string ItemGem = "offline_gem";

        [Serializable]
        public class Account
        {
            public string id;
            public string username;
            public string password;
        }

        [Serializable]
        public class ItemStack
        {
            public string id;
            public int count;
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
            public ItemStack[] inventory;
        }

        [Serializable]
        public class WorldInfo
        {
            public string id;
            public string name;
        }

        [Serializable]
        public class LootLine
        {
            public string itemId;
            public int count;
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

        static readonly string[] KillLootPool =
        {
            ItemFang, ItemHide, ItemBone, ItemOre, ItemCloth, ItemPotion, ItemGem
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

        public static void EnsureInventory(Hero hero)
        {
            if (hero == null)
                return;
            if (hero.inventory == null)
                hero.inventory = new ItemStack[0];
        }

        public static string ItemDisplayName(string itemId)
        {
            switch (itemId)
            {
                case ItemScrap: return "Прибрежный хлам";
                case ItemFang: return "Клык хищника";
                case ItemHide: return "Шкура зверя";
                case ItemBone: return "Кость";
                case ItemPotion: return "Зелье лечения";
                case ItemOre: return "Рудный осколок";
                case ItemCloth: return "Обрывок ткани";
                case ItemGem: return "Тусклый самоцвет";
                default: return itemId ?? "?";
            }
        }

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
                {
                    EnsureInventory(h);
                    result.Add(h);
                }
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
                questGoalCount = 0,
                inventory = new ItemStack[0]
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
                {
                    EnsureInventory(h);
                    var clone = Clone(h);
                    EnsureInventory(clone);
                    if (h.inventory == null)
                    {
                        h.inventory = new ItemStack[0];
                        Save(db);
                    }
                    return clone;
                }
            }
            error = "invalid_hero";
            return null;
        }

        public static void SaveHero(Hero hero)
        {
            if (hero == null || string.IsNullOrEmpty(hero.id))
                return;
            EnsureInventory(hero);
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

        public static void ApplyLoot(Hero hero, string itemId, int count)
        {
            if (hero == null || string.IsNullOrEmpty(itemId) || count <= 0)
                return;
            EnsureInventory(hero);
            var list = new List<ItemStack>(hero.inventory ?? new ItemStack[0]);
            bool found = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].id == itemId)
                {
                    list[i].count += count;
                    found = true;
                    break;
                }
            }
            if (!found)
                list.Add(new ItemStack { id = itemId, count = count });
            hero.inventory = list.ToArray();
            SaveHero(hero);
        }

        public static bool TryConsumeItem(Hero hero, string itemId, int count)
        {
            if (hero == null || string.IsNullOrEmpty(itemId) || count <= 0)
                return false;
            EnsureInventory(hero);
            var list = new List<ItemStack>(hero.inventory ?? new ItemStack[0]);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null || list[i].id != itemId || list[i].count < count)
                    continue;
                list[i].count -= count;
                if (list[i].count <= 0)
                    list.RemoveAt(i);
                hero.inventory = list.ToArray();
                SaveHero(hero);
                return true;
            }
            return false;
        }

        /// <summary>Roll 1–2 stack drops + small coin bonus for a Coastal kill.</summary>
        public static LootLine[] RollKillLoot(Hero hero)
        {
            if (hero == null)
                return new LootLine[0];
            EnsureInventory(hero);
            var drops = new List<LootLine>();
            int rolls = UnityEngine.Random.Range(1, 3);
            for (int i = 0; i < rolls; i++)
            {
                string id = KillLootPool[UnityEngine.Random.Range(0, KillLootPool.Length)];
                int count = id == ItemPotion || id == ItemGem ? 1 : UnityEngine.Random.Range(1, 3);
                ApplyLoot(hero, id, count);
                drops.Add(new LootLine { itemId = id, count = count });
            }
            int coins = UnityEngine.Random.Range(15, 46);
            hero.money += coins;
            SaveHero(hero);
            drops.Add(new LootLine { itemId = "money", count = coins });
            return drops.ToArray();
        }

        /// <summary>
        /// Original Haitan QuestConfig chain (1001701…): gossip / gather / kill + finishNpc.
        /// payloadId = NPC id, collection id, or monster template id.
        /// </summary>
        public static void ApplyQuestEvent(Hero hero, string type, long payloadId = 0)
        {
            if (hero == null || hero.questId <= 0)
                return;
            EnsureInventory(hero);
            LegacyOfEternity.World.OriginalQuestCatalog.EnsureLoaded();
            var q = LegacyOfEternity.World.OriginalQuestCatalog.Get(hero.questId);
            if (q == null)
                return;

            bool changed = false;
            if (type == "talk" || type == "turnin")
            {
                if (q.finishNpcId != 0 && payloadId != 0 && payloadId != q.finishNpcId)
                    return;
                if (q.kind == LegacyOfEternity.World.OriginalQuestKind.Gossip)
                    changed = CompleteQuest(hero, q);
                else if (LegacyOfEternity.World.OriginalQuestCatalog.GoalComplete(hero, q) && q.finishNpcId != 0)
                    changed = CompleteQuest(hero, q);
            }
            else if (type == "gather" && q.kind == LegacyOfEternity.World.OriginalQuestKind.Gather)
            {
                if (q.goalId != 0 && payloadId != 0 && payloadId != q.goalId)
                    return;
                GrantScrap(hero);
                hero.questProgress = Mathf.Min(hero.questProgress + 1, Mathf.Max(1, q.goalCount));
                hero.questGoalCount = q.goalCount;
                changed = true;
                if (hero.questProgress >= q.goalCount && q.finishNpcId == 0)
                    CompleteQuest(hero, q);
            }
            else if (type == "kill" && q.kind == LegacyOfEternity.World.OriginalQuestKind.KillMonster)
            {
                if (q.goalId != 0 && payloadId != 0 && payloadId != q.goalId)
                    return;
                hero.questProgress = Mathf.Min(hero.questProgress + 1, Mathf.Max(1, q.goalCount));
                hero.questGoalCount = q.goalCount;
                changed = true;
                if (hero.questProgress >= q.goalCount && q.finishNpcId == 0)
                    CompleteQuest(hero, q);
            }

            if (changed)
                SaveHero(hero);
        }

        static void GrantScrap(Hero hero)
        {
            EnsureInventory(hero);
            var list = new List<ItemStack>(hero.inventory);
            bool found = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].id == ItemScrap)
                {
                    list[i].count += 1;
                    found = true;
                    break;
                }
            }
            if (!found)
                list.Add(new ItemStack { id = ItemScrap, count = 1 });
            hero.inventory = list.ToArray();
        }

        static bool CompleteQuest(Hero hero, LegacyOfEternity.World.OriginalQuestDef q)
        {
            hero.experience += Mathf.Max(0, q.expReward);
            hero.money += Mathf.Max(0, q.moneyReward);
            GrantConfigItems(hero, q.otherReward);
            GrantConfigItems(hero, RewardForClass(hero.heroClass, q));
            int next = q.afterQuestId;
            if (next > 0)
            {
                hero.questId = next;
                hero.questProgress = 0;
                var nq = LegacyOfEternity.World.OriginalQuestCatalog.Get(next);
                hero.questGoalCount = nq != null ? Mathf.Max(0, nq.goalCount) : 0;
            }
            else
            {
                hero.questId = 0;
                hero.questProgress = 0;
                hero.questGoalCount = 0;
            }
            return true;
        }

        public static string QuestTitle(int questId) =>
            LegacyOfEternity.World.OriginalQuestCatalog.Title(questId);

        public static string QuestObjective(int questId) =>
            LegacyOfEternity.World.OriginalQuestCatalog.Objective(questId);
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

        static Hero Clone(Hero src)
        {
            var clone = JsonUtility.FromJson<Hero>(JsonUtility.ToJson(src));
            EnsureInventory(clone);
            return clone;
        }
    }
}
