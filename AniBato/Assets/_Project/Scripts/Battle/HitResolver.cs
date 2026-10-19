using System.Collections.Generic;

namespace AniBato.Battle
{
    public readonly struct HitContact
    {
        public readonly FighterState Attacker;
        public readonly FighterState Target;
        public readonly AttackInstance Attack;
        public readonly int Facing;
        public readonly bool IsProjectile;
        public readonly Projectile Projectile;
        public HitContact(FighterState attacker, FighterState target, AttackInstance attack, bool projectile = false, int facing = 0, Projectile projectileRef = null)
        { Attacker = attacker; Target = target; Attack = attack; Facing = facing == 0 ? attacker.Facing : facing; IsProjectile = projectile; Projectile = projectileRef; }
    }

    public sealed class HitResolver
    {
        // Gather both directions BEFORE applying damage so equal-tick attacks can trade.
        public void Gather(FighterState attacker, FighterState target, List<HitContact> contacts)
        {
            AttackInstance attack = attacker.Attack;
            if (attack == null || !attack.IsDamageActive || attack.HasHit(target.PlayerIndex) ||
                attack.Definition.SpawnProjectile || attack.Definition.NonDamaging || target.ImmunityFrom(attacker.PlayerIndex) > 0 ||
                attacker.FrozenThisTick || attacker.Mode == FighterMode.KO || target.Mode == FighterMode.KO || target.HeartRecoveryRemaining > 0) return;
            if (attack.Overlaps(attacker, target.Hurtbox) && attack.MarkHit(target.PlayerIndex))
                contacts.Add(new HitContact(attacker, target, attack, facing: attack.Definition.NormalDirection == NormalDirection.Back || attack.Definition.NormalDirection == NormalDirection.AirBack ? -attacker.Facing : attacker.Facing));
        }
        public void Gather(Projectile projectile, FighterState target, List<HitContact> contacts)
        {
            if (!projectile.CanDamage || projectile.Owner == target || projectile.ContactCooldown > 0 || projectile.Attack.Clashed || projectile.RemainingFrames <= 0 || projectile.Attack.HasHit(target.PlayerIndex) || target.Mode == FighterMode.KO ||
                target.ImmunityFrom(projectile.Owner.PlayerIndex) > 0 || target.HeartRecoveryRemaining > 0) return;
            if (projectile.Hitbox.Overlaps(target.Hurtbox) && projectile.Attack.MarkHit(target.PlayerIndex))
                contacts.Add(new HitContact(projectile.Owner, target, projectile.Attack, true, projectile.Facing, projectile));
        }
    }
}
