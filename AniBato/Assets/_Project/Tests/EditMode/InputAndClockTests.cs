using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class InputAndClockTests
    {
        [Test]
        public void PressAndReleaseBetweenTicksIsLatchedOnce()
        {
            var latch = new FrameInputLatch();
            latch.Sample(new FrameInput(held: InputButtons.Attack, pressed: InputButtons.Attack));
            latch.Sample(default);
            Assert.That(latch.Consume().IsPressed(InputButtons.Attack), Is.True);
            Assert.That(latch.Consume().Pressed, Is.EqualTo(InputButtons.None));
        }
        [Test]
        public void MultipleRenderSamplesMergeEdgesAndCatchupRepeatsOnlyHeld()
        {
            var latch = new FrameInputLatch();
            latch.Sample(new FrameInput(1, held: InputButtons.Attack, pressed: InputButtons.Attack));
            latch.Sample(new FrameInput(1, held: InputButtons.Attack, pressed: InputButtons.Jump));
            Assert.That(latch.Consume().Pressed, Is.EqualTo(InputButtons.Attack | InputButtons.Jump));
            var next = latch.Consume(); Assert.That(next.Held, Is.EqualTo(InputButtons.Attack)); Assert.That(next.X, Is.EqualTo(1));
            Assert.That(next.Pressed, Is.EqualTo(InputButtons.None));
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void RenderFramePartitionDoesNotChangeSimulationResult(int fps)
        {
            var actual = BattleTestFactory.Create(true); var expected = BattleTestFactory.Create(true); var clock = new FixedTickClock();
            System.Action tick = () =>
            {
                int index = actual.Match.TickCount;
                actual.Tick(InputAt(index), default);
            };
            for (int i = 0; i < fps * 2; i++) clock.Advance(1d / fps, tick);
            for (int i = 0; i < 120; i++) expected.Tick(InputAt(i), default);
            Assert.That(actual.Match.TickCount, Is.EqualTo(120));
            Assert.That(actual.P1.Position.X, Is.EqualTo(expected.P1.Position.X));
            Assert.That(actual.P1.Position.Y, Is.EqualTo(expected.P1.Position.Y));
            Assert.That(actual.P1.Mode, Is.EqualTo(expected.P1.Mode));
            Assert.That(actual.P2.Position.X, Is.EqualTo(expected.P2.Position.X));
            Assert.That(actual.P2.NormalHitCount, Is.EqualTo(expected.P2.NormalHitCount));
            Assert.That(actual.P2.Hearts, Is.EqualTo(expected.P2.Hearts));
            Assert.That(actual.Match.RemainingTicks, Is.EqualTo(expected.Match.RemainingTicks));
        }
        private static FrameInput InputAt(int tick) => new FrameInput(tick < 40 ? 1 : 0,
            pressed: tick == 10 || tick == 25 ? InputButtons.Jump : tick == 0 || tick == 45 ? InputButtons.Attack : InputButtons.None);
        [Test]
        public void SlowRenderFrameDoesNotDiscardSimulationTicks()
        {
            var clock = new FixedTickClock(); int ticks = 0;
            Assert.That(clock.Advance(0.5, () => ticks++), Is.EqualTo(30)); Assert.That(ticks, Is.EqualTo(30));
        }
        [Test]
        public void ButtonBufferExpiresAndDoubleTapCreatesDash()
        {
            var buffer = new FighterInputBuffer(); var rules = PhaseOneDefaults.Rules();
            buffer.Push(new FrameInput(1, pressed: InputButtons.Jump), 1, rules);
            Assert.That(buffer.Consume(InputButtons.Jump, 7), Is.False);
            buffer.Push(default, 2, rules); buffer.Push(new FrameInput(1), 8, rules);
            Assert.That(buffer.Consume(InputButtons.Dash, 8), Is.True); Assert.That(buffer.Consume(InputButtons.Dash, 8), Is.False);
        }
        [Test]
        public void AttackPressedDuringHitstopSurvivesUntilNextAction()
        {
            var sim = BattleTestFactory.Create(true);
            for (int i = 0; i < 5; i++) sim.Tick(new FrameInput(pressed: i == 0 ? InputButtons.Attack : InputButtons.None), default);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            BattleTestFactory.Idle(sim, 4);
            Assert.That(sim.P1.Attack.NextQueued, Is.True);
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void PhaseTwoCombatAndEventsAreIdenticalAcrossRenderPartitions(int fps)
        {
            var rules = PhaseOneDefaults.Rules(); rules.InitialHearts = 9; rules.EnableRingOut = false;
            var actual = BattleTestFactory.Create(true, rules); var expected = BattleTestFactory.Create(true, rules);
            var actualEvents = new System.Collections.Generic.List<BattleEvent>();
            var expectedEvents = new System.Collections.Generic.List<BattleEvent>();
            var clock = new FixedTickClock();
            for (int i = 0; i < fps * 5; i++) clock.Advance(1d / fps, () => CombatTick(actual, actualEvents));
            for (int i = 0; i < 300; i++) CombatTick(expected, expectedEvents);
            Assert.That(actual.Match.TickCount, Is.EqualTo(300));
            Assert.That(actualEvents, Is.EqualTo(expectedEvents));
            Assert.That(actual.P1.Position, Is.EqualTo(expected.P1.Position));
            Assert.That(actual.P2.Position, Is.EqualTo(expected.P2.Position));
            Assert.That(actual.P1.Gauge, Is.EqualTo(expected.P1.Gauge));
            Assert.That(actual.P2.Gauge, Is.EqualTo(expected.P2.Gauge));
            Assert.That(actual.P2.Hearts, Is.EqualTo(expected.P2.Hearts));
            Assert.That(actual.Projectiles.Count, Is.EqualTo(expected.Projectiles.Count));
            for (int i = 0; i < actual.Projectiles.Count; i++)
                Assert.That(actual.Projectiles[i].Position, Is.EqualTo(expected.Projectiles[i].Position));
        }
        private static void CombatTick(BattleSimulation simulation, System.Collections.Generic.List<BattleEvent> events)
        {
            int frame = simulation.Match.TickCount % 60;
            var p1 = new FrameInput(pressed: frame == 0 ? InputButtons.Special : frame == 20 || frame == 40 ? InputButtons.Attack : InputButtons.None);
            var p2 = new FrameInput(held: frame < 10 ? InputButtons.Guard : InputButtons.None,
                pressed: frame == 20 ? InputButtons.Counter : frame == 40 ? InputButtons.Dodge : InputButtons.None);
            simulation.Tick(p1, p2); events.AddRange(simulation.Events);
        }
        [Test]
        public void LatchedDashKeepsDirectionAfterReleaseBeforeTick()
        {
            var latch = new FrameInputLatch(); latch.Sample(new FrameInput(1, pressed: InputButtons.Dash)); latch.Sample(default);
            var sim = BattleTestFactory.Create(); sim.Tick(default, latch.Consume());
            Assert.That(sim.P2.Facing, Is.EqualTo(1)); Assert.That(sim.P2.Velocity.X, Is.EqualTo(sim.P2.Definition.DashSpeed));
        }
    }
}
