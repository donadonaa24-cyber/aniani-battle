namespace AniBato.Battle
{
    public enum BattleEventKind { Hit, HeartLost, Break, KO, RingOut, TimeUp, MatchEnded,
        Guarded, Dodged, CounterSuccess, JustGuard, JustDodge, ComboFinish, GaugeChanged, UltimateStarted,
        StageShrinkWarning, StageShrinkStarted, StageSegmentWarning, StageShrunk, AttackStarted, GuardBroken, Clash, SupportCollected, BarrierAbsorbed, SupportWalletCollected }
    public readonly struct BattleEvent
    {
        public readonly BattleEventKind Kind;
        public readonly int Tick;
        public readonly int SourcePlayer;
        public readonly int TargetPlayer;
        public readonly long AttackId;
        public readonly int Value;
        public readonly AttackKind AttackKind;
        public readonly MatchEndReason EndReason;
        public BattleEvent(BattleEventKind kind, int tick, int source = -1, int target = -1, long attackId = 0, int value = 0, AttackKind attackKind = AttackKind.Normal, MatchEndReason endReason = default)
        { Kind = kind; Tick = tick; SourcePlayer = source; TargetPlayer = target; AttackId = attackId; Value = value; AttackKind = attackKind; EndReason = endReason; }
    }
}
