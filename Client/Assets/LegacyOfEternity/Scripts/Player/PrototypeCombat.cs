using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Combat;

namespace LegacyOfEternity.Player
{
    /// <summary>Local PvE prototype. Damage must move to authoritative Go server before online play.</summary>
    [RequireComponent(typeof(CharacterClassSelector))]
    public sealed class PrototypeCombat : MonoBehaviour
    {
        [SerializeField] private CharacterClassSelector selector;
        [SerializeField] private Text healthLabel;
        [SerializeField, Range(1, 1000)] private int maxHealth = 100;
        [SerializeField] private bool keyboardPreview = true;

        private CombatEncounter encounter;
        private int currentHealth;
        private bool dead;

        public int Health => currentHealth;
        public bool IsDead => dead;

        private void Awake()
        {
            if (selector == null) selector = GetComponent<CharacterClassSelector>();
            encounter = GetComponent<CombatEncounter>();
            currentHealth = maxHealth;
            RefreshHealth();
        }

        private void Update()
        {
            if (!keyboardPreview) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Attack();
            if (Input.GetKeyDown(KeyCode.Alpha2)) Skill1();
            if (Input.GetKeyDown(KeyCode.Alpha3)) Skill2();
            if (Input.GetKeyDown(KeyCode.Alpha4)) Skill3();
            if (Input.GetKeyDown(KeyCode.Alpha5)) Skill4();
            if (Input.GetKeyDown(KeyCode.Tab) && encounter != null) encounter.TargetNext();
            if (Input.GetKeyDown(KeyCode.H)) TakeHit();
            if (Input.GetKeyDown(KeyCode.K)) Die();
            if (Input.GetKeyDown(KeyCode.R)) Respawn();
        }

        public void Attack() => UseAction(0);
        public void Skill1() => UseAction(1);
        public void Skill2() => UseAction(2);
        public void Skill3() => UseAction(3);
        public void Skill4() => UseAction(4);
        public void Skill(int slot) { if (slot >= 1 && slot <= 4) UseAction(slot); }

        private void UseAction(int slot)
        {
            if (dead) return;
            if (encounter == null) encounter = GetComponent<CombatEncounter>();
            if (encounter != null && !encounter.Execute(slot)) return;
            Play(slot == 0 ? "Attack" : "Skill" + slot);
        }

        public void TakeHit() => ReceiveDamage(10);

        public void ReceiveDamage(int amount)
        {
            if (dead || amount <= 0) return;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            RefreshHealth();
            if (currentHealth == 0) Die();
            else Play("Hit");
        }

        public void Die()
        {
            if (dead) return;
            dead = true;
            currentHealth = 0;
            RefreshHealth();
            Play("Death", true);
        }

        public void Respawn()
        {
            dead = false;
            currentHealth = maxHealth;
            RefreshHealth();
            var animator = selector != null ? selector.ActiveAnimator : null;
            if (animator != null)
            {
                ResetActionTriggers(animator);
                animator.SetBool("Dead", false);
                animator.SetTrigger("Reset");
            }
            if (encounter == null) encounter = GetComponent<CombatEncounter>();
            if (encounter != null) encounter.OnHeroRespawn();
        }

        private static void ResetActionTriggers(Animator animator)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Skill1");
            animator.ResetTrigger("Skill2");
            animator.ResetTrigger("Skill3");
            animator.ResetTrigger("Skill4");
            animator.ResetTrigger("Hit");
        }

        private void Play(string trigger, bool allowDead = false)
        {
            if (dead && !allowDead) return;
            var animator = selector != null ? selector.ActiveAnimator : null;
            if (animator == null) return;
            ResetActionTriggers(animator);
            if (trigger == "Death") animator.SetBool("Dead", true);
            else animator.SetTrigger(trigger);
        }

        private void RefreshHealth()
        {
            if (healthLabel != null)
                healthLabel.text = "HP " + currentHealth + " / " + maxHealth;
        }
    }
}
