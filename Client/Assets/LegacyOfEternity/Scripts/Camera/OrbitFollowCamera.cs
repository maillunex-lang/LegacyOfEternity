using UnityEngine;

namespace LegacyOfEternity.CameraSystem
{
    /// <summary>Standalone follow camera for the prototype, independent of legacy FBCamera.</summary>
    public sealed class OrbitFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(2f)] private float distance = 9f;
        [SerializeField, Range(10f, 75f)] private float pitch = 35f;
        [SerializeField, Min(1f)] private float smoothness = 10f;
        [SerializeField] private float yaw = 0f;
        [SerializeField] private Vector3 targetOffset = Vector3.up * 1.5f;

        public void SetTarget(Transform value) { target = value; }

        private void LateUpdate()
        {
            if (target == null) return;

            // Static angle for first milestone; touch orbit controls will be added with the mobile HUD.
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = target.position + targetOffset - rotation * Vector3.forward * distance;
            float t = 1f - Mathf.Exp(-smoothness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
            transform.rotation = rotation;
        }
    }
}
