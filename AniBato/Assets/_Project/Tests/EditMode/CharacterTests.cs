using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class CharacterTests
    {
        private static BattleSimulation Create(int character, float targetX = 1.5f)
        {
            var stage = new StageGeometry(new[] { new StagePlatform(new BattleRect(new BattleVector(0, -0.6f), new BattleVector(36, 1.2f)), false) },
                -24, 24, -10, new BattleVector(0, 1.1f), new BattleVector(targetX, 1.1f));
            return new BattleSimulation(PhaseThreeDefaults.Character(character), PhaseTwoTestFactory.Character(), PhaseOneDefaults.Rules(), stage);
        }
        private static FrameInput Special(SpecialSlot slot) => new FrameInput(slot == SpecialSlot.Side ? 1 : 0,
            slot == SpecialSlot.Up ? 1 : slot == SpecialSlot.Down ? -1 : 0, pressed: InputButtons.Special);
        private static void Idle(BattleSimulation sim, int ticks) => BattleTestFactory.Idle(sim, ticks);

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void EachCharacterHasIndependentValidMovesAndGroundAirNormals(int character)
        {
            var c = PhaseThreeDefaults.Character(character).Snapshot();
            Assert.That(c.GroundNormals.Length, Is.EqualTo(3)); Assert.That(c.AirNormals.Length, Is.EqualTo(3));
            Assert.That(c.Specials.Length, Is.EqualTo(4)); Assert.That(c.Ultimate.Kind, Is.EqualTo(AttackKind.Ultimate));
            var sim = Create(character); sim.Tick(new FrameInput(1), default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(c.MoveSpeed / 60).Within(0.0001));
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Idle(sim, c.GroundNormals[0].StartupFrames + 1);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
            var air = Create(character); air.Tick(new FrameInput(pressed: InputButtons.Jump), default);
            Idle(air, air.Rules.GroundJumpStartupFrames);
            air.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(air.P1.Attack.AirborneAttack, Is.True);
            Assert.That(air.P1.Attack.Definition.Name, Is.EqualTo(c.AirNormals[0].Name));
        }
        [TestCase(0, SpecialSlot.Neutral)] [TestCase(0, SpecialSlot.Side)] [TestCase(0, SpecialSlot.Up)] [TestCase(0, SpecialSlot.Down)]
        [TestCase(1, SpecialSlot.Neutral)] [TestCase(1, SpecialSlot.Side)] [TestCase(1, SpecialSlot.Up)] [TestCase(1, SpecialSlot.Down)]
        [TestCase(2, SpecialSlot.Neutral)] [TestCase(2, SpecialSlot.Side)] [TestCase(2, SpecialSlot.Up)]
        public void DamagingSpecialsLoseOneHeartKeepAccumulationAndHitOnlyOnce(int character, SpecialSlot slot)
        {
            var sim = Create(character, slot == SpecialSlot.Up ? 0.3f : character == 0 && slot == SpecialSlot.Side ? 3.2f : 1.5f);
            sim.P2.NormalHitCount = 2;
            sim.Tick(Special(slot), default);
            Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(slot));
            Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(sim.P1.Definition.Special(slot).Name));
            var hits = new List<BattleEvent>();
            for (int i = 0; i < 100; i++)
            {
                hits.AddRange(sim.Events.Where(e => e.Kind == BattleEventKind.Hit));
                sim.Tick(character == 1 && slot == SpecialSlot.Down && i == 14 ? new FrameInput(pressed: InputButtons.Special) : default, default);
            }
            Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2));
            Assert.That(hits.Count, Is.EqualTo(1));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void UltimateCosts100StartsOneCutInAndLosesTwoHeartsOnce(int character)
        {
            var sim = Create(character); sim.P1.Gauge = 100; sim.P2.NormalHitCount = 2;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            Assert.That(sim.P1.Gauge, Is.Zero); Assert.That(sim.P1.UltimateUses, Is.EqualTo(1));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.UltimateStarted), Is.EqualTo(1));
            int time = sim.Match.RemainingTicks, tick = sim.Match.TickCount;
            var position = sim.P1.Position; var targetPosition = sim.P2.Position;
            for (int i = 0; i < 30; i++) sim.Tick(new FrameInput(1, pressed: InputButtons.Attack), new FrameInput(-1));
            Assert.That(sim.CutInRemainingFrames, Is.Zero); Assert.That(sim.Match.RemainingTicks, Is.EqualTo(time));
            Assert.That(sim.Match.TickCount, Is.EqualTo(tick)); Assert.That(sim.P1.Position, Is.EqualTo(position));
            Assert.That(sim.P2.Position, Is.EqualTo(targetPosition)); Assert.That(sim.P1.Attack.Frame, Is.Zero);
            Assert.That(sim.Events, Is.Empty);
            Idle(sim, 110);
            Assert.That(sim.P2.Hearts, Is.EqualTo(1)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2));
            sim.P1.Gauge = 100; sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.Gauge, Is.EqualTo(100));
            Assert.That(sim.P1.UltimateUses, Is.EqualTo(1));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void UltimateWhiffHasFullRecoveryAndDoesNotHitOutsideRange(int character)
        {
            var sim = Create(character, -12); sim.P1.Gauge = 100;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default); Idle(sim, 30);
            var ultimate = sim.P1.Definition.Ultimate;
            Idle(sim, ultimate.TotalFrames - 1);
            Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(sim.P1.Attack.Definition.IsNormal, Is.True);
            Assert.That(ultimate.RecoveryFrames, Is.GreaterThanOrEqualTo(60));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void EachUpSpecialCanReturnFromNearRingOutAndResetsOnLanding(int character)
        {
            var sim = Create(character, -10); sim.P1.Position = new BattleVector(19, -2);
            sim.P1.Grounded = false; sim.P1.JumpsUsed = 2; sim.P1.Velocity = default;
            sim.Tick(new FrameInput(-1, 1, pressed: InputButtons.Special), default);
            float highest = -2;
            for (int i = 0; i < 55; i++) { sim.Tick(default, default); highest = System.Math.Max(highest, sim.P1.Position.Y); }
            Assert.That(highest, Is.GreaterThan(1.1f)); Assert.That(sim.P1.UpSpecialUsed, Is.True);
            sim.Tick(Special(SpecialSlot.Up), default); Assert.That(sim.P1.Attack, Is.Null);
            for (int i = 0; i < 100 && !sim.P1.Grounded && !sim.Match.IsFinished; i++) sim.Tick(new FrameInput(-1), default);
            Assert.That(sim.Match.IsFinished, Is.False); Assert.That(sim.P1.Grounded, Is.True);
            Assert.That(sim.P1.UpSpecialUsed, Is.False);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void CounterEffectsBelongToDefenderAndApplyTheirCharacterData(int character)
        {
            var attacker = PhaseTwoTestFactory.Character(); var defender = PhaseThreeDefaults.Character(character);
            var stage = new StageGeometry(new[] { PhaseOneDefaults.Stage()[0] }, -24, 24, -10, new BattleVector(0, 1.1f), new BattleVector(1.5f, 1.1f));
            var sim = new BattleSimulation(attacker, defender, PhaseOneDefaults.Rules(), stage); sim.P2.NormalHitCount = 2;
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), new FrameInput(pressed: InputButtons.Counter));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.CounterSuccess), Is.EqualTo(1));
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1));
            Assert.That(sim.P2.BuffRemaining, Is.EqualTo(character == 0 ? 180 : 0));
            Assert.That(sim.P1.Velocity.X, Is.EqualTo(character == 1 ? -8 : -4));
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(character == 2 ? 1 : 2));
            Assert.That(sim.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void IndependentCounterHooksDoNotLeakBetweenPlayers()
        {
            var sim = Create(0); var first = new CounterProbe(); var second = new CounterProbe();
            sim.P1.CounterEffect = first; sim.P2.CounterEffect = second;
            sim.P1.Attack = new AttackInstance(999, PhaseTwoTestFactory.Character().GroundNormals[0], 0, false);
            sim.Tick(default, new FrameInput(pressed: InputButtons.Counter));
            Assert.That(first.Calls, Is.Zero); Assert.That(second.Calls, Is.EqualTo(1));
        }
        private sealed class CounterProbe : ICounterEffect
        {
            public int Calls;
            public void OnSuccess(FighterState defender, FighterState attacker, int tick, in BattleRuleDefinition rules, List<BattleEvent> events) => Calls++;
        }
        [Test]
        public void ToumaBuffIncreasesMovementAndDashAndExpiresExactly()
        {
            var sim = Create(0, -10); SkillSystem.Buff(sim.P1, sim.P1.Definition.Counter.Buff);
            sim.Tick(new FrameInput(1), default);
            Assert.That(sim.P1.Velocity.X, Is.EqualTo(12.5f));
            sim.Tick(new FrameInput(1, pressed: InputButtons.Dash), default);
            Assert.That(sim.P1.Velocity.X, Is.EqualTo(24.7f).Within(0.0001));
            Idle(sim, 177); Assert.That(sim.P1.BuffRemaining, Is.EqualTo(1));
            sim.Tick(new FrameInput(-1), default);
            Assert.That(sim.P1.BuffRemaining, Is.Zero); Assert.That(sim.P1.Velocity.X, Is.EqualTo(-10));
        }
        [Test]
        public void MealHealsOnlyOnActiveFrameCannotHealHeartsAndRespectsCooldown()
        {
            var sim = Create(2); sim.P1.NormalHitCount = 2; sim.P1.Hearts = 2;
            sim.Tick(Special(SpecialSlot.Down), default); Idle(sim, 23);
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(2));
            sim.Tick(default, default); Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1));
            Assert.That(sim.P1.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Idle(sim, 40); sim.Tick(Special(SpecialSlot.Down), default); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Down), Is.GreaterThan(0));
            Idle(sim, 240); sim.Tick(Special(SpecialSlot.Down), default); Idle(sim, 24);
            Assert.That(sim.P1.NormalHitCount, Is.Zero); Assert.That(sim.P1.Hearts, Is.EqualTo(2));
            SkillSystem.Heal(sim.P1, 10); Assert.That(sim.P1.NormalHitCount, Is.Zero);
        }
        [Test]
        public void InterruptedMealDoesNotHealAndStillHasCooldown()
        {
            var sim = Create(2); sim.P1.NormalHitCount = 1; sim.Tick(Special(SpecialSlot.Down), default);
            sim.P2.Attack = new AttackInstance(500, PhaseTwoTestFactory.Character().GroundNormals[0], 0, false);
            sim.Tick(default, default); Idle(sim, 35);
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(2));
            Assert.That(sim.P1.Hearts, Is.EqualTo(3)); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Down), Is.GreaterThan(0));
        }
        [Test]
        public void OnigiriFliesInAnArcAndRetainsItsLaunchDirection()
        {
            var sim = Create(2, 12); sim.Tick(Special(SpecialSlot.Neutral), default); Idle(sim, 8);
            var projectile = sim.Projectiles.Single(); var start = projectile.Position;
            Assert.That(projectile.Velocity.Y, Is.EqualTo(8));
            sim.P1.Facing = -1; Idle(sim, 25);
            Assert.That(projectile.Position.X, Is.GreaterThan(start.X)); Assert.That(projectile.Velocity.Y, Is.LessThan(0));
            Assert.That(projectile.Position.Y, Is.GreaterThan(start.Y));
            Idle(sim, 36); Assert.That(sim.Projectiles, Is.Empty);
        }
        [TestCase(30, 30)] [TestCase(1000, 45)]
        public void ParalysisCapsDurationAndBlocksMovementAndActionsWithoutArmor(int requested, int expected)
        {
            var c = PhaseThreeDefaults.Character(2); var rice = c.Specials[0]; rice.ParalysisFrames = requested; c.Specials[0] = rice;
            var stage = new StageGeometry(new[] { PhaseOneDefaults.Stage()[0] }, -24, 24, -10, new BattleVector(0, 1.1f), new BattleVector(1.5f, 1.1f));
            var sim = new BattleSimulation(c, PhaseTwoTestFactory.Character(), PhaseOneDefaults.Rules(), stage);
            sim.Tick(Special(SpecialSlot.Neutral), default); Idle(sim, 8);
            Assert.That(sim.P2.ParalysisRemaining, Is.EqualTo(expected));
            Idle(sim, 3); // Hitstop is separate and does not consume paralysis.
            for (int i = 0; i < expected; i++)
            {
                sim.Tick(default, new FrameInput(-1, pressed: InputButtons.Attack | InputButtons.Jump));
                Assert.That(sim.P2.Attack, Is.Null);
                Assert.That(sim.P2.Velocity.Y, Is.LessThan(sim.P2.Definition.JumpSpeed), "麻痺中のジャンプ入力で上昇してはいけません。");
                Assert.That(sim.P2.CanControlMovement, Is.False);
            }
            Assert.That(sim.P2.ParalysisRemaining, Is.Zero);
            sim.Tick(default, new FrameInput(pressed: InputButtons.Attack)); Assert.That(sim.P2.Attack, Is.Not.Null);
        }
        [Test]
        public void ToumaPulseMarksTargetForDataDrivenTime()
        {
            var sim = Create(0); sim.Tick(Special(SpecialSlot.Neutral), default); Idle(sim, 6);
            Assert.That(sim.P2.MarkRemaining, Is.EqualTo(90)); Idle(sim, 89);
            Assert.That(sim.P2.MarkRemaining, Is.EqualTo(1)); sim.Tick(default, default); Assert.That(sim.P2.MarkRemaining, Is.Zero);
        }
        [Test]
        public void RenStanceRequiresNewInputAndDoesNotActAsCounter()
        {
            var sim = Create(1); sim.Tick(Special(SpecialSlot.Down), default); Idle(sim, 20);
            Assert.That(sim.P1.Attack.IsActive, Is.False); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Special));
            Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            sim.P2.Attack = new AttackInstance(600, PhaseTwoTestFactory.Character().GroundNormals[0], 0, false);
            sim.Tick(default, default);
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.CounterSuccess), Is.False);
            var timeout = Create(1); timeout.Tick(Special(SpecialSlot.Down), default); Idle(timeout, 72);
            Assert.That(timeout.P1.Attack, Is.Null); Assert.That(timeout.P1.Mode, Is.EqualTo(FighterMode.Recovery));
            Assert.That(timeout.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void RenRushHasExtraRecoveryWhenGuarded()
        {
            var sim = Create(1); sim.P2.Mode = FighterMode.Guard; sim.P2.DefenseAge = 10;
            sim.Tick(Special(SpecialSlot.Side), new FrameInput(held: InputButtons.Guard));
            for (int i = 0; i < 6; i++) sim.Tick(default, new FrameInput(held: InputButtons.Guard));
            Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery));
            Assert.That(sim.P1.ActionFrames, Is.GreaterThanOrEqualTo(40));
        }
        [TestCase(0)] [TestCase(2)]
        public void ProjectileDefenseErasesOnlyItsConfiguredNumberAndNotFriendlyShots(int character)
        {
            var sim = Create(character, 10); var shot = PhaseTwoDefaults.Special(SpecialSlot.Neutral);
            shot.StartupFrames = 0; shot.ProjectileSpeed = 0.1f; shot.Hitbox = new BattleRect(new BattleVector(9, 0), new BattleVector(0.5f, 0.5f));
            // Enemy shots spawn at x=1, in the shield. Damage contact is kept away until erasure.
            sim.P2.Attack = new AttackInstance(1, shot, 0, false);
            sim.Tick(Special(character == 0 ? SpecialSlot.Down : SpecialSlot.Side), default);
            var second = new AttackInstance(2, shot, 0, false); sim.P2.Attack = second;
            sim.Tick(default, default);
            Assert.That(sim.Projectiles.Count, Is.EqualTo(2));
            Idle(sim, character == 0 ? 3 : 5);
            Assert.That(sim.Projectiles.Count, Is.EqualTo(character == 0 ? 0 : 1));
        }
        [Test]
        public void CrossUltimateUsesUnionOfTwoRectanglesWithoutHittingCorners()
        {
            var sim = Create(1); var attack = new AttackInstance(30, sim.P1.Definition.Ultimate, 0, false);
            Assert.That(attack.Overlaps(sim.P1, new BattleRect(new BattleVector(0, 5), new BattleVector(1, 1))), Is.True);
            Assert.That(attack.Overlaps(sim.P1, new BattleRect(new BattleVector(4, 5), new BattleVector(1, 1))), Is.False);
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)] [TestCase(240)]
        public void CharacterEffectsAndCutInAreDeterministicAcrossRenderRates(int fps)
        {
            var a = Create(2, 12); var b = Create(2, 12); a.P1.Gauge = b.P1.Gauge = 100;
            var ae = new List<BattleEvent>(); var be = new List<BattleEvent>(); int ai = 0, bi = 0;
            System.Action<BattleSimulation, List<BattleEvent>, int> tick = (sim, events, i) =>
            {
                sim.Tick(i == 0 ? Special(SpecialSlot.Neutral) : i == 100 ? new FrameInput(pressed: InputButtons.Ultimate) : default, default);
                events.AddRange(sim.Events);
            };
            var clock = new FixedTickClock();
            for (int i = 0; i < fps * 5; i++) clock.Advance(1d / fps, () => tick(a, ae, ai++));
            for (int i = 0; i < 300; i++) tick(b, be, bi++);
            Assert.That(ae, Is.EqualTo(be)); Assert.That(a.P1.Position, Is.EqualTo(b.P1.Position));
            Assert.That(a.Match.RemainingTicks, Is.EqualTo(b.Match.RemainingTicks));
            Assert.That(a.P2.ParalysisRemaining, Is.EqualTo(b.P2.ParalysisRemaining));
            Assert.That(a.P1.UltimateUses, Is.EqualTo(1));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void ThreeGroundNormalsCanChainAndProduceSingleComboFinish(int character)
        {
            var sim = Create(character); var events = new List<BattleEvent>();
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            for (int i = 0; i < 100 && !sim.Match.IsFinished; i++)
            {
                events.AddRange(sim.Events);
                sim.Tick(new FrameInput(pressed: i == 1 || i == 24 ? InputButtons.Attack : InputButtons.None), default);
            }
            Assert.That(events.Count(e => e.Kind == BattleEventKind.Hit), Is.EqualTo(3));
            Assert.That(events.Count(e => e.Kind == BattleEventKind.ComboFinish), Is.EqualTo(1));
            Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
        }
        [Test]
        public void CutInPausesStatusAndProjectilesAndDiscardsCinematicButtonEdges()
        {
            var sim = Create(0, 12); sim.Tick(Special(SpecialSlot.Neutral), default); Idle(sim, 26);
            sim.P1.Gauge = 100; sim.P2.MarkRemaining = 60;
            SkillSystem.Buff(sim.P1, sim.P1.Definition.Counter.Buff);
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            var shot = sim.Projectiles.Single(); var position = shot.Position; int lifetime = shot.RemainingFrames;
            int mark = sim.P2.MarkRemaining, buff = sim.P1.BuffRemaining;
            Idle(sim, 29); sim.Tick(default, new FrameInput(pressed: InputButtons.Attack));
            Assert.That(shot.Position, Is.EqualTo(position)); Assert.That(shot.RemainingFrames, Is.EqualTo(lifetime));
            Assert.That(sim.P2.MarkRemaining, Is.EqualTo(mark)); Assert.That(sim.P1.BuffRemaining, Is.EqualTo(buff));
            sim.Tick(default, default); Assert.That(sim.P2.Attack, Is.Null);
            Assert.That(sim.P1.BuffRemaining, Is.EqualTo(buff - 1));
        }
        [Test]
        public void SimultaneousUltimatesShareOneThirtyFrameFreeze()
        {
            var c = PhaseThreeDefaults.Character(1);
            var sim = new BattleSimulation(c, c, PhaseOneDefaults.Rules(), PhaseOneDefaults.Stage());
            sim.P1.Gauge = sim.P2.Gauge = 100;
            var ultimate = new FrameInput(pressed: InputButtons.Ultimate); sim.Tick(ultimate, ultimate);
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.UltimateStarted), Is.EqualTo(2));
            Assert.That(sim.CutInRemainingFrames, Is.EqualTo(30)); Idle(sim, 30);
            Assert.That(sim.CutInRemainingFrames, Is.Zero); Assert.That(sim.Match.TickCount, Is.EqualTo(1));
            sim.Tick(default, default); Assert.That(sim.Match.TickCount, Is.EqualTo(2));
        }
        [Test]
        public void LastTickUltimateDoesNotDelayTimeoutOrMakeTimeNegative()
        {
            var sim = Create(0, -12); sim.Match.RemainingTicks = 1; sim.P1.Gauge = 100;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            Assert.That(sim.Match.IsFinished, Is.True); Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.TimeUp));
            Assert.That(sim.Match.RemainingTicks, Is.Zero); Assert.That(sim.CutInRemainingFrames, Is.Zero);
            Idle(sim, 60); Assert.That(sim.Match.RemainingTicks, Is.Zero);
        }
        [Test]
        public void DataDrivenSelfBuffAndStartupAndKnockbackMultipliersAreUsed()
        {
            var c = PhaseThreeDefaults.Character(0); var a = c.Specials[3];
            a.SelfBuff = new BuffDefinition { DurationFrames = 60, MoveMultiplier = 1.1f, DashMultiplier = 1.2f, StartupMultiplier = 0.5f };
            a.KnockbackMultiplier = 2; c.Specials[3] = a;
            var sim = new BattleSimulation(c, PhaseTwoTestFactory.Character(), PhaseOneDefaults.Rules(),
                new StageGeometry(new[] { PhaseOneDefaults.Stage()[0] }, -24, 24, -10, new BattleVector(0, 1.1f), new BattleVector(1.5f, 1.1f)));
            sim.Tick(Special(SpecialSlot.Down), default); Idle(sim, 4);
            Assert.That(sim.P1.BuffRemaining, Is.EqualTo(60));
            Assert.That(sim.P2.Velocity.X, Is.EqualTo(10)); Assert.That(sim.P2.Velocity.Y, Is.EqualTo(8));
            Idle(sim, 45); sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(sim.P1.Attack.Definition.StartupFrames, Is.EqualTo(2));
        }
        [Test]
        public void ParalyzedFighterCanStillBeDamagedAndDefendedRiceDoesNotParalyze()
        {
            var sim = Create(2); sim.P2.ParalysisRemaining = 30;
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default); Idle(sim, 5);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.ParalysisRemaining, Is.GreaterThan(0));
            var guard = Create(2); guard.P2.Mode = FighterMode.Guard; guard.P2.DefenseAge = 10;
            guard.Tick(Special(SpecialSlot.Neutral), new FrameInput(held: InputButtons.Guard));
            for (int i = 0; i < 10; i++) guard.Tick(default, new FrameInput(held: InputButtons.Guard));
            Assert.That(guard.P2.Hearts, Is.EqualTo(3)); Assert.That(guard.P2.ParalysisRemaining, Is.Zero);
        }
    }
}
