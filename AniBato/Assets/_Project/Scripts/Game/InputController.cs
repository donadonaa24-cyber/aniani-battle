using AniBato.Battle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using AniBato.Presentation;

namespace AniBato.Game
{
    [System.Flags]
    public enum MenuAction { ConfirmP1 = 1, ConfirmP2 = 2, Confirm = 3, Cancel = 4, Start = 8, AnyKey = 16 }

    public sealed class InputController : MonoBehaviour
    {
        [SerializeField] private InputBindings bindings;
        private readonly FrameInputLatch[] latches = { new FrameInputLatch(), new FrameInputLatch() };
        private readonly Gamepad[] assignedPads = new Gamepad[2];
        private readonly Gamepad[] reservedPads = new Gamepad[2];
        private sealed class ButtonEdge { public bool Held, Pressed; public int Frame = -1; public uint LastPressUpdate = uint.MaxValue; }
        private readonly Dictionary<ButtonControl, ButtonEdge> edges = new Dictionary<ButtonControl, ButtonEdge>();
        private readonly HashSet<ButtonControl> blocked = new HashSet<ButtonControl>();
        private readonly Dictionary<ButtonControl, MenuAction> menuGesture = new Dictionary<ButtonControl, MenuAction>();
        public bool WaitingForMenuRelease => blocked.Count > 0;
        public string GamepadName(int player) => assignedPads[player] != null ? assignedPads[player].displayName : "未接続";
        public Gamepad AssignedGamepad(int player) => assignedPads[player];
        public void GuardScreenTransition(MenuAction action = MenuAction.Confirm)
        { foreach (var gesture in menuGesture) if ((gesture.Value & action) != 0) blocked.Add(gesture.Key); Clear(); }
        public void RefreshMenuReleaseGuard()
        {
            menuGesture.Clear();
            foreach (var button in edges.Keys) Edge(button);
            blocked.RemoveWhere(button => !button.device.added || !button.isPressed && !Edge(button));
        }
        // A physical rising edge is cached once per render frame. A persistent Input System
        // wasPressedThisFrame flag can never become a second screen action on the next frame.
        private bool Edge(ButtonControl button)
        {
            if (button == null || !button.device.added) return false;
            if (!edges.TryGetValue(button, out var state)) { state = new ButtonEdge(); edges.Add(button, state); }
            if (state.Frame != Time.frameCount)
            {
                bool held = button.isPressed; uint update = UnityEngine.InputSystem.LowLevel.InputState.updateCount;
                // Include a full press/release delivered between render polls, while rejecting
                // an already observed update's edge. Held buttons rely on physical rising edges.
                state.Pressed = held && !state.Held || !held && !state.Held && button.wasPressedThisFrame && state.LastPressUpdate != update;
                if (state.Pressed) state.LastPressUpdate = update;
                state.Held = held; state.Frame = Time.frameCount;
            }
            return state.Pressed;
        }
        private bool Pressed(ButtonControl button) => Edge(button) && !blocked.Contains(button);
        private bool MenuButton(ButtonControl button, MenuAction action)
        {
            bool pressed = Pressed(button);
            if (pressed) { menuGesture.TryGetValue(button, out var previous); menuGesture[button] = previous | action; }
            return pressed;
        }
        private bool MenuKey(Key key, MenuAction action) => ActiveKeyboard != null && key != Key.None && MenuButton(ActiveKeyboard[key], action);
        private bool MenuKeys(Key primary, Key[] alternatives, MenuAction action)
        { bool value = MenuKey(primary, action); if (alternatives != null) foreach (var key in alternatives) value |= MenuKey(key, action); return value; }
        private Keyboard assignedKeyboard;
        private readonly Vector2[] previousMenuAxes = new Vector2[2];
        private readonly Vector2[] stickSamples = new Vector2[2], dpadSamples = new Vector2[2];
        private readonly uint[] axesUpdates = { uint.MaxValue, uint.MaxValue };
        private void SamplePadAxes(int player)
        {
            uint update = UnityEngine.InputSystem.LowLevel.InputState.updateCount;
            if (axesUpdates[player] == update) return;
            axesUpdates[player] = update;
            var pad = assignedPads[player];
            stickSamples[player] = pad != null && pad.added ? pad.leftStick.ReadValue() : Vector2.zero;
            dpadSamples[player] = pad != null && pad.added ? pad.dpad.ReadValue() : Vector2.zero;
        }
        public InputBindings Bindings => bindings;
        public ControlConfiguration Configuration { get; } = new ControlConfiguration();
        private bool configurationReady;
        private readonly InputLabelDevice[] labelDevice = { InputLabelDevice.Keyboard, InputLabelDevice.Keyboard };
        public InputLabelDevice LabelDevice(int player) => labelDevice[player];
        public void Configure(InputBindings value) { bindings = value; Configuration.Initialize(value); configurationReady = true; }
        private void EnsureConfiguration() { if (!configurationReady && bindings) { Configuration.Initialize(bindings); configurationReady = true; } }
        public PlayerKeyBindings EffectiveKeys(int player) { EnsureConfiguration(); return Configuration.Keyboard(player, bindings.ForPlayer(player)); }
        public string Label(int player, ControlAction action)
        {
            EnsureConfiguration();
            return labelDevice[player] == InputLabelDevice.Keyboard ? ((Key)Configuration.Profile(player, false)[action]).ToString() :
                InputGlyphs.Pad(Configuration.Profile(player, true)[action], labelDevice[player]);
        }
        public string ConfirmLabel(int player) => labelDevice[player] == InputLabelDevice.Keyboard ? Label(player, ControlAction.Attack) : InputGlyphs.Pad(5, labelDevice[player]);
        public string CancelLabel(int player) => labelDevice[player] == InputLabelDevice.Keyboard ? Label(player, ControlAction.Special) : InputGlyphs.Pad(14, labelDevice[player]);
        public bool CaptureBinding(int player, bool gamepad, ControlAction action)
        {
            EnsureConfiguration();
            if (gamepad)
            {
                var p = assignedPads[player]; if (p == null || !p.added) return false;
                for (int id = 1; id <= 16; id++) if (Pressed(ControlConfiguration.PadButton(p, id)))
                { Configuration.Profile(player, true).Rebind(action, id); Configuration.Save(player, true); blocked.Add(ControlConfiguration.PadButton(p, id)); Clear(); return true; }
            }
            else if (ActiveKeyboard != null) foreach (var key in ActiveKeyboard.allKeys)
                if (key.keyCode != Key.Escape && key.keyCode != Key.F9 && key.keyCode != Key.F11 && key.keyCode != Key.Tab && Pressed(key))
                { Configuration.Profile(player, false).Rebind(action, (int)key.keyCode); Configuration.Save(player, false); blocked.Add(key); Clear(); return true; }
            return false;
        }
        public void AssignKeyboard(Keyboard keyboard) { assignedKeyboard = keyboard; Clear(); }

