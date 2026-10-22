using System;
using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class SupportItemTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleSimulation World()
        {
            var rules=PhaseOneDefaults.Rules();rules.Support=SupportDefinition.Default;rules.Ledge=LedgeDefinition.Default;
            return new BattleSimulation(Setup.Character(2).ToDefinition(),Setup.Character(1).ToDefinition(),rules,Setup.Stages[0].ToGeometry());
        }
        private sealed class Rig
        {
            public readonly BattleSimulation Sim=World();
            public readonly ProjectileSystem Shots=new ProjectileSystem();
            public readonly List<BattleEvent> Events=new List<BattleEvent>();
            public readonly SupportSystem System;
            public readonly SupportDefinition Rules;
            private int tick;
            public Rig(uint seed=1,bool fast=true)
            {
                Rules=SupportDefinition.Default;
                if(fast){Rules.FirstMinimumFrames=Rules.FirstMaximumFrames=1;Rules.IntervalMinimumFrames=Rules.IntervalMaximumFrames=1;}
                Rules.Seed=seed;System=new SupportSystem(Rules);
                Sim.P1.Position=new BattleVector(-25,10);Sim.P2.Position=new BattleVector(25,10);
            }
            public void Step(int frames=1)
            {for(int i=0;i<frames;i++)System.Step(Sim.P1,Sim.P2,Sim.Stage,Sim.Rules,++tick,Shots,Events);}
            public FighterState Collect(int player=0)
            {
                while(System.Pickup==null)Step();var f=player==0?Sim.P1:Sim.P2;
                f.Position=System.Pickup.Position;Step();
                Assert.That(Events.Any(e=>e.Kind==BattleEventKind.SupportCollected),Is.True);return f;
            }
        }
        private static Rig With(SupportKind kind)
        {
            for(uint seed=1;seed<500;seed++){var r=new Rig(seed);r.Step();if(r.System.Pickup.Kind==kind)return r;}
            throw new Exception("Support draw coverage missing");
        }
        [Test]
        public void ProductHasAllApprovedRulesAndFourteenNativeArtworkEntries()
        {
            var d=AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            Assert.That(d.SchemaVersion,Is.EqualTo(15));Assert.That(d.Support.Enabled,Is.True);
            Assert.That(d.Support.FirstMinimumFrames,Is.EqualTo(480));Assert.That(d.Support.FirstMaximumFrames,Is.EqualTo(720));
            Assert.That(d.Support.IntervalMinimumFrames,Is.EqualTo(600));Assert.That(d.Support.IntervalMaximumFrames,Is.EqualTo(960));
            Assert.That(d.Support.ReuseFrames,Is.EqualTo(600));Assert.That(d.Support.EffectFrames,Is.EqualTo(300));
            Assert.That(d.Support.SlowMultiplier,Is.EqualTo(.6f));Assert.That(d.Support.PawnCount,Is.EqualTo(6));
            Assert.That(d.Support.PawnStaminaDamage,Is.EqualTo(15));Assert.That(d.Support.CarParalysisFrames,Is.EqualTo(120));
            Assert.That(Setup.SupportArt.Entries.Length,Is.EqualTo(14));Assert.That(Setup.SupportArt.FieldShader,Is.Not.Null);
            foreach(var e in Setup.SupportArt.Entries){Assert.That(e.Sprite,Is.Not.Null);Assert.That(e.VisibleSize.x,Is.GreaterThan(0));Assert.That(e.VisibleSize.y,Is.GreaterThan(0));}
        }
        [Test]
        public void BuildCopyKeepsEverySupportSpriteMeasurementAndDoesNotMutateTheSource()
        {
            string original=UnityEngine.JsonUtility.ToJson(Setup.SupportArt);
            try
            {
                var copy=AniBato.EditorTools.WebGLScenePacking.CopyForBuild(Setup).SupportArt;
                Assert.That(copy,Is.Not.SameAs(Setup.SupportArt));Assert.That(copy.Entries.Length,Is.EqualTo(14));
                for(int i=0;i<14;i++){Assert.That(copy.Entries[i].Sprite,Is.SameAs(Setup.SupportArt.Entries[i].Sprite));Assert.That(copy.Entries[i].VisibleSize,Is.EqualTo(Setup.SupportArt.Entries[i].VisibleSize));}
                Assert.That(copy.FieldShader,Is.SameAs(Setup.SupportArt.FieldShader));
                Assert.That(UnityEngine.JsonUtility.ToJson(Setup.SupportArt),Is.EqualTo(original));
            }
            finally{AniBato.EditorTools.WebGLScenePacking.ReleaseCopies();}
        }
        [Test]
        public void BarrierAlsoAbsorbsDamageFromTheOpponentsSuccessfulCounter()
        {
            var sim=World();var f=sim.P1;f.SupportBarrierRemaining=300;f.SupportBarrierUnits=3;
            var contact=new HitContact(f,sim.P2,new AttackInstance(1,f.Definition.GroundNormals[0],0,false));
            new DefenseSystem().Apply(contact,DefenseOutcome.Counter,new ComboTracker(),new DamageSystem(),1,sim.Rules,new List<BattleEvent>());
            Assert.That(f.NormalHitCount,Is.Zero);Assert.That(f.SupportBarrierUnits,Is.EqualTo(2));Assert.That(f.Hearts,Is.EqualTo(3));
        }
        [Test]
        public void BagHasNoDuplicateUntilAllEightAndNeverRepeatsAcrossBags()
        {
            var random=new SupportRandom(321);SupportKind? last=null;
            for(int bag=0;bag<50;bag++)
            {var set=new HashSet<SupportKind>();for(int i=0;i<8;i++){var k=random.Draw();if(i==0)Assert.That(k,Is.Not.EqualTo(last));set.Add(k);last=k;}Assert.That(set.Count,Is.EqualTo(8));}
        }
        [Test]
        public void SpawnWindowAndPerPlayerCooldownLeaveTheItemForTheOpponent()
        {
            var r=new Rig(fast:false);Assert.That(r.System.SpawnCountdown,Is.InRange(480,720));
            r.Step(r.System.SpawnCountdown-1);Assert.That(r.System.Pickup,Is.Null);r.Step();Assert.That(r.System.Pickup,Is.Not.Null);
            var f=r.Collect();Assert.That(f.SupportReuseRemaining,Is.EqualTo(600));Assert.That(r.System.SpawnCountdown,Is.InRange(600,960));
            var fast=new Rig();fast.Step();fast.Sim.P1.SupportReuseRemaining=100;fast.Sim.P1.Position=fast.System.Pickup.Position;
            fast.Step();Assert.That(fast.System.Pickup,Is.Not.Null);Assert.That(fast.System.Actors,Is.Empty);
            fast.Sim.P2.Position=fast.System.Pickup.Position;fast.Step();Assert.That(fast.System.Pickup,Is.Null);Assert.That(fast.System.Actors.Single().Owner,Is.SameAs(fast.Sim.P2));
        }
        [TestCase(0)] [TestCase(1)]
        public void EightEffectsApplyToTheCorrectOwnerAndNeverChangeTheirAttack(int player)
        {
            foreach(SupportKind kind in Enum.GetValues(typeof(SupportKind)))
            {
                var r=With(kind);var owner=player==0?r.Sim.P1:r.Sim.P2;var enemy=player==0?r.Sim.P2:r.Sim.P1;
                owner.Hearts=2;owner.StaminaUnits=20*60;
                var attack=new AttackInstance(981,owner.Definition.GroundNormals[0],0,false);owner.Attack=attack;
                r.Collect(player);Assert.That(owner.Attack,Is.SameAs(attack));Assert.That(owner.SupportReuseRemaining,Is.EqualTo(600));
                Assert.That(enemy.SupportReuseRemaining,Is.Zero);
                if(kind==SupportKind.Alma){Assert.That(enemy.MoveMultiplier,Is.EqualTo(.6f));Assert.That(enemy.DashMultiplier,Is.EqualTo(.6f));Assert.That(enemy.StartupMultiplier,Is.EqualTo(1));Assert.That(owner.SupportSlowRemaining,Is.Zero);}
                if(kind==SupportKind.SakiHana){Assert.That(owner.SupportInfiniteRemaining,Is.EqualTo(300));Assert.That(enemy.SupportInfiniteRemaining,Is.Zero);}
                if(kind==SupportKind.Sara)Assert.That(owner.Hearts,Is.EqualTo(3));
                if(kind==SupportKind.Mio){Assert.That(owner.SupportBarrierUnits,Is.EqualTo(3));Assert.That(enemy.SupportBarrierUnits,Is.Zero);}
                r.Step(r.Rules.PortalFrames);
                if(kind==SupportKind.ShionCar){var shot=r.Shots.Projectiles.Single();Assert.That(shot.Attack.Definition.ParalysisFrames,Is.EqualTo(120));Assert.That(shot.Owner,Is.SameAs(owner));}
                if(kind==SupportKind.Mai){Assert.That(r.Shots.Projectiles.Single().Attack.Definition.Kind,Is.EqualTo(AttackKind.Special));}
                if(kind==SupportKind.Tsuyoshi)Assert.That(r.System.Wallet,Is.Not.Null);
                if(kind==SupportKind.Hota){r.Step(r.Rules.PawnCount*r.Rules.PawnIntervalFrames);Assert.That(r.Shots.Projectiles.Count,Is.EqualTo(6));Assert.That(r.Shots.Projectiles.All(p=>p.Attack.Definition.StaminaDamage==15&&p.Attack.Definition.CanHitLedge),Is.True);}
                Assert.That(owner.StaminaUnits,Is.EqualTo(20*60),"Summoning charges no projectile stamina");
            }
        }
        [Test]
        public void StatusExpiryAndGlobalStopsConsumeExactlyTheApprovedTime()
        {
            var r=With(SupportKind.Alma);r.Collect();var enemy=r.Sim.P2;
            enemy.FrozenThisTick=true;r.Step(80);Assert.That(enemy.SupportSlowRemaining,Is.EqualTo(300));
            enemy.FrozenThisTick=false;r.Step(299);Assert.That(enemy.SupportSlowRemaining,Is.EqualTo(1));r.Step();Assert.That(enemy.MoveMultiplier,Is.EqualTo(1));
            var sim=World();sim.P1.SupportInfiniteRemaining=300;sim.P2.SupportSlowRemaining=300;typeof(BattleSimulation).GetProperty("CutInRemainingFrames").SetValue(sim,10);
            BattleTestFactory.Idle(sim,10);Assert.That(sim.P1.SupportInfiniteRemaining,Is.EqualTo(300));Assert.That(sim.P2.SupportSlowRemaining,Is.EqualTo(300));
        }
        [Test]
        public void InfiniteStaminaCoversActionsGuardAndLedgeButNotTheFiveSecondLimit()
        {
            var sim=World();var f=sim.P1;f.SupportInfiniteRemaining=600;f.StaminaUnits=0;
            Assert.That(StaminaSystem.Spend(f,40),Is.True);Assert.That(f.StaminaUnits,Is.Zero);
            f.Mode=FighterMode.Guard;StaminaSystem.Step(f);Assert.That(f.StaminaUnits,Is.Zero);
            var floor=sim.Stage[0].Bounds;f.Mode=FighterMode.Fall;f.Grounded=false;
            f.Position=new BattleVector(floor.Left-f.Definition.HurtboxSize.X*.5f-.1f,floor.Top-f.Definition.HurtboxSize.Y*.5f-.1f);
            LedgeSystem.TryCatch(f,default,sim.Stage,sim.Rules);Assert.That(f.Mode,Is.EqualTo(FighterMode.LedgeHang));
            for(int i=0;i<299;i++)LedgeSystem.StepAttached(f,default,sim.Stage,sim.Rules);
            Assert.That(f.Mode,Is.EqualTo(FighterMode.LedgeHang));LedgeSystem.StepAttached(f,default,sim.Stage,sim.Rules);
            Assert.That(f.Mode,Is.EqualTo(FighterMode.Fall));Assert.That(f.StaminaUnits,Is.Zero);
        }
        [TestCase(AttackKind.Normal,1)] [TestCase(AttackKind.Special,3)] [TestCase(AttackKind.Ultimate,0)]
        public void BarrierStopsThreeNormalHitsOrOneSpecialButUltimatePierces(AttackKind kind,int expectedCost)
        {
            var sim=World();var f=sim.P2;f.SupportBarrierRemaining=300;f.SupportBarrierUnits=3;
            var d=sim.P1.Definition.GroundNormals[0];d.Kind=kind;d.SpawnProjectile=false;d.HitstopFrames=0;
            var events=new List<BattleEvent>();var damage=new DamageSystem();var combo=new ComboTracker();
            int count=kind==AttackKind.Normal?3:1;
            for(int i=0;i<count;i++){damage.Apply(new HitContact(sim.P1,f,new AttackInstance(i+1,d,0,false),false),combo,i,sim.Rules,events);Assert.That(f.SupportBarrierUnits,Is.EqualTo(3-Math.Min(3,expectedCost*(i+1))));}
            Assert.That(f.Hearts,Is.EqualTo(kind==AttackKind.Ultimate?1:3));Assert.That(f.NormalHitCount,Is.Zero);
            if(kind!=AttackKind.Ultimate){Assert.That(f.SupportBarrierRemaining,Is.Zero);Assert.That(events.Count(e=>e.Kind==BattleEventKind.BarrierAbsorbed),Is.EqualTo(count));}
        }
        [Test]
        public void CarArrowAndPawnUseDefenseOwnerImmunityAndSingleHitHistory()
        {
            foreach(var kind in new[]{SupportKind.ShionCar,SupportKind.Mai,SupportKind.Hota})
            {
                var r=With(kind);var owner=r.Collect();r.Step(r.Rules.PortalFrames);var shot=r.Shots.Projectiles.First();var target=r.Sim.P2;
                target.Position=shot.Position;owner.Position=shot.Position;var contacts=new List<HitContact>();var hit=new HitResolver();
                hit.Gather(shot,owner,contacts);Assert.That(contacts,Is.Empty);hit.Gather(shot,target,contacts);Assert.That(contacts.Count,Is.EqualTo(1));
                hit.Gather(shot,target,contacts);Assert.That(contacts.Count,Is.EqualTo(1));
                target.Mode=FighterMode.Guard;target.DefenseAge=100;
                Assert.That(new DefenseSystem().Evaluate(contacts[0],r.Sim.Rules),Is.EqualTo(DefenseOutcome.Guard));
                target.Mode=FighterMode.Idle;target.StaminaUnits=60*60;var current=target.Attack;
                new DamageSystem().Apply(contacts[0],new ComboTracker(),1,r.Sim.Rules,r.Events);
                Assert.That(target.Hearts,Is.EqualTo(kind==SupportKind.Mai?2:3));
                if(kind==SupportKind.ShionCar)Assert.That(target.ParalysisRemaining,Is.EqualTo(120));
                if(kind==SupportKind.Hota){Assert.That(target.StaminaUnits,Is.EqualTo(45*60));Assert.That(target.Attack,Is.SameAs(current));}
            }
        }
        [Test]
        public void SupportProjectilesPauseDuringHitstopAndResumeWithoutLosingTheirLifetime()
        {
            foreach(var kind in new[]{SupportKind.ShionCar,SupportKind.Mai,SupportKind.Hota})
            {
                var r=With(kind);r.Collect();r.Step(r.Rules.PortalFrames);var shot=r.Shots.Projectiles.First();
                var position=shot.Position;int life=shot.RemainingFrames;
                r.Sim.P1.FrozenThisTick=true;r.Shots.Step(r.Sim.P1,r.Sim.P2,stage:r.Sim.Stage);
                Assert.That(shot.Position,Is.EqualTo(position));Assert.That(shot.RemainingFrames,Is.EqualTo(life));
                r.Sim.P1.FrozenThisTick=false;r.Sim.P2.FrozenThisTick=true;r.Shots.Step(r.Sim.P1,r.Sim.P2,stage:r.Sim.Stage);
                Assert.That(shot.Position,Is.EqualTo(position));Assert.That(shot.RemainingFrames,Is.EqualTo(life));
                r.Sim.P2.FrozenThisTick=false;r.Shots.Step(r.Sim.P1,r.Sim.P2,stage:r.Sim.Stage);
                Assert.That(shot.Position,Is.Not.EqualTo(position));Assert.That(shot.RemainingFrames,Is.EqualTo(life-1));
            }
        }
        [TestCase(0)] [TestCase(1)]
        public void WalletIsAvailableToEitherPlayerEvenDuringTheirReuseCooldown(int player)
        {
            var r=With(SupportKind.Tsuyoshi);r.Collect();r.Step(r.Rules.PortalFrames);var f=player==0?r.Sim.P1:r.Sim.P2;
            f.SupportReuseRemaining=500;f.StaminaUnits=0;f.Position=r.System.Wallet.Position;r.Step();
            Assert.That(r.System.Wallet,Is.Null);Assert.That(f.StaminaUnits,Is.EqualTo(f.Definition.Stamina.Maximum*60));
        }
        [Test]
        public void SpawnsAvoidSolidWallsAndRemainOnTheShrinkingMainPlatform()
        {
            var r=new Rig();var floor=r.Sim.Stage[0].Bounds;
            r.Sim.Stage.AddSolid(new BattleRect(new BattleVector(floor.Center.X,floor.Top+3),new BattleVector(floor.Size.X*.8f,6)));
            r.Step();Assert.That(r.System.Pickup,Is.Not.Null);
            Assert.That(Math.Abs(r.System.Pickup.Position.X),Is.GreaterThan(floor.Size.X*.4f));
            r.Sim.Stage.SetPlatform(0,new StagePlatform(new BattleRect(new BattleVector(0,floor.Center.Y),new BattleVector(6,floor.Size.Y)),false));
            r.Step();Assert.That(r.System.Pickup,Is.Null,"Fully obstructed floor must not contain an inaccessible pickup");
        }
        [Test]
        public void SameSeedAndInputsStayDeterministicAndNewMatchClearsAllSupportState()
        {
            var a=new Rig(89);var b=new Rig(89);
            for(int i=0;i<1800;i++)
            {a.Step();b.Step();Assert.That(a.System.Pickup?.Kind,Is.EqualTo(b.System.Pickup?.Kind));Assert.That(a.System.Pickup?.Position,Is.EqualTo(b.System.Pickup?.Position));Assert.That(a.System.SpawnCountdown,Is.EqualTo(b.System.SpawnCountdown));}
            var sim=World();sim.P1.SupportInfiniteRemaining=sim.P1.SupportSlowRemaining=sim.P1.SupportReuseRemaining=300;sim.P1.SupportBarrierUnits=3;
            sim=World();Assert.That(sim.P1.SupportInfiniteRemaining+sim.P1.SupportSlowRemaining+sim.P1.SupportReuseRemaining+sim.P1.SupportBarrierUnits,Is.Zero);Assert.That(sim.Supports.Pickup,Is.Null);Assert.That(sim.Supports.Actors,Is.Empty);
        }
    }
}
