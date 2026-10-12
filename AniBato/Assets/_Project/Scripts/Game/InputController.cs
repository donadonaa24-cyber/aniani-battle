using AniBato.Battle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

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
        public InputBindings Bindings => bindings;
        public void Configure(InputBindings value) => bindings = value;
        public void AssignKeyboard(Keyboard keyboard) { assignedKeyboard = keyboard; Clear(); }

        // Explicit device assignment; one gamepad cannot control both fighters.
        public void AssignGamepad(int player, Gamepad pad)
        {
            if (player < 0 || player > 1) throw new System.ArgumentOutOfRangeException(nameof(player));
            if (pad != null && assignedPads[1 - player] == pad)
            { assignedPads[1 - player] = null; reservedPads[1 - player] = null; previousMenuAxes[1 - player] = Vector2.zero; }
            assignedPads[player] = pad; reservedPads[player] = pad; previousMenuAxes[player] = Vector2.zero; Clear();
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
            { assignedPads[i] = null; previousMenuAxes[i] = Vector2.zero; }
            // Native disconnects may be reported as Removed before Reconnected. Retain identity.
            if (assignedKeyboard == device) assignedKeyboard = null;
            Clear();
        }
        public void Capture()
        {
            if (!bindings) return;
            for (int i = 0; i < 2; i++)
            {
                FrameInput keyboard = ReadKeyboard(bindings.ForPlayer(i));
                FrameInput pad = ReadGamepad(assignedPads[i]);
                var input = new FrameInput(Mathf.Clamp(keyboard.X + pad.X, -1, 1), Mathf.Clamp(keyboard.Y + pad.Y, -1, 1),
                    keyboard.Held | pad.Held, keyboard.Pressed | pad.Pressed);
                latches[i].Sample(input);
            }
        }
        public FrameInput Consume(int player) => latches[player].Consume();
        public void Clear() { foreach (var latch in latches) latch.Clear(); }
        private void OnApplicationFocus(bool focus) { if (!focus) Clear(); }
        public bool DebugBoxesPressed => KeyPressed(Key.F9);
        public bool RematchPressed => bindings && KeyPressed(bindings.Rematch);
        public bool FullscreenPressed => bindings && KeyPressed(bindings.Fullscreen);
        public int CharacterSelectionDelta(int player) =>
            (KeyPressed(player == 0 ? Key.F2 : Key.F4) ? 1 : 0) - (KeyPressed(player == 0 ? Key.F1 : Key.F3) ? 1 : 0);

        public bool AnyMenuKeyPressed
        { get { bool value = false; if (ActiveKeyboard != null) foreach (var key in ActiveKeyboard.allKeys) value |= MenuButton(key, MenuAction.AnyKey); return value; } }
        public bool EscapePressed => MenuKey(Key.Escape, MenuAction.Cancel);
        public MenuInput ReadMenu(int player)
        {
            var keys = bindings.ForPlayer(player);
            int x = (AnyPressed(keys.Right, keys.AlternateRight) ? 1 : 0) - (AnyPressed(keys.Left, keys.AlternateLeft) ? 1 : 0);
            int y = (AnyPressed(keys.Down, keys.AlternateDown) ? 1 : 0) - (AnyPressed(keys.Up, keys.AlternateUp) ? 1 : 0);
            var confirmAction = player == 0 ? MenuAction.ConfirmP1 : MenuAction.ConfirmP2;
            bool confirm = MenuKeys(keys.Attack, keys.AlternateAttack, confirmAction) | MenuKeys(keys.Jump, keys.AlternateJump, confirmAction) | (player == 0 && MenuKey(Key.Enter, confirmAction));
            bool cancel = MenuKeys(keys.Special, keys.AlternateSpecial, MenuAction.Cancel);
            var pad = assignedPads[player];
            Vector2 axes = Vector2.zero;
            bool start = false;
            if (pad != null && pad.added)
            {
                axes = pad.leftStick.ReadValue(); if (pad.dpad.ReadValue().sqrMagnitude > axes.sqrMagnitude) axes = pad.dpad.ReadValue();
                if (Mathf.Abs(axes.x) > 0.5f && Mathf.Abs(previousMenuAxes[player].x) <= 0.5f) x += axes.x > 0 ? 1 : -1;
                if (Mathf.Abs(axes.y) > 0.5f && Mathf.Abs(previousMenuAxes[player].y) <= 0.5f) y += axes.y > 0 ? -1 : 1;
                confirm |= MenuButton(pad.buttonSouth, confirmAction);
                cancel |= MenuButton(pad.buttonEast, MenuAction.Cancel);
                start = MenuButton(pad.startButton, MenuAction.Start);
            }
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
        private FrameInput ReadGamepad(Gamepad pad)
        {
            if (pad == null || !pad.added) return default;
            InputButtons held = InputButtons.None, pressed = InputButtons.None;
            ReadButton(pad.buttonSouth, InputButtons.Jump, ref held, ref pressed); ReadButton(pad.buttonWest, InputButtons.Attack, ref held, ref pressed);
            ReadButton(pad.buttonNorth, InputButtons.Special, ref held, ref pressed); ReadButton(pad.leftTrigger, InputButtons.Guard, ref held, ref pressed);
            ReadButton(pad.rightTrigger, InputButtons.Dodge, ref held, ref pressed); ReadButton(pad.rightShoulder, InputButtons.Counter, ref held, ref pressed);
            ReadButton(pad.leftShoulder, InputButtons.Ultimate, ref held, ref pressed); ReadButton(pad.leftStickButton, InputButtons.Dash, ref held, ref pressed);
            Vector2 axis = pad.leftStick.ReadValue();
            if (pad.dpad.ReadValue().sqrMagnitude > axis.sqrMagnitude) axis = pad.dpad.ReadValue();
            return new FrameInput(axis.x, axis.y, held, pressed);
        }
    }
}

