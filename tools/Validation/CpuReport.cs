using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AniBato.Battle;

internal static class CpuReport
{
    public static int Trace(int first, int second, int seed, int difficulty = 2)
    {
        var sim = new BattleSimulation(PhaseSixADefaults.Character(first), PhaseSixADefaults.Character(second), PhaseSixADefaults.Rules(), PhaseOneDefaults.Stage());
        var a = new CpuBrain(CpuDefinition.ForDifficulty(difficulty), (uint)seed); var b = new CpuBrain(CpuDefinition.ForDifficulty(difficulty), (uint)seed ^ 0x9e3779b9u);
        while (!sim.Match.IsFinished)
        {
            var x = a.NextInput(sim, 0); var y = b.NextInput(sim, 1);
            if (sim.P1.Position.Y < 0 || sim.P2.Position.Y < 0)
                Console.WriteLine($"{sim.Match.TickCount}: P1 {sim.P1.Mode} ({sim.P1.Position.X:F2},{sim.P1.Position.Y:F2}) face{sim.P1.Facing} jump{sim.P1.JumpsUsed} sta{sim.P1.Stamina:F1} up{sim.P1.UpSpecialUsed} stun{sim.P1.HitstunRemaining} act{sim.P1.ActionFrames} input{x.X},{x.Y},{x.Pressed} / P2 {sim.P2.Mode} ({sim.P2.Position.X:F2},{sim.P2.Position.Y:F2}) face{sim.P2.Facing} jump{sim.P2.JumpsUsed} sta{sim.P2.Stamina:F1} up{sim.P2.UpSpecialUsed} input{y.X},{y.Y},{y.Pressed}");
            sim.Tick(x, y);
            foreach (var e in sim.Events) if (e.Kind == BattleEventKind.AttackStarted || e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.CounterSuccess)
                Console.WriteLine($"  {sim.Match.TickCount} {e.Kind} {e.SourcePlayer}->{e.TargetPlayer}, P1 {sim.P1.Mode} x{sim.P1.Position.X:F2} {sim.P1.Attack?.Definition.Name} P2 {sim.P2.Mode} x{sim.P2.Position.X:F2} {sim.P2.Attack?.Definition.Name}");
        }
        return 0;
    }
    public static int Write(string stem, bool legacyGauge = false)
    {
        var rules = PhaseSixADefaults.Rules();
        if (legacyGauge) { var old = rules.Gauge; old.OnHit = 12; old.OnDamaged = 8; rules.Gauge = old; }
        var rows = CpuBalanceStudy.Run(new[] { PhaseSixADefaults.Character(0), PhaseSixADefaults.Character(1), PhaseSixADefaults.Character(2) },
            rules, PhaseOneDefaults.Stage(), new[] { CpuDefinition.ForDifficulty(0), CpuDefinition.ForDifficulty(1), CpuDefinition.ForDifficulty(2) });
        string csv = CpuBalanceStudy.Csv(rows);
        var md = new StringBuilder("# CPU自動対戦集計\n\n.NET直接駆動。純C#の作成既定値を使用。Unity生成済みScriptableObjectでの結果ではありません。\n\n" +
            "各難易度・全9組（同キャラ含む、P1/P2両順）×seed 1〜30、合計810試合。70秒、ハート3、縮小あり、ゲージ初期0。\n" +
            "CPUシードはP1=seed、P2=seed XOR 0x9e3779b9。時間は試合tick/60（カットイン停止を含まない）。\n" +
            "自滅近似は『場外敗者が直近120tickに被弾・カウンターを受けていない』。意図を証明する指標ではありません。\n\n");
        var gauge = rules.Gauge;
        md.AppendLine($"ゲージ: 命中{gauge.OnHit} / 被弾{gauge.OnDamaged} / カウンター{gauge.OnCounter} / ジャスト{gauge.OnJustGuard}、最大/消費100、1人1試合1回。\n");
        md.AppendLine("| 難易度 | 組合せ P1-P2 | 試合数 | P1勝/P2勝/分 | 平均秒 | KO/場外/時間 | 縮小到達率 | 超必殺発動試合率 | 発動数/命中数 | 自滅近似 | 両者攻撃/ハート減少 |\n|---|---|---:|---|---:|---|---:|---:|---|---:|---|");
        string[] names = { "透真", "蓮", "千鶴" }, difficulties = { "やさしい", "ふつう", "むずかしい" };
        foreach (var g in rows.GroupBy(r => r.Difficulty))
        {
            Append(md, g.ToArray(), difficulties[g.Key], "全組");
            foreach (var matchup in g.GroupBy(r => new { r.Player1, r.Player2 }))
                Append(md, matchup.ToArray(), difficulties[g.Key], names[matchup.Key.Player1] + "-" + names[matchup.Key.Player2]);
        }
        string beforePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stem)), "phase5b-cpu-final.csv");
        if (File.Exists(beforePath))
        {
            var before = File.ReadAllLines(beforePath).Skip(1).Where(line => !String.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToArray();
            if (before.Length != rows.Count) throw new InvalidOperationException("before/afterの試合数が一致しません。");
            foreach (var row in rows)
                if (!before.Any(old => int.Parse(old[0]) == row.Difficulty && int.Parse(old[1]) == row.Player1 && int.Parse(old[2]) == row.Player2 && int.Parse(old[3]) == row.Seed))
                    throw new InvalidOperationException("before/afterの組合せ・seedが一致しません。");
            var comparison = new StringBuilder("# フェーズ6A CPU集計：before → after\n\n" +
                "beforeはClaude検証済みフェーズ5Bの純C#集計。afterはPhaseSixADefaultsと現行CPU。両方とも全9組×30seed×3難易度=810試合。生成済みScriptableObjectの実集計ではありません。\n\n" +
                "| 難易度 | 平均決着秒 | 縮小到達率 | 超必殺あり試合率 | 自滅近似 |\n|---|---|---|---|---|\n");
            for (int d = 0; d < 3; d++)
            {
                var old = before.Where(row => int.Parse(row[0]) == d).ToArray(); var now = rows.Where(row => row.Difficulty == d).ToArray();
                comparison.AppendLine(FormattableString.Invariant($"| {difficulties[d]} | {old.Average(row => int.Parse(row[4]) / 60d):F2}→{now.Average(row => row.Ticks / 60d):F2} | {old.Count(row => int.Parse(row[4]) >= 3600) * 100d / old.Length:F1}→{now.Count(row => row.ShrinkReached) * 100d / now.Length:F1}% | {old.Count(row => int.Parse(row[10]) > 0) * 100d / old.Length:F1}→{now.Count(row => row.UltimateStarts > 0) * 100d / now.Length:F1}% | {old.Sum(row => int.Parse(row[12]))}→{now.Sum(row => row.UnforcedRingOuts)} |"));
            }
            comparison.AppendLine("\n目標：平均35〜55秒・縮小20〜40%・超必殺半数前後。原本のハート3／通常3ヒット=1／特殊1／超必殺2／70秒は保持。ガード相性はユーザー決定に従い変更。対人難易度・相性は実機確認が必要。\n\nCSVはTestResults/phase5b-cpu-final.csv・phase6a-cpu-final.csv。実SOはCpuBalanceReportBuilder.Writeでphase6a-cpu-unity.csv/.mdへ別保存。\n\n---\n");
            md.Insert(0, comparison.ToString());
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(stem)));
        File.WriteAllText(stem + ".csv", csv, new UTF8Encoding(false));
        File.WriteAllText(stem + ".md", md.ToString(), new UTF8Encoding(false));
        foreach (var g in rows.GroupBy(r => r.Difficulty))
            Console.WriteLine(difficulties[g.Key] + $": {g.Count()}試合 / 平均 {g.Average(r => r.Ticks/60d):F2}秒 / 縮小 {g.Count(r => r.Ticks >= 3600)/(double)g.Count():P1} / 超必殺 {g.Count(r => r.UltimateStarts > 0)/(double)g.Count():P1} / 自滅近似 {g.Sum(r => r.UnforcedRingOuts)} / 無ハート減少 {g.Count(r => r.HeartsLost == 0)}");
        Console.WriteLine("CPU集計: " + Path.GetFullPath(stem + ".md")); return 0;
    }
    private static void Append(StringBuilder md, CpuMatchMetrics[] r, string difficulty, string matchup)
    {
        string rate = (r.Count(x => x.UltimateStarts > 0) * 100.0 / r.Length).ToString("F1", CultureInfo.InvariantCulture);
        string seconds = r.Average(x => x.Ticks / 60.0).ToString("F2", CultureInfo.InvariantCulture);
        string first = (r.Count(x => x.Result == MatchResult.Player1Wins) * 100.0 / r.Length).ToString("F1", CultureInfo.InvariantCulture);
        string second = (r.Count(x => x.Result == MatchResult.Player2Wins) * 100.0 / r.Length).ToString("F1", CultureInfo.InvariantCulture);
        md.AppendLine($"| {difficulty} | {matchup} | {r.Length} | {r.Count(x => x.Result == MatchResult.Player1Wins)}/{r.Count(x => x.Result == MatchResult.Player2Wins)}/{r.Count(x => x.Result == MatchResult.Draw)} ({first}%/{second}%) | {seconds} | {r.Count(x => x.Reason == MatchEndReason.Hearts)}/{r.Count(x => x.Reason == MatchEndReason.RingOut)}/{r.Count(x => x.Reason == MatchEndReason.TimeUp)} | {r.Count(x => x.ShrinkReached) * 100d / r.Length:F1}% | {rate}% | {r.Sum(x => x.UltimateStarts)}/{r.Sum(x => x.UltimateHits)} | {r.Sum(x => x.UnforcedRingOuts)} | {r.Count(x => x.Attacks.All(a => a > 0))}/{r.Count(x => x.HeartsLost > 0)} |");
    }
}
