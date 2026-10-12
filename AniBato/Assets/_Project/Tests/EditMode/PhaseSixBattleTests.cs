using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class PhaseSixBattleTests
    {
        internal static BattleSimulation Create(bool adjacent = false, int character = 0) => BattleTestFactory.Create(adjacent, PhaseSixADefaults.Rules(), PhaseSixADefaults.Character(character));
        private static FrameInput Button(InputButtons button, float x = 0, float y = 0) => new FrameInput(x, y, button, button);
        [TestCase(0, 0, NormalDirection.AirNeutral)] [TestCase(1, 0, NormalDirection.AirForward)]
        [TestCase(-1, 0, NormalDirection.AirBack)] [TestCase(0, 1, NormalDirection.AirUp)]
        [TestCase(0, -1, NormalDirection.AirDown)] [TestCase(1, 1, NormalDirection.AirForwardUp)]
        [TestCase(1, -1, NormalDirection.AirForwardDown)]
        public void AerialDirectionsUseFacingAndNeverTurn(float x, float y, NormalDirection expected)
        {
            foreach (int facing in new[] { -1, 1 })
            {
                var sim = Create(); sim.P1.Grounded = false; sim.P1.Position = new BattleVector(0, 8); sim.P1.Facing = facing;
                sim.Tick(Button(InputButtons.Attack, x * facing, y), default);
                Assert.That(sim.P1.Attack.Definition.NormalDirection, Is.EqualTo(expected)); Assert.That(sim.P1.Facing, Is.EqualTo(facing));
                Assert.That(sim.P1.Attack.Definition.Kind, Is.EqualTo(AttackKind.Normal)); Assert.That(sim.P1.Attack.Definition.IsCloseRange, Is.True);
                var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
                Assert.That(player.Sample.State, Is.EqualTo((AnimationState)((int)AnimationState.AirNeutral + (int)expected - (int)NormalDirection.AirNeutral)));
            }
        }
        [TestCase(-1)] [TestCase(1)]
        public void GroundBackAndCrouchPrioritizeDownBeforeBack(int facing)
        {
            var sim = Create(); sim.P1.Facing = facing; sim.Tick(Button(InputButtons.Attack, -facing), default);
            Assert.That(sim.P1.Attack.Definition.NormalDirection, Is.EqualTo(NormalDirection.Back));
            Assert.That(sim.P1.Attack.WorldHitbox(sim.P1).Center.X * facing, Is.LessThan(sim.P1.Position.X * facing));
            var crouch = Create(); crouch.P1.Facing = facing; crouch.Tick(Button(InputButtons.Attack, -facing, -1), default);
            Assert.That(crouch.P1.Attack.Definition.NormalDirection, Is.EqualTo(NormalDirection.Crouch)); Assert.That(crouch.P1.Attack.Definition.BreaksGuard, Is.True);
        }
        [Test]
        public void CrouchKeepsFeetLowersHeadAndCannotMoveOrDropOnATap()
        {
            var sim = Create(); sim.P1.Position = new BattleVector(sim.Stage[1].Bounds.Center.X, sim.Stage[1].Bounds.Top + sim.P1.Definition.HurtboxSize.Y / 2); sim.P1.Grounded = true; sim.P1.GroundPlatform = 1;
            var before = sim.P1.Hurtbox; sim.Tick(new FrameInput(1, -1), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Crouch)); Assert.That(sim.P1.Position.X, Is.EqualTo(before.Center.X));
            Assert.That(sim.P1.Hurtbox.Bottom, Is.EqualTo(before.Bottom).Within(.001)); Assert.That(sim.P1.Hurtbox.Top, Is.LessThan(before.Top));
            Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.DropThroughRemaining, Is.Zero);
            for (int i = 1; i < 18; i++) sim.Tick(new FrameInput(y: -1), default);
            Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.DropThroughRemaining, Is.EqualTo(sim.Rules.DropThroughFrames));
        }
        [Test]
        public void DiagonalThresholdFallbackAndSnapshotAreDataDriven()
        {
            var c = PhaseSixADefaults.Character(0); c.DiagonalThreshold = .7f; c.DirectionalNormals[8] = default;
            var sim = BattleTestFactory.Create(definition: c); sim.P1.Grounded = false;
            Assert.That(DirectionalAttackSelector.Select(sim.P1, new BattleVector(1, 1), .3f), Is.EqualTo(NormalDirection.AirUp));
            c.DirectionalNormals[6] = default;
            Assert.That(sim.P1.Definition.DirectionalNormals[6].ActiveFrames, Is.GreaterThan(0));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void ForwardAirMovesAndDownLandingHasRecovery(int character)
        {
            var sim = Create(character: character); sim.P1.Position = new BattleVector(0, 8); sim.P1.Grounded = false;
            sim.Tick(Button(InputButtons.Attack, 1, 1), default); BattleTestFactory.Idle(sim, sim.P1.Attack.Definition.StartupFrames);
            Assert.That(sim.P1.Velocity.X, Is.GreaterThan(0)); Assert.That(sim.P1.Velocity.Y, Is.GreaterThan(0));
            var down = Create(character: character); down.P1.Position = new BattleVector(0, 1.2f); down.P1.Grounded = false; down.P1.Velocity = new BattleVector(0, -10);
            down.Tick(Button(InputButtons.Attack, y: -1), default);
            Assert.That(down.P1.Grounded, Is.True); Assert.That(down.P1.Mode, Is.EqualTo(FighterMode.Recovery)); Assert.That(down.P1.ActionFrames, Is.EqualTo(18));
        }
        [TestCase(InputButtons.Jump, 8)] [TestCase(InputButtons.Dash, 8)] [TestCase(InputButtons.Dodge, 18)]
        public void ActionsConsumeOnlyOnStart(InputButtons action, int cost)
        {
            var sim = Create(); sim.Tick(Button(action), default);
            Assert.That(sim.P1.Stamina, Is.EqualTo(100 - cost));
            sim.Tick(new FrameInput(held: action), default); Assert.That(sim.P1.Stamina, Is.EqualTo(100 - cost));
        }
        [TestCase(InputButtons.Dash)] [TestCase(InputButtons.Dodge)] [TestCase(InputButtons.Jump)]
        public void EmptyStaminaRejectsExpensiveAirActions(InputButtons button)
        {
            var sim = Create(); sim.P1.StaminaUnits = 0; sim.P1.Grounded = false; sim.P1.Position = new BattleVector(0, 8); sim.P1.JumpsUsed = 1;
            sim.Tick(Button(button), default); Assert.That(sim.P1.StaminaRejectedRemaining, Is.GreaterThan(0));
            Assert.That(sim.P1.JumpsUsed, Is.EqualTo(1)); Assert.That(sim.P1.AirDashesUsed, Is.Zero); Assert.That(sim.P1.Mode, Is.Not.EqualTo(FighterMode.Dodge));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void EmptyStaminaAllowsGroundJumpAndUpRecovery(int character)
        {
            var ground = Create(character: character); ground.P1.StaminaUnits = 0; ground.Tick(Button(InputButtons.Jump), default);
            Assert.That(ground.P1.Velocity.Y, Is.GreaterThan(0)); Assert.That(ground.P1.Grounded, Is.False);
            var air = Create(character: character); air.P1.StaminaUnits = 0; air.P1.Position = new BattleVector(19, -1); air.P1.Grounded = false; air.P1.JumpsUsed = 2;
            air.Tick(Button(InputButtons.Special, -1, 1), default); BattleTestFactory.Idle(air, 5);
            Assert.That(air.P1.Attack.Definition.Name, Is.EqualTo(air.P1.Definition.Special(SpecialSlot.Up).Name));
            Assert.That(air.P1.Velocity.Y, Is.GreaterThan(0)); Assert.That(air.Match.IsFinished, Is.False);
            for (int tick = 0; tick < 180 && !air.P1.Grounded; tick++)
            {
                air.Tick(new FrameInput(-1), default);
                Assert.That(air.Match.IsFinished, Is.False, "Empty stamina must not make the recovery impossible.");
            }
            Assert.That(air.P1.Grounded, Is.True, "Recovery must actually return to a platform.");
        }
        [Test]
        public void RecoveryHasExactDelayAndGroundRate()
        {
            var sim = Create(); StaminaSystem.Spend(sim.P1, 20); BattleTestFactory.Idle(sim, 30);
            Assert.That(sim.P1.Stamina, Is.EqualTo(80)); BattleTestFactory.Idle(sim, 30); Assert.That(sim.P1.Stamina, Is.EqualTo(96));
            BattleTestFactory.Idle(sim, 30); Assert.That(sim.P1.Stamina, Is.EqualTo(100));
        }
        [Test]
        public void RepeatedForwardUpNormalsCannotResetGravityEveryActiveTickAndGainHeightForever()
        {
            var rules = PhaseSixADefaults.Rules(); rules.EnableRingOut = false;
            var sim = BattleTestFactory.Create(rules: rules, definition: PhaseSixADefaults.Character(0));
            sim.P1.Position = new BattleVector(0, 8); sim.P1.Grounded = false; sim.P1.StaminaUnits = 0;
            float maximum = sim.P1.Position.Y;
            for (int tick = 0; tick < 600; tick++)
            { sim.Tick(Button(InputButtons.Attack, 1, 1), default); maximum = System.Math.Max(maximum, sim.P1.Position.Y); }
            Assert.That(maximum, Is.LessThan(9)); Assert.That(sim.P1.Position.Y, Is.LessThan(8));
        }
        private static HitContact Contact(BattleSimulation sim, AttackKind kind, bool crouch = false)
        {
            var a = crouch ? sim.P1.Definition.DirectionalNormals[1] : kind == AttackKind.Normal ? sim.P1.Definition.GroundNormals[0] : kind == AttackKind.Special ? sim.P1.Definition.Special(SpecialSlot.Side) : sim.P1.Definition.Ultimate;
            return new HitContact(sim.P1, sim.P2, new AttackInstance(100, a, 0, false));
        }
        [TestCase(AttackKind.Normal, 8)] [TestCase(AttackKind.Special, 20)] [TestCase(AttackKind.Ultimate, 65)]
        public void OrdinaryGuardBlocksEveryKindAndSpendsExactCost(AttackKind kind, int cost)
        {
            var sim = Create(); sim.P2.Mode = FighterMode.Guard; sim.P2.DefenseAge = 6;
            var defense = new DefenseSystem(); var c = Contact(sim, kind); Assert.That(defense.Evaluate(c, sim.Rules), Is.EqualTo(DefenseOutcome.Guard));
            defense.Apply(c, DefenseOutcome.Guard, sim.ComboP1, new DamageSystem(), 1, sim.Rules, new List<BattleEvent>());
            Assert.That(sim.P2.Stamina, Is.EqualTo(100 - cost)); Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
        }
        [TestCase(AttackKind.Normal)] [TestCase(AttackKind.Special)] [TestCase(AttackKind.Ultimate)]
        public void EmptyStaminaBreaksNonUltimateWithoutDamageAndUltimateHitsTwo(AttackKind kind)
        {
            var sim = Create(); sim.P2.Mode = FighterMode.Guard; sim.P2.DefenseAge = 6; sim.P2.StaminaUnits = 0;
            var c = Contact(sim, kind); var defense = new DefenseSystem(); var outcome = defense.Evaluate(c, sim.Rules);
            var events = new List<BattleEvent>();
            if (outcome == DefenseOutcome.Hit) new DamageSystem().Apply(c, sim.ComboP1, 1, sim.Rules, events);
            else defense.Apply(c, outcome, sim.ComboP1, new DamageSystem(), 1, sim.Rules, events);
            Assert.That(sim.P2.Hearts, Is.EqualTo(kind == AttackKind.Ultimate ? 1 : 3)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.P2.StaminaRejectedRemaining, Is.GreaterThan(0));
            if (kind != AttackKind.Ultimate) { Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Recovery)); Assert.That(sim.P2.GuardLockRemaining, Is.EqualTo(36)); Assert.That(events.Any(e => e.Kind == BattleEventKind.GuardBroken), Is.True); }
        }
        [TestCase(0)] [TestCase(6)]
        public void CrouchBreaksEvenFreshGuardAndAddsExactlyOne(int age)
        {
            var sim = Create(); sim.P2.Mode = FighterMode.Guard; sim.P2.DefenseAge = age; var c = Contact(sim, AttackKind.Normal, true);
            var defense = new DefenseSystem(); Assert.That(defense.Evaluate(c, sim.Rules), Is.EqualTo(DefenseOutcome.CrouchBreak));
            defense.Apply(c, DefenseOutcome.CrouchBreak, sim.ComboP1, new DamageSystem(), 1, sim.Rules, new List<BattleEvent>());
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.GuardLockRemaining, Is.EqualTo(36));
            sim.P2.Mode = FighterMode.Counter; Assert.That(defense.Evaluate(c, sim.Rules), Is.EqualTo(DefenseOutcome.Counter));
        }
        [Test]
        public void JustGuardWithEmptyStaminaStillBlocksUltimateWithoutCost()
        {
            var sim = Create(); sim.P2.StaminaUnits = 0; sim.P2.Mode = FighterMode.Guard; var c = Contact(sim, AttackKind.Ultimate);
            var defense = new DefenseSystem(); Assert.That(defense.Evaluate(c, sim.Rules), Is.EqualTo(DefenseOutcome.JustGuard));
            defense.Apply(c, DefenseOutcome.JustGuard, sim.ComboP1, new DamageSystem(), 1, sim.Rules, new List<BattleEvent>());
            Assert.That(sim.P2.Stamina, Is.Zero); Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.Gauge, Is.EqualTo(sim.Rules.Gauge.OnJustGuard));
        }
        [Test]
        public void PracticeFreezesHeartsTimeAndRescuesRingOutButCanBeDisabled()
        {
            var sim = Create(); sim.Practice = new PracticeOptions(); sim.P1.Hearts = 0; sim.P1.Position = new BattleVector(30, -12); sim.Tick(default, default);
            Assert.That(sim.P1.Hearts, Is.EqualTo(3)); Assert.That(sim.Stage.IsOutside(sim.P1.Position), Is.False); Assert.That(sim.Match.RemainingTicks, Is.EqualTo(4200)); Assert.That(sim.Match.IsFinished, Is.False);
            sim.Practice.FreezeHearts = sim.Practice.FreezeTimer = false; sim.P1.Position = new BattleVector(30, -12); sim.Tick(default, default);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut)); Assert.That(sim.Match.RemainingTicks, Is.EqualTo(4199));
        }
        [TestCase(1, 0, 23, 1)] [TestCase(1, 1, 28, 1)] [TestCase(1, 1, 21, 2)] [TestCase(2, 0, 1, 2)] [TestCase(2, 1, 5, 2)]
        public void CpuKeepsRecoveryResourcesAndNeverRushesOffStageWithOldFacing(int first, int second, int seed, int difficulty)
        {
            var row = CpuMatchMetrics.Run(PhaseSixADefaults.Character(first), PhaseSixADefaults.Character(second), PhaseSixADefaults.Rules(), PhaseOneDefaults.Stage(), CpuDefinition.ForDifficulty(difficulty), seed);
            Assert.That(row.UnforcedRingOuts, Is.Zero); Assert.That(row.Attacks[0], Is.GreaterThan(0)); Assert.That(row.Attacks[1], Is.GreaterThan(0));
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void StaminaAndGuardEventsAreIdenticalAcrossRenderRates(int fps)
        {
            var a = Create(); var b = Create(); var ea = new List<BattleEvent>(); var eb = new List<BattleEvent>(); var clock = new FixedTickClock();
            var ca = new CpuBrain(CpuDefinition.ForDifficulty(1), 41); var cb = new CpuBrain(CpuDefinition.ForDifficulty(1), 41);
            var da = new CpuBrain(CpuDefinition.ForDifficulty(1), 93); var db = new CpuBrain(CpuDefinition.ForDifficulty(1), 93);
            for (int frame = 0; frame < fps * 70; frame++) clock.Advance(1d / fps, () => { a.Tick(ca.NextInput(a, 0), da.NextInput(a, 1)); ea.AddRange(a.Events); });
            for (int tick = 0; tick < 4200; tick++) { b.Tick(cb.NextInput(b, 0), db.NextInput(b, 1)); eb.AddRange(b.Events); }
            Assert.That(ea, Is.EqualTo(eb)); Assert.That(a.P1.Position, Is.EqualTo(b.P1.Position)); Assert.That(a.P2.Position, Is.EqualTo(b.P2.Position));
            Assert.That(a.P1.StaminaUnits, Is.EqualTo(b.P1.StaminaUnits)); Assert.That(a.P2.StaminaUnits, Is.EqualTo(b.P2.StaminaUnits)); Assert.That(a.Match.Result, Is.EqualTo(b.Match.Result));
        }
    }
}
