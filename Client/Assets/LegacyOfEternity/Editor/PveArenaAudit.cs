using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Combat;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    public static class PveArenaAudit
    {
        [MenuItem("Legacy of Eternity/Audit PvE Arena")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity");
            var root = scene.GetRootGameObjects();
            var all = root.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hero = all.FirstOrDefault(x => x.name == "LOE Prototype Hero");
            var hud = all.FirstOrDefault(x => x.name == "LOE Mobile HUD");
            var monsters = all.FirstOrDefault(x => x.name == "LOE PvE Arena");
            var errors = new List<string>();
            if (hero == null || hud == null || monsters == null)
                errors.Add("Required hero, arena or HUD is missing");
            CombatEnemy[] enemies = monsters != null ? monsters.GetComponentsInChildren<CombatEnemy>(true) : Array.Empty<CombatEnemy>();
            if (enemies.Length != 3) errors.Add("Expected 3 enemies but found " + enemies.Length);
            int curves = 0;
            int matched = 0;
            int unmatched = 0;
            int verifiedClips = 0;
            foreach (var enemy in enemies)
            {
                var visual = enemy.transform.Find(enemy.name.Replace(" Enemy"," Visual"));
                if (visual == null) { errors.Add("Missing enemy visual " + enemy.name); continue; }
                var animator = visual.GetComponent<Animator>();
                var cc = enemy.GetComponent<CharacterController>();
                if (cc == null || animator == null || animator.runtimeAnimatorController == null)
                { errors.Add("Missing enemy CC/Animator " + enemy.name); continue; }
                var controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller == null || controller.layers.Length != 1) { errors.Add("Invalid controller " + enemy.name); continue; }
                if (visual.GetComponentsInChildren<Renderer>(true).Length < 1) errors.Add("Missing mesh " + enemy.name);
                var paths = new HashSet<string>(visual.GetComponentsInChildren<Transform>(true).Select(t =>
                {
                    var parts = new List<string>();
                    while (t != visual) { parts.Add(t.name); t = t.parent; }
                    parts.Reverse(); return string.Join("/", parts);
                }));
                foreach (var name in new[] {"Idle","Run","Attack","Hit","Death"})
                {
                    var state = controller.layers[0].stateMachine.states.Select(s => s.state).FirstOrDefault(s => s.name == name);
                    var clip = state != null ? state.motion as AnimationClip : null;
                    if (clip == null) { errors.Add("Missing " + name + " in " + enemy.name); continue; }
                    verifiedClips++;
                    if ((name == "Idle" || name == "Run") != clip.isLooping) errors.Add("Incorrect loop " + name + " " + enemy.name);
                    var binding = AnimationUtility.GetCurveBindings(clip);
                    curves += binding.Length;
                    int found = binding.Count(b => paths.Contains(b.path));
                    matched += found;
                    unmatched += binding.Length - found;
                    var missing = binding.Where(b => !paths.Contains(b.path)).Select(b => b.path).Distinct().ToArray();
                    if (missing.Length > 0) Debug.Log("LOE_PVE_UNMATCHED " + enemy.name + " " + name + " missingPaths=" + string.Join(",", missing.Take(8)));
                    if (found == 0 || binding.Length == 0) errors.Add("No skeleton matches for " + name + " " + enemy.name);
                    Debug.Log("LOE_PVE_CLIP " + enemy.name + " " + name + " curves=" + binding.Length + " matched=" + found + " unmatched=" + (binding.Length-found));
                }
                float distance = Vector3.Distance(enemy.transform.position, hero != null ? hero.position : Vector3.zero);
                if (distance > 16f) errors.Add("Enemy too far " + enemy.name + " " + distance);
                Debug.Log("LOE_PVE_SPAWN " + enemy.name + " position=" + enemy.transform.position + " hp=" + enemy.MaxHealth);
            }
            if (hero != null && hero.GetComponent<CombatEncounter>() == null) errors.Add("No hero combat encounter");
            if (hud != null)
            {
                var panel = hud.Find("PvE Target HUD");
                if (panel?.Find("Target Name")?.GetComponent<Text>() == null || panel.Find("Target Message")?.GetComponent<Text>() == null)
                    errors.Add("Target HUD incomplete");
                var b = hud.Find("Select Next Target")?.GetComponent<Button>();
                if (b == null || b.onClick.GetPersistentEventCount() != 1 || b.onClick.GetPersistentMethodName(0) != "TargetNext")
                    errors.Add("Target selection button missing/wrong listener");
            }
            Debug.Log("LOE_PVE_AUDIT monsters=" + enemies.Length + " clips=" + verifiedClips + " curves=" + curves + " matched=" + matched + " unmatched=" + unmatched + " issues=" + errors.Count);
            foreach (var error in errors) Debug.LogError("LOE_PVE_AUDIT_ISSUE " + error);
            if (errors.Count > 0) throw new Exception("PvE arena audit has " + errors.Count + " problems");
        }
    }
}
