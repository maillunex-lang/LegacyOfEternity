using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    /// <summary>Idempotent setup of four prototype combat animation state machines and HUD actions.</summary>
    public static class CombatPrototypeBuilder
    {
        const string ScenePath="Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        const string Root="Assets/LegacyOfEternity/";
        const string Legacy="Assets/LegacyAssets/AssetsRaw/Animation/player/";
        static readonly string[] Classes={"Warrior","Assassin","Mage","Archer"};
        static readonly string[] Folders={"zhanshi/ZS_M_01","cike/CK_W_01","fashi/FS_W_01","GJS/GJS_M_01"};
        static readonly string[] Ids={"ZS_M_01","CK_W_01","FS_W_01","GJS_M_01"};
        static readonly string[] States={"Attack","Skill1","Skill2","Skill3","Skill4","Hit","Death"};
        static readonly string[] ClipNames={"attack001","skill001","skill002","skill003","skill004","hit014","die001"};

        static AnimationClip FindClip(int index,string stem)
        {
            var path=Legacy+Folders[index]+"/"+Ids[index]+"@"+stem+".FBX";
            if(AssetImporter.GetAtPath(path) is not ModelImporter imp)
                throw new Exception("Missing combat clip importer "+path);
            if(imp.animationType!=ModelImporterAnimationType.Generic)
            {
                imp.animationType=ModelImporterAnimationType.Generic;
                imp.SaveAndReimport();
            }
            var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c=>!c.name.StartsWith("__preview__"));
            if(clip==null)throw new Exception("Missing animation clip "+path);
            return clip;
        }
        static AnimatorState FindState(AnimatorStateMachine sm,string name) =>
            sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name);

        static void EnsureFloat(AnimatorController ctrl,string name)
        {
            if(!ctrl.parameters.Any(x=>x.name==name))
                ctrl.AddParameter(name,AnimatorControllerParameterType.Float);
        }
        static void EnsureBool(AnimatorController ctrl,string name)
        {
            if(!ctrl.parameters.Any(x=>x.name==name))
                ctrl.AddParameter(name,AnimatorControllerParameterType.Bool);
        }
        static void EnsureTrigger(AnimatorController ctrl,string name)
        {
            if(!ctrl.parameters.Any(x=>x.name==name))
                ctrl.AddParameter(name,AnimatorControllerParameterType.Trigger);
        }
