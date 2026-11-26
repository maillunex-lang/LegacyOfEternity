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
        [SerializeField] private Vector3 targetOffset = Vector3.zero;
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
        private Vector2 lastGestureMid;
        private float lastGestureAngle;
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

        private static float TouchAngle(Vector2 first, Vector2 second)
        {
            Vector2 direction = second - first;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
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
                    yaw += Input.GetAxis("Mouse X") * 3.4f;
                    height = Mathf.Clamp(height - Input.GetAxis("Mouse Y") * .5f, minHeight, maxHeight);
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
                if (finger.phase == TouchPhase.Began && IsOverUi(finger.fingerId))
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
                Vector2 midpoint = (a.position + b.position) * .5f;
                float span = Vector2.Distance(a.position, b.position);
                float angle = TouchAngle(a.position, b.position);
                if (!gestureInitialized || gestureFirstId != a.fingerId || gestureSecondId != b.fingerId)
                {
                    gestureInitialized = true;
                    gestureFirstId = a.fingerId;
                    gestureSecondId = b.fingerId;
                    lastGestureSpan = span;
                    lastGestureMid = midpoint;
                    lastGestureAngle = angle;
                }
                else
                {
                    // Pinch to zoom, shared two-finger drag rotates and changes height.
                    // Two-finger twist rotates as well, with a separate yaw contribution.
                    distance = Mathf.Clamp(distance - (span - lastGestureSpan) * zoomRatio, minDistance, maxDistance);
                    Vector2 shift = midpoint - lastGestureMid;
                    yaw += shift.x * rotationRatio + Mathf.DeltaAngle(lastGestureAngle, angle);
                    height = Mathf.Clamp(height - shift.y * heightRatio, minHeight, maxHeight);
                    lastGestureSpan = span;
                    lastGestureMid = midpoint;
                    lastGestureAngle = angle;
                }
            }
            else
            {
                gestureInitialized = false;
                if (eligible == 1)
                {
                    if (activeSingleId != a.fingerId)
                    {
                        activeSingleId = a.fingerId;
                    }
                    else if (a.phase == TouchPhase.Moved)
                    {
                        // Drag on empty world: original CameraControl Rotate(dx,dy).
                        yaw += a.deltaPosition.x * rotationRatio;
                        height = Mathf.Clamp(height - a.deltaPosition.y * heightRatio, minHeight, maxHeight);
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
                    if (hits[i].distance < nearest) nearest = hits[i].distance;
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
