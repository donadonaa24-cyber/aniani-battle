using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AniBato.Game
{
    [Serializable]
    public struct PlayerKeyBindings
    {
        public Key Left, Right, Up, Down;
        public Key Jump, Attack, Special, Guard, Dodge, Counter, Ultimate, Dash;
        public Key[] AlternateLeft, AlternateRight, AlternateUp, AlternateDown;
        public Key[] AlternateJump, AlternateAttack, AlternateSpecial, AlternateGuard;
        public Key[] AlternateDodge, AlternateCounter, AlternateUltimate, AlternateDash;
    }
    [CreateAssetMenu(menuName = "あにバト/入力設定")]
    public sealed class InputBindings : ScriptableObject
    {
        public PlayerKeyBindings P1;
        public PlayerKeyBindings P2;
        public Key Rematch = Key.R;
        public Key Fullscreen = Key.F11;
        public int SchemaVersion;
        public PlayerKeyBindings ForPlayer(int player) => player == 0 ? P1 : P2;
        public static PlayerKeyBindings DefaultP1 => new PlayerKeyBindings
        {
            Left = Key.A, Right = Key.D, Up = Key.W, Down = Key.S,
            Jump = Key.Space, Attack = Key.J, Special = Key.K, Guard = Key.L,
            Dodge = Key.LeftShift, Counter = Key.I, Ultimate = Key.U, Dash = Key.F
        };
        public static PlayerKeyBindings DefaultP2 => new PlayerKeyBindings
        {
            Left = Key.LeftArrow, Right = Key.RightArrow, Up = Key.UpArrow, Down = Key.DownArrow,
            Jump = Key.RightCtrl, Attack = Key.Delete, Special = Key.End, Guard = Key.PageDown,
            Dodge = Key.RightShift, Counter = Key.PageUp, Ultimate = Key.Home, Dash = Key.RightAlt,
            AlternateJump = new[] { Key.Numpad0 }, AlternateAttack = new[] { Key.Numpad1 },
            AlternateSpecial = new[] { Key.Numpad2 }, AlternateGuard = new[] { Key.Numpad3 },
            AlternateDodge = new[] { Key.Numpad4 }, AlternateCounter = new[] { Key.Numpad5 },
            AlternateUltimate = new[] { Key.Numpad6 }, AlternateDash = new[] { Key.NumpadEnter }
        };
    }
}
