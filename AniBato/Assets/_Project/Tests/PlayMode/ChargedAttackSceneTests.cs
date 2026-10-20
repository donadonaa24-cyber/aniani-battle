using System.Collections;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator DedicatedChargeKeyHoldsAtOneTwoAndThreeSecondsInBothFacingsWithoutMovingCollisionThenRestores()
        {
            var keys = controller.Bindings.P1;
            foreach (var character in runner.Setup.Characters) foreach (int facing in new[] { 1, -1 })
            {
                runner.BeginMatch(character, runner.Setup.Character(2), runner.CurrentStage, false); view.RenderFrame();
                var sim = runner.Simulation; Set(sim.P1, "Facing", facing); var start = sim.P1.Position; var hurtbox = sim.P1.Hurtbox;
                var idle = body.sprite; var scale = body.transform.localScale;
                Set(sim.P2,"Position",new BattleVector(start.X-facing*8,sim.P2.Position.Y));
                yield return Key(keys.Attack, true);
                for (int i = 1; i <= 200; i++)
                {
                    Tick(); Assert.That(view.RenderedSample.IsCharging, Is.True);
                    Assert.That(body.sprite, Is.SameAs(character.PaperMotion.Charging.Frames[0]));
                    Assert.That(body.flipX, Is.EqualTo(facing < 0));
                    if (i == 60 || i == 120 || i == 180) Assert.That(sim.P1.ChargeFrames, Is.EqualTo(i));
                }
                Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Charging)); Assert.That(sim.P1.ChargeFrames, Is.EqualTo(180));
                Assert.That(sim.P1.Position, Is.EqualTo(start)); Assert.That(sim.P1.Hurtbox.Center, Is.EqualTo(hurtbox.Center));
                Assert.That(sim.P1.Attack, Is.Null); Assert.That(body.sprite, Is.Not.SameAs(idle));
                Assert.That(body.transform.localScale, Is.EqualTo(scale)); Assert.That(Angle(body), Is.EqualTo(-10 * facing).Within(.001));
                Assert.That(body.transform.localPosition.y, Is.LessThan(-sim.P1.Definition.HurtboxSize.Y * .5f));
                Assert.That(body.color, Is.Not.EqualTo(Color.white));
                Assert.That(view.transform.Find("Charge Paper 0").GetComponent<SpriteRenderer>().enabled, Is.True);
                int tick = sim.Match.TickCount; var offset = body.transform.localPosition; var rotation = body.transform.localRotation;
                for (int render = 0; render < 5; render++) { view.RenderFrame(); Assert.That(body.transform.localPosition, Is.EqualTo(offset)); Assert.That(body.transform.localRotation, Is.EqualTo(rotation)); }
                Assert.That(sim.Match.TickCount, Is.EqualTo(tick)); Assert.That(sim.P1.Position, Is.EqualTo(start));
                Capture(character.PaperMotion.name + "_charged_" + facing, false, "paper_charge");
                CaptureActionEffects("paper_charge", character.PaperMotion.name + "_charge_body_" + facing, body);
                yield return Key(keys.Attack, false); Tick();
                Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P1.Attack.ChargeFrames, Is.EqualTo(180));
                Assert.That(view.RenderedSample.IsCharging, Is.False);
                Assert.That(body.sprite, Is.SameAs(character.PaperMotion.Find(AnimationState.Attack1).Frames[0]));
                Assert.That(view.transform.Find("Charge Paper 0").GetComponent<SpriteRenderer>().enabled, Is.False);
                for (int i = 0; i < 120; i++) Tick();
                Assert.That(sim.P1.ChargeFrames, Is.Zero); Assert.That(sim.P1.Attack, Is.Null);
                Assert.That(sim.Match.IsFinished,Is.False,"原画の復帰確認では対戦相手を射線の後ろへ隔離する");
                Assert.That(body.color, Is.EqualTo(Color.white)); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Idle));
                Assert.That(body.sprite, Is.SameAs(idle));
            }
        }
        [UnityTest]
        public IEnumerator MinimumChargeKeepsUpDownAndNeutralReleaseArtAndShortTapHasNoChargedDamage()
        {
            var keys = controller.Bindings.P1;
            foreach (var data in runner.Setup.Characters) foreach (int direction in new[] { 0, 1, -1 })
            {
                runner.BeginMatch(data, runner.Setup.Character(8), runner.CurrentStage, false); var sim = runner.Simulation;
                var key = direction > 0 ? keys.Up : keys.Down;
                if (direction != 0) yield return Key(key, true);
                yield return Key(keys.Attack, true); for (int i = 0; i < 60; i++) Tick();
                Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Charging)); Assert.That(sim.P1.Attack, Is.Null);
                Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Charging.Frames[0]));
                yield return Key(keys.Attack, false); Tick(); if (direction != 0) yield return Key(key, false);
                Assert.That(sim.P1.Attack.ChargeFrames, Is.EqualTo(60)); Assert.That(view.RenderedSample.IsCharging, Is.False);
                var expected = direction == 0 ? AnimationState.Attack1 : direction > 0 ? AnimationState.AirUp : AnimationState.CrouchAttack;
                Assert.That(view.RenderedSample.State, Is.EqualTo(expected));
                Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(expected).Frame(view.RenderedSample)));
                runner.RestartMatch(); yield return Key(keys.Attack, true); for (int i = 0; i < 30; i++) Tick();
                yield return Key(keys.Attack, false); Tick();
                Assert.That(runner.Simulation.P1.Attack.ChargeFrames, Is.Zero); Assert.That(view.RenderedSample.IsCharging, Is.False);
                Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(AnimationState.Attack1).Frames[0]));
            }
        }
        [UnityTest]
        public IEnumerator IncomingNormalAndLossOfGroundReleaseChargeArtworkInEveryCharacter()
        {
            var keys = controller.Bindings.P1; var opponent = controller.Bindings.P2;
            foreach (var data in runner.Setup.Characters)
            {
                runner.BeginMatch(data, runner.Setup.Character(8), runner.CurrentStage, false); var sim = runner.Simulation;
                Set(sim.P1, "Position", new BattleVector(0, sim.P1.Position.Y));
                Set(sim.P2, "Position", new BattleVector(1.1f, sim.P1.Position.Y)); Set(sim.P2, "Facing", -1);
                yield return Key(opponent.Attack, true); Tick(); yield return Key(opponent.Attack, false); Tick();
                yield return Key(keys.Attack, true); Tick(); Assert.That(view.RenderedSample.IsCharging, Is.True);
                int limit = 0; while (sim.P1.HitstunRemaining == 0 && limit++ < 80) Tick();
                Assert.That(limit, Is.LessThan(80)); yield return Key(keys.Attack, false);
                limit = 0; while (view.RenderedSample.IsCharging && limit++ < 80) Tick();
                Assert.That(limit, Is.LessThan(80)); Assert.That(sim.P1.ChargeFrames, Is.Zero);
                Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(AnimationState.Hitstun).Frame(view.RenderedSample)));
                Assert.That(view.transform.Find("Charge Paper 0").GetComponent<SpriteRenderer>().enabled, Is.False);
                runner.RestartMatch(); sim = runner.Simulation; yield return Key(keys.Attack, true); Tick();
                Assert.That(view.RenderedSample.IsCharging, Is.True);
                Set(sim.P1, "Position", new BattleVector(0, 30)); Set(sim.P1, "Grounded", false); Tick();
                Assert.That(view.RenderedSample.IsCharging, Is.False); Assert.That(sim.P1.ChargeFrames, Is.Zero);
                Assert.That(body.sprite, Is.Not.SameAs(data.PaperMotion.Charging.Frames[0]));
                yield return Key(keys.Attack, false);
            }
        }
    }
}
