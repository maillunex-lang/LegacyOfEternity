using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    /// <summary>Imports original class weapons into the PvE scene. Strictly refuses to invent a hand bone.</summary>
    public static class M052WeaponBuilder
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const string Original = "Assets/LegacyAssets/AssetsRaw/Animation/player/";
        private const string Materials = "Assets/LegacyOfEternity/Weapons/";
        private static readonly string[] Classes = {"Warrior", "Assassin", "Mage", "Archer"};
        private static readonly string[] Weapons = {
            "zhanshi/ZS_M_wuqi/ZS_M_wuqi_01/ZS_M_wuqi_01",
            "cike/CK_W_wuqi/CK_W_wuqi_01_a/CK_W_wuqi_01_a",
            "fashi/FS_W_wuqi/FS_W_wuqi_01/FS_W_wuqi_01",
            "GJS/GJS_M_wuqi/GJS_M_01_wuqi/GJS_M_01_wuqi"
        };
        private static readonly string[] TextureExt = {".png", ".tga", ".png", ".tga"};

        private static Transform LocateHand(Transform character)
        {
            // Original class models contain a DM_R_Hand dummy attachment marker.
            // Prefer exact original attachment points; never select an arbitrary limb.
            string[] anchors = { "DM_R_Hand", "E_Hand_R", "Bip001 R Hand" };
            var bones = character.GetComponentsInChildren<Transform>(true)
                .Where(t => t != character && !t.GetComponentsInParent<Transform>(true).Any(p => p.name == "Original Weapon Attachment"))
                .ToArray();
            foreach (var exact in anchors)
            {
                var match = bones.FirstOrDefault(t => t.name == exact);
                if (match != null) return match;
            }
            Debug.LogWarning("LOE_WEAPON_SOCKET_MISSING " + character.name + " expected=" + string.Join(",", anchors));
            return null;
        }
        [MenuItem("Legacy of Eternity/M05.2 Attach Original Weapons")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder(Materials.TrimEnd('/')))
                AssetDatabase.CreateFolder("Assets/LegacyOfEternity", "Weapons");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var hero = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "LOE Prototype Hero");
            if (hero == null) throw new Exception("Missing hero");
            var issues = new List<string>();
            var prepared = new List<(Transform visual, Transform hand, GameObject model, Material material, string id)>();
            for (int i = 0; i < Classes.Length; i++)
            {
                var visual = hero.Find(Classes[i] + " Visual");
                string modelPath = Original + Weapons[i] + ".FBX";
                string texturePath = Original + Weapons[i] + TextureExt[i];
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (visual == null || model == null || texture == null)
                {
                    issues.Add(Classes[i] + ": missing visual/model/texture " + modelPath);
                    continue;
                }
                var hand = LocateHand(visual);
                if (hand == null)
                {
                    issues.Add(Classes[i] + ": right-hand bone not uniquely identifiable; manual rig mapping required");
                    continue;
                }
                string materialPath = Materials + Classes[i] + "OriginalWeapon.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (mat == null)
                {
                    var shader = Shader.Find("Standard");
                    if (shader == null) { issues.Add("Standard shader unavailable"); continue; }
                    mat = new Material(shader) { mainTexture = texture };
                    AssetDatabase.CreateAsset(mat, materialPath);
                }
                if (mat.mainTexture != texture) { mat.mainTexture = texture; EditorUtility.SetDirty(mat); }
                prepared.Add((visual, hand, model, mat, Weapons[i].Split('/').Last()));
                Debug.Log("LOE_WEAPON_PREPARED class=" + Classes[i] + " bone=" + hand.name + " model=" + modelPath);
            }
            if (issues.Count > 0)
            {
                foreach (var issue in issues) Debug.LogError("LOE_WEAPON_ISSUE " + issue);
                throw new Exception("Weapon validation failed: " + string.Join(" | ", issues));
            }
            foreach (var weapon in prepared)
            {
                if (weapon.visual.name != "Warrior Visual" && weapon.visual.name != "Assassin Visual")
                    continue;
                var oldAttachments = weapon.hand.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name == "Original Weapon Attachment").ToArray();
                foreach (var old in oldAttachments) UnityEngine.Object.DestroyImmediate(old.gameObject);
                var mount = new GameObject("Original Weapon Attachment");
                mount.transform.SetParent(weapon.hand, false);
                var equipped = (GameObject)PrefabUtility.InstantiatePrefab(weapon.model);
                if (equipped == null) throw new Exception("Weapon instantiation failed " + weapon.id);
                equipped.transform.SetParent(mount.transform, false);
                equipped.transform.localPosition = Vector3.zero;
                equipped.transform.localRotation = Quaternion.identity;
                equipped.transform.localScale = Vector3.one;
                equipped.name = "Original " + weapon.id;
                var renderers = equipped.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) throw new Exception("Weapon FBX has no renderers: " + weapon.id);
                foreach (var renderer in renderers)
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => weapon.material).ToArray();
                var component = weapon.visual.GetComponent<WeaponEquipment>();
                if (component == null) component = weapon.visual.gameObject.AddComponent<WeaponEquipment>();
                component.Configure(mount.transform, equipped, weapon.id);
                Debug.Log("LOE_WEAPON_ATTACHED " + weapon.visual.name + " hand=" + weapon.hand.name +
                          " renderers=" + renderers.Length + " weapon=" + weapon.id);
            }
            // Reconstruct the assassin's second weapon exactly as in original dual-blade attachment logic.
            // Use separate FBX geometry, aligned at zero rotation/translation on the left hand.
            var assassin = hero.Find("Assassin Visual");
            if (assassin == null) throw new Exception("Assassin visual missing");
            var leftHand = assassin.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "DM_L_Hand");
            if (leftHand == null) throw new Exception("Original assassin DM_L_Hand not found");
            var leftModel = AssetDatabase.LoadAssetAtPath<GameObject>(
                Original + "cike/CK_W_wuqi/CK_W_wuqi_01_b/CK_W_wuqi_01_b.FBX");
            var leftTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                Original + "cike/CK_W_wuqi/CK_W_wuqi_01_b/CK_W_wuqi_01_b.tga");
            if (leftModel == null || leftTexture == null)
                throw new Exception("Original assassin second weapon or texture missing");
            var leftMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                Materials + "AssassinOriginalWeaponLeft.mat");
            if (leftMaterial == null)
            {
                leftMaterial = new Material(Shader.Find("Standard")) { mainTexture = leftTexture };
                AssetDatabase.CreateAsset(leftMaterial, Materials + "AssassinOriginalWeaponLeft.mat");
            }
            var oldLeft = leftHand.Find("Original Left Weapon Attachment");
            if (oldLeft != null) UnityEngine.Object.DestroyImmediate(oldLeft.gameObject);
            var leftMount = new GameObject("Original Left Weapon Attachment");
            leftMount.transform.SetParent(leftHand, false);
            var leftInstance = (GameObject)PrefabUtility.InstantiatePrefab(leftModel);
            if (leftInstance == null) throw new Exception("Original assassin left FBX instantiation failed");
            leftInstance.transform.SetParent(leftMount.transform, false);
            leftInstance.name = "Original CK_W_wuqi_01_b";
            leftInstance.transform.localPosition = Vector3.zero;
            leftInstance.transform.localRotation = Quaternion.identity;
            leftInstance.transform.localScale = Vector3.one;
            var leftRenderers = leftInstance.GetComponentsInChildren<Renderer>(true);
            if (leftRenderers.Length == 0) throw new Exception("Second assassin weapon has no renderers");
            foreach (var renderer in leftRenderers)
                renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => leftMaterial).ToArray();
            var assassinEquipment = assassin.GetComponent<WeaponEquipment>();
            if (assassinEquipment == null) throw new Exception("Assassin right-hand equipment missing");
            assassinEquipment.ConfigureLeft(leftMount.transform, leftInstance, "CK_W_wuqi_01_b");
            Debug.Log("LOE_M0531_ASSASSIN_DUAL left=" + leftHand.name +
                " right=" + assassinEquipment.RightHandSocket.name +
                " leftRenderers=" + leftRenderers.Length);
            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save weapon scene");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M052_WEAPONS_ATTACHED=" + prepared.Count +
                      " CAUTION: visually verify orientation on Idle/Run/Attack/Skill/Death for every class");
        }

        [MenuItem("Legacy of Eternity/M05.2 Audit Weapon Attachments")]
        public static void Audit()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var hero = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .First(t => t.name == "LOE Prototype Hero");
            var failures = new List<string>();
            foreach (var cls in Classes)
            {
                var visual = hero.Find(cls + " Visual");
                var weapon = visual != null ? visual.GetComponent<WeaponEquipment>() : null;
                if (weapon == null || weapon.RightHandSocket == null || weapon.EquippedWeapon == null ||
                    !weapon.RightHandSocket.IsChildOf(visual) ||
                    !weapon.EquippedWeapon.transform.IsChildOf(weapon.RightHandSocket) ||
                    weapon.EquippedWeapon.GetComponentsInChildren<Renderer>(true).Length == 0)
                    failures.Add(cls);
            }
            var warrior = hero.Find("Warrior Visual")?.GetComponent<WeaponEquipment>();
            if (warrior == null || warrior.EquipmentId != "ZS_M_wuqi_01")
                failures.Add("Warrior model should be ZS_M_wuqi_01");
            var assassin = hero.Find("Assassin Visual")?.GetComponent<WeaponEquipment>();
            if (assassin == null || assassin.EquipmentId != "CK_W_wuqi_01_a" ||
                assassin.LeftHandSocket == null || assassin.LeftHandSocket.name != "Original Left Weapon Attachment" ||
                assassin.LeftEquippedWeapon == null || assassin.LeftEquipmentId != "CK_W_wuqi_01_b" ||
                !assassin.LeftEquippedWeapon.transform.IsChildOf(assassin.LeftHandSocket) ||
                !assassin.LeftHandSocket.IsChildOf(hero.Find("Assassin Visual")) ||
                assassin.LeftEquippedWeapon.GetComponentsInChildren<Renderer>(true).Length == 0)
                failures.Add("Assassin dual weapon rig");
            Debug.Log("LOE_M052_AUDIT attachments=" + (4-failures.Count) + "/4 problems=" + string.Join(",",failures));
            if (failures.Count > 0) throw new Exception("M05.2 failed for " + string.Join(",",failures));
        }
    }
}
