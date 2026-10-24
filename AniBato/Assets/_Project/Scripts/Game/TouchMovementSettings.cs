using UnityEngine;

namespace AniBato.Game
{
    public enum TouchMovementMode { Stick, DirectionalPad }

    public sealed class TouchMovementSettings
    {
        public const string CombatStorage = "AniBato.TouchMovement.Combat";
        public const string MenuStorage = "AniBato.TouchMovement.Menu";
        public TouchMovementMode Combat { get; private set; }
        public TouchMovementMode Menu { get; private set; }
        public static TouchMovementSettings Load() => new TouchMovementSettings {
            Combat = Read(CombatStorage, TouchMovementMode.Stick),
            Menu = Read(MenuStorage, TouchMovementMode.DirectionalPad)
        };
        private static TouchMovementMode Read(string key, TouchMovementMode fallback)
        {
            int value = PlayerPrefs.GetInt(key, (int)fallback);
            return value == 0 || value == 1 ? (TouchMovementMode)value : fallback;
        }
        public TouchMovementMode ForContext(bool combat) => combat ? Combat : Menu;
        public void Set(bool combat, TouchMovementMode mode)
        {
            if (mode != TouchMovementMode.Stick && mode != TouchMovementMode.DirectionalPad) return;
            if (combat) Combat = mode; else Menu = mode;
            PlayerPrefs.SetInt(combat ? CombatStorage : MenuStorage, (int)mode); PlayerPrefs.Save();
        }
        public static string Label(TouchMovementMode mode) => mode == TouchMovementMode.Stick ? "スティック" : "十字キー";
    }
}