// Use named action states to keep original clip references explicit and inspectable.
        static void ConfigureController(int i)
        {
            var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(Root+Classes[i]+"Prototype.controller");
            if(ctrl==null)throw new Exception("Missing controller "+Classes[i]);
            if(ctrl.layers.Length==0)throw new Exception("Controller has no layer");
            var sm=ctrl.layers[0].stateMachine;
            var idle=FindState(sm,"Idle");
            var run=FindState(sm,"Run");
            if(idle==null||run==null)throw new Exception("Missing locomotion states for "+Classes[i]);
            EnsureFloat(ctrl,"Speed");
            EnsureBool(ctrl,"Dead");
            EnsureTrigger(ctrl,"Reset");
            foreach(var trigger in States.Take(6))EnsureTrigger(ctrl,trigger);
            for(int n=0;n<States.Length;n++)
            {
                var name=States[n];
                var clip=FindClip(i,ClipNames[n]);
                var state=FindState(sm,name) ?? sm.AddState(name);
                state.motion=clip;
                state.writeDefaultValues=true;
                if(name=="Death")continue;
                if(!sm.anyStateTransitions.Any(t=>t.destinationState==state))
                {
                    var t=sm.AddAnyStateTransition(state);
                    t.hasExitTime=false;t.duration=.10f;t.canTransitionToSelf=false;
                    t.AddCondition(AnimatorConditionMode.If,0,name);
                    t.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
                }
                if(!state.transitions.Any(t=>t.destinationState==idle))
                {
                    var back=state.AddTransition(idle);
                    back.hasExitTime=true;back.exitTime=.92f;back.duration=.12f;
                    back.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
                }
            }
            var death=FindState(sm,"Death");
            if(!sm.anyStateTransitions.Any(t=>t.destinationState==death))
            {
                var t=sm.AddAnyStateTransition(death);
                t.hasExitTime=false;t.duration=.08f;t.canTransitionToSelf=false;
                t.AddCondition(AnimatorConditionMode.If,0,"Dead");
            }
            foreach(var transition in sm.anyStateTransitions.Where(t=>t.destinationState==death)) transition.canTransitionToSelf=false;
            if(!death.transitions.Any(t=>t.destinationState==idle))
            {
                var t=death.AddTransition(idle);
                t.hasExitTime=false;t.duration=.08f;t.canTransitionToSelf=false;
                t.AddCondition(AnimatorConditionMode.If,0,"Reset");
            }
            // Locomotion transitions must not override combat/death playback.
            // Existing idle/run transitions only evaluate in their own states.
            EditorUtility.SetDirty(ctrl);
            Debug.Log("LOE_COMBAT_CLASS "+Classes[i]+" actionClips=7");
        }
        static Text TextLabel(Transform parent,string name,string value,int size)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Text));
            g.transform.SetParent(parent,false);
            var r=g.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
            var t=g.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;
            t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;
            return t;
        }
        static Button Button(Transform canvas,string name,string caption,float x,float y,float width,float height,UnityEngine.Object target,string method)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(UnityEngine.UI.Button));
            go.transform.SetParent(canvas,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=new Vector2(1,0);rect.anchorMax=new Vector2(1,0);rect.pivot=new Vector2(1,0);
            rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(width,height);
            go.GetComponent<Image>().color=new Color(.07f,.19f,.30f,.75f);
            TextLabel(go.transform,"Caption",caption,21);
            var button=go.GetComponent<UnityEngine.UI.Button>();
            UnityEventTools.AddVoidPersistentListener(button.onClick,(UnityEngine.Events.UnityAction)Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction),target,method));
            return button;
        }
        public static void Run()
        {
            for(int i=0;i<4;i++)ConfigureController(i);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero=all.FirstOrDefault(t=>t.name=="LOE Prototype Hero");
            var canvas=all.FirstOrDefault(t=>t.name=="LOE Mobile HUD");
            if(hero==null||canvas==null)throw new Exception("Missing prototype hero or HUD");
            var combat=hero.GetComponent<PrototypeCombat>() ?? hero.gameObject.AddComponent<PrototypeCombat>();
            var panel=canvas.Find("Combat Actions");
            if(panel==null)
            {
                var g=new GameObject("Combat Actions",typeof(RectTransform));
                g.transform.SetParent(canvas,false);panel=g.transform;
                var r=g.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
                Button(panel,"Attack","АТАКА",-28,185,142,75,combat,"Attack");
                Button(panel,"Skill 1","НАВЫК 1",-185,185,142,75,combat,"Skill1");
                Button(panel,"Skill 2","НАВЫК 2",-342,185,142,75,combat,"Skill2");
                Button(panel,"Skill 3","НАВЫК 3",-185,100,142,75,combat,"Skill3");
                Button(panel,"Skill 4","НАВЫК 4",-342,100,142,75,combat,"Skill4");
                Button(panel,"Take Hit","УРОН",-28,100,142,75,combat,"TakeHit");
                Button(panel,"Die","СМЕРТЬ",-28,20,142,65,combat,"Die");
                Button(panel,"Respawn","ОЖИТЬ",-185,20,142,65,combat,"Respawn");
                var hp=new GameObject("HP",typeof(RectTransform),typeof(Image));
                hp.transform.SetParent(panel,false);
                var hpR=hp.GetComponent<RectTransform>();hpR.anchorMin=new Vector2(1,1);hpR.anchorMax=new Vector2(1,1);
                hpR.pivot=new Vector2(1,1);hpR.sizeDelta=new Vector2(250,57);hpR.anchoredPosition=new Vector2(-25,-150);
                hp.GetComponent<Image>().color=new Color(.18f,.06f,.06f,.75f);
                var text=TextLabel(hp.transform,"Health Value","HP 100 / 100",24);
                var so=new SerializedObject(combat);so.FindProperty("healthLabel").objectReferenceValue=text;so.ApplyModifiedPropertiesWithoutUndo();
            }
            if(!EditorSceneManager.SaveScene(scene))throw new Exception("Scene save failed");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_COMBAT_PROTOTYPE_OK classes=4 actionClips=28 controls=8 hpPreview=1");
        }
    }
}
