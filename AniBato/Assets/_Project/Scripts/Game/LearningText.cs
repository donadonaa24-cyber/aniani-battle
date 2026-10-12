using AniBato.Battle;
using AniBato.Presentation;

namespace AniBato.Game
{
    public static class LearningText
    {
        public static readonly string[] Actions = { "左", "右", "上", "下", "ジャンプ", "通常攻撃", "特殊攻撃", "ガード", "回避", "カウンター", "超必殺", "ダッシュ", "ポーズ" };
        public static string DashSetting(InputController input) => "左右2回入力のダッシュ  " + (input.Configuration.DoubleTapDashEnabled ? "ON" : "OFF");
        public static string DashHelp(InputController input) => (input.Configuration.DoubleTapDashEnabled ? "左右2回、または " : "") + input.Label(0, ControlAction.Dash) + " でダッシュ（2回入力 " + (input.Configuration.DoubleTapDashEnabled ? "ON" : "OFF") + "）。";
        public static string Tutorial(TutorialStep step, InputController input)
        {
            string j = input.Label(0, ControlAction.Jump), a = input.Label(0, ControlAction.Attack), s = input.Label(0, ControlAction.Special),
                g = input.Label(0, ControlAction.Guard), d = input.Label(0, ControlAction.Dodge);
            switch (step)
            {
                case TutorialStep.Move: return "1 移動：" + input.Label(0, ControlAction.Left) + " / " + input.Label(0, ControlAction.Right) + " で1歩進もう。パッドは左スティックも使える。";
                case TutorialStep.Jump: return "2 ジャンプ：" + j + "。地上ジャンプはスタミナ切れでも使える。";
                case TutorialStep.DoubleJump: return "3 2段ジャンプ：空中でもう一度 " + j + "。";
                case TutorialStep.NormalCombo: return "4 通常3段：" + a + " を3回押そう。押し続けるだけでは連打にならない。";
                case TutorialStep.Directional: return "5 方向攻撃：" + input.Label(0, ControlAction.Down) + "＋" + a + "、背後＋" + a + "、空中で " + a + "。前後はキャラの向き基準、空中は向き固定。";
                case TutorialStep.Specials: return "6 特殊4種：方向なし / 横 / " + input.Label(0, ControlAction.Up) + " / " + input.Label(0, ControlAction.Down) + " ＋ " + s + "。蓮の下はもう一度攻撃で急襲。";
                case TutorialStep.Guard: return "7 ガード：" + g + " を保持して防ごう。全種類を防ぐがスタミナ消費。超必殺は不足すると貫通。";
                case TutorialStep.GuardBreak: return "8 ガード崩し：近づいて " + input.Label(0, ControlAction.Down) + "＋" + a + "。ガードを崩し、被弾カウント＋1。";
                case TutorialStep.Dodge: return "9 回避：攻撃が来たら " + d + "。スタミナが必要。超必殺にはジャストのみ有効。";
                case TutorialStep.Counter: return "10 カウンター：近距離の通常に合わせ " + input.Label(0, ControlAction.Counter) + "。特殊には負ける。";
                case TutorialStep.JustDefense: return "11 ジャスト防御：命中直前の5Fに " + g + " または " + d + "。ジャストガード消費0。";
                case TutorialStep.Ultimate: return "12 超必殺：" + input.Label(0, ControlAction.Ultimate) + "。ここではLINKを補充。ハート−2、1試合1回。";
                case TutorialStep.Stamina: return "13 スタミナ：" + DashHelp(input) + " ジャンプでも消費。着地して操作を止め回復しよう。";
                case TutorialStep.Rules: return "14 ルール：ハート3 / 被弾3で−1 / 場外は即敗北 / 70秒の最後10秒で縮小。確認したら " + a + "。";
                default: return "チュートリアル完了！ 練習を続けるか、ポーズからキャラ選択へ。";
            }
        }
        public static string Commands(CharacterData data, InputController input, int page)
        {
            string a = input.Label(0, ControlAction.Attack), s = input.Label(0, ControlAction.Special);
            if (page == 0) return "地上弱3段    " + a + " ×3（押下）    最大3ヒット\n" +
                "足払い    ↓＋" + a + "    ガード崩し・被弾＋1\n背面打ち    背後＋" + a + "    向き基準の後ろへ\n" +
                "空中ニュートラル    方向なし＋" + a + "    周囲を打つ\n空中前 / 後ろ    前 / 背後＋" + a + "    前進 / 向き固定\n" +
                "空中上 / 下    ↑ / ↓＋" + a + "    下は着地隙18F\n空中斜め前上 / 前下    前＋↑ / ↓＋" + a + "    前進攻撃\n" +
                "全通常：被弾＋1、累計3でハート−1。しゃがみ以外はガード可。";
            string[] directions = { "方向なし", "横", "上", "下" }; string text = "";
            for (int i = 0; i < 4; i++) text += data.Specials[i].Definition.Name + "    " + directions[i] + "＋" + s + "    " +
                (i == 2 ? "復帰（スタミナ不要）" : data.Specials[i].Definition.NonDamaging ? "蓄積1回復" : data.Specials[i].Definition.RequiresFollowup ? "攻撃で急襲" : "ハート−1、ガード可") + "\n";
            text += data.Ultimate.Definition.Name + "    " + input.Label(0, ControlAction.Ultimate) + "    LINK100、ハート−2、1回\n";
            text += "カウンター    " + input.Label(0, ControlAction.Counter) + "    近距離通常を返す＋" + (data.Silhouette == 0 ? "移動強化" : data.Silhouette == 1 ? "吹っ飛ばし強化" : "蓄積1回復") + "\n";
            return text + "ガード " + input.Label(0, ControlAction.Guard) + " / 回避 " + input.Label(0, ControlAction.Dodge) + " / " + DashHelp(input);
        }
        public static string InputDisplay(in FrameInput frame, InputController input)
        {
            string text = (frame.X < -.3f ? "←" : frame.X > .3f ? "→" : "") + (frame.Y < -.3f ? "↓" : frame.Y > .3f ? "↑" : "");
            for (int i = 0; i < 8; i++) if (((int)(frame.Held | frame.Pressed) & 1 << i) != 0) text += " " + input.Label(0, (ControlAction)(i + 4));
            return string.IsNullOrEmpty(text) ? "入力なし" : text;
        }
    }
}
