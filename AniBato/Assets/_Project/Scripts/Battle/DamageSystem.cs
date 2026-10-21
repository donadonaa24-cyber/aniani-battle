using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class DamageSystem
    {
        public void Apply(in HitContact contact, ComboTracker combo, int tick, in BattleRuleDefinition rules, List<BattleEvent> events, StageGeometry stage = null)
        {
            var source = contact.Attacker;
            var target = contact.Target;
            if (target.HeartRecoveryRemaining > 0) return;
            LedgeSystem.Interrupt(target);
            int beforeHearts = target.Hearts;
            var attack = contact.Attack;
            if (target.Mode == FighterMode.Guard && attack.Definition.IsUltimate &&
                !StaminaSystem.CanSpend(target, StaminaSystem.GuardCost(target, AttackKind.Ultimate))) StaminaSystem.Reject(target);
            combo.RegisterHit(tick, rules, attack.Definition.IsNormal);
            events.Add(new BattleEvent(BattleEventKind.Hit, tick, source.PlayerIndex, target.PlayerIndex, attack.Id, combo.HitCount, attack.Definition.Kind));
            if (attack.Definition.IsNormal || attack.Definition.DamageMode == AttackDamageMode.NormalAccumulation)
            {
                AccumulateNormal(target, combo, source.PlayerIndex, attack.Id, tick, rules, events);
            }
            // Damage hooks already preserve normal accumulation for future specials/ultimates.
            else if (attack.Definition.DamageMode != AttackDamageMode.StatusOnly && combo.TryLoseHeart(rules, attack.Definition.IsUltimate))
                LoseHearts(target, attack.Definition.IsUltimate ? 2 : 1, source.PlayerIndex, attack.Id, tick, events);
            GaugeSystem.Add(source, rules.Gauge.OnHit, tick, rules, events);
            GaugeSystem.Add(target, rules.Gauge.OnDamaged, tick, rules, events);
            source.HitstopRemaining = Math.Max(source.HitstopRemaining, attack.Definition.HitstopFrames);
            target.HitstopRemaining = Math.Max(target.HitstopRemaining, attack.Definition.HitstopFrames);
            target.HitstunRemaining = attack.Definition.HitstunFrames;
            target.Attack = null; target.DashRemaining = 0;
            target.ChargeFrames = 0;
            target.ActionFrames = 0; target.GuardStunRemaining = 0;
            bool finish = attack.Definition.IsNormal && combo.Finished;
            BattleVector knockback = (finish ? rules.ComboFinishKnockback : attack.Definition.Knockback) * SkillSystem.Multiplier(attack.Definition.KnockbackMultiplier);
            target.Velocity = new BattleVector(knockback.X * contact.Facing, knockback.Y);
            BeginRecoil(target, attack.Definition, contact.Facing);
            target.LastDamageLostHeart = target.Hearts < beforeHearts;
            if (target.Hearts < beforeHearts && target.Hearts > 0 && rules.HeartRecoveryFrames > 0)
                BeginHeartRecovery(target, contact.Facing, rules);
            target.FastFalling = false;
            if (finish)
            {
                target.HitstunRemaining = rules.ComboFinishHitstunFrames;
                target.FollowupImmunity[source.PlayerIndex] = rules.FollowupImmunityFrames;
                events.Add(new BattleEvent(BattleEventKind.ComboFinish, tick, source.PlayerIndex, target.PlayerIndex, attack.Id));
            }
            bool charged = rules.Charge.Enabled && attack.ChargeFrames >= rules.Charge.MinimumFrames && stage != null;
            target.MaximumChargeLaunch=charged && attack.ChargeFrames>=rules.Charge.MaximumFrames && rules.Charge.MaximumTravelBeyondFloor>0;
            if (charged)
            {
                knockback = ChargedAttack.Knockback(target, contact.Facing, attack.ChargeFrames, stage, rules.Charge);
                target.Velocity = new BattleVector(knockback.X * contact.Facing, knockback.Y);
                target.HitstunRemaining = ChargedAttack.Hitstun(attack.ChargeFrames,rules.Charge);
                target.DamageRecoilRemaining = 0; target.DamageRecoilSpeed = 0;
            }
            if (target.Velocity.Y > 0) { target.Grounded = false; target.GroundPlatform = -1; }
            target.Mode = target.Hearts > 0 ? (finish || charged ? FighterMode.Knockback : FighterMode.Hitstun) : FighterMode.KO;
            SkillSystem.OnHit(target, attack.Definition);
        }

        public void AccumulateNormal(FighterState target, ComboTracker combo, int source, long attack, int tick,
            in BattleRuleDefinition rules, List<BattleEvent> events)
        {
            if (target.HeartRecoveryRemaining > 0) return;
            int beforeHearts = target.Hearts;
            target.NormalHitCount++;
            if (target.NormalHitCount < rules.NormalHitsPerHeart) return;
            if (!combo.TryLoseHeart(rules))
            { target.NormalHitCount = rules.NormalHitsPerHeart - 1; return; }
            target.NormalHitCount = 0;
            LoseHearts(target, 1, source, attack, tick, events);
            if (target.Hearts < beforeHearts && target.Hearts > 0 && rules.HeartRecoveryFrames > 0)
                BeginHeartRecovery(target, target.Facing * -1, rules);
            events.Add(new BattleEvent(BattleEventKind.Break, tick, source, target.PlayerIndex, attack));
        }
        internal static void BeginHeartRecovery(FighterState target, int direction, in BattleRuleDefinition rules)
        {
            target.HeartRecoveryRemaining = rules.HeartRecoveryFrames;
            float x = System.Math.Max(System.Math.Abs(target.Velocity.X), rules.HeartRecoveryKnockback.X);
            target.Velocity = new BattleVector(direction * x, System.Math.Max(target.Velocity.Y, rules.HeartRecoveryKnockback.Y));
            target.Grounded = false; target.GroundPlatform = -1;
        }
        internal static void BeginRecoil(FighterState target, in AttackDefinition attack, int direction)
        {
            target.DamageRecoilRemaining = target.Hearts > 0 ? attack.DamageRecoilFrames : 0;
            target.DamageRecoilSpeed = target.DamageRecoilRemaining > 0 ?
                attack.DamageRecoilDistance * BattleRuleDefinition.TickRate / attack.DamageRecoilFrames * direction : 0;
        }

        private static void LoseHearts(FighterState target, int amount, int source, long attack, int tick, List<BattleEvent> events)
        {
            int lost = Math.Min(amount, target.Hearts);
            target.Hearts -= lost;
            events.Add(new BattleEvent(BattleEventKind.HeartLost, tick, source, target.PlayerIndex, attack, lost));
            if (target.Hearts == 0) events.Add(new BattleEvent(BattleEventKind.KO, tick, source, target.PlayerIndex, attack));
        }
    }
}
