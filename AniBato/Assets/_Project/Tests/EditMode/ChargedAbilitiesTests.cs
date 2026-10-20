using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class ChargedAbilitiesTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleSimulation Create(int index)
        {
            var rules = PhaseOneDefaults.Rules(); rules.EnableRingOut = false;
            rules.CombatInput = CombatInputDefinition.Default; rules.CombatInput.DownSpecialEnabled = rules.CombatInput.UpRecoveryEnabled = true;
            var c = Setup.Character(index).ToDefinition(); c.UltimateCutInFrames=0;
            var sim = BattleTestFactory.Create(false,rules,c); sim.P1.Gauge=rules.Gauge.Maximum;
            sim.P1.Position=new BattleVector(0,100); sim.P1.Grounded=false; sim.P1.GroundPlatform=-1;
            sim.P2.Position=new BattleVector(-20,100); sim.P2.Grounded=false; sim.P2.GroundPlatform=-1;
            return sim;
        }
        private static void Start(BattleSimulation sim, bool up=false, bool special=false) => sim.Tick(
            new FrameInput(y:up?1:0,held:special||up?InputButtons.Special:InputButtons.Ultimate,
                pressed:special||up?InputButtons.Special:InputButtons.Ultimate),default);
        [TestCase(3,30,7.5f)] [TestCase(4,30,7.5f)]
        [TestCase(3,45,9.75f)] [TestCase(4,45,9.75f)] [TestCase(3,60,12f)] [TestCase(4,60,12f)]
        public void TeleportWaitsAtLeastHalfASecondAndLongerHoldingTravelsFarther(int index,int frames,float distance)
        {
            var sim=Create(index); Start(sim,true); var start=sim.P1.Position;
            for(int i=1;i<frames;i++)
            {
                sim.Tick(new FrameInput(1,0,held:frames==30?InputButtons.None:InputButtons.Special),default);
                Assert.That(sim.P1.Position,Is.EqualTo(start)); Assert.That(sim.P1.Attack.IsActive,Is.False);
            }
            sim.Tick(new FrameInput(1,0,held:frames==60?InputButtons.Special:InputButtons.None),default);
            Assert.That(sim.P1.Attack.RecoveryStarted,Is.True); Assert.That(sim.P1.Attack.RecoveryChargeFrames,Is.EqualTo(frames));
            Assert.That(sim.P1.Position.X-start.X,Is.EqualTo(distance).Within(.001));
            Assert.That(sim.P1.Attack.RecoveryOrigin,Is.EqualTo(start)); Assert.That(sim.P1.UpSpecialUsed,Is.True);
        }
        [TestCase(3)] [TestCase(4)]
        public void FrozenChargeDoesNotAdvanceAndTakingDamageCancelsTheTransfer(int index)
        {
            var sim=Create(index); Start(sim,true); BattleTestFactory.Idle(sim,20); sim.P1.HitstopRemaining=5;
            int age=sim.P1.Attack.RecoveryChargeFrames; var position=sim.P1.Position;
            BattleTestFactory.Idle(sim,5); Assert.That(sim.P1.Attack.RecoveryChargeFrames,Is.EqualTo(age));
            var hit=new AttackInstance(99,PhaseOneDefaults.Normal(),0,true);
            new DamageSystem().Apply(new HitContact(sim.P2,sim.P1,hit),new ComboTracker(),0,sim.Rules,new List<BattleEvent>());
            Assert.That(sim.P1.Attack,Is.Null); Assert.That(sim.P1.Position,Is.EqualTo(position)); Assert.That(sim.P1.UpSpecialUsed,Is.True);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void SlowerRecoveryUsesOneAndAHalfTimesTheTravelTicksWithTheSameMotionClock(int index)
        {
            var actual=Setup.Character(index).ToDefinition().Special(SpecialSlot.Up);
            var original=AniBato.EditorTools.DirectionalRecoveryBuilder.Up(index);
            Assert.That(actual.ActiveFrames,Is.EqualTo(original.ActiveFrames*1.5f));
            Assert.That(actual.Recovery.MotionTimeScale,Is.EqualTo(2f/3));
            Assert.That(actual.ActiveFrames*actual.Recovery.MotionTimeScale,Is.EqualTo(original.ActiveFrames).Within(.001));
            Assert.That(actual.Recovery.Speed,Is.EqualTo(original.Recovery.Speed));
            Assert.That(actual.Recovery.HorizontalSpeed,Is.EqualTo(original.Recovery.HorizontalSpeed));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void ChargeHoldsThePreparationAndAutomaticallyReleasesAtItsOwnMaximum(int index)
        {
            var sim=Create(index); bool special=index==8; Start(sim,special:special); var attack=sim.P1.Attack;
            var charge=attack.Definition.AbilityCharge; Assert.That(charge.Enabled,Is.True);
            var position=sim.P1.Position;
            for(int frame=1;frame<charge.MaximumFrames;frame++)
            {
                sim.Tick(new FrameInput(held:special?InputButtons.Special:InputButtons.Ultimate),default);
                Assert.That(attack.IsActive,Is.False); Assert.That(sim.P1.Position,Is.EqualTo(position));
                Assert.That(sim.Projectiles,Is.Empty);
            }
            sim.Tick(new FrameInput(held:special?InputButtons.Special:InputButtons.Ultimate),default);
            Assert.That(attack.AbilityChargeReleased,Is.True); Assert.That(attack.IsActive,Is.True);
            Assert.That(attack.AbilityChargeFrames,Is.EqualTo(charge.MaximumFrames));
            Assert.That(sim.P1.UltimateUses,Is.EqualTo(special?0:1));
        }
        [Test]
        public void FullyChargedAlmaMatchesTwoThirdsOfTheMainFloorAndDamagesItsOuterEdge()
        {
            var sim=Create(4); Start(sim); var attack=sim.P1.Attack;
            for(int i=0;i<119;i++)sim.Tick(new FrameInput(held:InputButtons.Ultimate),default);
            float width=sim.Stage[0].Bounds.Size.X*2f/3;
            sim.P2.Position=new BattleVector(width*.5f,100); sim.P2.Velocity=default;
            sim.Tick(new FrameInput(held:InputButtons.Ultimate),default);
            Assert.That(attack.Definition.Hitbox.Size.X,Is.EqualTo(width).Within(.001)); Assert.That(sim.P2.Hearts,Is.EqualTo(1));
            sim.P2.Position=new BattleVector(-width*.5f,100); BattleTestFactory.Idle(sim,20); Assert.That(sim.P2.Hearts,Is.EqualTo(1));
        }
        [Test]
        public void BatChargeBoostsKnockbackWhileCurryAndBlackHoleExpandTheirSecondaryEffects()
        {
            foreach(int index in new[]{2,3,8})
            {
                var sim=Create(index); Start(sim,special:index==8); var attack=sim.P1.Attack; var before=attack.Definition;
                for(int i=0;i<before.AbilityCharge.MaximumFrames;i++) sim.Tick(new FrameInput(held:index==8?InputButtons.Special:InputButtons.Ultimate),default);
                if(index==2)Assert.That(attack.Definition.Aftermath.Size.X,Is.EqualTo(before.Aftermath.Size.X*2.5f));
                if(index==3)Assert.That(attack.Definition.Ability.ProjectilePullRadius,Is.EqualTo(before.Ability.ProjectilePullRadius*2));
                if(index==8)Assert.That(attack.Definition.Knockback.X,Is.EqualTo(before.Knockback.X*3.5f));
            }
        }
        [Test]
        public void ChargedRocketExpandsItsActualBlastAtGroundContactWithoutLosingTwoHeartsTwice()
        {
            var sim=Create(0);sim.P2.Position=new BattleVector(8,1.1f);sim.P2.Grounded=true;sim.P2.GroundPlatform=0;
            Start(sim);for(int i=0;i<120;i++)sim.Tick(new FrameInput(held:InputButtons.Ultimate),default);
            var shot=sim.Projectiles.Single();Assert.That(shot.Exploded,Is.False);
            Assert.That(shot.Hitbox.Size.X,Is.EqualTo(Setup.Character(0).Ultimate.Definition.Hitbox.Size.X));
            int wait=0;while(!shot.Exploded&&wait++<100)sim.Tick(default,default);
            Assert.That(wait,Is.LessThan(100));Assert.That(shot.Hitbox.Size,Is.EqualTo(new BattleVector(15,8.75f)));
            Assert.That(shot.Hitbox.Bottom,Is.EqualTo(sim.Stage[0].Bounds.Top).Within(.001));
            Assert.That(sim.P2.Hearts,Is.EqualTo(1));BattleTestFactory.Idle(sim,20);Assert.That(sim.P2.Hearts,Is.EqualTo(1));
        }
        [Test]
        public void CooldownModelShowsIndependentSlotsLandingAndPlacementRestrictions()
        {
            var sim=Create(7); sim.P1.SpecialCooldowns[0]=70;sim.P1.SpecialCooldowns[2]=12;sim.P1.UpSpecialUsed=true;sim.P1.PlacedCount=2;
            Assert.That(SpecialCooldownDisplay.State(sim.P1,SpecialSlot.Neutral),Is.EqualTo("1.2秒"));
            Assert.That(SpecialCooldownDisplay.State(sim.P1,SpecialSlot.Up),Does.Contain("0.2秒").And.Contain("要着地"));
            Assert.That(SpecialCooldownDisplay.State(sim.P1,SpecialSlot.Down),Does.Contain("設置上限"));
            Assert.That(SpecialCooldownDisplay.Ready(sim.P1,SpecialSlot.Down),Is.False);
        }
    }
}
