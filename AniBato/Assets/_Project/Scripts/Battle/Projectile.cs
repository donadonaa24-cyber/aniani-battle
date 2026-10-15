using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class Projectile
    {
        public FighterState Owner { get; }
        public AttackInstance Attack { get; }
        public BattleVector Position { get; private set; }
        public int Facing { get; }
        public int RemainingFrames { get; private set; }
        public BattleVector Velocity { get; private set; }
        public BattleRect Hitbox => new BattleRect(Position, Attack.Definition.Hitbox.Size);
        internal Projectile(FighterState owner, AttackInstance attack, FighterState target = null, StageGeometry stage = null, float offsetX = 0)
        {
            Owner = owner; Attack = attack;
            Facing = attack.Definition.NormalDirection == NormalDirection.Back || attack.Definition.NormalDirection == NormalDirection.AirBack ? -owner.Facing : owner.Facing;
            Position = attack.WorldHitbox(owner).Center;
            if (attack.Definition.Ability.ShotOrigin == ShotOrigin.OpponentAbove && target != null)
            {
                float x = target.Position.X;
                float range = attack.Definition.Ability.TargetRange;
                if (range > 0) x = System.Math.Max(owner.Position.X - range, System.Math.Min(owner.Position.X + range, x));
                Position = new BattleVector(x + attack.Definition.Hitbox.Center.X * owner.Facing, target.Position.Y + attack.Definition.Hitbox.Center.Y);
            }
            else if (attack.Definition.Ability.ShotOrigin == ShotOrigin.StageCenter && stage != null)
                Position = new BattleVector(stage[0].Bounds.Center.X + attack.Definition.Hitbox.Center.X, stage[0].Bounds.Top + attack.Definition.Hitbox.Center.Y);
            Position += new BattleVector(offsetX, 0);
            RemainingFrames = attack.Definition.ProjectileLifetimeFrames;
            Velocity = new BattleVector(Facing * attack.Definition.ProjectileSpeed + Facing * attack.Definition.ProjectileLaunchVelocity.X,
                attack.Definition.ProjectileLaunchVelocity.Y);
        }
        internal void Step()
        {
            Velocity = new BattleVector(Velocity.X, Velocity.Y - Attack.Definition.ProjectileGravity * BattleRuleDefinition.TickSeconds);
            Position += Velocity * BattleRuleDefinition.TickSeconds;
            RemainingFrames--;
        }
    }

    public sealed class ProjectileSystem
    {
        private readonly List<Projectile> projectiles = new List<Projectile>();
        public IReadOnlyList<Projectile> Projectiles { get; }
        public ProjectileSystem() => Projectiles = projectiles.AsReadOnly();
        public void RemoveClashed() => projectiles.RemoveAll(p => p.Attack.Clashed);
        public void Step(FighterState p1, FighterState p2, int maximumPerOwner = 0, StageGeometry stage = null)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Step();
                if (projectiles[i].RemainingFrames <= 0 || projectiles[i].Attack.Clashed || projectiles[i].Owner.Mode == FighterMode.KO) projectiles.RemoveAt(i);
            }
            Spawn(p1, p2, maximumPerOwner, stage); Spawn(p2, p1, maximumPerOwner, stage);
            Erase(p1); Erase(p2);
        }
        private void Erase(FighterState fighter)
        {
            var shield = fighter.Attack;
            if (fighter.FrozenThisTick || shield == null || !shield.IsActive || shield.ErasuresRemaining == 0) return;
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                if (projectile.Owner == fighter || !shield.Overlaps(fighter, projectile.Hitbox)) continue;
                projectiles.RemoveAt(i);
                if (shield.ErasuresRemaining > 0 && --shield.ErasuresRemaining == 0) break;
            }
        }
        private void Spawn(FighterState owner, FighterState target, int maximum, StageGeometry stage)
        {
            var attack = owner.Attack;
            if (owner.FrozenThisTick || attack == null || !attack.IsActive || !attack.Definition.SpawnProjectile || attack.ProjectileSpawned ||
                attack.Definition.Ability.ProjectileAtDamageFrame && !attack.IsDamageActive) return;
            attack.ProjectileSpawned = true;
            bool normalSlot = attack.Definition.Ability.IndependentNormalProjectile;
            if (attack.Definition.Ability.ReplaceOwnProjectiles)
                projectiles.RemoveAll(p => p.Owner == owner && (!normalSlot || p.Attack.Definition.Ability.IndependentNormalProjectile));
            // A formation is one attack for the owner's limit and shared hit history.
            var instances = new HashSet<AttackInstance>();
            foreach (var shot in projectiles)
                if (shot.Owner == owner && shot.Attack.Definition.Ability.IndependentNormalProjectile == normalSlot) instances.Add(shot.Attack);
            if (maximum > 0 && instances.Count >= maximum) return;
            int requested = System.Math.Max(1, attack.Definition.Ability.ProjectileCount);
            for (int i = 0; i < requested; i++)
            {
                float offset = (i - (requested - 1) * .5f) * attack.Definition.Ability.ProjectileSpacing;
                projectiles.Add(new Projectile(owner, attack, target, stage, offset));
            }
        }
    }
}
