using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public static class ProjectileAttractionSystem
    {
        public static BattleVector Velocity(FighterState target, IReadOnlyList<Projectile> projectiles)
        {
            if (target.Mode == FighterMode.KO || target.IsFrozenThisTick || target.HeartRecoveryRemaining > 0) return default;
            var velocity = default(BattleVector);
            foreach (var field in projectiles)
            {
                if (field.Owner == target || field.Owner.Mode == FighterMode.KO || field.RemainingFrames <= 1 ||
                    field.Attack.Clashed || field.Attack.HasHit(target.PlayerIndex) || target.ImmunityFrom(field.Owner.PlayerIndex) > 0) continue;
                var ability = field.Attack.Definition.Ability;
                if (ability.ProjectilePullRadius <= 0 || ability.ProjectilePullSpeed <= 0) continue;
                var center = target.Hurtbox.Center;
                var delta = new BattleVector(field.Position.X - center.X, field.Position.Y - center.Y);
                float squaredDistance = delta.X * delta.X + delta.Y * delta.Y;
                if (squaredDistance <= 0 || squaredDistance > ability.ProjectilePullRadius * ability.ProjectilePullRadius) continue;
                float distance = (float)Math.Sqrt(squaredDistance);
                float speed = Math.Min(ability.ProjectilePullSpeed, distance / BattleRuleDefinition.TickSeconds);
                velocity += delta * (speed / distance);
            }
            return velocity;
        }
    }
}
