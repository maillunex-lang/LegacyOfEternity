using System.Collections;
using UnityEngine;
using LegacyOfEternity.Player;
using LegacyOfEternity.Offline;

namespace LegacyOfEternity.Combat
{
    /// <summary>Local PvE dummy. No server authority or loot is implemented yet.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CombatEnemy : MonoBehaviour
    {
        [SerializeField] private string displayName = "МОНСТР";
        [SerializeField, Min(1)] private int maxHealth = 65;
        [SerializeField, Min(0)] private int attackDamage = 7;
        [SerializeField, Min(.1f)] private float moveSpeed = 2.4f;
        [SerializeField, Min(1f)] private float aggroRadius = 3.5f;
        [SerializeField, Min(.5f)] private float attackRadius = 1.8f;
        [SerializeField, Min(.1f)] private float attackInterval = 2.8f;
        [SerializeField, Min(1f)] private float respawnAfter = 12f;
        [SerializeField] private Transform hero;
        [SerializeField] private Animator animator;

        private CharacterController character;
        private PrototypeCombat player;
        private Vector3 home;
        private int health;
        private bool dead;
        private bool engaged;
        private float nextAttack;
        private Coroutine strike;
        private Coroutine respawn;

        public event System.Action<int> DamageApplied;
        /// <summary>Fired once when this enemy dies (local PvE). Used by offline quest tracker.</summary>
        public static event System.Action<CombatEnemy> Killed;
        public string DisplayName => displayName;
        public int Health => health;
        public int MaxHealth => maxHealth;
        public bool IsDead => dead;
        public Vector3 AimPoint => transform.position + Vector3.up * 1f;

        public void Configure(Transform target, Animator visualAnimator, string title, int hp, int damage)
        {
            hero = target;
            animator = visualAnimator;
            displayName = title;
            maxHealth = Mathf.Max(1, hp);
            attackDamage = Mathf.Max(0, damage);
        }

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            player = hero != null ? hero.GetComponent<PrototypeCombat>() : null;
            home = transform.position;
            health = maxHealth;
            if (animator != null) animator.applyRootMotion = false;
        }

        private void Update()
        {
            if (!OfflineSession.Ready || dead || hero == null || player == null || player.IsDead)
            {
                if (animator != null) animator.SetFloat("Speed", 0f);
                return;
            }

            Vector3 delta = hero.position - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance > aggroRadius && !engaged)
            {
                if (animator != null) animator.SetFloat("Speed", 0f);
                return;
            }

            if (distance > .1f)
            {
                var facing = Quaternion.LookRotation(delta.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, 420f * Time.deltaTime);
            }

            bool walking = distance > attackRadius * .82f;
            Vector3 velocity = walking ? delta.normalized * moveSpeed : Vector3.zero;
            velocity.y = character.isGrounded ? -2f : -9.81f;
            character.Move(velocity * Time.deltaTime);
            if (animator != null) animator.SetFloat("Speed", walking ? moveSpeed : 0f);

            if (distance <= attackRadius && Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackInterval;
                if (animator != null) animator.SetTrigger("Attack");
                if (strike != null) StopCoroutine(strike);
                strike = StartCoroutine(StrikeAfterWindup());
            }
        }

        private IEnumerator StrikeAfterWindup()
        {
            yield return new WaitForSeconds(.48f);
            if (!dead && hero != null && player != null && !player.IsDead)
            {
                Vector3 d = hero.position - transform.position;
                d.y = 0;
                if (d.sqrMagnitude <= (attackRadius + .35f) * (attackRadius + .35f))
                    player.ReceiveDamage(attackDamage);
            }
            strike = null;
        }

        public bool ApplyDamage(int amount)
        {
            if (dead || amount <= 0) return false;
            engaged = true;
            int beforeHit = health;
            health = Mathf.Max(0, health - amount);
            if (beforeHit != health) DamageApplied?.Invoke(beforeHit - health);
            if (health <= 0)
            {
                Kill();
            }
            else if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
            return true;
        }

        private void Kill()
        {
            dead = true;
            engaged = false;
            if (strike != null) { StopCoroutine(strike); strike = null; }
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("Dead", true);
            }
            Killed?.Invoke(this);
            if (respawn != null) StopCoroutine(respawn);
            respawn = StartCoroutine(RespawnLater());
        }

        private IEnumerator RespawnLater()
        {
            yield return new WaitForSeconds(respawnAfter);
            dead = false;
            engaged = false;
            health = maxHealth;
            character.enabled = false;
            transform.position = home;
            character.enabled = true;
            nextAttack = Time.time + 1.5f;
            if (animator != null)
            {
                animator.SetBool("Dead", false);
                animator.SetTrigger("Reset");
            }
            respawn = null;
        }

        public void ResetAggro()
        {
            engaged = false;
            nextAttack = Mathf.Max(nextAttack, Time.time + 1f);
        }
    }
}
