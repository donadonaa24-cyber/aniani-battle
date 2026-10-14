using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class PaperRosterTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [Test]
        public void NineEntrantsHaveOwnNamesPortraitsAndExplicitPrototypeMoveSharing()
        {
            string[] names={"透真","蓮","千鶴","湊","シロ","結衣","黒獅子王","暁","龍太"};
            Assert.That(Setup.Characters.Length,Is.EqualTo(9));
            int[] bases={0,0,1,1,2,2};
            for(int i=0;i<9;i++)
            {
                var data=Setup.Character(i);Assert.That(data.ToDefinition().Name,Is.EqualTo(names[i]));
                Assert.That(data.SelectionSprite,Is.Not.Null);Assert.That(data.SelectionSprite.texture.width,Is.EqualTo(1304));
                Assert.That(data.SelectionSprite.texture.height,Is.EqualTo(1450));Assert.That(data.PrototypeSharedMoves,Is.EqualTo(i>=3));
                if(i<3)continue;
                var original=Setup.Character(bases[i-3]);
                Assert.That(data.GroundNormals,Is.EqualTo(original.GroundNormals));Assert.That(data.AirNormals,Is.EqualTo(original.AirNormals));
                Assert.That(data.DirectionalNormals,Is.EqualTo(original.DirectionalNormals));Assert.That(data.Specials,Is.EqualTo(original.Specials));
                Assert.That(data.Ultimate,Is.SameAs(original.Ultimate));Assert.That(data.PaperMotion.Sprite,Is.SameAs(data.SelectionSprite));
                Assert.DoesNotThrow(()=>new BattleSimulation(data.ToDefinition(),original.ToDefinition(),PhaseOneDefaults.Rules(),Setup.Stages[0].ToGeometry()));
            }
            Assert.That(Setup.Characters.Select(c=>c.SelectionSprite).Distinct().Count(),Is.EqualTo(9));
        }
        [Test]
        public void WorkGridContainsAllNineOnceAndBothAxesWrapInsideTheVisibleGrid()
        {
            Assert.That(PaperRosterGrid.Layout.OrderBy(i=>i),Is.EqualTo(Enumerable.Range(0,9)));
            for(int i=0;i<9;i++)
            {
                Assert.That(PaperRosterGrid.Move(PaperRosterGrid.Move(i,1,0),-1,0),Is.EqualTo(i));
                Assert.That(PaperRosterGrid.Move(PaperRosterGrid.Move(i,0,1),0,-1),Is.EqualTo(i));
            }
            Assert.That(PaperRosterGrid.Move(0,0,1),Is.EqualTo(3));Assert.That(PaperRosterGrid.Move(3,0,1),Is.EqualTo(4));
        }
        [Test]
        public void SharedGeometryLeavesRecoverySpaceAndThemesDoNotChangeRingOutOrPlatforms()
        {
            foreach(var stage in Setup.Stages)
            {
                var g=stage.ToGeometry();Assert.That(g.PlatformCount,Is.EqualTo(3));Assert.That(g[0].OneWay,Is.False);
                Assert.That(g[1].OneWay&&g[2].OneWay,Is.True);
                Assert.That(g.LeftBlastLine,Is.LessThan(g[0].Bounds.Left));Assert.That(g.RightBlastLine,Is.GreaterThan(g[0].Bounds.Right));
                Assert.That(g.IsOutside(new BattleVector(g[0].Bounds.Right+.2f,-2)),Is.False);
                Assert.That(g.IsOutside(new BattleVector(g.RightBlastLine+.01f,1)),Is.True);
                Assert.That(g.IsOutside(new BattleVector(g.LeftBlastLine-.01f,1)),Is.True);
                Assert.That(g.IsOutside(new BattleVector(0,g.BottomBlastLine-.01f)),Is.True);
            }
        }
    }
}
