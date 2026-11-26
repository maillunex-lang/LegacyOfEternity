using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyOfEternity.Player
{
    // Prototype actions only: animation preview and local HP; no server combat or damage authority.
    [RequireComponent(typeof(CharacterClassSelector))]
    public sealed class PrototypeCombat : MonoBehaviour
    {
        [SerializeField] private CharacterClassSelector selector;
        [SerializeField] private Text healthLabel;
        [SerializeField, Range(1,1000)] private int maxHealth=100;
        [SerializeField] private bool keyboardPreview=true;
        private int currentHealth;
        private Coroutine currentAction;
        private bool dead;
        public int Health => currentHealth;
        public bool IsDead => dead;

        private void Awake()
        {
            if(selector==null)selector=GetComponent<CharacterClassSelector>();
            currentHealth=maxHealth;
            RefreshHealth();
        }
        private void Update()
        {
            if(!keyboardPreview)return;
            if(Input.GetKeyDown(KeyCode.Alpha1))Attack();
            if(Input.GetKeyDown(KeyCode.Alpha2))Skill(1);
            if(Input.GetKeyDown(KeyCode.Alpha3))Skill(2);
            if(Input.GetKeyDown(KeyCode.Alpha4))Skill(3);
            if(Input.GetKeyDown(KeyCode.Alpha5))Skill(4);
            if(Input.GetKeyDown(KeyCode.H))TakeHit();
            if(Input.GetKeyDown(KeyCode.K))Die();
            if(Input.GetKeyDown(KeyCode.R))Respawn();
        }
        public void Attack() => Play("Attack");
        public void Skill1() => Skill(1);
        public void Skill2() => Skill(2);
        public void Skill3() => Skill(3);
        public void Skill4() => Skill(4);
        public void Skill(int slot) { if(slot>=1 && slot<=4)Play("Skill"+slot); }
        public void TakeHit() { if(!dead){currentHealth=Mathf.Max(0,currentHealth-10);RefreshHealth();if(currentHealth==0)Die();else Play("Hit");} }
        public void Die() { if(dead)return;dead=true;currentHealth=0;RefreshHealth();Play("Death",true); }
        public void Respawn()
        {
            if(currentAction!=null){StopCoroutine(currentAction);currentAction=null;}
            dead=false;currentHealth=maxHealth;RefreshHealth();
            var animator=selector.ActiveAnimator;
            if(animator!=null){animator.SetBool("Dead",false);animator.SetTrigger("Reset");}
        }
        private void Play(string trigger,bool allowDead=false)
        {
            if(dead&&!allowDead)return;
            var animator=selector.ActiveAnimator;
            if(animator==null)return;
            if(currentAction!=null){StopCoroutine(currentAction);currentAction=null;}
            animator.ResetTrigger("Attack");animator.ResetTrigger("Skill1");animator.ResetTrigger("Skill2");
            animator.ResetTrigger("Skill3");animator.ResetTrigger("Skill4");animator.ResetTrigger("Hit");
            if(trigger=="Death")animator.SetBool("Dead",true);
            else animator.SetTrigger(trigger);
        }
        private void RefreshHealth(){if(healthLabel!=null)healthLabel.text="HP "+currentHealth+" / "+maxHealth;}
    }
}
