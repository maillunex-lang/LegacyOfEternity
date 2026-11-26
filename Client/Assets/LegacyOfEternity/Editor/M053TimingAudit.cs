using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using LegacyOfEternity.Combat;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    public static class M053TimingAudit
    {
        private static readonly string[] ClassNames = { "Warrior", "Assassin", "Mage", "Archer" };

        [MenuItem("Legacy of Eternity/M05.3 Audit Original Impact Timings")]
        public static void Run()
        {
            int checkedCount = 0, over = 0;
            foreach (var (klass, name) in ClassNames.Select((n, i) => ((CharacterClass)i, n)))
            {
                string path = "Assets/LegacyOfEternity/" + name + "Prototype.controller";
                var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (ctrl == null) throw new Exception("Missing original class animation controller: " + path);
                var states = ctrl.layers[0].stateMachine.states.Select(s => s.state).ToArray();
                for (int slot = 0; slot < 5; slot++)
                {
                    string stateName = slot == 0 ? "Attack" : "Skill" + slot;
                    var state = states.FirstOrDefault(s => s.name == stateName);
                    if (state == null || !(state.motion is AnimationClip clip))
                        throw new Exception("Missing original animation " + name + "/" + stateName);
                    float[] hits = CombatImpactProfile.HitTimes(klass, slot);
                    float lastHit = hits.Last();
                    bool exceeds = lastHit > clip.length + .1f;
                    if (exceeds) over++;
                    Debug.Log("LOE_M053_TIMING class=" + name + " slot=" + slot +
                        " clip=" + clip.name + " clipSeconds=" + clip.length.ToString("F3") +
                        " originalLastHit=" + lastHit.ToString("F3") + " beyondClip=" + exceeds);
                    checkedCount++;
                }
            }
            if (over > 0) throw new Exception("Original skill hit markers exceed imported clip length: " + over);
            Debug.Log("LOE_M053_TIMING_AUDIT checked=" + checkedCount + " overClip=" + over +
                " needsAnimationSpeedAdjustment=" + (over > 0));
        }
    }
}
