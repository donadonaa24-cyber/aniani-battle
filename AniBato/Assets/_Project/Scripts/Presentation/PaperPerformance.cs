using System;

namespace AniBato.Presentation
{
    [Serializable]
    public struct PaperPerformanceSettings
    {
        public int StepTicks, WinStepTicks;
        public float AnticipationDegrees, StrikeDegrees, RecoilDegrees, SpinDegrees, WinDegrees;
        public float PullDistance, StrikeDistance, RecoilDistance, WinLift;
        public static PaperPerformanceSettings Default => new PaperPerformanceSettings {
            StepTicks = 3, WinStepTicks = 12, AnticipationDegrees = 8, StrikeDegrees = 15,
            RecoilDegrees = 14, SpinDegrees = 45, WinDegrees = 3,
            PullDistance = .06f, StrikeDistance = .10f, RecoilDistance = .08f, WinLift = .025f };
    }
    public readonly struct PaperPerformanceSample
    {
        public readonly float Degrees, X, Y;
        public readonly bool RotateAboutCenter;
        public PaperPerformanceSample(float degrees, float x, float y = 0, bool center = false)
        { Degrees = degrees; X = x; Y = y; RotateAboutCenter = center; }
    }
    public static class PaperPerformance
    {
        // Fixed-tick key poses. Repeated rendering never advances motion or changes battle state.
        public static PaperPerformanceSample Evaluate(in PaperPerformanceSettings settings, in AnimationSample sample, bool heavy = false)
        {
            int step = Math.Max(1, settings.StepTicks), age = Math.Max(0, sample.Age);
            float direction = sample.FlipX ? -1 : 1;
            if (sample.State == AnimationState.Knockback)
                return new PaperPerformanceSample(direction * (age / step) * settings.SpinDegrees, 0, center: true);
            if (sample.State == AnimationState.Win)
            {
                int phase = age / Math.Max(1, settings.WinStepTicks) % 4;
                return new PaperPerformanceSample((phase == 1 ? 1 : phase == 3 ? -1 : 0) * settings.WinDegrees, 0,
                    phase % 2 == 1 ? settings.WinLift : 0);
            }
            if (sample.State == AnimationState.Hitstun)
            {
                float strength = (heavy ? 1.5f : 1) * Math.Max(0, 1 - age / step * .25f);
                return new PaperPerformanceSample(direction * settings.RecoilDegrees * strength, -direction * settings.RecoilDistance * strength);
            }
            if (!sample.IsAttack) return default;
            if (sample.AttackFrame < sample.Startup)
                return new PaperPerformanceSample(direction * settings.AnticipationDegrees, -direction * settings.PullDistance);
            if (sample.AttackFrame < sample.Startup + sample.Active)
                return new PaperPerformanceSample(-direction * settings.StrikeDegrees, direction * settings.StrikeDistance);
            int recoveryStep = Math.Max(0, sample.AttackFrame - sample.Startup - sample.Active) / step;
            float recoil = recoveryStep == 0 ? .4f : 0;
            return new PaperPerformanceSample(direction * settings.AnticipationDegrees * recoil, -direction * settings.PullDistance * recoil);
        }
    }
}
