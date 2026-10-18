using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperChargeArtworkTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void AllNineUseOneDistinctNativeChargeKeyAndReturnToUnchangedActionPoses(int index)
        {
            var entry = PaperChargeArtworkBuilder.Read().characters.Single(c => c.characterIndex == index);
            var paper = Setup.Character(index).PaperMotion; var pose = paper.Charging;
            Assert.That(pose.Valid, Is.True); Assert.That(pose.States, Is.Empty);
            var sprite = pose.Frames.Single(); Assert.That(sprite, Is.Not.SameAs(paper.Sprite));
            Assert.That(paper.Poses.SelectMany(p => p.Frames), Has.No.Member(sprite));
            Assert.That(sprite.texture.width, Is.EqualTo(1304)); Assert.That(sprite.texture.height, Is.EqualTo(1450));
            Assert.That(sprite.pixelsPerUnit, Is.EqualTo(532.5893f).Within(.001));
            Assert.That(sprite.pivot, Is.EqualTo(new Vector2(652, 122)));
            Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)), Is.EqualTo(File.ReadAllBytes("../" + entry.frames.Single().prepared)));
            foreach (bool flip in new[] { false, true }) foreach (int age in new[] { 0, 59, 60, 120, 180, 360 })
            {
                var sample = new AnimationSample { State = AnimationState.Idle, IsCharging = true, Age = age, FlipX = flip };
                Assert.That(paper.TrySprite(sample, false, out var actual, out var selected), Is.True);
                Assert.That(actual, Is.SameAs(sprite)); Assert.That(selected, Is.SameAs(pose));
                Assert.That(paper.Transform(sample, pose).Degrees, Is.Zero);
                Assert.That(paper.Transform(sample, pose).Lift, Is.Zero);
            }
            var idle = new AnimationSample { State = AnimationState.Idle };
            Assert.That(paper.TrySprite(idle, false, out var standing, out _), Is.True); Assert.That(standing, Is.SameAs(paper.Sprite));
            foreach (var state in new[] { AnimationState.Attack1, AnimationState.Attack2, AnimationState.Attack3 })
                Assert.That(paper.Find(state).Frames.Length, Is.EqualTo(2));
            Assert.That(paper.Find(AnimationState.AirNeutral).Frames.Length, Is.EqualTo(1));
            Assert.That(paper.Find(AnimationState.CrouchAttack).Frames.Length, Is.EqualTo(1));
        }
        [Test]
        public void ChargeSampleHonorsFreezeAndClearsOnReleaseAirborneDamageAndResult()
        {
            var sim = new BattleSimulation(PhaseThreeDefaults.Character(0), PhaseThreeDefaults.Character(2), PhaseSixADefaults.Rules(), PhaseOneDefaults.Stage());
            var player = new FighterAnimationPlayer(); sim.P1.Mode = FighterMode.Charging; sim.P1.Grounded = true;
            player.Reset(sim.P1, sim.Match); Assert.That(player.Sample.IsCharging, Is.True);
            sim.P1.Mode = FighterMode.Hitstun; sim.P1.HitstunRemaining = 5;
            player.Advance(sim.P1, sim.Match, true, false, false); Assert.That(player.Sample.IsCharging, Is.True);
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.IsCharging, Is.False);
            sim.P1.HitstunRemaining = 0; sim.P1.Mode = FighterMode.Charging; sim.P1.Grounded = false;
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.IsCharging, Is.False);
            sim.P1.Grounded = true; player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.IsCharging, Is.True);
            sim.P1.Mode = FighterMode.Attack; sim.P1.Attack = new AttackInstance(1, sim.P1.Definition.GroundNormals[0], 0, false);
            player.Advance(sim.P1, sim.Match, false, false, false); Assert.That(player.Sample.IsCharging, Is.False); Assert.That(player.Sample.IsAttack, Is.True);
            sim.P1.Attack = null; sim.P1.Mode = FighterMode.Charging; sim.Match.Result = MatchResult.Player1Wins;
            player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(player.Sample.IsCharging, Is.False); Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Win));
        }
        [Test]
        public void WebCopiesKeepChargeFramesAndLeaveOriginalMotionDataUnchanged()
        {
            var originals = Setup.Characters.Select(c => JsonUtility.ToJson(c.PaperMotion)).ToArray();
            try
            {
                var copy = WebGLScenePacking.CopyForBuild(Setup);
                for (int index = 0; index < 9; index++)
                {
                    var source = Setup.Character(index).PaperMotion; var packed = copy.Character(index).PaperMotion;
                    Assert.That(packed, Is.Not.SameAs(source)); Assert.That(packed.Charging, Is.Not.SameAs(source.Charging));
                    Assert.That(packed.Charging.Frames.Single(), Is.SameAs(source.Charging.Frames.Single()));
                    Assert.That(packed.Charging.HeadHeight, Is.EqualTo(source.Charging.HeadHeight));
                }
                Assert.That(Setup.Characters.Select(c => JsonUtility.ToJson(c.PaperMotion)), Is.EqualTo(originals));
            }
            finally { WebGLScenePacking.ReleaseCopies(); }
        }
    }
}
