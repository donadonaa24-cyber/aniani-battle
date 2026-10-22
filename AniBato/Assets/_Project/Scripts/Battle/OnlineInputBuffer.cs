using System;
using System.Collections.Generic;
using System.Globalization;

namespace AniBato.Battle
{
    // Ordered, reliable transport. Missing input stops simulation; it never predicts an action.
    public sealed class OnlineInputBuffer
    {
        public const int Delay = 6, MaximumAhead = 180;
        private readonly Dictionary<int, FrameInput>[] frames = { new Dictionary<int, FrameInput>(), new Dictionary<int, FrameInput>() };
        public int NextFrame { get; private set; }
        public int LocalPlayer { get; }
        public int LocalTarget => NextFrame + Delay;
        public bool LocalQueued => frames[LocalPlayer].ContainsKey(LocalTarget);
        public OnlineInputBuffer(int localPlayer)
        {
            if (localPlayer < 0 || localPlayer > 1) throw new ArgumentOutOfRangeException(nameof(localPlayer));
            LocalPlayer = localPlayer;
            for (int i = 0; i < Delay; i++) { frames[0][i] = default; frames[1][i] = default; }
        }
        public void Add(int player, int frame, in FrameInput input)
        {
            if (player < 0 || player > 1 || frame < 0 || frame > NextFrame + MaximumAhead) throw new FormatException("入力フレーム範囲が不正です");
            if (frame < NextFrame) return;
            if (frames[player].ContainsKey(frame)) throw new FormatException("入力が重複しています");
            frames[player].Add(frame, input);
        }
        public bool TryTake(out FrameInput first, out FrameInput second)
        {
            first = second = default;
            if (!frames[0].TryGetValue(NextFrame, out first) || !frames[1].TryGetValue(NextFrame, out second)) return false;
            frames[0].Remove(NextFrame); frames[1].Remove(NextFrame); NextFrame++; return true;
        }
    }
    public static class OnlineInputCodec
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        public static string Encode(int frame, in FrameInput input)
        {
            var values = new List<string> { frame.ToString(Invariant), ((int)input.Held).ToString(Invariant), ((int)input.Pressed).ToString(Invariant), input.DisableDoubleTapDash ? "1" : "0", F(input.X), F(input.Y) };
            for (int i = 0; i < 8; i++) { var d = input.DirectionFor((InputButtons)(1 << i)); values.Add(F(d.X)); values.Add(F(d.Y)); }
            return string.Join(",", values);
        }
        private static string F(float value) => value.ToString("R", Invariant);
        public static FrameInput Decode(string text, out int frame)
        {
            if (text == null || text.Length > 600) throw new FormatException("入力が長すぎます");
            var v = text.Split(',');
            if (v.Length != 22 || !int.TryParse(v[0], NumberStyles.None, Invariant, out frame) || frame < 0 || frame > 100000) throw new FormatException("入力形式が不正です");
            if (!int.TryParse(v[1], out int held) || held < 0 || held > 255 || !int.TryParse(v[2], out int pressed) || pressed < 0 || pressed > 255 || v[3] != "0" && v[3] != "1") throw new FormatException("ボタン形式が不正です");
            var dirs = new BattleVector[8];
            for (int i = 0; i < 8; i++) dirs[i] = new BattleVector(Axis(v[6 + i * 2]), Axis(v[7 + i * 2]));
            return new FrameInput(Axis(v[4]), Axis(v[5]), (InputButtons)held, (InputButtons)pressed, dirs, v[3] == "1");
        }
        private static float Axis(string value)
        {
            if (!float.TryParse(value, NumberStyles.Float, Invariant, out float n) || float.IsNaN(n) || float.IsInfinity(n) || n < -1 || n > 1) throw new FormatException("方向入力が不正です");
            return n;
        }
    }
}
