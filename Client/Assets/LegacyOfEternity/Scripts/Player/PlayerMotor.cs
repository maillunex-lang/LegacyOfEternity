using UnityEngine;
using LegacyOfEternity.Offline;

namespace LegacyOfEternity.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
        [SerializeField, Min(1f)] private float turnSpeed = 720f;
        [SerializeField] private Transform cameraTransform;
        private CharacterController controller;
        private PrototypeCombat combat;
        private static Vector2 touchInput;
        private Vector3 autoDirection;
        private Vector3? questTarget;
        private const float QuestArriveDistance = 2.4f;

        // World-space movement requested by AutoCombatController; collision remains
        // handled here by the same CharacterController used for manual movement.
        public void SetAutomaticDirection(Vector3 direction)
        {
            direction.y = 0f;
            autoDirection = Vector3.ClampMagnitude(direction, 1f);
        }

        /// <summary>Tap-quest auto-run target. Cleared on manual input or arrival.</summary>
        public void SetQuestTarget(Vector3? worldPosition)
        {
            questTarget = worldPosition;
            if (!worldPosition.HasValue)
                return;
            var auto = GetComponent<AutoCombatController>();
            if (auto != null && auto.IsEnabled)
                auto.StopForManualMovement();
        }

        public bool HasQuestTarget => questTarget.HasValue;

        public static void SetMobileInput(Vector2 value) { touchInput = Vector2.ClampMagnitude(value, 1f); }
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            combat = GetComponent<PrototypeCombat>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }
        private void OnDisable()
        {
            touchInput = Vector2.zero;
            autoDirection = Vector3.zero;
            questTarget = null;
        }
        private void Update()
        {
            if (!OfflineSession.Ready)
                return;
            Vector2 input = combat != null && combat.IsDead ? Vector2.zero : ReadInput();
            if (input.sqrMagnitude > .0001f)
            {
                questTarget = null;
                var auto = GetComponent<AutoCombatController>();
                if (auto != null && auto.IsEnabled) auto.StopForManualMovement();
            }
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();
            Vector3 direction = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
            if (input.sqrMagnitude <= .0001f && (combat == null || !combat.IsDead))
            {
                if (questTarget.HasValue)
                {
                    Vector3 to = questTarget.Value - transform.position;
                    to.y = 0f;
                    if (to.magnitude <= QuestArriveDistance)
                    {
                        questTarget = null;
                        direction = Vector3.zero;
                    }
                    else
                        direction = to.normalized;
                }
                else
                    direction = autoDirection;
            }
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion facing = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }
            Vector3 velocity = direction * moveSpeed;
            velocity.y = controller.isGrounded ? -2f : -9.81f;
            controller.Move(velocity * Time.deltaTime);
        }
        private static Vector2 ReadInput()
        {
            float x = 0f, y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            Vector2 keyboard = Vector2.ClampMagnitude(new Vector2(x,y), 1f);
            return keyboard.sqrMagnitude > 0 ? keyboard : touchInput;
        }
    }
}