        // Explicit device assignment; one gamepad cannot control both fighters.
        public void AssignGamepad(int player, Gamepad pad)
        {
            if (player < 0 || player > 1) throw new System.ArgumentOutOfRangeException(nameof(player));
            if (pad != null && assignedPads[1 - player] == pad)
            { assignedPads[1 - player] = null; reservedPads[1 - player] = null; previousMenuAxes[1 - player] = Vector2.zero; }
            assignedPads[player] = pad; reservedPads[player] = pad; previousMenuAxes[player] = Vector2.zero; axesUpdates[player] = uint.MaxValue; Clear();
        }
        private void OnEnable() => InputSystem.onDeviceChange += OnDeviceChange;
        private void Start()
        {
            for (int i = 0; i < 2 && i < Gamepad.all.Count; i++)
                if (assignedPads[i] == null && reservedPads[i] == null && assignedPads[1 - i] != Gamepad.all[i]) AssignGamepad(i, Gamepad.all[i]);
        }
        private void OnDisable() { InputSystem.onDeviceChange -= OnDeviceChange; Clear(); }
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {
                if (device is Gamepad pad)
                {
                    for (int i = 0; i < 2; i++) if (reservedPads[i] == pad) { AssignGamepad(i, pad); return; }
                    for (int i = 0; i < 2; i++) if (assignedPads[i] == null && reservedPads[i] == null && assignedPads[1 - i] != pad) { AssignGamepad(i, pad); break; }
                }
                return;
            }
            if (change != InputDeviceChange.Removed && change != InputDeviceChange.Disconnected) return;
            for (int i = 0; i < assignedPads.Length; i++) if (assignedPads[i] == device)
            { assignedPads[i] = null; previousMenuAxes[i] = Vector2.zero; axesUpdates[i] = uint.MaxValue; }
            // Native disconnects may be reported as Removed before Reconnected. Retain identity.
            if (assignedKeyboard == device) assignedKeyboard = null;
            Clear();
        }
        public void Capture()
        {
            if (!bindings) return;
            for (int i = 0; i < 2; i++)
            {
                FrameInput keyboard = ReadKeyboard(EffectiveKeys(i));
                FrameInput pad = ReadGamepad(assignedPads[i], i);
                if (keyboard.Pressed != InputButtons.None || keyboard.X != 0 || keyboard.Y != 0) labelDevice[i] = InputLabelDevice.Keyboard;
                if (pad.Pressed != InputButtons.None || pad.X != 0 || pad.Y != 0) SetPadLabel(i);
                var input = new FrameInput(Mathf.Clamp(keyboard.X + pad.X, -1, 1), Mathf.Clamp(keyboard.Y + pad.Y, -1, 1),
                    keyboard.Held | pad.Held, keyboard.Pressed | pad.Pressed, disableDoubleTapDash: !Configuration.DoubleTapDashEnabled);
                latches[i].Sample(input);
            }
        }
        public FrameInput Consume(int player) => latches[player].Consume();
        public void Clear() { foreach (var latch in latches) latch.Clear(); }
        private void OnApplicationFocus(bool focus) { if (!focus) Clear(); }
        public bool DebugBoxesPressed => KeyPressed(Key.F9);
        public bool PracticeRatePressed => KeyPressed(Key.F5);
        public bool PracticePausePressed => KeyPressed(Key.F6);
        public bool PracticeStepPressed => KeyPressed(Key.F7);
        public bool RematchPressed => bindings && KeyPressed(bindings.Rematch);
        public bool FullscreenPressed => bindings && KeyPressed(bindings.Fullscreen);
        public int CharacterSelectionDelta(int player) =>
            (KeyPressed(player == 0 ? Key.F2 : Key.F4) ? 1 : 0) - (KeyPressed(player == 0 ? Key.F1 : Key.F3) ? 1 : 0);

