using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Player;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Editor
{
    public static class M057OriginalSkillIcons
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private static readonly string[] ButtonNames = { "Attack", "Skill 1", "Skill 2", "Skill 3", "Skill 4" };

        // Source: Resources/Lua/Data/t_skill.lua.txt. These are exact original
        // atlas names, resolved to original sprite slices using their GUIDs.
        private static readonly string[][] Sprites = {
            new[] { "UIMain:ico_attack", "UIMain:Skill_zhanshi_1", "UIMain:Skill_zhanshi_2", "UIMain:Skill_zhanshi_3", "UIMain:Skill_zhanshi_4" },
            new[] { "UIMain2:ico_attack3", "UIMain2:Skill_cike_1", "UIMain2:Skill_cike_2", "UIMain2:Skill_cike_3", "UIMain2:Skill_cike_4" },
            new[] { "UIMain:ico_attack2", "UIMain:Skill_fashi_1", "UIMain:Skill_fashi_2", "UIMain:Skill_fashi_3", "UIMain:Skill_fashi_4" },
            new[] { "UIMain2:ico_attack4", "UIMain2:Skill_sheshou_1", "UIMain2:Skill_sheshou_2", "UIMain2:Skill_sheshou_3", "UIMain2:Skill_sheshou_4" }
        };

        private static Sprite[] Resolve()
        {
            var result = new Sprite[20];
            var a = AssetDatabase.LoadAllAssetsAtPath("Assets/LegacyAssets/Atlas/UIMain.png").OfType<Sprite>().ToArray();
            var b = AssetDatabase.LoadAllAssetsAtPath("Assets/LegacyAssets/Atlas/UIMain2.png").OfType<Sprite>().ToArray();
            for (int cl=0;cl<4;cl++)
                for (int slot=0;slot<5;slot++)
                {
                    var refName=Sprites[cl][slot];
                    var parts=refName.Split(':');
                    var pool=parts[0]=="UIMain" ? a : b;
                    var requested=parts[1];
                    var sprite=pool.FirstOrDefault(s=>s.name == requested || s.name == requested+".png");
                    if (sprite == null)
                        throw new Exception("Original sprite not found " + refName + " ("+pool.Length+" imported)");
                    result[cl*5+slot]=sprite;
                }
            return result;
        }

        [MenuItem("Legacy of Eternity/M05.7 Install Original Skill Icons")]
        public static void Run()
        {
            var sourceSprites=Resolve();
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero=all.FirstOrDefault(t=>t.name=="LOE Prototype Hero");
            var hud=all.FirstOrDefault(t=>t.name=="LOE Mobile HUD");
            if (hero==null || hud==null) throw new Exception("Hero/HUD missing");
            var selector=hero.GetComponent<CharacterClassSelector>();
            var panel=hud.Find("Combat Actions");
            if (selector==null || panel==null) throw new Exception("Selector/action panel missing");
            var images=new Image[5];
            for(int slot=0;slot<5;slot++)
            {
                var button=panel.Find(ButtonNames[slot]);
                if(button==null || button.GetComponent<Button>()==null)
                    throw new Exception("Missing combat button "+ButtonNames[slot]);
                var imageTransform=button.Find("Original Skill Icon");
                if(imageTransform==null)
                {
                    var child=new GameObject("Original Skill Icon",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                    child.transform.SetParent(button,false);
                    imageTransform=child.transform;
                }
                var rect=(RectTransform)imageTransform;
                rect.anchorMin=new Vector2(.5f,.5f);
                rect.anchorMax=new Vector2(.5f,.5f);
                rect.anchoredPosition=Vector2.zero;
                // Each existing combat button is 142x75. Preserve layout, button input and cooldown text.
                rect.sizeDelta=new Vector2(60f,60f);
                var image=imageTransform.GetComponent<Image>();
                image.raycastTarget=false;
                image.preserveAspect=true;
                image.color=Color.white;
                image.sprite=sourceSprites[slot];
                // Behind existing cooldown overlay and gameplay text.
                imageTransform.SetAsFirstSibling();
                images[slot]=image;
            }
            var icons=hero.GetComponent<OriginalSkillIconHud>();
            if(icons==null)icons=hero.gameObject.AddComponent<OriginalSkillIconHud>();
            icons.Configure(selector,images,sourceSprites);
            EditorUtility.SetDirty(icons);
            if(!EditorSceneManager.SaveScene(scene))throw new Exception("Failed to save scene");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M057_ICONS_INSTALLED classes=4 sprites=20 buttons=5 source=original_lua_and_atlas");
        }

        [MenuItem("Legacy of Eternity/M05.7 Audit Original Skill Icons")]
        public static void Audit()
        {
            Resolve();
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero=all.First(t=>t.name=="LOE Prototype Hero");
            var hud=all.First(t=>t.name=="LOE Mobile HUD");
            var panel=hud.Find("Combat Actions");
            var comp=hero.GetComponent<OriginalSkillIconHud>();
            if(comp==null)throw new Exception("Missing OriginalSkillIconHud");
            for(int i=0;i<5;i++)
            {
                var button=panel.Find(ButtonNames[i]);
                var image=button?.Find("Original Skill Icon")?.GetComponent<Image>();
                if(image==null || image.sprite==null || image.raycastTarget)
                    throw new Exception("Missing/non-interactive original icon at "+ButtonNames[i]);
                if(button.GetComponent<Button>()==null || button.Find("Cooldown Overlay")==null)
                    throw new Exception("Combat button/cooldown damaged at "+ButtonNames[i]);
            }
            var so=new SerializedObject(comp);
            var sprites=so.FindProperty("classSprites");
            var images=so.FindProperty("skillImages");
            var expected=Resolve();
            if(sprites.arraySize!=20 || images.arraySize!=5)throw new Exception("Wrong sprite arrays");
            for(int i=0;i<20;i++)
                if(sprites.GetArrayElementAtIndex(i).objectReferenceValue!=expected[i])
                    throw new Exception("M057 wrong icon mapping "+i);
            Debug.Log("LOE_M057_AUDIT classes=4 originalSprites=20 skillButtons=5 result=PASS");
        }
    }
}
