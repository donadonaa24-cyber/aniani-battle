using AniBato.Battle;

namespace AniBato.Presentation
{
    // Presentation clock only. Never writes a fighter, the simulation clock, or Time.timeScale.
    public sealed class BattleFeedback
    {
        public float ShakeRemaining { get; private set; }
        public float FlashRemaining { get; private set; }
        public float SlowRemaining { get; private set; }
        public float FinishRemaining { get; private set; }
        public string FinishText { get; private set; } = "";
        public void Apply(BattleEvent value, float shake, float flash, float finish)
        {
            if (value.Kind == BattleEventKind.Hit || value.Kind == BattleEventKind.Break || value.Kind == BattleEventKind.ComboFinish)
                ShakeRemaining = shake;
            if (value.Kind == BattleEventKind.JustGuard || value.Kind == BattleEventKind.JustDodge)
                FlashRemaining = SlowRemaining = flash;
            if (value.Kind == BattleEventKind.UltimateStarted) FlashRemaining = flash;
            if (value.Kind == BattleEventKind.MatchEnded)
            { FinishRemaining = finish; FinishText = value.EndReason == MatchEndReason.RingOut ? "RING OUT!" : value.EndReason == MatchEndReason.TimeUp ? "TIME UP!" : "K.O.!"; }
        }
        public void Advance(float seconds)
        {
            ShakeRemaining = System.Math.Max(0, ShakeRemaining - seconds); FlashRemaining = System.Math.Max(0, FlashRemaining - seconds);
            SlowRemaining = System.Math.Max(0, SlowRemaining - seconds); FinishRemaining = System.Math.Max(0, FinishRemaining - seconds);
        }
        public void Reset() { ShakeRemaining = FlashRemaining = SlowRemaining = FinishRemaining = 0; FinishText = ""; }
    }
}
