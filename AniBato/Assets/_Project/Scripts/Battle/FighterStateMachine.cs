using System;

namespace AniBato.Battle
{
    public sealed class FighterStateMachine
    {
        private long nextAttackId = 1;
        public void Step(FighterState fighter, in FrameInput input, int tick, in BattleRuleDefinition rules, StageGeometry stage = null)
        {
            if (fighter.Mode != FighterMode.JumpStartup) fighter.JumpStartupRemaining = 0;
            if (fighter.Mode != FighterMode.Charging) fighter.ChargeFrames = 0;
            fighter.InputBuffer.Push(input, tick, rules);
            StaminaSystem.Step(fighter);
            fighter.DownHeldFrames = fighter.Grounded && input.Y < -rules.DirectionThreshold ? fighter.DownHeldFrames + 1 : 0;
            fighter.ParalyzedThisTick = fighter.ParalysisRemaining > 0;
            SkillSystem.Step(fighter);
            if (fighter.HeartRecoveryRemaining > 0) fighter.HeartRecoveryRemaining--;
            if (fighter.CounterCooldownRemaining > 0) fighter.CounterCooldownRemaining--;
            for (int i = 0; i < fighter.FollowupImmunity.Length; i++)
                if (fighter.FollowupImmunity[i] > 0) fighter.FollowupImmunity[i]--;
            fighter.FrozenThisTick = fighter.HitstopRemaining > 0;
            if (fighter.FrozenThisTick) { fighter.HitstopRemaining--; return; }
            if (fighter.Mode == FighterMode.KO) return;
            if(fighter.MaximumChargeLaunch && fighter.HitstunRemaining==0 && (fighter.Grounded || stage!=null &&
                fighter.Position.X>=stage[0].Bounds.Left && fighter.Position.X<=stage[0].Bounds.Right)) fighter.MaximumChargeLaunch=false;
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
            if (LedgeSystem.StepAttached(fighter,input,stage,rules)) return;
            if (fighter.Mode == FighterMode.JumpStartup)
            {
                if (!fighter.Grounded)
                {
                    fighter.JumpStartupRemaining = 0; fighter.Mode = FighterMode.Fall;
                    RefreshLocomotion(fighter); return;
                }
                if (--fighter.JumpStartupRemaining <= 0) LaunchJump(fighter, fighter.JumpStartupDirection);
                return;
            }
            if (fighter.Mode == FighterMode.Charging)
            {
                fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
                if (!fighter.Grounded) { fighter.ChargeFrames = 0; fighter.Mode = FighterMode.Fall; return; }
                if (input.IsHeld(InputButtons.Attack)) { fighter.ChargeFrames = Math.Min(rules.Charge.MaximumFrames, fighter.ChargeFrames + 1); return; }
                int frames = fighter.ChargeFrames; fighter.ChargeFrames = 0;
                StartNormal(fighter, 0, false, fighter.ChargeDirection, rules, frames >= rules.Charge.MinimumFrames ? frames : 0);
                fighter.InputBuffer.ClearActions(); return;
            }
            if (StepDefense(fighter, input, rules)) return;
            if (fighter.Attack != null)
            {
                if (AbilityChargeSystem.IsCharging(fighter) && AbilityChargeSystem.Hold(fighter, input, stage)) return;
                if (RecoveryMotionSystem.Charge(fighter, input)) return;
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
                    SkillSystem.OnComplete(fighter, previous);
                    fighter.Attack = null;
                    if (previous.NextQueued) StartNormal(fighter, previous.ChainIndex + 1, previous.AirborneAttack, previous.Definition.NormalDirection, rules);
                }
            }

            if (fighter.Attack != null)
            {
                if (fighter.Attack.Definition.IsNormal)
                {
                    var attacks = fighter.Attack.AirborneAttack ? fighter.Definition.AirNormals : fighter.Definition.GroundNormals;
                    if (!(rules.CombatInput.SingleAerialNormal && fighter.Attack.AirborneAttack) && fighter.Attack.ChainIndex + 1 < System.Math.Min(rules.MaxComboHits, attacks.Length) && fighter.InputBuffer.Consume(InputButtons.Attack, tick))
                        fighter.Attack.NextQueued = true;
                }
                fighter.Mode = fighter.Attack.Definition.IsNormal ? FighterMode.Attack : FighterMode.Special;
                ApplyAttackMotion(fighter, rules);
                SkillSystem.OnActive(fighter);
                return;
            }

