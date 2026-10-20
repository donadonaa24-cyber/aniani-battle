using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class AttackStaminaTests
    {
        [TestCase(1.7777778f)] [TestCase(1.6f)] [TestCase(1.3333333f)]
        public void MaximumLaunchLeavesTheCombatViewWhileOrdinarySubjectsStillTrackNormally(float aspect)
        {
            var stage=Setup.Stages[0].ToGeometry();var position=new BattleVector(28,3);
            Assert.That(CameraFraming.Subject(stage,position,false),Is.EqualTo(position));
            var subject=CameraFraming.Subject(stage,position,true);
            var frame=CameraFraming.Calculate(stage,new BattleVector(0,1.1f),subject,aspect,false,5.2f,1.2f,2,2.5f);
            Assert.That(position.X,Is.GreaterThan(frame.Center.X+frame.Size*aspect));
            Assert.That(stage.IsOutside(position),Is.False,"画面外と場外ラインを区別し復帰可能域に置く");
        }
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleRuleDefinition Rules=>AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void AllThreeNormalStagesPayTheirOwnTotalExactlyOnce(int index)
        {
            var sim=new BattleSimulation(Setup.Character(index).ToDefinition(),Setup.Character(index).ToDefinition(),Rules,Setup.Stages[0].ToGeometry());
            int expected=sim.P1.StaminaUnits;var s=sim.P1.Definition.Stamina;
            sim.Tick(new FrameInput(pressed:InputButtons.Attack),default);
            for(int chain=0;chain<3;chain++)
            {
                var attack=sim.P1.Attack;Assert.That(attack,Is.Not.Null);Assert.That(attack.ChainIndex,Is.EqualTo(chain));
                int cost=StaminaSystem.NormalCost(sim.P1,attack.Definition,chain,false,0,Rules.Charge);expected-=cost*60;
                for(int tick=0;!attack.IsActive&&tick<120;tick++)sim.Tick(default,default);
                Assert.That(sim.P1.StaminaUnits,Is.EqualTo(expected),"single total cost, no projectile double payment");
                if(chain<2)attack.NextQueued=true;
                for(int tick=0;sim.P1.Attack==attack&&tick<180;tick++)sim.Tick(default,default);
            }
            Assert.That(sim.P1.Stamina,Is.GreaterThan(0));Assert.That(sim.P1.Attack,Is.Null);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void EmptyStaminaBlocksNormalAndChargedReleaseButKeepsWalkingAndRecovery(int index)
        {
            var sim=BattleTestFactory.Create(false,Rules,Setup.Character(index).ToDefinition());sim.P1.StaminaUnits=0;sim.P1.StaminaDelayRemaining=1000;
            sim.Tick(new FrameInput(pressed:InputButtons.Attack),default);Assert.That(sim.P1.Attack,Is.Null);Assert.That(sim.P1.StaminaRejectedRemaining,Is.EqualTo(30));
            float x=sim.P1.Position.X;sim.Tick(new FrameInput(1),default);Assert.That(sim.P1.Position.X,Is.GreaterThan(x));
            sim.Tick(new FrameInput(held:InputButtons.Attack,pressed:InputButtons.Attack),default);
            for(int i=1;i<180;i++)sim.Tick(new FrameInput(held:InputButtons.Attack),default);
            sim.Tick(default,default);Assert.That(sim.P1.Attack,Is.Null);Assert.That(sim.P1.StaminaUnits,Is.Zero);
            sim.Tick(new FrameInput(y:1,pressed:InputButtons.Special),default);Assert.That(sim.P1.Attack,Is.Not.Null);
            Assert.That(sim.P1.Attack.Definition.Recovery.Style,Is.Not.EqualTo(RecoveryStyle.None));
        }
        [TestCase(0)] [TestCase(6)] [TestCase(8)]
        public void RecoveryWaitsUntilAttackEndsThenWalkingRecoversAtTheGroundRate(int index)
        {
            var f=BattleTestFactory.Create(false,Rules,Setup.Character(index).ToDefinition()).P1;var s=f.Definition.Stamina;
            f.StaminaUnits=0;f.Attack=new AttackInstance(1,f.Definition.GroundNormals[0],0,false);
            for(int i=0;i<180;i++)StaminaSystem.Step(f);Assert.That(f.StaminaUnits,Is.Zero);
            f.Attack=null;f.Mode=FighterMode.Move;f.Grounded=true;
            for(int i=0;i<s.RecoveryDelayFrames;i++)StaminaSystem.Step(f);Assert.That(f.StaminaUnits,Is.Zero);
            for(int i=0;i<60;i++)StaminaSystem.Step(f);Assert.That(f.StaminaUnits,Is.EqualTo(s.GroundRecoveryPerSecond*60));
            f.Grounded=false;for(int i=0;i<60;i++)StaminaSystem.Step(f);
            Assert.That(f.StaminaUnits,Is.EqualTo((s.GroundRecoveryPerSecond+s.ActiveRecoveryPerSecond)*60));
        }
        [TestCase(60)] [TestCase(120)] [TestCase(180)]
        public void ChargeCostIncreasesWithHeldTimeAndCannotDoubleChargeAProjectile(int frames)
        {
            var sim=BattleTestFactory.Create(false,Rules,Setup.Character(0).ToDefinition());var s=sim.P1.Definition.Stamina;
            sim.Tick(new FrameInput(held:InputButtons.Attack,pressed:InputButtons.Attack),default);
            for(int i=1;i<frames;i++)sim.Tick(new FrameInput(held:InputButtons.Attack),default);
            sim.Tick(default,default);var a=sim.P1.Attack;
            int cost=s.ChargeMinimumCost+(int)System.Math.Ceiling((s.ChargeMaximumCost-s.ChargeMinimumCost)*Rules.Charge.Strength(frames));
            for(int i=0;!a.IsActive&&i<120;i++)sim.Tick(default,default);
            Assert.That(sim.P1.StaminaUnits,Is.EqualTo((s.Maximum-cost)*60));Assert.That(a.ChargeFrames,Is.EqualTo(frames));
            Assert.That(sim.Projectiles.Count,Is.EqualTo(1));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void MaximumNormalChargeLaunchesOutsideTheFloorButLeavesControlAndARecoveryWindow(int index)
        {
            foreach(int facing in new[]{-1,1})
            {
                var sim=new BattleSimulation(Setup.Character(index).ToDefinition(),Setup.Character(index).ToDefinition(),Rules,Setup.Stages[0].ToGeometry());
                sim.P1.Facing=facing;sim.P2.Position=new BattleVector(0,sim.P2.Position.Y);
                var definition=sim.P1.Definition.GroundNormals[0];definition.HitstopFrames=0;
                var attack=new AttackInstance(91,definition,0,false){ChargeFrames=180};
                new DamageSystem().Apply(new HitContact(sim.P1,sim.P2,attack),new ComboTracker(),0,Rules,new List<BattleEvent>(),sim.Stage);
                BattleTestFactory.Idle(sim,Rules.Charge.MaximumHitstunFrames);
                Assert.That(sim.P2.Position.X*facing,Is.GreaterThan(sim.Stage[0].Bounds.Right+8));
                Assert.That(sim.Stage.IsOutside(sim.P2.Position),Is.False);Assert.That(sim.Match.IsFinished,Is.False);
                Assert.That(sim.P2.MaximumChargeLaunch,Is.True);
                Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));Assert.That(sim.P2.Hearts,Is.EqualTo(3));
                Assert.That(sim.P2.UpSpecialUsed,Is.False);Assert.That(sim.P2.JumpsUsed,Is.LessThan(sim.P2.Definition.MaxJumps));
                float previous=sim.P2.Position.X*facing;
                sim.Tick(default,new FrameInput(-facing,1,pressed:InputButtons.Special));
                Assert.That(sim.P2.Attack,Is.Not.Null);Assert.That(sim.P2.Attack.Definition.Recovery.Style,Is.Not.EqualTo(RecoveryStyle.None));
                Assert.That(sim.P2.Position.X*facing,Is.LessThanOrEqualTo(previous+.001f));
            }
        }
    }
}
