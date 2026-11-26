using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyOfEternity.Combat
{
    /// <summary>World-space health, current target and damage numbers for a local PvE enemy.</summary>
    [RequireComponent(typeof(CombatEnemy))]
    public sealed class CombatEnemyHud : MonoBehaviour
    {
        [SerializeField] private CombatEncounter encounter;
        private CombatEnemy enemy;
        private Canvas display;
        private Image background;
        private RectTransform healthFill;
        private float lastDamage;

        public void Configure(CombatEncounter combat) => encounter = combat;

        private void Awake()
        {
            enemy = GetComponent<CombatEnemy>();
            BuildHud();
        }

        private void OnEnable()
        {
            if (enemy == null) enemy = GetComponent<CombatEnemy>();
            if (enemy != null) enemy.DamageApplied += OnDamage;
        }

        private void OnDisable()
        {
            if (enemy != null) enemy.DamageApplied -= OnDamage;
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("Enemy World HUD", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            display = canvasObject.GetComponent<Canvas>();
            display.renderMode = RenderMode.WorldSpace;
            var canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(170f, 50f);
            canvasRect.localPosition = new Vector3(0f, 2.2f, 0f);
            canvasRect.localScale = Vector3.one * .01f;

            var backgroundObject = new GameObject("HP Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(canvasRect, false);
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(.07f, .45f);
            backgroundRect.anchorMax = new Vector2(.93f, .75f);
            backgroundRect.offsetMin = backgroundRect.offsetMax = Vector2.zero;
            background = backgroundObject.GetComponent<Image>();
            background.color = new Color(.35f, .06f, .07f, .91f);
            background.raycastTarget = false;

            var fillObject = new GameObject("HP Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(backgroundRect, false);
            healthFill = fillObject.GetComponent<RectTransform>();
            healthFill.anchorMin = Vector2.zero;
            healthFill.anchorMax = Vector2.one;
            healthFill.offsetMin = healthFill.offsetMax = Vector2.zero;
            var fill = fillObject.GetComponent<Image>();
            fill.color = new Color(.25f, .92f, .40f, .94f);
            fill.raycastTarget = false;

            var textObject = new GameObject("Monster Name", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvasRect, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f,.67f);
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var caption = textObject.GetComponent<Text>();
            caption.text = enemy.DisplayName;
            caption.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            caption.fontSize = 17;
            caption.color = Color.white;
            caption.alignment = TextAnchor.MiddleCenter;
            caption.raycastTarget = false;
        }

        private void LateUpdate()
        {
            if (display == null || enemy == null) return;
            bool visible = !enemy.IsDead || Time.time - lastDamage < .9f;
            display.enabled = visible;
            if (!visible) return;

            var camera = Camera.main;
            if (camera != null) display.transform.rotation = camera.transform.rotation;

            // Change the RectTransform instead of using a sprite-dependent filled Image.
            float ratio = Mathf.Clamp01((float)enemy.Health / Mathf.Max(1, enemy.MaxHealth));
            healthFill.anchorMax = new Vector2(ratio, 1f);
            background.color = encounter != null && encounter.CurrentTarget == enemy
                ? new Color(1f, .75f, .08f, .96f)
                : new Color(.35f, .06f, .07f, .91f);
        }

        private void OnDamage(int actualAmount)
        {
            if (actualAmount <= 0 || display == null) return;
            lastDamage = Time.time;
            var popupObject = new GameObject("Damage Number", typeof(RectTransform), typeof(Text));
            popupObject.transform.SetParent(display.transform, false);
            var rect = popupObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 35f);
            rect.anchoredPosition = new Vector2(Random.Range(-20f,20f), 35f);
            var label = popupObject.GetComponent<Text>();
            label.text = actualAmount.ToString();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 30;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(1f, .88f, .45f, 1f);
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            StartCoroutine(AnimatePopup(rect, label));
        }

        private IEnumerator AnimatePopup(RectTransform rect, Text label)
        {
            float elapsed = 0f;
            var from = rect.anchoredPosition;
            const float duration = .8f;
            while (elapsed < duration && rect != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed/duration);
                rect.anchoredPosition = from + Vector2.up * (55f * t);
                label.color = new Color(1f,.88f,.45f, 1f-t);
                yield return null;
            }
            if (rect != null) Destroy(rect.gameObject);
        }
    }
}
