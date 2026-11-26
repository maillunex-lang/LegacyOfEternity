using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LegacyOfEternity.Editor {
public static class AnimationLibraryAudit {
    public static void Run() {
        const string library="Assets/LegacyAssets/AssetsRaw/Animation";
        if (!AssetDatabase.IsValidFolder(library)) throw new Exception("Library folder not found");
        var guids=AssetDatabase.FindAssets("t:AnimationClip",new[]{library});
        int ok=0,failed=0;
        foreach(var guid in guids) {
            var p=AssetDatabase.GUIDToAssetPath(guid);
            if(AssetDatabase.LoadAssetAtPath<AnimationClip>(p)!=null) ok++;
            else {failed++;Debug.LogError("LOE_ANIM_UNLOADABLE "+p);}
        }
        var allFiles=Directory.GetFiles(library,"*",SearchOption.AllDirectories);
        int fbx=allFiles.Count(p=>p.EndsWith(".FBX",StringComparison.OrdinalIgnoreCase));
        int anim=allFiles.Count(p=>p.EndsWith(".anim",StringComparison.OrdinalIgnoreCase));
        var report="{\"importedAnimationClipAssets\":"+ok+",\"failed\":"+failed+",\"fbxFiles\":"+fbx+",\"animFiles\":"+anim+"}";
        File.WriteAllText("../Docs/unity-animation-audit.json",report);
        Debug.Log("LOE_ANIMATION_AUDIT_OK clipAssets="+ok+" failed="+failed+" fbxFiles="+fbx+" animFiles="+anim);
        if(failed>0)throw new Exception("AnimationClip load errors: "+failed);
    }
}
}
