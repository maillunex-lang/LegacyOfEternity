using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LegacyOfEternity.Editor {
public static class CoastalAudit {
public static void Run() {
var s=EditorSceneManager.OpenScene("Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity");
var ts=s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
var rs=ts.SelectMany(t=>t.GetComponents<Renderer>()).Where(r=>r.enabled && r.gameObject.activeInHierarchy).ToArray();
var ms=rs.SelectMany(r=>r.sharedMaterials).ToArray();
Debug.Log("LOE_AUDIT renderers="+rs.Length+" nullMaterials="+ms.Count(m=>m==null)+" unsupportedShaders="+ms.Count(m=>m!=null&&(m.shader==null||!m.shader.isSupported))+" colliders="+ts.SelectMany(t=>t.GetComponents<Collider>()).Count(c=>c.enabled&&c.gameObject.activeInHierarchy));
foreach(var m in ms.Where(m=>m!=null).Distinct().OrderBy(m=>m.name)) Debug.Log("LOE_MATERIAL "+m.name+" shader="+(m.shader==null?"NULL":m.shader.name)+" supported="+(m.shader!=null&&m.shader.isSupported));
var cam=ts.SelectMany(t=>t.GetComponents<Camera>()).Single(c=>c.enabled&&c.gameObject.activeInHierarchy);
Debug.Log("LOE_CAMERA "+cam.transform.position+" graphics="+SystemInfo.graphicsDeviceType);
if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null){
var rt=new RenderTexture(1280,720,24); cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
System.IO.File.WriteAllBytes("../Docs/coastal-preview.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);
}
}
}
}
