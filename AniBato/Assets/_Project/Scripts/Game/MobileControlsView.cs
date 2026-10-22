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
        private int lastBindings;
        public bool Visible => panel && panel.activeSelf;
        public Gamepad VirtualPad => bootstrap && bootstrap.control != null ? bootstrap.control.device as Gamepad : null;
        public IReadOnlyDictionary<string, MobileControlButton> Buttons => buttons;
        public void Configure(GameFlowController controller)
        {
            flow = controller; requested = Application.isMobilePlatform;
            panel = new GameObject("Translucent Mobile Controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            panel.SetActive(false); panel.transform.SetParent(transform, false);
            var canvas = panel.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            var scaler = panel.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540); scaler.matchWidthOrHeight = .5f;
            var safeObject = new GameObject("Safe Area", typeof(RectTransform)); safeObject.transform.SetParent(panel.transform, false);
            safe = safeObject.GetComponent<RectTransform>(); safe.offsetMin = safe.offsetMax = Vector2.zero;
            var device = new GameObject("Virtual Device Anchor", typeof(OnScreenButton)); device.transform.SetParent(panel.transform, false);
            bootstrap = device.GetComponent<OnScreenButton>(); bootstrap.controlPath = "<Gamepad>/buttonSouth";
            Add("left", "◀", ControlAction.Left, false, new Vector2(35, 110), 70);
            Add("right", "▶", ControlAction.Right, false, new Vector2(185, 110), 70);
            Add("up", "▲", ControlAction.Up, false, new Vector2(110, 185), 70);
            Add("down", "▼", ControlAction.Down, false, new Vector2(110, 35), 70);
            Add("dash", "ダッシュ", ControlAction.Dash, false, new Vector2(235, 35), 70);
            Add("jump", "跳ぶ", ControlAction.Jump, true, new Vector2(-55, 55), 84);
            Add("attack", "通常", ControlAction.Attack, true, new Vector2(-150, 55), 84);
            Add("special", "特殊", ControlAction.Special, true, new Vector2(-55, 155), 84);
            Add("guard", "防御", ControlAction.Guard, true, new Vector2(-245, 55), 70);
            Add("dodge", "回避", ControlAction.Dodge, true, new Vector2(-245, 145), 70);
            Add("counter", "反撃", ControlAction.Counter, true, new Vector2(-150, 155), 70);
            Add("ultimate", "超必殺", ControlAction.Ultimate, true, new Vector2(-55, 250), 70);
            Add("pause", "ポーズ", ControlAction.Pause, false, new Vector2(110, 270), 70, 38);
            Add("commands", "技表", null, false, new Vector2(235, 235), 70, 38);
        }
        private void Add(string key, string title, ControlAction? action, bool right, Vector2 position, float width, float height = 0)
        {
            var go = new GameObject("Touch " + key, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(MobileControlButton));
            go.transform.SetParent(safe, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(right ? 1 : 0, 0);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(width, height == 0 ? width : height);
            var outline = go.GetComponent<Outline>(); outline.effectColor = new Color(1, .92f, .75f, .48f); outline.effectDistance = new Vector2(2, -2);
            var words = new GameObject("Label", typeof(RectTransform), typeof(Text)); words.transform.SetParent(go.transform, false);
            var text = words.GetComponent<Text>(); text.font = RuntimeFonts.Japanese; text.text = title; text.fontSize = key == "dash" || key == "pause" ? 14 : 20;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            var button = go.GetComponent<MobileControlButton>(); button.controlPath = "<Gamepad>/buttonSouth"; button.Configure(go.GetComponent<Image>(), text);
            buttons.Add(key, button); if (action.HasValue) actions.Add(key, action.Value);
        }
        public void SetEnabled(bool value) { requested = value; SetVisible(value); }
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
            SetVisible(requested);
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
            bool combat = flow.Screen == GameScreen.Battle || flow.Screen == GameScreen.Countdown;
            int hash = 17; var profile = flow.Input.Configuration.Profile(0, true);
            for (int i = 0; i < 13; i++) hash = unchecked(hash * 31 + profile[(ControlAction)i]);
            if (hash == lastBindings && combat == lastCombat) return;
            ReleaseAll(); lastBindings = hash; lastCombat = combat;
            foreach (var pair in buttons)
            {
                string key = pair.Key; var button = pair.Value;
                bool primaryMenu = key == "left" || key == "right" || key == "up" || key == "down" || key == "jump" || key == "special" || key == "pause" || key == "commands";
                button.gameObject.SetActive(combat || primaryMenu);
                string path;
                if (key == "commands") path = "<Gamepad>/select";
                else if (!combat && key == "jump") path = "<Gamepad>/buttonSouth";
                else if (!combat && key == "special") path = "<Gamepad>/buttonEast";
                else if (!combat && (key == "left" || key == "right" || key == "up" || key == "down")) path = "<Gamepad>/dpad/" + key;
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
            foreach (var button in buttons.Values) if (button) button.ReleaseContacts();
            if (VirtualPad != null && VirtualPad.added) InputSystem.ResetDevice(VirtualPad);
            if (flow && flow.Input) flow.Input.Clear();
        }
        public bool ContainsButtonPoint(Vector2 point)
        {
            if (!Visible) return false;
            foreach (var button in buttons.Values)
                if (button.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, point)) return true;
            return false;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) ReleaseAll(); }
        private void OnApplicationPause(bool pause) { if (pause) ReleaseAll(); }
        private void OnDisable() { SetVisible(false); }
    }
}
