using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class PhaseTwoAttackTests
    {
        [Test]
        public void Spec05_SpecialLosesOneHeartAndKeepsNormalAccumulation()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.P2.NormalHitCount = 2;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2));
        }
        [Test]
        public void Review_HeartCapKeepsTwoOfThreeUntilNextOpportunity()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character()); sim.P2.NormalHitCount = 2;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), default);
            BattleTestFactory.Idle(sim, 3);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Break), Is.False);
            BattleTestFactory.Idle(sim, sim.Rules.ComboWindowFrames + 1);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(1)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Break), Is.True);
        }
        [Test]
        public void Spec06_ThreeHitComboFinishesAndBlocksSameAttackerThenExpires()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            BattleTestFactory.Hit(sim); BattleTestFactory.Hit(sim);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.Mode, Is.EqualTo(FighterMode.Knockback));
            Assert.That(sim.P2.Velocity.X, Is.EqualTo(sim.Rules.ComboFinishKnockback.X));
            Assert.That(sim.P2.Velocity.Y, Is.EqualTo(sim.Rules.ComboFinishKnockback.Y));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.ComboFinish), Is.True);
            var attack = new AttackInstance(1234, sim.P1.Definition.GroundNormals[0], 0, false);
            sim.P1.Attack = attack; sim.P2.Position = new BattleVector(1.5f, 1.1f); sim.P2.Velocity = default;
            sim.P2.Grounded = true;
            sim.Tick(default, default);
            Assert.That(sim.P2.NormalHitCount, Is.Zero); Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Hit), Is.False);
            Assert.That(attack.HasHit(1), Is.False);
            BattleTestFactory.Idle(sim, sim.Rules.FollowupImmunityFrames);
            sim.P2.Position = new BattleVector(1.5f, 1.1f); sim.P2.Velocity = default; sim.P2.Grounded = true;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
        }
        [Test]
        public void SeparateAccumulatedBreakHasNoComboFinish()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            for (int i = 0; i < 2; i++) { BattleTestFactory.Hit(sim); BattleTestFactory.Idle(sim, 60); }
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(2));
            Assert.That(sim.P2.Velocity.X, Is.Zero);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.ComboFinish), Is.False);
        }
        [Test]
        public void ComboStartingAtTwoAccumulatedHitsCannotLoseTwoHearts()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character()); sim.P2.NormalHitCount = 2;
            BattleTestFactory.Hit(sim); BattleTestFactory.Hit(sim);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(2)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.ComboFinish), Is.True);
        }
        [Test]
        public void RepeatedSpecialsShareComboHeartBudgetUntilWindowExpires()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            for (int i = 0; i < 5; i++)
            { sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), default); BattleTestFactory.Idle(sim, 3); }
            Assert.That(sim.P2.Hearts, Is.EqualTo(2));
            BattleTestFactory.Idle(sim, sim.Rules.ComboWindowFrames + 1);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), default); Assert.That(sim.P2.Hearts, Is.EqualTo(1));
        }
        [TestCase(0, 0, SpecialSlot.Neutral)] [TestCase(1, 0, SpecialSlot.Side)]
        [TestCase(0, 1, SpecialSlot.Up)] [TestCase(0, -1, SpecialSlot.Down)]
        public void DirectionSelectsFourSpecialSlots(float x, float y, SpecialSlot slot)
        {
            var sim = BattleTestFactory.Create();
            sim.Tick(new FrameInput(x, y, pressed: InputButtons.Special), default);
            Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(slot));
            Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(sim.P1.Definition.Special(slot).Name));
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Special));
        }
        [Test]
        public void ProjectileOutlivesOwnerAttackAndHitsOnlyOnceWithSameId()
        {
            var character = PhaseTwoTestFactory.Character(); var projectile = character.Specials[0];
            projectile.SpawnProjectile = true; projectile.ProjectileSpeed = 0.1f; projectile.ProjectileLifetimeFrames = 60;
            character.Specials[0] = projectile;
            var sim = BattleTestFactory.Create(true, definition: character); var ids = new List<long>();
            for (int i = 0; i < 45; i++)
            {
                sim.Tick(i == 0 ? PhaseTwoTestFactory.Button(InputButtons.Special) : default, default);
                ids.AddRange(sim.Events.Where(e => e.Kind == BattleEventKind.Hit).Select(e => e.AttackId));
            }
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.Projectiles.Count, Is.EqualTo(1));
            Assert.That(ids.Count, Is.EqualTo(1)); Assert.That(ids[0], Is.EqualTo(sim.Projectiles[0].Attack.Id));
            Assert.That(sim.P2.Hearts, Is.EqualTo(2));
            BattleTestFactory.Idle(sim, 20); Assert.That(sim.Projectiles, Is.Empty);
        }
        [Test]
        public void Spec12_UltimateCosts100LosesTwoHeartsAndPreservesAccumulation()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.P1.Gauge = 100; sim.P2.NormalHitCount = 2;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(1)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2));
            Assert.That(sim.P1.UltimateUses, Is.EqualTo(1)); Assert.That(sim.P1.Gauge, Is.EqualTo(sim.Rules.Gauge.OnHit));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.UltimateStarted), Is.True);
        }
        [Test]
        public void UltimateRequiresFullGaugeAndCanOnlyBeUsedOncePerMatch()
        {
            var sim = BattleTestFactory.Create(definition: PhaseTwoTestFactory.Character());
            sim.P1.Gauge = 99; sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), default);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.UltimateUses, Is.Zero);
            sim.P1.Gauge = 100; sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), default);
            Assert.That(sim.P1.Gauge, Is.Zero); Assert.That(sim.P1.UltimateUses, Is.EqualTo(1));
            BattleTestFactory.Idle(sim, 100); sim.P1.Gauge = 100;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), default);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.Gauge, Is.EqualTo(100));
        }
        [Test]
        public void WhiffedUltimateKeepsItsLongDataDrivenRecovery()
        {
            var sim = BattleTestFactory.Create(); sim.P1.Gauge = 100;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), default);
            BattleTestFactory.Idle(sim, sim.P1.Definition.Ultimate.TotalFrames - 1);
            Assert.That(sim.P1.Attack, Is.Not.Null);
            sim.Tick(default, default); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void GaugeIncreasesOnHitAndDamageAndClampsAt100()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P1.Gauge, Is.EqualTo(sim.Rules.Gauge.OnHit)); Assert.That(sim.P2.Gauge, Is.EqualTo(sim.Rules.Gauge.OnDamaged));
            sim.P1.Gauge = 99; sim.P2.Gauge = 99; BattleTestFactory.Idle(sim, 20);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P1.Gauge, Is.EqualTo(100)); Assert.That(sim.P2.Gauge, Is.EqualTo(100));
        }
        [Test]
        public void UpSpecialReturnsFromOffstageAndCannotRepeatBeforeLanding()
        {
            var sim = BattleTestFactory.Create();
            sim.P1.Position = new BattleVector(19, -2); sim.P1.Grounded = false; sim.P1.JumpsUsed = 2; sim.P1.Velocity = default;
            sim.Tick(new FrameInput(-1, 1, pressed: InputButtons.Special), default);
            bool aboveStage = false;
            for (int i = 0; i < 45; i++)
            {
                sim.Tick(default, default); aboveStage |= sim.P1.Position.Y > 1.1f;
            }
            Assert.That(sim.P1.UpSpecialUsed, Is.True); Assert.That(aboveStage, Is.True);
            sim.Tick(new FrameInput(y: 1, pressed: InputButtons.Special), default);
            Assert.That(sim.P1.Attack, Is.Null);
            for (int i = 0; i < 100 && !sim.P1.Grounded && !sim.Match.IsFinished; i++) sim.Tick(new FrameInput(-1), default);
            Assert.That(sim.Match.IsFinished, Is.False); Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.UpSpecialUsed, Is.False);
            sim.Tick(new FrameInput(y: 1, pressed: InputButtons.Special), default); Assert.That(sim.P1.Attack, Is.Not.Null);
        }
        [Test]
        public void Spec15_KnockbackCrossingBlastLineLosesImmediately()
        {
            var sim = BattleTestFactory.Create();
            sim.P2.Position = new BattleVector(23.9f, 1); sim.P2.Grounded = false;
            sim.P2.Mode = FighterMode.Knockback; sim.P2.HitstunRemaining = 20; sim.P2.Velocity = new BattleVector(18, 0);
            sim.Tick(default, default);
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut)); Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player1Wins));
        }
        [Test]
        public void SimultaneousPressesUseDeclaredPriorityAndDiscardLowerActions()
        {
            var sim = BattleTestFactory.Create(); sim.P1.Gauge = 100;
            sim.Tick(new FrameInput(1, 1, held: InputButtons.Guard, pressed: (InputButtons)255), default);
            Assert.That(sim.P1.Attack.Definition.IsUltimate, Is.True); Assert.That(sim.P1.JumpsUsed, Is.Zero);
            BattleTestFactory.Idle(sim, 100);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Idle));
        }
        [Test]
        public void FinishBlocksFourthContactAlreadyGatheredInSameTick()
        {
            var character = PhaseTwoTestFactory.Character();
            var ranged = character.Specials[0]; ranged.Kind = AttackKind.Normal; ranged.IsCloseRange = false;
            ranged.SpawnProjectile = true; ranged.ProjectileSpeed = 0.1f; ranged.ProjectileLifetimeFrames = 60;
            ranged.ActiveFrames = 3; ranged.RecoveryFrames = 0;
            character.GroundNormals[0] = ranged;
            var sim = BattleTestFactory.Create(true, definition: character);
            sim.P2.Position = new BattleVector(10, 1.1f);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.Projectiles.Count, Is.EqualTo(1));
            sim.ComboP1.RegisterHit(0, sim.Rules); sim.ComboP1.RegisterHit(0, sim.Rules);
            sim.P2.NormalHitCount = 2;
            // Melee finishes the combo while the earlier projectile also overlaps in this tick.
            sim.P2.Position = new BattleVector(1.5f, 1.1f); sim.P2.Velocity = default; sim.P2.Grounded = true;
            sim.P1.Attack = new AttackInstance(223, character.GroundNormals[2], 0, false);
            sim.Tick(default, default);
            Assert.That(sim.P2.NormalHitCount, Is.Zero); Assert.That(sim.P2.Hearts, Is.EqualTo(2));
            Assert.That(sim.Events.Count(e => e.Kind == BattleEventKind.Hit), Is.EqualTo(1));
        }
        [Test]
        public void SpecialDirectionSurvivesReleaseBeforeTickAndBufferedRecovery()
        {
            var latch = new FrameInputLatch();
            latch.Sample(new FrameInput(y: 1, pressed: InputButtons.Special));
            latch.Sample(PhaseTwoTestFactory.Button(InputButtons.Jump)); latch.Sample(default);
            var sim = BattleTestFactory.Create(); sim.Tick(latch.Consume(), default);
            Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Up));
            var buffered = BattleTestFactory.Create();
            buffered.P1.Mode = FighterMode.Recovery; buffered.P1.ActionFrames = 2;
            buffered.Tick(new FrameInput(-1, pressed: InputButtons.Special), default);
            buffered.Tick(default, default); buffered.Tick(default, default);
            Assert.That(buffered.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Side));
            Assert.That(buffered.P1.Facing, Is.EqualTo(-1));
        }
        [TestCase(InputButtons.Counter | InputButtons.Dodge, FighterMode.Counter)]
        [TestCase(InputButtons.Dodge | InputButtons.Special, FighterMode.Dodge)]
        [TestCase(InputButtons.Special | InputButtons.Attack, FighterMode.Special)]
        [TestCase(InputButtons.Attack | InputButtons.Guard, FighterMode.Attack)]
        [TestCase(InputButtons.Guard | InputButtons.Jump, FighterMode.Guard)]
        [TestCase(InputButtons.Jump | InputButtons.Dash, FighterMode.Jump)]
        public void ActionPriorityAlsoAppliesBelowUltimate(InputButtons buttons, FighterMode expected)
        {
            var sim = BattleTestFactory.Create();
            sim.Tick(new FrameInput(pressed: buttons), default);
            if (expected == FighterMode.Jump) BattleTestFactory.Idle(sim, sim.Rules.GroundJumpStartupFrames);
            Assert.That(sim.P1.Mode, Is.EqualTo(expected));
        }
        [Test]
        public void UltimateDamageIgnoresAlreadySpentComboHeartBudget()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), default); BattleTestFactory.Idle(sim, 3);
            sim.P1.Gauge = 100; sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), default);
            Assert.That(sim.P2.Hearts, Is.Zero); Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player1Wins));
        }
        [Test]
        public void SecondJumpAndAirDashCanReturnFromOffstage()
        {
            var sim = BattleTestFactory.Create();
            sim.P1.Position = new BattleVector(20, 0.1f); sim.P1.Grounded = false; sim.P1.JumpsUsed = 1; sim.P1.Velocity = default;
            sim.Tick(new FrameInput(-1, pressed: InputButtons.Jump), default);
            for (int i = 0; i < 5; i++) sim.Tick(new FrameInput(-1), default);
            sim.Tick(new FrameInput(-1, pressed: InputButtons.Dash), default);
            Assert.That(sim.P1.JumpsUsed, Is.EqualTo(2)); Assert.That(sim.P1.AirDashesUsed, Is.EqualTo(1));
            BattleTestFactory.Idle(sim, 90);
            Assert.That(sim.Match.IsFinished, Is.False); Assert.That(sim.P1.Grounded, Is.True);
            Assert.That(sim.P1.Position.X, Is.LessThan(18));
        }
        [Test]
        public void UltimateUsesPressedDirectionToHitBehindPreviousFacing()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.P2.Position = new BattleVector(-1.5f, 1.1f); sim.P1.Gauge = 100;
            sim.Tick(new FrameInput(-1, pressed: InputButtons.Ultimate), default);
            Assert.That(sim.P1.Facing, Is.EqualTo(-1)); Assert.That(sim.P2.Hearts, Is.EqualTo(1));
        }
        [Test]
        public void LatchedButtonsKeepIndependentDirectionsWhenHigherActionIsUnavailable()
        {
            var latch = new FrameInputLatch();
            latch.Sample(new FrameInput(1, pressed: InputButtons.Counter));
            latch.Sample(new FrameInput(y: 1, pressed: InputButtons.Special)); latch.Sample(default);
            var sim = BattleTestFactory.Create(); sim.P1.CounterCooldownRemaining = 5;
            sim.Tick(latch.Consume(), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Special)); Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Up));
        }
    }
}
