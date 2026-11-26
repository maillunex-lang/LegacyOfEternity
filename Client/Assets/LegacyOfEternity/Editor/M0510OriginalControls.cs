using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Player;
using LegacyOfEternity.CameraSystem;
using LegacyOfEternity.Mobile;

namespace LegacyOfEternity.Editor
{
    /// <summary>
    /// M05.10: actual original HUD base 1136x640, real original UIMain art,
    /// preserve the existing battle callbacks and original SillUI slot layout.
    /// </summary>
    public static class M0510OriginalControls
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const string AtlasPath = "Assets/LegacyAssets/Atlas/UIMain.png";

        private static Sprite GetSprite(string name)
        {
            var sprite=AssetDatabase.LoadAllAssetsAtPath(AtlasPath).OfType<Sprite>()
                .FirstOrDefault(s=>s.name==name || s.name==name+".png");
            if(sprite==null)throw new Exception("Original UIMain sprite missing: "+name);
            return sprite;
        }

        private static RectTransform Place(Transform node, Vector2 at, Vector2 size)
        {
            var rect=node as RectTransform;
            if(rect==null)throw new Exception("No RectTransform on "+node.name);
            rect.anchorMin=rect.anchorMax=new Vector2(1,0);
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=at;
            rect.sizeDelta=size;
            rect.localScale=Vector3.one;
            return rect;
        }

        private static Image Picture(Transform parent,string name,Sprite sprite,Vector2 size,Vector2 pos)
        {
            var node=parent.Find(name);
            if(node==null)
            {
                var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                obj.transform.SetParent(parent,false);
                node=obj.transform;
            }
            var rect=node as RectTransform;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
            var img=node.GetComponent<Image>();
            if(img==null)throw new Exception("No Image in "+name);
            img.sprite=sprite;
            img.preserveAspect=true;
            img.color=Color.white;
            img.raycastTarget=false;
            node.SetAsFirstSibling();
            return img;
        }

        [MenuItem("Legacy of Eternity/M05.10 Install Correct Original Controls")]
        public static void Run()
        {
            var originalAutoOn=GetSprite("main_guaji");
            var originalAutoOff=GetSprite("main_guaji_disable");
            var originalSwitch=GetSprite("btn_switch1");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero=all.FirstOrDefault(x=>x.name=="LOE Prototype Hero");
            var hud=all.FirstOrDefault(x=>x.name=="LOE Mobile HUD");
            var camera=all.Select(x=>x.GetComponent<OrbitFollowCamera>()).FirstOrDefault(x=>x!=null);
            if(hero==null || hud==null || camera==null)throw new Exception("Hero/HUD/orbit camera missing");

            // Original SkillUIParent and UIMain were authored at 1136x640,
            // NOT the previously configured 1920x1080 virtual screen.
            var scaler=hud.GetComponent<CanvasScaler>();
            if(scaler==null)throw new Exception("HUD CanvasScaler missing");
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1136,640);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.5f;

            // Existing attack/skill placement and 20 authentic class icons remain.
            // In the corrected reference space, the same 70/110 UI units display at
            // their original physical proportions instead of 0.6x the source.
            var actions=hud.Find("Combat Actions");
            if(actions==null)throw new Exception("Combat Actions missing");
            foreach(var name in new[]{"Attack","Skill 1","Skill 2","Skill 3","Skill 4"})
            {
                var button=actions.Find(name);
                if(button==null || button.GetComponent<Button>()?.onClick.GetPersistentEventCount()==0 ||
                   button.Find("Original Skill Icon")?.GetComponent<Image>()==null)
                    throw new Exception("Original button disconnected "+name);
                // Original skill icon is the visual signifier, not a text overlay.
                foreach(var label in button.GetComponentsInChildren<Text>(true))
                    if(label.gameObject.name!="Cooldown Overlay")
                        label.enabled=false;
            }

            var auto=hud.Find("Auto Combat Toggle");
            var autoButton=auto?.GetComponent<Button>();
            var autoController=hero.GetComponent<AutoCombatController>();
            var indicator=auto?.Find("AUTO Indicator")?.GetComponent<Text>();
            if(autoButton==null || autoController==null || indicator==null ||
               autoButton.onClick.GetPersistentEventCount()!=1 ||
               autoButton.onClick.GetPersistentMethodName(0)!="Toggle")
                throw new Exception("Auto combat callback missing");
            Place(auto,new Vector2(-69,312),new Vector2(112,118));
            var autoBg=auto.GetComponent<Image>();
            autoBg.color=new Color(.07f,.15f,.18f,.18f);
            autoBg.raycastTarget=true;
            var autoPicture=Picture(auto,"Original Auto Icon",originalAutoOff,new Vector2(78,78),new Vector2(0,15));
            var visual=auto.GetComponent<OriginalAutoButtonArt>();
            if(visual==null)visual=auto.gameObject.AddComponent<OriginalAutoButtonArt>();
            visual.Configure(autoController,autoPicture,originalAutoOn,originalAutoOff);
            var autoRect=indicator.rectTransform;
            autoRect.anchorMin=new Vector2(.02f,.01f);
            autoRect.anchorMax=new Vector2(.98f,.33f);
            autoRect.offsetMin=Vector2.zero;
            autoRect.offsetMax=Vector2.zero;
            indicator.fontSize=19;
            indicator.resizeTextForBestFit=true;
            indicator.resizeTextMinSize=13;
            indicator.resizeTextMaxSize=19;
            indicator.alignment=TextAnchor.MiddleCenter;
            indicator.raycastTarget=false;
            indicator.enabled=true;
            autoController.Configure(indicator);

