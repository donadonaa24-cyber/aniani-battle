using System;
using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class NormalAdvanceTests
    {
        [TestCase(0, 1)] [TestCase(0, -1)] [TestCase(1, 1)] [TestCase(1, -1)] [TestCase(2, 1)] [TestCase(2, -1)]
        public void EveryRosterNormalAdvancesItsConfiguredDistanceAndRetainsExistingActiveMotion(int character, int facing)
        {
            var definition = PhaseSixADefaults.Character(character);
            foreach (var normal in definition.GroundNormals.Concat(definition.AirNormals).Concat(definition.DirectionalNormals.Skip(1)))
            {
                Assert.That(normal.NormalForwardDistance, Is.EqualTo(.18f), normal.Name);
                var single = definition; single.DirectionalNormals = null;
                single.GroundNormals = new[] { normal }; single.AirNormals = new[] { normal };
                var sim = BattleTestFactory.Create(definition: single);
                sim.P1.Facing = facing;
                sim.P1.Position = new BattleVector(0, 30); sim.P1.Grounded = false; sim.P1.GroundPlatform = -1;
                sim.P2.Position = new BattleVector(15, 1.1f);
                for (int tick = 0; tick < normal.TotalFrames; tick++)
                {
                    sim.Tick(tick == 0 ? new FrameInput(pressed: InputButtons.Attack) : default, default);
                    Assert.That(sim.P1.Attack.Frame, Is.EqualTo(tick));
                    int movingTicks = Math.Min(tick + 1, normal.StartupFrames + normal.ActiveFrames);
                    int activeTicks = Math.Max(0, Math.Min(tick - normal.StartupFrames + 1, normal.ActiveFrames));
                    float expected = facing * (movingTicks * normal.NormalForwardDistance / (normal.StartupFrames + normal.ActiveFrames) +
                        activeTicks * normal.ActiveVelocity.X / BattleRuleDefinition.TickRate);
                    Assert.That(sim.P1.Position.X, Is.EqualTo(expected).Within(.00002f), normal.Name + " tick " + tick);
                    if (sim.P1.Attack.IsActive)
                        Assert.That(sim.P1.Velocity.X, Is.EqualTo(facing * (normal.NormalForwardSpeed + normal.ActiveVelocity.X)).Within(.00001f));
                    if (tick >= normal.StartupFrames + normal.ActiveFrames) Assert.That(sim.P1.Velocity.X, Is.Zero);
                }
                sim.Tick(default, default);
                Assert.That(sim.P1.Attack, Is.Null);
                Assert.That(sim.P1.Velocity.X, Is.Zero);
            }
        }

        [TestCase(0, 1)] [TestCase(0, -1)] [TestCase(1, 1)] [TestCase(1, -1)] [TestCase(2, 1)] [TestCase(2, -1)]
        public void ThreeGroundNormalsAccumulateThreeStepsWithoutChangingFacingOrTiming(int character, int facing)
        {
            var definition = PhaseSixADefaults.Character(character);
            var sim = BattleTestFactory.Create(definition: definition);
            sim.P1.Position = new BattleVector(0, 1.1f); sim.P1.Facing = facing; sim.P2.Position = new BattleVector(15, 1.1f);
            int[] frames = new int[3]; long lastId = 0; var starts = new List<float>();
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            while (sim.P1.Attack != null)
            {
                var attack = sim.P1.Attack;
                if (attack.Id != lastId) { lastId = attack.Id; starts.Add(sim.P1.Position.X); }
                frames[attack.ChainIndex]++;
                Assert.That(sim.P1.Facing, Is.EqualTo(facing));
                bool queue = attack.ChainIndex < 2 && attack.Frame == attack.Definition.StartupFrames;
                sim.Tick(queue ? new FrameInput(-facing, pressed: InputButtons.Attack) : default, default);
            }
            Assert.That(starts.Count, Is.EqualTo(3));
            for (int i = 0; i < 3; i++) Assert.That(frames[i], Is.EqualTo(definition.GroundNormals[i].TotalFrames));
            Assert.That(sim.P1.Position.X, Is.EqualTo(.54f * facing).Within(.00002f));
        }

        [Test]
        public void HitstopFreezesFrameAndPositionThenResumesTheSameStep()
        {
            var sim = BattleTestFactory.Create(adjacent: true);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            while (sim.P1.HitstopRemaining == 0) sim.Tick(default, default);
            float before = sim.P1.Position.X; int frame = sim.P1.Attack.Frame, frozen = sim.P1.HitstopRemaining;
            for (int i = 0; i < frozen; i++)
            {
                sim.Tick(default, default);
                Assert.That(sim.P1.IsFrozenThisTick, Is.True);
                Assert.That(sim.P1.Position.X, Is.EqualTo(before)); Assert.That(sim.P1.Attack.Frame, Is.EqualTo(frame));
            }
            sim.Tick(default, default);
            Assert.That(sim.P1.Position.X, Is.GreaterThan(before)); Assert.That(sim.P1.Attack.Frame, Is.EqualTo(frame + 1));
            while (sim.P1.Attack != null) sim.Tick(default, default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(.18f).Within(.00002f));
        }

        [Test]
        public void DamageInterruptsAdvanceWithoutLeavingAForwardVelocity()
        {
            var sim = BattleTestFactory.Create(adjacent: true);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            var attack = PhaseOneDefaults.Normal(); attack.Knockback = default; attack.HitstopFrames = 0; attack.HitstunFrames = 8;
            // Isolate interrupted forward motion; recoil itself is exercised by DamageRecoilTests.
            attack.DamageRecoilDistance = 0; attack.DamageRecoilFrames = 0;
            var incoming = new AttackInstance(900, attack, 0, false);
            new DamageSystem().Apply(new HitContact(sim.P2, sim.P1, incoming), new ComboTracker(), 1, sim.Rules, new List<BattleEvent>());
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.Velocity.X, Is.Zero);
            float before = sim.P1.Position.X;
            BattleTestFactory.Idle(sim, 10);
            Assert.That(sim.P1.Position.X, Is.EqualTo(before)); Assert.That(sim.P1.Attack, Is.Null);
        }

        [Test]
        public void FasterStartupBuffPreservesTheTotalStepDistance()
        {
            var sim = BattleTestFactory.Create(); sim.P1.Position = new BattleVector(0, 1.1f);
            sim.P1.Buff = new BuffDefinition { StartupMultiplier = .5f }; sim.P1.BuffRemaining = 100;
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(sim.P1.Attack.Definition.StartupFrames, Is.EqualTo(2));
            while (sim.P1.Attack != null) sim.Tick(default, default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(.18f).Within(.00002f));
        }

        [Test]
        public void SpecialsIgnoreTheNormalStepField()
        {
            var definition = PhaseOneDefaults.Character();
            var special = definition.Specials[0]; special.NormalForwardDistance = 2; definition.Specials[0] = special;
            var sim = BattleTestFactory.Create(definition: definition); float before = sim.P1.Position.X;
            sim.Tick(new FrameInput(pressed: InputButtons.Special), default);
            while (sim.P1.Attack != null) sim.Tick(default, default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(before));
        }

        [TestCase(1)] [TestCase(-1)]
        public void AdvancingAttackUsesSweptWallCollision(int facing)
        {
            var definition = PhaseOneDefaults.Character();
            var stage = new StageGeometry(new[] {
                new StagePlatform(new BattleRect(new BattleVector(0, -.6f), new BattleVector(36, 1.2f)), false),
                new StagePlatform(new BattleRect(new BattleVector(facing * 1.1f, 3), new BattleVector(1, 6)), false)
            }, -24, 24, -10, new BattleVector(0, 1.1f), new BattleVector(-facing * 5, 1.1f));
            var sim = new BattleSimulation(definition, definition, PhaseOneDefaults.Rules(), stage);
            sim.P1.Facing = facing; sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            while (sim.P1.Attack != null) sim.Tick(default, default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(facing * .05f).Within(.00002f));
        }

        [TestCase(1)] [TestCase(-1)]
        public void CpuRejectsAnOutwardStepNearTheSafeEdge(int facing)
        {
            var sim = BattleTestFactory.Create(); var cpu = CpuDefinition.ForDifficulty(1);
            cpu.ReactionFrames = 0; cpu.DecisionFrames = 1; cpu.AttackChance = 1;
            cpu.SpecialChance = cpu.CounterChance = cpu.JumpChance = 0;
            var brain = new CpuBrain(cpu, 2);
            sim.P1.Position = new BattleVector(facing * (18 - cpu.EdgeMargin - .1f), 1.1f); sim.P1.Facing = facing;
            sim.P2.Position = new BattleVector(sim.P1.Position.X + facing, 1.1f);
            brain.NextInput(sim, 0);
            Assert.That(brain.NextInput(sim, 0).IsPressed(InputButtons.Attack), Is.False);
        }

        [TestCase(-1f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidAdvanceDistancesAreRejected(float distance)
        {
            var attack = PhaseOneDefaults.Normal(); attack.NormalForwardDistance = distance;
            Assert.Throws<ArgumentException>(() => attack.Validate());
        }

        [Test]
        public void LandingCancelsAnAirNormalAndStopsItsStepDuringLandingRecovery()
        {
            var definition = PhaseSixADefaults.Character(0);
            var attack = definition.DirectionalNormals[(int)NormalDirection.AirForwardDown];
            var sim = BattleTestFactory.Create(definition: definition);
            sim.P1.Position = new BattleVector(0, 1.105f); sim.P1.Grounded = false; sim.P1.GroundPlatform = -1;
            sim.P1.Attack = new AttackInstance(1, attack, 0, true); sim.P1.Attack.Frame = attack.StartupFrames - 1; sim.P1.Mode = FighterMode.Attack;
            sim.Tick(default, default);
            Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery)); Assert.That(sim.P1.Velocity.X, Is.Zero);
            float landingX = sim.P1.Position.X;
            BattleTestFactory.Idle(sim, attack.LandingRecoveryFrames);
            Assert.That(sim.P1.Position.X, Is.EqualTo(landingX));
        }

        [TestCase(1)] [TestCase(-1)]
        public void AdvancingPastAPhysicalLedgeFallsInsteadOfHovering(int facing)
        {
            var sim = BattleTestFactory.Create(); sim.P1.Facing = facing;
            sim.P1.Position = new BattleVector(facing * (18 + .55f - .1f), 1.1f);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            BattleTestFactory.Idle(sim, 10);
            Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.GroundPlatform, Is.EqualTo(-1));
            Assert.That(sim.P1.Position.Y, Is.LessThan(1.1f));
        }

        [TestCase(1)] [TestCase(-1)]
        public void CpuRejectsAChainWhoseNextStepWouldCrossTheSafeEdge(int facing)
        {
            var sim = BattleTestFactory.Create(); var cpu = CpuDefinition.ForDifficulty(1);
            cpu.ReactionFrames = 0; cpu.DecisionFrames = 1; cpu.ComboChance = 1;
            var brain = new CpuBrain(cpu, 2);
            sim.P1.Position = new BattleVector(facing * (18 - cpu.EdgeMargin - .2f), 1.1f); sim.P1.Facing = facing;
            sim.P2.Position = new BattleVector(sim.P1.Position.X + facing, 1.1f);
            sim.P1.Attack = new AttackInstance(1, sim.P1.Definition.GroundNormals[0], 0, false);
            sim.P1.Attack.Frame = sim.P1.Attack.Definition.StartupFrames; sim.P1.Mode = FighterMode.Attack;
            Assert.That(brain.NextInput(sim, 0).IsPressed(InputButtons.Attack), Is.False);
            Assert.That(sim.P1.Attack.NextQueued, Is.False);
        }
    }
}
