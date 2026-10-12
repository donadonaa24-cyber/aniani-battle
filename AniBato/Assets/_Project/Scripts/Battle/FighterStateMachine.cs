using System;

namespace AniBato.Battle
{
    public sealed class FighterStateMachine
    {
        private long nextAttackId = 1;
        public void Step(FighterState fighter, in FrameInput input, int tick, in BattleRuleDefinition rules)
        {
            fighter.InputBuffer.Push(input, tick, rules);
            fighter.ParalyzedThisTick = fighter.ParalysisRemaining > 0;
            SkillSystem.Step(fighter);
            if (fighter.HeartRecoveryRemaining > 0) fighter.HeartRecoveryRemaining--;
            if (fighter.CounterCooldownRemaining > 0) fighter.CounterCooldownRemaining--;
            for (int i = 0; i < fighter.FollowupImmunity.Length; i++)
                if (fighter.FollowupImmunity[i] > 0) fighter.FollowupImmunity[i]--;
            fighter.FrozenThisTick = fighter.HitstopRemaining > 0;
            if (fighter.FrozenThisTick) { fighter.HitstopRemaining--; return; }
            if (fighter.Mode == FighterMode.KO) return;
            if (fighter.DropThroughRemaining > 0) fighter.DropThroughRemaining--;
            if (fighter.HitstunRemaining > 0)
            {
                fighter.HitstunRemaining--;
                if (fighter.ParalysisRemaining > 0) fighter.ParalysisRemaining--;
                if (fighter.Mode != FighterMode.Knockback) fighter.Mode = FighterMode.Hitstun;
                return;
            }
            if (fighter.ParalysisRemaining > 0)
            { fighter.ParalysisRemaining--; fighter.Mode = FighterMode.Hitstun; return; }
            if (StepDefense(fighter, input, rules)) return;
            if (fighter.Attack != null)
            {
                var stance = fighter.Attack;
                if (stance.Definition.RequiresFollowup && stance.Frame == stance.Definition.StartupFrames - 1)
                {
                    if (!fighter.InputBuffer.Consume(InputButtons.Special, tick) && !fighter.InputBuffer.Consume(InputButtons.Attack, tick))
                    {
                        if (--stance.StanceRemaining > 0) return;
                        fighter.Attack = null; fighter.Mode = FighterMode.Recovery;
                        fighter.ActionFrames = stance.Definition.RecoveryFrames; return;
                    }
                    fighter.InputBuffer.ClearActions();
                }
                fighter.Attack.Frame++;
                if (fighter.Attack.IsComplete)
                {
                    var previous = fighter.Attack;
                    fighter.Attack = null;
                    if (previous.NextQueued) StartNormal(fighter, previous.ChainIndex + 1, previous.AirborneAttack);
                }
            }

            if (fighter.Attack != null)
            {
                if (fighter.Attack.Definition.IsNormal)
                {
                    var attacks = fighter.Attack.AirborneAttack ? fighter.Definition.AirNormals : fighter.Definition.GroundNormals;
                    if (fighter.Attack.ChainIndex + 1 < attacks.Length && fighter.InputBuffer.Consume(InputButtons.Attack, tick))
                        fighter.Attack.NextQueued = true;
                }
                fighter.Mode = fighter.Attack.Definition.IsNormal ? FighterMode.Attack : FighterMode.Special;
                ApplyAttackMotion(fighter);
                SkillSystem.OnActive(fighter);
                return;
            }

            if (GaugeSystem.CanUseUltimate(fighter, rules) && fighter.InputBuffer.Consume(InputButtons.Ultimate, tick))
            {
                fighter.Gauge -= rules.Gauge.UltimateCost; fighter.UltimateUses++;
                float ultimateX = fighter.InputBuffer.DirectionFor(InputButtons.Ultimate).X;
                if (Math.Abs(ultimateX) > rules.DirectionThreshold) fighter.Facing = ultimateX > 0 ? 1 : -1;
                StartAttack(fighter, fighter.Definition.Ultimate, 0, !fighter.Grounded); fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.CounterCooldownRemaining == 0 && fighter.InputBuffer.Consume(InputButtons.Counter, tick))
            {
                BeginDefense(fighter, FighterMode.Counter); fighter.ActionFrames = rules.Defense.CounterWindowFrames;
                fighter.CounterCooldownRemaining = rules.Defense.CounterCooldownFrames;
                fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.InputBuffer.Consume(InputButtons.Dodge, tick))
            {
                BeginDefense(fighter, FighterMode.Dodge); fighter.ActionFrames = rules.Defense.DodgeInvulnerableFrames;
                fighter.JustDodgeSucceeded = false;
                float dodgeX = fighter.InputBuffer.DirectionFor(InputButtons.Dodge).X;
                int direction = dodgeX > rules.DirectionThreshold ? 1 : dodgeX < -rules.DirectionThreshold ? -1 : fighter.Facing;
                fighter.Facing = direction;
                fighter.Velocity = new BattleVector(direction * (fighter.Grounded ? rules.Defense.GroundDodgeSpeed : rules.Defense.AirDodgeSpeed), 0);
                fighter.InputBuffer.ClearActions(); return;
            }
            BattleVector specialDirection = fighter.InputBuffer.DirectionFor(InputButtons.Special);
            SpecialSlot slot = specialDirection.Y > rules.DirectionThreshold ? SpecialSlot.Up : specialDirection.Y < -rules.DirectionThreshold ? SpecialSlot.Down :
                Math.Abs(specialDirection.X) > rules.DirectionThreshold ? SpecialSlot.Side : SpecialSlot.Neutral;
            if (fighter.SpecialCooldown(slot) == 0 && (slot != SpecialSlot.Up || !fighter.UpSpecialUsed) && fighter.InputBuffer.Consume(InputButtons.Special, tick))
            {
                if (Math.Abs(specialDirection.X) > rules.DirectionThreshold) fighter.Facing = specialDirection.X > 0 ? 1 : -1;
                fighter.CurrentSpecial = slot;
                if (slot == SpecialSlot.Up) { fighter.UpSpecialUsed = true; fighter.FastFalling = false; }
                StartAttack(fighter, fighter.Definition.Special(slot), 0, !fighter.Grounded);
                fighter.SpecialCooldowns[(int)slot] = fighter.Definition.Special(slot).CooldownFrames;
                fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.InputBuffer.Consume(InputButtons.Attack, tick))
            {
                float attackX = fighter.InputBuffer.DirectionFor(InputButtons.Attack).X;
                if (Math.Abs(attackX) > rules.DirectionThreshold) fighter.Facing = attackX > 0 ? 1 : -1;
                StartNormal(fighter, 0, !fighter.Grounded); fighter.InputBuffer.ClearActions(); return;
            }
            if (input.IsHeld(InputButtons.Guard) || fighter.InputBuffer.Consume(InputButtons.Guard, tick))
            { BeginDefense(fighter, FighterMode.Guard); fighter.InputBuffer.ClearActions(); return; }
            if (fighter.JumpsUsed < fighter.Definition.MaxJumps && fighter.InputBuffer.Consume(InputButtons.Jump, tick))
            {
                fighter.JumpsUsed++; fighter.Grounded = false; fighter.GroundPlatform = -1;
                fighter.Velocity = new BattleVector(fighter.Velocity.X, fighter.Definition.JumpSpeed);
                fighter.FastFalling = false; fighter.DashRemaining = 0;
                if (Math.Abs(input.X) > rules.DirectionThreshold) fighter.Facing = input.X > 0 ? 1 : -1;
                fighter.Velocity = new BattleVector(input.X * fighter.Definition.MoveSpeed * fighter.MoveMultiplier, fighter.Velocity.Y);
                fighter.Mode = FighterMode.Jump; fighter.InputBuffer.ClearActions(); return;
            }
            if ((fighter.Grounded || fighter.AirDashesUsed < fighter.Definition.MaxAirDashes) &&
                fighter.InputBuffer.Consume(InputButtons.Dash, tick))
            {
                fighter.DashDirection = fighter.InputBuffer.ConsumeDashDirection(fighter.Facing);
                fighter.Facing = fighter.DashDirection;
                fighter.DashRemaining = fighter.Grounded ? fighter.Definition.GroundDashFrames : fighter.Definition.AirDashFrames;
                if (!fighter.Grounded) { fighter.AirDashesUsed++; fighter.FastFalling = false; }
            }
            if (fighter.Mode == FighterMode.Dash && fighter.Grounded &&
                input.X * fighter.DashDirection > rules.DirectionThreshold && fighter.DashRemaining == 0)
                fighter.DashRemaining = 1;
            if (fighter.DashRemaining > 0)
            {
                fighter.DashRemaining--;
                fighter.Velocity = new BattleVector(fighter.DashDirection * fighter.Definition.DashSpeed * fighter.DashMultiplier, 0);
                fighter.Mode = FighterMode.Dash;
                return;
            }
            if (Math.Abs(input.X) > rules.DirectionThreshold) fighter.Facing = input.X > 0 ? 1 : -1;
            fighter.Velocity = new BattleVector(input.X * fighter.Definition.MoveSpeed * fighter.MoveMultiplier, fighter.Velocity.Y);
            fighter.Mode = FighterMode.Idle;
            RefreshLocomotion(fighter);
        }

