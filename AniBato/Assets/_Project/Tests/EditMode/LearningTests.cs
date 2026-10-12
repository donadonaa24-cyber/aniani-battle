using System;
using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class LearningTests
    {
        private static BattleSimulation Sim() => PhaseSixBattleTests.Create();
        [TestCase(TutorialStep.Move)] [TestCase(TutorialStep.Jump)] [TestCase(TutorialStep.DoubleJump)]
        [TestCase(TutorialStep.NormalCombo)] [TestCase(TutorialStep.Directional)] [TestCase(TutorialStep.Specials)]
        [TestCase(TutorialStep.Stamina)] [TestCase(TutorialStep.Rules)]
        public void EachActionLessonRequiresItsActualCondition(TutorialStep step)
        {
            var sim = Sim(); var tutorial = new TutorialSession { Step = step }; tutorial.Prepare(sim);
            tutorial.Observe(sim, default); Assert.That(tutorial.Step, Is.EqualTo(step), "Idle must not pass a task.");
            var input = default(FrameInput);
            switch (step)
            {
                case TutorialStep.Move: sim.P1.Position = new BattleVector(sim.P1.Position.X + 1, sim.P1.Position.Y); break;
                case TutorialStep.Jump: sim.P1.Grounded = false; sim.P1.JumpsUsed = 1; sim.P1.Velocity = new BattleVector(0, 1); break;
                case TutorialStep.DoubleJump: sim.P1.Grounded = false; sim.P1.JumpsUsed = 2; input = new FrameInput(pressed: InputButtons.Jump); break;
                case TutorialStep.NormalCombo: sim.P1.Attack = new AttackInstance(1, sim.P1.Definition.GroundNormals[2], 2, false); break;
                case TutorialStep.Directional:
                    int id = 1; foreach (int slot in new[] { 1, 2, 3 })
                    { sim.P1.Attack = new AttackInstance(id++, sim.P1.Definition.DirectionalNormals[slot], 0, slot == 3); tutorial.Observe(sim, default); }
                    Assert.That(tutorial.DirectionMask, Is.EqualTo(7)); Assert.That(tutorial.Step, Is.EqualTo(TutorialStep.Specials)); return;
                case TutorialStep.Specials:
                    for (int slot = 0; slot < 4; slot++)
                    { sim.P1.CurrentSpecial = (SpecialSlot)slot; sim.P1.Attack = new AttackInstance(slot + 1, sim.P1.Definition.Special((SpecialSlot)slot), 0, false); tutorial.Observe(sim, default); }
                    Assert.That(tutorial.SpecialMask, Is.EqualTo(15)); Assert.That(tutorial.Step, Is.EqualTo(TutorialStep.Guard)); return;
                case TutorialStep.Stamina:
                    sim.P1.StaminaUnits -= 20 * 60; tutorial.Observe(sim, default); Assert.That(tutorial.Step, Is.EqualTo(step));
                    sim.P1.StaminaUnits = 100 * 60; break;
                case TutorialStep.Rules: input = new FrameInput(pressed: InputButtons.Attack); break;
            }
            tutorial.Observe(sim, input); Assert.That(tutorial.Step, Is.EqualTo(step + 1));
        }
        [TestCase(TutorialStep.Guard)] [TestCase(TutorialStep.GuardBreak)] [TestCase(TutorialStep.Dodge)]
        [TestCase(TutorialStep.Counter)] [TestCase(TutorialStep.JustDefense)] [TestCase(TutorialStep.Ultimate)]
        public void DefenseAndUltimateLessonsUseSuccessfulCombatEvents(TutorialStep step)
        {
            var c = PhaseSixADefaults.Character(0); c.UltimateCutInFrames = 0;
            var normal = c.GroundNormals[0]; normal.StartupFrames = 0; normal.HitstopFrames = 0; c.GroundNormals[0] = normal;
            var special = c.Specials[0]; special.StartupFrames = 0; special.SpawnProjectile = false; special.Hitbox = normal.Hitbox; c.Specials[0] = special;
            var ultimate = c.Ultimate; ultimate.StartupFrames = 0; ultimate.Hitbox = normal.Hitbox; c.Ultimate = ultimate;
            var crouch = c.DirectionalNormals[1]; crouch.StartupFrames = 0; crouch.Hitbox = normal.Hitbox; c.DirectionalNormals[1] = crouch;
            var sim = BattleTestFactory.Create(true, PhaseSixADefaults.Rules(), c); var tutorial = new TutorialSession { Step = step };
            tutorial.Prepare(sim); tutorial.Observe(sim, default); Assert.That(tutorial.Step, Is.EqualTo(step));
            FrameInput p1, p2;
            if (step == TutorialStep.Ultimate) { p1 = new FrameInput(pressed: InputButtons.Ultimate); p2 = default; }
            else if (step == TutorialStep.GuardBreak) { p1 = new FrameInput(y: -1, pressed: InputButtons.Attack); p2 = new FrameInput(held: InputButtons.Guard); }
            else
            {
                var defense = step == TutorialStep.Counter ? InputButtons.Counter : step == TutorialStep.Dodge ? InputButtons.Dodge : InputButtons.Guard;
                p1 = new FrameInput(held: defense, pressed: defense); p2 = new FrameInput(pressed: step == TutorialStep.Counter ? InputButtons.Attack : InputButtons.Special);
            }
            sim.Tick(p1, p2); tutorial.Observe(sim, p1);
            Assert.That(tutorial.Step, Is.EqualTo(step + 1));
        }
        [Test]
        public void CoachProducesTheRequestedOpponentsAndResetRestartsAtMove()
        {
            var sim = Sim(); var options = new PracticeOptions { Opponent = PracticeOpponent.Guard };
            Assert.That(options.DummyInput(sim).IsHeld(InputButtons.Guard), Is.True);
            options.Opponent = PracticeOpponent.Jump; Assert.That(options.DummyInput(sim).IsPressed(InputButtons.Jump), Is.True);
            var tutorial = new TutorialSession { Step = TutorialStep.Counter }; sim.Match.TickCount = 120;
            Assert.That(tutorial.CoachInput(sim).IsPressed(InputButtons.Attack), Is.True); tutorial.Reset(); Assert.That(tutorial.Step, Is.EqualTo(TutorialStep.Move));
        }
        [Test]
        public void ProfileSwapsDuplicatesPersistsAndResetsWithoutMutatingAnotherPlayer()
        {
            var defaults = Enumerable.Range(1, 13).ToArray(); var a = new ControlProfile(defaults); var b = new ControlProfile(defaults);
            a.Rebind(ControlAction.Jump, a[ControlAction.Attack]); Assert.That(a[ControlAction.Jump], Is.EqualTo(6)); Assert.That(a[ControlAction.Attack], Is.EqualTo(5));
            Assert.That(b[ControlAction.Jump], Is.EqualTo(5)); var loaded = new ControlProfile(defaults);
            Assert.That(loaded.Load(a.Save(), n => n > 0 && n <= 16), Is.True); Assert.That(loaded.Save(), Is.EqualTo(a.Save()));
            Assert.That(loaded.Load("1:1,1,3,4,5,6,7,8,9,10,11,12,13", n => n > 0), Is.False); Assert.That(loaded.Save(), Is.EqualTo(a.Save()));
            loaded.Reset(); Assert.That(loaded.Save(), Is.EqualTo(b.Save())); Assert.That(defaults[4], Is.EqualTo(5));
        }
        [TestCase("DualSenseGamepadHID", "Wireless Controller", "Sony", InputLabelDevice.PlayStation)]
        [TestCase("XInputController", "Xbox Controller", "Microsoft", InputLabelDevice.Xbox)]
        public void DeviceGlyphsArePlatformSpecific(string layout, string product, string manufacturer, InputLabelDevice expected)
        {
            var device = InputGlyphs.Detect(layout, product, manufacturer); Assert.That(device, Is.EqualTo(expected));
            Assert.That(InputGlyphs.Pad(5, device), Is.EqualTo(expected == InputLabelDevice.PlayStation ? "×" : "A"));
            Assert.That(InputGlyphs.Pad(13, device), Is.EqualTo(expected == InputLabelDevice.PlayStation ? "OPTIONS" : "START"));
            Assert.That(InputGlyphs.Pad(14, device), Is.EqualTo(expected == InputLabelDevice.PlayStation ? "○" : "B"));
        }
    }
}
