using System;
using System.IO;
using System.Linq;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperRosterMotionTests
    {
        [TestCase("toma")] [TestCase("ren")] [TestCase("minato")] [TestCase("shiro")]
        [TestCase("yui")] [TestCase("king")] [TestCase("akatsuki")] [TestCase("ryuta")]
        public void CompletedMotionUsesSevenDistinctNativeImagesAndPreservesAllOtherPoses(string key)
        {
            var entry = PaperRosterMotionBuilder.Completed().SingleOrDefault(m => m.key == key);
            if (entry == null) { Assert.That(File.Exists(PaperRosterMotionBuilder.PathFor(key)), Is.False); return; }
            var setup = AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            var paper = setup.Character(entry.characterIndex).PaperMotion;
            Assert.That(paper.WalkLeanDegrees, Is.EqualTo(15));
            Assert.That(paper.DashLeanDegrees, Is.EqualTo(key == "shiro" ? 10 : 45));
            var states = new[] { AnimationState.Dash, AnimationState.Jump, AnimationState.Fall, AnimationState.Dodge,
                AnimationState.CounterSuccess, AnimationState.Hitstun };
            string[] ids = { "run", "jump", "fall", "dodge", "counter_success", "hit_light" };
            for (int i = 0; i < states.Length; i++) Assert.That(paper.Find(states[i]).Key, Is.EqualTo(ids[i]));
            Assert.That(paper.Find(AnimationState.Knockback), Is.SameAs(paper.Find(AnimationState.Hitstun)));
            Assert.That(paper.Find(AnimationState.Hitstun, true), Is.SameAs(paper.HeavyDamage));
            Assert.That(paper.Find(AnimationState.Knockback, true), Is.SameAs(paper.HeavyDamage));
            Assert.That(paper.Find(AnimationState.CounterSuccess), Is.Not.SameAs(paper.Find(AnimationState.Counter)));
            Assert.That(paper.Find(AnimationState.Counter).Key, Is.EqualTo("counter"));
            var poses = states.Select(s => paper.Find(s)).Concat(new[] { paper.HeavyDamage }).ToArray();
            Assert.That(poses.Select(p => p.Frames.Single()).Distinct().Count(), Is.EqualTo(7));
            foreach (var pose in poses)
            {
                var sprite = pose.Frames.Single(); var source = entry.frames.Single(f => f.id == pose.Key);
                Assert.That(sprite, Is.Not.SameAs(paper.Sprite));
                Assert.That(sprite.texture.width, Is.EqualTo(1304)); Assert.That(sprite.texture.height, Is.EqualTo(1450));
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(532.5893f).Within(.001));
                Assert.That(sprite.pivot, Is.EqualTo(new Vector2(652, 122)));
                Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)), Is.EqualTo(File.ReadAllBytes("../" + source.prepared)));
                Assert.That(pose.Frame(10000), Is.SameAs(sprite), "1原画を保持する");
            }
            foreach (AnimationState state in Enum.GetValues(typeof(AnimationState)))
                Assert.That(paper.Poses.Count(p => p.States.Contains(state)), Is.EqualTo(1), state.ToString());
            foreach (var state in new[] { AnimationState.Attack1, AnimationState.Attack2, AnimationState.Attack3 })
                Assert.That(paper.Find(state).Frames.Length, Is.EqualTo(2));
            Assert.That(paper.Find(AnimationState.AirNeutral).Frames.Length, Is.EqualTo(1));
            Assert.That(paper.Find(AnimationState.CrouchAttack).Frames.Length, Is.EqualTo(1));
            Assert.That(paper.Find(AnimationState.Idle).Frames.Single(), Is.SameAs(paper.Sprite));
        }
    }
}