        private void StartNormal(FighterState fighter, int chain, bool air)
        {
            var attacks = air ? fighter.Definition.AirNormals : fighter.Definition.GroundNormals;
            StartAttack(fighter, attacks[chain], chain, air);
        }
        private void StartAttack(FighterState fighter, in AttackDefinition attack, int chain, bool air)
        {
            var adjusted = attack;
            if (fighter.BuffRemaining > 0)
                adjusted.StartupFrames = (int)Math.Ceiling(attack.StartupFrames * SkillSystem.Multiplier(fighter.Buff.StartupMultiplier));
            if (adjusted.RequiresFollowup) adjusted.StartupFrames = Math.Max(1, adjusted.StartupFrames);
            fighter.Attack = new AttackInstance(nextAttackId++, adjusted, chain, air);
            fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
            fighter.DashRemaining = 0;
            fighter.Mode = attack.IsNormal ? FighterMode.Attack : FighterMode.Special;
            ApplyAttackMotion(fighter);
            SkillSystem.OnActive(fighter);
        }
        private static void ApplyAttackMotion(FighterState fighter)
        {
            var attack = fighter.Attack;
            if (attack.IsActive && (attack.Definition.ActiveVelocity.X != 0 || attack.Definition.ActiveVelocity.Y != 0))
            {
                fighter.Velocity = new BattleVector(attack.Definition.ActiveVelocity.X * fighter.Facing, attack.Definition.ActiveVelocity.Y);
                if (fighter.Velocity.Y > 0) { fighter.Grounded = false; fighter.GroundPlatform = -1; fighter.FastFalling = false; }
            }
            else fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
        }
        private static void BeginDefense(FighterState fighter, FighterMode mode)
        {
            fighter.Mode = mode; fighter.DefenseAge = 0; fighter.DashRemaining = 0;
            fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
            fighter.JustExcludedAttacks.Clear(); fighter.JustExcludedAttacks.UnionWith(fighter.PreDefenseOverlaps);
        }
        private static bool StepDefense(FighterState fighter, in FrameInput input, in BattleRuleDefinition rules)
        {
            if (fighter.Mode == FighterMode.Guard)
            {
                fighter.DefenseAge++;
                if (fighter.GuardStunRemaining > 0)
                { fighter.GuardStunRemaining--; return true; }
                fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
                if (input.IsHeld(InputButtons.Guard)) return true;
                fighter.Mode = FighterMode.Recovery; fighter.ActionFrames = rules.Defense.GuardReleaseFrames;
            }
            if (fighter.Mode == FighterMode.Dodge || fighter.Mode == FighterMode.Counter)
            {
                fighter.DefenseAge++;
                if (--fighter.ActionFrames > 0) return true;
                fighter.ActionFrames = fighter.Mode == FighterMode.Counter ? rules.Defense.CounterFailureFrames :
                    fighter.JustDodgeSucceeded ? rules.Defense.JustDodgeRecoveryFrames : rules.Defense.DodgeRecoveryFrames;
                fighter.Mode = FighterMode.Recovery;
                fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
            }
            if (fighter.Mode == FighterMode.Recovery && fighter.ActionFrames > 0)
            { fighter.ActionFrames--; return true; }
            if (fighter.Mode == FighterMode.Recovery) fighter.Mode = FighterMode.Idle;
            return false;
        }

        public static void RefreshLocomotion(FighterState fighter)
        {
            if (fighter.Mode == FighterMode.KO || !fighter.CanControlMovement || fighter.Mode == FighterMode.Dash) return;
            fighter.Mode = fighter.Grounded ? (fighter.Velocity.X == 0 ? FighterMode.Idle : FighterMode.Move) :
                (fighter.Velocity.Y > 0 ? FighterMode.Jump : FighterMode.Fall);
        }
    }
}
