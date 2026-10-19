using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator SimultaneousNormalsProduceAVisibleClashAndBothRecoverWithoutDamage()
        {
            var character = runner.Setup.Character(2); var first = controller.Bindings.P1; var second = controller.Bindings.P2;
            runner.BeginMatch(character, character, runner.CurrentStage, false);
            var sim = runner.Simulation; float center = character.GroundNormals[0].Definition.Hitbox.Center.X;
            Set(sim.P1, "Position", new BattleVector(-center, 1.1f)); Set(sim.P2, "Position", new BattleVector(center, 1.1f));
            yield return Key(first.Attack, true); yield return Key(second.Attack, true); Tick();
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Charging)); Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Charging));
            yield return Key(first.Attack, false); yield return Key(second.Attack, false); Tick();
            int frames = character.GroundNormals[0].Definition.TotalFrames;
            bool clashed = false;
            for (int frame = 0; frame < frames; frame++)
            {
                if (sim.Events.Any(e => e.Kind == BattleEventKind.Clash)) { clashed = true; break; }
                Tick();
            }
            Assert.That(clashed, Is.True); Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P2.Attack, Is.Null);
            Assert.That(sim.P1.NormalHitCount + sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery)); Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Recovery));
            Assert.That(sim.P1.HitstopRemaining, Is.GreaterThan(0)); Assert.That(sim.P2.HitstopRemaining, Is.GreaterThan(0));
            Assert.That(UnityEngine.Object.FindObjectsByType<UnityEngine.SpriteRenderer>(UnityEngine.FindObjectsSortMode.None).Any(r => r.name.StartsWith("Hit Spark") && r.enabled), Is.True);
            Capture("normal_clash", false, "rework_r1");
        }
        [UnityTest]
        public IEnumerator EveryCharacterUsesGroundUpAndDownAndDirectionalSpecialsThroughRealInput()
        {
            var keys = controller.Bindings.P1;
            foreach (var character in runner.Setup.Characters)
            {
                foreach (bool up in new[] { true, false })
                {
                    runner.BeginMatch(character, runner.Setup.Character(2), runner.CurrentStage, false);
                    var direction = up ? keys.Up : keys.Down;
                    yield return Key(direction, true); yield return Key(keys.Attack, true); Tick();
                    Assert.That(runner.Simulation.P1.Mode, Is.EqualTo(FighterMode.Charging));
                    yield return Key(keys.Attack, false); Tick(); yield return Key(direction, false);
                    var a = runner.Simulation.P1.Attack;
                    Assert.That(a.Definition.NormalDirection, Is.EqualTo(up ? NormalDirection.Up : NormalDirection.Crouch));
                    Assert.That(view.RenderedSample.State, Is.EqualTo(up ? AnimationState.AirUp : AnimationState.CrouchAttack));
                    Capture(character.name + (up ? "_up_prepare" : "_down_prepare"), true, "rework_r1");
                    for (int tick = 0; tick < a.Definition.StartupFrames; tick++) Tick();
                    Assert.That(a.IsActive, Is.True); Assert.That(a.IsDamageActive, Is.True);
                    Capture(character.name + (up ? "_up_active" : "_down_active"), true, "rework_r1");
                }
                foreach (var direction in new[] { keys.Up, keys.Down, keys.Left, keys.Right })
                {
                    runner.BeginMatch(character, runner.Setup.Character(2), runner.CurrentStage, false);
                    yield return Key(direction, true); yield return Key(keys.Special, true); Tick();
                    var slot = direction == keys.Down ? SpecialSlot.Down : direction == keys.Up ? SpecialSlot.Up : SpecialSlot.Neutral;
                    Assert.That(runner.Simulation.P1.CurrentSpecial, Is.EqualTo(slot));
                    Assert.That(runner.Simulation.P1.Attack.Definition.Name, Is.EqualTo(character.Specials[(int)slot].Definition.Name));
                    Assert.That(view.RenderedSample.State, Is.EqualTo(slot == SpecialSlot.Down ? AnimationState.SpecialDown : slot == SpecialSlot.Up ? AnimationState.SpecialUp : AnimationState.SpecialNeutral));
                    yield return Key(keys.Special, false); yield return Key(direction, false);
                }
            }
        }
        [UnityTest]
        public IEnumerator PracticeUltimatesStartReadyAndCanBeUsedThreeTimesThenNormalBattleRestoresLimits()
        {
            var keys = controller.Bindings.P1;
            var character = runner.Setup.Character(2);
            runner.BeginMatch(character, runner.Setup.Character(0), runner.CurrentStage, false);
            runner.Practice = new PracticeOptions();
            Assert.That(runner.Simulation.P1.Gauge, Is.EqualTo(runner.Simulation.Rules.Gauge.Maximum));
            for (int use = 1; use <= 3; use++)
            {
                yield return Key(keys.Ultimate, true); Tick();
                Assert.That(runner.Simulation.P1.UltimateUses, Is.EqualTo(use));
                Assert.That(GaugeSystem.CanUseUltimate(runner.Simulation.P1, runner.Simulation.Rules), Is.True);
                yield return Key(keys.Ultimate, false);
                int frames = character.Ultimate.Definition.TotalFrames + character.Movement.UltimateCutInFrames + 12;
                for (int frame = 0; frame < frames; frame++) Tick();
                Assert.That(runner.Simulation.P1.Gauge, Is.EqualTo(runner.Simulation.Rules.Gauge.Maximum));
            }
            runner.Practice = null; runner.BeginMatch(character, runner.Setup.Character(0), runner.CurrentStage, false);
            Assert.That(runner.Simulation.P1.UnlimitedUltimates, Is.False);
            Assert.That(runner.Simulation.P1.Gauge, Is.Zero); Assert.That(runner.Simulation.P1.UltimateUses, Is.Zero);
        }
    }
}
