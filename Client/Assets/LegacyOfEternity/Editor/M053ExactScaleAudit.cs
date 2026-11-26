using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LegacyOfEternity.Editor {
public static class M053ExactScaleAudit {
 public static void Run() {
  string[] cs={"ZS_M","CK_W","FS_W","GJS_M"};
  string[] classes={"Warrior","Assassin","Mage","Archer"};
  var sb=new StringBuilder();
  var scene=EditorSceneManager.OpenScene("Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity");
  var hero=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).First(r=>r.name=="LOE Prototype Hero");
  sb.AppendLine("Class | SourceModelHeight | RuntimeModelHeight | RuntimeScale | SourcePrefabScale | FXScaleBasedOnSourceRig");
  for(int i=0;i<4;i++){
    var v=hero.Find(classes[i]+" Visual");
    var model=AssetDatabase.LoadAssetAtPath<GameObject>(
      "Assets/LegacyAssets/AssetsRaw/Animation/player/"+new[]{"zhanshi/ZS_M_01/ZS_M_01","cike/CK_W_01/CK_W_01","fashi/FS_W_01/FS_W_01","GJS/GJS_M_01/GJS_M_01"}[i]+".FBX");
    if(v==null||model==null)throw new Exception("Character asset missing "+classes[i]);
    var modelRenderers=model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    if(modelRenderers.Length==0)throw new Exception("Character mesh missing");
    float originalHeight=0;
    foreach(var r in modelRenderers){
      if(r.sharedMesh==null)continue;
      originalHeight=Mathf.Max(originalHeight, r.sharedMesh.bounds.size.y);
    }
    var vr=v.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    Bounds b=vr[0].bounds;foreach(var rr in vr)b.Encapsulate(rr.bounds);
    float multiplier=v.lossyScale.x;
    sb.AppendLine(classes[i]+"|"+originalHeight.ToString("F4")+"|"+b.size.y.ToString("F4")+"|"+v.localScale.x.ToString("F4")+"|1.0000|"+multiplier.ToString("F4"));
    Debug.Log("LOE_SCALE_CLASS class="+classes[i]+" importedHeight="+originalHeight+" normalizedHeight="+b.size.y+" normalizedLocalScale="+v.localScale.x);
  }
  var path=Path.GetFullPath("../Docs/M053-original-relative-scale.txt");
  File.WriteAllText(path,sb.ToString());Debug.Log("LOE_SCALE_AUDIT_FILE "+path);
 }
}}