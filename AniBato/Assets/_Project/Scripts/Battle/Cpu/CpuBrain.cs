using System;

namespace AniBato.Battle
{
    // Only reads simulation state. Delayed opponent snapshots model reaction time; safety uses current geometry.
    public sealed class CpuBrain : ICpuBrain
    {
        private struct Observation
        {
            public BattleVector Position;
            public FighterMode Mode;
            public AttackDefinition Attack;
            public int AttackFrame, Facing;
            public bool Attacking;
        }
        public CpuDefinition Definition { get; }
        private readonly Observation[] history;
        private int historyIndex, samples, cooldown, guardRemaining;
        private uint random;
        private FrameInput movement;
        private long queuedChain = -1;
        private bool recoveryQueued;
        private bool wasAttacking;
        private int attackRest;
        private AttackInstance committedAttack;
        private int consecutiveWhiffs;
        private long lastLandedAttack;
        public CpuBrain(in CpuDefinition definition, uint seed)
        { definition.Validate(); Definition = definition; history = new Observation[definition.ReactionFrames + 1]; random = seed == 0 ? 1u : seed; }
        private bool Chance(float probability)
        { random ^= random << 13; random ^= random >> 17; random ^= random << 5; return (random & 0xffffff) / 16777216f < probability; }
        public FrameInput NextInput(BattleSimulation sim, int player)
        {
            if (player < 0 || player > 1) throw new ArgumentOutOfRangeException(nameof(player));
            if (sim.Match.IsFinished || sim.CutInRemainingFrames > 0) return default;
            var self = player == 0 ? sim.P1 : sim.P2; var opponent = player == 0 ? sim.P2 : sim.P1;
            foreach (var feedback in sim.Events)
                if (feedback.Kind == BattleEventKind.Hit && feedback.SourcePlayer == player)
                { lastLandedAttack = feedback.AttackId; consecutiveWhiffs = 0; }
            if (wasAttacking && self.Attack == null)
            {
                attackRest = Definition.AttackRestFrames;
                if (committedAttack != null && !committedAttack.Definition.NonDamaging)
                    consecutiveWhiffs = committedAttack.Id == lastLandedAttack ? 0 : consecutiveWhiffs + 1;
            }
            wasAttacking = self.Attack != null;
            if (self.Attack != null) committedAttack = self.Attack;
            if (attackRest > 0) attackRest--;
            history[historyIndex] = new Observation { Position = opponent.Position, Mode = opponent.Mode, Facing = opponent.Facing,
                Attacking = opponent.Attack != null, Attack = opponent.Attack == null ? default : opponent.Attack.Definition,
                AttackFrame = opponent.Attack == null ? 0 : opponent.Attack.Frame };
            historyIndex = (historyIndex + 1) % history.Length; samples++;
            var seen = history[samples < history.Length ? 0 : historyIndex];
            var floor = sim.Stage[0].Bounds;
            var safe = sim.Shrink.WarningIssued ? sim.Shrink.NextMainBounds : floor;
            float left = safe.Left + Definition.EdgeMargin, right = safe.Right - Definition.EdgeMargin;
            int inward = Math.Sign(floor.Center.X - self.Position.X);
            if (self.Mode != FighterMode.Recovery) recoveryQueued = false;
            // Let stun/hitstop finish; do not generate hidden buffered defenses during freezes.
            if (self.HitstopRemaining > 0 || self.ParalysisRemaining > 0) return default;
            if (samples > Definition.ReactionFrames && self.Position.X >= left && self.Position.X <= right &&
                (self.HitstunRemaining == sim.Rules.InputBufferFrames || self.Mode == FighterMode.Recovery && self.ActionFrames > 0 && self.ActionFrames <= sim.Rules.InputBufferFrames && !recoveryQueued) &&
                Math.Abs(seen.Position.X - self.Position.X) < Definition.MediumDistance &&
                attackRest == 0 && opponent.HeartRecoveryRemaining == 0 && Chance(Definition.AttackChance))
            {
                recoveryQueued = true;
                float range = Math.Abs(seen.Position.X - self.Position.X), height = seen.Position.Y - self.Position.Y;
                int facing = Math.Sign(seen.Position.X - self.Position.X);
                if (InRange(self.Definition.GroundNormals[0], range, height, opponent.Definition.HurtboxSize) &&
                    SafeAttack(self, safe, self.Definition.GroundNormals[0], facing)) return Action(facing, InputButtons.Attack);
                if (seen.Mode != FighterMode.Guard && self.SpecialCooldown(SpecialSlot.Neutral) == 0 && InRange(self.Definition.Special(SpecialSlot.Neutral), range, height, opponent.Definition.HurtboxSize))
                    return new FrameInput(facing, held: InputButtons.Special, pressed: InputButtons.Special, actionX: 0, actionY: 0);
            }
            if (self.HitstunRemaining > 0) return default;
            if (self.Position.X < left || self.Position.X > right || self.Position.Y < floor.Top)
            {
                guardRemaining = 0; cooldown = 0; movement = new FrameInput(inward);
                if (!self.CanControlMovement) return movement;
                bool falling = self.Velocity.Y <= 0;
                if (!self.Grounded && falling && self.JumpsUsed < self.Definition.MaxJumps && StaminaSystem.CanSpend(self, self.Definition.Stamina.AirJumpCost)) return Action(inward, InputButtons.Jump);
                if (!self.Grounded && !self.UpSpecialUsed && self.SpecialCooldown(SpecialSlot.Up) == 0 && falling)
                    return Action(inward, InputButtons.Special, 1);
                if (!self.Grounded && self.AirDashesUsed < self.Definition.MaxAirDashes && self.Position.Y > floor.Top + 1 &&
                    StaminaSystem.CanSpend(self, self.Definition.Stamina.AirDashCost) &&
                    (self.Position.X < floor.Left || self.Position.X > floor.Right)) return Action(inward, InputButtons.Dash);
                return movement;
            }
            if (guardRemaining > 0 && self.GuardLockRemaining == 0 && StaminaSystem.CanSpend(self, self.Definition.Stamina.SpecialGuardCost))
            { guardRemaining--; return new FrameInput(held: InputButtons.Guard); }
            guardRemaining = 0;
            float dx = seen.Position.X - self.Position.X, dy = seen.Position.Y - self.Position.Y;
            int toward = Math.Sign(dx); float distance = Math.Abs(dx);
            if (self.Attack != null)
            {
                if (self.Attack.Definition.RequiresFollowup && self.Attack.Frame >= self.Attack.Definition.StartupFrames - 2)
                    return Action(toward, InputButtons.Attack);
                if (self.Attack.Definition.IsNormal && !self.Attack.NextQueued && queuedChain != self.Attack.Id &&
                    self.Attack.Frame >= self.Attack.Definition.StartupFrames)
                {
                    queuedChain = self.Attack.Id;
                    if (distance <= Definition.CloseDistance + 1 && SafeNormalChain(self, safe, sim.Rules.MaxComboHits) && Chance(Definition.ComboChance))
                        return Action(toward, InputButtons.Attack);
                }
                return default;
            }
            if (!self.CanControlMovement) return default;
            if (--cooldown > 0) return movement;
            cooldown = Definition.DecisionFrames;
            movement = new FrameInput(toward);
            if (samples <= Definition.ReactionFrames) return default;
            if (seen.Attacking && Threatens(seen, self, Definition.MediumDistance))
            {
                int impact = seen.Attack.StartupFrames + seen.Attack.DamageDelayFrames - seen.AttackFrame;
                if (seen.Attack.IsNormal && seen.Attack.IsCloseRange && self.CounterCooldownRemaining == 0 &&
                    distance <= Definition.CloseDistance + 1 && Chance(Definition.CounterChance)) return Action(toward, InputButtons.Counter);
                bool justTiming = impact > 0 && impact <= sim.Rules.Defense.JustWindowFrames;
                if (seen.Attack.IsUltimate)
                {
                    if (self.GuardLockRemaining == 0 && (justTiming && Chance(Definition.JustChance) || StaminaSystem.CanSpend(self, self.Definition.Stamina.UltimateGuardCost) && Chance(Definition.GuardChance)))
                    { guardRemaining = Math.Max(1, impact + seen.Attack.ActiveFrames); return Action(toward, InputButtons.Guard); }
                }
                else if (!seen.Attack.BreaksGuard && self.GuardLockRemaining == 0 && StaminaSystem.CanSpend(self, StaminaSystem.GuardCost(self, seen.Attack.Kind)) &&
                    Chance(justTiming ? Definition.GuardChance + (1 - Definition.GuardChance) * Definition.JustChance : Definition.GuardChance))
                {
                    int flight = seen.Attack.SpawnProjectile ? (int)Math.Ceiling(distance / seen.Attack.ProjectileSpeed * 60) : 0;
                    guardRemaining = Math.Max(Definition.DecisionFrames, impact + seen.Attack.ActiveFrames + flight);
                    return Action(toward, InputButtons.Guard);
                }
                if (!seen.Attack.IsUltimate && Chance(Definition.DodgeChance) && SafeMotion(self, safe, -toward * sim.Rules.Defense.GroundDodgeSpeed, sim.Rules.Defense.DodgeInvulnerableFrames))
                    return Action(-toward, InputButtons.Dodge);
            }
            // Reposition after committing to an attack, and respect the opponent's visible
            // recovery blink. Defense above remains available throughout this spacing phase.
            if (attackRest > 0 || opponent.HeartRecoveryRemaining > 0)
            {
                int direction = distance < Definition.MediumDistance - 1 ? -toward : distance > Definition.MediumDistance + 1 ? toward : 0;
                movement = SafeMotion(self, safe, direction * self.Definition.MoveSpeed, Definition.DecisionFrames) ? new FrameInput(direction) : default;
                return movement;
            }
            if (Definition.WhiffApproachAttempts > 0 && consecutiveWhiffs >= Definition.WhiffApproachAttempts && distance > Definition.CloseDistance && self.Grounded && System.Math.Abs(dy) < 2)
                return new FrameInput(toward);
            if (GaugeSystem.CanUseUltimate(self, sim.Rules) && InRange(self.Definition.Ultimate, distance, dy, opponent.Definition.HurtboxSize) &&
                SafeAttack(self, safe, self.Definition.Ultimate, toward)) return Action(toward, InputButtons.Ultimate);
            if (self.Grounded && self.GroundPlatform > 0 && dy < -2) return new FrameInput(toward, -1);
            if (dy > 2 && self.JumpsUsed < self.Definition.MaxJumps && (self.Grounded || self.Velocity.Y <= 0) &&
                StaminaSystem.CanSpend(self, self.Definition.Stamina.AirJumpCost + self.Definition.Stamina.AirDashCost)) return Action(toward, InputButtons.Jump);
            var normals = self.Grounded ? self.Definition.GroundNormals : self.Definition.AirNormals;
            bool close = InRange(normals[0], distance, dy, opponent.Definition.HurtboxSize);
            if (seen.Mode == FighterMode.Counter && Chance(Definition.CounterChance))
            {
                var neutral = self.Definition.Special(SpecialSlot.Neutral);
                if (self.SpecialCooldown(SpecialSlot.Neutral) == 0 && InRange(neutral, distance, dy, opponent.Definition.HurtboxSize))
                    return new FrameInput(toward, held: InputButtons.Special, pressed: InputButtons.Special, actionX: 0, actionY: 0);
                movement = new FrameInput(self.Hearts <= opponent.Hearts && distance < Definition.CloseDistance ? -toward : 0);
                return movement;
            }
            if (seen.Mode == FighterMode.Guard && close &&
                SafeAttack(self, safe, DirectionalAttackSelector.Attack(self.Definition, self.Grounded ? NormalDirection.Crouch : NormalDirection.Weak, 0, !self.Grounded), toward) &&
                Chance(Definition.AttackChance)) return Action(toward, InputButtons.Attack, self.Grounded ? -1 : 0);
            if (seen.Mode != FighterMode.Guard && Chance(Definition.SpecialChance))
            {
                SpecialSlot slot = ChooseSpecial(self, seen, distance, dy);
                var special = self.Definition.Special(slot);
                if (self.SpecialCooldown(slot) == 0 && (slot != SpecialSlot.Up || !self.UpSpecialUsed && self.Grounded) &&
                    (special.NonDamaging || InRange(special, distance, dy, opponent.Definition.HurtboxSize)) && SafeAttack(self, safe, special, toward))
                    return new FrameInput(toward, held: InputButtons.Special, pressed: InputButtons.Special,
                        actionX: slot == SpecialSlot.Neutral ? 0 : toward, actionY: slot == SpecialSlot.Up ? 1 : slot == SpecialSlot.Down ? -1 : 0);
            }
            if (close && SafeAttack(self, safe, normals[0], toward) && Chance(Definition.AttackChance)) return Action(toward, InputButtons.Attack);
            // A safe dash must also be short enough not to overshoot the opponent.
            float dashTravel = self.Definition.DashSpeed * self.DashMultiplier * self.Definition.GroundDashFrames / 60f;
            if (self.Mode != FighterMode.Dash && self.Grounded && StaminaSystem.CanSpend(self, self.Definition.Stamina.GroundDashCost + self.Definition.Stamina.AirJumpCost) &&
                distance > dashTravel + Definition.CloseDistance && SafeMotion(self, safe, toward * self.Definition.DashSpeed * self.DashMultiplier, self.Definition.GroundDashFrames))
                return Action(toward, InputButtons.Dash);
            if (self.Grounded && distance > Definition.MediumDistance && Chance(Definition.JumpChance)) return Action(toward, InputButtons.Jump);
            if (close) movement = default;
            return movement;
        }
        private static bool Threatens(Observation seen, FighterState self, float medium) =>
            Math.Abs(seen.Position.X - self.Position.X) < medium && Math.Abs(seen.Position.Y - self.Position.Y) < 3 &&
            seen.AttackFrame < seen.Attack.StartupFrames + seen.Attack.ActiveFrames &&
            (seen.Position.X - self.Position.X) * seen.Facing <= 0;
        private SpecialSlot ChooseSpecial(FighterState self, Observation seen, float distance, float dy)
        {
            if (dy > 1.8f) return SpecialSlot.Up;
            var down = self.Definition.Special(SpecialSlot.Down);
            if (down.SelfHealNormalHits > 0 && self.NormalHitCount > 0 && distance > Definition.MediumDistance) return SpecialSlot.Down;
            if (down.ProjectileEraseCount != 0 && seen.Attack.SpawnProjectile) return SpecialSlot.Down;
            if (down.RequiresFollowup && (seen.Mode == FighterMode.Guard || seen.Mode == FighterMode.Recovery)) return SpecialSlot.Down;
            if (distance < Definition.CloseDistance) return SpecialSlot.Side;
            return SpecialSlot.Neutral;
        }
        private bool SafeAttack(FighterState self, BattleRect floor, AttackDefinition attack, int facing)
        {
            int direction = self.Grounded ? facing : self.Facing;
            float windup = attack.NormalForwardSpeed * attack.StartupFrames / BattleRuleDefinition.TickRate;
            float total = (attack.IsNormal ? attack.NormalForwardDistance : 0) + attack.ActiveVelocity.X * attack.ActiveFrames / BattleRuleDefinition.TickRate;
            return SafeMotion(self, floor, windup * direction * BattleRuleDefinition.TickRate, 1) &&
                SafeMotion(self, floor, total * direction * BattleRuleDefinition.TickRate, 1);
        }
        private bool SafeNormalChain(FighterState self, BattleRect floor, int maxComboHits)
        {
            var current = self.Attack;
            var normals = current.AirborneAttack ? self.Definition.AirNormals : self.Definition.GroundNormals;
            int next = current.ChainIndex + 1;
            if (next >= Math.Min(maxComboHits, normals.Length)) return false;
            var attack = DirectionalAttackSelector.Attack(self.Definition, current.Definition.NormalDirection, next, current.AirborneAttack);
            int remaining = Math.Max(0, current.Definition.StartupFrames + current.Definition.ActiveFrames - current.Frame - 1);
            float travel = remaining * (current.Definition.NormalForwardSpeed + current.Definition.ActiveVelocity.X) / BattleRuleDefinition.TickRate;
            travel += attack.NormalForwardDistance + attack.ActiveVelocity.X * attack.ActiveFrames / BattleRuleDefinition.TickRate;
            return SafeMotion(self, floor, travel * self.Facing * BattleRuleDefinition.TickRate, 1);
        }
        private bool SafeMotion(FighterState self, BattleRect floor, float speed, int frames)
        {
            float end = self.Position.X + speed * frames / 60f;
            return end > floor.Left + Definition.EdgeMargin && end < floor.Right - Definition.EdgeMargin;
        }
        private static bool InRange(AttackDefinition attack, float dx, float dy, BattleVector target)
        {
            float reach = Math.Abs(attack.Hitbox.Center.X) + attack.Hitbox.Size.X * .5f + target.X * .5f;
            reach += Math.Abs(attack.ActiveVelocity.X) * attack.ActiveFrames / 60f;
            if (attack.IsNormal) reach += attack.NormalForwardDistance;
            if (attack.SpawnProjectile) reach += attack.ProjectileSpeed * attack.ProjectileLifetimeFrames / 60f;
            return dx < reach - .15f && Math.Abs(dy - attack.Hitbox.Center.Y) < (attack.Hitbox.Size.Y + target.Y) * .5f;
        }
        private static FrameInput Action(int x, InputButtons button, float y = 0) => new FrameInput(x, y, button, button);
    }
}
