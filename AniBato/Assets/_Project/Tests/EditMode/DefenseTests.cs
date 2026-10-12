using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class DefenseTests
    {
        [Test]
        public void UserDecision_NormalIsBlockedByBothFreshAndOldGuard()
        {
            foreach (int age in new[] { 0, 6 })
            {
                var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
                for (int i = 0; i < age; i++) sim.Tick(default, PhaseTwoTestFactory.Guard);
                sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), PhaseTwoTestFactory.Guard);
                Assert.That(sim.P2.NormalHitCount, Is.Zero);
                Assert.That(sim.P2.Hearts, Is.EqualTo(3));
                Assert.That(sim.Events.Any(e => e.Kind == (age == 0 ? BattleEventKind.JustGuard : BattleEventKind.Guarded)), Is.True);
            }
        }
        [Test]
        public void Spec08_SpecialIsGuardedWithDataDrivenStunAndPush()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            for (int i = 0; i < 6; i++) sim.Tick(default, PhaseTwoTestFactory.Guard);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), PhaseTwoTestFactory.Guard);
            Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Guarded), Is.True);
            Assert.That(sim.P2.GuardStunRemaining, Is.EqualTo(sim.Rules.Defense.GuardStunFrames));
            Assert.That(sim.P2.Velocity.X, Is.EqualTo(sim.Rules.Defense.GuardPushSpeed));
        }
        [TestCase(true)] [TestCase(false)]
        public void Spec09_CounterWinsSameTickInEitherPlayerOrder(bool attackerIsP1)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            var attack = PhaseTwoTestFactory.Button(InputButtons.Attack); var counter = PhaseTwoTestFactory.Button(InputButtons.Counter);
            sim.Tick(attackerIsP1 ? attack : counter, attackerIsP1 ? counter : attack);
            var attacker = attackerIsP1 ? sim.P1 : sim.P2; var defender = attackerIsP1 ? sim.P2 : sim.P1;
            Assert.That(defender.NormalHitCount, Is.Zero); Assert.That(defender.Hearts, Is.EqualTo(3));
            Assert.That(attacker.NormalHitCount, Is.EqualTo(1));
            Assert.That(attacker.Attack, Is.Null); Assert.That(attacker.Mode, Is.EqualTo(FighterMode.Knockback));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.CounterSuccess), Is.True);
        }
        [Test]
        public void Spec10_CounterMissHasFullRecoveryAndCooldown()
        {
            var sim = BattleTestFactory.Create();
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Counter), default);
            BattleTestFactory.Idle(sim, sim.Rules.Defense.CounterWindowFrames);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery));
            float start = sim.P1.Position.X;
            for (int i = 0; i < sim.Rules.Defense.CounterFailureFrames - 1; i++)
                sim.Tick(new FrameInput(1, pressed: InputButtons.Counter | InputButtons.Attack), default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(start)); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P1.CounterCooldownRemaining, Is.GreaterThan(0));
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.P1.Attack, Is.Not.Null, "失敗硬直終了直後には攻撃できる必要があります。");
        }
        [Test]
        public void Spec11_CounterBreaksAtThirdAccumulatedHitAndInvokesHookOnce()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.P1.NormalHitCount = 2; var effect = new CounterProbe(); sim.CounterEffect = effect;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), PhaseTwoTestFactory.Button(InputButtons.Counter));
            Assert.That(sim.P1.NormalHitCount, Is.Zero); Assert.That(sim.P1.Hearts, Is.EqualTo(2));
            Assert.That(sim.P1.ImmunityFrom(1), Is.EqualTo(sim.Rules.Defense.CounterTargetImmunityFrames));
            Assert.That(sim.P2.CounterCooldownRemaining, Is.GreaterThan(0)); Assert.That(effect.Calls, Is.EqualTo(1));
            Assert.That(sim.P2.Gauge, Is.EqualTo(sim.Rules.Gauge.OnCounter));
        }
        [TestCase(AttackKind.Special)] [TestCase(AttackKind.Ultimate)]
        public void CounterLosesToSpecialAndUltimate(AttackKind kind)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.P1.Gauge = 100;
            sim.Tick(PhaseTwoTestFactory.Button(kind == AttackKind.Special ? InputButtons.Special : InputButtons.Ultimate),
                PhaseTwoTestFactory.Button(InputButtons.Counter));
            Assert.That(sim.P2.Hearts, Is.EqualTo(kind == AttackKind.Special ? 2 : 1));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.CounterSuccess), Is.False);
        }
        [Test]
        public void RangedNormalCannotBeCountered()
        {
            var character = PhaseTwoTestFactory.Character();
            var attack = character.GroundNormals[0]; attack.IsCloseRange = false; character.GroundNormals[0] = attack;
            var sim = BattleTestFactory.Create(true, definition: character);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), PhaseTwoTestFactory.Button(InputButtons.Counter));
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P1.NormalHitCount, Is.Zero);
        }
        [TestCase(InputButtons.Attack)] [TestCase(InputButtons.Special)]
        public void NormalDodgeAvoidsNormalAndSpecialAfterJustWindow(InputButtons button)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.Tick(default, PhaseTwoTestFactory.Button(InputButtons.Dodge));
            for (int i = 0; i < 5; i++) sim.Tick(default, default);
            sim.P2.Position = new BattleVector(1.5f, 1.1f);
            sim.Tick(PhaseTwoTestFactory.Button(button), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Dodged), Is.True);
        }
        [TestCase(InputButtons.Guard)] [TestCase(InputButtons.Dodge)]
        public void Spec14_JustDefenseAvoidsUltimateAndRewardsGauge(InputButtons button)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character()); sim.P1.Gauge = 100;
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate),
                button == InputButtons.Guard ? PhaseTwoTestFactory.Guard : PhaseTwoTestFactory.Button(button));
            Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.P2.Gauge, Is.EqualTo(button == InputButtons.Guard ? sim.Rules.Gauge.OnJustGuard : sim.Rules.Gauge.OnJustDodge));
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery));
            Assert.That(sim.Events.Any(e => e.Kind == (button == InputButtons.Guard ? BattleEventKind.JustGuard : BattleEventKind.JustDodge)), Is.True);
        }
        [TestCase(InputButtons.Guard)] [TestCase(InputButtons.Dodge)]
        public void UserDecision_UltimateIsGuardedButPiercesNormalDodge(InputButtons button)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character()); sim.P1.Gauge = 100;
            sim.Tick(default, button == InputButtons.Guard ? PhaseTwoTestFactory.Guard : PhaseTwoTestFactory.Button(button));
            for (int i = 0; i < 5; i++) sim.Tick(default, button == InputButtons.Guard ? PhaseTwoTestFactory.Guard : default);
            sim.P2.Position = new BattleVector(1.5f, 1.1f);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Ultimate), button == InputButtons.Guard ? PhaseTwoTestFactory.Guard : default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(button == InputButtons.Guard ? 3 : 1));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Guarded), Is.EqualTo(button == InputButtons.Guard));
        }
        [TestCase(InputButtons.Guard)] [TestCase(InputButtons.Dodge)]
        public void AlreadyOverlappingActiveUltimateCannotBecomeJustDefense(InputButtons button)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.P1.Attack = new AttackInstance(999, sim.P1.Definition.Ultimate, 0, false);
            sim.Tick(default, button == InputButtons.Guard ? PhaseTwoTestFactory.Guard : PhaseTwoTestFactory.Button(button));
            Assert.That(sim.P2.Hearts, Is.EqualTo(button == InputButtons.Guard ? 3 : 1));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.Guarded), Is.EqualTo(button == InputButtons.Guard));
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.JustGuard || e.Kind == BattleEventKind.JustDodge), Is.False);
        }
        [TestCase(4, true)] [TestCase(5, false)]
        public void JustGuardWindowHasExactBoundary(int age, bool expectedJust)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            for (int i = 0; i < age; i++) sim.Tick(default, PhaseTwoTestFactory.Guard);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), PhaseTwoTestFactory.Guard);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.JustGuard), Is.EqualTo(expectedJust));
        }
        [Test]
        public void GuardStopsMovementAndReleaseHasRecovery()
        {
            var sim = BattleTestFactory.Create(); float start = sim.P1.Position.X;
            sim.Tick(new FrameInput(1, held: InputButtons.Guard), default);
            sim.Tick(new FrameInput(1, held: InputButtons.Guard), default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(start));
            sim.Tick(new FrameInput(1), default); Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery));
            BattleTestFactory.Idle(sim, sim.Rules.Defense.GuardReleaseFrames);
            sim.Tick(new FrameInput(1), default); Assert.That(sim.P1.Position.X, Is.GreaterThan(start));
        }
        [Test]
        public void AirDodgeIsAvailableAndRecoversBeforeMovement()
        {
            var sim = BattleTestFactory.Create(); sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Jump), default);
            sim.Tick(new FrameInput(1, pressed: InputButtons.Dodge), default);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Dodge)); Assert.That(sim.P1.Grounded, Is.False);
            Assert.That(sim.P1.Velocity.X, Is.EqualTo(sim.Rules.Defense.AirDodgeSpeed));
            BattleTestFactory.Idle(sim, sim.Rules.Defense.DodgeInvulnerableFrames);
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery));
        }
        [TestCase(7, true)] [TestCase(8, false)]
        public void CounterWindowHasExactBoundary(int age, bool success)
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.Tick(default, PhaseTwoTestFactory.Button(InputButtons.Counter));
            for (int i = 1; i < age; i++) sim.Tick(default, default);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), default);
            Assert.That(sim.Events.Any(e => e.Kind == BattleEventKind.CounterSuccess), Is.EqualTo(success));
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(success ? 0 : 1));
        }
        [Test]
        public void NormalProjectileCannotBeCounteredEvenWithCloseRangeFlag()
        {
            var character = PhaseTwoTestFactory.Character(); var attack = character.GroundNormals[0];
            attack.SpawnProjectile = true; attack.ProjectileSpeed = 1; attack.ProjectileLifetimeFrames = 30;
            character.GroundNormals[0] = attack;
            var sim = BattleTestFactory.Create(true, definition: character);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), PhaseTwoTestFactory.Button(InputButtons.Counter));
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P1.NormalHitCount, Is.Zero);
        }
        [Test]
        public void PiercingFlagPreventsNormalDodgeOutsideJustWindow()
        {
            var character = PhaseTwoTestFactory.Character(); var attack = character.Specials[0];
            attack.PiercesNormalDodge = true; character.Specials[0] = attack;
            var sim = BattleTestFactory.Create(true, definition: character);
            sim.Tick(default, PhaseTwoTestFactory.Button(InputButtons.Dodge)); BattleTestFactory.Idle(sim, 5);
            sim.P2.Position = new BattleVector(1.5f, 1.1f);
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Special), default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(2));
        }
        [Test]
        public void JustDodgeHasReactionAdvantageOverMeleeAttacker()
        {
            var sim = BattleTestFactory.Create(true, definition: PhaseTwoTestFactory.Character());
            sim.Tick(PhaseTwoTestFactory.Button(InputButtons.Attack), PhaseTwoTestFactory.Button(InputButtons.Dodge));
            Assert.That(sim.P2.ActionFrames, Is.LessThan(sim.P1.ActionFrames));
            BattleTestFactory.Idle(sim, sim.Rules.Defense.JustDodgeRecoveryFrames);
            sim.Tick(new FrameInput(-1), default);
            Assert.That(sim.P2.Mode, Is.Not.EqualTo(FighterMode.Recovery));
            Assert.That(sim.P1.Mode, Is.EqualTo(FighterMode.Recovery));
        }
        private sealed class CounterProbe : ICounterEffect
        {
            public int Calls;
            public void OnSuccess(FighterState defender, FighterState attacker, int tick, in BattleRuleDefinition rules,
                System.Collections.Generic.List<BattleEvent> events) { Calls++; }
        }
    }
}
