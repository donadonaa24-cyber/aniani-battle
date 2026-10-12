using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class MatchTests
    {
        [Test]
        public void Exactly4200TicksIs70SecondsAndFinishedMatchDoesNotAdvance()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 4199);
            Assert.That(sim.Match.IsFinished, Is.False); Assert.That(sim.Match.RemainingTicks, Is.EqualTo(1));
            sim.Tick(default, default); Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.TimeUp));
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Draw)); Assert.That(sim.Match.RemainingSeconds, Is.Zero);
            BattleTestFactory.Idle(sim, 20); Assert.That(sim.Match.TickCount, Is.EqualTo(4200));
        }
        [TestCase(1, MatchResult.Player1Wins)]
        [TestCase(3, MatchResult.Player1Wins)]
        [TestCase(0, MatchResult.Draw)]
        public void TimeUpUsesHeartsThenFewerAccumulatedHitsThenDraw(int hits, MatchResult expected)
        {
            // Isolate the tiebreak: combo knockback can otherwise leave a fighter on a disappearing edge.
            var rules = PhaseOneDefaults.Rules(); rules.Shrink.Enabled = false;
            var sim = BattleTestFactory.Create(true, rules, BattleTestFactory.StationaryAttack());
            for (int i = 0; i < hits; i++) BattleTestFactory.Hit(sim);
            BattleTestFactory.Idle(sim, 4200 - sim.Match.TickCount);
            Assert.That(sim.Match.Result, Is.EqualTo(expected)); Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.TimeUp));
        }
        [Test]
        public void PlayerTwoCanWinTiebreak()
        {
            var sim = BattleTestFactory.Create(true, definition: BattleTestFactory.StationaryAttack());
            sim.Tick(default, new FrameInput(pressed: InputButtons.Attack)); BattleTestFactory.Idle(sim, 4199);
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player2Wins));
        }
        [Test]
        public void HeartsKOWinsOverTimeUpOnSameTick()
        {
            var rules = PhaseOneDefaults.Rules(); rules.InitialHearts = 1;
            var sim = BattleTestFactory.Create(true, rules, BattleTestFactory.StationaryAttack(startup: 0));
            BattleTestFactory.Hit(sim); BattleTestFactory.Hit(sim);
            BattleTestFactory.Idle(sim, 4199 - sim.Match.TickCount);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.Hearts));
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player1Wins));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.TimeUp), Is.False);
        }
        [Test]
        public void SimultaneousHeartKOIsDraw()
        {
            var rules = PhaseOneDefaults.Rules(); rules.InitialHearts = 1;
            var sim = BattleTestFactory.Create(true, rules, BattleTestFactory.StationaryAttack(startup: 0));
            for (int i = 0; i < 3; i++)
            {
                var attack = new FrameInput(pressed: InputButtons.Attack); sim.Tick(attack, attack);
                if (i < 2) BattleTestFactory.Idle(sim, 25);
            }
            Assert.That(sim.P1.Hearts, Is.Zero); Assert.That(sim.P2.Hearts, Is.Zero);
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Draw));
        }
        [Test]
        public void RingOutHasPriorityOverHeartKOAndTimeUp()
        {
            var rules = PhaseOneDefaults.Rules(); rules.InitialHearts = 1;
            var character = BattleTestFactory.StationaryAttack(startup: 0);
            var stage = new StageGeometry(new[] { new StagePlatform(new BattleRect(new BattleVector(0, -0.6f), new BattleVector(36, 1.2f)), false) },
                -24, 1.6f, -10, new BattleVector(0, 1.1f), new BattleVector(1.5f, 1.1f));
            var sim = new BattleSimulation(character, character, rules, stage);
            BattleTestFactory.Hit(sim); BattleTestFactory.Hit(sim);
            BattleTestFactory.Idle(sim, 4199 - sim.Match.TickCount);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(1, pressed: InputButtons.Dash));
            Assert.That(sim.P2.Hearts, Is.Zero);
            Assert.That(sim.Match.RemainingTicks, Is.Zero);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut));
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player1Wins));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.TimeUp), Is.False);
        }
        [Test]
        public void WalkingBeyondBlastLineLosesImmediately()
        {
            var sim = BattleTestFactory.Create();
            for (int i = 0; i < 300 && !sim.Match.IsFinished; i++) sim.Tick(new FrameInput(-1), default);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut));
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player2Wins));
        }
    }
}