            if (GaugeSystem.CanUseUltimate(fighter, rules) && fighter.InputBuffer.Consume(InputButtons.Ultimate, tick))
            {
                if (!fighter.UnlimitedUltimates) fighter.Gauge -= rules.Gauge.UltimateCost; fighter.UltimateUses++;
                float ultimateX = fighter.InputBuffer.DirectionFor(InputButtons.Ultimate).X;
                if (fighter.Grounded && Math.Abs(ultimateX) > rules.DirectionThreshold) fighter.Facing = ultimateX > 0 ? 1 : -1;
                StartAttack(fighter, fighter.Definition.Ultimate, 0, !fighter.Grounded, rules); fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.CounterCooldownRemaining == 0 && fighter.InputBuffer.Consume(InputButtons.Counter, tick))
            {
                BeginDefense(fighter, FighterMode.Counter); fighter.ActionFrames = rules.Defense.CounterWindowFrames;
                fighter.CounterCooldownRemaining = rules.Defense.CounterCooldownFrames;
                fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.InputBuffer.Consume(InputButtons.Dodge, tick))
            {
                if (!StaminaSystem.Spend(fighter, fighter.Definition.Stamina.DodgeCost)) { StopRejectedAction(fighter); return; }
                BeginDefense(fighter, FighterMode.Dodge); fighter.ActionFrames = rules.Defense.DodgeInvulnerableFrames;
                fighter.JustDodgeSucceeded = false;
                float dodgeX = fighter.InputBuffer.DirectionFor(InputButtons.Dodge).X;
                int direction = dodgeX > rules.DirectionThreshold ? 1 : dodgeX < -rules.DirectionThreshold ? -1 : fighter.Facing;
                if (fighter.Grounded) fighter.Facing = direction;
                fighter.Velocity = new BattleVector(direction * (fighter.Grounded ? rules.Defense.GroundDodgeSpeed : rules.Defense.AirDodgeSpeed), 0);
                fighter.InputBuffer.ClearActions(); return;
            }
            BattleVector specialDirection = fighter.InputBuffer.DirectionFor(InputButtons.Special);
            SpecialSlot slot = rules.CombatInput.SpecialDirection(specialDirection, rules.DirectionThreshold);
            var special = fighter.Definition.Special(slot);
            bool placementAvailable = special.Ability.PlacementLimit == 0 || fighter.PlacedCount < special.Ability.PlacementLimit;
            if (fighter.SpecialCooldown(slot) == 0 && placementAvailable && (!special.Ability.GroundOnly || fighter.Grounded) &&
                (slot != SpecialSlot.Up || !fighter.UpSpecialUsed) && fighter.InputBuffer.Consume(InputButtons.Special, tick))
            {
                if (fighter.Grounded && Math.Abs(specialDirection.X) > rules.DirectionThreshold) fighter.Facing = specialDirection.X > 0 ? 1 : -1;
                fighter.CurrentSpecial = slot;
                if (slot == SpecialSlot.Up) { fighter.UpSpecialUsed = true; fighter.FastFalling = false; }
                StartAttack(fighter, fighter.Definition.Special(slot), 0, !fighter.Grounded, rules);
                fighter.Attack.RecoveryDirection = RecoveryMotionSystem.Direction(specialDirection.X, specialDirection.Y, new BattleVector(0, 1));
                fighter.SpecialCooldowns[(int)slot] = fighter.Definition.Special(slot).CooldownFrames;
                fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.InputBuffer.Consume(InputButtons.Attack, tick))
            {
                var direction = DirectionalAttackSelector.Select(fighter, fighter.InputBuffer.DirectionFor(InputButtons.Attack), rules.DirectionThreshold, rules.CombatInput.SingleAerialNormal);
                if (rules.Charge.Enabled && fighter.Grounded && input.IsHeld(InputButtons.Attack))
                {
                    fighter.ChargeFrames = 1; fighter.ChargeDirection = direction; fighter.Mode = FighterMode.Charging;
                    fighter.Velocity = default; fighter.DashRemaining = 0; fighter.InputBuffer.ClearActions(); return;
                }
                StartNormal(fighter, 0, !fighter.Grounded, direction, rules); fighter.InputBuffer.ClearActions(); return;
            }
            if (fighter.GuardLockRemaining == 0 && (input.IsHeld(InputButtons.Guard) || fighter.InputBuffer.Consume(InputButtons.Guard, tick)))
            { BeginDefense(fighter, FighterMode.Guard); fighter.InputBuffer.ClearActions(); return; }
            if (fighter.JumpsUsed < fighter.Definition.MaxJumps && fighter.InputBuffer.Consume(InputButtons.Jump, tick))
            {
                bool groundJump = fighter.Grounded;
                if (!StaminaSystem.Spend(fighter, groundJump ? fighter.Definition.Stamina.JumpCost : fighter.Definition.Stamina.AirJumpCost, groundJump))
                { StopRejectedAction(fighter); return; }
                fighter.FastFalling = false; fighter.DashRemaining = 0;
                if (groundJump && Math.Abs(input.X) > rules.DirectionThreshold) fighter.Facing = input.X > 0 ? 1 : -1;
                fighter.InputBuffer.ClearActions();
                if (groundJump && rules.GroundJumpStartupFrames > 0)
                {
                    fighter.JumpStartupRemaining = rules.GroundJumpStartupFrames;
                    fighter.JumpStartupDirection = input.X;
                    fighter.Velocity = new BattleVector(0, 0); fighter.Mode = FighterMode.JumpStartup;
                }
                else LaunchJump(fighter, input.X);
                return;
            }
            if ((fighter.Grounded || fighter.AirDashesUsed < fighter.Definition.MaxAirDashes) &&
                fighter.InputBuffer.Consume(InputButtons.Dash, tick))
            {
                if (!StaminaSystem.Spend(fighter, fighter.Grounded ? fighter.Definition.Stamina.GroundDashCost : fighter.Definition.Stamina.AirDashCost))
                { StopRejectedAction(fighter); return; }
                fighter.DashDirection = fighter.InputBuffer.ConsumeDashDirection(fighter.Facing);
                if (fighter.Grounded) fighter.Facing = fighter.DashDirection;
                fighter.DashRemaining = fighter.Grounded ? fighter.Definition.GroundDashFrames : fighter.Definition.AirDashFrames;
                if (!fighter.Grounded) { fighter.AirDashesUsed++; fighter.FastFalling = false; }
            }
            if (fighter.Grounded && input.Y < -rules.DirectionThreshold)
            { fighter.DashRemaining = 0; fighter.Velocity = new BattleVector(0, fighter.Velocity.Y); fighter.Mode = FighterMode.Crouch; return; }
            if (fighter.Mode == FighterMode.Dash && fighter.Grounded &&
                input.X * fighter.DashDirection > rules.DirectionThreshold && fighter.DashRemaining == 0 &&
                StaminaSystem.Spend(fighter, fighter.Definition.Stamina.GroundDashCost)) fighter.DashRemaining = fighter.Definition.GroundDashFrames;
            if (fighter.DashRemaining > 0)
            {
                fighter.DashRemaining--;
                fighter.Velocity = new BattleVector(fighter.DashDirection * fighter.Definition.DashSpeed * fighter.DashMultiplier, 0);
                fighter.Mode = FighterMode.Dash;
                return;
            }
            if (fighter.Grounded && Math.Abs(input.X) > rules.DirectionThreshold) fighter.Facing = input.X > 0 ? 1 : -1;
            fighter.Velocity = new BattleVector(input.X * fighter.Definition.MoveSpeed * fighter.MoveMultiplier, fighter.Velocity.Y);
            fighter.Mode = FighterMode.Idle;
            RefreshLocomotion(fighter);
        }

