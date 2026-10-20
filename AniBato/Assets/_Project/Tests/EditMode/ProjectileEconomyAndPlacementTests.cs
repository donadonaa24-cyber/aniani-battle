using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class ProjectileEconomyAndPlacementTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [TestCase(0,false,6)] [TestCase(0,true,18)] [TestCase(2,true,18)] [TestCase(6,true,18)] [TestCase(7,true,18)]
        public void AReleasedShotOrThreeMemberFormationPaysOnlyOnce(int index,bool special,int cost)
        {
            var sim=BattleTestFactory.Create(false,definition:Setup.Character(index).ToDefinition());
            var a=special?sim.P1.Definition.Specials[0]:sim.P1.Definition.GroundNormals[2];
            a.StartupFrames=0;a.DamageDelayFrames=0;var attack=new AttackInstance(1,a,0,false);sim.P1.Attack=attack;
            var projectiles=new ProjectileSystem();int before=sim.P1.StaminaUnits;
            projectiles.Step(sim.P1,sim.P2,3,sim.Stage);
            Assert.That(projectiles.Projectiles.Count,Is.EqualTo(index==6?3:1));
            Assert.That(sim.P1.StaminaUnits,Is.EqualTo(before-cost*60));
            projectiles.Step(sim.P1,sim.P2,3,sim.Stage);Assert.That(sim.P1.StaminaUnits,Is.EqualTo(before-cost*60));
            Assert.That(a.RecoveryFrames,Is.GreaterThanOrEqualTo(12));
        }
        [Test]
        public void InsufficientStaminaDoesNotFireOrDeleteTheExistingDrone()
        {
            var sim=BattleTestFactory.Create(false,definition:Setup.Character(0).ToDefinition());
            var a=sim.P1.Definition.Specials[0];a.StartupFrames=0;
            var projectiles=new ProjectileSystem();sim.P1.Attack=new AttackInstance(1,a,0,false);projectiles.Step(sim.P1,sim.P2,3,sim.Stage);
            var original=projectiles.Projectiles.Single();sim.P1.StaminaUnits=17*60;sim.P1.Attack=new AttackInstance(2,a,0,false);
            projectiles.Step(sim.P1,sim.P2,3,sim.Stage);
            Assert.That(projectiles.Projectiles.Single(),Is.SameAs(original));Assert.That(sim.P1.StaminaUnits,Is.EqualTo(17*60));
            Assert.That(sim.P1.StaminaRejectedRemaining,Is.EqualTo(30));
        }
        [Test]
        public void InterruptedStartupAndUltimateOrGroundPlacementDoNotPayProjectileStamina()
        {
            var sim=BattleTestFactory.Create(false,definition:Setup.Character(0).ToDefinition());
            var system=new ProjectileSystem();int before=sim.P1.StaminaUnits;
            sim.P1.Attack=new AttackInstance(1,sim.P1.Definition.Specials[0],0,false);system.Step(sim.P1,sim.P2,3,sim.Stage);
            sim.P1.Attack=null;Assert.That(system.Projectiles,Is.Empty);Assert.That(sim.P1.StaminaUnits,Is.EqualTo(before));
            foreach(int index in new[]{0,3,7})
            {
                var f=Setup.Character(index).ToDefinition();var attack=index==0?f.Ultimate:f.Specials[3];
                Assert.That(StaminaSystem.ProjectileCost(sim.P1,attack),Is.Zero);
            }
        }
        [TestCase(3)] [TestCase(7)]
        public void PlacementCommitsForOneSecondThenEighteenTicksAndDamageInterruptsIt(int index)
        {
            var rules=PhaseOneDefaults.Rules();rules.CombatInput=CombatInputDefinition.Default;rules.CombatInput.DownSpecialEnabled=true;
            var sim=BattleTestFactory.Create(false,rules,Setup.Character(index).ToDefinition());
            Assert.That(sim.P1.Definition.Special(SpecialSlot.Down).CooldownFrames,Is.EqualTo(300));
            Assert.That(sim.P1.Definition.Special(SpecialSlot.Down).Ability.PlacementLimit,Is.EqualTo(2));
            sim.Tick(new FrameInput(y:-1,pressed:InputButtons.Special),default);var origin=sim.P1.Position;
            for(int i=0;i<59;i++)
            { sim.Tick(new FrameInput(1),default);Assert.That(sim.P1.Position,Is.EqualTo(origin));Assert.That(sim.Projectiles,Is.Empty); }
            sim.Tick(new FrameInput(1),default);Assert.That(sim.Projectiles.Count,Is.EqualTo(1));Assert.That(sim.P1.Position,Is.EqualTo(origin));
            for(int i=0;i<18;i++){sim.Tick(new FrameInput(1),default);Assert.That(sim.P1.Position,Is.EqualTo(origin));}
            sim.Tick(new FrameInput(1),default);Assert.That(sim.P1.Position.X,Is.GreaterThan(origin.X));
            var interrupted=BattleTestFactory.Create(false,rules,Setup.Character(index).ToDefinition());
            interrupted.Tick(new FrameInput(y:-1,pressed:InputButtons.Special),default);BattleTestFactory.Idle(interrupted,30);
            new DamageSystem().Apply(new HitContact(interrupted.P2,interrupted.P1,new AttackInstance(99,PhaseOneDefaults.Normal(),0,false)),
                new ComboTracker(),0,rules,new List<BattleEvent>());
            BattleTestFactory.Idle(interrupted,80);Assert.That(interrupted.Projectiles,Is.Empty);Assert.That(interrupted.P1.Attack,Is.Null);
        }
    }
}
