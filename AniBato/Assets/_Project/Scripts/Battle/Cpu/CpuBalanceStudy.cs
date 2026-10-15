using System;
using System.Collections.Generic;
using System.Text;

namespace AniBato.Battle
{
    public static class CpuBalanceStudy
    {
        public static List<CpuMatchMetrics> Run(CharacterDefinition[] roster, BattleRuleDefinition rules, StageGeometry stage,
            CpuDefinition[] difficulties, int seeds = 30)
        {
            if (roster == null || roster.Length < 1 || difficulties == null || difficulties.Length < 1 || seeds < 1)
                throw new ArgumentException("CPU集計条件が不正です。");
            var rows = new List<CpuMatchMetrics>();
            for (int d = 0; d < difficulties.Length; d++) for (int a = 0; a < roster.Length; a++) for (int b = 0; b < roster.Length; b++) for (int seed = 1; seed <= seeds; seed++)
            {
                var row = CpuMatchMetrics.Run(roster[a], roster[b], rules, stage, difficulties[d], seed);
                row.Player1 = a; row.Player2 = b; row.Difficulty = d; rows.Add(row);
            }
            return rows;
        }
        public static string Csv(IEnumerable<CpuMatchMetrics> rows)
        {
            var csv = new StringBuilder("difficulty,p1,p2,seed,ticks,result,reason,p1Attacks,p2Attacks,heartsLost,ultimateStarts,ultimateHits,unforcedRingOuts,shrinkReached,normalHits,specialHits,clashes,guarded,counters\n");
            foreach (var r in rows) csv.AppendLine(string.Join(",", r.Difficulty, r.Player1, r.Player2, r.Seed, r.Ticks, r.Result, r.Reason,
                r.Attacks[0], r.Attacks[1], r.HeartsLost, r.UltimateStarts, r.UltimateHits, r.UnforcedRingOuts, r.ShrinkReached, r.NormalHits, r.SpecialHits, r.Clashes, r.Guarded, r.Counters));
            return csv.ToString();
        }
    }
}
