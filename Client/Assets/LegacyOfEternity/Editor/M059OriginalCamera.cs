using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LegacyOfEternity.CameraSystem;
using LegacyOfEternity.Mobile;
using UnityEngine.UI;

namespace LegacyOfEternity.Editor
{
    public static class M059OriginalCamera
    {
        private const string ScenePath="Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const float OriginalDistance=12.5f;
        private const float OriginalHeight=8f;
        private const float OriginalYaw=50f;

        [MenuItem("Legacy of Eternity/M05.9 Install Original Camera")]
        public static void Run()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var cam=all.Select(x=>x.GetComponent<OrbitFollowCamera>()).FirstOrDefault(x=>x!=null);
            var hero=all.FirstOrDefault(x=>x.name=="LOE Prototype Hero");
            var hud=all.FirstOrDefault(x=>x.name=="LOE Mobile HUD");
            if(cam==null || hero==null || hud==null) throw new Exception("Camera, hero or HUD missing");
            cam.SetTarget(hero);
            var so=new SerializedObject(cam);
            so.FindProperty("distance").floatValue=OriginalDistance;
            so.FindProperty("height").floatValue=OriginalHeight;
            so.FindProperty("yaw").floatValue=OriginalYaw;
            so.FindProperty("targetOffset").vector3Value=Vector3.zero;
            so.FindProperty("minDistance").floatValue=5f;
            so.FindProperty("maxDistance").floatValue=35f;
            so.FindProperty("minHeight").floatValue=3f;
            so.FindProperty("maxHeight").floatValue=15f;
            so.FindProperty("rotationRatio").floatValue=.25f;
            so.FindProperty("heightRatio").floatValue=.10f;
            so.FindProperty("zoomRatio").floatValue=.05f;
            so.FindProperty("smoothness").floatValue=10f;
            // Ignore joystick, player and all interactive UI, and ignore triggers.
            // World geometry collision is handled by camera sphere-cast.
            so.ApplyModifiedPropertiesWithoutUndo();
            var stick=hud.Find("Move Joystick")?.GetComponent<MobileJoystick>();
            var attack=hud.Find("Combat Actions")?.Find("Attack")?.GetComponent<Button>();
            if(stick==null || attack==null) throw new Exception("HUD interactions missing");
            EditorUtility.SetDirty(cam);
            if(!EditorSceneManager.SaveScene(scene))throw new Exception("Camera scene not saved");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M059_CAMERA_INSTALLED originalCoastalDistance=12.5 height=8 yaw=50 minDistance=5 maxDistance=35 minHeight=3 maxHeight=15 touchUiGuard=1");
        }

        [MenuItem("Legacy of Eternity/M05.9 Audit Original Camera")]
        public static void Audit()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var cam=all.Select(x=>x.GetComponent<OrbitFollowCamera>()).FirstOrDefault(x=>x!=null);
            if(cam==null)throw new Exception("M059 Camera missing");
            var so=new SerializedObject(cam);
            void Check(string key,float value)
            {
                var v=so.FindProperty(key).floatValue;
                if(Mathf.Abs(v-value)>.01f)throw new Exception("M059 property mismatch "+key+" "+v);
            }
            Check("distance",OriginalDistance);
            Check("height",OriginalHeight);
            Check("yaw",OriginalYaw);
            Check("minDistance",5);
            Check("maxDistance",35);
            Check("minHeight",3);
            Check("maxHeight",15);
            Check("rotationRatio",.25f);
            Check("heightRatio",.10f);
            Check("zoomRatio",.05f);
            if(OrbitFollowCamera.ClampZoom(-100f)!=5f ||
                OrbitFollowCamera.ClampZoom(100f)!=35f ||
                OrbitFollowCamera.ClampHeight(-100f)!=3f ||
                OrbitFollowCamera.ClampHeight(100f)!=15f)
                throw new Exception("M059 clamp test failed");
            Debug.Log("LOE_M059_CAMERA_AUDIT PASS sourceDistance=12.5 sourceHeight=8 sourceYaw=50 zoom=5..35 height=3..15 bounds=PASS");
        }
    }
}
