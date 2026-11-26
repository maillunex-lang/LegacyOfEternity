using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Combat
{
    /// <summary>Single-player PvE action/target controller, replace damage authority with server RPCs later.</summary>
    [RequireComponent(typeof(PrototypeCombat))]
    public sealed class CombatEncounter : MonoBehaviour
    {
        [SerializeField] private CombatEnemy[] enemies;
        [SerializeField] private Text targetLabel;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private CharacterClassSelector selector;

        private PrototypeCombat hero;
        private CombatEnemy currentTarget;
        private readonly float[] nextAllowed = new float[5];
        private string feedback = "";
        private float feedbackUntil;

        public CombatEnemy CurrentTarget => currentTarget;

        // Read-only combat state for both manual HUD and automatic control.
        public float CooldownRemaining(int slot)
        {
            if (slot < 0 || slot >= nextAllowed.Length) return float.PositiveInfinity;
            return Mathf.Max(0f, nextAllowed[slot] - Time.time);
        }

        public float CombatRange(int slot)
        {
            return slot >= 0 && slot < nextAllowed.Length ? AllowedRange(slot) : 0f;
        }

        public CombatEnemy[] TrackedEnemies => enemies;

        public bool SelectAutomaticTarget(CombatEnemy candidate)
        {
            if (candidate == null || candidate.IsDead || enemies == null ||
                System.Array.IndexOf(enemies, candidate) < 0) return false;
            currentTarget = candidate;
            return true;
        }

        public void Configure(CombatEnemy[] spawned, Text targetText, Text feedbackText)
        {
            enemies = spawned;
            targetLabel = targetText;
            feedbackLabel = feedbackText;
        }

        private void Awake()
        {
            hero = GetComponent<PrototypeCombat>();
            if (selector == null) selector = GetComponent<CharacterClassSelector>();
        }

        private void Start()
        {
            SelectNearest();
            RefreshHud();
        }

        private void Update()
        {
            if (currentTarget != null && (currentTarget.IsDead || FlatDistance(currentTarget.transform.position, transform.position) > 28f))
                currentTarget = null;
            HandleTargetTap();
            if (currentTarget == null) SelectNearest();
            RefreshHud();
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            Vector3 d = a - b;
            d.y = 0f;
            return d.magnitude;
        }

        private void HandleTargetTap()
        {
            if (Camera.main == null) return;
            Vector2 screenPosition = Vector2.zero;
            bool pressed = false;
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                screenPosition = touch.position;
                pressed = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                screenPosition = Input.mousePosition;
                pressed = true;
            }
            if (!pressed) return;
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var enemy = hit.collider.GetComponentInParent<CombatEnemy>();
                if (enemy != null && !enemy.IsDead && FlatDistance(enemy.transform.position, transform.position) <= 28f)
                {
                    currentTarget = enemy;
                    ShowFeedback("ЦЕЛЬ ВЫБРАНА: " + enemy.DisplayName);
                }
            }
        }

        private void SelectNearest()
        {
            if (enemies == null) return;
            float best = 24f;
            CombatEnemy nearest = null;
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                var distance = FlatDistance(enemy.transform.position, transform.position);
                if (distance < best) { best = distance; nearest = enemy; }
            }
            currentTarget = nearest;
        }

        public void TargetNext()
        {
            if (enemies == null || enemies.Length == 0) return;
            int start = System.Array.IndexOf(enemies, currentTarget);
            for (int i = 1; i <= enemies.Length; i++)
            {
                int index = (start + i + enemies.Length) % enemies.Length;
                if (enemies[index] != null && !enemies[index].IsDead)
                {
                    currentTarget = enemies[index];
                    ShowFeedback("ЦЕЛЬ ВЫБРАНА: " + currentTarget.DisplayName);
                    RefreshHud();
                    return;
                }
            }
            currentTarget = null;
            ShowFeedback("ЖИВЫХ ЦЕЛЕЙ НЕТ");
        }

        private float AllowedRange(int slot)
        {
            var klass = selector != null ? selector.CurrentClass : CharacterClass.Warrior;
            float baseRange = klass == CharacterClass.Mage || klass == CharacterClass.Archer ? 9f : 2.9f;
            return slot == 3 ? baseRange + 1.5f : baseRange;
        }

        public bool Execute(int slot)
        {
            if (slot < 0 || slot >= nextAllowed.Length || hero == null || hero.IsDead) return false;
            if (currentTarget == null || currentTarget.IsDead) SelectNearest();
            if (currentTarget == null) { ShowFeedback("НЕТ ЦЕЛИ"); return false; }
            float remaining = nextAllowed[slot] - Time.time;
            if (remaining > 0f) { ShowFeedback("ПЕРЕЗАРЯДКА " + remaining.ToString("F1") + " с"); return false; }
            float allowedRange = AllowedRange(slot);
            if (FlatDistance(transform.position, currentTarget.transform.position) > allowedRange)
            {
                ShowFeedback("ЦЕЛЬ СЛИШКОМ ДАЛЕКО");
                return false;
            }

            int damage;
            float cooldown;
            switch (slot)
            {
                case 1: damage = 20; cooldown = 3f; break;
                case 2: damage = 26; cooldown = 5f; break;
                case 3: damage = 18; cooldown = 6f; break;
                case 4: damage = 38; cooldown = 9f; break;
                default: damage = 12; cooldown = .8f; break;
            }
            nextAllowed[slot] = Time.time + cooldown;
            var currentClass = selector != null ? selector.CurrentClass : CharacterClass.Warrior;
            var hitTimes = CombatImpactProfile.HitTimes(currentClass, slot);
            GetComponent<OriginalCombatEffects>()?.PlayAction(slot, currentTarget, hitTimes[0]);
            StartCoroutine(ResolveAction(currentTarget, damage, slot, allowedRange, hitTimes));
            ShowFeedback(slot == 0 ? "АТАКА" : "НАВЫК " + slot);
            return true;
        }

        private IEnumerator ResolveAction(CombatEnemy primary, int damage, int slot,
                                          float maxDistance, float[] hitTimes)
        {
            // Timings follow original skill-display hit markers. Never duplicate the
            // prototype's total damage when the animation has multiple strikes.
            if (hitTimes == null || hitTimes.Length == 0) yield break;
            float previousTime = 0f;
            for (int index = 0; index < hitTimes.Length; index++)
            {
                float nextTime = Mathf.Max(previousTime, hitTimes[index]);
                yield return new WaitForSeconds(Mathf.Max(0f, nextTime - previousTime));
                previousTime = nextTime;
                if (hero == null || hero.IsDead || primary == null || primary.IsDead) yield break;
                if (FlatDistance(transform.position, primary.transform.position) > maxDistance + .5f)
                    yield break;
                int pulse = damage / hitTimes.Length + (index < damage % hitTimes.Length ? 1 : 0);
                if (pulse <= 0) continue;

                if (slot == 3 && enemies != null)
                {
                    foreach (var enemy in enemies)
                        if (enemy != null && !enemy.IsDead &&
                            FlatDistance(enemy.transform.position, primary.transform.position) <= 3.5f)
                            enemy.ApplyDamage(pulse);
                }
                else
                    primary.ApplyDamage(pulse);
                RefreshHud();
            }
        }

        public void OnHeroRespawn()
        {
            if (enemies != null)
                foreach (var enemy in enemies)
                    if (enemy != null) enemy.ResetAggro();
            for (int i = 0; i < nextAllowed.Length; i++) nextAllowed[i] = 0;
        }

        private void ShowFeedback(string message)
        {
            feedback = message;
            feedbackUntil = Time.time + 1.5f;
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (targetLabel != null)
            {
                if (currentTarget == null || currentTarget.IsDead)
                    targetLabel.text = "ЦЕЛЬ: НЕТ";
                else
                    targetLabel.text = currentTarget.DisplayName + "  HP " + currentTarget.Health + "/" + currentTarget.MaxHealth;
            }
            if (feedbackLabel != null)
                feedbackLabel.text = Time.time <= feedbackUntil ? feedback : "АТАКА / НАВЫКИ / СМЕНА ЦЕЛИ";
        }
    }
}
