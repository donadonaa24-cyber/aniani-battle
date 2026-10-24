using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class UiLayoutTarget : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private RectTransform rect, parent;
        private Vector2 baseAnchor, basePosition, dragOffset, lastParentSize;
        private Vector3 baseScale;
        private float baseOpacity;
        private Action<float> setOpacity;
        private CanvasGroup group;
        private Image hitArea;
        private UiLayoutProfile standalone;
        public UiCustomizationView Owner { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public bool Editing => Owner && Owner.IsEditing;
        public RectTransform Rect => rect;
        public void Initialize(string id, string title, float opacity, Action<float> alpha = null, bool addHitArea = false)
        {
            Id = id; Title = title; rect = (RectTransform)transform; parent = (RectTransform)rect.parent;
            baseAnchor = rect.anchorMin; basePosition = rect.anchoredPosition; baseScale = rect.localScale; baseOpacity = opacity;
            setOpacity = alpha;
            if (alpha == null)
            {
                group = gameObject.GetComponent<CanvasGroup>();
                if (!group) group = gameObject.AddComponent<CanvasGroup>();
            }
            if (addHitArea)
            {
                hitArea = gameObject.AddComponent<Image>(); hitArea.color = Color.clear; hitArea.raycastTarget = false;
            }
            standalone = UiLayoutProfile.Load(); Apply();
        }
        public void Bind(UiCustomizationView owner) { Owner = owner; Apply(); }
        private UiLayoutProfile Profile => Owner ? Owner.Profile : standalone;
        public UiLayoutEntry Current()
        {
            var e = Profile?.Find(Id); if (e != null) return e;
            var p = parent.rect.size;
            return new UiLayoutEntry { Id = Id, X = baseAnchor.x + basePosition.x / Mathf.Max(1, p.x),
                Y = baseAnchor.y + basePosition.y / Mathf.Max(1, p.y), Scale = 1, Opacity = baseOpacity };
        }
        public void Apply()
        {
            if (!parent || parent.rect.width <= 0 || parent.rect.height <= 0) return;
            var e = Current(); var size = parent.rect.size;
            float scale = Mathf.Min(e.Scale, size.x / Mathf.Max(1, rect.rect.width * baseScale.x), size.y / Mathf.Max(1, rect.rect.height * baseScale.y));
            rect.localScale = baseScale * scale;
            var extent = Vector2.Scale(rect.rect.size, new Vector2(rect.localScale.x, rect.localScale.y));
            float x = Mathf.Clamp(e.X, extent.x * rect.pivot.x / size.x, 1 - extent.x * (1 - rect.pivot.x) / size.x);
            float y = Mathf.Clamp(e.Y, extent.y * rect.pivot.y / size.y, 1 - extent.y * (1 - rect.pivot.y) / size.y);
            rect.anchorMin = rect.anchorMax = new Vector2(x, y); rect.anchoredPosition = Vector2.zero;
            if (setOpacity != null) setOpacity(e.Opacity); else group.alpha = e.Opacity;
            if (hitArea) hitArea.raycastTarget = Editing;
            lastParentSize = size;
        }
        private void LateUpdate()
        {
            if (parent && (parent.rect.size != lastParentSize || hitArea && hitArea.raycastTarget != Editing)) Apply();
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (!Editing) return;
            Owner.Select(this);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, e.pressEventCamera, out var point);
            dragOffset = (Vector2)rect.localPosition - point;
        }
        public void OnDrag(PointerEventData e)
        {
            if (!Editing) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, e.pressEventCamera, out var point);
            var p = point + dragOffset; var current = Current();
            Owner.Profile.Set(Id, (p.x - parent.rect.xMin) / parent.rect.width, (p.y - parent.rect.yMin) / parent.rect.height, current.Scale, current.Opacity);
            Apply(); Owner.RefreshValues();
        }
    }
}
