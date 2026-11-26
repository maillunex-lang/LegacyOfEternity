using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using LegacyOfEternity.Mobile;
namespace LegacyOfEternity.Editor
{
    public static class CoastalEnhance
    {
        const string Path = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        [MenuItem("Legacy of Eternity/Enhance Coastal Prototype")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene(Path,OpenSceneMode.Single);
            var roots=scene.GetRootGameObjects();
            var objects=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var renderers=objects.SelectMany(t=>t.GetComponents<Renderer>()).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
            int materialSlots=0;
            foreach(var r in renderers)
            {
                var mats=r.sharedMaterials;
                if(mats==null||mats.Length==0)continue;
                bool changed=false;
                for(int i=0;i<mats.Length;i++)if(mats[i]==null){mats[i]=AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");materialSlots++;changed=true;}
                if(changed)r.sharedMaterials=mats;
            }
            var hero=objects.Select(t=>t.gameObject).FirstOrDefault(g=>g.name=="LOE Prototype Hero");
            if(hero==null)throw new Exception("Missing LOE hero");
            var cam=objects.SelectMany(t=>t.GetComponents<Camera>()).FirstOrDefault(c=>c.enabled);
            if(cam==null)throw new Exception("Missing active camera");
            // Improve visibility with a safe, stable player view, avoiding a downward-looking shot into the hero mesh.
            cam.nearClipPlane=0.08f;
            cam.farClipPlane=1500f;
            cam.fieldOfView=65;
            if(!objects.Any(t=>t.gameObject.name=="LOE Sunlight"))
            {
                var sun=new GameObject("LOE Sunlight");
                var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.15f;light.shadows=LightShadows.Soft;
                sun.transform.rotation=Quaternion.Euler(48,-30,0);
            }
            if(!objects.Any(t=>t.gameObject.name=="LOE Mobile HUD"))
            {
                var canvasObj=new GameObject("LOE Mobile HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
                var canvas=canvasObj.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
                var scaler=canvasObj.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=0.5f;
                var pad=new GameObject("Move Joystick",typeof(RectTransform),typeof(Image),typeof(MobileJoystick));
                pad.transform.SetParent(canvasObj.transform,false);
                var rect=pad.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,0);rect.anchorMax=new Vector2(0,0);rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(200,200);rect.anchoredPosition=new Vector2(160,160);
                var image=pad.GetComponent<Image>();image.color=new Color(.08f,.12f,.2f,.4f);image.raycastTarget=true;
                var knob=new GameObject("Handle",typeof(RectTransform),typeof(Image));knob.transform.SetParent(pad.transform,false);
                var knobRect=knob.GetComponent<RectTransform>();knobRect.anchorMin=new Vector2(.5f,.5f);knobRect.anchorMax=new Vector2(.5f,.5f);knobRect.pivot=new Vector2(.5f,.5f);knobRect.sizeDelta=new Vector2(84,84);
                knob.GetComponent<Image>().color=new Color(.7f,.87f,1f,.8f);
                var joy=pad.GetComponent<MobileJoystick>();
                var serialized=new SerializedObject(joy);serialized.FindProperty("handle").objectReferenceValue=knobRect;serialized.ApplyModifiedPropertiesWithoutUndo();
                if(UnityEngine.Object.FindFirstObjectByType<EventSystem>()==null)
                {
                    var ev=new GameObject("LOE EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
                }
            }
            if(!EditorSceneManager.SaveScene(scene,Path))throw new Exception("Unable to save scene");
            var ms=renderers.SelectMany(r=>r.sharedMaterials).ToArray();
            Debug.Log($"LOE_ENHANCE_OK repairedSlots={materialSlots} remainingNull={ms.Count(m=>m==null)} renderers={renderers.Length} mobileHUD=1");
        }
    }
}
