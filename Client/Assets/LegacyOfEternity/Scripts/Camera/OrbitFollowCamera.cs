using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace LegacyOfEternity.CameraSystem
{
    /// <summary>
    /// Adapted from original CameraFollow/CameraControl of Legacy of Eternity.
    /// Coastal scene defaults: distance=12.5, height=8, yaw=50. Original HUD input
    /// uses distance 5..35, height 3..15, rotation 0.25deg/pixel and zoom .05/pixel.
    /// Fingers beginning over the HUD are never captured for camera actions.
    /// </summary>
    public sealed class OrbitFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(5f, 35f)] private float distance = 12.5f;
        [SerializeField, Range(3f, 15f)] private float height = 8f;
        [SerializeField, Min(1f)] private float smoothness = 10f;
        [SerializeField] private float yaw = 50f;
        [SerializeField] private Vector3 targetOffset = Vector3.up;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 35f;
        [SerializeField] private float minHeight = 3f;
        [SerializeField] private float maxHeight = 15f;
        [SerializeField] private float rotationRatio = .25f;
        [SerializeField] private float heightRatio = .10f;
        [SerializeField] private float zoomRatio = .05f;
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField] private float collisionRadius = .23f;
        [SerializeField] private float collisionPadding = .3f;
        [SerializeField] private float minimumClearance = .5f;

        private readonly HashSet<int> blockedFingers = new HashSet<int>();
        private int activeSingleId = -1;
        private bool gestureInitialized;
        private int gestureFirstId = -1;
        private int gestureSecondId = -1;
        private float lastGestureSpan;
        private Vector2 singleTouchStart;
        private bool singleTouchDragging;
        private Vector3 currentFocus;
        private bool focused;

        public void SetTarget(Transform value) { target = value; }

        public void ConfigureOriginal(float initialDistance, float initialHeight, float initialYaw)
        {
            distance = Mathf.Clamp(initialDistance, minDistance, maxDistance);
            height = Mathf.Clamp(initialHeight, minHeight, maxHeight);
            yaw = initialYaw;
        }

        public static float ClampZoom(float value) { return Mathf.Clamp(value, 5f, 35f); }
        public static float ClampHeight(float value) { return Mathf.Clamp(value, 3f, 15f); }

        private void Awake()
        {
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            height = Mathf.Clamp(height, minHeight, maxHeight);
        }

        private bool IsOverUi(int id)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(id);
        }

        // The EventSystem may not have updated its pointer cache yet at touch Began.
        // Raycast the actual finger position to prevent camera stealing HUD touches.
        private bool IsOverUi(Touch finger)
        {
            var system = EventSystem.current;
            if (system == null) return false;
            if (system.IsPointerOverGameObject(finger.fingerId)) return true;
            var data = new PointerEventData(system) { position = finger.position };
            var results = new List<RaycastResult>();
            system.RaycastAll(data, results);
            return results.Count > 0;
        }

        private static bool CanGesture(Touch first, Touch second, HashSet<int> blocked)
        {
            return !blocked.Contains(first.fingerId) && !blocked.Contains(second.fingerId) &&
                first.phase != TouchPhase.Ended && first.phase != TouchPhase.Canceled &&
                second.phase != TouchPhase.Ended && second.phase != TouchPhase.Canceled;
        }

        private void Update()
        {
            ReadTouch();
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.touchCount == 0)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > .001f && !IsOverUi(-1))
                    distance = Mathf.Clamp(distance - scroll * 10f, minDistance, maxDistance);
                if (Input.GetMouseButton(1) && !IsOverUi(-1))
                {
                    float dx = Input.GetAxis("Mouse X");
                    float dy = Input.GetAxis("Mouse Y");
                    if (Mathf.Abs(dx) > Mathf.Abs(dy)) yaw += dx * 3.4f;
                    else height = Mathf.Clamp(height - dy * .5f, minHeight, maxHeight);
                }
            }
