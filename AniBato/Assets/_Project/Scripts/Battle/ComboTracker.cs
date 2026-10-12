namespace AniBato.Battle
{
    /// <summary>Per-attacker/target damage budget and consecutive normal-hit sequence.</summary>
    public sealed class ComboTracker
    {
        public int HitCount { get; private set; }
        public int HeartsLost { get; private set; }
        public int ConsecutiveNormals { get; private set; }
        public bool Finished { get; private set; }
        public int LastHitTick { get; private set; } = int.MinValue / 2;
        public void RegisterHit(int tick, in BattleRuleDefinition rules, bool normal = true)
        {
            if (tick - LastHitTick > rules.ComboWindowFrames || Finished)
            { HitCount = 0; HeartsLost = 0; ConsecutiveNormals = 0; Finished = false; }
            HitCount = System.Math.Min(HitCount + 1, rules.MaxComboHits); LastHitTick = tick;
            ConsecutiveNormals = normal ? ConsecutiveNormals + 1 : 0;
            Finished = ConsecutiveNormals >= rules.MaxComboHits;
        }
        public bool TryLoseHeart(in BattleRuleDefinition rules, bool ultimate = false)
        {
            if (!ultimate && HeartsLost >= rules.MaxHeartsLostPerCombo) return false;
            HeartsLost++; return true;
        }
    }
}
