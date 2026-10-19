using System;

namespace AniBato.Battle
{
    public enum AbilityMotion { None, Teleport, TwoPhaseRush }
    public enum ShotOrigin { Owner, OpponentAbove, StageCenter }
    public enum AbilityLook { Default, Bullet, Drone, Rocket, BlackHole, Emp, Alma, Rook, Bishop, King, Card, Pyramid, Spell, Curry, Teleport, RiceBall, Bat }
    [Serializable]
    public struct AttackAbility
    {
        public AbilityMotion Motion;
        public int ChangeFrame;
        public BattleVector FirstVelocity, SecondVelocity;
        public float TeleportRange, TargetGap, TargetRange;
        public ShotOrigin ShotOrigin;
        public int ProjectileCount;
        public float ProjectileSpacing;
        public float ProjectilePullRadius, ProjectilePullSpeed;
        public bool AllowZeroProjectileSpeed, ReplaceOwnProjectiles, ProjectileAtDamageFrame;
        public bool IndependentNormalProjectile;
        public AbilityLook Look;
        public void Validate()
        {
            if (ProjectileCount < 0 || ProjectileCount > 16 || ProjectileSpacing < 0 || !Finite(ProjectileSpacing) ||
                ProjectilePullRadius < 0 || ProjectilePullSpeed < 0 || !Finite(ProjectilePullRadius) || !Finite(ProjectilePullSpeed) ||
                (ProjectilePullRadius > 0) != (ProjectilePullSpeed > 0) ||
                TeleportRange < 0 || TargetGap < 0 || TargetRange < 0 || !Finite(TeleportRange) || !Finite(TargetGap) || !Finite(TargetRange) ||
                !Finite(FirstVelocity.X) || !Finite(FirstVelocity.Y) || !Finite(SecondVelocity.X) || !Finite(SecondVelocity.Y) ||
                Motion == AbilityMotion.Teleport && TeleportRange <= 0 || Motion == AbilityMotion.TwoPhaseRush && ChangeFrame < 1)
                throw new ArgumentException("固有技の移動・投射定義が不正です。");
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public static class AbilityMotionSystem
    {
        public static void Step(FighterState self, FighterState target, StageGeometry stage)
        {
            var attack = self.Attack;
            if (attack == null || !attack.IsActive || self.IsFrozenThisTick) return;
            var ability = attack.Definition.Ability;
            if (ability.Motion == AbilityMotion.Teleport && !attack.AbilityApplied)
            {
                attack.AbilityApplied = true;
                var floor = stage[0].Bounds; float half = self.Definition.HurtboxSize.X * .5f;
                float desiredX = Math.Max(floor.Left + half, Math.Min(floor.Right - half, target.Position.X - self.Facing * ability.TargetGap));
                var delta = new BattleVector(desiredX - self.Position.X, target.Position.Y - self.Position.Y);
                float length = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                if (length > ability.TeleportRange) delta *= ability.TeleportRange / length;
                var destination = self.Position + delta;
                if (stage.IsOutside(destination)) return;
                var body = new BattleRect(destination, self.Definition.HurtboxSize);
                for (int i = 0; i < stage.PlatformCount; i++)
                    if (stage.IsActive(i) && !stage[i].OneWay && body.Overlaps(stage[i].Bounds)) return;
                self.Position = destination;
                self.Velocity = default; self.Grounded = false; self.GroundPlatform = -1;
            }
            if (ability.Motion != AbilityMotion.TwoPhaseRush) return;
            int age = attack.Frame - attack.Definition.StartupFrames;
            var velocity = age < ability.ChangeFrame ? ability.FirstVelocity : ability.SecondVelocity;
            float x = velocity.X * self.Facing, y = velocity.Y == 0 ? self.Velocity.Y : velocity.Y;
            var bounds = stage[0].Bounds; float inset = self.Definition.HurtboxSize.X * .5f;
            // A previous clamp can land a float fraction beyond the exact inset.
            if (self.Position.X >= bounds.Left + inset - .001f && self.Position.X <= bounds.Right - inset + .001f)
            {
                float next = self.Position.X + x * BattleRuleDefinition.TickSeconds;
                float limited = Math.Max(bounds.Left + inset, Math.Min(bounds.Right - inset, next));
                x = (limited - self.Position.X) / BattleRuleDefinition.TickSeconds;
            }
            self.Velocity = new BattleVector(x, y);
            if (y > 0) { self.Grounded = false; self.GroundPlatform = -1; self.FastFalling = false; }
        }
    }
}
