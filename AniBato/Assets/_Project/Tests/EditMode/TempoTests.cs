using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class TempoTests
    {
        private static BattleSimulation Create() => new BattleSimulation(PhaseFiveBDefaults.Character(0), PhaseFiveBDefaults.Character(1), PhaseFiveBDefaults.Rules(), PhaseOneDefaults.Stage());
        private static void Strike(BattleSimulation simulation, AttackKind kind, int tick = 1)
        {
            var definition = kind == AttackKind.Normal ? simulation.P1.Definition.GroundNormals[0] : kind == AttackKind.Special ? simulation.P1.Definition.Specials[1] : simulation.P1.Definition.Ultimate;
            new DamageSystem().Apply(new HitContact(simulation.P1, simulation.P2, new AttackInstance(tick, definition, 0, false)),
                simulation.ComboP1, tick, simulation.Rules, new List<BattleEvent>());
        }
        [TestCase(AttackKind.Special, 2)] [TestCase(AttackKind.Ultimate, 1)]
        public void CurrentTuningPreservesSpecialAndUltimateHeartDamage(AttackKind kind, int hearts)
        {
            var simulation = Create(); simulation.P2.NormalHitCount = 2; Strike(simulation, kind);
            Assert.That(simulation.Rules.InitialHearts, Is.EqualTo(3)); Assert.That(simulation.P2.Hearts, Is.EqualTo(hearts));
            Assert.That(simulation.P2.NormalHitCount, Is.EqualTo(2)); Assert.That(simulation.P2.HeartRecoveryRemaining, Is.EqualTo(75));
        }
        [Test]
        public void ThreeNormalsBreakOneHeartAndComboLimitStillHoldsAfterRecovery()
        {
            var simulation = Create();
            for (int i = 1; i <= 3; i++) Strike(simulation, AttackKind.Normal, i);
            Assert.That(simulation.P2.Hearts, Is.EqualTo(2)); Assert.That(simulation.P2.NormalHitCount, Is.Zero);
            Assert.That(simulation.P2.HeartRecoveryRemaining, Is.EqualTo(75));
            var mixed = Create(); Strike(mixed, AttackKind.Special);
            mixed.P2.HeartRecoveryRemaining = 0; // Isolate the mixed combo's independent heart-loss cap.
            for (int i = 2; i <= 4; i++) Strike(mixed, AttackKind.Normal, i);
            Assert.That(mixed.P2.Hearts, Is.EqualTo(2)); Assert.That(mixed.P2.NormalHitCount, Is.EqualTo(2));
        }
        [Test]
        public void InvulnerabilityRejectsEveryDamageKindAndCannotBeRefreshedByAttacking()
        {
            var simulation = Create(); Strike(simulation, AttackKind.Special);
            Assert.That(simulation.P2.Velocity.X, Is.GreaterThanOrEqualTo(12)); Assert.That(simulation.P2.Velocity.Y, Is.GreaterThanOrEqualTo(8));
            BattleTestFactory.Idle(simulation, 10); int remaining = simulation.P2.HeartRecoveryRemaining;
            foreach (AttackKind kind in System.Enum.GetValues(typeof(AttackKind))) Strike(simulation, kind, 30);
            Assert.That(simulation.P2.Hearts, Is.EqualTo(2)); Assert.That(simulation.P2.NormalHitCount, Is.Zero);
            Assert.That(simulation.P2.HeartRecoveryRemaining, Is.EqualTo(remaining));
            simulation.Tick(default, new FrameInput(pressed: InputButtons.Attack));
            Assert.That(simulation.P2.HeartRecoveryRemaining, Is.EqualTo(remaining - 1));
            BattleTestFactory.Idle(simulation, remaining - 1); Assert.That(simulation.P2.HeartRecoveryRemaining, Is.Zero);
            Strike(simulation, AttackKind.Special, 100); Assert.That(simulation.P2.Hearts, Is.EqualTo(1));
        }
        [Test]
        public void InvulnerabilityRejectsGatheredMeleeAndProjectileContacts()
        {
            var simulation = Create(); simulation.P2.Position = simulation.P1.Position;
            simulation.P2.HeartRecoveryRemaining = 60;
            var definition = PhaseOneDefaults.Normal(); definition.StartupFrames = 0;
            simulation.P1.Attack = new AttackInstance(50, definition, 0, false);
            var contacts = new List<HitContact>(); new HitResolver().Gather(simulation.P1, simulation.P2, contacts);
            Assert.That(contacts, Is.Empty); Assert.That(simulation.P1.Attack.HasHit(1), Is.False);
            definition.SpawnProjectile = true; definition.ProjectileSpeed = 1; definition.ProjectileLifetimeFrames = 100;
            var projectile = new Projectile(simulation.P1, new AttackInstance(51, definition, 0, false));
            simulation.P2.Position = projectile.Position; new HitResolver().Gather(projectile, simulation.P2, contacts);
            Assert.That(contacts, Is.Empty); Assert.That(projectile.Attack.HasHit(1), Is.False);
        }
        [Test]
        public void SpecialCooldownCannotBeBypassedByBufferingOrChangingFacing()
        {
            var simulation = Create(); simulation.Tick(new FrameInput(pressed: InputButtons.Special), default);
            Assert.That(simulation.P1.SpecialCooldown(SpecialSlot.Neutral), Is.EqualTo(120));
            long original = simulation.P1.Attack.Id;
            for (int tick = 1; tick < 120; tick++)
            {
                simulation.Tick(new FrameInput(-1, pressed: InputButtons.Special, actionX: 0, actionY: 0), default);
                Assert.That(simulation.Events.Any(e => e.Kind == BattleEventKind.AttackStarted && e.SourcePlayer == 0), Is.False);
                if (simulation.P1.Attack != null) Assert.That(simulation.P1.Attack.Id, Is.EqualTo(original));
            }
            simulation.Tick(new FrameInput(pressed: InputButtons.Special), default);
            Assert.That(simulation.P1.Attack, Is.Not.Null); Assert.That(simulation.P1.Attack.Id, Is.Not.EqualTo(original));
            Assert.That(simulation.P1.SpecialCooldown(SpecialSlot.Neutral), Is.EqualTo(120));
        }
        [Test]
        public void OnlyOneProjectilePerOwnerCanBeOnScreen()
        {
            var simulation = Create(); var system = new ProjectileSystem();
            var definition = simulation.P1.Definition.Specials[0]; definition.StartupFrames = 0; definition.ProjectileLifetimeFrames = 500;
            simulation.P1.Attack = new AttackInstance(1, definition, 0, false); system.Step(simulation.P1, simulation.P2, 1);
            simulation.P1.Attack = new AttackInstance(2, definition, 0, false); system.Step(simulation.P1, simulation.P2, 1);
            Assert.That(system.Projectiles.Count, Is.EqualTo(1)); Assert.That(system.Projectiles[0].Attack.Id, Is.EqualTo(1));
            simulation.P2.Attack = new AttackInstance(3, definition, 0, false); system.Step(simulation.P1, simulation.P2, 1);
            Assert.That(system.Projectiles.Count, Is.EqualTo(2));
        }
        [Test]
        public void MigrationUpdatesDefaultsOnceAndPreservesCustomTuning()
        {
            var original = PhaseOneDefaults.Rules(); var migrated = BalanceMigration.Rules(original);
            Assert.That(migrated, Is.EqualTo(PhaseFiveBDefaults.Rules())); Assert.That(BalanceMigration.Rules(migrated), Is.EqualTo(migrated));
            var custom = original; var gauge = custom.Gauge; gauge.OnHit = 3; custom.Gauge = gauge;
            Assert.That(BalanceMigration.Rules(custom).Gauge.OnHit, Is.EqualTo(3));
            for (int difficulty = 0; difficulty < 3; difficulty++)
            {
                var cpu = BalanceMigration.Cpu(CpuDefinition.LegacyDifficulty(difficulty), difficulty);
                Assert.That(cpu, Is.EqualTo(CpuDefinition.ForDifficulty(difficulty))); Assert.That(BalanceMigration.Cpu(cpu, difficulty), Is.EqualTo(cpu));
                var edited = CpuDefinition.LegacyDifficulty(difficulty); edited.GuardChance = .123f;
                Assert.That(BalanceMigration.Cpu(edited, difficulty).GuardChance, Is.EqualTo(.123f));
            }
            var old = PhaseThreeDefaults.Character(0).Specials[0]; var fresh = PhaseFiveBDefaults.Character(0).Specials[0];
            Assert.That(BalanceMigration.Special(old, old, fresh), Is.EqualTo(fresh));
            var tuned = old; tuned.StartupFrames = 33; Assert.That(BalanceMigration.Special(tuned, old, fresh).StartupFrames, Is.EqualTo(33));
        }
        [Test]
        public void FixedSeedSmallStudyLocksTempoShrinkAndUltimateTargets()
        {
            var rows = CpuBalanceStudy.Run(new[] { PhaseFiveBDefaults.Character(0), PhaseFiveBDefaults.Character(1), PhaseFiveBDefaults.Character(2) },
                PhaseFiveBDefaults.Rules(), PhaseOneDefaults.Stage(), new[] { CpuDefinition.ForDifficulty(1) }, 20);
            Assert.That(rows.Average(row => row.Ticks / 60d), Is.InRange(35, 55));
            Assert.That(rows.Count(row => row.Ticks >= 3600) / (double)rows.Count, Is.InRange(.20, .40));
            Assert.That(rows.Count(row => row.UltimateStarts > 0) / (double)rows.Count, Is.InRange(.5, .70));
            Assert.That(rows.Sum(row => row.UnforcedRingOuts), Is.Zero);
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void CurrentCombatStateAndEventsRemainIdenticalAcrossRenderRates(int fps)
        {
            var a = Create(); var b = Create(); var actual = new List<BattleEvent>(); var expected = new List<BattleEvent>();
            var firstA = new CpuBrain(CpuDefinition.ForDifficulty(1), 23); var secondA = new CpuBrain(CpuDefinition.ForDifficulty(1), 97);
            var firstB = new CpuBrain(CpuDefinition.ForDifficulty(1), 23); var secondB = new CpuBrain(CpuDefinition.ForDifficulty(1), 97);
            var clock = new FixedTickClock(); var snapshots = new AniBato.Presentation.RenderSnapshots(); snapshots.Reset(a);
            for (int frame = 0; frame < fps * 20; frame++)
            {
                clock.Advance(1d / fps, () => { snapshots.BeforeTick(a); a.Tick(firstA.NextInput(a, 0), secondA.NextInput(a, 1)); snapshots.AfterTick(a); actual.AddRange(a.Events); }, 5);
                snapshots.FighterPosition(0, clock.Fraction); snapshots.FighterPosition(1, clock.Fraction);
            }
            for (int tick = 0; tick < 1200; tick++) { b.Tick(firstB.NextInput(b, 0), secondB.NextInput(b, 1)); expected.AddRange(b.Events); }
            Assert.That(actual, Is.EqualTo(expected)); Assert.That(a.P1.Position, Is.EqualTo(b.P1.Position)); Assert.That(a.P2.Position, Is.EqualTo(b.P2.Position));
            Assert.That(a.P1.Hearts, Is.EqualTo(b.P1.Hearts)); Assert.That(a.P2.NormalHitCount, Is.EqualTo(b.P2.NormalHitCount));
            Assert.That(a.P1.Gauge, Is.EqualTo(b.P1.Gauge)); Assert.That(a.P2.HeartRecoveryRemaining, Is.EqualTo(b.P2.HeartRecoveryRemaining));
            Assert.That(a.P1.SpecialCooldown(SpecialSlot.Neutral), Is.EqualTo(b.P1.SpecialCooldown(SpecialSlot.Neutral)));
            Assert.That(a.Match.RemainingTicks, Is.EqualTo(b.Match.RemainingTicks));
        }
    }
}
