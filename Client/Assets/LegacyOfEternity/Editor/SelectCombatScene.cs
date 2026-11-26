using System;
using UnityEditor;
using UnityEngine;

namespace LegacyOfEternity.Editor
{
    public static class SelectCombatScene
    {
        public static void Run()
        {
            const string path="Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
                throw new Exception("Missing combat prototype scene "+path);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};
            var enabled=EditorBuildSettings.scenes;
            if(enabled.Length!=1 || enabled[0].path!=path || !enabled[0].enabled)
                throw new Exception("Build scene verification failed");
            Debug.Log("LOE_COMBAT_BUILD_SCENE_OK "+enabled[0].path);
        }
    }
}
