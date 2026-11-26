using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    public static class FourClassesBuilder
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const string OutPath = "Assets/LegacyOfEternity/";
        private const string InPath = "Assets/LegacyAssets/AssetsRaw/Animation/player/";
        private static readonly string[] Folders = {"zhanshi/ZS_M_01","cike/CK_W_01","fashi/FS_W_01","GJS/GJS_M_01"};
        private static readonly string[] Prefixes = {"ZS_M_01","CK_W_01","FS_W_01","GJS_M_01"};
        private static readonly string[] ClassNames = {"Warrior","Assassin","Mage","Archer"};

        private static AnimationClip GetClip(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new Exception("No importer for "+path);
            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.SaveAndReimport();
            }
            var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
            if(clips.Length == 0)throw new Exception("No animation for "+path);
            return clips[0];
        }
        private static void SetupController(int idx, Animator animator)
        {
            string dest=OutPath+ClassNames[idx]+"Prototype.controller";
            var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(dest);
            if (ctrl == null)
            {
                var folder=InPath+Folders[idx]+"/"+Prefixes[idx]+"@";
                var idle=GetClip(folder+"idle.FBX");
                var run=GetClip(folder+"run.FBX");
                ctrl=AnimatorController.CreateAnimatorControllerAtPath(dest);
                ctrl.AddParameter("Speed",AnimatorControllerParameterType.Float);
                var sm=ctrl.layers[0].stateMachine;
                var a=sm.AddState("Idle");a.motion=idle;
                var b=sm.AddState("Run");b.motion=run;sm.defaultState=a;
                var t=a.AddTransition(b);t.hasExitTime=false;t.duration=.12f;t.AddCondition(AnimatorConditionMode.Greater,.15f,"Speed");
                t=b.AddTransition(a);t.hasExitTime=false;t.duration=.12f;t.AddCondition(AnimatorConditionMode.Less,.15f,"Speed");
            }
            animator.runtimeAnimatorController=ctrl;
            animator.applyRootMotion=false;
        }
        private static GameObject CreateVisual(int idx,Transform hero)
        {
            var basePath=InPath+Folders[idx]+"/";
            var p=basePath+Prefixes[idx]+".FBX";
            var importer=AssetImporter.GetAtPath(p) as ModelImporter;
            if(importer == null)throw new Exception("Model not found "+p);
            if(importer.animationType != ModelImporterAnimationType.Generic)
            {importer.animationType=ModelImporterAnimationType.Generic;importer.SaveAndReimport();}
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if(model == null)throw new Exception("AssetDatabase model null "+p);
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name=ClassNames[idx]+" Visual";
            visual.transform.SetParent(hero,false);
            var rs=visual.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("No renderers "+p);
            var b=rs[0].bounds; foreach(var r in rs)b.Encapsulate(r.bounds);
            if(b.size.y<.001f)throw new Exception("Invalid bounds "+p);
            visual.transform.localScale*=1.8f/b.size.y;
            b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            visual.transform.position += hero.position+Vector3.down*.95f-new Vector3(b.center.x,b.min.y,b.center.z);
            var mat=AssetDatabase.LoadAssetAtPath<Material>(OutPath+ClassNames[idx]+"Prototype.mat");
            if(mat==null)
            {
                var shader=Shader.Find("Standard");
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(basePath+Prefixes[idx]+".tga");
                if(shader==null||texture==null)throw new Exception("No shader or texture "+p);
                mat=new Material(shader){mainTexture=texture};
                AssetDatabase.CreateAsset(mat,OutPath+ClassNames[idx]+"Prototype.mat");
            }
            foreach(var r in rs) r.sharedMaterials=r.sharedMaterials.Select(x=>mat).ToArray();
            var animator=visual.GetComponent<Animator>();
            if(animator==null)animator=visual.AddComponent<Animator>();
            SetupController(idx,animator);
            Debug.Log("LOE_CLASS_MODEL "+ClassNames[idx]+" meshRenderers="+rs.Length+" size="+b.size);
            return visual;
        }
        private static Button AddButton(Transform parent,string name,string text,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=new Vector2(.5f,1f);rect.anchorMax=new Vector2(.5f,1f);
            rect.pivot=new Vector2(.5f,1f);rect.anchoredPosition=position;rect.sizeDelta=size;
            go.GetComponent<Image>().color=new Color(.06f,.14f,.22f,.75f);
            var textGo=new GameObject("Label",typeof(RectTransform),typeof(Text));
            textGo.transform.SetParent(go.transform,false);
            var r=textGo.GetComponent<RectTransform>();
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
            var t=textGo.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text=text;t.alignment=TextAnchor.MiddleCenter;t.fontSize=24;t.color=Color.white;
            return go.GetComponent<Button>();
        }
        public static void Run()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var hero=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="LOE Prototype Hero");
            var old=hero.Find("Warrior Visual");
            if(old!=null) old.name="Warrior Visual";
            var visuals=new GameObject[4];
            visuals[0]=old!=null?old.gameObject:CreateVisual(0,hero);
            for(int i=1;i<4;i++)
            {
                var found=hero.Find(ClassNames[i]+" Visual");
                visuals[i]=found!=null?found.gameObject:CreateVisual(i,hero);
            }
            var selector=hero.GetComponent<CharacterClassSelector>();
            if(selector==null)selector=hero.gameObject.AddComponent<CharacterClassSelector>();
            var so=new SerializedObject(selector);
            var entries=so.FindProperty("classVisuals");entries.arraySize=4;
            for(int i=0;i<4;i++)entries.GetArrayElementAtIndex(i).objectReferenceValue=visuals[i];
            var canvas=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name=="LOE Mobile HUD");
            if(canvas==null) throw new Exception("Mobile HUD not found");
            var panel=canvas.Find("Class Selection");
            if(panel==null)
            {
                var go=new GameObject("Class Selection",typeof(RectTransform),typeof(Image));
                go.transform.SetParent(canvas,false);panel=go.transform;
                var r=go.GetComponent<RectTransform>();
                r.anchorMin=new Vector2(.5f,1);r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);
                r.anchoredPosition=new Vector2(0,-18);r.sizeDelta=new Vector2(680,100);
                go.GetComponent<Image>().color=new Color(.03f,.08f,.16f,.7f);
                var labelGo=new GameObject("Selected Class",typeof(RectTransform),typeof(Text));
                labelGo.transform.SetParent(panel,false);
                var tr=labelGo.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=new Vector2(200,10);tr.offsetMax=new Vector2(-200,-10);
                var label=labelGo.GetComponent<Text>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=32;label.color=Color.white;label.alignment=TextAnchor.MiddleCenter;label.text="ВОИН";
                so.FindProperty("classLabel").objectReferenceValue=label;
                var prev=AddButton(panel,"Previous","<",new Vector2(-270,-16),new Vector2(90,66));
                var next=AddButton(panel,"Next",">",new Vector2(270,-16),new Vector2(90,66));
                UnityEditor.Events.UnityEventTools.AddPersistentListener(prev.onClick,selector.PreviousClass);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(next.onClick,selector.NextClass);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            for(int i=0;i<visuals.Length;i++)visuals[i].SetActive(i==0);
            if(!EditorSceneManager.SaveScene(scene))throw new Exception("Scene save failed");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_FOUR_CLASSES_OK count=4 UI=1 models="+string.Join(",",ClassNames));
        }
    }
}
