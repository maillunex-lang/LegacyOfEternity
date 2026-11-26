using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LegacyOfEternity.Editor {
public static class M053ShaderDiagnostics {
 public static void Run() {
  var root="Assets/LegacyOfEternity/Effects";
  var lines=new List<string>();
  var names=new Dictionary<string,int>();
  int missing=0, unsupported=0, empty=0, total=0, rendererCount=0;
  foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{root})) {
    var path=AssetDatabase.GUIDToAssetPath(guid);
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
    if(prefab==null)continue;
    foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true)) {
      rendererCount++;
      var mats=renderer.sharedMaterials;
      for(int i=0;i<mats.Length;i++){
        total++;
        var mat=mats[i];
        if(mat==null){missing++;lines.Add("MISSING_MATERIAL|"+path+"|"+renderer.name+"|"+i);continue;}
        var shader=mat.shader;
        string shaderName=shader!=null?shader.name:"NULL";
        if(!names.ContainsKey(shaderName))names[shaderName]=0;
        names[shaderName]++;
        if(shader==null || shaderName=="Hidden/InternalErrorShader" || !shader.isSupported){
          unsupported++;lines.Add("UNSUPPORTED|"+path+"|"+renderer.name+"|"+mat.name+"|"+shaderName);
        }
        var candidates=new[]{"_MainTex","_Texture","_Tex","_mask"};
        string texInfo=string.Join(";",candidates.Where(k=>mat.HasProperty(k))
          .Select(k=>k+":"+(mat.GetTexture(k)!=null?mat.GetTexture(k).name:"NONE")));
        if(!candidates.Any(k=>mat.HasProperty(k) && mat.GetTexture(k)!=null)) {
          empty++;lines.Add("NO_TEXTURE|"+path+"|"+renderer.name+"|"+mat.name+"|"+shaderName+"|"+texInfo);
        }
        if(mat.HasProperty("_Color"))lines.Add("COLOR|"+path+"|"+mat.name+"|"+shaderName+"|"+mat.GetColor("_Color").ToString("F3"));
        if(mat.HasProperty("_TintColor"))lines.Add("TINT|"+path+"|"+mat.name+"|"+shaderName+"|"+mat.GetColor("_TintColor").ToString("F3"));
        lines.Add("MAT|"+path+"|"+renderer.name+"|"+mat.name+"|"+shaderName+"|"+AssetDatabase.GetAssetPath(mat)+"|"+texInfo);
      }
    }
  }
  lines.Insert(0,"SUMMARY|prefabs=20|renderers="+rendererCount+"|slots="+total+"|missingMaterial="+missing+"|unsupported="+unsupported+"|missingTexture="+empty);
  foreach(var kv in names.OrderByDescending(x=>x.Value))lines.Insert(1,"SHADER|"+kv.Key+"|"+kv.Value);
  var target=Path.GetFullPath("../Docs/M053-shader-diagnostics.txt");
  Directory.CreateDirectory(Path.GetDirectoryName(target));
  File.WriteAllLines(target,lines);
  Debug.Log("LOE_VFX_DIAG_SUMMARY slots="+total+" missing="+missing+" unsupported="+unsupported+" emptyTextures="+empty+" report="+target);
  if(unsupported!=0)throw new Exception("VFX references unsupported original shaders: "+unsupported);
}
}}