        private static void StopRejectedAction(FighterState fighter)
        { fighter.InputBuffer.ClearActions(); fighter.DashRemaining = 0; fighter.Velocity = new BattleVector(0, fighter.Velocity.Y); fighter.Mode = FighterMode.Idle; RefreshLocomotion(fighter); }
        private static void LaunchJump(FighterState fighter, float direction)
        {
            fighter.JumpStartupRemaining = 0;
            fighter.JumpsUsed++; fighter.Grounded = false; fighter.GroundPlatform = -1;
            fighter.Velocity = new BattleVector(direction * fighter.Definition.MoveSpeed * fighter.MoveMultiplier, fighter.Definition.JumpSpeed);
            fighter.FastFalling = false; fighter.DashRemaining = 0; fighter.Mode = FighterMode.Jump;
        }
        private void StartNormal(FighterState fighter, int chain, bool air, NormalDirection direction, in BattleRuleDefinition rules, int chargeFrames=0)
        {
            var attack=DirectionalAttackSelector.Attack(fighter.Definition,direction,chain,air);
            int cost=StaminaSystem.NormalCost(fighter,attack,chain,air,chargeFrames,rules.Charge);
            bool deferred=attack.SpawnProjectile && attack.Ability.ProjectileChannel==ProjectileChannel.Shared;
            if(cost>0 && (deferred ? !StaminaSystem.CanSpend(fighter,cost) : !StaminaSystem.Spend(fighter,cost)))
            {StaminaSystem.Reject(fighter);StopRejectedAction(fighter);return;}
            StartAttack(fighter,attack,chain,air,rules);
            fighter.Attack.ChargeFrames=chargeFrames;fighter.Attack.PlannedStaminaCost=cost;
        }
        private void StartAttack(FighterState fighter, in AttackDefinition attack, int chain, bool air, in BattleRuleDefinition rules)
        {
            var adjusted = attack;
            if (fighter.BuffRemaining > 0 && !attack.Recovery.ChargedTeleport && !attack.AbilityCharge.Enabled)
                adjusted.StartupFrames = (int)Math.Ceiling(attack.StartupFrames * fighter.StartupMultiplier);
            if (adjusted.RequiresFollowup) adjusted.StartupFrames = Math.Max(1, adjusted.StartupFrames);
            fighter.Attack = new AttackInstance(nextAttackId++, adjusted, chain, air);
            if (attack.Recovery.ChargedTeleport || attack.AbilityCharge.Enabled) fighter.Velocity = default;
            if (!(air && attack.IsNormal && rules.CombatInput.FlowingAerialNormal)) fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
            fighter.DashRemaining = 0;
            fighter.Mode = attack.IsNormal ? FighterMode.Attack : FighterMode.Special;
            ApplyAttackMotion(fighter, rules);
            SkillSystem.OnActive(fighter);
        }
        private static void ApplyAttackMotion(FighterState fighter, in BattleRuleDefinition rules)
        {
            var attack = fighter.Attack;
            if (attack.AirborneAttack && attack.Definition.IsNormal && rules.CombatInput.FlowingAerialNormal) return;
            if (attack.IsActive && attack.Definition.Recovery.Style != RecoveryStyle.None) return;
            if (attack.IsActive && attack.Definition.Ability.Motion != AbilityMotion.None) return;
            if (attack.IsActive && (attack.Definition.ActiveVelocity.X != 0 || attack.Definition.ActiveVelocity.Y != 0))
            {
                float vertical = attack.Definition.VerticalMotionOnStartOnly &&
                    (attack.Definition.ActiveVelocity.Y == 0 || attack.Frame != attack.Definition.StartupFrames) ? fighter.Velocity.Y : attack.Definition.ActiveVelocity.Y;
                fighter.Velocity = new BattleVector(attack.Definition.ActiveVelocity.X * fighter.Facing, vertical);
                if (fighter.Velocity.Y > 0) { fighter.Grounded = false; fighter.GroundPlatform = -1; fighter.FastFalling = false; }
            }
            else fighter.Velocity = new BattleVector(0, fighter.Velocity.Y);
            if (attack.Definition.IsNormal && attack.Frame < attack.Definition.StartupFrames + attack.Definition.ActiveFrames)
                fighter.Velocity = new BattleVector(fighter.Velocity.X + attack.Definition.NormalForwardSpeed * fighter.Facing, fighter.Velocity.Y);
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
            if (fighter.Mode == FighterMode.KO || !fighter.CanControlMovement || fighter.Mode == FighterMode.Dash || fighter.Mode == FighterMode.Crouch && fighter.Grounded) return;
            fighter.Mode = fighter.Grounded ? (fighter.Velocity.X == 0 ? FighterMode.Idle : FighterMode.Move) :
                (fighter.Velocity.Y > 0 ? FighterMode.Jump : FighterMode.Fall);
        }
    }
}
