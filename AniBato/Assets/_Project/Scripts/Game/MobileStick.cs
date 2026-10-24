using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class MobileStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Vector2"), SerializeField] private string path = "<Gamepad>/leftStick";
        protected override string controlPathInternal { get => path; set => path = value; }
        private RectTransform rect, knob;
        private Image background, thumb;
        private UiLayoutTarget layout;
        private int pointer;
        public bool Held { get; private set; }
        public Vector2 Value { get; private set; }
        public void Configure(Image image, RectTransform handle)
        { rect = (RectTransform)transform; background = image; knob = handle; thumb = handle.GetComponent<Image>(); }
        public void SetOpacity(float opacity)
        {
            background.color = new Color(1, .89f, .65f, opacity);
            thumb.color = new Color(1, .95f, .82f, Mathf.Min(1, opacity + .3f));
        }
        private bool Editing { get { if (!layout) layout = GetComponent<UiLayoutTarget>(); return layout && layout.Editing; } }
        public void OnPointerDown(PointerEventData e)
        {
            if (!isActiveAndEnabled || Editing || Held) return;
            pointer = e.pointerId; Held = true; Move(e);
        }
        public void OnDrag(PointerEventData e) { if (!Editing && Held && pointer == e.pointerId) Move(e); }
        public void OnPointerUp(PointerEventData e) { if (Held && pointer == e.pointerId) ReleaseContact(); }
        private void Move(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var local);
            float radius = Mathf.Max(1, rect.rect.width * .32f);
            Value = Vector2.ClampMagnitude(local / radius, 1);
            if (Value.magnitude < .12f) Value = Vector2.zero;
            knob.anchoredPosition = Value * radius;
            if (control != null && control.device.added) SendValueToControl(Value);
        }
        public void ReleaseContact()
        {
            Held = false; Value = Vector2.zero; if (knob) knob.anchoredPosition = Vector2.zero;
            if (control != null && control.device.added) SendValueToControl(Vector2.zero);
        }
        protected override void OnDisable() { ReleaseContact(); base.OnDisable(); }
    }
}
