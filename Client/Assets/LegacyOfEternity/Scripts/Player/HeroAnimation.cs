using UnityEngine;
namespace LegacyOfEternity.Player {
public sealed class HeroAnimation : MonoBehaviour {
[SerializeField] Animator animator;
CharacterController motor;
public void SetAnimator(Animator selected) { animator=selected; }
void Awake(){motor=GetComponent<CharacterController>();}
void LateUpdate(){if(animator!=null&&motor!=null){var v=motor.velocity;v.y=0;animator.SetFloat("Speed",v.magnitude);}}
}
}
