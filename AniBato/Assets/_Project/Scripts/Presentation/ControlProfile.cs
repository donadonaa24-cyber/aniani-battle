using System;

namespace AniBato.Presentation
{
    public enum ControlAction { Left, Right, Up, Down, Jump, Attack, Special, Guard, Dodge, Counter, Ultimate, Dash, Pause }
    public enum InputLabelDevice { Keyboard, Xbox, PlayStation }
    public sealed class ControlProfile
    {
        public const int Count = 13;
        private readonly int[] defaults, values;
        public ControlProfile(int[] initial)
        { if (initial == null || initial.Length != Count) throw new ArgumentException("Expected thirteen bindings."); defaults = (int[])initial.Clone(); values = (int[])initial.Clone(); }
        public int this[ControlAction action] => values[(int)action];
        public bool IsDefault(ControlAction action) => values[(int)action] == defaults[(int)action];
        // A duplicate swaps the displaced action to the previous button; no hidden double action.
        public void Rebind(ControlAction action, int value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            int index = (int)action, previous = values[index];
            for (int i = 0; i < Count; i++) if (i != index && values[i] == value) values[i] = previous;
            values[index] = value;
        }
        public void Reset() => Array.Copy(defaults, values, Count);
        public string Save() => "1:" + string.Join(",", values);
        public bool Load(string text, Func<int, bool> allowed)
        {
            if (text == null || !text.StartsWith("1:")) return false;
            var parts = text.Substring(2).Split(','); if (parts.Length != Count) return false;
            var loaded = new int[Count]; var unique = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < Count; i++) if (!int.TryParse(parts[i], out loaded[i]) || !allowed(loaded[i]) || !unique.Add(loaded[i])) return false;
            Array.Copy(loaded, values, Count); return true;
        }
    }
    public static class InputGlyphs
    {
        // IDs are platform-neutral, matching InputController.PadButton.
        public static string Pad(int button, InputLabelDevice device)
        {
            string[] xbox = { "", "←", "→", "↑", "↓", "A", "X", "Y", "LT", "RT", "RB", "LB", "LS", "START", "B", "RS", "BACK" };
            string[] ps = { "", "←", "→", "↑", "↓", "×", "□", "△", "L2", "R2", "R1", "L1", "L3", "OPTIONS", "○", "R3", "SHARE" };
            return button > 0 && button < xbox.Length ? (device == InputLabelDevice.PlayStation ? ps : xbox)[button] : "?";
        }
        public static InputLabelDevice Detect(string layout, string product, string manufacturer)
        {
            string description = (layout + " " + product + " " + manufacturer).ToLowerInvariant();
            return description.Contains("dual") || description.Contains("playstation") || description.Contains("sony") ? InputLabelDevice.PlayStation : InputLabelDevice.Xbox;
        }
    }
}
