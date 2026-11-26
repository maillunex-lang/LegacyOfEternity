using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using LegacyOfEternity.Player;
namespace LegacyOfEternity.Editor {
public static class HeroBuilder {
const string Base="Assets/LegacyAssets/AssetsRaw/Animation/player/zhanshi/ZS_M_01/";
const string ScenePath="Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
static AnimationClip Clip(string name){
var p=Base+"ZS_M_01@"+name+".FBX";
var imp=AssetImporter.GetAtPath(p) as ModelImporter;
if(imp==null)throw new Exception("Missing "+p);
imp.animationType=ModelImporterAnimationType.Generic;
var clips=imp.defaultClipAnimations;
foreach(var c in clips)c.loopTime=true;
imp.clipAnimations=clips;imp.SaveAndReimport();
return AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
}
public static void Run(){
var p=Base+"ZS_M_01.FBX";
var imp=AssetImporter.GetAtPath(p) as ModelImporter;
if(imp==null)throw new Exception("Missing model");
imp.animationType=ModelImporterAnimationType.Generic;imp.SaveAndReimport();
var idle=Clip("idle");var run=Clip("run");
var model=AssetDatabase.LoadAssetAtPath<GameObject>(p);
var scene=EditorSceneManager.OpenScene(ScenePath);
var hero=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="LOE Prototype Hero").gameObject;
if(hero.transform.Find("Warrior Visual")!=null)throw new Exception("Hero already installed");
var visual=(GameObject)PrefabUtility.InstantiatePrefab(model,scene);
visual.name="Warrior Visual";visual.transform.SetParent(hero.transform,false);
var rs=visual.GetComponentsInChildren<Renderer>();
if(rs.Length==0)throw new Exception("Model has no renderers");
var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
if(b.size.y<0.001f)throw new Exception("Invalid model bounds");
visual.transform.localScale*=1.8f/b.size.y;
b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
visual.transform.position+=hero.transform.position+Vector3.down*0.95f-new Vector3(b.center.x,b.min.y,b.center.z);
var shader=Shader.Find("Standard");
var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"ZS_M_01.tga");
if(tex==null||shader==null)throw new Exception("Missing texture or shader");
var mat=new Material(shader){mainTexture=tex};
AssetDatabase.CreateAsset(mat,"Assets/LegacyOfEternity/WarriorPrototype.mat");
foreach(var r in rs)r.sharedMaterials=r.sharedMaterials.Select(m=>mat).ToArray();
var ctrl=AnimatorController.CreateAnimatorControllerAtPath("Assets/LegacyOfEternity/WarriorPrototype.controller");
ctrl.AddParameter("Speed",AnimatorControllerParameterType.Float);
var sm=ctrl.layers[0].stateMachine;var a=sm.AddState("Idle");a.motion=idle;var bstate=sm.AddState("Run");bstate.motion=run;sm.defaultState=a;
var tr=a.AddTransition(bstate);tr.hasExitTime=false;tr.duration=.12f;tr.AddCondition(AnimatorConditionMode.Greater,.15f,"Speed");
tr=bstate.AddTransition(a);tr.hasExitTime=false;tr.duration=.12f;tr.AddCondition(AnimatorConditionMode.Less,.15f,"Speed");
var animator=visual.GetComponent<Animator>();if(animator==null)animator=visual.AddComponent<Animator>();animator.runtimeAnimatorController=ctrl;animator.applyRootMotion=false;
var capsule=hero.GetComponent<MeshRenderer>();if(capsule!=null)capsule.enabled=false;
var driver=hero.AddComponent<HeroAnimation>();var so=new SerializedObject(driver);so.FindProperty("animator").objectReferenceValue=animator;so.ApplyModifiedPropertiesWithoutUndo();
if(!EditorSceneManager.SaveScene(scene))throw new Exception("Save failed");
AssetDatabase.SaveAssets();
Debug.Log("LOE_HERO_OK renderers="+rs.Length+" idle="+idle.length+" run="+run.length);
}
}
}
