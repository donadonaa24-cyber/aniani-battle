using System;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class CpuTests
    {
        [Test]
        public void DifficultyChangesReactionCombosAndDefense()
        {
            var a = CpuDefinition.ForDifficulty(0); var b = CpuDefinition.ForDifficulty(1); var c = CpuDefinition.ForDifficulty(2);
            Assert.That(a.ReactionFrames, Is.GreaterThan(b.ReactionFrames)); Assert.That(b.ReactionFrames, Is.GreaterThan(c.ReactionFrames));
            Assert.That(c.ComboChance, Is.GreaterThan(a.ComboChance)); Assert.That(c.JustChance, Is.GreaterThan(a.JustChance));
            a.GuardChance = 2; Assert.Throws<ArgumentException>(() => a.Validate());
        }
        [TestCase(-17)] [TestCase(17)]
        public void NearEdgeReturnsInwardWithoutDashingOut(float x)
        {
            var sim = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(0)); sim.P1.Position = new BattleVector(x, 1.1f);
            var input = new CpuBrain(CpuDefinition.ForDifficulty(1), 17).NextInput(sim, 0);
            Assert.That(input.X, Is.EqualTo(-Math.Sign(x))); Assert.That(input.IsPressed(InputButtons.Dash), Is.False);
        }
        [Test]
        public void RecoveryUsesRemainingJumpThenUpSpecialThenAirDash()
        {
            var sim = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(0));
            sim.P1.Position = new BattleVector(-20, -2); sim.P1.Grounded = false; sim.P1.Velocity = new BattleVector(0, -10);
            var cpu = new CpuBrain(CpuDefinition.ForDifficulty(1), 12);
            Assert.That(cpu.NextInput(sim, 0).IsPressed(InputButtons.Jump), Is.True);
            sim.P1.JumpsUsed = 2; var input = cpu.NextInput(sim, 0);
            Assert.That(input.IsPressed(InputButtons.Special), Is.True); Assert.That(input.ActionY, Is.EqualTo(1));
            sim.P1.UpSpecialUsed = true; sim.P1.Position = new BattleVector(-20, 3);
            Assert.That(cpu.NextInput(sim, 0).IsPressed(InputButtons.Dash), Is.True);
        }
        [Test]
        public void ShrinkWarningReturnsTowardNextFloorBeforeItDisappears()
        {
            var sim = BattleTestFactory.Create(); BattleTestFactory.Idle(sim, 3300);
            sim.P1.Position = new BattleVector(16, 1.1f);
            Assert.That(new CpuBrain(CpuDefinition.ForDifficulty(0), 1).NextInput(sim, 0).X, Is.EqualTo(-1));
        }
        [Test]
        public void SeedReproducesWholeMatchInputsAndOutcome()
        {
            var a = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(1)); var b = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(1));
            var aa = new CpuBrain(CpuDefinition.ForDifficulty(2), 231); var ab = new CpuBrain(CpuDefinition.ForDifficulty(2), 72);
            var ba = new CpuBrain(CpuDefinition.ForDifficulty(2), 231); var bb = new CpuBrain(CpuDefinition.ForDifficulty(2), 72);
            for (int i = 0; i < 4300 && !a.Match.IsFinished; i++)
            {
                var x = aa.NextInput(a, 0); var y = ba.NextInput(b, 0);
                Assert.That(x.Pressed, Is.EqualTo(y.Pressed)); Assert.That(x.X, Is.EqualTo(y.X)); Assert.That(x.ActionY, Is.EqualTo(y.ActionY));
                a.Tick(x, ab.NextInput(a, 1)); b.Tick(y, bb.NextInput(b, 1));
                Assert.That(a.P1.Position.X, Is.EqualTo(b.P1.Position.X)); Assert.That(a.P2.Hearts, Is.EqualTo(b.P2.Hearts));
            }
            Assert.That(a.Match.IsFinished, Is.True); Assert.That(a.Match.Result, Is.EqualTo(b.Match.Result));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void CpuMatchesAttackLoseHeartsAndFinishAcrossRoster(int difficulty)
        {
            int ultMatches = 0, total = 0, unforced = 0;
            for (int p1 = 0; p1 < 3; p1++) for (int p2 = 0; p2 < 3; p2++) for (int seed = 1; seed <= 30; seed++)
            {
                var match = CpuMatchMetrics.Run(PhaseFiveBDefaults.Character(p1), PhaseFiveBDefaults.Character(p2), PhaseFiveBDefaults.Rules(),
                    PhaseOneDefaults.Stage(), CpuDefinition.ForDifficulty(difficulty), seed);
                Assert.That(match.Attacks[0], Is.GreaterThan(0)); Assert.That(match.Attacks[1], Is.GreaterThan(0));
                Assert.That(match.HeartsLost, Is.GreaterThan(0), $"match {p1}-{p2} seed {seed}, {match.Reason}, {match.Result}"); Assert.That(match.Ticks, Is.LessThanOrEqualTo(4200));
                if (match.UltimateStarts > 0) ultMatches++; total++; unforced += match.UnforcedRingOuts;
            }
            if (difficulty == 1) Assert.That(ultMatches / (double)total, Is.GreaterThanOrEqualTo(.5), "ふつう同士の超必殺発動率が目安未満です。");
            Assert.That(unforced / (double)total, Is.LessThan(.1), "直近2秒に被弾していない場外負けが多すぎます。");
        }
        [Test]
        public void NoOpponentIdleCpuDoesNotRunOffStage()
        {
            var sim = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(0)); var cpu = new CpuBrain(CpuDefinition.ForDifficulty(1), 71);
            while (!sim.Match.IsFinished) sim.Tick(cpu.NextInput(sim, 0), default);
            Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Player1Wins), "待機相手との対戦でCPU自身が場外負けしています。");
        }
        [Test]
        public void BothCpusSurviveAllShrinkStepsWhenOffenseIsDisabled()
        {
            var sim = BattleTestFactory.Create(); var data = CpuDefinition.ForDifficulty(1);
            data.AttackChance = data.SpecialChance = data.CounterChance = data.DodgeChance = data.GuardChance = data.JustChance = data.JumpChance = 0;
            var a = new CpuBrain(data, 23); var b = new CpuBrain(data, 97);
            while (!sim.Match.IsFinished) sim.Tick(a.NextInput(sim, 0), b.NextInput(sim, 1));
            Assert.That(sim.Match.TickCount, Is.EqualTo(4200)); Assert.That(sim.Shrink.CompletedSteps, Is.EqualTo(5));
            Assert.That(sim.Match.EndReason, Is.EqualTo(MatchEndReason.TimeUp)); Assert.That(sim.Match.Result, Is.EqualTo(MatchResult.Draw));
        }
        [Test]
        public void UltimateJustGuardIsHeldThroughImpactAndOnlyProducesInput()
        {
            var sim = BattleTestFactory.Create(); sim.P1.Position = new BattleVector(0, 1.1f); sim.P2.Position = new BattleVector(2, 1.1f);
            var ultimate = sim.P2.Definition.Ultimate; sim.P2.Attack = new AttackInstance(99, ultimate, 0, false);
            sim.P2.Attack.Frame = ultimate.StartupFrames - 5; sim.P2.Mode = FighterMode.Special;
            var data = CpuDefinition.ForDifficulty(2); data.ReactionFrames = 0; data.DecisionFrames = 1; data.JustChance = 1;
            var brain = new CpuBrain(data, 13); var input = brain.NextInput(sim, 0);
            Assert.That(input.IsPressed(InputButtons.Guard), Is.True); Assert.That(sim.P1.Mode, Is.Not.EqualTo(FighterMode.Guard));
            sim.Tick(input, default);
            for (int i = 0; i < 4; i++) sim.Tick(brain.NextInput(sim, 0), default);
            Assert.That(sim.Events, Has.Some.Matches<BattleEvent>(e => e.Kind == BattleEventKind.JustGuard));
            Assert.That(sim.P1.Hearts, Is.EqualTo(3));
        }
    }
}
