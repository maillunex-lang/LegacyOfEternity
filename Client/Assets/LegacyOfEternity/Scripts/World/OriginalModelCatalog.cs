using System;
using UnityEngine;

namespace LegacyOfEternity.World
{
    [Serializable]
    public sealed class OriginalModelEntry
    {
        public long modelId;
        public string resourcesPath;
    }

    [Serializable]
    public sealed class OriginalNpcLookEntry
    {
        public long npcId;
        public long lookModelId;
        public string name;
    }

    [Serializable]
    public sealed class OriginalMonsterStatEntry
    {
        public long id;
        public string name;
        public int hp;
        public int att;
    }

    [Serializable]
    public sealed class OriginalModelCatalogFile
    {
        public string source;
        public OriginalNpcLookEntry[] npcLooks;
        public OriginalMonsterStatEntry[] monsterStats;
        public OriginalModelEntry[] models;
    }

    public static class OriginalModelCatalog
    {
        static OriginalModelCatalogFile data;
        static bool loaded;

        public static void EnsureLoaded()
        {
            if (loaded)
                return;
            var asset = Resources.Load<TextAsset>("OriginalMap/HaitanModels");
            if (asset == null)
                throw new Exception("Missing Resources/OriginalMap/HaitanModels.json");
            data = JsonUtility.FromJson<OriginalModelCatalogFile>(asset.text);
            data.npcLooks ??= Array.Empty<OriginalNpcLookEntry>();
            data.monsterStats ??= Array.Empty<OriginalMonsterStatEntry>();
            data.models ??= Array.Empty<OriginalModelEntry>();
            loaded = true;
            Debug.Log("LOE_ORIGINAL_MODELS loaded=" + data.models.Length +
                      " npcLooks=" + data.npcLooks.Length +
                      " monsterStats=" + data.monsterStats.Length);
        }

        public static GameObject LoadVisual(long modelId)
        {
            EnsureLoaded();
            if (modelId == 0)
                return null;
            string path = null;
            for (int i = 0; i < data.models.Length; i++)
            {
                if (data.models[i] != null && data.models[i].modelId == modelId)
                {
                    path = data.models[i].resourcesPath;
                    break;
                }
            }
            if (string.IsNullOrEmpty(path))
                return null;
            return Resources.Load<GameObject>(path);
        }

        public static long NpcLookModelId(long npcId)
        {
            EnsureLoaded();
            for (int i = 0; i < data.npcLooks.Length; i++)
                if (data.npcLooks[i] != null && data.npcLooks[i].npcId == npcId)
                    return data.npcLooks[i].lookModelId;
            return 0;
        }

        public static string NpcName(long npcId)
        {
            EnsureLoaded();
            for (int i = 0; i < data.npcLooks.Length; i++)
                if (data.npcLooks[i] != null && data.npcLooks[i].npcId == npcId &&
                    !string.IsNullOrEmpty(data.npcLooks[i].name))
                    return data.npcLooks[i].name;
            return "NPC " + npcId;
        }

        public static OriginalMonsterStatEntry MonsterStats(long templateId)
        {
            EnsureLoaded();
            for (int i = 0; i < data.monsterStats.Length; i++)
                if (data.monsterStats[i] != null && data.monsterStats[i].id == templateId)
                    return data.monsterStats[i];
            return null;
        }

        public static GameObject AttachVisual(Transform parent, long modelId)
        {
            var prefab = LoadVisual(modelId);
            if (prefab == null)
                return null;
            var visual = UnityEngine.Object.Instantiate(prefab, parent, false);
            OriginalVisualSanitize.Sanitize(visual);
            visual.name = "Visual_" + modelId;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.SetActive(true);

            // Strip colliders from visual so CharacterController owns movement.
            foreach (var col in visual.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.Destroy(col);

            return visual;
        }
    }
}
