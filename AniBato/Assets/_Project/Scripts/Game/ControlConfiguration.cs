using System;
using AniBato.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AniBato.Game
{
    public sealed class ControlConfiguration
    {
        public const string DoubleTapStorage = "AniBato.Controls.DoubleTapDash";
        public bool DoubleTapDashEnabled { get; private set; } = true;
        public void ToggleDoubleTapDash()
        { DoubleTapDashEnabled = !DoubleTapDashEnabled; PlayerPrefs.SetInt(DoubleTapStorage, DoubleTapDashEnabled ? 1 : 0); PlayerPrefs.Save(); }
        private readonly ControlProfile[] keyboard = new ControlProfile[2], pad = new ControlProfile[2];
        public ControlProfile Profile(int player, bool gamepad) => gamepad ? pad[player] : keyboard[player];
        private static string Storage(int player, bool gamepad) => "AniBato.Controls.v1." + player + (gamepad ? ".pad" : ".keyboard");
        public void Initialize(InputBindings bindings)
        {
            DoubleTapDashEnabled = PlayerPrefs.GetInt(DoubleTapStorage, 1) != 0;
            for (int i = 0; i < 2; i++)
            {
                var k = bindings.ForPlayer(i);
                keyboard[i] = new ControlProfile(new[] { (int)k.Left, (int)k.Right, (int)k.Up, (int)k.Down, (int)k.Jump, (int)k.Attack,
                    (int)k.Special, (int)k.Guard, (int)k.Dodge, (int)k.Counter, (int)k.Ultimate, (int)k.Dash, i == 0 ? (int)Key.Escape : (int)Key.Backspace });
                pad[i] = new ControlProfile(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
                keyboard[i].Load(PlayerPrefs.GetString(Storage(i, false), ""), value => Enum.IsDefined(typeof(Key), value) && value != (int)Key.None);
                pad[i].Load(PlayerPrefs.GetString(Storage(i, true), ""), value => value >= 1 && value <= 16);
            }
        }
        public void Save(int player, bool gamepad) { PlayerPrefs.SetString(Storage(player, gamepad), Profile(player, gamepad).Save()); PlayerPrefs.Save(); }
        public void Reset(int player, bool gamepad) { Profile(player, gamepad).Reset(); Save(player, gamepad); }
        public PlayerKeyBindings Keyboard(int player, PlayerKeyBindings original)
        {
            var p = keyboard[player];
            original.Left = (Key)p[ControlAction.Left]; original.Right = (Key)p[ControlAction.Right]; original.Up = (Key)p[ControlAction.Up]; original.Down = (Key)p[ControlAction.Down];
            original.Jump = (Key)p[ControlAction.Jump]; original.Attack = (Key)p[ControlAction.Attack]; original.Special = (Key)p[ControlAction.Special]; original.Guard = (Key)p[ControlAction.Guard];
            original.Dodge = (Key)p[ControlAction.Dodge]; original.Counter = (Key)p[ControlAction.Counter]; original.Ultimate = (Key)p[ControlAction.Ultimate]; original.Dash = (Key)p[ControlAction.Dash];
            if (!p.IsDefault(ControlAction.Left)) original.AlternateLeft = null; if (!p.IsDefault(ControlAction.Right)) original.AlternateRight = null;
            if (!p.IsDefault(ControlAction.Up)) original.AlternateUp = null; if (!p.IsDefault(ControlAction.Down)) original.AlternateDown = null;
            if (!p.IsDefault(ControlAction.Jump)) original.AlternateJump = null; if (!p.IsDefault(ControlAction.Attack)) original.AlternateAttack = null;
            if (!p.IsDefault(ControlAction.Special)) original.AlternateSpecial = null; if (!p.IsDefault(ControlAction.Guard)) original.AlternateGuard = null;
            if (!p.IsDefault(ControlAction.Dodge)) original.AlternateDodge = null; if (!p.IsDefault(ControlAction.Counter)) original.AlternateCounter = null;
            if (!p.IsDefault(ControlAction.Ultimate)) original.AlternateUltimate = null; if (!p.IsDefault(ControlAction.Dash)) original.AlternateDash = null;
            return original;
        }
        public static ButtonControl PadButton(Gamepad p, int id)
        {
            switch (id)
            {
                case 1: return p.dpad.left; case 2: return p.dpad.right; case 3: return p.dpad.up; case 4: return p.dpad.down;
                case 5: return p.buttonSouth; case 6: return p.buttonWest; case 7: return p.buttonNorth; case 8: return p.leftTrigger;
                case 9: return p.rightTrigger; case 10: return p.rightShoulder; case 11: return p.leftShoulder; case 12: return p.leftStickButton;
                case 13: return p.startButton; case 14: return p.buttonEast; case 15: return p.rightStickButton; case 16: return p.selectButton;
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
    }
}
