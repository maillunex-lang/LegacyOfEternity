from pathlib import Path
p=Path(r"C:\Source\LegacyOfEternity\Client\Assets\LegacyOfEternity\Scripts\Camera\OrbitFollowCamera.cs")
code=p.read_text(encoding="utf-8-sig")
original=code
def patch(old,new):
 global code
 n=code.count(old)
 if n!=1:raise ValueError(f"expected one replacement: {n}, {old[:70]!r}")
 code=code.replace(old,new)
patch("        private Vector2 lastGestureMid;\n        private float lastGestureAngle;\n", "        private Vector2 singleTouchStart;\n        private bool singleTouchDragging;\n")
patch("        private static float TouchAngle(Vector2 first, Vector2 second)\n        {\n            Vector2 direction = second - first;\n            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;\n        }\n\n", "")
patch("        private bool IsOverUi(int id)\n        {\n            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(id);\n        }\n", """        private bool IsOverUi(int id)
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
""")
patch("""                if (Input.GetMouseButton(1) && !IsOverUi(-1))
                {
                    yaw += Input.GetAxis("Mouse X") * 3.4f;
                    height = Mathf.Clamp(height - Input.GetAxis("Mouse Y") * .5f, minHeight, maxHeight);
                }""","""                if (Input.GetMouseButton(1) && !IsOverUi(-1))
                {
                    float dx = Input.GetAxis("Mouse X");
                    float dy = Input.GetAxis("Mouse Y");
                    if (Mathf.Abs(dx) > Mathf.Abs(dy)) yaw += dx * 3.4f;
                    else height = Mathf.Clamp(height - dy * .5f, minHeight, maxHeight);
                }""")
patch("                if (finger.phase == TouchPhase.Began && IsOverUi(finger.fingerId))", "                if (finger.phase == TouchPhase.Began && IsOverUi(finger))")
patch("""                float angle = TouchAngle(a.position, b.position);
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
                }""","""                if (!gestureInitialized || gestureFirstId != a.fingerId || gestureSecondId != b.fingerId)
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
                }""")
patch("                Vector2 midpoint = (a.position + b.position) * .5f;\n","")
patch("""                if (eligible == 1)
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
                }""","""                // A second finger on the joystick or battle HUD must not rotate
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
                }""")
patch("                activeSingleId = -1;\n                Vector2", "                activeSingleId = -1;\n                Vector2") if False else None
patch("        [SerializeField] private Vector3 targetOffset = Vector3.zero;", "        [SerializeField] private Vector3 targetOffset = Vector3.up;")
patch("                    if (hits[i].distance < nearest) nearest = hits[i].distance;", "                    if (hits[i].distance > collisionRadius && hits[i].distance < nearest) nearest = hits[i].distance;")
assert code!=original
p.write_text(code,encoding="utf-8",newline="\n")
print("CAMERA_AXIS_LOCK_AND_UI_RAYCAST_UPDATED")
