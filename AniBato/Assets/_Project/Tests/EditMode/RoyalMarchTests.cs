using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class RoyalMarchTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [TestCase(-1)] [TestCase(1)]
        public void ExistingThreeMinionsEmergeForOneSecondWaitForTheOrderAndMarchToTheEdge(int facing)
        {
            var c=Setup.Character(6).ToDefinition();var rules=PhaseOneDefaults.Rules();rules.EnableRingOut=false;
            var sim=BattleTestFactory.Create(false,rules,c);sim.P1.Facing=facing;
            sim.P1.Position=new BattleVector(-facing*15,sim.P1.Position.Y);sim.P2.Position=new BattleVector(0,1000);
            sim.Tick(new FrameInput(pressed:InputButtons.Special),default);sim.Tick(default,default);
            var shots=sim.Projectiles.ToArray();Assert.That(shots.Length,Is.EqualTo(3));
            Assert.That(shots.Select(p=>p.VariantIndex),Is.EqualTo(new[]{0,1,2}));
            var positions=shots.Select(p=>p.Position).ToArray();
            for(int i=0;i<60;i++)
            {sim.Tick(default,default);Assert.That(shots.All(p=>!p.CanDamage),Is.True);}
            Assert.That(shots.All(p=>p.MarchCommand),Is.True);
            Assert.That(shots.Select(p=>p.Position),Is.EqualTo(positions));
            for(int i=0;i<23;i++)sim.Tick(default,default);
            Assert.That(shots.All(p=>p.MarchCommand),Is.True);sim.Tick(default,default);
            Assert.That(shots.All(p=>p.CanDamage&&!p.MarchCommand),Is.True);
            var starts=shots.Select(p=>p.Position.X).ToArray();sim.P1.Facing=-facing;
            for(int i=0;i<60;i++)sim.Tick(default,default);
            for(int i=0;i<3;i++)
            {
                Assert.That((shots[i].Position.X-starts[i])*facing,Is.EqualTo(12).Within(.001));
                Assert.That(shots[i].Facing,Is.EqualTo(facing));
                Assert.That(shots[i].Hitbox.Size.Y,Is.EqualTo(Setup.Character(6).PaperMotion.StandingHeight).Within(.001));
            }
            int waited=0;while(shots.Any(p=>p.MarchExitAge<0)&&waited++<180)sim.Tick(default,default);
            Assert.That(shots.All(p=>p.MarchExitAge>=0&&!p.CanDamage),Is.True);
            foreach(var p in shots)Assert.That(facing>0?p.Hitbox.Right:p.Hitbox.Left,
                Is.EqualTo(facing>0?sim.Stage[0].Bounds.Right:sim.Stage[0].Bounds.Left).Within(.001));
            for(int i=0;i<24;i++)sim.Tick(default,default);Assert.That(sim.Projectiles,Is.Empty);
            Assert.That(sim.P1.Hearts,Is.EqualTo(3));
        }
        [Test]
        public void ThreeStagesShareTheWiderBlastZoneWithoutChangingAnyPlatform()
        {
            var geometry=Setup.Stages[0].ToGeometry();var original=PhaseOneDefaults.Stage();
            foreach(var stage in Setup.Stages)
            {
                var actual=stage.ToGeometry();Assert.That(actual.LeftBlastLine,Is.EqualTo(-32));
                Assert.That(actual.RightBlastLine,Is.EqualTo(32));Assert.That(actual.BottomBlastLine,Is.EqualTo(-16));
                Assert.That(actual.PlatformCount,Is.EqualTo(3));
                for(int i=0;i<3;i++)Assert.That(actual[i],Is.EqualTo(original[i]));
                Assert.That(actual.IsOutside(new BattleVector(25,-12)),Is.False);
                Assert.That(actual.IsOutside(new BattleVector(32.01f,0)),Is.True);
                Assert.That(actual.IsOutside(new BattleVector(0,-16.01f)),Is.True);
            }
        }
    }
}