#endif
        }

        private void ReadTouch()
        {
            // Never assign a finger to the camera if it began on the joystick,
            // attack button, skill, AUTO toggle or another UI graphic.
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch finger = Input.GetTouch(i);
                if (finger.phase == TouchPhase.Began && IsOverUi(finger))
                    blockedFingers.Add(finger.fingerId);
            }

            Touch a = default;
            Touch b = default;
            int eligible = 0;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch finger = Input.GetTouch(i);
                if (finger.phase == TouchPhase.Ended || finger.phase == TouchPhase.Canceled) continue;
                if (blockedFingers.Contains(finger.fingerId)) continue;
                if (eligible == 0) a = finger;
                else if (eligible == 1) b = finger;
                eligible++;
            }

            if (eligible >= 2 && CanGesture(a, b, blockedFingers))
            {
                activeSingleId = -1;
                float span = Vector2.Distance(a.position, b.position);
                if (!gestureInitialized || gestureFirstId != a.fingerId || gestureSecondId != b.fingerId)
                {
                    gestureInitialized = true;
                    gestureFirstId = a.fingerId;
                    gestureSecondId = b.fingerId;
                    lastGestureSpan = span;
                }
                else
                {
                    // Original CameraControl.CheckTwo:
                    // fingers moving together call Move(), which is intentionally empty.
                    // Only opposing finger movement applies pinch zoom, never rotation.
                    if (a.phase == TouchPhase.Moved || b.phase == TouchPhase.Moved)
                    {
                        float opposite = Vector2.Dot(a.deltaPosition, b.deltaPosition);
                        if (opposite <= 0f && Mathf.Abs(span - lastGestureSpan) >= 1.5f)
                            distance = Mathf.Clamp(distance - (span - lastGestureSpan) * zoomRatio, minDistance, maxDistance);
                    }
                    lastGestureSpan = span;
                }
            }
            else
            {
                gestureInitialized = false;
                // A second finger on the joystick or battle HUD must not rotate
                // the camera using the remaining free finger.
                if (eligible == 1 && Input.touchCount == 1)
                {
                    if (activeSingleId != a.fingerId)
                    {
                        activeSingleId = a.fingerId;
                        singleTouchStart = a.position;
                        singleTouchDragging = false;
                    }
                    else if (a.phase == TouchPhase.Moved)
                    {
                        if (!singleTouchDragging &&
                            (a.position - singleTouchStart).sqrMagnitude >= 25f * 25f)
                            singleTouchDragging = true;
                        if (singleTouchDragging)
                        {
                            // Exact original axis lock: horizontal OR vertical,
                            // never both at once.
                            float dx = a.deltaPosition.x, dy = a.deltaPosition.y;
                            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                                yaw += dx * rotationRatio;
                            else
                                height = Mathf.Clamp(height - dy * heightRatio, minHeight, maxHeight);
                        }
                    }
                }
                else
                {
                    activeSingleId = -1;
                }
            }

            // UI ownership is maintained until the end of each finger sequence.
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch finger = Input.GetTouch(i);
                if (finger.phase == TouchPhase.Ended || finger.phase == TouchPhase.Canceled)
                    blockedFingers.Remove(finger.fingerId);
            }
            if (Input.touchCount == 0) blockedFingers.Clear();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            float t = 1f - Mathf.Exp(-smoothness * Time.deltaTime);
            Vector3 focus = target.position + targetOffset;
            if (!focused) { currentFocus = focus; focused = true; }
            else currentFocus = Vector3.Lerp(currentFocus, focus, t);

            // Original CameraFollow places the camera at a horizontal distance
            // and raises it by height * (distance / HEIGHTSLOPE), then LookAt.
            Quaternion direction = Quaternion.Euler(0, yaw, 0);
            Vector3 horizontal = -(direction * Vector3.forward) * distance;
            float elevation = height * (distance / 11f);
            Vector3 desired = currentFocus + horizontal + Vector3.up * elevation;

            // Obstacle safety: shorten the camera ray, ignore hero colliders.
            Vector3 ray = desired - currentFocus;
            float rayLength = ray.magnitude;
            if (rayLength > .001f)
            {
                RaycastHit[] hits = Physics.SphereCastAll(currentFocus, collisionRadius,
                    ray / rayLength, rayLength, collisionLayers, QueryTriggerInteraction.Ignore);
                float nearest = rayLength;
                for (int i = 0; i < hits.Length; i++)
                {
                    if (target != null && (hits[i].transform == target || hits[i].transform.IsChildOf(target))) continue;
                    if (hits[i].distance > collisionRadius && hits[i].distance < nearest) nearest = hits[i].distance;
                }
                if (nearest < rayLength) desired = currentFocus + ray.normalized * Mathf.Max(minimumClearance, nearest - collisionPadding);
            }
            transform.position = Vector3.Lerp(transform.position, desired, t);
            Vector3 look = currentFocus - transform.position;
            if (look.sqrMagnitude > .0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look, Vector3.up), t);
        }
    }
}
