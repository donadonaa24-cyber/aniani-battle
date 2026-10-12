using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class MatchResolver
    {
        public void Resolve(MatchState match, FighterState p1, FighterState p2, StageGeometry stage,
            in BattleRuleDefinition rules, List<BattleEvent> events)
        {
            bool out1 = rules.EnableRingOut && stage.IsOutside(p1.Position);
            bool out2 = rules.EnableRingOut && stage.IsOutside(p2.Position);
            if (out1 || out2)
            {
                if (out1) RingOut(p1, match.TickCount, events);
                if (out2) RingOut(p2, match.TickCount, events);
                Finish(match, ResultForDefeats(out1, out2), MatchEndReason.RingOut, events);
            }
            else if (p1.Hearts == 0 || p2.Hearts == 0)
                Finish(match, ResultForDefeats(p1.Hearts == 0, p2.Hearts == 0), MatchEndReason.Hearts, events);
            else if (match.RemainingTicks == 0)
            {
                events.Add(new BattleEvent(BattleEventKind.TimeUp, match.TickCount));
                MatchResult result = p1.Hearts != p2.Hearts ? (p1.Hearts > p2.Hearts ? MatchResult.Player1Wins : MatchResult.Player2Wins) :
                    p1.NormalHitCount != p2.NormalHitCount ? (p1.NormalHitCount < p2.NormalHitCount ? MatchResult.Player1Wins : MatchResult.Player2Wins) : MatchResult.Draw;
                Finish(match, result, MatchEndReason.TimeUp, events);
            }
        }
        private static MatchResult ResultForDefeats(bool p1, bool p2) => p1 && p2 ? MatchResult.Draw : p1 ? MatchResult.Player2Wins : MatchResult.Player1Wins;
        private static void RingOut(FighterState fighter, int tick, List<BattleEvent> events)
        { fighter.Mode = FighterMode.KO; events.Add(new BattleEvent(BattleEventKind.RingOut, tick, target: fighter.PlayerIndex)); }
        private static void Finish(MatchState match, MatchResult result, MatchEndReason reason, List<BattleEvent> events)
        { match.Result = result; match.EndReason = reason; events.Add(new BattleEvent(BattleEventKind.MatchEnded, match.TickCount, value: (int)result, endReason: reason)); }
    }
}
