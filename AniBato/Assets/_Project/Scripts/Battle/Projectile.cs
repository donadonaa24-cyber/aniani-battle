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
        internal Projectile(FighterState owner, AttackInstance attack)
        {
            Owner = owner; Attack = attack; Facing = owner.Facing;
            Position = attack.WorldHitbox(owner).Center;
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
        public void Step(FighterState p1, FighterState p2, int maximumPerOwner = 0)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Step();
                if (projectiles[i].RemainingFrames <= 0 || projectiles[i].Owner.Mode == FighterMode.KO) projectiles.RemoveAt(i);
            }
            Spawn(p1, maximumPerOwner); Spawn(p2, maximumPerOwner);
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
        private void Spawn(FighterState owner, int maximum)
        {
            var attack = owner.Attack;
            if (owner.FrozenThisTick || attack == null || !attack.IsActive || !attack.Definition.SpawnProjectile || attack.ProjectileSpawned) return;
            attack.ProjectileSpawned = true;
            int count = 0; foreach (var shot in projectiles) if (shot.Owner == owner) count++;
            if (maximum > 0 && count >= maximum) return;
            projectiles.Add(new Projectile(owner, attack));
        }
    }
}
