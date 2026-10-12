namespace AniBato.Battle
{
    public enum MatchResult { Playing, Player1Wins, Player2Wins, Draw }
    public enum MatchEndReason { None, RingOut, Hearts, TimeUp }
    public sealed class MatchState
    {
        public int TickCount { get; internal set; }
        public int RemainingTicks { get; internal set; }
        public double RemainingSeconds => RemainingTicks / (double)BattleRuleDefinition.TickRate;
        public MatchResult Result { get; internal set; } = MatchResult.Playing;
        public MatchEndReason EndReason { get; internal set; }
        public bool IsFinished => Result != MatchResult.Playing;
        internal MatchState(int duration) { RemainingTicks = duration; }
    }
}
