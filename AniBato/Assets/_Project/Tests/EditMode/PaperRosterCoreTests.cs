using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperRosterCoreTests
    {
        private static PaperRosterCoreBuilder.Manifest Manifest => JsonUtility.FromJson<PaperRosterCoreBuilder.Manifest>(File.ReadAllText(PaperRosterCoreBuilder.ManifestPath));
        [Test]
        public void CompletedCharacterBatchesHaveFiveDistinctWholePosesAndSharedScale()
        {
            var setup = AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            Assert.That(Manifest.characters.Length, Is.GreaterThanOrEqualTo(1));
            foreach (var entry in Manifest.characters)
            {
                var data = setup.Character(entry.characterIndex); var paper = data.PaperMotion;
                Assert.That(data.Movement.Name, Is.EqualTo(entry.name)); Assert.That(paper, Is.Not.Null);
                Assert.That(paper.SchemaVersion, Is.EqualTo(3)); Assert.That(paper.Enabled, Is.True);
                Assert.That(entry.poses.Select(p => p.key), Is.EquivalentTo(new[] { "guard", "crouch", "counter", "win", "lose" }));
                var sprites = entry.poses.Select(s => paper.Find(State(s.key)).Frames.Single()).ToArray();
                Assert.That(sprites.Distinct().Count(), Is.EqualTo(5));
                foreach (var sprite in sprites.Concat(new[] { paper.Sprite }))
                {
                    Assert.That(sprite.texture.width, Is.EqualTo(1304)); Assert.That(sprite.texture.height, Is.EqualTo(1450));
                    Assert.That(sprite.pixelsPerUnit, Is.EqualTo(532.5893f).Within(.001));
                    Assert.That(sprite.pivot.x, Is.EqualTo(652).Within(.001)); Assert.That(sprite.pivot.y, Is.EqualTo(122).Within(.001));
                }
                Assert.That(paper.Find(AnimationState.Crouch).HeadHeight, Is.LessThan(paper.StandingHeight * .85f));
                Assert.That(paper.Find(AnimationState.Guard).Frame(10000), Is.SameAs(paper.Find(AnimationState.Guard).Frame(0)));
                Assert.That(paper.Find(AnimationState.CounterSuccess), Is.SameAs(paper.Find(AnimationState.Counter)));
                Assert.That(paper.WalkLeanDegrees, Is.EqualTo(15)); Assert.That(paper.DashLeanDegrees, Is.EqualTo(45));
                if (!data.PrototypeSharedMoves) Assert.That(paper.Supports(AnimationState.Attack1), Is.False);
            }
        }
        [Test]
        public void ImportedCoreImagesMatchPreparedFilesAndKeepHardTransparentCutoutEdges()
        {
            foreach (var entry in Manifest.characters) foreach (var source in entry.poses)
            {
                string path = "Assets/_Project/Art/PaperCore/" + entry.key + "_" + source.key + ".png";
                byte[] prepared = File.ReadAllBytes("../" + source.prepared);
                Assert.That(File.ReadAllBytes(path), Is.EqualTo(prepared));
                using (var sha = SHA256.Create())
                    Assert.That(System.BitConverter.ToString(sha.ComputeHash(prepared)).Replace("-", "").ToLowerInvariant(), Is.EqualTo(source.sha256));
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None)); Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
                var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    Assert.That(ImageConversion.LoadImage(image, prepared), Is.True);
                    var colors = image.GetPixels32(); int lowest = 1450; bool opaque = false, transparent = false, fractional = false;
                    for (int y = 0; y < 1450; y++) for (int x = 0; x < 1304; x++)
                    {
                        byte alpha = colors[y * 1304 + x].a; if (alpha != 0 && alpha != 255) fractional = true;
                        if (alpha == 255) { lowest = System.Math.Min(lowest, y); opaque = true; } else transparent = true;
                    }
                    Assert.That(fractional, Is.False); Assert.That(opaque && transparent, Is.True); Assert.That(lowest, Is.EqualTo(121));
                }
                finally { Object.DestroyImmediate(image); }
            }
        }
        private static AnimationState State(string key) => key == "guard" ? AnimationState.Guard : key == "crouch" ? AnimationState.Crouch : key == "counter" ? AnimationState.Counter : key == "win" ? AnimationState.Win : AnimationState.Lose;
    }
}
