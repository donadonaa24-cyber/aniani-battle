using System;

namespace AniBato.Battle
{
    /// <summary>Retains all elapsed time; never discards ticks because a render frame is slow.</summary>
    public sealed class FixedTickClock
    {
        public const double StepSeconds = 1d / BattleRuleDefinition.TickRate;
        private double accumulator;
        public double Fraction => Math.Min(1, accumulator / StepSeconds);
        public double PendingSeconds => accumulator;
        public void Reset() => accumulator = 0;
        public int Advance(double elapsedSeconds, Action tick, int maximumTicks = int.MaxValue)
        {
            if (maximumTicks < 1) throw new ArgumentOutOfRangeException(nameof(maximumTicks));
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            accumulator += elapsedSeconds;
            int count = 0;
            while (count < maximumTicks && accumulator + 1e-10 >= StepSeconds)
            { accumulator -= StepSeconds; tick(); count++; }
            if (accumulator < 0) accumulator = 0;
            return count;
        }
    }
}
