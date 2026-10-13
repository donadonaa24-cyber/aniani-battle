using System;

namespace AniBato.Presentation
{
    [Serializable]
    public struct PaperRockingSettings
    {
        public float IdleDegrees, WalkDegrees, WalkBob, FootMinX, FootMaxX;
        public int IdleFps, WalkFps;
        public void Validate()
        {
            if (!Finite(IdleDegrees) || !Finite(WalkDegrees) || !Finite(WalkBob) || !Finite(FootMinX) || !Finite(FootMaxX) ||
                IdleDegrees < 0 || IdleDegrees > 15 || WalkDegrees < 0 || WalkDegrees > 15 || WalkBob < 0 ||
                FootMinX > FootMaxX || IdleFps < 1 || IdleFps > 60 || WalkFps < 1 || WalkFps > 60)
                throw new ArgumentException("紙人形の傾き・更新速度・足元が不正です。");
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
    public readonly struct PaperRockingSample
    {
        public readonly float Degrees, Lift;
        public readonly int Phase;
        public PaperRockingSample(float degrees, float lift, int phase) { Degrees = degrees; Lift = lift; Phase = phase; }
    }
    public static class PaperRockingMotion
    {
        private static readonly float[] Walk = { -1, -.4f, 0, .4f, 1, .4f, 0, -.4f };
        private static readonly float[] Bob = { 0, .5f, 1, .5f, 0, .5f, 1, .5f };
        private static readonly float[] Idle = { 0, 1, 0, -1 };
        public static PaperRockingSample Evaluate(in PaperRockingSettings settings, int age, bool moving, bool flipX)
        {
            settings.Validate();
            int fps = moving ? settings.WalkFps : settings.IdleFps;
            int phase = (int)((long)Math.Max(0, age) * fps / 60 % (moving ? Walk.Length : Idle.Length));
            float angle = (moving ? Walk[phase] * settings.WalkDegrees : Idle[phase] * settings.IdleDegrees) * (flipX ? -1 : 1);
            float minX = flipX ? -settings.FootMaxX : settings.FootMinX;
            float maxX = flipX ? -settings.FootMinX : settings.FootMaxX;
            double sin = Math.Sin(angle * Math.PI / 180);
            float lift = (float)Math.Max(0, -Math.Min(sin * minX, sin * maxX)) + (moving ? Bob[phase] * settings.WalkBob : 0);
            return new PaperRockingSample(angle, lift, phase);
        }
    }
}