        public bool AnyMenuKeyPressed
        { get { bool value = false; if (ActiveKeyboard != null) foreach (var key in ActiveKeyboard.allKeys) value |= MenuButton(key, MenuAction.AnyKey); return value; } }
        public bool EscapePressed => MenuKey(Key.Escape, MenuAction.Cancel);
        public bool CommandListPressed => MenuKey(Key.Tab, MenuAction.Start) || assignedPads[0] != null && MenuButton(assignedPads[0].selectButton, MenuAction.Start);
        public string CommandListLabel => labelDevice[0] == InputLabelDevice.Keyboard ? "Tab" : InputGlyphs.Pad(16, labelDevice[0]);
        public MenuInput ReadMenu(int player)
        {
            var keys = EffectiveKeys(player);
            int x = (AnyPressed(keys.Right, keys.AlternateRight) ? 1 : 0) - (AnyPressed(keys.Left, keys.AlternateLeft) ? 1 : 0);
            int y = (AnyPressed(keys.Down, keys.AlternateDown) ? 1 : 0) - (AnyPressed(keys.Up, keys.AlternateUp) ? 1 : 0);
            var confirmAction = player == 0 ? MenuAction.ConfirmP1 : MenuAction.ConfirmP2;
            bool confirm = MenuKeys(keys.Attack, keys.AlternateAttack, confirmAction) | MenuKeys(keys.Jump, keys.AlternateJump, confirmAction) | (player == 0 && MenuKey(Key.Enter, confirmAction));
            bool cancel = MenuKeys(keys.Special, keys.AlternateSpecial, MenuAction.Cancel);
            if (x != 0 || y != 0 || confirm || cancel) labelDevice[player] = InputLabelDevice.Keyboard;
            var pad = assignedPads[player];
            Vector2 axes = Vector2.zero;
            bool start = false;
            if (pad != null && pad.added)
            {
                SamplePadAxes(player);
                axes = stickSamples[player]; if (dpadSamples[player].sqrMagnitude > axes.sqrMagnitude) axes = dpadSamples[player];
                if (Mathf.Abs(axes.x) > 0.5f && Mathf.Abs(previousMenuAxes[player].x) <= 0.5f) x += axes.x > 0 ? 1 : -1;
                if (Mathf.Abs(axes.y) > 0.5f && Mathf.Abs(previousMenuAxes[player].y) <= 0.5f) y += axes.y > 0 ? -1 : 1;
                confirm |= MenuButton(pad.buttonSouth, confirmAction);
                cancel |= MenuButton(pad.buttonEast, MenuAction.Cancel);
                start = MenuButton(ControlConfiguration.PadButton(pad, Configuration.Profile(player, true)[ControlAction.Pause]), MenuAction.Start);
                if (axes.sqrMagnitude > .1f || Edge(pad.buttonSouth) || Edge(pad.buttonEast) || start) SetPadLabel(player);
            }
            start |= MenuKey((Key)Configuration.Profile(player, false)[ControlAction.Pause], MenuAction.Start);
            previousMenuAxes[player] = axes;
            return new MenuInput(System.Math.Sign(x), System.Math.Sign(y), confirm, cancel, start);
        }

