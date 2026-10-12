using System;

namespace AniBato.Battle
{
    [Flags]
    public enum InputButtons
    {
        None = 0, Jump = 1 << 0, Attack = 1 << 1, Special = 1 << 2, Guard = 1 << 3,
        Dodge = 1 << 4, Counter = 1 << 5, Ultimate = 1 << 6, Dash = 1 << 7
    }

    public readonly struct FrameInput
    {
        public readonly float X;
        public readonly float Y;
        public readonly float ActionX;
        public readonly float ActionY;
        public readonly InputButtons Held;
        public readonly InputButtons Pressed;
        private readonly BattleVector[] buttonDirections;
        public FrameInput(float x = 0, float y = 0, InputButtons held = InputButtons.None, InputButtons pressed = InputButtons.None,
            float? actionX = null, float? actionY = null)
        {
            X = Math.Max(-1f, Math.Min(1f, x)); Y = Math.Max(-1f, Math.Min(1f, y));
            Held = held; Pressed = pressed;
            ActionX = Math.Max(-1f, Math.Min(1f, actionX ?? X)); ActionY = Math.Max(-1f, Math.Min(1f, actionY ?? Y));
            buttonDirections = null;
        }
        internal FrameInput(float x, float y, InputButtons held, InputButtons pressed, BattleVector[] directions)
            : this(x, y, held, pressed) { buttonDirections = (BattleVector[])directions.Clone(); }
        public BattleVector DirectionFor(InputButtons button)
        {
            if (buttonDirections == null) return new BattleVector(ActionX, ActionY);
            for (int i = 0; i < buttonDirections.Length; i++)
                if ((int)button == (1 << i)) return buttonDirections[i];
            throw new ArgumentException("単一の行動ボタンを指定してください。");
        }
        public bool IsHeld(InputButtons button) => (Held & button) != 0;
        public bool IsPressed(InputButtons button) => (Pressed & button) != 0;
        public FrameInput WithoutPressed() => new FrameInput(X, Y, Held);
    }

    /// <summary>Update samples OR their edges until the next simulation tick consumes them.</summary>
    public sealed class FrameInputLatch
    {
        private FrameInput latest;
        private InputButtons pending;
        private readonly BattleVector[] directions = new BattleVector[8];
        public void Sample(in FrameInput input)
        {
            latest = input;
            for (int i = 0; i < directions.Length; i++)
                if (((int)input.Pressed & (1 << i)) != 0) directions[i] = input.DirectionFor((InputButtons)(1 << i));
            pending |= input.Pressed;
        }
        public FrameInput Consume()
        {
            var input = pending == InputButtons.None ? new FrameInput(latest.X, latest.Y, latest.Held) :
                new FrameInput(latest.X, latest.Y, latest.Held, pending, directions);
            pending = InputButtons.None;
            return input;
        }
        public void Clear() { latest = default; pending = InputButtons.None; }
    }
}
