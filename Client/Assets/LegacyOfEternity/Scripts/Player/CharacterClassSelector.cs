using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LegacyOfEternity.Player
{
    public enum CharacterClass { Warrior = 0, Assassin = 1, Mage = 2, Archer = 3 }

    /// <summary>Prototype class selector. The player controller stays in place when switching visual classes.</summary>
    public sealed class CharacterClassSelector : MonoBehaviour
    {
        [SerializeField] private GameObject[] classVisuals = new GameObject[4];
        [SerializeField] private Text classLabel;
        [SerializeField] private CharacterClass startingClass = CharacterClass.Warrior;
        private CharacterClass currentClass;
        public CharacterClass CurrentClass => currentClass;
        public Animator ActiveAnimator => classVisuals!=null && classVisuals.Length> (int)currentClass && classVisuals[(int)currentClass]!=null ? classVisuals[(int)currentClass].GetComponent<Animator>() : null;
        private static readonly string[] Names = { "ВОИН", "АССАСИН", "МАГ", "ЛУЧНИК" };

        private void Awake()
        {
            var stored = PlayerPrefs.GetInt("LOE.Class", (int)startingClass);
            SelectClass(Mathf.Clamp(stored,0,3));
        }

        public void SelectClass(int index)
        {
            index = Mathf.Clamp(index,0,3);
            currentClass = (CharacterClass)index;
            for (int i=0;i<classVisuals.Length;i++)
                if (classVisuals[i]!=null) classVisuals[i].SetActive(i==index);
            var anim = GetComponent<HeroAnimation>();
            if (anim != null) anim.SetAnimator(classVisuals[index]?.GetComponent<Animator>());
            var active = ActiveAnimator;
            var combat = GetComponent<PrototypeCombat>();
            if(active != null && combat != null)
            {
                active.SetBool("Dead", combat.IsDead);
                if(!combat.IsDead) active.SetTrigger("Reset");
            }
            if (classLabel != null) classLabel.text = Names[index];
            PlayerPrefs.SetInt("LOE.Class",index);
        }
        public void NextClass() => SelectClass(((int)currentClass+1)%4);
        public void PreviousClass() => SelectClass(((int)currentClass+3)%4);
    }
}
