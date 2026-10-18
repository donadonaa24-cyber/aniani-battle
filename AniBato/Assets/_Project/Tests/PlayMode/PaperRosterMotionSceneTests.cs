using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        private CharacterData[] MotionCharacters() => runner.Setup.Characters.Where(c => c.PaperMotion && c.Movement.Name != "千鶴" &&
            c.PaperMotion.HeavyDamage != null && c.PaperMotion.HeavyDamage.Valid).ToArray();
        private void AssertMotionFrame(AnimationState state, int facing, bool heavy = false)
        {
            var paper = runner.Character(0).PaperMotion;
            Assert.That(view.RenderedSample.State, Is.EqualTo(state)); Assert.That(view.UsesPaperSprite, Is.True);
            Assert.That(body.sprite, Is.SameAs(paper.Find(state, heavy).Frames.Single()));
            Assert.That(body.flipX, Is.EqualTo(facing < 0)); Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one));
            var sim = runner.Simulation; var position = sim.P1.Position; var velocity = sim.P1.Velocity;
            int tick = sim.Match.TickCount; var rotation = body.transform.localRotation; var offset = body.transform.localPosition;
            for (int render = 0; render < 5; render++)
            {
                view.RenderFrame(); Assert.That(body.transform.localRotation, Is.EqualTo(rotation));
                Assert.That(body.transform.localPosition, Is.EqualTo(offset));
            }
            Assert.That(sim.Match.TickCount, Is.EqualTo(tick)); Assert.That(sim.P1.Position, Is.EqualTo(position));
            Assert.That(sim.P1.Velocity, Is.EqualTo(velocity));
        }
        [UnityTest]
        public IEnumerator DedicatedRunJumpFallAndDodgeFollowRealInputsAndKeepGravityAndFixedLean()
        {
            var characters = MotionCharacters(); Assert.That(characters.Length, Is.GreaterThan(0)); var keys = controller.Bindings.P1;
            foreach (var data in characters) foreach (int facing in new[] { 1, -1 })
            {
                string directory = "paper_motion_" + data.PaperMotion.name.Replace("CorePaperMotion", "");
                runner.BeginMatch(data, runner.Setup.Character(8), runner.CurrentStage, false); var sim = runner.Simulation;
                Set(sim.P1, "Position", new BattleVector(0, sim.P1.Position.Y)); Set(sim.P2, "Position", new BattleVector(8, sim.P2.Position.Y));
                var travel = facing > 0 ? keys.Right : keys.Left;
                int dashDegrees = data.Movement.Name == "シロ" ? 10 : 45;
                yield return Key(travel, true); yield return Key(keys.Dash, true); Tick();
                AssertMotionFrame(AnimationState.Dash, facing);
                Assert.That(Angle(body), Is.EqualTo(-dashDegrees * facing).Within(.001)); CaptureActionEffects(directory, "run_" + facing, body);
                for (int i = 0; i < 4; i++) { Tick(); AssertMotionFrame(AnimationState.Dash, facing); Assert.That(Angle(body), Is.EqualTo(-dashDegrees * facing).Within(.001)); }
                yield return Key(keys.Dash, false); yield return Key(travel, false);
                runner.RestartMatch(); sim = runner.Simulation; Set(sim.P1, "Facing", facing);
                yield return Key(keys.Jump, true); Tick(); yield return Key(keys.Jump, false);
                int limit = 0; while (view.RenderedSample.State != AnimationState.Jump && limit++ < 30) Tick();
                Assert.That(limit, Is.LessThan(30)); AssertMotionFrame(AnimationState.Jump, facing); CaptureActionEffects(directory, "jump_" + facing, body);
                Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.Velocity.Y, Is.GreaterThan(0));
                float vy = sim.P1.Velocity.Y; Tick(); Assert.That(sim.P1.Velocity.Y, Is.LessThan(vy));
                limit = 0; while (view.RenderedSample.State != AnimationState.Fall && limit++ < 100) Tick();
                Assert.That(limit, Is.LessThan(100)); AssertMotionFrame(AnimationState.Fall, facing); CaptureActionEffects(directory, "fall_" + facing, body);
                Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.Velocity.Y, Is.LessThan(0));
                vy = sim.P1.Velocity.Y; Tick(); Assert.That(sim.P1.Velocity.Y, Is.LessThan(vy));
                limit = 0; while (!sim.P1.Grounded && limit++ < 100) Tick(); Assert.That(sim.P1.Grounded, Is.True);
                for (int i = 0; i < 30; i++) Tick(); Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Sprite));
                runner.RestartMatch(); sim = runner.Simulation; Set(sim.P1, "Facing", facing);
                yield return Key(keys.Dodge, true); Tick(); AssertMotionFrame(AnimationState.Dodge, facing);
                CaptureActionEffects(directory, "dodge_" + facing, body); yield return Key(keys.Dodge, false);
                for (int i = 0; i < 90; i++) Tick(); Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Sprite));
            }
        }
        [UnityTest]
        public IEnumerator ActualCounterAndNormalDamageChooseSuccessLightAndHeartLossArtAndThenRelease()
        {
            var keys = controller.Bindings.P1; var opponent = controller.Bindings.P2;
            foreach (var data in MotionCharacters()) foreach (int facing in new[] { 1, -1 })
            {
                string directory = "paper_motion_" + data.PaperMotion.name.Replace("CorePaperMotion", "");
                for (int mode = 0; mode < 3; mode++)
                {
                    runner.BeginMatch(data, runner.Setup.Character(8), runner.CurrentStage, false); var sim = runner.Simulation;
                    Set(sim.P1, "Position", new BattleVector(0, sim.P1.Position.Y)); Set(sim.P1, "Facing", facing);
                    Set(sim.P2, "Position", new BattleVector(1.1f * facing, sim.P1.Position.Y)); Set(sim.P2, "Facing", -facing);
                    Set(sim.P1, "NormalHitCount", mode == 2 ? 2 : 0);
                    yield return Key(opponent.Attack, true); Tick(); yield return Key(opponent.Attack, false); Tick();
                    Assert.That(sim.P2.Attack, Is.Not.Null);
                    if (mode == 0) { yield return Key(keys.Counter, true); Tick(); }
                    int limit = 0; var expected = mode == 0 ? AnimationState.CounterSuccess : AnimationState.Hitstun;
                    while (view.RenderedSample.State != expected && limit++ < 80) Tick();
                    Assert.That(limit, Is.LessThan(80), data.Movement.Name + " mode=" + mode);
                    AssertMotionFrame(expected, facing, mode == 2);
                    CaptureActionEffects(directory, mode == 0 ? "counter_success_" + facing : (mode == 1 ? "hit_light_" : "hit_heavy_") + facing, body);
                    Assert.That(sim.P1.Hearts, Is.EqualTo(mode == 2 ? 2 : 3));
                    Assert.That(sim.P1.NormalHitCount, Is.EqualTo(mode == 1 ? 1 : 0));
                    if (mode == 0)
                    {
                        Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Attack, Is.Null);
                        yield return Key(keys.Counter, false);
                    }
                    else
                    {
                        Assert.That(sim.P1.LastDamageLostHeart, Is.EqualTo(mode == 2));
                        var sprite = body.sprite; var sample = view.RenderedSample; int frozen = sim.P1.HitstopRemaining;
                        for (int i = 0; i < frozen; i++) { Tick(); Assert.That(body.sprite, Is.SameAs(sprite)); Assert.That(view.RenderedSample.Age, Is.EqualTo(sample.Age)); }
                        var player = ((FighterAnimationPlayer[])typeof(BattleRunner).GetField("animations", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(runner))[0];
                        var property = typeof(FighterAnimationPlayer).GetProperty("Sample"); var original = player.Sample;
                        try
                        {
                            property.SetValue(player, new AnimationSample { State = AnimationState.Knockback, Age = 6, FlipX = facing < 0 }); view.RenderFrame();
                            AssertMotionFrame(AnimationState.Knockback, facing, mode == 2); Assert.That(Mathf.Abs(Angle(body)), Is.EqualTo(90).Within(.001));
                            CaptureActionEffects(directory, (mode == 1 ? "tumble_light_" : "tumble_heavy_") + facing, body);
                        }
                        finally { property.SetValue(player, original); view.RenderFrame(); }
                    }
                    for (int i = 0; i < 150; i++) Tick();
                    Assert.That(sim.Match.IsFinished, Is.False); Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Sprite));
                }
            }
        }
    }
}
