using System;
using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class StageShrinkTests
    {
        [TestCase(2, 3, 0, 0, MatchResult.Player2Wins)]
        [TestCase(3, 3, 1, 2, MatchResult.Player1Wins)]
        [TestCase(3, 3, 2, 2, MatchResult.Draw)]
        public void TimeUpComparesHeartsThenAccumulationWithShrinkEnabled(int hearts1, int hearts2, int hits1, int hits2, MatchResult result)
        {
            var sim = BattleTestFactory.Create(); sim.P1.Hearts = hearts1; sim.P2.Hearts = hearts2;
            sim.P1.NormalHitCount = hits1; sim.P2.NormalHitCount = hits2;
            BattleTestFactory.Idle(sim, 4200);
            Assert.That(sim.Shrink.CompletedSteps, Is.EqualTo(5)); Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.TimeUp));
            Assert.That(sim.Match.Result, Is.EqualTo(result));
        }
        [Test]
        public void WarningAt15SecondsAndStartAtExactly10Seconds()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 3299);
            Assert.That(sim.Shrink.WarningIssued, Is.False);
            sim.Tick(default, default);
            Assert.That(sim.Events.Single().Kind, Is.EqualTo(BattleEventKind.StageShrinkWarning));
            Assert.That(sim.Match.RemainingSeconds, Is.EqualTo(15));
            BattleTestFactory.Idle(sim, 299); Assert.That(sim.Shrink.Started, Is.False);
            sim.Tick(default, default);
            Assert.That(sim.Events.Single().Kind, Is.EqualTo(BattleEventKind.StageShrinkStarted));
            Assert.That(sim.Match.RemainingSeconds, Is.EqualTo(10));
        }
        [Test]
        public void EveryRemovalHasFullWarningAndFinalShapeIsOnlyCentral30Percent()
        {
            var sim = BattleTestFactory.Create(); var warnings = new Dictionary<int, int>();
            int removed = 0;
            while (!sim.Match.IsFinished)
            {
                sim.Tick(default, default);
                foreach (var e in sim.Events)
                {
                    if (e.Kind == BattleEventKind.StageSegmentWarning) warnings.Add(e.Value, e.Tick);
                    if (e.Kind == BattleEventKind.StageShrunk)
                    { Assert.That(e.Tick - warnings[e.Value], Is.EqualTo(60)); removed++; }
                }
                if (sim.Shrink.CompletedSteps >= 2)
                { Assert.That(sim.Stage.IsActive(1), Is.False); Assert.That(sim.Stage.IsActive(2), Is.False); }
            }
            Assert.That(removed, Is.EqualTo(5)); Assert.That(sim.Stage[0].Bounds.Size.X, Is.EqualTo(10.8f).Within(0.0001));
            Assert.That(sim.Stage[0].Bounds.Center.X, Is.Zero); Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.TimeUp));
        }
        [Test]
        public void RemovedFloorMakesFighterFallThenLoseBeyondBlastLine()
        {
            var source = PhaseOneDefaults.Stage(); var platforms = Enumerable.Range(0, source.PlatformCount).Select(i => source[i]).ToArray();
            var stage = new StageGeometry(platforms, -24, 24, -10, new BattleVector(-17, 1.1f), new BattleVector(0, 1.1f));
            var character = PhaseOneDefaults.Character(); var sim = new BattleSimulation(character, character, PhaseOneDefaults.Rules(), stage);
            BattleTestFactory.Idle(sim, 3707); Assert.That(sim.P1.Grounded, Is.True);
            sim.Tick(default, default);
            Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.Match.IsFinished, Is.False);
            Assert.That(sim.P1.Position.Y, Is.LessThan(1.1f));
            for (int i = 0; i < 120 && !sim.Match.IsFinished; i++) sim.Tick(default, default);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut)); Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player2Wins));
            Assert.That(source[0].Bounds.Size.X, Is.EqualTo(36)); Assert.That(stage[0].Bounds.Size.X, Is.EqualTo(36));
        }
        [Test]
        public void RemovedUpperPlatformCannotBeLandedOn()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 3816);
            sim.P1.Position = new BattleVector(-8, 5.3f); sim.P1.Velocity = new BattleVector(0, -10);
            sim.P1.Grounded = false; sim.P1.GroundPlatform = -1;
            sim.Tick(default, default);
            Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.Stage.IsActive(1), Is.False);
        }
        [Test]
        public void RemovalTickAllowsOnlyTheRemainingAirJump()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 3707);
            sim.P1.Position = new BattleVector(-17, 1.1f);
            Assert.That(sim.P1.JumpsUsed, Is.Zero);
            sim.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.JumpsUsed, Is.EqualTo(2));
            Assert.That(sim.P1.Velocity.Y, Is.GreaterThan(0));
        }
        [Test]
        public void ShrinkIsDeterministicAndResetsForNewMatch()
        {
            var a = BattleTestFactory.Create(); var b = BattleTestFactory.Create();
            for (int i = 0; i < 4140; i++)
            {
                a.Tick(default, default); b.Tick(default, default);
                Assert.That(a.Stage[0].Bounds.Size.X, Is.EqualTo(b.Stage[0].Bounds.Size.X));
                Assert.That(a.Events.Select(e => e.Kind), Is.EqualTo(b.Events.Select(e => e.Kind)));
            }
            var fresh = BattleTestFactory.Create(); Assert.That(fresh.Stage[0].Bounds.Size.X, Is.EqualTo(36));
            Assert.That(fresh.Shrink.WarningIssued, Is.False); Assert.That(fresh.Stage.IsActive(1), Is.True);
        }
        [Test]
        public void CutInFreezesShrinkTogetherWithMatchClock()
        {
            var sim = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(0)); BattleTestFactory.Idle(sim, 3599);
            sim.P1.Gauge = 100;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            int tick = sim.Match.TickCount; float width = sim.Stage[0].Bounds.Size.X;
            Assert.That(sim.Shrink.Started, Is.True);
            Assert.That(sim.CutInRemainingFrames, Is.GreaterThan(0));
            while (sim.CutInRemainingFrames > 0)
            { sim.Tick(default, default); Assert.That(sim.Match.TickCount, Is.EqualTo(tick)); Assert.That(sim.Stage[0].Bounds.Size.X, Is.EqualTo(width)); }
        }
        [Test]
        public void RingOutDuringFinalTickHasPriorityOverTimeUpAfterShrink()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 4199);
            sim.P1.Grounded = false; sim.P1.Position = new BattleVector(0, -9.9f); sim.P1.Velocity = new BattleVector(0, -26);
            sim.Tick(default, default);
            Assert.That(sim.Match.RemainingTicks, Is.Zero); Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut));
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player2Wins));
        }
        [Test]
        public void InvalidWarningOrEndTimingIsRejected()
        {
            var definition = StageShrinkDefinition.Default; definition.StepWarningFrames = 109;
            Assert.Throws<ArgumentException>(() => definition.Validate());
            definition = StageShrinkDefinition.Default; definition.StepIntervalFrames = 120;
            Assert.Throws<ArgumentException>(() => definition.Validate());
        }
    }
}
