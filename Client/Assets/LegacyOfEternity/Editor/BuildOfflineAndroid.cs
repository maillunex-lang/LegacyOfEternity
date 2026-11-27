using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace LegacyOfEternity.Editor
{
    /// <summary>
    /// 0.6.0 offline-login APK: builds CoastalPrototype without regenerating the scene.
    /// Audits from M05 are optional via LOE_SKIP_ANDROID_AUDITS=1 for cold Library imports.
    /// </summary>
    public static class BuildOfflineAndroid
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";

        public static void Run()
        {
            Debug.Log("LOE_OFFLINE_ANDROID_START");
            if (Environment.GetEnvironmentVariable("LOE_SKIP_ANDROID_AUDITS") != "1")
            {
                M052WeaponBuilder.Audit();
                M054AutoCombatBuilder.Audit();
                M055CombatHudBuilder.Audit();
                PveArenaAudit.Run();
                M057OriginalSkillIcons.Audit();
                M058OriginalCombatHud.Audit();
                M059OriginalCamera.Audit();
                M0510OriginalControls.Audit();
            }
            else
                Debug.Log("LOE_OFFLINE_ANDROID_AUDITS_SKIPPED");

            var shortToolchain = Environment.GetEnvironmentVariable("LOE_ANDROID_TOOLCHAIN_ROOT");
            if (!string.IsNullOrEmpty(shortToolchain) &&
                Directory.Exists(Path.Combine(shortToolchain, "NDK")) &&
                Directory.Exists(Path.Combine(shortToolchain, "SDK")) &&
                Directory.Exists(Path.Combine(shortToolchain, "OpenJDK")))
            {
                UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = Path.Combine(shortToolchain, "NDK");
                UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = Path.Combine(shortToolchain, "SDK");
                UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = Path.Combine(shortToolchain, "OpenJDK");
                Debug.Log("LOE_OFFLINE_ANDROID_SHORT_TOOLCHAIN=" + shortToolchain);
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.companyName = "Lunex";
            PlayerSettings.productName = "Legacy of Eternity";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lunex.legacyofeternity");
            PlayerSettings.bundleVersion = "0.6.2";
            PlayerSettings.Android.bundleVersionCode = 17;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;

            string output = Path.GetFullPath("../Builds/Android/LegacyOfEternity-0.6.2-alpha.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = BuildTarget.Android,
                locationPathName = output,
                options = BuildOptions.None
            });
            Debug.Log("LOE_OFFLINE_ANDROID_BUILD result=" + report.summary.result +
                      " errors=" + report.summary.totalErrors +
                      " warnings=" + report.summary.totalWarnings +
                      " bytes=" + report.summary.totalSize + " path=" + output);
            if (report.summary.result == BuildResult.Succeeded && File.Exists(output))
                Debug.Log("LOE APK OK " + output);
            else
                throw new Exception("Offline Android build FAILED: " + report.summary.result + " path=" + output);
        }
    }
}

