using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class MovementTests
    {
        [Test]
        public void SustainedGroundDashTraverses36UnitsInAboutTwoSeconds()
        {
            var sim = BattleTestFactory.Create(); float start = sim.P1.Position.X;
            for (int i = 0; i < 60; i++) sim.Tick(new FrameInput(1, pressed: i == 0 ? InputButtons.Dash : InputButtons.None), default);
            Assert.That(sim.P1.Position.X - start, Is.EqualTo(17).Within(0.001));
            Assert.That(36f / (sim.P1.Position.X - start), Is.InRange(2, 3));
        }
        [Test]
        public void DoubleJumpAirDashAndLandingResetAirResources()
        {
            var sim = BattleTestFactory.Create();
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default); Assert.That(sim.P1.JumpsUsed, Is.EqualTo(1));
            BattleTestFactory.Idle(sim, 5); sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Assert.That(sim.P1.JumpsUsed, Is.EqualTo(2));
            sim.Tick(new FrameInput(1, pressed: InputButtons.Dash), default); Assert.That(sim.P1.AirDashesUsed, Is.EqualTo(1));
            BattleTestFactory.Idle(sim, 100);
            Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.JumpsUsed, Is.Zero); Assert.That(sim.P1.AirDashesUsed, Is.Zero);
        }
        [Test]
        public void UpperPlatformPassesUpwardAndCatchesFallingFighter()
        {
            var source = PhaseOneDefaults.Stage();
            var platforms = new StagePlatform[source.PlatformCount]; for (int i = 0; i < platforms.Length; i++) platforms[i] = source[i];
            var stage = new StageGeometry(platforms, -24, 24, -10, new BattleVector(-8, 1.1f), source.SpawnP2);
            var character = PhaseOneDefaults.Character(); var rules = PhaseOneDefaults.Rules();
            var sim = new BattleSimulation(character, character, rules, stage);
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            bool above = false;
            for (int i = 0; i < 60; i++)
            { sim.Tick(new FrameInput(pressed: i == 10 ? InputButtons.Jump : InputButtons.None), default); above |= sim.P1.Position.Y > stage[1].Bounds.Top + character.HurtboxSize.Y * 0.5f; }
            Assert.That(above, Is.True); Assert.That(sim.P1.GroundPlatform, Is.EqualTo(1));
            sim.Tick(new FrameInput(y: -1), default); BattleTestFactory.Idle(sim, 8);
            Assert.That(sim.P1.Position.Y, Is.LessThan(stage[1].Bounds.Top));
            BattleTestFactory.Idle(sim, 40); Assert.That(sim.P1.GroundPlatform, Is.Zero);
        }
        [Test]
        public void DownInAirFastFalls()
        {
            var sim = BattleTestFactory.Create(); sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            BattleTestFactory.Idle(sim, 6);
            sim.Tick(new FrameInput(y: -1), default);
            Assert.That(sim.P1.Velocity.Y, Is.EqualTo(-36));
        }
    }
}
