using System;
using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class RosterRefinementTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleSimulation Create(int index)
        {
            var c = Setup.Character(index).ToDefinition(); c.UltimateCutInFrames = 0;
            var r = PhaseOneDefaults.Rules(); r.CombatInput = CombatInputDefinition.Default;
            return BattleTestFactory.Create(false, r, c);
        }
        private static Projectile Fire(BattleSimulation sim, bool ultimate)
        {
            sim.P1.Gauge = sim.Rules.Gauge.Maximum;
            sim.Tick(new FrameInput(pressed: ultimate ? InputButtons.Ultimate : InputButtons.Special), default);
            for (int i = 0; sim.Projectiles.Count == 0 && i < 150; i++) sim.Tick(default, default);
            Assert.That(sim.Projectiles.Count, Is.GreaterThan(0)); return sim.Projectiles[0];
        }
        [Test]
        public void DroneHomesSlowlyChangesDirectionAndExpiresAfterThreeSecondsWithoutTouchingOwner()
        {
            var sim = Create(0); var shot = Fire(sim, false); var original = shot.Position;
            Assert.That(shot.RemainingFrames, Is.EqualTo(180));
            sim.P2.Position = original + new BattleVector(8, 6); sim.P2.Grounded = false;
            sim.Tick(default, default);
            Assert.That(shot.Velocity.X, Is.GreaterThan(0)); Assert.That(shot.Velocity.Y, Is.GreaterThan(0));
            Assert.That(shot.Velocity.X * shot.Velocity.X + shot.Velocity.Y * shot.Velocity.Y, Is.EqualTo(36).Within(.001));
            sim.P2.Position = shot.Position + new BattleVector(-8, 6); sim.Tick(default, default);
            Assert.That(shot.Velocity.X, Is.LessThan(0)); Assert.That(shot.Facing, Is.EqualTo(-1));
            for (int i = 0; i < 178; i++)
            { sim.P2.Position = new BattleVector(0, 30); sim.P2.Velocity = default; sim.Tick(default, default); }
            Assert.That(sim.Projectiles, Is.Empty); Assert.That(sim.P1.Hearts, Is.EqualTo(3)); Assert.That(sim.P1.NormalHitCount, Is.Zero);
        }
        [TestCase(1)] [TestCase(-1)]
        public void KingMarchesInHisFacingDirectionAndDealsOnlyTwoSecondParalysis(int facing)
        {
            var sim = Create(6); sim.P1.Facing = facing; sim.P2.Position=new BattleVector(sim.P1.Position.X+facing*7,sim.P2.Position.Y);var first = Fire(sim, false);
            var shots = sim.Projectiles.ToArray(); Assert.That(shots.Length, Is.EqualTo(3));
            Assert.That(shots.All(p=>p.RoyalMarch&&p.Facing==facing&&!p.CanDamage),Is.True);
            Assert.That(shots.All(p=>System.Math.Abs(p.Position.X-sim.P1.Position.X)<2),Is.True);
            Assert.That(shots.Select(p => p.Attack).Distinct().Count(), Is.EqualTo(1));
            int hits = 0;
            for (int tick = 0; tick < 310; tick++)
            {
                sim.Tick(default, default);
                foreach (var hit in sim.Events.Where(e => e.Kind == BattleEventKind.Hit))
                { hits++; Assert.That(sim.P2.ParalysisRemaining, Is.EqualTo(120)); }
                Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
            }
            Assert.That(hits, Is.EqualTo(1)); Assert.That(sim.Projectiles, Is.Empty);
        }
        [TestCase(1)] [TestCase(-1)]
        public void KingRushesLeftRightLeftInOrderAndOnlyTheFinisherLosesTwoHearts(int facing)
        {
            var sim = Create(6); sim.P1.Facing = facing; Fire(sim, true); var shots = sim.Projectiles.ToArray();
            Assert.That(shots.Select(p => p.DelayRemaining), Is.EqualTo(new[] { 0, 18, 36 }));
            Assert.That(shots.Select(p => Math.Sign(p.Velocity.X)), Is.EqualTo(new[] { 1, -1, 1 }));
            var hits = new List<long>();
            for (int tick = 0; tick < 150; tick++)
            {
                sim.Tick(default, default);
                hits.AddRange(sim.Events.Where(e => e.Kind == BattleEventKind.Hit).Select(e => e.AttackId));
                Assert.That(sim.P2.Hearts, Is.EqualTo(hits.Contains(shots[2].Attack.Id) ? 1 : 3));
                Assert.That(sim.P2.NormalHitCount, Is.Zero);
            }
            Assert.That(hits, Is.EqualTo(shots.Select(p => p.Attack.Id))); Assert.That(sim.Projectiles, Is.Empty);
        }
        [TestCase(false)] [TestCase(true)]
        public void PlayerTwoKingTargetsOnlyPlayerOneWithTheSameFormationRules(bool ultimate)
        {
            var sim=Create(6);sim.P2.Gauge=sim.Rules.Gauge.Maximum;
            sim.Tick(default,new FrameInput(pressed:ultimate?InputButtons.Ultimate:InputButtons.Special));
            int hits=0;
            for(int tick=0;tick<250;tick++)
            {sim.Tick(default,default);hits+=sim.Events.Count(e=>e.Kind==BattleEventKind.Hit&&e.SourcePlayer==1);}
            Assert.That(hits,Is.EqualTo(ultimate?3:1));Assert.That(sim.P1.Hearts,Is.EqualTo(ultimate?1:3));
            Assert.That(sim.P1.NormalHitCount,Is.Zero);Assert.That(sim.P2.Hearts,Is.EqualTo(3));Assert.That(sim.P2.NormalHitCount,Is.Zero);
        }
        [Test]
        public void PyramidLandsAsASolidWallForExactlyTenSecondsAndKeepsTheThreeBasePlatforms()
        {
            var sim = Create(7); sim.P2.Position = new BattleVector(0, sim.P2.Position.Y);
            var bases = Enumerable.Range(0, sim.Stage.PlatformCount).Select(i => sim.Stage[i]).ToArray();
            var shot = Fire(sim, true); Assert.That(shot.Hitbox.Size, Is.EqualTo(new BattleVector(5, 3.4f)));
            for (int i = 0; !shot.Landed && i < 160; i++) sim.Tick(default, default);
            Assert.That(shot.Landed, Is.True); Assert.That(shot.CanDamage, Is.False); Assert.That(shot.RemainingFrames, Is.EqualTo(600));
            int wall = sim.Stage.CollisionPlatformCount - 1; Assert.That(wall, Is.EqualTo(3)); Assert.That(sim.Stage[wall].OneWay, Is.False);
            Assert.That(sim.Stage.Snapshot().IsActive(wall), Is.True);
            // A fighter inside the newly materialised wall is expelled rather than trapped.
            sim.P1.Position = shot.Position; sim.P1.Grounded = false; sim.P1.Attack = null; sim.P1.Velocity = default;
            int frozenTicks=0;
            do { sim.Tick(default,default);frozenTicks++; } while(sim.P1.IsFrozenThisTick&&frozenTicks<20);
            Assert.That(frozenTicks,Is.LessThan(20));Assert.That(sim.P1.Hurtbox.Overlaps(sim.Stage[wall].Bounds), Is.False);
            sim.P1.Position = new BattleVector(-10, 1.1f); sim.P1.Velocity = default;
            Assert.That(shot.RemainingFrames,Is.EqualTo(600-frozenTicks));
            BattleTestFactory.Idle(sim, shot.RemainingFrames-1); Assert.That(sim.Stage.IsActive(wall), Is.True); Assert.That(shot.RemainingFrames, Is.EqualTo(1));
            sim.Tick(default, default); Assert.That(sim.Stage.IsActive(wall), Is.False); Assert.That(sim.Projectiles.Contains(shot), Is.False);
            Assert.That(sim.Stage.PlatformCount, Is.EqualTo(3));
            for (int i = 0; i < 3; i++) Assert.That(sim.Stage[i], Is.EqualTo(bases[i]));
        }
        [Test]
        public void DynamicWallBlocksWalkingThenExpiresWithoutChangingBlastLines()
        {
            var sim = Create(0); int wall = sim.Stage.AddSolid(new BattleRect(new BattleVector(0, 2), new BattleVector(3, 4)));
            var copy = sim.Stage.Snapshot(); sim.P1.Position = new BattleVector(-4, 1.1f); sim.P1.Velocity = default;
            for (int i = 0; i < 60; i++) sim.Tick(new FrameInput(1), default);
            Assert.That(sim.P1.Hurtbox.Right, Is.LessThanOrEqualTo(-1.5f + .001f)); Assert.That(sim.Stage.LeftBlastLine, Is.EqualTo(-24));
            sim.Stage.DeactivatePlatform(wall); Assert.That(copy.IsActive(wall), Is.True);
            for (int i = 0; i < 60; i++) sim.Tick(new FrameInput(1), default);
            Assert.That(sim.P1.Position.X, Is.GreaterThan(1.5f));
        }
        [Test]
        public void PyramidPassesTheUpperPlatformAndHitsTheFighterOnTheMainFloor()
        {
            var sim=Create(7);sim.P2.Position=new BattleVector(8,1.1f);var shot=Fire(sim,true);
            for(int tick=0;!shot.Landed&&tick<150;tick++)sim.Tick(default,default);
            Assert.That(shot.Landed,Is.True);Assert.That(shot.Hitbox.Bottom,Is.EqualTo(sim.Stage[0].Bounds.Top).Within(.001f));
            Assert.That(shot.RemainingFrames,Is.EqualTo(600));Assert.That(sim.P2.Hearts,Is.EqualTo(1));
            Assert.That(sim.P2.NormalHitCount,Is.Zero);Assert.That(shot.CanDamage,Is.False);
        }
        [Test]
        public void CurryLeavesFiveAdditionalSecondsOfOpponentOnlyRepeatedOneThirdDamageAfterTheSplash()
        {
            var sim = Create(2); sim.P1.NormalHitCount = 2; sim.P1.Gauge = sim.Rules.Gauge.Maximum;
            sim.P2.Position = new BattleVector(14, 1.1f); sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default);
            Projectile residue = null;
            for (int tick = 0; residue == null && tick < 150; tick++)
            { sim.Tick(default, default); residue = sim.Projectiles.FirstOrDefault(p => p.Attack.Definition.Ability.Look == AbilityLook.CurryResidue); }
            Assert.That(residue, Is.Not.Null); Assert.That(residue.Visible, Is.False); Assert.That(residue.CanDamage, Is.False);
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1));
            // The curry already spilled; being hit now cannot retroactively cancel it.
            sim.P1.Attack = null; sim.P1.Mode = FighterMode.Hitstun; sim.P1.HitstunRemaining = 12;
            for(int tick=0;!residue.Visible&&tick<150;tick++)sim.Tick(default, default);
            Assert.That(residue.Visible,Is.True);
            Assert.That(residue.RemainingFrames, Is.EqualTo(300));
            var ownContacts = new List<HitContact>(); new HitResolver().Gather(residue, sim.P1, ownContacts); Assert.That(ownContacts, Is.Empty);
            int hits = 0, previousHit = -100;
            for (int tick = 0; tick < 300; tick++)
            {
                sim.P2.Position = new BattleVector(residue.Position.X, sim.P2.Definition.HurtboxSize.Y * .5f);
                sim.P2.Velocity = default; sim.P2.DamageRecoilRemaining = 0; sim.Tick(default, default);
                foreach (var hit in sim.Events.Where(e => e.Kind == BattleEventKind.Hit && e.AttackId == residue.Attack.Id))
                { Assert.That(tick - previousHit, Is.GreaterThanOrEqualTo(60)); previousHit = tick; hits++; }
            }
            Assert.That(hits, Is.GreaterThanOrEqualTo(3)); Assert.That(sim.Projectiles.Contains(residue), Is.False);
            Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P1.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void InterruptedBeforeCurryIsSpilledProducesNoAftermath()
        {
            var sim = Create(2); sim.P1.Gauge = sim.Rules.Gauge.Maximum;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default); sim.P1.Attack = null;
            BattleTestFactory.Idle(sim, 200); Assert.That(sim.Projectiles, Is.Empty);
        }
        [Test]
        public void HitstopDelaysResidueUntilTheOriginalSplashFinishes()
        {
            var sim=Create(2);sim.P2.Position=new BattleVector(14,1.1f);sim.P1.Gauge=sim.Rules.Gauge.Maximum;
            sim.Tick(new FrameInput(pressed:InputButtons.Ultimate),default);Projectile residue=null;
            for(int tick=0;residue==null&&tick<150;tick++)
            {sim.Tick(default,default);residue=sim.Projectiles.FirstOrDefault(p=>p.Attack.Definition.Ability.Look==AbilityLook.CurryResidue);}
            Assert.That(residue,Is.Not.Null);var original=sim.P1.Attack;int delay=residue.DelayRemaining;
            sim.P1.HitstopRemaining=5;
            BattleTestFactory.Idle(sim,5);Assert.That(residue.DelayRemaining,Is.EqualTo(delay));Assert.That(residue.Visible,Is.False);
            for(int tick=0;!residue.Visible&&tick<150;tick++)sim.Tick(default,default);
            Assert.That(residue.Visible,Is.True);Assert.That(original.IsActive,Is.False);Assert.That(residue.RemainingFrames,Is.EqualTo(300));
        }
        [Test]
        public void RyutaChantGrantsTwentySecondSpeedOnlyAfterCompletionAndShortBuffsCannotShortenIt()
        {
            var sim = Create(8); sim.P1.Gauge = sim.Rules.Gauge.Maximum;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default); var attack = sim.P1.Attack;
            for(int tick=0;sim.P1.Attack==attack&&tick<150;tick++) { Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1)); sim.Tick(default, default); }
            Assert.That(sim.P1.Attack,Is.Null);
            Assert.That(sim.P1.SpeedBuffRemaining, Is.EqualTo(1200)); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1.5f));
            SkillSystem.Buff(sim.P1, new BuffDefinition { Channel = BuffChannel.MovementBoost, DurationFrames = 300, MoveMultiplier = 1.5f, DashMultiplier = 1.5f });
            Assert.That(sim.P1.SpeedBuffRemaining, Is.EqualTo(1200));
            SkillSystem.Buff(sim.P1, new BuffDefinition { DurationFrames = 20, StartupMultiplier = .8f, MoveMultiplier = 1.1f });
            Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1.5f)); Assert.That(sim.P1.StartupMultiplier, Is.EqualTo(.8f));
            BattleTestFactory.Idle(sim, 20); Assert.That(sim.P1.StartupMultiplier, Is.EqualTo(1)); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1.5f));
            BattleTestFactory.Idle(sim, 1180); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1)); Assert.That(sim.P1.DashMultiplier, Is.EqualTo(1));
        }
        [Test]
        public void InterruptedChantDoesNotGrantTheSpeedBuff()
        {
            var sim = Create(8); sim.P1.Gauge = sim.Rules.Gauge.Maximum;
            sim.Tick(new FrameInput(pressed: InputButtons.Ultimate), default); sim.P1.Attack = null; BattleTestFactory.Idle(sim, 200);
            Assert.That(sim.P1.SpeedBuffRemaining, Is.Zero); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1));
        }
        [Test]
        public void RefinementMigrationPreservesSubsequentTuningAndAllNineStaminaSetsAreValid()
        {
            int[] capacity = {100,110,105,95,90,100,130,100,120};
            for (int i = 0; i < 9; i++)
            {
                var data = Setup.Character(i); Assert.That(data.RefinementSchemaVersion, Is.EqualTo(i==7?2:1));
                Assert.That(data.Movement.Stamina.Maximum, Is.EqualTo(capacity[i])); Assert.DoesNotThrow(() => data.ToDefinition());
            }
            var clone = UnityEngine.Object.Instantiate(Setup.Character(0));
            try
            {
                var movement = clone.Movement; movement.Stamina.Maximum = 117; clone.Movement = movement;
                RosterRefinementBuilder.Apply(new[] {clone}); Assert.That(clone.Movement.Stamina.Maximum, Is.EqualTo(117));
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }
        }
    }
}
