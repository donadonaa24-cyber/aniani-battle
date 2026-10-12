using System;
using System.Globalization;
using System.IO;
using System.Text;
using AniBato.Battle;

internal static class PhaseSixReport
{
    public static int Write()
    {
        var text = new StringBuilder("## フェーズ6A：方向通常とスタミナ（ユーザー決定・2026-10-12）\n\n純C# `PhaseSixADefaults` から生成。発生/持続/硬直/着地隙は60HzのF、判定・速度はゲーム単位。全技Kind=Normal・近距離カウンター対象。地上弱3段と既存特殊・超必殺の値は保持。空中方向のXは持続速度、Yは最初の持続tickだけ設定し、その後は重力を進める。Y=0なら既存の縦速度を維持する。\n\n");
        text.AppendLine("| キャラ | 技 | 発生 | 持続 | 硬直 | 着地隙 | 判定中心 (x,y) | 判定幅×高 | 持続移動 (x,y)/秒 |\n|---|---|---:|---:|---:|---:|---|---|---|");
        for (int i = 0; i < 3; i++)
        {
            var c = PhaseSixADefaults.Character(i);
            for (int slot = 1; slot < 10; slot++)
            {
                var a = c.DirectionalNormals[slot];
                text.AppendLine(FormattableString.Invariant($"| {c.Name} | {PhaseSixADefaults.Names[slot]} | {a.StartupFrames} | {a.ActiveFrames} | {a.RecoveryFrames} | {a.LandingRecoveryFrames} | ({a.Hitbox.Center.X:0.###},{a.Hitbox.Center.Y:0.###}) | {a.Hitbox.Size.X:0.###}×{a.Hitbox.Size.Y:0.###} | ({a.ActiveVelocity.X:0.###},{a.ActiveVelocity.Y:0.###}) |"));
            }
        }
        text.AppendLine("\n| キャラ | 最大 | 地上静止回復/秒 | 行動・空中回復/秒 | 回復遅延F | 地上跳 | 空中跳 | 地上ダッシュ | 空中ダッシュ | 回避 |\n|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
        for (int i = 0; i < 3; i++)
        {
            var c = PhaseSixADefaults.Character(i); var s = c.Stamina;
            text.AppendLine($"| {c.Name} | {s.Maximum} | {s.GroundRecoveryPerSecond} | {s.ActiveRecoveryPerSecond} | {s.RecoveryDelayFrames} | {s.JumpCost} | {s.AirJumpCost} | {s.GroundDashCost} | {s.AirDashCost} | {s.DodgeCost} |");
        }
        text.AppendLine("\n全員：ガード保持6/秒、成立時 通常8 / 特殊20 / 超必殺65、ジャストガード0。ガードブレイク18F・ガード不能36F。通常/特殊の不足時はダメージなしでブレイク、超必殺の不足時は貫通しハート2。しゃがみはスタミナに関わらず崩して通常蓄積+1。地上ダッシュ保持は1区間終了後に再消費。地上ジャンプは不足分を0で打ち切って必ず実行、上復帰は消費0。透真は回復型、蓮は少量・低消費の機動型、千鶴は大容量の持久型。\n\n下長押し18Fで上足場から降下、しゃがみHurtbox高さ55%（足位置固定）。斜め判定は前方向と上下の両方が0.45超。斜め枠未登録時は縦方向へ（データで横優先へ変更可）。空中後ろ斜めは上下へ。下系は上向きノックバック2で、メテオを採用しない。\n");
        File.WriteAllText("TestResults/phase6a-balance-tables.md", text.ToString(), new UTF8Encoding(false)); return 0;
    }
}