            var targetButton=hud.Find("Select Next Target");
            var targetAction=targetButton?.GetComponent<Button>();
            if(targetAction==null || targetAction.onClick.GetPersistentEventCount()!=1 ||
               targetAction.onClick.GetPersistentMethodName(0)!="TargetNext")
                throw new Exception("Target switch callback missing");
            Place(targetButton,new Vector2(-211,312),new Vector2(112,118));
            var targetBg=targetButton.GetComponent<Image>();
            targetBg.color=new Color(.07f,.15f,.18f,.18f);
            targetBg.raycastTarget=true;
            Picture(targetButton,"Original Target Icon",originalSwitch,new Vector2(82,82),new Vector2(0,15));
            var captionText=targetButton.Find("Caption")?.GetComponent<Text>();
            if(captionText==null)throw new Exception("Target caption missing");
            captionText.text="\u0426\u0415\u041B\u042C"; // ЦЕЛЬ
            captionText.fontSize=19;
            captionText.alignment=TextAnchor.MiddleCenter;
            captionText.resizeTextForBestFit=true;
            captionText.resizeTextMinSize=13;
            captionText.resizeTextMaxSize=19;
            captionText.raycastTarget=false;
            var caption=captionText.rectTransform;
            caption.anchorMin=new Vector2(.02f,.01f);
            caption.anchorMax=new Vector2(.98f,.33f);
            caption.offsetMin=Vector2.zero;
            caption.offsetMax=Vector2.zero;
            targetButton.SetAsLastSibling();
            auto.SetAsLastSibling();

            // Original CameraFollow.getTargetPosition adds one metre before orbiting.
            // Maintain the original source 12.5/8/50 profile and the corrected
            // axis-exclusive CameraControl gesture handling.
            var so=new SerializedObject(camera);
            so.FindProperty("targetOffset").vector3Value=Vector3.up;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(camera);
            if(!EditorSceneManager.SaveScene(scene))throw new Exception("M0510 scene save failed");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M0510_INSTALLED sourceCanvas=1136x640 originalSkills=5 autoSource=main_guaji targetSource=btn_switch1 originalCameraFocusY=1 callbacks=preserved");
        }

        [MenuItem("Legacy of Eternity/M05.10 Audit Correct Original Controls")]
        public static void Audit()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero=all.First(x=>x.name=="LOE Prototype Hero");
            var hud=all.First(x=>x.name=="LOE Mobile HUD");
            var scaler=hud.GetComponent<CanvasScaler>();
            if(scaler==null || scaler.referenceResolution!=new Vector2(1136,640))
                throw new Exception("M0510 wrong canvas source resolution");
            var auto=hud.Find("Auto Combat Toggle");
            var target=hud.Find("Select Next Target");
            if(auto?.GetComponent<Button>()?.onClick.GetPersistentMethodName(0)!="Toggle" ||
               target?.GetComponent<Button>()?.onClick.GetPersistentMethodName(0)!="TargetNext" ||
               auto.GetComponent<OriginalAutoButtonArt>()==null ||
               auto.Find("Original Auto Icon")?.GetComponent<Image>()?.sprite==null ||
               target.Find("Original Target Icon")?.GetComponent<Image>()?.sprite!=GetSprite("btn_switch1"))
                throw new Exception("M0510 original auto/target UI missing or disconnected");
            var camera=all.Select(x=>x.GetComponent<OrbitFollowCamera>()).First(x=>x!=null);
            if(new SerializedObject(camera).FindProperty("targetOffset").vector3Value!=Vector3.up)
                throw new Exception("M0510 camera focus not original");
            var panel=hud.Find("Combat Actions");
            foreach(var name in new[]{"Attack","Skill 1","Skill 2","Skill 3","Skill 4"})
            {
                var b=panel.Find(name);
                if(b?.GetComponent<Button>()?.onClick.GetPersistentEventCount()==0 ||
                   b.Find("Original Skill Icon")?.GetComponent<Image>()?.sprite==null)
                    throw new Exception("M0510 action slot disconnected "+name);
            }
            Debug.Log("LOE_M0510_AUDIT PASS sourceCanvas=1136x640 originalArt=3 skills=5 auto=1 target=1 cameraFocus=1");
        }
    }
}
