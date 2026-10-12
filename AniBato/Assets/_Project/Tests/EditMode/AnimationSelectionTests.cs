using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class AnimationSelectionTests
    {
        private static BattleSimulation Create() => new BattleSimulation(PhaseThreeDefaults.Character(2), PhaseThreeDefaults.Character(0), PhaseOneDefaults.Rules(), PhaseOneDefaults.Stage());
        [TestCase(FighterMode.Idle, AnimationState.Idle)]
        [TestCase(FighterMode.Move, AnimationState.Move)]
        [TestCase(FighterMode.Dash, AnimationState.Dash)]
        [TestCase(FighterMode.Jump, AnimationState.Jump)]
        [TestCase(FighterMode.Fall, AnimationState.Fall)]
        [TestCase(FighterMode.Guard, AnimationState.Guard)]
        [TestCase(FighterMode.Dodge, AnimationState.Dodge)]
        [TestCase(FighterMode.Counter, AnimationState.Counter)]
        [TestCase(FighterMode.Hitstun, AnimationState.Hitstun)]
        [TestCase(FighterMode.Knockback, AnimationState.Knockback)]
        [TestCase(FighterMode.KO, AnimationState.KO)]
        public void ModesSelectTheirOwnClips(FighterMode mode, AnimationState expected)
        { var sim = Create(); sim.P1.Mode = mode; Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo(expected)); }
        [TestCase(false, 0, AnimationState.Attack1)]
        [TestCase(false, 1, AnimationState.Attack2)]
        [TestCase(false, 2, AnimationState.Attack3)]
        [TestCase(true, 0, AnimationState.AirAttack)]
        [TestCase(true, 1, AnimationState.AirAttack)]
        [TestCase(true, 2, AnimationState.AirAttack)]
        public void NormalChainsAndAirNormalsSelectCorrectClip(bool air, int chain, AnimationState state)
        {
            var sim = Create(); sim.P1.Attack = new AttackInstance(1, (air ? sim.P1.Definition.AirNormals : sim.P1.Definition.GroundNormals)[chain], chain, air);
            Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo(state));
        }
        [TestCase(SpecialSlot.Neutral, AnimationState.SpecialNeutral)]
        [TestCase(SpecialSlot.Side, AnimationState.SpecialSide)]
        [TestCase(SpecialSlot.Up, AnimationState.SpecialUp)]
        [TestCase(SpecialSlot.Down, AnimationState.SpecialDown)]
        public void SpecialSlotsSelectCorrectClip(SpecialSlot slot, AnimationState state)
        {
            var sim = Create(); sim.P1.CurrentSpecial = slot; sim.P1.Attack = new AttackInstance(1, sim.P1.Definition.Special(slot), 0, false);
            Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo(state));
        }
        [Test]
        public void UltimateOverridesSpecialSlot()
        {
            var sim = Create(); sim.P1.CurrentSpecial = SpecialSlot.Down; sim.P1.Attack = new AttackInstance(1, sim.P1.Definition.Ultimate, 0, false);
            Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo(AnimationState.Ultimate));
        }
        [Test]
        public void AttackPosesFollowEveryStartupActiveAndRecoveryTick()
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
            var definition = sim.P1.Definition.GroundNormals[0]; var attack = new AttackInstance(8, definition, 0, false); sim.P1.Attack = attack;
            for (int frame = 0; frame < definition.TotalFrames; frame++)
            {
                attack.Frame = frame; player.Advance(sim.P1, sim.Match, false, false, false);
                int pose = player.Sample.FrameIndex(3, 10, false, 1);
                Assert.That(pose, Is.EqualTo(frame < definition.StartupFrames ? 0 : frame < definition.StartupFrames + definition.ActiveFrames ? 1 : 2), "attack frame " + frame);
            }
        }
        [Test]
        public void LoopingIdleAndNonLoopingLandingClampAtTheirOwnEnd()
        {
            var sample = new AnimationSample { Age = 60, State = AnimationState.Idle };
            Assert.That(sample.FrameIndex(4, 8, true, 0), Is.Zero);
            Assert.That(sample.FrameIndex(2, 15, false, 0), Is.EqualTo(1));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void HitstopFreezesAgePoseAndFacingEvenOnLastFrozenTick(int character)
        {
            var sim = new BattleSimulation(PhaseThreeDefaults.Character(character), PhaseThreeDefaults.Character((character + 1) % 3), PhaseOneDefaults.Rules(), PhaseOneDefaults.Stage());
            var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
            var sample = player.Sample; sim.P1.Facing = -1; sim.P1.Mode = FighterMode.Move;
            for (int tick = 0; tick < 8; tick++) { player.Advance(sim.P1, sim.Match, true, false, false); Assert.That(player.Sample.Age, Is.EqualTo(sample.Age)); Assert.That(player.Sample.State, Is.EqualTo(sample.State)); Assert.That(player.Sample.FlipX, Is.EqualTo(sample.FlipX)); }
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.FlipX, Is.True); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Move));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void AllPlayableDefinitionsSelectTheirAttackSlotsAndActivePoses(int character)
        {
            var sim = new BattleSimulation(PhaseThreeDefaults.Character(character), PhaseThreeDefaults.Character((character + 1) % 3), PhaseOneDefaults.Rules(), PhaseOneDefaults.Stage());
            var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
            for (int chain = 0; chain < 3; chain++)
            {
                sim.P1.Attack = new AttackInstance(chain + 1, sim.P1.Definition.GroundNormals[chain], chain, false);
                Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo((AnimationState)((int)AnimationState.Attack1 + chain)));
                sim.P1.Attack.Frame = sim.P1.Attack.Definition.StartupFrames;
                player.Advance(sim.P1, sim.Match, false, false, false);
                Assert.That(player.Sample.FrameIndex(chain == 2 ? 4 : 3, 10, false, 1), Is.EqualTo(1));
                sim.P1.Attack = new AttackInstance(chain + 10, sim.P1.Definition.AirNormals[chain], chain, true);
                Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo(AnimationState.AirAttack));
            }
            foreach (SpecialSlot slot in System.Enum.GetValues(typeof(SpecialSlot)))
            {
                sim.P1.CurrentSpecial = slot; sim.P1.Attack = new AttackInstance(20 + (int)slot, sim.P1.Definition.Special(slot), 0, false);
                Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo((AnimationState)((int)AnimationState.SpecialNeutral + (int)slot)));
            }
            sim.P1.Attack = new AttackInstance(30, sim.P1.Definition.Ultimate, 0, false);
            Assert.That(FighterAnimationPlayer.StateFor(sim.P1), Is.EqualTo(AnimationState.Ultimate));
        }
        [Test]
        public void CinematicTicksFreezeAttackSample()
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
            sim.P1.Attack = new AttackInstance(8, sim.P1.Definition.Ultimate, 0, false);
            player.Advance(sim.P1, sim.Match, false, false, false); var sample = player.Sample;
            for (int tick = 0; tick < 30; tick++) player.Advance(sim.P1, sim.Match, false, true, false);
            Assert.That(player.Sample.Age, Is.EqualTo(sample.Age)); Assert.That(player.Sample.AttackFrame, Is.EqualTo(sample.AttackFrame));
        }
        [Test]
        public void GroundContactPlaysEightTickLandingAndActionTakesPriority()
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); sim.P1.Grounded = false; sim.P1.Mode = FighterMode.Fall; player.Reset(sim.P1, sim.Match);
            sim.P1.Grounded = true; sim.P1.Mode = FighterMode.Idle;
            for (int i = 0; i < 8; i++) { player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Land)); }
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Idle));
            sim.P1.Grounded = false; player.Advance(sim.P1, sim.Match, false, false, false);
            sim.P1.Grounded = true; sim.P1.Attack = new AttackInstance(5, sim.P1.Definition.GroundNormals[0], 0, false);
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Attack1));
        }
        [Test]
        public void CounterWaitHoldsReadyPoseAndSuccessImmediatelyShowsParry()
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); sim.P1.Mode = FighterMode.Counter; player.Reset(sim.P1, sim.Match);
            for (int i = 0; i < 12; i++) { player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.FrameIndex(3, 12, false, 0), Is.Zero); }
            sim.P1.Mode = FighterMode.Recovery; player.Advance(sim.P1, sim.Match, false, false, true);
            Assert.That(player.Sample.State, Is.EqualTo(AnimationState.CounterSuccess)); Assert.That(player.Sample.FrameIndex(3, 12, false, 1), Is.EqualTo(1));
        }
        [Test]
        public void MatchResultPlaysWinnerAndThenLoserRecoveryWithoutAdvancingSimulation()
        {
            var sim = Create(); var winner = new FighterAnimationPlayer(); var loser = new FighterAnimationPlayer(); winner.Reset(sim.P1, sim.Match); loser.Reset(sim.P2, sim.Match);
            sim.Match.Result = MatchResult.Player1Wins; sim.P2.Mode = FighterMode.KO; int tick = sim.Match.TickCount;
            for (int i = 0; i < 25; i++) { winner.Advance(sim.P1, sim.Match, false, false, false); loser.Advance(sim.P2, sim.Match, false, false, false); }
            Assert.That(winner.Sample.State, Is.EqualTo(AnimationState.Win)); Assert.That(winner.Sample.Age, Is.EqualTo(24));
            Assert.That(loser.Sample.State, Is.EqualTo(AnimationState.Lose)); Assert.That(sim.Match.TickCount, Is.EqualTo(tick));
        }
        [Test]
        public void RestartResetsAllPresentationTimers()
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match);
            player.Advance(sim.P1, sim.Match, false, false, true); player.Reset(sim.P1, sim.Match);
            Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Idle)); Assert.That(player.Sample.Age, Is.Zero);
        }
        [Test]
        public void AssetTimingControlsLandingDuration()
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); player.Configure(new AnimationTiming { LandingTicks = 3, CounterSuccessTicks = 6, KnockoutTicks = 12 });
            sim.P1.Grounded = false; player.Reset(sim.P1, sim.Match); sim.P1.Grounded = true;
            for (int tick = 0; tick < 3; tick++) { player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Land)); }
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Idle));
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void RenderRateDoesNotChangeFixedTickAnimation(int renderFps)
        {
            var sim = Create(); var player = new FighterAnimationPlayer(); player.Reset(sim.P1, sim.Match); var clock = new FixedTickClock();
            for (int frame = 0; frame < renderFps; frame++) clock.Advance(1.0 / renderFps, () => player.Advance(sim.P1, sim.Match, false, false, false));
            Assert.That(player.Sample.Age, Is.EqualTo(60)); Assert.That(player.Sample.FrameIndex(4, 8, true, 0), Is.Zero);
        }
    }
}

