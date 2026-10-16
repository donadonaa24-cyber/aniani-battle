using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
namespace AniBato.Tests
{
    public sealed class PaperReleasePresentationTests
    {
        [Test]
        public void UltimateImpactHoldsForThirtySixTicksAndNewActionsInterruptImmediately()
        {
            var impact=new AnimationSample{State=AnimationState.Ultimate,IsAttack=true,AttackFrame=12,Startup=10,DamageDelay=2};
            var hold=new PaperUltimatePoseHold();
            Assert.That(hold.Evaluate(impact,100,36).State,Is.EqualTo(AnimationState.Ultimate));
            var idle=new AnimationSample{State=AnimationState.Idle};
            for(int tick=101;tick<136;tick++)
            {
                for(int renders=0;renders<3;renders++)Assert.That(hold.Evaluate(idle,tick,36).State,Is.EqualTo(AnimationState.Ultimate));
            }
            Assert.That(hold.Evaluate(idle,136,36).State,Is.EqualTo(AnimationState.Idle));
            foreach(var state in new[]{AnimationState.Move,AnimationState.Jump,AnimationState.Hitstun,AnimationState.Knockback,AnimationState.Attack1,AnimationState.Guard,AnimationState.Win})
            {
                hold.Reset();hold.Evaluate(impact,100,36);
                Assert.That(hold.Evaluate(new AnimationSample{State=state},101,36).State,Is.EqualTo(state));
                Assert.That(hold.Evaluate(idle,102,36).State,Is.EqualTo(AnimationState.Idle));
            }
            hold.Evaluate(impact,200,36);Assert.That(hold.Evaluate(idle,0,36).State,Is.EqualTo(AnimationState.Idle));
        }
        [Test]
        public void ChizuruReleaseIsASeparateNativePoseAndSwitchesOnEmissionRatherThanDamageDelay()
        {
            var setup=AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            var pose=setup.Character(2).PaperMotion.Find(AnimationState.SpecialNeutral);
            Assert.That(pose.Frames.Length,Is.EqualTo(2));Assert.That(pose.Frames[0],Is.Not.SameAs(pose.Frames[1]));
            Assert.That(pose.SwitchOnProjectileRelease,Is.True);
            foreach(var frame in pose.Frames){Assert.That(frame.texture.width,Is.EqualTo(1304));Assert.That(frame.texture.height,Is.EqualTo(1450));Assert.That(frame.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001));}
            var sample=new AnimationSample{IsAttack=true,Startup=12,DamageDelay=4,ProjectileReleaseFrame=12,AttackFrame=11};
            Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[0]));sample.AttackFrame=12;Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[1]));
            sample.ProjectileReleaseFrame=16;sample.AttackFrame=15;Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[0]));sample.AttackFrame=16;Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[1]));
        }
    }
}
