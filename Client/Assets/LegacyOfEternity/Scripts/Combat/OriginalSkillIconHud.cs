using System;
using UnityEngine;
using UnityEngine.UI;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Combat
{
    /// <summary>
    /// Original icons from t_skill.lua.txt and the UIMain/UIMain2 Unity sprite atlases.
    /// Visual-only, does not alter combat or cooldown timing.
    /// </summary>
    public sealed class OriginalSkillIconHud : MonoBehaviour
    {
        [SerializeField] private CharacterClassSelector selector;
        [SerializeField] private Image[] skillImages = new Image[5];
        [SerializeField] private Sprite[] classSprites = new Sprite[20];
        private int lastClass = -1;

        public void Configure(CharacterClassSelector classSelector, Image[] images, Sprite[] sprites)
        {
            if (images == null || images.Length != 5 || sprites == null || sprites.Length != 20)
                throw new ArgumentException("Expected 5 UI images and 20 original icon sprites");
            selector = classSelector;
            skillImages = images;
            classSprites = sprites;
            lastClass = -1;
            Apply();
        }

        private void Awake()
        {
            if (selector == null) selector = GetComponent<CharacterClassSelector>();
        }

        private void OnEnable()
        {
            lastClass = -1;
            Apply();
        }

        private void LateUpdate()
        {
            if (selector != null && (int)selector.CurrentClass != lastClass)
                Apply();
        }

        private void Apply()
        {
            if (selector == null || classSprites == null || classSprites.Length != 20 ||
                skillImages == null || skillImages.Length != 5)
                return;

            int currentClass = Mathf.Clamp((int)selector.CurrentClass, 0, 3);
            for (int slot = 0; slot < 5; slot++)
            {
                var image = skillImages[slot];
                var sprite = classSprites[currentClass * 5 + slot];
                if (image == null || sprite == null) continue;
                image.sprite = sprite;
                image.enabled = true;
                image.preserveAspect = true;
            }
            lastClass = currentClass;
        }
    }
}
