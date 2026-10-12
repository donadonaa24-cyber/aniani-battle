using System;
using AniBato.Battle;

namespace AniBato.Presentation
{
    public readonly struct CameraFrame
    {
        public readonly BattleVector Center;
        public readonly float Size;
        public readonly BattleRect Required;
        public CameraFrame(BattleVector center, float size, BattleRect required)
        { Center = center; Size = size; Required = required; }
    }

    // Display-only framing. Never changes world coordinates, hurtboxes or simulation state.
    public static class CameraFraming
    {
        public static CameraFrame Calculate(StageGeometry stage, BattleVector p1, BattleVector p2,
            float aspect, bool overview, float minimum, float padding, float boundaryInset, float headroom)
        {
            if (aspect <= 0) throw new ArgumentOutOfRangeException(nameof(aspect));
            float left = Math.Min(p1.X, p2.X) - padding, right = Math.Max(p1.X, p2.X) + padding;
            float bottom = Math.Min(-1.8f, Math.Min(p1.Y, p2.Y) - padding);
            float top = Math.Max(p1.Y, p2.Y) + headroom;
            if (overview)
            { left = Math.Min(left, stage.LeftBlastLine + boundaryInset); right = Math.Max(right, stage.RightBlastLine - boundaryInset); }
            // Upper platforms remain visible in combat. After they disappear, the smaller
            // central floor defines the frame, so zoom naturally follows the shrink.
            bool upper = false;
            for (int i = 1; i < stage.PlatformCount; i++) if (stage.IsActive(i))
            {
                upper = true;
                left = Math.Min(left, stage[i].Bounds.Left - padding);
                right = Math.Max(right, stage[i].Bounds.Right + padding);
                top = Math.Max(top, stage[i].Bounds.Top + headroom);
            }
            if (!upper)
            { left = Math.Min(left, stage[0].Bounds.Left - padding); right = Math.Max(right, stage[0].Bounds.Right + padding); }
            var center = new BattleVector((left + right) * .5f, (bottom + top) * .5f);
            var required = new BattleRect(center, new BattleVector(right - left, top - bottom));
            float size = Math.Max(minimum, Math.Max(required.Size.Y * .5f, required.Size.X * .5f / aspect));
            return new CameraFrame(center, size, required);
        }
        // While following, expanding the view is immediate. A slowly expanding camera
        // would crop newly entering platforms/fighters during zoom-out or aspect changes.
        public static float ContainingSize(CameraFrame frame, BattleVector actualCenter, float aspect, float desired)
        {
            float x = Math.Max(Math.Abs(frame.Required.Left - actualCenter.X), Math.Abs(frame.Required.Right - actualCenter.X));
            float y = Math.Max(Math.Abs(frame.Required.Bottom - actualCenter.Y), Math.Abs(frame.Required.Top - actualCenter.Y));
            return Math.Max(desired, Math.Max(x / aspect, y));
        }
    }
}
