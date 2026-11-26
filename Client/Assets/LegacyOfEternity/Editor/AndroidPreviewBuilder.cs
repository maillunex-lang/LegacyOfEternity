using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace LegacyOfEternity.Editor {
public static class AndroidPreviewBuilder {
const string ScenePath="Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
public static void Polish(){
var scene=EditorSceneManager.OpenScene(ScenePath);
RenderSettings.ambientMode=AmbientMode.Trilight;
RenderSettings.ambientSkyColor=new Color(.65f,.72f,.82f);
RenderSettings.ambientEquatorColor=new Color(.48f,.49f,.46f);
RenderSettings.ambientGroundColor=new Color(.30f,.32f,.34f);
RenderSettings.ambientIntensity=1f;
RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.59f,.70f,.79f);RenderSettings.fogStartDistance=45;RenderSettings.fogEndDistance=180;
var sky=Shader.Find("Skybox/Procedural");
if(sky!=null){
const string path="Assets/LegacyOfEternity/CoastalDaylight.mat";
var m=AssetDatabase.LoadAssetAtPath<Material>(path);
if(m==null){m=new Material(sky);AssetDatabase.CreateAsset(m,path);}
m.SetColor("_SkyTint",new Color(.65f,.77f,.9f));m.SetColor("_GroundColor",new Color(.52f,.58f,.61f));m.SetFloat("_Exposure",1.1f);RenderSettings.skybox=m;
}
var sun=GameObject.Find("LOE Sunlight").GetComponent<Light>();sun.intensity=1.05f;sun.color=new Color(1,.94f,.84f);sun.shadowStrength=.55f;sun.shadowBias=.08f;sun.shadowNormalBias=.45f;RenderSettings.sun=sun;
var camera=Camera.main;camera.clearFlags=CameraClearFlags.Skybox;camera.fieldOfView=55;
var follow=camera.GetComponent<LegacyOfEternity.CameraSystem.OrbitFollowCamera>();
if(follow!=null){var so=new SerializedObject(follow);so.FindProperty("distance").floatValue=7.5f;so.FindProperty("pitch").floatValue=30f;so.ApplyModifiedPropertiesWithoutUndo();}
QualitySettings.shadowDistance=45;QualitySettings.shadowResolution=ShadowResolution.Medium;QualitySettings.antiAliasing=2;
var hud=GameObject.Find("LOE Mobile HUD");
if(hud!=null){
var pad=hud.transform.Find("Move Joystick");if(pad!=null){
const string texPath="Assets/LegacyOfEternity/JoystickCircle.asset";
var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
if(tex==null){tex=new Texture2D(128,128,TextureFormat.RGBA32,false);for(int y=0;y<128;y++)for(int x=0;x<128;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f));tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(63-d)));}tex.Apply();AssetDatabase.CreateAsset(tex,texPath);}
const string spPath="Assets/LegacyOfEternity/JoystickCircleSprite.asset";
var sp=AssetDatabase.LoadAssetAtPath<Sprite>(spPath);if(sp==null){sp=Sprite.Create(tex,new Rect(0,0,128,128),new Vector2(.5f,.5f));AssetDatabase.CreateAsset(sp,spPath);}
pad.GetComponent<Image>().sprite=sp;pad.GetComponent<Image>().color=new Color(.12f,.19f,.25f,.55f);
var knob=pad.Find("Handle");if(knob!=null){knob.GetComponent<Image>().sprite=sp;knob.GetComponent<Image>().raycastTarget=false;}
}}
EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("LOE_ANDROID_POLISH_OK");
}
public static void Build(){
// Use the short-path junction on this build host only. New machines use Unity Hub defaults.
if(Directory.Exists("C:/loe-android/NDK")) {
UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath="C:/loe-android/NDK";
UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath="C:/loe-android/SDK";
UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath="C:/loe-android/OpenJDK";
}
LocomotionFix.Run();
Polish();
EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
PlayerSettings.productName="Legacy of Eternity";PlayerSettings.companyName="Lunex";
PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.lunex.legacyofeternity");
PlayerSettings.bundleVersion="0.1.1";PlayerSettings.Android.bundleVersionCode=2;
PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
PlayerSettings.Android.useCustomKeystore=false;
EditorUserBuildSettings.buildAppBundle=false;
var output=Path.GetFullPath("../Builds/Android/LegacyOfEternity-0.1.1-alpha.apk");Directory.CreateDirectory(Path.GetDirectoryName(output));
var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None});
Debug.Log("LOE_ANDROID_BUILD result="+report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+" bytes="+report.summary.totalSize+" path="+output);
if(report.summary.result!=BuildResult.Succeeded||!File.Exists(output))throw new Exception("Android build failed");
}
}}
