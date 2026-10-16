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
        public void NineEntrantsHaveOwnNamesPortraitsAndIndependentActiveMovesWithOldBackups()
        {
            string[] names={"透真","蓮","千鶴","湊","シロ","結衣","黒獅子王","暁","龍太"};
            Assert.That(Setup.Characters.Length,Is.EqualTo(9));
            int[] bases={0,0,1,1,2,2};
            for(int i=0;i<9;i++)
            {
                var data=Setup.Character(i);Assert.That(data.ToDefinition().Name,Is.EqualTo(names[i]));
                Assert.That(data.SelectionSprite,Is.Not.Null);Assert.That(data.SelectionSprite.texture.width,Is.EqualTo(1304));
                Assert.That(data.SelectionSprite.texture.height,Is.EqualTo(1450));Assert.That(data.PrototypeSharedMoves,Is.False);
                Assert.That(data.MoveSetSchemaVersion,Is.GreaterThanOrEqualTo(1));
                if(i<3)continue;
                var original=Setup.Character(bases[i-3]);
                Assert.That(data.GroundNormals.Intersect(original.GroundNormals),Is.Empty);Assert.That(data.AirNormals.Intersect(original.AirNormals),Is.Empty);
                foreach(int slot in new[]{1,2,3})Assert.That(data.DirectionalNormals[slot],Is.Not.SameAs(original.DirectionalNormals[slot]));
                foreach(int slot in new[]{4,5,6,7,8,9})Assert.That(data.DirectionalNormals[slot],Is.SameAs(original.DirectionalNormals[slot]));
                Assert.That(data.Specials[0],Is.Not.SameAs(original.Specials[0]));
                foreach(int slot in new[]{1,2,3})Assert.That(data.Specials[slot],Is.SameAs(original.Specials[slot]));
                Assert.That(data.Ultimate,Is.Not.SameAs(original.Ultimate));Assert.That(data.PaperMotion.Sprite,Is.SameAs(data.SelectionSprite));
                Assert.DoesNotThrow(()=>new BattleSimulation(data.ToDefinition(),original.ToDefinition(),PhaseOneDefaults.Rules(),Setup.Stages[0].ToGeometry()));
            }
            Assert.That(Setup.Characters.Select(c=>c.SelectionSprite).Distinct().Count(),Is.EqualTo(9));
        }
        [Test]
        public void TwoRowsFollowTheRequestedOrderAndHorizontalWrappingStaysInTheSameRow()
        {
            Assert.That(PaperRosterGrid.Layout.OrderBy(i=>i),Is.EqualTo(Enumerable.Range(0,9)));
            Assert.That(PaperRosterGrid.Layout,Is.EqualTo(new[]{6,3,7,5,1,4,0,2,8}));
            for(int i=0;i<9;i++)
            {
                Assert.That(PaperRosterGrid.Move(PaperRosterGrid.Move(i,1,0),-1,0),Is.EqualTo(i));
                Assert.That(PaperRosterGrid.UpperRow(PaperRosterGrid.Move(i,1,0)),Is.EqualTo(PaperRosterGrid.UpperRow(i)));
                Assert.That(PaperRosterGrid.UpperRow(PaperRosterGrid.Move(i,0,1)),Is.Not.EqualTo(PaperRosterGrid.UpperRow(i)));
                Assert.That(PaperRosterGrid.Move(i,0,-1),Is.EqualTo(PaperRosterGrid.Move(i,0,1)));
            }
            Assert.That(PaperRosterGrid.Move(5,1,0),Is.EqualTo(1));Assert.That(PaperRosterGrid.Move(1,-1,0),Is.EqualTo(5));
            Assert.That(PaperRosterGrid.Move(0,0,1),Is.EqualTo(3));Assert.That(PaperRosterGrid.Move(3,0,1),Is.EqualTo(0));
            Assert.That(PaperRosterGrid.Move(6,-1,0),Is.EqualTo(7));Assert.That(PaperRosterGrid.Move(8,1,0),Is.EqualTo(5));
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
