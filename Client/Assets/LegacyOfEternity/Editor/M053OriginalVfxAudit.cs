using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LegacyOfEternity.Editor
{
    public static class M053OriginalVfxAudit
    {
        private static readonly string Root = "Assets/LegacyAssets/Resources/Effect/skill/player/";
        private static readonly string[] Classes = { "ZS_M", "CK_W", "FS_W", "GJS_M" };
        private static readonly string[] Kinds = { "attack001", "skill001", "skill002", "skill003", "skill004" };

        [MenuItem("Legacy of Eternity/M05.3 Audit Original Effects")]
        public static void Run()
        {
            int count = 0, broken = 0;
            foreach (string cls in Classes)
            foreach (string kind in Kinds)
            {
                string suffix = cls == "GJS_M" ? "_s" : "_S";
                string path = Root + cls + "/" + cls + "_" + kind + suffix + ".prefab";
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source == null)
                {
                    Debug.LogError("LOE_M053_PREFAB_MISSING " + path);
                    broken++;
                    continue;
                }
                int systems = source.GetComponentsInChildren<ParticleSystem>(true).Length;
                int meshes = source.GetComponentsInChildren<MeshFilter>(true).Count(m => m.sharedMesh != null);
                int renderers = source.GetComponentsInChildren<Renderer>(true).Count(r =>
                    r.sharedMaterials.Any(m => m != null && m.shader != null && m.shader.isSupported));
                int missingScripts = source.GetComponentsInChildren<Transform>(true)
                    .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                count++;
                Debug.Log("LOE_M053_PREFAB class=" + cls + " slot=" + kind +
                    " particles=" + systems + " meshes=" + meshes + " renderers=" + renderers +
                    " missingScripts=" + missingScripts);
                if (systems == 0 && renderers == 0) broken++;
            }
            Debug.Log("LOE_M053_PREFAB_AUDIT checked=" + count + " empty=" + broken);
            if (broken > 0) throw new Exception("M05.3 original FX prefab coverage failed: " + broken);
        }
    }
}
