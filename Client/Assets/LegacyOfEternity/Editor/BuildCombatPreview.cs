using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LegacyOfEternity.Editor
{
    public static class BuildCombatPreview
    {
        public static void Run()
        {
            const string scene="Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
            const string output="../Builds/Windows/LegacyOfEternity.exe";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)==null)
                throw new Exception("Scene missing: "+scene);
            Directory.CreateDirectory("../Builds/Windows");
            var options=new BuildPlayerOptions
            {
                scenes=new []{scene},
                locationPathName=output,
                target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.Development
            };
            var report=BuildPipeline.BuildPlayer(options);
            Debug.Log("LOE_COMBAT_WINDOWS_BUILD result="+report.summary.result+
                " errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+
                " size="+report.summary.totalSize+" destination="+output);
            if(report.summary.result!=BuildResult.Succeeded || !File.Exists(output))
                throw new Exception("Preview build failed");
        }
    }
}
