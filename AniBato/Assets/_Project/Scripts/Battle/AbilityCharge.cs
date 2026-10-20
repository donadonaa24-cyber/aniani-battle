using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct AbilityChargeDefinition
    {
        public int MinimumFrames, MaximumFrames;
        public float MaximumRangeScale, MaximumStageWidthFraction, MaximumKnockbackScale;
        public bool WidthOnly, FormationOnly;
        public bool Enabled => MinimumFrames > 0;
        public void Validate()
        {
            if (MinimumFrames < 0 || MaximumFrames < 0 || Enabled && (MaximumFrames <= MinimumFrames ||
                !Finite(MaximumRangeScale) || MaximumRangeScale < 1 || !Finite(MaximumStageWidthFraction) ||
                MaximumStageWidthFraction < 0 || MaximumStageWidthFraction > 1 || !Finite(MaximumKnockbackScale) || MaximumKnockbackScale < 1))
                throw new ArgumentException("特殊技の溜め時間・拡大率が不正です。");
        }
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    }
    public static class AbilityChargeSystem
    {
        public static bool IsCharging(FighterState f) => f.Attack != null && f.Attack.Definition.AbilityCharge.Enabled && !f.Attack.AbilityChargeReleased;
        internal static bool Hold(FighterState f, in FrameInput input, StageGeometry stage)
        {
            if (!IsCharging(f)) return false;
            var attack = f.Attack; var definition = attack.Definition; var charge = definition.AbilityCharge;
            attack.AbilityChargeFrames++; f.Velocity = default;
            attack.Frame = Math.Min(attack.AbilityChargeFrames, definition.StartupFrames - 1);
            var button = definition.IsUltimate ? InputButtons.Ultimate : InputButtons.Special;
            if (attack.AbilityChargeFrames < charge.MinimumFrames || input.IsHeld(button) && attack.AbilityChargeFrames < charge.MaximumFrames) return true;
            float amount = Math.Max(0, Math.Min(1, (attack.AbilityChargeFrames - charge.MinimumFrames) / (float)(charge.MaximumFrames - charge.MinimumFrames)));
            float maximum = charge.MaximumStageWidthFraction > 0 && stage != null ? Math.Max(1, stage[0].Bounds.Size.X * charge.MaximumStageWidthFraction / definition.Hitbox.Size.X) : charge.MaximumRangeScale;
            float scale = 1 + (maximum - 1) * amount;
            attack.AbilityRangeScale = scale; attack.AbilityChargeReleased = true;
            if (!charge.FormationOnly)
            {
                if (!definition.ProjectileBehavior.ExplodeOnLanding)
                    definition.Hitbox = Scale(definition.Hitbox, scale, charge.WidthOnly, definition.SpawnProjectile);
                if (definition.HasSecondaryHitbox) definition.SecondaryHitbox = Scale(definition.SecondaryHitbox, scale, charge.WidthOnly, definition.SpawnProjectile);
                definition.Aftermath.Size = new BattleVector(definition.Aftermath.Size.X * scale, definition.Aftermath.Size.Y * (charge.WidthOnly ? 1 : scale));
                definition.Ability.ProjectilePullRadius *= scale;
                definition.ProjectileBehavior.ExplosionSize *= scale;
            }
            else definition.ProjectileBehavior.FormationVerticalSpread *= amount;
            definition.Knockback *= 1 + (charge.MaximumKnockbackScale - 1) * amount;
            attack.Definition = definition; attack.Frame = definition.StartupFrames - 1;
            return false;
        }
        private static BattleRect Scale(BattleRect box, float scale, bool widthOnly, bool projectile) => new BattleRect(
            // Falling projectile origins remain above the target, rather than moving farther into the sky.
            projectile ? box.Center : new BattleVector(box.Center.X * scale, box.Center.Y * (widthOnly ? 1 : scale)),
            new BattleVector(box.Size.X * scale, box.Size.Y * (widthOnly ? 1 : scale)));
    }
}
