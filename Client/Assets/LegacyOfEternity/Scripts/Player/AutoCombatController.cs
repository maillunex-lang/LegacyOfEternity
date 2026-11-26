using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Player
{
    /// <summary>Local, foreground-only PvE automation. All damage still goes through PrototypeCombat.</summary>
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PrototypeCombat))]
    [RequireComponent(typeof(CombatEncounter))]
    public sealed class AutoCombatController : MonoBehaviour
    {
        [SerializeField, Min(3f)] private float detectionRadius = 16f;
        [SerializeField, Min(.5f)] private float stuckTimeout = 1.75f;
        [SerializeField] private Text stateLabel;
        [SerializeField] private bool autoEnabled = false;

        private PlayerMotor motor;
        private PrototypeCombat hero;
        private CombatEncounter encounter;
        private readonly Dictionary<int, float> unreachableUntil = new Dictionary<int, float>();
        private float nextAttackDecision;
        private float nextAcquisition;
        private float stuckSince;
        private Vector3 lastPosition;
        private CombatEnemy pursuedTarget;

        public bool IsEnabled => autoEnabled;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            hero = GetComponent<PrototypeCombat>();
            encounter = GetComponent<CombatEncounter>();
            // Never restore auto from PlayerPrefs and never farm offline.
            autoEnabled = false;
            RefreshIndicator();
        }

        private void OnDisable() => DisableAuto();

        public void Configure(Text label)
        {
            stateLabel = label;
            RefreshIndicator();
        }

        public void Toggle() => SetEnabled(!autoEnabled);

        public void StopForManualMovement() => SetEnabled(false);

        public void SetEnabled(bool value)
        {
            if (value && (hero == null || hero.IsDead)) value = false;
            autoEnabled = value;
            if (!value)
            {
                pursuedTarget = null;
                if (motor != null) motor.SetAutomaticDirection(Vector3.zero);
            }
            else
            {
                nextAcquisition = 0f;
                nextAttackDecision = 0f;
                lastPosition = transform.position;
                stuckSince = Time.time;
            }
            RefreshIndicator();
        }

        private void DisableAuto()
        {
            autoEnabled = false;
            pursuedTarget = null;
            if (motor != null) motor.SetAutomaticDirection(Vector3.zero);
            RefreshIndicator();
        }

        private void RefreshIndicator()
        {
            if (stateLabel == null) return;
            stateLabel.text = autoEnabled ? "АВТО: ВКЛ" : "АВТО: ВЫКЛ";
            stateLabel.color = autoEnabled ? new Color(.40f, 1f, .65f) : Color.white;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            var delta = a-b;
            delta.y = 0f;
            return delta.magnitude;
        }

        private bool IsBlocked(CombatEnemy enemy)
        {
            if (enemy == null || enemy.IsDead) return true;
            var start = transform.position + Vector3.up * 1.6f;
            var end = enemy.AimPoint;
            var delta = end-start;
            var length = delta.magnitude;
            if (length < .01f) return false;
            if (!Physics.Raycast(start, delta/length, out var hit, length,
                                 ~0, QueryTriggerInteraction.Ignore)) return false;
            // A collision with the chosen target is a clear line of attack.
            return hit.collider.GetComponentInParent<CombatEnemy>() != enemy;
        }

        private bool IsAvailable(CombatEnemy enemy)
        {
            if (enemy == null || enemy.IsDead) return false;
            if (FlatDistance(enemy.transform.position, transform.position) > detectionRadius) return false;
            if (unreachableUntil.TryGetValue(enemy.GetInstanceID(), out var until) && until > Time.time)
                return false;
            return !IsBlocked(enemy);
        }

        private CombatEnemy ChooseTarget()
        {
            var known = encounter.TrackedEnemies;
            if (known == null) return null;
            CombatEnemy result = null;
            var best = detectionRadius;
            foreach (var candidate in known)
            {
                if (!IsAvailable(candidate)) continue;
                float distance = FlatDistance(candidate.transform.position, transform.position);
                if (distance < best) { best = distance; result = candidate; }
            }
            return result;
        }

        private void MarkUnreachable(CombatEnemy enemy)
        {
            if (enemy != null)
                unreachableUntil[enemy.GetInstanceID()] = Time.time + 6f;
            pursuedTarget = null;
            motor.SetAutomaticDirection(Vector3.zero);
            nextAcquisition = Time.time + .4f;
        }

        private void Update()
        {
            if (!autoEnabled) return;
            if (hero == null || hero.IsDead)
            {
                DisableAuto();
                return;
            }

            // Recheck invalidated targets without letting physics hits on the player block selection.
            var target = pursuedTarget;
            if (!IsAvailable(target))
            {
                target = null;
                pursuedTarget = null;
            }
            if (target == null && Time.time >= nextAcquisition)
            {
                target = ChooseTarget();
                nextAcquisition = Time.time + .45f;
                pursuedTarget = target;
                lastPosition = transform.position;
                stuckSince = Time.time;
            }
            if (target == null)
            {
                motor.SetAutomaticDirection(Vector3.zero);
                return;
            }
            if (!encounter.SelectAutomaticTarget(target))
            {
                MarkUnreachable(target);
                return;
            }

            var delta = target.transform.position - transform.position;
            delta.y = 0;
            float distance = delta.magnitude;
            if (distance <= .01f)
            {
                motor.SetAutomaticDirection(Vector3.zero);
                return;
            }

            var facing = Quaternion.LookRotation(delta / distance, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, 720f * Time.deltaTime);
            float desiredRange = Mathf.Max(.7f, encounter.CombatRange(0) * .85f);

            if (distance > desiredRange)
            {
                motor.SetAutomaticDirection(delta.normalized);
                if (FlatDistance(lastPosition, transform.position) >= .22f)
                {
                    lastPosition = transform.position;
                    stuckSince = Time.time;
                }
                else if (Time.time - stuckSince > stuckTimeout)
                    MarkUnreachable(target);
                return;
            }

            motor.SetAutomaticDirection(Vector3.zero);
            stuckSince = Time.time;
            lastPosition = transform.position;
            if (Time.time < nextAttackDecision) return;

            // One decision/cast at a time, respecting the same cooldown array as manual combat.
            for (int slot = 4; slot >= 0; slot--)
            {
                if (encounter.CooldownRemaining(slot) > .01f ||
                    distance > encounter.CombatRange(slot)) continue;
                switch (slot)
                {
                    case 4: hero.Skill4(); break;
                    case 3: hero.Skill3(); break;
                    case 2: hero.Skill2(); break;
                    case 1: hero.Skill1(); break;
                    default: hero.Attack(); break;
                }
                nextAttackDecision = Time.time + .7f;
                break;
            }
        }
    }
}
