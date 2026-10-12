using System.Collections.Generic;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class PhaseSixBTests
    {
        [TestCase(false, -1)] [TestCase(false, 1)] [TestCase(true, -1)] [TestCase(true, 1)]
        public void DoubleTapOptionOnlyDisablesAutomaticDash(bool disabled, int direction)
        {
            var sim = PhaseSixBattleTests.Create();
            sim.Tick(new FrameInput(direction, disableDoubleTapDash: disabled), default);
            sim.Tick(new FrameInput(disableDoubleTapDash: disabled), default);
            sim.Tick(new FrameInput(direction, disableDoubleTapDash: disabled), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(disabled ? FighterMode.Move : FighterMode.Dash));
            Assert.That(sim.P1.Velocity.X, Is.EqualTo(direction * (disabled ? sim.P1.Definition.MoveSpeed : sim.P1.Definition.DashSpeed)));
        }
        [TestCase(-1)] [TestCase(1)]
        public void DedicatedDashWorksWithDoubleTapOffForBothFighters(int direction)
        {
            var sim = PhaseSixBattleTests.Create();
            var frame = new FrameInput(direction, pressed: InputButtons.Dash, disableDoubleTapDash: true);
            sim.Tick(frame, frame);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Dash)); Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Dash));
            Assert.That(sim.P1.DashDirection, Is.EqualTo(direction)); Assert.That(sim.P2.DashDirection, Is.EqualTo(direction));
        }
        [Test]
        public void SwitchingOffClearsAutomaticBufferAndCannotReuseOldFirstTap()
        {
            var buffer = new FighterInputBuffer(); var rules = PhaseSixADefaults.Rules();
            buffer.Push(new FrameInput(1),1,rules); buffer.Push(default,2,rules); buffer.Push(new FrameInput(1),3,rules);
            buffer.Push(new FrameInput(disableDoubleTapDash:true),4,rules); Assert.That(buffer.Consume(InputButtons.Dash,4),Is.False);
            buffer.Push(new FrameInput(1),5,rules); Assert.That(buffer.Consume(InputButtons.Dash,5),Is.False);
            buffer.Push(default,6,rules); buffer.Push(new FrameInput(1),7,rules); Assert.That(buffer.Consume(InputButtons.Dash,7),Is.True);
            buffer.Push(new FrameInput(pressed:InputButtons.Dash,disableDoubleTapDash:true),8,rules);
            buffer.Push(new FrameInput(disableDoubleTapDash:true),9,rules); Assert.That(buffer.Consume(InputButtons.Dash,9),Is.True);
        }
        [Test]
        public void LatchAndCatchupPreserveDashOptionAndPressedDirection()
        {
            var latch = new FrameInputLatch(); latch.Sample(new FrameInput(-1,pressed:InputButtons.Dash,disableDoubleTapDash:true));
            latch.Sample(new FrameInput(disableDoubleTapDash:true)); var first = latch.Consume();
            Assert.That(first.DirectionFor(InputButtons.Dash).X,Is.EqualTo(-1)); Assert.That(first.DisableDoubleTapDash,Is.True);
            Assert.That(first.WithoutPressed().DisableDoubleTapDash,Is.True); Assert.That(latch.Consume().DisableDoubleTapDash,Is.True);
            Assert.That(latch.Consume().Pressed,Is.EqualTo(InputButtons.None));
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void DashOptionSwitchesRemainDeterministicAcrossRenderRates(int fps)
        {
            var actual = PhaseSixBattleTests.Create(); var expected = PhaseSixBattleTests.Create(); var clock = new FixedTickClock();
            var aEvents = new List<BattleEvent>(); var bEvents = new List<BattleEvent>();
            for (int i=0;i<fps*2;i++) clock.Advance(1d/fps,()=>Step(actual,aEvents));
            for (int i=0;i<120;i++) Step(expected,bEvents);
            Assert.That(actual.P1.Position,Is.EqualTo(expected.P1.Position)); Assert.That(actual.P2.Position,Is.EqualTo(expected.P2.Position));
            Assert.That(actual.P1.Mode,Is.EqualTo(expected.P1.Mode)); Assert.That(actual.P1.Stamina,Is.EqualTo(expected.P1.Stamina));
            Assert.That(actual.Match.TickCount,Is.EqualTo(120)); Assert.That(aEvents,Is.EqualTo(bEvents));
        }
        private static void Step(BattleSimulation sim,List<BattleEvent> events)
        {
            int tick=sim.Match.TickCount;
            sim.Tick(new FrameInput(tick%12<3?1:tick%12>=6&&tick%12<9?-1:0,
                pressed:tick%30==0?InputButtons.Dash:InputButtons.None,disableDoubleTapDash:tick>=30&&tick<90),default);
            events.AddRange(sim.Events);
        }
        [TestCase(16f/9)] [TestCase(16f/10)] [TestCase(4f/3)]
        public void FramingContainsUpperPlatformsOverviewBoundariesAndShrinksCentrally(float aspect)
        {
            var stage=PhaseOneDefaults.Stage();
            var combat=CameraFraming.Calculate(stage,stage.SpawnP1,stage.SpawnP2,aspect,false,5.2f,1.2f,2,2.5f);
            for(int i=1;i<stage.PlatformCount;i++)
            { Assert.That(combat.Center.X-combat.Size*aspect,Is.LessThan(stage[i].Bounds.Left)); Assert.That(combat.Center.X+combat.Size*aspect,Is.GreaterThan(stage[i].Bounds.Right)); Assert.That(combat.Center.Y+combat.Size,Is.GreaterThan(stage[i].Bounds.Top)); }
            var overview=CameraFraming.Calculate(stage,stage.SpawnP1,stage.SpawnP2,aspect,true,5.2f,1.2f,2,2.5f);
            Assert.That(overview.Center.X-overview.Size*aspect,Is.LessThanOrEqualTo(stage.LeftBlastLine+2));
            Assert.That(overview.Center.X+overview.Size*aspect,Is.GreaterThanOrEqualTo(stage.RightBlastLine-2));
            var sim=PhaseSixBattleTests.Create(); while(!sim.Match.IsFinished) sim.Tick(default,default);
            var small=CameraFraming.Calculate(sim.Stage,new BattleVector(-1,1.1f),new BattleVector(1,1.1f),aspect,false,5.2f,1.2f,2,2.5f);
            Assert.That(small.Center.X,Is.Zero); Assert.That(small.Size,Is.LessThan(combat.Size));
            float lagged=CameraFraming.ContainingSize(combat,new BattleVector(5,5),aspect,1);
            Assert.That(5-lagged*aspect,Is.LessThanOrEqualTo(combat.Required.Left+.001f));
            Assert.That(5+lagged,Is.GreaterThanOrEqualTo(combat.Required.Top-.001f));
        }
    }
}
