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
    /// M05 Android alpha: builds the CURRENT verified PvE scene, without running
    /// older scene generators that could overwrite mounted weapons or the AUTO HUD.
    /// </summary>
    public static class BuildM05Android
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";

        public static void Run()
        {
            Debug.Log("LOE_M05_ANDROID_START: scene audits first, no legacy scene rebuilding");
            M052WeaponBuilder.Audit();
            M054AutoCombatBuilder.Audit();
            M055CombatHudBuilder.Audit();
            PveArenaAudit.Run();
            M053OriginalVfxBuilder.Audit();
            M053TimingAudit.Run();
            M053ShaderDiagnostics.Run();
            M057OriginalSkillIcons.Audit();
            M058OriginalCombatHud.Audit();
            M059OriginalCamera.Audit();
            M0510OriginalControls.Audit();

            var shortToolchain = Environment.GetEnvironmentVariable("LOE_ANDROID_TOOLCHAIN_ROOT");
            if (!string.IsNullOrEmpty(shortToolchain) &&
                Directory.Exists(Path.Combine(shortToolchain, "NDK")) &&
                Directory.Exists(Path.Combine(shortToolchain, "SDK")) &&
                Directory.Exists(Path.Combine(shortToolchain, "OpenJDK")))
            {
                UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = Path.Combine(shortToolchain, "NDK");
                UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = Path.Combine(shortToolchain, "SDK");
                UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = Path.Combine(shortToolchain, "OpenJDK");
                Debug.Log("LOE_M05_ANDROID_SHORT_TOOLCHAIN=" + shortToolchain);
            }

            // M05.11: Unity's generated Gradle settings should use standard public repositories.
            // Older Unity mirror repositories may not resolve Android Gradle Plugin dependencies.
            string settingsTemplatePath = "Assets/Plugins/Android/settingsTemplate.gradle";
            Debug.Log("LOE_M0511_GRADLE_REPOS=" + settingsTemplatePath);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.companyName = "Lunex";
            PlayerSettings.productName = "Legacy of Eternity";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lunex.legacyofeternity");
            PlayerSettings.bundleVersion = "0.5.5";
            PlayerSettings.Android.bundleVersionCode = 14;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            // Preserve the Unity debug signing identity used for previous alpha APKs.
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;

            string output = Path.GetFullPath("../Builds/Android/LegacyOfEternity-0.5.5-alpha.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = BuildTarget.Android,
                locationPathName = output,
                options = BuildOptions.None
            });
            Debug.Log("LOE_M05_ANDROID_BUILD result=" + report.summary.result +
                      " errors=" + report.summary.totalErrors +
                      " warnings=" + report.summary.totalWarnings +
                      " bytes=" + report.summary.totalSize + " path=" + output);
            // M05.11: preserve individual Unity BuildReport diagnostics.
            string diagnosticsPath = Path.GetFullPath("../Docs/unity-android-0.5.5-buildreport.txt");
            using (var writer = new StreamWriter(diagnosticsPath, false, new System.Text.UTF8Encoding(false)))
            {
                writer.WriteLine("Result: " + report.summary.result);
                writer.WriteLine("Errors: " + report.summary.totalErrors);
                writer.WriteLine("Warnings: " + report.summary.totalWarnings);
                foreach (var step in report.steps)
                {
                    writer.WriteLine("STEP: " + step.name);
                    foreach (var msg in step.messages)
                        writer.WriteLine(msg.type + ": " + msg.content);
                }
            }
            Debug.Log("LOE_M0511_BUILDREPORT=" + diagnosticsPath);
            if (report.summary.result != BuildResult.Succeeded || !File.Exists(output))
                throw new Exception("M05 Android build FAILED: " + report.summary.result + " path=" + output);
        }
    }
}
