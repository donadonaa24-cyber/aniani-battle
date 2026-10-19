using System;
using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AniBato.Tests
{
    public sealed class BlackHoleAttractionTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleRuleDefinition Rules => AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
        private static BattleSimulation Create(int owner = 0, float x = 8, float y = 1.1f, bool pull = true, int lifetime = 180, StageGeometry stage = null)
        {
            var caster = Setup.Character(3).ToDefinition(); caster.UltimateCutInFrames = 0;
            var attack = caster.Ultimate; attack.StartupFrames = 0; attack.ProjectileLifetimeFrames = lifetime;
            if (!pull) { attack.Ability.ProjectilePullRadius = 0; attack.Ability.ProjectilePullSpeed = 0; }
            caster.Ultimate = attack; var other = Setup.Character(2).ToDefinition();
            var sim = new BattleSimulation(owner == 0 ? caster : other, owner == 0 ? other : caster, Rules, stage ?? PhaseOneDefaults.Stage());
            var source = owner == 0 ? sim.P1 : sim.P2; var target = owner == 0 ? sim.P2 : sim.P1;
            source.Position = new BattleVector(-14, 1.1f); source.Gauge = Rules.Gauge.Maximum;
            target.Position = new BattleVector(x, y); target.Grounded = y == 1.1f; target.Velocity = default;
            var input = new FrameInput(pressed: InputButtons.Ultimate);
            sim.Tick(owner == 0 ? input : default, owner == 1 ? input : default);
            Assert.That(sim.Projectiles.Count, Is.EqualTo(1)); return sim;
        }
        [TestCase(0, 1)] [TestCase(0, -1)] [TestCase(1, 1)] [TestCase(1, -1)]
        public void StrongPullBeatsAwayMovementAndDealsExactlyTwoHeartsOnceForEitherPlayerAndSide(int owner, int side)
        {
            var sim = Create(owner, side * 8); var target = owner == 0 ? sim.P2 : sim.P1;
            var source = owner == 0 ? sim.P1 : sim.P2; var field = sim.Projectiles.Single(); var center = field.Position;
            target.NormalHitCount = 2; float previous = Math.Abs(target.Position.X); int hits = 0, lost = 0, elapsed = 0;
            while (target.Hearts == 3 && elapsed++ < 90)
            {
                var away = new FrameInput(side);
                sim.Tick(owner == 0 ? default : away, owner == 0 ? away : default);
                hits += sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.SourcePlayer == owner);
                lost += sim.Events.Where(e => e.Kind == BattleEventKind.HeartLost && e.TargetPlayer == target.PlayerIndex).Sum(e => e.Value);
                Assert.That(field.Position, Is.EqualTo(center));
                if (target.Hearts == 3) { Assert.That(Math.Abs(target.Position.X), Is.LessThan(previous)); previous = Math.Abs(target.Position.X); }
                Assert.That(source.Position.X, Is.EqualTo(-14), "自分の場に引かれない");
            }
            Assert.That(elapsed, Is.InRange(2, 60)); Assert.That(hits, Is.EqualTo(1)); Assert.That(lost, Is.EqualTo(2));
            Assert.That(target.Hearts, Is.EqualTo(1)); Assert.That(target.NormalHitCount, Is.EqualTo(2));
            Assert.That(ProjectileAttractionSystem.Velocity(target, sim.Projectiles), Is.EqualTo(default(BattleVector)));
            for (int i = 0; i < 90; i++)
            {
                target.Position = center; sim.Tick(default, default);
                Assert.That(target.Hearts, Is.EqualTo(1)); Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.HeartLost), Is.False);
            }
        }
        [Test]
        public void AirborneTargetIsPulledVerticallyAsWellAsHorizontallyWithoutOverwritingGravity()
        {
            var sim = Create(x: 6, y: 8); var control = Create(x: 6, y: 8, pull: false);
            var force = ProjectileAttractionSystem.Velocity(sim.P2, sim.Projectiles);
            Assert.That(force.X, Is.LessThan(0)); Assert.That(force.Y, Is.LessThan(0));
            sim.Tick(default, default); control.Tick(default, default);
            Assert.That(sim.P2.Position.X, Is.LessThan(control.P2.Position.X)); Assert.That(sim.P2.Position.Y, Is.LessThan(control.P2.Position.Y));
            Assert.That(sim.P2.Velocity.Y, Is.EqualTo(control.P2.Velocity.Y), "引力を持続速度として残さない");
            Assert.That(sim.P2.Hearts, Is.EqualTo(3), "引力の範囲へ入っただけでは減らさない");
        }
        [TestCase(1)] [TestCase(-1)]
        public void AwayDashAlsoMovesTowardTheField(int side)
        {
            var sim = Create(x: side * 8); var input = new FrameInput(side, held: InputButtons.Dash, pressed: InputButtons.Dash);
            sim.Tick(default, input);
            Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Dash));
            Assert.That(Math.Abs(sim.P2.Position.X), Is.LessThan(8)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void AnotherUltimateCutInFreezesTheExistingFieldAndItsPull()
        {
            var sim = Create(); sim.P2.Gauge = sim.Rules.Gauge.Maximum;
            var position = sim.P2.Position; var field = sim.Projectiles.Single(); int lifetime = field.RemainingFrames;
            sim.Tick(default, new FrameInput(pressed: InputButtons.Ultimate));
            Assert.That(sim.CutInRemainingFrames, Is.GreaterThan(10));
            for (int i = 0; i < 10; i++)
            { sim.Tick(default, default); Assert.That(sim.P2.Position, Is.EqualTo(position)); Assert.That(field.RemainingFrames, Is.EqualTo(lifetime)); }
        }
        [Test]
        public void OutsideRangeExpiredFieldsAndOrdinaryProjectilesHaveNoPullOrResidualMomentum()
        {
            var sim = Create(x: 12); var start = sim.P2.Position;
            BattleTestFactory.Idle(sim, 50); Assert.That(sim.P2.Position, Is.EqualTo(start)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            sim = Create(lifetime: 2); float x = sim.P2.Position.X; sim.Tick(default, default);
            Assert.That(sim.P2.Position.X, Is.LessThan(x)); x = sim.P2.Position.X;
            sim.Tick(default, default); Assert.That(sim.Projectiles, Is.Empty); Assert.That(sim.P2.Position.X, Is.EqualTo(x));
            sim.Tick(default, default); Assert.That(sim.P2.Position.X, Is.EqualTo(x)); Assert.That(sim.P2.Velocity.X, Is.Zero);
            foreach (var data in Setup.Characters)
                foreach (var attack in data.GroundNormals.Concat(data.Specials).Append(data.Ultimate).Distinct())
                    if (attack != Setup.Character(3).Ultimate)
                    { Assert.That(attack.Definition.Ability.ProjectilePullRadius, Is.Zero); Assert.That(attack.Definition.Ability.ProjectilePullSpeed, Is.Zero); }
        }
        [Test]
        public void JustGuardConsumesTheFieldContactAndEndsItsPullWithoutDamage()
        {
            var sim = Create(x: 3.7f); bool defended = false;
            for (int i = 0; i < 6; i++)
            {
                sim.Tick(default, new FrameInput(held: InputButtons.Guard, pressed: i == 0 ? InputButtons.Guard : InputButtons.None));
                defended |= sim.Events.Any(e => e.Kind == BattleEventKind.JustGuard);
            }
            Assert.That(defended, Is.True); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.Projectiles.Single().Attack.HasHit(1), Is.True);
            Assert.That(ProjectileAttractionSystem.Velocity(sim.P2, sim.Projectiles), Is.EqualTo(default(BattleVector)));
        }
        [Test]
        public void HitstopProtectionOwnerKoAndFinishedMatchDoNotDragTheTarget()
        {
            var sim = Create(); var start = sim.P2.Position; sim.P2.HitstopRemaining = 3;
            for (int i = 0; i < 3; i++) { sim.Tick(default, default); Assert.That(sim.P2.Position, Is.EqualTo(start)); }
            sim.P2.HeartRecoveryRemaining = 10; Assert.That(ProjectileAttractionSystem.Velocity(sim.P2, sim.Projectiles), Is.EqualTo(default(BattleVector)));
            sim.P2.HeartRecoveryRemaining = 0; sim.P2.FrozenThisTick = false; sim.P2.FollowupImmunity[0] = 2;
            Assert.That(ProjectileAttractionSystem.Velocity(sim.P2, sim.Projectiles), Is.EqualTo(default(BattleVector)));
            sim.P2.FollowupImmunity[0] = 0; sim.P1.Mode = FighterMode.KO;
            Assert.That(ProjectileAttractionSystem.Velocity(sim.P2, sim.Projectiles), Is.EqualTo(default(BattleVector)));
            sim.P1.Mode = FighterMode.Idle; sim.Match.Result = MatchResult.Player1Wins;
            sim.Tick(default, default); Assert.That(sim.P2.Position, Is.EqualTo(start));
        }
        [Test]
        public void SolidWallStopsThePullInsteadOfTeleportingThroughIt()
        {
            var original = PhaseOneDefaults.Stage(); var platforms = Enumerable.Range(0, original.PlatformCount).Select(i => original[i]).ToList();
            platforms.Add(new StagePlatform(new BattleRect(new BattleVector(4, 3), new BattleVector(1, 6)), false));
            var stage = new StageGeometry(platforms.ToArray(), -24, 24, -10, original.SpawnP1, original.SpawnP2);
            var sim = Create(stage: stage); float stop = 4.5f + sim.P2.Definition.HurtboxSize.X * .5f;
            for (int i = 0; i < 60; i++) { sim.Tick(default, default); Assert.That(sim.P2.Position.X, Is.GreaterThanOrEqualTo(stop - .0001f)); }
            Assert.That(sim.P2.Position.X, Is.EqualTo(stop).Within(.0001)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void ReplacementUsesOnlyTheNewFieldAndCenterForceIsFinite()
        {
            var sim = Create(); var shots = new ProjectileSystem(); var source = sim.P1;
            var definition = source.Definition.Ultimate; definition.StartupFrames = 0;
            source.Attack = new AttackInstance(100, definition, 0, false); shots.Step(source, sim.P2, 3, sim.Stage);
            var first = shots.Projectiles.Single(); source.Attack = new AttackInstance(101, definition, 0, false); shots.Step(source, sim.P2, 3, sim.Stage);
            Assert.That(shots.Projectiles.Count, Is.EqualTo(1)); Assert.That(shots.Projectiles.Single(), Is.Not.SameAs(first));
            var force = ProjectileAttractionSystem.Velocity(sim.P2, shots.Projectiles);
            Assert.That(Math.Sqrt(force.X * force.X + force.Y * force.Y), Is.EqualTo(definition.Ability.ProjectilePullSpeed).Within(.0001));
            sim.P2.Position = shots.Projectiles.Single().Position;
            Assert.That(ProjectileAttractionSystem.Velocity(sim.P2, shots.Projectiles), Is.EqualTo(default(BattleVector)));
        }
        [Test]
        public void IdenticalInputStreamsProduceIdenticalAttractionAndDamage()
        {
            var first = Create(); var second = Create();
            for (int i = 0; i < 200; i++)
            {
                var input = new FrameInput(i < 30 ? 1 : 0); first.Tick(default, input); second.Tick(default, input);
                Assert.That(first.P2.Position, Is.EqualTo(second.P2.Position)); Assert.That(first.P2.Velocity, Is.EqualTo(second.P2.Velocity));
                Assert.That(first.P2.Hearts, Is.EqualTo(second.P2.Hearts)); Assert.That(first.Projectiles.Count, Is.EqualTo(second.Projectiles.Count));
            }
        }
        [Test]
        public void MigrationAddsAttractionOnceAndRepeatedBuildAllKeepsTunedValues()
        {
            var copies = Setup.Characters.Select(c => Object.Instantiate(c)).ToArray(); var ultimate = Object.Instantiate(copies[3].Ultimate); copies[3].Ultimate = ultimate;
            try
            {
                copies[3].MoveSetSchemaVersion = 1; var before = ultimate.Definition;
                var value = before; value.Ability.ProjectilePullRadius = value.Ability.ProjectilePullSpeed = 0; ultimate.Definition = value;
                RosterMoveSetBuilder.Apply(copies);
                Assert.That(ultimate.Definition.Ability.ProjectilePullRadius, Is.EqualTo(10)); Assert.That(ultimate.Definition.Ability.ProjectilePullSpeed, Is.EqualTo(26));
                Assert.That(ultimate.Definition.Hitbox, Is.EqualTo(before.Hitbox)); Assert.That(ultimate.Definition.ProjectileLifetimeFrames, Is.EqualTo(180));
                value = ultimate.Definition; value.Ability.ProjectilePullRadius = 7; value.Ability.ProjectilePullSpeed = 28; ultimate.Definition = value;
                RosterMoveSetBuilder.Apply(copies);
                Assert.That(ultimate.Definition.Ability.ProjectilePullRadius, Is.EqualTo(7)); Assert.That(ultimate.Definition.Ability.ProjectilePullSpeed, Is.EqualTo(28));
            }
            finally { foreach (var copy in copies) Object.DestroyImmediate(copy); Object.DestroyImmediate(ultimate); }
        }
        [TestCase(-1f, 26f)] [TestCase(10f, -1f)] [TestCase(0f, 26f)] [TestCase(10f, 0f)]
        [TestCase(float.NaN, 26f)] [TestCase(10f, float.PositiveInfinity)]
        public void InvalidAttractionSettingsAreRejected(float radius, float speed)
        {
            var ability = new AttackAbility { ProjectilePullRadius = radius, ProjectilePullSpeed = speed };
            Assert.Throws<ArgumentException>(() => ability.Validate());
        }
    }
}
