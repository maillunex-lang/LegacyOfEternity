using UnityEngine;
using UnityEngine.EventSystems;
using LegacyOfEternity.Player;
namespace LegacyOfEternity.Mobile
{
    public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField, Min(20f)] private float range = 65f;
        private RectTransform area;
        private void Awake() { area = GetComponent<RectTransform>(); }
        public void OnPointerDown(PointerEventData eventData) { OnDrag(eventData); }
        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area,eventData.position,eventData.pressEventCamera,out Vector2 local)) return;
            Vector2 value = Vector2.ClampMagnitude(local/range,1f);
            if (handle != null) handle.anchoredPosition = value*range;
            PlayerMotor.SetMobileInput(value);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            PlayerMotor.SetMobileInput(Vector2.zero);
        }
        private void OnDisable() { PlayerMotor.SetMobileInput(Vector2.zero); }
    }
}
