using System;
using AniBato.Battle;

namespace AniBato.Presentation
{
    // Presentation clock only: anticipation starts when the teleport attack is accepted.
    // The destination never feeds the anticipation curve, including on the teleport tick.
    public sealed class TeleportCameraCue
    {
        private CameraFrame start;
        private float elapsed, remaining;
        public bool Active { get; private set; }
        public void Reset() { Active = false; elapsed = remaining = 0; }
        public CameraFrame Advance(bool requested, float delta, CameraFrame current, CameraFrame overview, float seconds, float hold)
        {
            if (requested && !Active) { Active = true; start = current; elapsed = 0; }
            if (!Active) return current;
            delta = Math.Max(0, delta);
            elapsed += delta;
            if (requested) remaining = hold; else remaining -= delta;
            float t = Math.Min(1, elapsed / Math.Max(.01f, seconds));
            t = t * t * (3 - 2 * t);
            var result = new CameraFrame(new BattleVector(start.Center.X + (overview.Center.X - start.Center.X) * t,
                start.Center.Y + (overview.Center.Y - start.Center.Y) * t), start.Size + (Math.Max(start.Size, overview.Size) - start.Size) * t, overview.Required);
            if (!requested && remaining <= 0 && elapsed >= seconds) Active = false;
            return result;
        }
    }
}
