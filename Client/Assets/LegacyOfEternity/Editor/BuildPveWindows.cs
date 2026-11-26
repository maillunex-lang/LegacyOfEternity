using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LegacyOfEternity.Editor
{
    public static class BuildPveWindows
    {
        public static void Run()
        {
            const string scene = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
            const string output = "../Builds/Windows/PvE-0.2/LegacyOfEternity.exe";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null)
                throw new Exception("Missing PvE scene " + scene);
            PveArenaAudit.Run();
            Directory.CreateDirectory("../Builds/Windows/PvE-0.2");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new [] { scene },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            Debug.Log("LOE_PVE_WINDOWS_BUILD result=" + report.summary.result +
                " errors=" + report.summary.totalErrors +
                " warnings=" + report.summary.totalWarnings +
                " bytes=" + report.summary.totalSize +
                " path=" + output);
            if (report.summary.result != BuildResult.Succeeded || !File.Exists(output))
                throw new Exception("Windows PvE build failed");
        }
    }
}
