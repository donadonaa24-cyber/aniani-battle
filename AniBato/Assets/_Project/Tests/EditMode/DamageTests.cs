using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class DamageTests
    {
        [Test]
        public void ThreeSeparateNormalsAccumulateAndBreakOneHeart()
        {
            var sim = BattleTestFactory.Create(true, definition: BattleTestFactory.StationaryAttack());
            for (int i = 1; i <= 3; i++)
            {
                BattleTestFactory.Hit(sim);
                Assert.That(sim.P2.NormalHitCount, Is.EqualTo(i % 3));
                Assert.That(sim.P2.Hearts, Is.EqualTo(i == 3 ? 2 : 3));
                BattleTestFactory.Idle(sim, 60); // independent hits, outside the combo window
            }
        }
        [Test]
        public void AccumulationPersistsThroughTimeMoveJumpAndGuardInput()
        {
            var sim = BattleTestFactory.Create(true, definition: BattleTestFactory.StationaryAttack());
            BattleTestFactory.Hit(sim); BattleTestFactory.Idle(sim, 600);
            sim.Tick(default, new FrameInput(1, held: InputButtons.Guard, pressed: InputButtons.Jump));
            BattleTestFactory.Idle(sim, 120);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void SameAttackOverlappingManyTicksOnlyHitsOnce()
        {
            var sim = BattleTestFactory.Create(true, definition: BattleTestFactory.StationaryAttack(30));
            int hits = 0;
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            for (int i = 0; i < 38; i++)
            { sim.Tick(default, default); hits += sim.Events.Count(e => e.Kind == BattleEventKind.Hit); }
            Assert.That(hits, Is.EqualTo(1)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
        }
        [Test]
        public void QueuedThreeAttackChainUsesUniqueIdsAndEnds()
        {
            var sim = BattleTestFactory.Create(true, definition: BattleTestFactory.StationaryAttack());
            var ids = new System.Collections.Generic.HashSet<long>();
            for (int i = 0; i < 60; i++)
            {
                var input = new FrameInput(pressed: i == 0 || i == 7 || i == 22 ? InputButtons.Attack : InputButtons.None);
                sim.Tick(input, default);
                foreach (var e in sim.Events) if (e.Kind == BattleEventKind.Hit) ids.Add(e.AttackId);
            }
            Assert.That(ids.Count, Is.EqualTo(3)); Assert.That(sim.P2.Hearts, Is.EqualTo(2));
            Assert.That(sim.P2.NormalHitCount, Is.Zero); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.ComboP1.HitCount, Is.EqualTo(3));
        }
        [Test]
        public void HitStopPausesAttackFramesButNotMatchClock()
        {
            var sim = BattleTestFactory.Create(true);
            for (int i = 0; i < 5; i++) sim.Tick(new FrameInput(pressed: i == 0 ? InputButtons.Attack : InputButtons.None), default);
            Assert.That(sim.P1.HitstopRemaining, Is.EqualTo(4));
            int frame = sim.P1.Attack.Frame;
            BattleTestFactory.Idle(sim, 4);
            Assert.That(sim.P1.Attack.Frame, Is.EqualTo(frame)); Assert.That(sim.Match.TickCount, Is.EqualTo(9));
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
        }
        [Test]
        public void ComboHeartCapResetsAfterWindowAndUltimateHasExceptionHook()
        {
            var rules = PhaseOneDefaults.Rules(); var tracker = new ComboTracker();
            tracker.RegisterHit(1, rules); Assert.That(tracker.TryLoseHeart(rules), Is.True);
            tracker.RegisterHit(10, rules); Assert.That(tracker.TryLoseHeart(rules), Is.False);
            Assert.That(tracker.TryLoseHeart(rules, true), Is.True);
            tracker.RegisterHit(100, rules); Assert.That(tracker.HitCount, Is.EqualTo(1));
            Assert.That(tracker.TryLoseHeart(rules), Is.True);
        }
        [Test]
        public void EventsAreTickLocalAndIncludeBreakHeartLostAndKO()
        {
            var rules = PhaseOneDefaults.Rules(); rules.InitialHearts = 1;
            var sim = BattleTestFactory.Create(true, rules, BattleTestFactory.StationaryAttack(startup: 0));
            BattleTestFactory.Hit(sim); BattleTestFactory.Hit(sim);
            sim.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Assert.That(sim.Events.Select(e => e.Kind), Does.Contain(BattleEventKind.Break));
            Assert.That(sim.Events.Select(e => e.Kind), Does.Contain(BattleEventKind.HeartLost));
            Assert.That(sim.Events.Select(e => e.Kind), Does.Contain(BattleEventKind.KO));
            sim.Tick(default, default); Assert.That(sim.Events, Is.Empty);
        }
        [Test]
        public void AssetDefinitionArraysAreSnapshotted()
        {
            var character = BattleTestFactory.StationaryAttack();
            var sim = BattleTestFactory.Create(true, definition: character);
            character.GroundNormals[0] = default;
            BattleTestFactory.Hit(sim); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
        }
        [Test]
        public void NineIndependentNormalsEndMatchByHearts()
        {
            var sim = BattleTestFactory.Create(true, definition: BattleTestFactory.StationaryAttack());
            for (int i = 0; i < 9; i++)
            { BattleTestFactory.Hit(sim); if (i < 8) BattleTestFactory.Idle(sim, 60); }
            Assert.That(sim.P2.Hearts, Is.Zero); Assert.That(sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player1Wins));
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.Hearts));
        }
    }
}
