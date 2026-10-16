using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace AniBato.Tests
{
    public sealed class GeneratedStageArtTests
    {
        [Test]
        public void EveryThemeHasSevenDistinctGeneratedLayersAndValidDeckRegions()
        {
            var setup=AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            foreach(var stage in setup.Stages)
            {
                var art=stage.GeneratedArt;Assert.That(art&&art.Complete,Is.True);
                Assert.That(new[]{art.Far,art.Mid,art.Frame,art.Main,art.Upper,art.Warning,art.Removal}.Distinct().Count(),Is.EqualTo(7));
                foreach(var pair in new[]{(art.Main,art.MainRegion),(art.Upper,art.UpperRegion)})
                {
                    Assert.That(pair.Item2.width,Is.GreaterThan(100));Assert.That(pair.Item2.height,Is.GreaterThan(10));
                    Assert.That(pair.Item2.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(pair.Item2.yMin,Is.GreaterThanOrEqualTo(0));
                    Assert.That(pair.Item2.xMax,Is.LessThanOrEqualTo(pair.Item1.texture.width));Assert.That(pair.Item2.yMax,Is.LessThanOrEqualTo(pair.Item1.texture.height));
                }
                Assert.That(stage.Thumbnail,Is.SameAs(art.Far));
            }
        }
        [Test]
        public void EnsembleHasNineSeparateFaceHotspotsInsideTheSameOriginal()
        {
            var art=AssetDatabase.LoadAssetAtPath<PaperEnsembleArt>("Assets/_Project/Data/PaperEnsemble.asset");
            Assert.That(art.Artwork.texture.width,Is.EqualTo(1672));Assert.That(art.Artwork.texture.height,Is.EqualTo(941));
            Assert.That(art.Hotspots.Length,Is.EqualTo(9));
            foreach(var rect in art.Hotspots)
            {
                Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(0));
                Assert.That(rect.xMax,Is.LessThanOrEqualTo(1));Assert.That(rect.yMax,Is.LessThanOrEqualTo(1));
                Assert.That(rect.width*art.Artwork.texture.width,Is.GreaterThan(200));
            }
            for(int i=0;i<9;i++)for(int j=i+1;j<9;j++)Assert.That(art.Hotspots[i].Overlaps(art.Hotspots[j]),Is.False,"人物の選択領域は重ねない");
        }
    }
}
