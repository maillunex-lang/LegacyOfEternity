using System;
using UnityEngine;

namespace LegacyOfEternity.World
{
    [Serializable]
    public sealed class OriginalMapSpawn
    {
        public long id;
        public int spawnIndex;
        public int type;
        public long modelId;
        public double x;
        public double y;
        public double z;
        public double direction;

        public Vector3 Position => new Vector3((float)x, (float)y, (float)z);
        public Quaternion Rotation => Quaternion.Euler(0f, (float)(direction * Mathf.Rad2Deg), 0f);
    }

    [Serializable]
    public sealed class OriginalMapData
    {
        public long mapId;
        public int mapW;
        public int mapH;
        public string source;
        public string scene;
        public OriginalMapSpawn heroBirth;
        public OriginalMapSpawn[] monsters;
        public OriginalMapSpawn[] npcs;
        public OriginalMapSpawn[] collections;
        public OriginalMapSpawn[] portals;

        public static OriginalMapData LoadHaitan()
        {
            var asset = Resources.Load<TextAsset>("OriginalMap/110200011");
            if (asset == null)
                throw new Exception("Missing Resources/OriginalMap/110200011.json");
            var data = JsonUtility.FromJson<OriginalMapData>(asset.text);
            if (data == null || data.heroBirth == null)
                throw new Exception("Failed to parse OriginalMap/110200011.json");
            data.monsters ??= Array.Empty<OriginalMapSpawn>();
            data.npcs ??= Array.Empty<OriginalMapSpawn>();
            data.collections ??= Array.Empty<OriginalMapSpawn>();
            data.portals ??= Array.Empty<OriginalMapSpawn>();
            return data;
        }
    }
}
