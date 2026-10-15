using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct ChargeAttackDefinition
    {
        public bool Enabled;
        public int MinimumFrames, MaximumFrames, HitstunFrames;
        public float MinimumTravelFraction, VerticalSpeed, EdgeInset;
        public static ChargeAttackDefinition Default => new ChargeAttackDefinition
        { Enabled = true, MinimumFrames = 60, MaximumFrames = 180, HitstunFrames = 30, MinimumTravelFraction = .3f, VerticalSpeed = 4, EdgeInset = .05f };
        public void Validate()
        {
            if (Enabled && (MinimumFrames < 1 || MaximumFrames <= MinimumFrames || HitstunFrames < 1 ||
                MinimumTravelFraction <= 0 || MinimumTravelFraction > 1 || VerticalSpeed < 0 || EdgeInset < 0))
                throw new ArgumentException("溜め攻撃の定義が不正です。");
        }
        public float Strength(int frames) => !Enabled ? 0 : Math.Max(0, Math.Min(1, (frames - MinimumFrames) / (float)(MaximumFrames - MinimumFrames)));
    }

    public static class ChargedAttack
    {
        public static BattleVector Knockback(FighterState target, int direction, int frames, StageGeometry stage, in ChargeAttackDefinition settings)
        {
            BattleRect floor = stage[0].Bounds;
            for (int i = 0; i < stage.PlatformCount; i++)
                if (stage.IsActive(i) && !stage[i].OneWay) { floor = stage[i].Bounds; break; }
            float inset = target.Definition.HurtboxSize.X * .5f + settings.EdgeInset;
            float edge = direction > 0 ? floor.Right - inset : floor.Left + inset;
            float distance = Math.Max(0, (edge - target.Position.X) * direction);
            float fraction = settings.MinimumTravelFraction + (1 - settings.MinimumTravelFraction) * settings.Strength(frames);
            return new BattleVector(distance * fraction * BattleRuleDefinition.TickRate / settings.HitstunFrames, settings.VerticalSpeed);
        }
    }
}
