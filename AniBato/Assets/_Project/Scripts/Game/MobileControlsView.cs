using System.Collections.Generic;
using AniBato.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class MobileControlsView : MonoBehaviour
    {
        private GameFlowController flow;
        private GameObject panel, ownEvents;
        private RectTransform safe;
        private OnScreenButton bootstrap;
        private readonly Dictionary<string, MobileControlButton> buttons = new Dictionary<string, MobileControlButton>();
        private readonly Dictionary<string, ControlAction> actions = new Dictionary<string, ControlAction>();
        private Gamepad savedFirst, savedSecond;
        private bool requested, lastCombat;
        private bool editing;
        private RectTransform movement;
        private GameObject directionalPad;
        private Image directionCenter;
        public TouchMovementSettings MovementSettings { get; private set; }
        public TouchMovementMode ActiveMovementMode { get; private set; }
        public UiLayoutTarget MovementLayout { get; private set; }
        public MobileStick Stick { get; private set; }
        private int lastBindings;
        public bool Visible => panel && panel.activeSelf;
        public Gamepad VirtualPad => bootstrap && bootstrap.control != null ? bootstrap.control.device as Gamepad : null;
        public IReadOnlyDictionary<string, MobileControlButton> Buttons => buttons;
        public void Configure(GameFlowController controller)
        {
            flow = controller; requested = Application.isMobilePlatform; MovementSettings = TouchMovementSettings.Load();
            panel = new GameObject("Translucent Mobile Controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            panel.SetActive(false); panel.transform.SetParent(transform, false);
            var canvas = panel.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            var scaler = panel.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540); scaler.matchWidthOrHeight = .5f;
            var safeObject = new GameObject("Safe Area", typeof(RectTransform)); safeObject.transform.SetParent(panel.transform, false);
            safe = safeObject.GetComponent<RectTransform>(); safe.offsetMin = safe.offsetMax = Vector2.zero;
            var device = new GameObject("Virtual Device Anchor", typeof(OnScreenButton)); device.transform.SetParent(panel.transform, false);
            bootstrap = device.GetComponent<OnScreenButton>(); bootstrap.controlPath = "<Gamepad>/buttonSouth";
            AddStick();
            Add("dash", "ダッシュ", ControlAction.Dash, false, new Vector2(235, 35), 70);
            Add("jump", "跳ぶ", ControlAction.Jump, true, new Vector2(-55, 55), 84);
            Add("attack", "通常", ControlAction.Attack, true, new Vector2(-150, 55), 84);
            Add("special", "特殊", ControlAction.Special, true, new Vector2(-55, 155), 84);
            Add("guard", "防御", ControlAction.Guard, true, new Vector2(-245, 55), 70);
            Add("dodge", "回避", ControlAction.Dodge, true, new Vector2(-245, 145), 70);
            Add("counter", "反撃", ControlAction.Counter, true, new Vector2(-150, 155), 70);
            Add("ultimate", "超必殺", ControlAction.Ultimate, true, new Vector2(-55, 250), 70);
            Add("pause", "ポーズ", ControlAction.Pause, false, new Vector2(110, 270), 62);
            Add("commands", "技表", null, false, new Vector2(235, 235), 62);
            ApplyMovementMode(MovementSettings.Menu);
        }
        private void AddStick()
        {
            var root = new GameObject("Touch movement pad", typeof(RectTransform)); root.transform.SetParent(safe, false);
            movement = (RectTransform)root.transform; movement.anchorMin = movement.anchorMax = Vector2.zero;
            movement.anchoredPosition = new Vector2(110, 110); movement.sizeDelta = new Vector2(170, 170);
            // Parent under the inactive panel before adding OnScreenControl. Otherwise
            // its default path briefly creates a device and reserves a physical pad slot.
            var go = new GameObject("Touch stick", typeof(RectTransform), typeof(Image), typeof(Outline)); go.transform.SetParent(movement, false);
            var rect = (RectTransform)go.transform; rect.sizeDelta = new Vector2(170, 170);
            var image = go.GetComponent<Image>(); image.sprite = MobileControlArt.Circle; image.alphaHitTestMinimumThreshold = .1f;
            var edge = go.GetComponent<Outline>(); edge.effectColor = new Color(1, .92f, .75f, .48f); edge.effectDistance = new Vector2(2, -2);
            var handle = new GameObject("Stick thumb", typeof(RectTransform), typeof(Image)); handle.transform.SetParent(go.transform, false);
            var knob = (RectTransform)handle.transform; knob.sizeDelta = new Vector2(66, 66);
            handle.GetComponent<Image>().sprite = MobileControlArt.Circle; handle.GetComponent<Image>().raycastTarget = false;
            Stick = go.AddComponent<MobileStick>(); Stick.Configure(image, knob);
            directionalPad = new GameObject("Touch directional pad", typeof(RectTransform)); directionalPad.transform.SetParent(movement, false);
            ((RectTransform)directionalPad.transform).sizeDelta = new Vector2(170, 170);
            AddDirection("left", "◀", new Vector2(-56, 0)); AddDirection("right", "▶", new Vector2(56, 0));
            AddDirection("up", "▲", new Vector2(0, 56)); AddDirection("down", "▼", new Vector2(0, -56));
            var center = new GameObject("D-pad center", typeof(RectTransform), typeof(Image)); center.transform.SetParent(directionalPad.transform, false);
            ((RectTransform)center.transform).sizeDelta = new Vector2(50, 50); directionCenter = center.GetComponent<Image>(); directionCenter.raycastTarget = false;
            MovementLayout = root.AddComponent<UiLayoutTarget>();
            // Keep the old ID so existing stick positions, size and opacity work in both modes.
            MovementLayout.Initialize("control.stick", "移動パッド（共通）", .24f, SetMovementOpacity, true);
            flow.LayoutEditor.Register(MovementLayout);
        }
        private void AddDirection(string key, string label, Vector2 position)
        {
            var button = CreateButton(directionalPad.transform, key, label, new Vector2(.5f, .5f), position, new Vector2(56, 56));
            button.GetComponent<Image>().sprite = null;
        }
        private void SetMovementOpacity(float value)
        {
            Stick.SetOpacity(value);
            foreach (string key in new[] { "left", "right", "up", "down" }) buttons[key].SetOpacity(value);
            directionCenter.color = new Color(1, .89f, .65f, value * .6f);
        }
        private static bool Direction(string key) => key == "left" || key == "right" || key == "up" || key == "down";
        private void ApplyMovementMode(TouchMovementMode mode)
        {
            ActiveMovementMode = mode; Stick.gameObject.SetActive(mode == TouchMovementMode.Stick);
            directionalPad.SetActive(mode == TouchMovementMode.DirectionalPad);
            Stick.GetComponent<Image>().raycastTarget = !editing;
            foreach (string key in new[] { "left", "right", "up", "down" }) buttons[key].GetComponent<Image>().raycastTarget = !editing;
            MovementLayout.Apply();
        }
        public void SetMovementMode(bool combat, TouchMovementMode mode)
        { MovementSettings.Set(combat, mode); lastBindings = int.MinValue; BindButtons(); }
        private void Add(string key, string title, ControlAction? action, bool right, Vector2 position, float width, float height = 0)
        {
            var button = CreateButton(safe, key, title, new Vector2(right ? 1 : 0, 0), position, new Vector2(width, height == 0 ? width : height));
            if (action.HasValue) actions.Add(key, action.Value);
            flow.LayoutEditor.Register((RectTransform)button.transform, "control." + key, title, .24f, button.SetOpacity);
        }
        private MobileControlButton CreateButton(Transform parent, string key, string title, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject("Touch " + key, typeof(RectTransform), typeof(Image), typeof(Outline)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.sprite = MobileControlArt.Circle; image.alphaHitTestMinimumThreshold = .1f;
            var outline = go.GetComponent<Outline>(); outline.effectColor = new Color(1, .92f, .75f, .48f); outline.effectDistance = new Vector2(2, -2);
            var words = new GameObject("Label", typeof(RectTransform), typeof(Text)); words.transform.SetParent(go.transform, false);
            var text = words.GetComponent<Text>(); text.font = RuntimeFonts.Japanese; text.text = title; text.fontSize = key == "dash" || key == "pause" ? 14 : 20;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            var button = go.AddComponent<MobileControlButton>(); button.controlPath = "<Gamepad>/buttonSouth"; button.Configure(go.GetComponent<Image>(), text);
            buttons.Add(key, button); return button;
        }
        public void SetEnabled(bool value) { requested = value; SetVisible(value || editing); }
        public void SetEditing(bool value) { ReleaseAll(); editing = value; SetVisible(requested || editing); lastBindings = int.MinValue; BindButtons(); }
        private void EnsureEvents()
        {
            if (EventSystem.current) return;
            if (!ownEvents)
            {
                ownEvents = new GameObject("Mobile Pointer Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
                ownEvents.transform.SetParent(transform, false);
                ownEvents.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            ownEvents.SetActive(true);
        }
        private void SetVisible(bool value)
        {
            if (!panel || Visible == value || !flow || !flow.Input) return;
            var input = flow.Input;
            if (value)
            {
                savedFirst = input.AssignedGamepad(0); savedSecond = input.AssignedGamepad(1);
                EnsureEvents(); panel.SetActive(true);
                input.AssignGamepad(0, VirtualPad);
                if (savedSecond != null && savedSecond.added) input.AssignGamepad(1, savedSecond);
                else if (savedFirst != null && savedFirst.added) input.AssignGamepad(1, savedFirst);
                lastBindings = int.MinValue; BindButtons();
            }
            else
            {
                ReleaseAll(); panel.SetActive(false);
                input.AssignGamepad(1, null);
                input.AssignGamepad(0, savedFirst != null && savedFirst.added ? savedFirst : null);
                input.AssignGamepad(1, savedSecond != null && savedSecond.added ? savedSecond : null);
                if (ownEvents) ownEvents.SetActive(false);
            }
        }
        private void LateUpdate()
        {
            if (!flow) return;
            SetVisible(requested || editing);
            if (!Visible) return;
            var area = Screen.safeArea;
            ApplySafeArea(area, Screen.width, Screen.height);
            BindButtons();
        }
        public void ApplySafeArea(Rect area, int width, int height)
        {
            if (!safe || width <= 0 || height <= 0) return;
            safe.anchorMin = new Vector2(area.xMin / width, area.yMin / height);
            safe.anchorMax = new Vector2(area.xMax / width, area.yMax / height);
        }
        private void BindButtons()
        {
            if (VirtualPad == null || !VirtualPad.added) return;
            bool combat = flow.Screen == GameScreen.Battle || flow.Screen == GameScreen.Countdown || editing;
            int hash = 17; var profile = flow.Input.Configuration.Profile(0, true);
            for (int i = 0; i < 13; i++) hash = unchecked(hash * 31 + profile[(ControlAction)i]);
            var mode = MovementSettings.ForContext(combat);
            if (hash == lastBindings && combat == lastCombat && mode == ActiveMovementMode) return;
            ReleaseAll(); lastBindings = hash; lastCombat = combat;
            ApplyMovementMode(mode);
            foreach (var pair in buttons)
            {
                string key = pair.Key; var button = pair.Value;
                bool primaryMenu = key == "left" || key == "right" || key == "up" || key == "down" || key == "jump" || key == "special" || key == "pause" || key == "commands";
                button.gameObject.SetActive(combat || primaryMenu);
                string path;
                if (key == "commands") path = "<Gamepad>/select";
                else if (!combat && key == "jump") path = "<Gamepad>/buttonSouth";
                else if (!combat && key == "special") path = "<Gamepad>/buttonEast";
                else if (Direction(key)) path = "<Gamepad>/dpad/" + key;
                else
                {
                    var control = ControlConfiguration.PadButton(VirtualPad, profile[actions[key]]);
                    path = "<Gamepad>" + control.path.Substring(VirtualPad.path.Length);
                }
                // Input System 1.20 requires unregistering an on-screen control before rebinding it.
                if (button.controlPath != path)
                {
                    button.enabled = false; button.controlPath = path; button.enabled = true;
                }
            }
            buttons["jump"].SetLabel(combat ? "跳ぶ" : "決定"); buttons["special"].SetLabel(combat ? "特殊" : "戻る");
        }
        public void ReleaseAll()
        {
            if (Stick) Stick.ReleaseContact();
            foreach (var button in buttons.Values) if (button) button.ReleaseContacts();
            if (VirtualPad != null && VirtualPad.added) InputSystem.ResetDevice(VirtualPad);
            if (flow && flow.Input) flow.Input.Clear();
        }
        public bool ContainsButtonPoint(Vector2 point)
        {
            if (!Visible) return false;
            if (Stick && Stick.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)Stick.transform, point) && Stick.GetComponent<Image>().IsRaycastLocationValid(point, null)) return true;
            foreach (var button in buttons.Values)
                if (button.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, point) && button.GetComponent<Image>().IsRaycastLocationValid(point, null)) return true;
            return false;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) ReleaseAll(); }
        private void OnApplicationPause(bool pause) { if (pause) ReleaseAll(); }
        private void OnDisable() { SetVisible(false); }
    }
}
