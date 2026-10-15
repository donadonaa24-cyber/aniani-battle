using System.Collections;
using AniBato.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator ChargeUsesExistingWholeImageAndVisualSinkWithoutMovingCollisionThenRestores()
        {
            var keys = controller.Bindings.P1;
            foreach (var character in runner.Setup.Characters)
            {
                runner.BeginMatch(character, runner.Setup.Character(2), runner.CurrentStage, false); view.RenderFrame();
                var sim = runner.Simulation; var start = sim.P1.Position; var hurtbox = sim.P1.Hurtbox;
                var idle = body.sprite; var scale = body.transform.localScale;
                yield return Key(keys.Attack, true); for (int i = 0; i < 180; i++) Tick();
                Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Charging)); Assert.That(sim.P1.ChargeFrames, Is.EqualTo(180));
                Assert.That(sim.P1.Position, Is.EqualTo(start)); Assert.That(sim.P1.Hurtbox.Center, Is.EqualTo(hurtbox.Center));
                Assert.That(sim.P1.Attack, Is.Null); Assert.That(body.sprite, Is.SameAs(idle));
                Assert.That(body.transform.localScale, Is.EqualTo(scale)); Assert.That(Angle(body), Is.EqualTo(-10).Within(.001));
                Assert.That(body.transform.localPosition.y, Is.LessThan(-sim.P1.Definition.HurtboxSize.Y * .5f));
                Assert.That(body.color, Is.Not.EqualTo(Color.white));
                Assert.That(view.transform.Find("Charge Paper 0").GetComponent<SpriteRenderer>().enabled, Is.True);
                Capture(character.PaperMotion.name + "_charged", false, "rework_charge");
                Capture(character.PaperMotion.name + "_charged_body", true, "rework_charge");
                yield return Key(keys.Attack, false); Tick();
                Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P1.Attack.ChargeFrames, Is.EqualTo(180));
                Assert.That(view.transform.Find("Charge Paper 0").GetComponent<SpriteRenderer>().enabled, Is.False);
                for (int i = 0; i < 120; i++) Tick();
                Assert.That(sim.P1.ChargeFrames, Is.Zero); Assert.That(sim.P1.Attack, Is.Null);
                Assert.That(body.color, Is.EqualTo(Color.white)); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Idle));
            }
        }
    }
}
