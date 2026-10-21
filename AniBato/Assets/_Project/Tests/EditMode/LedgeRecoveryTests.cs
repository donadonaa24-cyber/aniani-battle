using System.Collections.Generic;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class LedgeRecoveryTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [Test]
        public void ProductRulesAndEveryNativeLedgeSpriteUseTheNewRecoveryContract()
        {
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            Assert.That(rules.Ledge.Enabled,Is.True);Assert.That(rules.Ledge.MaximumHoldFrames,Is.EqualTo(300));
            Assert.That(rules.Ledge.StaminaPerSecond,Is.EqualTo(25));
            var manifest=JsonUtility.FromJson<ArtManifest>(File.ReadAllText("../art_source/paper_ledge/adoption.json"));
            Assert.That(manifest.schemaVersion,Is.EqualTo(3));Assert.That(manifest.frames.Length,Is.EqualTo(9));
            Assert.That(manifest.userShapeConfirmed,Is.True);Assert.That(manifest.handPattern,Is.EqualTo("A"));Assert.That(manifest.thumbSide,Is.EqualTo("image-left"));
            foreach(var frame in manifest.frames)
            {
                Assert.That(frame.grippingHands,Is.EqualTo(1));Assert.That(frame.pose,Is.EqualTo("one_hand_hang"));
                Assert.That(frame.grippingHand,Is.EqualTo("left"));
                var data=Setup.Character(frame.index);var sprite=data.PaperMotion.LedgeHang;
                Assert.That(sprite,Is.Not.Null);Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));
                Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001));
                Assert.That(sprite.pivot.x,Is.EqualTo(frame.handX).Within(.001));
                Assert.That(sprite.pivot.y,Is.EqualTo(1450-frame.handY).Within(.001));
                Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)),Is.EqualTo(File.ReadAllBytes("../"+frame.prepared)));
                Assert.That(data.PaperMotion.Find(Presentation.AnimationState.LedgeHang).Frames.Single(),Is.SameAs(sprite));
                Assert.That(data.DirectionalNormals.Single(a=>a&&a.Definition.NormalDirection==NormalDirection.Crouch).Definition.CanHitLedge,Is.True);
                Assert.That(data.GroundNormals.All(a=>!a.Definition.CanHitLedge),Is.True);
                Assert.That(data.AirNormals.All(a=>!a.Definition.CanHitLedge),Is.True);
            }
        }
        [System.Serializable] private sealed class ArtManifest{public int schemaVersion;public bool userShapeConfirmed;public string handPattern,thumbSide;public ArtFrame[] frames;}
        [System.Serializable] private sealed class ArtFrame{public int index,handX,handY,grippingHands;public string prepared,pose,grippingHand;}
        private static BattleSimulation Create(int index=0)
        {
            var rules=PhaseOneDefaults.Rules(); rules.Ledge=LedgeDefinition.Default;
            rules.CombatInput=CombatInputDefinition.Default;rules.CombatInput.UpRecoveryEnabled=true;
            return new BattleSimulation(Setup.Character(index).ToDefinition(),Setup.Character(0).ToDefinition(),rules,Setup.Stages[0].ToGeometry());
        }
        private static void Near(BattleSimulation sim,FighterState f,int side,float head=-.1f)
        {
            var floor=sim.Stage[0].Bounds;
            f.Position=new BattleVector((side<0?floor.Left:floor.Right)+side*(f.Definition.HurtboxSize.X*.5f+.1f),floor.Top-f.Definition.HurtboxSize.Y*.5f+head);
            f.Grounded=false;f.GroundPlatform=-1;f.Mode=FighterMode.Fall;f.Velocity=default;
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void BothPlayersAtBothEdgesCatchAndExitByJumpOrInwardInput(int index)
        {
            foreach(int side in new[]{-1,1})foreach(int player in new[]{0,1})foreach(bool jump in new[]{false,true})
            {
                var sim=Create(index);var f=player==0?sim.P1:sim.P2;Near(sim,f,side);f.MaximumChargeLaunch=true;sim.Tick(default,default);
                Assert.That(f.Mode,Is.EqualTo(FighterMode.LedgeHang));Assert.That(f.Facing,Is.EqualTo(-side));
                Assert.That(f.MaximumChargeLaunch,Is.False,"つかまり成功でカメラの打ち出し表示を終了する");
                f.JumpsUsed=f.Definition.MaxJumps;f.UpSpecialUsed=true;
                var input=jump?new FrameInput(pressed:InputButtons.Jump):new FrameInput(-side,disableDoubleTapDash:true);
                sim.Tick(player==0?input:default,player==1?input:default);
                Assert.That(f.UpSpecialUsed,Is.True,"つかまるだけでは上復帰を補充しない");
                for(int tick=0;tick<100&&!f.Grounded;tick++)sim.Tick(default,default);
                Assert.That(f.Grounded,Is.True,index+" side="+side+" jump="+jump+" at "+f.Position.X+","+f.Position.Y);
                Assert.That(f.Position.X,Is.InRange(sim.Stage[0].Bounds.Left,sim.Stage[0].Bounds.Right));
                Assert.That(f.UpSpecialUsed,Is.False);Assert.That(f.LedgeRegrabBlocked,Is.False);
            }
        }
        [Test]
        public void HangingDrainsExactlyTwentyFivePerSecondAndReleasesAtExhaustionWithAFiveSecondCeiling()
        {
            var sim=Create();Near(sim,sim.P1,-1);sim.Tick(default,default);int initial=sim.P1.StaminaUnits;
            BattleTestFactory.Idle(sim,60);Assert.That(sim.P1.StaminaUnits,Is.EqualTo(initial-25*60));
            Assert.That(LedgeSystem.HoldFramesLeft(sim.P1,sim.Rules.Ledge),Is.EqualTo(180));
            Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.LedgeHang));BattleTestFactory.Idle(sim,60);
            Assert.That(sim.P1.StaminaUnits,Is.EqualTo(initial-50*60));Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.LedgeHang));
            BattleTestFactory.Idle(sim,120);
            Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.Fall));Assert.That(sim.P1.StaminaUnits,Is.Zero);
            Assert.That(sim.P1.LedgeRegrabBlocked,Is.True);
            Near(sim,sim.P1,-1);sim.Tick(default,default);Assert.That(sim.P1.Mode,Is.Not.EqualTo(FighterMode.LedgeHang));
            var rules=sim.Rules;var definition=sim.P1.Definition;definition.Stamina=default;
            sim=BattleTestFactory.Create(rules:rules,definition:definition);Near(sim,sim.P1,-1);sim.Tick(default,default);
            BattleTestFactory.Idle(sim,299);Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.LedgeHang));
            sim.Tick(default,default);Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.Fall));
        }
        [Test]
        public void EmptyStaminaDownInputAndHitstopHaveDistinctOutcomes()
        {
            var sim=Create();Near(sim,sim.P1,1);sim.P1.StaminaUnits=0;sim.P1.StaminaDelayRemaining=60;
            sim.Tick(default,default);Assert.That(sim.P1.Mode,Is.Not.EqualTo(FighterMode.LedgeHang));
            sim=Create();Near(sim,sim.P1,1);sim.Tick(new FrameInput(y:-1),default);Assert.That(sim.P1.Mode,Is.Not.EqualTo(FighterMode.LedgeHang));
            sim=Create();Near(sim,sim.P1,1);sim.Tick(default,default);var f=sim.P1;int stamina=f.StaminaUnits;var position=f.Position;
            f.HitstopRemaining=6;BattleTestFactory.Idle(sim,6);
            Assert.That(f.StaminaUnits,Is.EqualTo(stamina));Assert.That(f.LedgeAge,Is.Zero);Assert.That(f.Position,Is.EqualTo(position));
            f.StaminaUnits=25;sim.Tick(default,default);Assert.That(f.Mode,Is.EqualTo(FighterMode.Fall));
        }
        [Test]
        public void OrdinaryMeleeAndProjectilesDoNotConsumeTheirHitOnAHangingTargetButDownwardAttacksDo()
        {
            foreach(bool projectile in new[]{false,true})
            {
                var sim=Create();Near(sim,sim.P2,-1);sim.Tick(default,default);
                var definition=PhaseOneDefaults.Normal();definition.Hitbox=new BattleRect(default,new BattleVector(10,10));definition.StartupFrames=0;
                definition.SpawnProjectile=projectile;definition.ProjectileLifetimeFrames=60;definition.ProjectileSpeed=1;
                sim.P1.Position=sim.P2.Position;var attack=new AttackInstance(999,definition,0,false);
                sim.P1.Attack=attack;var contacts=new List<HitContact>();var resolver=new HitResolver();
                if(projectile)resolver.Gather(new Projectile(sim.P1,attack),sim.P2,contacts);else resolver.Gather(sim.P1,sim.P2,contacts);
                Assert.That(contacts.Count,Is.Zero);Assert.That(attack.HasHit(1),Is.False);
                definition.CanHitLedge=true;attack=new AttackInstance(1000,definition,0,false);sim.P1.Attack=attack;
                if(projectile)resolver.Gather(new Projectile(sim.P1,attack),sim.P2,contacts);else resolver.Gather(sim.P1,sim.P2,contacts);
                Assert.That(contacts.Count,Is.EqualTo(1));
                new DamageSystem().Apply(contacts[0],new ComboTracker(),0,sim.Rules,new List<BattleEvent>(),sim.Stage);
                Assert.That(sim.P2.Mode,Is.Not.EqualTo(FighterMode.LedgeHang));Assert.That(sim.P2.LedgeRegrabBlocked,Is.True);
                Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));
            }
        }
        [Test]
        public void UpperPlatformsHeightAndAttackBufferCannotCreateExtraGrabsOrEscapingAttacks()
        {
            var sim=Create();var upper=sim.Stage[1].Bounds;var f=sim.P1;
            f.Position=new BattleVector(upper.Left-.7f,upper.Top-f.Definition.HurtboxSize.Y*.5f);f.Grounded=false;f.Mode=FighterMode.Fall;
            sim.Tick(default,default);Assert.That(f.Mode,Is.Not.EqualTo(FighterMode.LedgeHang));
            Near(sim,f,-1,2);sim.Tick(default,default);Assert.That(f.Mode,Is.Not.EqualTo(FighterMode.LedgeHang),"走り落ちた瞬間は吸着しない");
            Near(sim,f,-1);sim.Tick(default,default);
            sim.Tick(new FrameInput(held:InputButtons.Attack,pressed:InputButtons.Attack|InputButtons.Special|InputButtons.Guard),default);
            Assert.That(f.Mode,Is.EqualTo(FighterMode.LedgeHang));Assert.That(f.Attack,Is.Null);
            sim.Tick(new FrameInput(1),default);BattleTestFactory.Idle(sim,25);
            Assert.That(f.Attack,Is.Null);Assert.That(f.Mode,Is.Not.EqualTo(FighterMode.Charging));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void DeepVerticalAirJumpThenInwardRecoveryStillReachTheStageForEveryFighter(int index)
        {
            foreach(int side in new[]{-1,1})
            {
                var sim=Create(index);var f=sim.P1;var floor=sim.Stage[0].Bounds;
                f.Position=new BattleVector((side<0?floor.Left:floor.Right)+side*2.2f,floor.Top-5);
                f.Grounded=false;f.GroundPlatform=-1;f.JumpsUsed=1;f.Mode=FighterMode.Fall;
                // Rise beside the solid stage before steering in, rather than jumping into its underside.
                sim.Tick(new FrameInput(pressed:InputButtons.Jump,disableDoubleTapDash:true),default);
                for(int tick=0;tick<20;tick++)sim.Tick(default,default);
                sim.Tick(new FrameInput(-side,1,pressed:InputButtons.Special,disableDoubleTapDash:true),default);
                for(int tick=0;tick<240&&!f.Grounded&&!sim.Match.IsFinished;tick++)sim.Tick(new FrameInput(-side,1,disableDoubleTapDash:true),default);
                Assert.That(f.Grounded,Is.True,index+" side="+side+" "+f.Position.X+","+f.Position.Y+" "+f.Mode);
                Assert.That(sim.Match.IsFinished,Is.False);
            }
        }
        [Test]
        public void DisappearingMainStageReleasesTheHangAndNewRoundsResetIt()
        {
            var sim=Create();Near(sim,sim.P1,-1);sim.Tick(default,default);sim.Stage.DeactivatePlatform(0);
            sim.Tick(default,default);Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.Fall));
            var fresh=Create();Assert.That(fresh.P1.LedgeRegrabBlocked,Is.False);Assert.That(fresh.P1.LedgeSide,Is.Zero);
        }
    }
}
