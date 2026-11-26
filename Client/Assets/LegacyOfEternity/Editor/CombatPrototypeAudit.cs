using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    public static class CombatPrototypeAudit
    {
        private static readonly string[] Classes = {"Warrior","Assassin","Mage","Archer"};
        private static readonly string[] ActionStates = {"Attack","Skill1","Skill2","Skill3","Skill4","Hit","Death"};
        private static readonly string[] ExpectedButtons = {"Attack","Skill 1","Skill 2","Skill 3","Skill 4","Take Hit","Die","Respawn"};
        public static void Run()
        {
            var problems=new List<string>();
            int totalClips=0,animatorCurves=0,matchedPaths=0,unmatchedPaths=0;
            for(int i=0;i<Classes.Length;i++)
            {
                var name=Classes[i];
                var path="Assets/LegacyOfEternity/"+name+"Prototype.controller";
                var c=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if(c==null){problems.Add(name+": controller missing");continue;}
                var sm=c.layers[0].stateMachine;
                foreach(var stateName in ActionStates)
                {
                    var state=sm.states.Select(x=>x.state).FirstOrDefault(x=>x.name==stateName);
                    if(state==null || !(state.motion is AnimationClip clip)){problems.Add(name+": missing state clip "+stateName);continue;}
                    totalClips++;
                    var bindings=AnimationUtility.GetCurveBindings(clip);
                    animatorCurves+=bindings.Length;
                    if(bindings.Length==0)problems.Add(name+": no curves "+stateName);
                    Debug.Log("LOE_COMBAT_CLIP "+name+" "+stateName+" "+clip.name+" curves="+bindings.Length+" duration="+clip.length.ToString("F2"));
                }
            }
            var scene=EditorSceneManager.OpenScene("Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity");
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero=all.FirstOrDefault(t=>t.name=="LOE Prototype Hero");
            var hud=all.FirstOrDefault(t=>t.name=="LOE Mobile HUD");
            if(hero==null||hud==null)problems.Add("scene hero or HUD missing");
            if(hero!=null)
            {
                var sel=hero.GetComponent<CharacterClassSelector>();
                var combat=hero.GetComponent<PrototypeCombat>();
                if(sel==null||combat==null)problems.Add("selector or combat component missing");
                foreach(var n in Classes)
                {
                    var visual=hero.Find(n+" Visual");
                    if(visual==null){problems.Add("Visual missing: "+n);continue;}
                    var animator=visual.GetComponent<Animator>();
                    if(animator==null||animator.runtimeAnimatorController==null)problems.Add("Animator not bound "+n);
                    if(visual.GetComponentsInChildren<Renderer>(true).Length==0)problems.Add("Renderer missing "+n);
                    if(animator==null)continue;
                    var paths=new HashSet<string>(visual.GetComponentsInChildren<Transform>(true).Select(t=>{
                        var seg=new List<string>();while(t!=visual){seg.Add(t.name);t=t.parent;}seg.Reverse();return string.Join("/",seg);
                    }));
                    var controller=animator.runtimeAnimatorController as AnimatorController;
                    if(controller==null)continue;
                    var clip=controller.layers[0].stateMachine.states.Select(x=>x.state).First(x=>x.name=="Attack").motion as AnimationClip;
                    if(clip==null)continue;
                    var bindings=AnimationUtility.GetCurveBindings(clip);
                    int matched=bindings.Count(x=>paths.Contains(x.path));
                    int unmatched=bindings.Length-matched;
                    matchedPaths+=matched;unmatchedPaths+=unmatched;
                    Debug.Log("LOE_COMBAT_BINDINGS "+n+" matched="+matched+" unmatched="+unmatched+" paths="+paths.Count);
                }
            }
            if(hud!=null)
            {
                var panel=hud.Find("Combat Actions");
                if(panel==null)problems.Add("Combat HUD panel missing");
                else foreach(var n in ExpectedButtons)
                {
                    var b=panel.Find(n)?.GetComponent<Button>();
                    if(b==null)problems.Add("Button missing "+n);
                    else if(b.onClick.GetPersistentEventCount()!=1)problems.Add("Button listener wrong "+n+": "+b.onClick.GetPersistentEventCount());
                    else Debug.Log("LOE_COMBAT_UI "+n+" => "+b.onClick.GetPersistentMethodName(0));
                }
            }
            Debug.Log("LOE_COMBAT_AUDIT results clips="+totalClips+" curves="+animatorCurves+" matched="+matchedPaths+" unmatched="+unmatchedPaths+" issues="+problems.Count);
            foreach(var p in problems)Debug.LogError("LOE_COMBAT_ISSUE "+p);
            if(problems.Count>0)throw new Exception("Combat audit failed with "+problems.Count+" problems");
        }
    }
}