        private Keyboard ActiveKeyboard => assignedKeyboard != null && assignedKeyboard.added ? assignedKeyboard : Keyboard.current;
        private bool KeyPressed(Key key) => ActiveKeyboard != null && key != Key.None && Pressed(ActiveKeyboard[key]);
        private bool KeyHeld(Key key) => ActiveKeyboard != null && key != Key.None && ActiveKeyboard[key].isPressed && !blocked.Contains(ActiveKeyboard[key]);
        private bool AnyHeld(Key primary, Key[] alternatives)
        { if (KeyHeld(primary)) return true; if (alternatives != null) foreach (Key key in alternatives) if (KeyHeld(key)) return true; return false; }
        private bool AnyPressed(Key primary, Key[] alternatives)
        { if (KeyPressed(primary)) return true; if (alternatives != null) foreach (Key key in alternatives) if (KeyPressed(key)) return true; return false; }
        private void ReadKey(Key key, Key[] alternatives, InputButtons flag, ref InputButtons held, ref InputButtons pressed)
        { if (AnyHeld(key, alternatives)) held |= flag; if (AnyPressed(key, alternatives)) pressed |= flag; }
        private FrameInput ReadKeyboard(PlayerKeyBindings keys)
        {
            InputButtons held = InputButtons.None, pressed = InputButtons.None;
            ReadKey(keys.Jump, keys.AlternateJump, InputButtons.Jump, ref held, ref pressed); ReadKey(keys.Attack, keys.AlternateAttack, InputButtons.Attack, ref held, ref pressed);
            ReadKey(keys.Special, keys.AlternateSpecial, InputButtons.Special, ref held, ref pressed); ReadKey(keys.Guard, keys.AlternateGuard, InputButtons.Guard, ref held, ref pressed);
            ReadKey(keys.Dodge, keys.AlternateDodge, InputButtons.Dodge, ref held, ref pressed); ReadKey(keys.Counter, keys.AlternateCounter, InputButtons.Counter, ref held, ref pressed);
            ReadKey(keys.Ultimate, keys.AlternateUltimate, InputButtons.Ultimate, ref held, ref pressed); ReadKey(keys.Dash, keys.AlternateDash, InputButtons.Dash, ref held, ref pressed);
            return new FrameInput((AnyHeld(keys.Right, keys.AlternateRight) ? 1 : 0) - (AnyHeld(keys.Left, keys.AlternateLeft) ? 1 : 0),
                (AnyHeld(keys.Up, keys.AlternateUp) ? 1 : 0) - (AnyHeld(keys.Down, keys.AlternateDown) ? 1 : 0), held, pressed);
        }
        private void ReadButton(ButtonControl button, InputButtons flag, ref InputButtons held, ref InputButtons pressed)
        { if (button.isPressed && !blocked.Contains(button)) held |= flag; if (Pressed(button)) pressed |= flag; }
        private void SetPadLabel(int player)
        { var p = assignedPads[player]; if (p != null) labelDevice[player] = InputGlyphs.Detect(p.layout, p.description.product, p.description.manufacturer); }
        private FrameInput ReadGamepad(Gamepad pad, int player)
        {
            if (pad == null || !pad.added) return default;
            InputButtons held = InputButtons.None, pressed = InputButtons.None;
            var profile = Configuration.Profile(player, true);
            for (int action = 4; action <= 11; action++)
                ReadButton(ControlConfiguration.PadButton(pad, profile[(ControlAction)action]), (InputButtons)(1 << (action - 4)), ref held, ref pressed);
            SamplePadAxes(player);
            Vector2 axis = stickSamples[player];
            var digital = new Vector2((ControlConfiguration.PadButton(pad, profile[ControlAction.Right]).isPressed ? 1 : 0) - (ControlConfiguration.PadButton(pad, profile[ControlAction.Left]).isPressed ? 1 : 0),
                (ControlConfiguration.PadButton(pad, profile[ControlAction.Up]).isPressed ? 1 : 0) - (ControlConfiguration.PadButton(pad, profile[ControlAction.Down]).isPressed ? 1 : 0));
            if (digital.sqrMagnitude > axis.sqrMagnitude) axis = digital;
            return new FrameInput(axis.x, axis.y, held, pressed);
        }
    }
}

