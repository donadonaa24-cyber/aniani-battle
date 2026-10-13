using System;
using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class DamageRecoilTests
    {
        private static void Damage(BattleSimulation sim, AttackDefinition attack, int facing, long id = 900)
        {
            new DamageSystem().Apply(new HitContact(sim.P1, sim.P2, new AttackInstance(id, attack, 0, false), facing: facing),
                new ComboTracker(), sim.Match.TickCount, sim.Rules, new List<BattleEvent>());
        }
        [TestCase(AttackKind.Normal, 1)] [TestCase(AttackKind.Normal, -1)]
        [TestCase(AttackKind.Special, 1)] [TestCase(AttackKind.Special, -1)]
        [TestCase(AttackKind.Ultimate, 1)] [TestCase(AttackKind.Ultimate, -1)]
        public void UnobstructedDamageAddsExactlyTheSameDistanceAsOneNormalStep(AttackKind kind, int facing)
        {
            var sim = BattleTestFactory.Create();
            var attack = kind == AttackKind.Normal ? PhaseOneDefaults.Normal() : kind == AttackKind.Special ? PhaseTwoDefaults.Special(SpecialSlot.Side) : PhaseTwoDefaults.Ultimate();
            attack.Knockback = default; attack.HitstopFrames = 0;
            float before = sim.P2.Position.X;
            Damage(sim, attack, facing);
            for (int tick = 1; tick <= 6; tick++)
            {
                sim.Tick(default, default);
                Assert.That(sim.P2.Position.X, Is.EqualTo(before + facing * .03f * tick).Within(.00002f));
                Assert.That(sim.P2.DamageRecoilRemaining, Is.EqualTo(6 - tick));
            }
            float end = sim.P2.Position.X; BattleTestFactory.Idle(sim, 3);
            Assert.That(sim.P2.Position.X, Is.EqualTo(end));
            Assert.That(Math.Abs(end - before), Is.EqualTo(PhaseOneDefaults.Normal().NormalForwardDistance).Within(.00002f));
        }
        [Test]
        public void RecoilAddsToKnockbackWithoutReplacingItsOriginalVelocity()
        {
            var sim = BattleTestFactory.Create(); var attack = PhaseOneDefaults.Normal(); attack.HitstopFrames = 0;
            float start = sim.P2.Position.X; Damage(sim, attack, 1);
            for (int i = 0; i < 6; i++)
            {
                sim.Tick(default, default);
                Assert.That(sim.P2.Velocity.X, Is.EqualTo(attack.Knockback.X));
            }
            Assert.That(sim.P2.Position.X - start, Is.EqualTo(.18f + attack.Knockback.X * 6 / 60).Within(.00002f));
        }
        [Test]
        public void HitstopPausesRecoilAndRepeatedHitReplacesRatherThanAccumulatesIt()
        {
            var sim = BattleTestFactory.Create(); var attack = PhaseOneDefaults.Normal(); attack.Knockback = default;
            Damage(sim, attack, 1); float start = sim.P2.Position.X;
            for (int i = 0; i < attack.HitstopFrames; i++)
            { sim.Tick(default, default); Assert.That(sim.P2.Position.X, Is.EqualTo(start)); Assert.That(sim.P2.DamageRecoilRemaining, Is.EqualTo(6)); }
            sim.Tick(default, default); Assert.That(sim.P2.DamageRecoilRemaining, Is.EqualTo(5));
            attack.HitstopFrames = 0; Damage(sim, attack, -1, 901); float replacementStart = sim.P2.Position.X;
            BattleTestFactory.Idle(sim, 6);
            Assert.That(sim.P2.Position.X, Is.EqualTo(replacementStart - .18f).Within(.00002f));
        }
        [Test]
        public void SeverityTracksActualHeartLossIncludingThirdNormalHitAndFinalKo()
        {
            var sim = BattleTestFactory.Create(); var normal = PhaseOneDefaults.Normal();
            Damage(sim, normal, 1); Assert.That(sim.P2.LastDamageLostHeart, Is.False);
            Damage(sim, normal, 1, 901); Assert.That(sim.P2.LastDamageLostHeart, Is.False);
            Damage(sim, normal, 1, 902); Assert.That(sim.P2.LastDamageLostHeart, Is.True); Assert.That(sim.P2.Hearts, Is.EqualTo(2));
            Damage(sim, normal, 1, 903); Assert.That(sim.P2.LastDamageLostHeart, Is.False);
            Damage(sim, PhaseTwoDefaults.Ultimate(), 1, 904);
            Assert.That(sim.P2.LastDamageLostHeart, Is.True); Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.KO));
            Assert.That(sim.P2.DamageRecoilRemaining, Is.Zero);
            float end = sim.P2.Position.X; sim.Tick(default, default); Assert.That(sim.P2.Position.X, Is.EqualTo(end));
        }
        [Test]
        public void GuardAndDodgeDoNotStartDamageRecoil()
        {
            foreach (var defense in new[] { InputButtons.Guard, InputButtons.Dodge })
            {
                var sim = BattleTestFactory.Create(adjacent: true);
                sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(held: defense, pressed: defense));
                for (int i = 0; i < 5; i++) sim.Tick(default, new FrameInput(held: defense));
                Assert.That(sim.P2.NormalHitCount, Is.Zero); Assert.That(sim.P2.DamageRecoilRemaining, Is.Zero);
                Assert.That(sim.P2.LastDamageLostHeart, Is.False);
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void AllRosterAttackKindsCarryTheSameSmallReaction(int character)
        {
            var c = PhaseSixADefaults.Character(character);
            foreach (var a in c.GroundNormals.Concat(c.AirNormals).Concat(c.DirectionalNormals.Skip(1)).Concat(c.Specials).Concat(new[] { c.Ultimate }))
            { Assert.That(a.DamageRecoilDistance, Is.EqualTo(.18f), a.Name); Assert.That(a.DamageRecoilFrames, Is.EqualTo(6), a.Name); }
        }
        [TestCase(-1f, 6)] [TestCase(float.NaN, 6)] [TestCase(float.PositiveInfinity, 6)] [TestCase(.18f, 0)] [TestCase(.18f, -1)]
        public void InvalidRecoilDataIsRejected(float distance, int frames)
        {
            var attack = PhaseOneDefaults.Normal(); attack.DamageRecoilDistance = distance; attack.DamageRecoilFrames = frames;
            Assert.Throws<ArgumentException>(() => attack.Validate());
        }

        [TestCase(1)] [TestCase(-1)]
        public void WallBlocksRecoilWithoutSavingTheBlockedDistanceForLater(int facing)
        {
            var character = PhaseOneDefaults.Character();
            var stage = new StageGeometry(new[] {
                new StagePlatform(new BattleRect(new BattleVector(0, -.6f), new BattleVector(36, 1.2f)), false),
                new StagePlatform(new BattleRect(new BattleVector(facing * 1.1f, 3), new BattleVector(1, 6)), false)
            }, -24, 24, -10, new BattleVector(-facing * 5, 1.1f), new BattleVector(0, 1.1f));
            var sim = new BattleSimulation(character, character, PhaseOneDefaults.Rules(), stage);
            var attack = PhaseOneDefaults.Normal(); attack.HitstopFrames = 0; attack.Knockback = default;
            Damage(sim, attack, facing); BattleTestFactory.Idle(sim, 6);
            Assert.That(sim.P2.Position.X, Is.EqualTo(facing * .05f).Within(.00002f));
            Assert.That(sim.P2.DamageRecoilRemaining, Is.Zero);
            float end = sim.P2.Position.X; BattleTestFactory.Idle(sim, 3); Assert.That(sim.P2.Position.X, Is.EqualTo(end));
        }

        [Test]
        public void PracticeRespawnClearsThePreviousHitReaction()
        {
            var sim = BattleTestFactory.Create(); sim.Practice = new PracticeOptions();
            Damage(sim, PhaseOneDefaults.Normal(), 1);
            sim.P2.Position = new BattleVector(25, 1.1f); sim.Practice.FinishTick(sim);
            Assert.That(sim.P2.Position.X, Is.EqualTo(sim.Stage.SpawnP2.X));
            Assert.That(sim.P2.DamageRecoilRemaining, Is.Zero); Assert.That(sim.P2.DamageRecoilSpeed, Is.Zero);
            Assert.That(sim.P2.LastDamageLostHeart, Is.False);
        }
    }
}
