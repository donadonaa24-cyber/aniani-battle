using System;

namespace AniBato.Battle
{
    // Same entry point for EditMode and the offline report; all inputs pass through Tick.
    public sealed class CpuMatchMetrics
    {
        public int Player1, Player2, Difficulty, Seed, Ticks, UltimateStarts, UltimateHits, HeartsLost, UnforcedRingOuts;
        public readonly int[] Attacks = new int[2];
        public MatchResult Result;
        public MatchEndReason Reason;
        public bool ShrinkReached;
        public static CpuMatchMetrics Run(CharacterDefinition p1, CharacterDefinition p2, BattleRuleDefinition rules, StageGeometry stage,
            CpuDefinition cpu, int seed)
        {
            var sim = new BattleSimulation(p1, p2, rules, stage);
            var a = new CpuBrain(cpu, (uint)seed); var b = new CpuBrain(cpu, (uint)seed ^ 0x9e3779b9u);
            var data = new CpuMatchMetrics { Seed = seed };
            int[] lastHostileHit = { -10000, -10000 };
            int limit = rules.DurationTicks + p1.UltimateCutInFrames + p2.UltimateCutInFrames + 2;
            for (int i = 0; i < limit && !sim.Match.IsFinished; i++)
            {
                sim.Tick(a.NextInput(sim, 0), b.NextInput(sim, 1));
                foreach (var e in sim.Events)
                {
                    if (e.Kind == BattleEventKind.AttackStarted) data.Attacks[e.SourcePlayer]++;
                    if (e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.CounterSuccess)
                        if (e.TargetPlayer >= 0) lastHostileHit[e.TargetPlayer] = e.Tick;
                    if (e.Kind == BattleEventKind.UltimateStarted) data.UltimateStarts++;
                    if (e.Kind == BattleEventKind.Hit && e.AttackKind == AttackKind.Ultimate) data.UltimateHits++;
                    if (e.Kind == BattleEventKind.HeartLost) data.HeartsLost += e.Value;
                    if (e.Kind == BattleEventKind.RingOut && e.TargetPlayer >= 0 && e.Tick - lastHostileHit[e.TargetPlayer] > 120) data.UnforcedRingOuts++;
                }
            }
            if (!sim.Match.IsFinished) throw new InvalidOperationException("CPU対戦が時間内に終了しませんでした。");
            data.Ticks = sim.Match.TickCount; data.Result = sim.Match.Result; data.Reason = sim.Match.EndReason; data.ShrinkReached = sim.Shrink.Started;
            return data;
        }
    }
}
