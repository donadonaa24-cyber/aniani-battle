using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class ChargedAttackTests
    {
        private static BattleSimulation Create()
        {
            var rules = PhaseSixADefaults.Rules(); rules.Charge = ChargeAttackDefinition.Default;
            return BattleTestFactory.Create(true, rules, BattleTestFactory.StationaryAttack(startup: 0));
        }
        private static void Hold(BattleSimulation sim, int frames)
        {
            sim.Tick(new FrameInput(held: InputButtons.Attack, pressed: InputButtons.Attack), default);
            for (int i = 1; i < frames; i++) sim.Tick(new FrameInput(held: InputButtons.Attack), default);
        }
        [TestCase(1, 0)] [TestCase(59, 0)] [TestCase(60, 60)] [TestCase(120, 120)] [TestCase(180, 180)] [TestCase(360, 180)]
        public void ReleaseUsesExactOneAndThreeSecondBoundariesAndNeverAutoFires(int frames, int chargedFrames)
        {
            var sim = Create(); Hold(sim, frames);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Charging)); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P2.NormalHitCount, Is.Zero); Assert.That(sim.P1.ChargeFrames, Is.EqualTo(System.Math.Min(frames, 180)));
            sim.Tick(default, default);
            Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P1.Attack.ChargeFrames, Is.EqualTo(chargedFrames));
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.Hit), Is.EqualTo(1));
            BattleTestFactory.Idle(sim, 50); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
            Assert.That(sim.P1.ChargeFrames, Is.Zero); Assert.That(sim.P1.Attack, Is.Null);
        }
        [TestCase(1)] [TestCase(-1)]
        public void FullyChargedHitTravelsToCurrentMainPlatformEdgeInBothDirections(int direction)
        {
            foreach (bool shrunk in new[] { false, true })
            {
                var sim = Create(); sim.P1.Facing = direction;
                if (shrunk) sim.Stage.SetPlatform(0, new StagePlatform(new BattleRect(new BattleVector(0, -.5f), new BattleVector(10, 1)), false));
                sim.P2.Position = new BattleVector(sim.P1.Position.X + 1.5f * direction, sim.P1.Position.Y);
                Hold(sim, 180); sim.Tick(default, default);
                Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Knockback));
                float edge = direction > 0 ? sim.Stage[0].Bounds.Right : sim.Stage[0].Bounds.Left;
                float expected = edge - direction * (sim.P2.Definition.HurtboxSize.X * .5f + sim.Rules.Charge.EdgeInset);
                BattleTestFactory.Idle(sim, sim.Rules.Charge.HitstunFrames);
                Assert.That(sim.P2.Position.X, Is.EqualTo(expected).Within(.002));
                Assert.That(sim.Stage.IsOutside(sim.P2.Position), Is.False); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            }
        }
        [Test]
        public void MoreChargeIncreasesTravelWithoutIncreasingNormalDamage()
        {
            float previous = 0;
            foreach (int frames in new[] { 60, 120, 180 })
            {
                var sim = Create(); float start = sim.P2.Position.X;
                Hold(sim, frames); sim.Tick(default, default); BattleTestFactory.Idle(sim, sim.Rules.Charge.HitstunFrames);
                float travel = sim.P2.Position.X - start;
                Assert.That(travel, Is.GreaterThan(previous)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
                Assert.That(sim.P2.Hearts, Is.EqualTo(3)); previous = travel;
            }
        }
        [Test]
        public void IncomingAttackInterruptsChargeAndAirborneNormalStartsWithoutCharge()
        {
            var sim = Create(); Hold(sim, 120);
            sim.Tick(new FrameInput(held: InputButtons.Attack), new FrameInput(held: InputButtons.Attack, pressed: InputButtons.Attack));
            sim.Tick(new FrameInput(held: InputButtons.Attack), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Hitstun)); Assert.That(sim.P1.ChargeFrames, Is.Zero);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1));
            sim = Create(); sim.P1.Position = new BattleVector(0, 8); sim.P1.Grounded = false;
            sim.Tick(new FrameInput(held: InputButtons.Attack, pressed: InputButtons.Attack), default);
            Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P1.Attack.AirborneAttack, Is.True);
            Assert.That(sim.P1.ChargeFrames, Is.Zero);
        }
        [Test]
        public void ChargedCrouchGuardBreakKeepsTheSameEdgeLaunchAndOneNormalHit()
        {
            var rules = PhaseSixADefaults.Rules(); rules.Charge = ChargeAttackDefinition.Default;
            var definition = PhaseSixADefaults.Character(0);
            var down = definition.DirectionalNormals[1]; down.StartupFrames = 0;
            down.Hitbox = PhaseOneDefaults.Normal().Hitbox; down.NormalForwardDistance = 0; down.HitstopFrames = 0;
            definition.DirectionalNormals[1] = down;
            var sim = BattleTestFactory.Create(true, rules, definition);
            for (int i = 0; i < 180; i++) sim.Tick(new FrameInput(y: -1, held: InputButtons.Attack, pressed: i == 0 ? InputButtons.Attack : InputButtons.None), new FrameInput(held: InputButtons.Guard));
            sim.Tick(default, new FrameInput(held: InputButtons.Guard));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.GuardBroken), Is.True);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            float edge = sim.Stage[0].Bounds.Right - sim.P2.Definition.HurtboxSize.X * .5f - rules.Charge.EdgeInset;
            BattleTestFactory.Idle(sim, rules.Charge.HitstunFrames);
            Assert.That(sim.P2.Position.X, Is.EqualTo(edge).Within(.002));
        }
        [Test]
        public void CurrentGameRulesExplicitlyEnableChargeWithoutChangingLegacyFixture()
        {
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            Assert.That(rules.SchemaVersion, Is.GreaterThanOrEqualTo(8)); Assert.That(rules.Charge.Enabled, Is.True);
            Assert.That(rules.Charge.MinimumFrames, Is.EqualTo(60)); Assert.That(rules.Charge.MaximumFrames, Is.EqualTo(180));
            Assert.That(PhaseSixADefaults.Rules().Charge.Enabled, Is.False);
            var invalid = rules; invalid.Charge.MaximumFrames = 60; Assert.Throws<System.ArgumentException>(() => invalid.Validate());
        }
    }
}
