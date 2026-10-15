using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class FryingPanAssetTests
    {
        private static CharacterData Chizuru => AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Chizuru.asset");
        [Test]
        public void EveryNormalUsesTwoDifferentPanPicturesAtTheExactCombatPhase()
        {
            AnimationState[] states = { AnimationState.Attack1, AnimationState.Attack2, AnimationState.Attack3, AnimationState.CrouchAttack,
                AnimationState.BackAttack, AnimationState.AirAttack, AnimationState.AirNeutral, AnimationState.AirForward,
                AnimationState.AirBack, AnimationState.AirUp, AnimationState.AirDown, AnimationState.AirForwardUp, AnimationState.AirForwardDown };
            foreach (var state in states)
            {
                var pose = Chizuru.PaperMotion.Find(state);
                Assert.That(pose.Frames.Length, Is.EqualTo(2)); Assert.That(pose.Frames[0], Is.Not.SameAs(pose.Frames[1]));
                Assert.That(pose.ReverseFacing, Is.EqualTo(state == AnimationState.BackAttack || state == AnimationState.AirBack));
                foreach (var sprite in pose.Frames) Assert.That(AssetDatabase.GetAssetPath(sprite), Does.Contain("_anticipation_").Or.Contain("_impact_"));
                // Every chosen source is now one of the six horizontal originals, not the vertical set.
                var manifest=JsonUtility.FromJson<AniBato.EditorTools.PaperActionBuilder.Manifest>(File.ReadAllText("../art_source/paper_cutout/runtime-manifest.json"));
                var row=manifest.poses.First(p=>p.states.Contains((int)state));
                Assert.That(row.frames.All(f=>f.source.Contains("/horizontal_")),Is.True);
                for (int frame = 0; frame < 38; frame++)
                {
                    var sample = new AnimationSample { State = state, IsAttack = true, Age = frame, AttackFrame = frame, Startup = 11, Active = 6, Recovery = 21 };
                    Assert.That(pose.Frame(sample), Is.SameAs(pose.Frames[frame < 11 ? 0 : 1]));
                }
            }
        }
        [Test]
        public void HeldPanFinisherRetainsTimingDamageAndForwardMotionAndKeepsBagSources()
        {
            var old = JsonUtility.FromJson<AttackDefinition>(File.ReadAllText("../art_source/paper_cutout/variants/bag-ground3-definition.json"));
            var current = Chizuru.GroundNormals[2].ToDefinition();
            Assert.That(old.SpawnProjectile, Is.True); Assert.That(current.SpawnProjectile, Is.False); Assert.That(current.IsCloseRange, Is.True);
            Assert.That(current.StartupFrames, Is.EqualTo(old.StartupFrames)); Assert.That(current.ActiveFrames, Is.EqualTo(old.ActiveFrames));
            Assert.That(current.RecoveryFrames, Is.EqualTo(old.RecoveryFrames)); Assert.That(current.HitstunFrames, Is.EqualTo(old.HitstunFrames));
            Assert.That(current.Kind, Is.EqualTo(old.Kind)); Assert.That(current.Knockback, Is.EqualTo(old.Knockback));
            Assert.That(current.HitstopFrames, Is.EqualTo(old.HitstopFrames)); Assert.That(current.DamageDelayFrames, Is.EqualTo(old.DamageDelayFrames));
            Assert.That(current.NormalForwardDistance, Is.EqualTo(old.NormalForwardDistance));
            Assert.That(current.DamageRecoilDistance, Is.EqualTo(old.DamageRecoilDistance));
            var before = JsonUtility.FromJson<AniBato.EditorTools.PaperActionBuilder.Manifest>(File.ReadAllText("../art_source/paper_cutout/variants/bag-runtime-manifest.json"));
            foreach (var row in before.poses.Where(p => p.key.StartsWith("attack") || p.key.StartsWith("air_") || p.key == "back_attack" || p.key == "crouch_attack"))
                using (var sha = SHA256.Create()) Assert.That(System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes("../" + row.source))).Replace("-", "").ToLowerInvariant(), Is.EqualTo(row.sha256));
        }
        [TestCase(1)] [TestCase(-1)]
        public void PanFinisherHitsOnceAtMeleeRangeWithoutSpawningAProjectile(int facing)
        {
            var character = Chizuru.ToDefinition(); character.GroundNormals = new[] { character.GroundNormals[2] };
            var sim = BattleTestFactory.Create(definition: character); sim.P1.Position = new BattleVector(0, 1.1f); sim.P1.Facing = facing;
            sim.P2.Position = new BattleVector(1.98f * facing, 1.1f); sim.P2.NormalHitCount = 2; int hits = 0;
            for (int tick = 0; tick < 90; tick++)
            {
                sim.Tick(tick == 0 ? new FrameInput(pressed: InputButtons.Attack) : default, default);
                hits += sim.Events.Count(e => e.Kind == BattleEventKind.Hit); Assert.That(sim.Projectiles.Count, Is.Zero);
            }
            Assert.That(hits, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
        }
    }
}
