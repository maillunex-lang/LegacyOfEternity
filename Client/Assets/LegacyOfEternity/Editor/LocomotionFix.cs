using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
namespace LegacyOfEternity.Editor {
public static class LocomotionFix {
static readonly string[] Names={"Warrior","Assassin","Mage","Archer"};
static readonly string[] Folders={"zhanshi/ZS_M_01","cike/CK_W_01","fashi/FS_W_01","GJS/GJS_M_01"};
static readonly string[] Ids={"ZS_M_01","CK_W_01","FS_W_01","GJS_M_01"};
const string Root="Assets/LegacyOfEternity/";
const string Source="Assets/LegacyAssets/AssetsRaw/Animation/player/";
static AnimationClip ConfigureClip(int i,string stem,bool loop) {
 string path=Source+Folders[i]+"/"+Ids[i]+"@"+stem+".FBX";
 var importer=AssetImporter.GetAtPath(path) as ModelImporter;
 if(importer==null)throw new Exception("Missing animation source "+path);
 bool changed=false;
 if(importer.animationType!=ModelImporterAnimationType.Generic){importer.animationType=ModelImporterAnimationType.Generic;changed=true;}
 var clips=importer.clipAnimations;
 if(clips==null||clips.Length==0)clips=importer.defaultClipAnimations;
 if(clips==null||clips.Length==0)throw new Exception("Missing model importer clips "+path);
 foreach(var c in clips){ if(c.loopTime!=loop || c.loopPose!=loop){c.loopTime=loop;c.loopPose=loop;changed=true;}}
 if(changed){importer.clipAnimations=clips;importer.SaveAndReimport();}
 var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(x=>!x.name.StartsWith("__preview__"));
 if(clip==null)throw new Exception("Missing animation clip "+path);
 if(clip.isLooping!=loop)throw new Exception("Loop mismatch "+path+" actual="+clip.isLooping);
 return clip;
}
[MenuItem("Legacy of Eternity/Fix Locomotion")]
public static void Run(){
 for(int i=0;i<4;i++){
 var stand=ConfigureClip(i,"stand",true);
 var run=ConfigureClip(i,"run",true);
 var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(Root+Names[i]+"Prototype.controller");
 if(ctrl==null)throw new Exception("Missing controller "+Names[i]);
 var sm=ctrl.layers[0].stateMachine;
 var idle=sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name=="Idle");
 var moving=sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name=="Run");
 if(idle==null||moving==null)throw new Exception("Missing locomotion states "+Names[i]);
 idle.motion=stand;moving.motion=run;
 idle.speed=1f;moving.speed=1f;
 idle.writeDefaultValues=true;moving.writeDefaultValues=true;
 foreach(var t in idle.transitions.Where(t=>t.destinationState==moving)){
   t.hasExitTime=false;t.duration=.12f;t.canTransitionToSelf=false;
 }
 foreach(var t in moving.transitions.Where(t=>t.destinationState==idle)){
   t.hasExitTime=false;t.duration=.12f;t.canTransitionToSelf=false;
 }
 EditorUtility.SetDirty(ctrl);
 Debug.Log("LOE_LOCOMOTION_CLASS "+Names[i]+" stand="+stand.name+" standLoop="+stand.isLooping+" run="+run.name+" runLoop="+run.isLooping+" runSeconds="+run.length);
 }
 AssetDatabase.SaveAssets();AssetDatabase.Refresh();
 Debug.Log("LOE_LOCOMOTION_FIX_OK classes=4 idleStand=4 loopRun=4");
}
}
}
