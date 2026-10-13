using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public enum DefenseOutcome { Hit, Guard, Dodge, Counter, JustGuard, JustDodge, GuardBreak, CrouchBreak }
    public interface ICounterEffect
    {
        void OnSuccess(FighterState defender, FighterState attacker, int tick, in BattleRuleDefinition rules, List<BattleEvent> events);
    }

    public sealed class DefenseSystem
    {
        public ICounterEffect CounterEffect { get; set; }
        public void CaptureOverlaps(FighterState target, FighterState source, IReadOnlyList<Projectile> projectiles)
        {
            target.PreDefenseOverlaps.Clear();
            var attack = source.Attack;
            if (attack != null && attack.IsDamageActive && !attack.Definition.SpawnProjectile && !attack.HasHit(target.PlayerIndex) &&
                !attack.Definition.NonDamaging && attack.Overlaps(source, target.Hurtbox)) target.PreDefenseOverlaps.Add(attack.Id);
            foreach (var projectile in projectiles)
                if (projectile.Owner != target && !projectile.Attack.HasHit(target.PlayerIndex) && projectile.Hitbox.Overlaps(target.Hurtbox))
                    target.PreDefenseOverlaps.Add(projectile.Attack.Id);
        }
        public DefenseOutcome Evaluate(in HitContact contact, in BattleRuleDefinition rules)
        {
            var target = contact.Target; var attack = contact.Attack.Definition;
            bool just = target.DefenseAge < rules.Defense.JustWindowFrames && !target.JustExcludedAttacks.Contains(contact.Attack.Id);
            if (target.Mode == FighterMode.Counter && attack.IsNormal && attack.IsCloseRange && !contact.IsProjectile)
                return DefenseOutcome.Counter;
            if (target.Mode == FighterMode.Guard)
            {
                if (attack.IsNormal && attack.BreaksGuard) return DefenseOutcome.CrouchBreak;
                if (just) return DefenseOutcome.JustGuard;
                if (StaminaSystem.CanSpend(target, StaminaSystem.GuardCost(target, attack.Kind))) return DefenseOutcome.Guard;
                return attack.IsUltimate ? DefenseOutcome.Hit : DefenseOutcome.GuardBreak;
            }
            if (target.Mode == FighterMode.Dodge)
            {
                if (just) return DefenseOutcome.JustDodge;
                if (!attack.IsUltimate && !attack.PiercesNormalDodge) return DefenseOutcome.Dodge;
            }
            return DefenseOutcome.Hit;
        }
        public void Apply(in HitContact contact, DefenseOutcome outcome, ComboTracker counterCombo, DamageSystem damage,
            int tick, in BattleRuleDefinition rules, List<BattleEvent> events)
        {
            var source = contact.Attacker; var target = contact.Target; var definition = rules.Defense;
            BattleEventKind kind;
            switch (outcome)
            {
                case DefenseOutcome.CrouchBreak:
                    damage.Apply(contact, counterCombo, tick, rules, events);
                    target.GuardLockRemaining = target.Definition.Stamina.GuardLockFrames;
                    target.HitstunRemaining = Math.Max(target.HitstunRemaining, target.Definition.Stamina.GuardBreakFrames);
                    kind = BattleEventKind.GuardBroken; break;
                case DefenseOutcome.GuardBreak:
                    target.StaminaUnits = 0; StaminaSystem.BreakGuard(target);
                    kind = BattleEventKind.GuardBroken; break;
                case DefenseOutcome.Counter:
                    int previousHearts = source.Hearts;
                    counterCombo.RegisterHit(tick, rules, false);
                    damage.AccumulateNormal(source, counterCombo, target.PlayerIndex, contact.Attack.Id, tick, rules, events);
                    DamageSystem.BeginRecoil(source, contact.Attack.Definition, -contact.Facing);
                    source.LastDamageLostHeart = source.Hearts < previousHearts;
                    source.Attack = null; source.DashRemaining = 0;
                    source.FastFalling = false; source.GuardStunRemaining = 0; source.ActionFrames = 0;
                    source.Velocity = new BattleVector(definition.CounterKnockback.X * -contact.Facing, definition.CounterKnockback.Y);
                    if (source.Hearts < previousHearts && source.Hearts > 0 && rules.HeartRecoveryFrames > 0)
                        DamageSystem.BeginHeartRecovery(source, -contact.Facing, rules);
                    if (source.Velocity.Y > 0) { source.Grounded = false; source.GroundPlatform = -1; }
                    source.HitstunRemaining = definition.CounterTargetStunFrames;
                    source.FollowupImmunity[target.PlayerIndex] = definition.CounterTargetImmunityFrames;
                    source.Mode = source.Hearts > 0 ? FighterMode.Knockback : FighterMode.KO;
                    target.Mode = FighterMode.Recovery; target.ActionFrames = definition.CounterSuccessFrames;
                    GaugeSystem.Add(target, rules.Gauge.OnCounter, tick, rules, events);
                    GaugeSystem.Add(source, rules.Gauge.OnDamaged, tick, rules, events);
                    kind = BattleEventKind.CounterSuccess;
                    (target.CounterEffect ?? CounterEffect)?.OnSuccess(target, source, tick, rules, events); break;
                case DefenseOutcome.Guard:
                case DefenseOutcome.JustGuard:
                    // Recheck after earlier contacts in the same tick consumed stamina.
                    if (outcome == DefenseOutcome.Guard && !StaminaSystem.Spend(target, StaminaSystem.GuardCost(target, contact.Attack.Definition.Kind)))
                    {
                        if (contact.Attack.Definition.IsUltimate) damage.Apply(contact, counterCombo, tick, rules, events);
                        else { target.StaminaUnits = 0; StaminaSystem.BreakGuard(target); events.Add(new BattleEvent(BattleEventKind.GuardBroken, tick, target.PlayerIndex, source.PlayerIndex, contact.Attack.Id)); }
                        return;
                    }
                    target.GuardStunRemaining = Math.Max(target.GuardStunRemaining,
                        outcome == DefenseOutcome.JustGuard ? definition.JustGuardStunFrames : definition.GuardStunFrames);
                    target.Velocity = new BattleVector(definition.GuardPushSpeed * contact.Facing, target.Velocity.Y);
                    kind = outcome == DefenseOutcome.JustGuard ? BattleEventKind.JustGuard : BattleEventKind.Guarded;
                    if (outcome == DefenseOutcome.JustGuard)
                    {
                        GaugeSystem.Add(target, rules.Gauge.OnJustGuard, tick, rules, events);
                        StunMeleeSource(contact, definition.JustAttackerStunFrames);
                    }
                    else if (contact.Attack.Definition.GuardedRecoveryFrames > 0)
                        StunMeleeSource(contact, contact.Attack.Definition.GuardedRecoveryFrames);
                    break;
                case DefenseOutcome.JustDodge:
                    target.JustDodgeSucceeded = true;
                    target.Mode = FighterMode.Recovery; target.ActionFrames = definition.JustDodgeRecoveryFrames;
                    target.Velocity = new BattleVector(0, target.Velocity.Y);
                    GaugeSystem.Add(target, rules.Gauge.OnJustDodge, tick, rules, events);
                    StunMeleeSource(contact, definition.JustAttackerStunFrames);
                    kind = BattleEventKind.JustDodge; break;
                case DefenseOutcome.Dodge: kind = BattleEventKind.Dodged; break;
                default: return;
            }
            events.Add(new BattleEvent(kind, tick, target.PlayerIndex, source.PlayerIndex, contact.Attack.Id));
        }
        private static void StunMeleeSource(in HitContact contact, int frames)
        {
            if (contact.IsProjectile || contact.Attacker.Mode == FighterMode.KO) return;
            contact.Attacker.Attack = null; contact.Attacker.DashRemaining = 0;
            contact.Attacker.Mode = FighterMode.Recovery;
            contact.Attacker.ActionFrames = Math.Max(contact.Attacker.ActionFrames, frames);
            contact.Attacker.Velocity = new BattleVector(0, contact.Attacker.Velocity.Y);
        }
    }
}
