using System;

namespace AniBato.Presentation
{
    public static class PaperTravelLean
    {
        // Directional lean has no periodic wobble or bob. Rendering cannot advance it.
        public static PaperRockingSample Evaluate(float degrees, bool flipX, float footMinX, float footMaxX)
        {
            if (float.IsNaN(degrees) || float.IsInfinity(degrees) || degrees < 0 || degrees > 60 ||
                float.IsNaN(footMinX) || float.IsInfinity(footMinX) || float.IsNaN(footMaxX) || float.IsInfinity(footMaxX) || footMinX > footMaxX)
                throw new ArgumentException("紙人形の移動傾きまたは足幅が不正です。");
            float angle = degrees * (flipX ? 1 : -1);
            double sin = Math.Sin(angle * Math.PI / 180);
            float min = flipX ? -footMaxX : footMinX, max = flipX ? -footMinX : footMaxX;
            return new PaperRockingSample(angle, (float)Math.Max(0, -Math.Min(min * sin, max * sin)), 0);
        }
    }
}
