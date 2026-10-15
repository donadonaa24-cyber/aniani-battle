using System.Collections;
using System.Linq;
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
        [UnityTest]
        public IEnumerator CompletedCoreProfilesUseRealInputsAndBothDirectionsWithStableSoles()
        {
            var characters = runner.Setup.Characters.Where(c => c.PaperMotion && c.PaperMotion.SchemaVersion == 3).ToArray();
            Assert.That(characters.Length, Is.GreaterThanOrEqualTo(1)); var keys = controller.Bindings.P1;
            foreach (var character in characters)
            {
                string directory = "paper_core_" + (character.Movement.Name == "透真" ? "toma" : character.Movement.Name == "蓮" ? "ren" : character.PaperMotion.name.Replace("CorePaperMotion", ""));
                foreach (int direction in new[] { 1, -1 })
                {
                    runner.BeginMatch(character, runner.Setup.Character(2), runner.CurrentStage, false); view.RenderFrame();
                    Assert.That(view.UsesPaperSprite, Is.True); Assert.That(body.sprite, Is.SameAs(character.PaperMotion.Sprite));
                    yield return Key(direction > 0 ? keys.Right : keys.Left, true); Tick();
                    Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Move)); Assert.That(body.flipX, Is.EqualTo(direction < 0));
                    Assert.That(Angle(body), Is.EqualTo(direction > 0 ? -15 : 15).Within(.001)); Capture("walk_" + direction, true, directory);
                    yield return Key(direction > 0 ? keys.Right : keys.Left, false); Tick();
                    Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Idle)); Capture("idle_" + direction, true, directory);
                    var actions = new[] { keys.Guard, keys.Down, keys.Counter }; var expected = new[] { AnimationState.Guard, AnimationState.Crouch, AnimationState.Counter };
                    for (int action = 0; action < actions.Length; action++)
                    {
                        yield return Key(actions[action], true); Tick();
                        Assert.That(view.RenderedSample.State, Is.EqualTo(expected[action]));
                        Assert.That(view.UsesPaperSprite, Is.True); Assert.That(body.sprite, Is.SameAs(character.PaperMotion.Find(expected[action]).Frame(0)));
                        Assert.That(body.flipX, Is.EqualTo(direction < 0)); Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one));
                        Assert.That(Angle(body), Is.EqualTo(0).Within(.001));
                        Assert.That(body.transform.localPosition.y, Is.EqualTo(-runner.Simulation.P1.Definition.HurtboxSize.Y * .5f).Within(.0001));
                        Capture(expected[action] + "_" + direction, true, directory);
                        yield return Key(actions[action], false); for (int tick = 0; tick < 50; tick++) Tick();
                        Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Idle));
                    }
                    var player = ((FighterAnimationPlayer[])typeof(BattleRunner).GetField("animations", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(runner))[0];
                    var property = typeof(FighterAnimationPlayer).GetProperty("Sample"); var original = player.Sample;
                    int ticks = runner.Simulation.Match.TickCount; var position = runner.Simulation.P1.Position;
                    try
                    {
                        foreach (var state in new[] { AnimationState.Win, AnimationState.Lose })
                        {
                            property.SetValue(player, new AnimationSample { State = state, FlipX = direction < 0 }); view.RenderFrame();
                            Assert.That(view.UsesPaperSprite, Is.True); Assert.That(body.sprite, Is.SameAs(character.PaperMotion.Find(state).Frame(0)));
                            Capture(state + "_" + direction, true, directory);
                        }
                        Assert.That(runner.Simulation.Match.TickCount, Is.EqualTo(ticks)); Assert.That(runner.Simulation.P1.Position, Is.EqualTo(position));
                    }
                    finally { property.SetValue(player, original); view.RenderFrame(); }
                }
            }
        }
    }
}
