using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class UiCustomizationView : MonoBehaviour
    {
        private GameFlowController flow;
        private readonly List<UiLayoutTarget> targets = new List<UiLayoutTarget>();
        private GameObject panel;
        private RectTransform canvasRect, toolbar, selectionFrame;
        private Text selectedLabel, sizeLabel, opacityLabel;
        private Slider sizeSlider, opacitySlider;
        private UiLayoutTarget selected;
        private UiLayoutProfile before;
        private bool refreshing;
        private readonly Vector3[] corners = new Vector3[4];
        private Action closed;
        public bool IsEditing { get; private set; }
        public UiLayoutProfile Profile { get; private set; }
        public IReadOnlyList<UiLayoutTarget> Targets => targets;
        public UiLayoutTarget Selected => selected;
        public void Configure(GameFlowController value) { flow = value; Profile = UiLayoutProfile.Load(); Build(); }
        public void Register(UiLayoutTarget target)
        {
            if (!targets.Contains(target)) targets.Add(target); target.Bind(this);
        }
        public UiLayoutTarget Register(RectTransform rect, string id, string title, float opacity, Action<float> alpha)
        {
            var target = rect.gameObject.AddComponent<UiLayoutTarget>(); target.Initialize(id, title, opacity, alpha);
            Register(target); return target;
        }
        public void Open(Action onClosed)
        {
            before = Profile.Clone(); closed = onClosed; IsEditing = true;
            flow.TouchControls.SetEditing(true);
            foreach (var target in HudLayoutGroup.Ensure(flow.HudRoot)) Register(target);
            Canvas.ForceUpdateCanvases(); panel.SetActive(true); ApplyAll();
            if (targets.Count > 0) Select(targets[0]);
        }
        public void Select(UiLayoutTarget target) { selected = target; RefreshValues(); }
        public void RefreshValues()
        {
            if (!selected) return; refreshing = true;
            var e = selected.Current(); selectedLabel.text = selected.Title;
            sizeSlider.value = e.Scale; opacitySlider.value = e.Opacity;
            sizeLabel.text = "大きさ " + Mathf.RoundToInt(e.Scale * 100) + "%";
            opacityLabel.text = "不透明度 " + Mathf.RoundToInt(e.Opacity * 100) + "%";
            refreshing = false;
        }
        public void SetSelectedValues(float scale, float opacity)
        {
            if (!selected) return; var e = selected.Current();
            Profile.Set(selected.Id, e.X, e.Y, scale, opacity); selected.Apply(); RefreshValues();
        }
        public void Save() { Profile.Save(); Close(); }
        public void Cancel() { Profile = before.Clone(); ApplyAll(); Close(); }
        public void ResetLayout() { Profile = new UiLayoutProfile(); ApplyAll(); RefreshValues(); }
        private void ApplyAll() { foreach (var target in targets) if (target) target.Apply(); }
        private void Close()
        {
            IsEditing = false; ApplyAll(); panel.SetActive(false); flow.TouchControls.SetEditing(false);
            var callback = closed; closed = null; callback?.Invoke();
        }
        private void Cycle(int direction)
        {
            if (targets.Count == 0) return;
            int index = Mathf.Max(0, targets.IndexOf(selected)); Select(targets[(index + direction + targets.Count) % targets.Count]);
        }
        private void LateUpdate()
        {
            if (!IsEditing || !selected) return;
            toolbar.localScale = Vector3.one * Mathf.Min(1, (canvasRect.rect.width - 20) / 680);
            selected.Rect.GetWorldCorners(corners);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, corners[0]), null, out var a);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, corners[2]), null, out var b);
            selectionFrame.anchoredPosition = (a + b) * .5f; selectionFrame.sizeDelta = b - a;
        }
        private void Build()
        {
            panel = new GameObject("Control and HUD Layout Editor", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            panel.transform.SetParent(transform, false); canvasRect = (RectTransform)panel.transform;
            var canvas = panel.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 3000;
            var scaler = panel.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540); scaler.matchWidthOrHeight = .5f;
            var frameObject = new GameObject("Selected UI outline", typeof(RectTransform)); frameObject.transform.SetParent(canvasRect, false);
            selectionFrame = (RectTransform)frameObject.transform;
            selectionFrame.anchorMin = selectionFrame.anchorMax = new Vector2(.5f, .5f);
            for (int edge = 0; edge < 4; edge++)
            {
                var line = Box(selectionFrame, "Border " + edge, Vector2.zero, Vector2.zero, new Color(.25f, 1, .9f, .9f));
                bool horizontal = edge < 2; float side = edge % 2;
                line.anchorMin = horizontal ? new Vector2(0, side) : new Vector2(side, 0);
                line.anchorMax = horizontal ? new Vector2(1, side) : new Vector2(side, 1);
                line.sizeDelta = horizontal ? new Vector2(0, 2) : new Vector2(2, 0);
                line.GetComponent<Image>().raycastTarget = false;
            }
            toolbar = Box(canvasRect, "Layout toolbar", new Vector2(0, -12), new Vector2(680, 212), new Color(.09f, .12f, .18f, .96f));
            toolbar.anchorMin = toolbar.anchorMax = toolbar.pivot = new Vector2(.5f, 1);
            Label(toolbar, "説明", new Vector2(0, -20), new Vector2(630, 28), "配置を編集：タップで選択、ドラッグで移動", 18);
            selectedLabel = Label(toolbar, "選択中", new Vector2(0, -56), new Vector2(370, 32), "", 21);
            Button(toolbar, "前の項目", new Vector2(-270, -56), new Vector2(66, 32), "◀", () => Cycle(-1));
            Button(toolbar, "次の項目", new Vector2(270, -56), new Vector2(66, 32), "▶", () => Cycle(1));
            sizeLabel = Label(toolbar, "大きさ", new Vector2(-236, -96), new Vector2(160, 26), "", 16);
            opacityLabel = Label(toolbar, "不透明度", new Vector2(-236, -132), new Vector2(160, 26), "", 16);
            sizeSlider = Range(toolbar, "Size slider", new Vector2(40, -96), .5f, 2);
            opacitySlider = Range(toolbar, "Opacity slider", new Vector2(40, -132), .15f, 1);
            sizeSlider.onValueChanged.AddListener(v => { if (!refreshing) SetSelectedValues(v, opacitySlider.value); });
            opacitySlider.onValueChanged.AddListener(v => { if (!refreshing) SetSelectedValues(sizeSlider.value, v); });
            Button(toolbar, "保存", new Vector2(-214, -180), new Vector2(180, 36), "保存して戻る", Save);
            Button(toolbar, "キャンセル", new Vector2(0, -180), new Vector2(180, 36), "変更を破棄", Cancel);
            Button(toolbar, "初期配置", new Vector2(214, -180), new Vector2(180, 36), "初期配置へ", ResetLayout);
            panel.SetActive(false);
        }
        private static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
            go.GetComponent<Image>().color = color; return rect;
        }
        private static Text Label(Transform parent, string name, Vector2 position, Vector2 size, string value, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.font = RuntimeFonts.Japanese; text.text = value; text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false;
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(.5f, 1); text.rectTransform.anchoredPosition = position; text.rectTransform.sizeDelta = size; return text;
        }
        private static void Button(Transform parent, string name, Vector2 position, Vector2 size, string value, UnityEngine.Events.UnityAction clicked)
        {
            var rect = Box(parent, name, position, size, new Color(.32f, .29f, .25f));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>(); button.onClick.AddListener(clicked);
            var label = Label(rect, "Label", Vector2.zero, size, value, 18); label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(.5f, .5f);
        }
        private static Slider Range(Transform parent, string name, Vector2 position, float minimum, float maximum)
        {
            var rect = Box(parent, name, position, new Vector2(380, 24), Color.clear);
            var track = Box(rect, "Track", Vector2.zero, new Vector2(0, 6), new Color(.65f, .7f, .77f, .8f));
            track.anchorMin = new Vector2(0, .5f); track.anchorMax = new Vector2(1, .5f);
            track.GetComponent<Image>().raycastTarget = false;
            var handle = Box(rect, "Handle", Vector2.zero, new Vector2(28, 8), new Color(1, .83f, .5f));
            handle.anchorMin = handle.anchorMax = new Vector2(.5f, .5f);
            var slider = rect.gameObject.AddComponent<Slider>(); slider.minValue = minimum; slider.maxValue = maximum;
            slider.handleRect = handle; slider.targetGraphic = handle.GetComponent<Image>(); return slider;
        }
    }
}
