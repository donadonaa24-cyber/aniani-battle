using System.Linq;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class BagThrowTests
    {
        [Test]
        public void OnlyChizurusThirdGroundNormalBecomesAThrowAndRetainsNormalTiming()
        {
            for(int c=0;c<3;c++)
            {
                var current=PhaseSixADefaults.Character(c);var previous=PhaseFiveBDefaults.Character(c);
                for(int slot=0;slot<3;slot++)
                {
                    var a=current.GroundNormals[slot];var old=previous.GroundNormals[slot];
                    Assert.That(a.SpawnProjectile,Is.EqualTo(c==2 && slot==2));
                    Assert.That(a.IsNormal,Is.True);
                    Assert.That(a.StartupFrames,Is.EqualTo(old.StartupFrames));Assert.That(a.ActiveFrames,Is.EqualTo(old.ActiveFrames));
                    Assert.That(a.RecoveryFrames,Is.EqualTo(old.RecoveryFrames));Assert.That(a.HitstunFrames,Is.EqualTo(old.HitstunFrames));
                    Assert.That(a.NormalForwardDistance,Is.EqualTo(.18f));Assert.That(a.DamageRecoilDistance,Is.EqualTo(.18f));
                }
                Assert.That(current.AirNormals.Concat(current.DirectionalNormals.Skip(1)).Any(a=>a.SpawnProjectile),Is.False);
                Assert.That(current.Specials.Select(a=>a.SpawnProjectile),Is.EqualTo(previous.Specials.Select(a=>a.SpawnProjectile)));
            }
        }
        [TestCase(1)] [TestCase(-1)]
        public void BagSpawnsOnceOnFirstActiveTickAndKeepsItsLaunchDirectionAfterOwnerTurns(int facing)
        {
            var chizuru=PhaseSixADefaults.Character(2);var bag=chizuru.GroundNormals[2];
            chizuru.GroundNormals=new[]{bag};chizuru.AirNormals=new[]{bag};chizuru.DirectionalNormals=null;
            var sim=BattleTestFactory.Create(definition:chizuru);sim.P1.Facing=facing;sim.P2.Position=new BattleVector(15,1.1f);
            sim.Tick(new FrameInput(pressed:InputButtons.Attack),default);
            while(sim.P1.Attack.Frame<bag.StartupFrames-1){Assert.That(sim.Projectiles.Count,Is.Zero);sim.Tick(default,default);}
            Assert.That(sim.Projectiles.Count,Is.Zero);sim.Tick(default,default);
            Assert.That(sim.Projectiles.Count,Is.EqualTo(1));var shot=sim.Projectiles[0];
            Assert.That(shot.RemainingFrames,Is.EqualTo(24));Assert.That(shot.Velocity.X,Is.EqualTo(12*facing));
            Assert.That(shot.Position.X,Is.EqualTo(sim.P1.Position.X+1.8f*facing).Within(.00001f));
            Assert.That(shot.Hitbox.Size,Is.EqualTo(new BattleVector(.4f,.36f)));long id=shot.Attack.Id;
            sim.P1.Facing=-facing;float x=shot.Position.X;
            for(int tick=0;tick<23;tick++){
                sim.Tick(default,default);Assert.That(sim.Projectiles.Count,Is.EqualTo(1));
                Assert.That(sim.Projectiles[0].Attack.Id,Is.EqualTo(id));Assert.That(shot.Facing,Is.EqualTo(facing));
                Assert.That(shot.Position.X,Is.EqualTo(x+(tick+1)*.2f*facing).Within(.00002f));
            }
            sim.Tick(default,default);Assert.That(sim.Projectiles.Count,Is.Zero);
        }
        [TestCase(1)] [TestCase(-1)]
        public void ThrownBagHitsNormallyOnceEvenWhileOverlappingAndProducesConfiguredRecoil(int facing)
        {
            var c=PhaseSixADefaults.Character(2);var bag=c.GroundNormals[2];
            bag.ProjectileSpeed=.1f;bag.Knockback=default;bag.HitstopFrames=0;bag.HitstunFrames=0;
            c.GroundNormals=new[]{bag};c.AirNormals=new[]{bag};c.DirectionalNormals=null;
            var sim=BattleTestFactory.Create(definition:c);sim.P1.Position=new BattleVector(0,1.1f);sim.P1.Facing=facing;sim.P2.Position=new BattleVector(1.98f*facing,1.1f);
            float before=sim.P2.Position.X;sim.Tick(new FrameInput(pressed:InputButtons.Attack),default);
            while(sim.Projectiles.Count==0)sim.Tick(default,default);
            Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));Assert.That(sim.P2.Hearts,Is.EqualTo(3));
            Assert.That(sim.P2.DamageRecoilRemaining,Is.EqualTo(6));
            BattleTestFactory.Idle(sim,6);Assert.That(sim.P2.Position.X,Is.EqualTo(before+.18f*facing).Within(.00002f));
            BattleTestFactory.Idle(sim,20);Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));
        }
        [TestCase(1)] [TestCase(-1)]
        public void ThirdNormalHeartCanBeLostToTheReleasedBag(int facing)
        {
            var c=PhaseSixADefaults.Character(2);var bag=c.GroundNormals[2];
            c.GroundNormals=new[]{bag};c.AirNormals=new[]{bag};c.DirectionalNormals=null;
            var sim=BattleTestFactory.Create(definition:c);sim.P1.Position=new BattleVector(0,1.1f);sim.P1.Facing=facing;
            sim.P2.Position=new BattleVector(1.98f*facing,1.1f);sim.P2.NormalHitCount=2;
            for(int tick=0;tick<=bag.StartupFrames;tick++)
                sim.Tick(tick==0?new FrameInput(pressed:InputButtons.Attack):default,default);
            Assert.That(sim.P2.Hearts,Is.EqualTo(2));Assert.That(sim.P2.NormalHitCount,Is.Zero);
            Assert.That(sim.P2.LastDamageLostHeart,Is.True);Assert.That(sim.P2.DamageRecoilRemaining,Is.EqualTo(6));
        }
        [TestCase(1)] [TestCase(-1)]
        public void BagRespectsTheUserApprovedNormalGuardRulesAndDoesNotApplyDamageRecoil(int facing)
        {
            var c=PhaseSixADefaults.Character(2);var bag=c.GroundNormals[2];
            c.GroundNormals=new[]{bag};c.AirNormals=new[]{bag};c.DirectionalNormals=null;
            var sim=BattleTestFactory.Create(definition:c);sim.P1.Position=new BattleVector(0,1.1f);sim.P1.Facing=facing;
            sim.P2.Position=new BattleVector(1.98f*facing,1.1f);sim.P2.NormalHitCount=2;
            for(int tick=0;tick<20;tick++)sim.Tick(default,new FrameInput(held:InputButtons.Guard));
            for(int tick=0;tick<=bag.StartupFrames;tick++)
                sim.Tick(tick==0?new FrameInput(pressed:InputButtons.Attack):default,new FrameInput(held:InputButtons.Guard));
            Assert.That(sim.Projectiles.Count,Is.EqualTo(1));Assert.That(sim.Projectiles[0].Attack.HasHit(1),Is.True);
            Assert.That(sim.Events.Any(e=>e.Kind==BattleEventKind.Guarded),Is.True);
            Assert.That(sim.P2.Hearts,Is.EqualTo(3));Assert.That(sim.P2.NormalHitCount,Is.EqualTo(2));
            Assert.That(sim.P2.LastDamageLostHeart,Is.False);Assert.That(sim.P2.DamageRecoilRemaining,Is.Zero);
        }
    }
}
