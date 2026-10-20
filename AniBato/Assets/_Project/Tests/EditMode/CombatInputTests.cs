using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class CombatInputTests
    {
        private static BattleRuleDefinition Rules()
        { var r = PhaseSixADefaults.Rules(); r.CombatInput = CombatInputDefinition.Default; return r; }
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [Test]
        public void EveryCurrentCharacterSelectsUpDownAndOneAirNormalInEveryDirection()
        {
            Assert.That(Setup.Characters.Length, Is.EqualTo(9));
            foreach (var data in Setup.Characters)
            {
                var definition = data.ToDefinition(); var sim = BattleTestFactory.Create(false, Rules(), definition);
                Assert.That(definition.UpNormal.ActiveFrames, Is.GreaterThan(0), definition.Name);
                Assert.That(definition.UpNormal.NormalDirection, Is.EqualTo(NormalDirection.Up));
                Assert.That(DirectionalAttackSelector.Select(sim.P1, new BattleVector(0, 1), .5f, true), Is.EqualTo(NormalDirection.Up));
                Assert.That(DirectionalAttackSelector.Select(sim.P1, new BattleVector(0, -1), .5f, true), Is.EqualTo(NormalDirection.Crouch));
                sim.Tick(new FrameInput(y: 1, pressed: InputButtons.Attack), default);
                Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(definition.UpNormal.Name));
                for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
                {
                    sim = BattleTestFactory.Create(false, Rules(), definition);
                    sim.P1.Grounded = false; sim.P1.GroundPlatform = -1; sim.P1.Position = new BattleVector(0, 100);
                    sim.Tick(new FrameInput(x, y, InputButtons.Attack, InputButtons.Attack), default);
                    Assert.That(sim.P1.Attack.Definition.NormalDirection, Is.EqualTo(NormalDirection.AirNeutral), definition.Name);
                    Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(definition.DirectionalNormals[3].Name));
                }
            }
        }
        [TestCase(1, 0)] [TestCase(-1, 0)] [TestCase(0, -1)] [TestCase(1, 1)]
        public void AirNormalPreservesOrdinaryHorizontalMotionGravityAndFastFall(float x, float y)
        {
            foreach (var data in Setup.Characters)
            {
                var a = BattleTestFactory.Create(false, Rules(), data.ToDefinition());
                var b = BattleTestFactory.Create(false, Rules(), data.ToDefinition());
                foreach (var sim in new[] { a, b })
                { sim.P1.Position = new BattleVector(0, 100); sim.P1.Grounded = false; sim.P1.GroundPlatform = -1; sim.P1.Velocity = new BattleVector(2, 3); }
                for (int tick = 0; tick < 40; tick++)
                {
                    float direction = tick < 12 ? x : -x;
                    a.Tick(new FrameInput(direction, y, tick == 0 ? InputButtons.Attack : InputButtons.None,
                        tick == 0 ? InputButtons.Attack : InputButtons.None, disableDoubleTapDash: true), default);
                    b.Tick(new FrameInput(direction, y, disableDoubleTapDash: true), default);
                    Assert.That(a.P1.Position.X, Is.EqualTo(b.P1.Position.X).Within(.00001), data.name + " tick " + tick);
                    Assert.That(a.P1.Position.Y, Is.EqualTo(b.P1.Position.Y).Within(.00001));
                    Assert.That(a.P1.Velocity.Y, Is.EqualTo(b.P1.Velocity.Y).Within(.00001));
                }
            }
        }
        [Test]
        public void AllSpecialDirectionsUseTheSameNeutralSlotAndSharedCooldownForAllNine()
        {
            foreach (var data in Setup.Characters) for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
            {
                var sim = BattleTestFactory.Create(false, Rules(), data.ToDefinition());
                sim.Tick(new FrameInput(x, y, pressed: InputButtons.Special), default);
                Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Neutral));
                Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(data.Specials[0].Definition.Name));
                Assert.That(sim.P1.UpSpecialUsed, Is.False);
                Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Neutral), Is.EqualTo(data.Specials[0].Definition.CooldownFrames));
            }
        }
        [Test]
        public void SameTickActiveNormalsClashOnceWithoutDamageAndClearQueuedAttacks()
        {
            var sim = BattleTestFactory.Create(true, Rules(), BattleTestFactory.StationaryAttack(startup: 0));
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(pressed: InputButtons.Attack));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.Clash), Is.EqualTo(1));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Hit), Is.False);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P2.Attack, Is.Null);
            Assert.That(sim.P1.NormalHitCount + sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.P1.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.P1.HitstopRemaining, Is.EqualTo(sim.Rules.CombatInput.ClashStopFrames));
            Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Recovery));
            for (int i = 0; i < 30; i++) { sim.Tick(default, default); Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Clash), Is.False); }
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Idle)); Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Idle));
        }
        [TestCase(false)] [TestCase(true)]
        public void StartupAndRecoveryDoNotClashWithAnActiveNormal(bool recovery)
        {
            var c = BattleTestFactory.StationaryAttack(startup: 3); var sim = BattleTestFactory.Create(true, Rules(), c);
            sim.P1.Attack = new AttackInstance(80, c.GroundNormals[0], 0, false) { Frame = 2 };
            sim.P2.Attack = new AttackInstance(81, c.GroundNormals[0], 0, false) { Frame = recovery ? 5 : -1 };
            sim.Tick(default, default);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Clash), Is.False);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P1.NormalHitCount, Is.Zero);
        }
        [Test]
        public void DifferentStartTimesStillClashWhenTheirActiveWindowsMeet()
        {
            var c = BattleTestFactory.StationaryAttack(activeFrames: 5, startup: 3); var sim = BattleTestFactory.Create(true, Rules(), c);
            sim.P1.Attack = new AttackInstance(80, c.GroundNormals[0], 0, false) { Frame = 3 };
            sim.P2.Attack = new AttackInstance(81, c.GroundNormals[0], 0, false) { Frame = 2 };
            sim.Tick(default, default);
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.Clash), Is.EqualTo(1));
            Assert.That(sim.P1.NormalHitCount + sim.P2.NormalHitCount, Is.Zero);
        }
        [Test]
        public void SecondaryNormalBoxAndNormalProjectileBothParticipateInClashes()
        {
            var c = BattleTestFactory.StationaryAttack(startup: 0); var a = c.GroundNormals[0];
            a.HasSecondaryHitbox = true; a.SecondaryHitbox = a.Hitbox;
            a.Hitbox = new BattleRect(new BattleVector(40, 0), new BattleVector(1, 1)); c.GroundNormals[0] = a;
            var sim = BattleTestFactory.Create(true, Rules(), c);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(pressed: InputButtons.Attack));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.Clash), Is.EqualTo(1));
            a.HasSecondaryHitbox = false; a.Hitbox = PhaseOneDefaults.Normal().Hitbox;
            a.SpawnProjectile = true; a.ProjectileSpeed = 1; a.ProjectileLifetimeFrames = 30; c.GroundNormals[0] = a;
            sim = BattleTestFactory.Create(true, Rules(), c);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(pressed: InputButtons.Attack));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.Clash), Is.EqualTo(1));
            Assert.That(sim.Projectiles.Count, Is.Zero); Assert.That(sim.P1.NormalHitCount + sim.P2.NormalHitCount, Is.Zero);
        }
        [TestCase(InputButtons.Special)] [TestCase(InputButtons.Ultimate)]
        public void SpecialAndUltimateKeepTheirExistingContactRules(InputButtons button)
        {
            var c = BattleTestFactory.StationaryAttack(startup: 0); var a = c.GroundNormals[0];
            a.Kind = AttackKind.Special; c.Specials[0] = a;
            a.Kind = AttackKind.Ultimate; c.Ultimate = a; c.UltimateCutInFrames = 0;
            var sim = BattleTestFactory.Create(true, Rules(), c); sim.P2.Gauge = sim.Rules.Gauge.Maximum;
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(pressed: button));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Clash), Is.False);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.SourcePlayer == 1), Is.True);
        }
        [Test]
        public void PracticeStartsFullAndAllowsRepeatedUltimatesEvenWhenHeartsAreNotFrozen()
        {
            var c = BattleTestFactory.StationaryAttack(startup: 0); var sim = BattleTestFactory.Create(false, Rules(), c);
            sim.Practice = new PracticeOptions { FreezeHearts = false };
            Assert.That(sim.P1.Gauge, Is.EqualTo(sim.Rules.Gauge.Maximum)); Assert.That(sim.P2.Gauge, Is.EqualTo(sim.Rules.Gauge.Maximum));
            for (int use = 1; use <= 3; use++)
            {
                sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
                Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.UltimateStarted), Is.True);
                Assert.That(sim.P1.UltimateUses, Is.EqualTo(use)); Assert.That(sim.P1.Gauge, Is.EqualTo(sim.Rules.Gauge.Maximum));
                int frames = c.Ultimate.TotalFrames + c.UltimateCutInFrames + 2;
                for (int frame = 0; frame < frames; frame++) sim.Tick(new FrameInput(held: InputButtons.Ultimate), default);
                Assert.That(sim.P1.UltimateUses, Is.EqualTo(use), "保持で連発しない");
            }
            sim.Practice = null;
            Assert.That(sim.P1.UnlimitedUltimates, Is.False); Assert.That(sim.P1.Gauge, Is.Zero); Assert.That(sim.P1.UltimateUses, Is.Zero);
            sim.P1.Gauge = sim.Rules.Gauge.Maximum; sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            BattleTestFactory.Idle(sim, c.Ultimate.TotalFrames + 2); sim.P1.Gauge = sim.Rules.Gauge.Maximum;
            Assert.That(GaugeSystem.CanUseUltimate(sim.P1, sim.Rules), Is.False);
        }
        [Test]
        public void CurrentRulesAndTutorialRequireOnlyCurrentAvailableInputs()
        {
            var r = AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            Assert.That(r.SchemaVersion, Is.EqualTo(12)); Assert.That(r.CombatInput.SingleAerialNormal && r.CombatInput.FlowingAerialNormal && r.CombatInput.SingleSpecial && r.CombatInput.NormalClashes, Is.True);
            Assert.That(r.CombatInput.UpRecoveryEnabled, Is.True);
            Assert.That(r.CombatInput.DownSpecialEnabled, Is.True);
            var sim = BattleTestFactory.Create(false, r, Setup.Characters[0].ToDefinition()); var t = new TutorialSession { Step = TutorialStep.Directional };
            var definitions = new[] { sim.P1.Definition.DirectionalNormals[1], sim.P1.Definition.UpNormal, sim.P1.Definition.DirectionalNormals[3] };
            for (int i = 0; i < 3; i++) { sim.P1.Attack = new AttackInstance(i + 1, definitions[i], 0, i == 2); t.Observe(sim, default); }
            Assert.That(t.DirectionMask, Is.EqualTo(7)); Assert.That(t.Step, Is.EqualTo(TutorialStep.Specials));
            sim.P1.CurrentSpecial = SpecialSlot.Neutral; sim.P1.Attack = new AttackInstance(9, sim.P1.Definition.Special(SpecialSlot.Neutral), 0, false); t.Observe(sim, default);
            Assert.That(t.Step, Is.EqualTo(TutorialStep.Specials)); Assert.That(t.SpecialMask, Is.EqualTo(1));
            sim.P1.CurrentSpecial = SpecialSlot.Down; sim.P1.Attack = new AttackInstance(10, sim.P1.Definition.Special(SpecialSlot.Down), 0, false); t.Observe(sim, default);
            Assert.That(t.Step, Is.EqualTo(TutorialStep.Specials)); Assert.That(t.SpecialMask, Is.EqualTo(9));
            sim.P1.CurrentSpecial = SpecialSlot.Up; sim.P1.Attack = new AttackInstance(11, sim.P1.Definition.Special(SpecialSlot.Up), 0, true); t.Observe(sim, default);
            Assert.That(t.Step, Is.EqualTo(TutorialStep.Guard)); Assert.That(t.SpecialMask, Is.EqualTo(13));
        }
    }
}
