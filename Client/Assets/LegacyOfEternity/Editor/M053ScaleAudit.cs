using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LegacyOfEternity.Editor {
public static class M053ScaleAudit {
 public static void Run(){
  string root="Assets/LegacyAssets/Resources/Effect/skill/player/";
  string[] cs={"ZS_M","CK_W","FS_W","GJS_M"};
  string[] actions={"attack001","skill001","skill002","skill003","skill004"};
  var sb=new StringBuilder("Class,Action,OriginalRootScale,AdaptedRootScale,VisualScale,CharacterHeightWorld,EffectSizeWorld,EffectToHeroRatio,NormalPrefabExists,NormalScale\n");
  var scene=EditorSceneManager.OpenScene("Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity");
  var hero=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Transform>(true)).First(x=>x.name=="LOE Prototype Hero");
  for(int j=0;j<cs.Length;j++){
    var visual=hero.Find(new[]{"Warrior Visual","Assassin Visual","Mage Visual","Archer Visual"}[j]);
    var renderers=visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    Bounds box=renderers[0].bounds;
    foreach(var r in renderers)box.Encapsulate(r.bounds);
    float height=box.size.y;
    for(int k=0;k<actions.Length;k++){
      string suffix=cs[j]=="GJS_M"?"_s":"_S";
      string name=cs[j]+"_"+actions[k];
      var original=AssetDatabase.LoadAssetAtPath<GameObject>(root+cs[j]+"/"+name+suffix+".prefab");
      var normal=AssetDatabase.LoadAssetAtPath<GameObject>(root+cs[j]+"/"+name+".prefab");
      var adapted=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LegacyOfEternity/Effects/"+cs[j]+"/"+name+suffix+"_Unity6.prefab");
      if(original==null||adapted==null)throw new Exception("Missing FX "+name);
      var ef=adapted.GetComponentsInChildren<Renderer>(true);
      if(ef.Length==0)throw new Exception("No FX renderer "+name);
      var efBox=ef[0].bounds;foreach(var r in ef)efBox.Encapsulate(r.bounds);
      var ratio=efBox.size.magnitude/height;
      sb.AppendLine(string.Join(",",cs[j],actions[k],original.transform.localScale.ToString("F3"),adapted.transform.localScale.ToString("F3"),visual.localScale.ToString("F3"),height.ToString("F4"),efBox.size.ToString("F3"),ratio.ToString("F3"),normal!=null,normal!=null?normal.transform.localScale.ToString("F3"):""));
    }
  }
  var path=Path.GetFullPath("../Docs/M053-effect-original-scale-audit.csv");
  File.WriteAllText(path,sb.ToString());
  Debug.Log("LOE_VFX_SCALE_AUDIT file="+path+"\n"+sb);
 }
}}