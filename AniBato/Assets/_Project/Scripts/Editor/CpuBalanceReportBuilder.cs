using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class CpuBalanceReportBuilder
    {
        [MenuItem("あにバト/CPU対戦集計（生成済みデータ810試合）")]
        public static void Write()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("再生を停止してください。");
            const string data = "Assets/_Project/Data/";
            var setup = AssetDatabase.LoadAssetAtPath<BattleSetup>(data + "BattleSetup.asset");
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>(data + "BattleRules.asset");
            if (!setup || !rules || setup.CpuDifficulties == null || setup.CpuDifficulties.Length != 3)
                throw new InvalidOperationException("先にBuildAllを実行してください。");
            var rows = CpuBalanceStudy.Run(setup.Characters.Select(c => c.ToDefinition()).ToArray(), rules.ToDefinition(),
                setup.Stages[0].ToGeometry(), setup.CpuDifficulties.Select(c => c.ToDefinition()).ToArray());
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string stem = Path.Combine(root, "TestResults/phase6b-cpu-unity"); Directory.CreateDirectory(Path.GetDirectoryName(stem));
            File.WriteAllText(stem + ".csv", CpuBalanceStudy.Csv(rows), new UTF8Encoding(false));
            var report = new StringBuilder("# CPU集計（Unityで生成済みのScriptableObject使用）\n\n各難易度・全9組×30seed。70秒、初期ゲージ0。時間=tick/60。自滅近似=場外敗者が直近120tickに被弾/カウンターを受けていない。\n\n");
            var gauge = rules.Definition.Gauge;
            report.AppendLine($"ゲージ: 命中{gauge.OnHit} / 被弾{gauge.OnDamaged} / カウンター{gauge.OnCounter} / ジャスト{gauge.OnJustGuard}\n");
            report.AppendLine("| 難易度 | 組合せ | P1勝率/P2勝率/分 | 平均秒 | KO/場外/時間 | 縮小到達 | 超必殺試合率 | 発動/命中 | 自滅近似 | 両者攻撃/ハート減少 |\n|---|---|---|---:|---|---:|---:|---|---:|---|");
            foreach (var d in rows.GroupBy(r => r.Difficulty))
            {
                Append(report, d.ToArray(), d.Key, "全組");
                foreach (var pair in d.GroupBy(r => new { r.Player1, r.Player2 }))
                    Append(report, pair.ToArray(), d.Key, setup.Characters[pair.Key.Player1].Movement.Name + "-" + setup.Characters[pair.Key.Player2].Movement.Name);
            }
            File.WriteAllText(stem + ".md", report.ToString(), new UTF8Encoding(false)); Debug.Log("CPU report: " + stem);
        }
        private static string Number(double x) => x.ToString("F1", CultureInfo.InvariantCulture);
        private static void Append(StringBuilder report, CpuMatchMetrics[] rows, int difficulty, string matchup)
        {
            int n = rows.Length;
            report.AppendLine($"| {(CpuDifficulty)difficulty} | {matchup} | {Number(rows.Count(r => r.Result == MatchResult.Player1Wins) * 100.0 / n)}%/{Number(rows.Count(r => r.Result == MatchResult.Player2Wins) * 100.0 / n)}%/{rows.Count(r => r.Result == MatchResult.Draw)} | {Number(rows.Average(r => r.Ticks / 60.0))} | {rows.Count(r => r.Reason == MatchEndReason.Hearts)}/{rows.Count(r => r.Reason == MatchEndReason.RingOut)}/{rows.Count(r => r.Reason == MatchEndReason.TimeUp)} | {Number(rows.Count(r => r.ShrinkReached) * 100.0 / n)}% | {Number(rows.Count(r => r.UltimateStarts > 0) * 100.0 / n)}% | {rows.Sum(r => r.UltimateStarts)}/{rows.Sum(r => r.UltimateHits)} | {rows.Sum(r => r.UnforcedRingOuts)} | {rows.Count(r => r.Attacks.All(a => a > 0))}/{rows.Count(r => r.HeartsLost > 0)} |");
        }
    }
}
