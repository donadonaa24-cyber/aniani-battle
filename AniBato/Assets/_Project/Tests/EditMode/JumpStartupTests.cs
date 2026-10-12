using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class JumpStartupTests
    {
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void GroundJumpStaysPlantedForExactlyTwoTicksThenLaunchesAtOriginalSpeed(int character)
        {
            var sim = BattleTestFactory.Create(definition: PhaseSixADefaults.Character(character));
            var start = sim.P1.Position; float stamina = sim.P1.Stamina;
            Assert.That(sim.Rules.GroundJumpStartupFrames, Is.EqualTo(2));
            sim.Tick(new FrameInput(-1, pressed: InputButtons.Jump), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.JumpStartup)); Assert.That(sim.P1.JumpStartupRemaining, Is.EqualTo(2));
            Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.Position, Is.EqualTo(start)); Assert.That(sim.P1.JumpsUsed, Is.Zero);
            Assert.That(sim.P1.Velocity, Is.EqualTo(default(BattleVector))); Assert.That(sim.P1.Facing, Is.EqualTo(-1));
            Assert.That(sim.P1.Stamina, Is.EqualTo(stamina - sim.P1.Definition.Stamina.JumpCost));
            sim.Tick(new FrameInput(held: InputButtons.Jump), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.JumpStartup)); Assert.That(sim.P1.Position, Is.EqualTo(start));
            Assert.That(sim.P1.JumpStartupRemaining, Is.EqualTo(1)); Assert.That(sim.P1.Grounded, Is.True);
            sim.Tick(new FrameInput(held: InputButtons.Jump), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Jump)); Assert.That(sim.P1.Grounded, Is.False);
            Assert.That(sim.P1.JumpsUsed, Is.EqualTo(1)); Assert.That(sim.P1.JumpStartupRemaining, Is.Zero);
            Assert.That(sim.P1.Velocity.Y, Is.EqualTo(sim.P1.Definition.JumpSpeed - sim.P1.Definition.Gravity / 60).Within(.0001));
            Assert.That(sim.P1.Velocity.X, Is.EqualTo(-sim.P1.Definition.MoveSpeed));
            Assert.That(sim.P1.Stamina, Is.EqualTo(stamina - sim.P1.Definition.Stamina.JumpCost), "Charge once on anticipation, never again on takeoff.");
        }
        [Test]
        public void AirJumpRemainsImmediateAndGroundStartupCanBeConfiguredToZero()
        {
            var sim = BattleTestFactory.Create(); sim.P1.Position = new BattleVector(0, 8); sim.P1.Grounded = false; sim.P1.JumpsUsed = 1;
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Assert.That(sim.P1.JumpsUsed, Is.EqualTo(2)); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Jump)); Assert.That(sim.P1.JumpStartupRemaining, Is.Zero);
            Assert.That(sim.P1.Velocity.Y, Is.EqualTo(sim.P1.Definition.JumpSpeed - sim.P1.Definition.Gravity / 60).Within(.0001));
            var rules = PhaseOneDefaults.Rules(); rules.GroundJumpStartupFrames = 0;
            var ground = BattleTestFactory.Create(rules: rules); ground.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Assert.That(ground.P1.Grounded, Is.False); Assert.That(ground.P1.JumpsUsed, Is.EqualTo(1));
            rules.GroundJumpStartupFrames = -1; Assert.Throws<System.ArgumentException>(() => rules.Validate());
        }
        [Test]
        public void InputPressedDuringAnticipationSurvivesUntilAirborneAction()
        {
            var sim = BattleTestFactory.Create(); sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(sim.P1.Attack, Is.Null); sim.Tick(default, default);
            Assert.That(sim.P1.Grounded, Is.False); sim.Tick(default, default);
            Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P1.Attack.AirborneAttack, Is.True);
            Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(sim.P1.Definition.AirNormals[0].Name));
        }
        [Test]
        public void HitDuringAnticipationCancelsPendingTakeoff()
        {
            var sim = BattleTestFactory.Create(true);
            sim.Tick(default, new FrameInput(pressed: InputButtons.Attack)); BattleTestFactory.Idle(sim, 2);
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.JumpStartup)); sim.Tick(default, default);
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P1.HitstunRemaining, Is.GreaterThan(0));
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Hitstun));
            BattleTestFactory.Idle(sim, 20);
            Assert.That(sim.P1.JumpStartupRemaining, Is.Zero); Assert.That(sim.P1.JumpsUsed, Is.Zero); Assert.That(sim.P1.Grounded, Is.True);
        }
        [Test]
        public void PlatformRemovalDuringAnticipationCancelsInsteadOfGivingFreeAirJump()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 3706);
            sim.P1.Position = new BattleVector(-17, 1.1f);
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.JumpStartup)); Assert.That(sim.P1.Grounded, Is.True);
            sim.Tick(default, default);
            Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Fall));
            Assert.That(sim.P1.JumpStartupRemaining, Is.Zero); Assert.That(sim.P1.JumpsUsed, Is.EqualTo(1)); Assert.That(sim.P1.Velocity.Y, Is.LessThan(0));
        }
        [Test]
        public void RingOutDuringAnticipationCannotLaunch()
        {
            var sim = BattleTestFactory.Create(); sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            sim.P1.Position = new BattleVector(0, -20); sim.P1.Grounded = false;
            sim.Tick(default, default);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut)); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.KO));
            Assert.That(sim.P1.JumpStartupRemaining, Is.Zero); Assert.That(sim.P1.Velocity.Y, Is.LessThanOrEqualTo(0));
        }
        [Test]
        public void HitstopPausesAnticipationAndPresentationUsesShallowLandFrame()
        {
            var sim = BattleTestFactory.Create(); var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default); player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(player.Sample.State, Is.EqualTo(AnimationState.JumpStartup)); Assert.That(player.Sample.Age, Is.Zero);
            Assert.That(player.Sample.FrameIndex(2, 15, false, 0), Is.Zero);
            sim.P1.HitstopRemaining = 3;
            for (int i = 0; i < 3; i++)
            {
                sim.Tick(default, default); player.Advance(sim.P1, sim.Match, sim.P1.IsFrozenThisTick, false, false);
                Assert.That(sim.P1.JumpStartupRemaining, Is.EqualTo(2)); Assert.That(player.Sample.Age, Is.Zero);
            }
            sim.Tick(default, default); player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(player.Sample.Age, Is.EqualTo(1)); Assert.That(player.Sample.FrameIndex(2, 15, false, 0), Is.Zero);
            sim.Tick(default, default); player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Jump)); Assert.That(player.Sample.Age, Is.Zero);
        }
    }
}
