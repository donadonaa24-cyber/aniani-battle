namespace AniBato.Battle
{
    public sealed class FighterInputBuffer
    {
        private readonly int[] expires = new int[8];
        private readonly BattleVector[] directions = new BattleVector[8];
        private int previousDirection;
        private int lastTapDirection;
        private int lastTapTick = int.MinValue / 2;
        private int dashDirection;
        private bool dashFromDoubleTap;
        public FighterInputBuffer() { for (int i = 0; i < expires.Length; i++) expires[i] = -1; }

        public void Push(in FrameInput input, int tick, in BattleRuleDefinition rules)
        {
            if (input.DisableDoubleTapDash)
            {
                lastTapDirection = 0; lastTapTick = int.MinValue / 2;
                if (dashFromDoubleTap) { expires[7] = -1; dashFromDoubleTap = false; }
            }
            for (int i = 0; i < expires.Length; i++)
                if (((int)input.Pressed & (1 << i)) != 0)
                { expires[i] = tick + rules.InputBufferFrames - 1; directions[i] = input.DirectionFor((InputButtons)(1 << i)); }
            int direction = input.X > rules.DirectionThreshold ? 1 : input.X < -rules.DirectionThreshold ? -1 : 0;
            if (!input.DisableDoubleTapDash && direction != 0 && direction != previousDirection)
            {
                if (lastTapDirection == direction && tick - lastTapTick <= rules.DoubleTapFrames)
                {
                    expires[7] = tick + rules.InputBufferFrames - 1;
                    dashDirection = direction;
                    dashFromDoubleTap = true;
                    lastTapTick = int.MinValue / 2;
                }
                else { lastTapDirection = direction; lastTapTick = tick; }
            }
            if (input.IsPressed(InputButtons.Dash))
            {
                dashFromDoubleTap = false;
                float dashX = input.DirectionFor(InputButtons.Dash).X;
                dashDirection = dashX > rules.DirectionThreshold ? 1 : dashX < -rules.DirectionThreshold ? -1 : 0;
            }
            previousDirection = direction;
        }

        public bool Consume(InputButtons button, int tick)
        {
            int index = Index(button);
            if (expires[index] < tick) return false;
            expires[index] = -1;
            return true;
        }
        public int ConsumeDashDirection(int fallback) => dashDirection == 0 ? fallback : dashDirection;
        public BattleVector DirectionFor(InputButtons button) => directions[Index(button)];
        public void ClearActions() { for (int i = 0; i < expires.Length; i++) expires[i] = -1; }
        private static int Index(InputButtons button)
        {
            int bits = (int)button;
            for (int i = 0; i < 8; i++) if (bits == (1 << i)) return i;
            throw new System.ArgumentException("単一の入力ボタンを指定してください。");
        }
    }
}
