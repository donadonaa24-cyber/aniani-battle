using System.IO;
using System.Security.Cryptography;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperCharacterAssetTests
    {
        [Test]
        public void OnlyChizuruUsesPaperAndOriginalBytesAndWorldHeightArePreserved()
        {
            var chizuru = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Chizuru.asset");
            var motion = chizuru.PaperMotion;
            Assert.That(motion, Is.Not.Null); Assert.That(motion.Enabled, Is.True); Assert.That(motion.SchemaVersion, Is.EqualTo(1));
            Assert.That(motion.Sprite, Is.Not.Null); Assert.That(motion.StandingHeight, Is.EqualTo(112f / 50));
            Assert.That(motion.Motion.WalkDegrees, Is.EqualTo(5)); Assert.That(motion.Motion.WalkFps, Is.EqualTo(8));
            Assert.That(motion.Motion.IdleDegrees, Is.EqualTo(.75f)); Assert.That(motion.Motion.IdleFps, Is.EqualTo(4));
            Assert.That(motion.Supports(AnimationState.Idle), Is.True); Assert.That(motion.Supports(AnimationState.Move), Is.True);
            foreach (AnimationState state in System.Enum.GetValues(typeof(AnimationState)))
                if (state != AnimationState.Idle && state != AnimationState.Move) Assert.That(motion.Supports(state), Is.False);
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Touma.asset").PaperMotion, Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Ren.asset").PaperMotion, Is.Null);
            const string image = "Assets/_Project/Art/Paper/idle_paper_chizuru.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(image);
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            Assert.That(width, Is.EqualTo(1246)); Assert.That(height, Is.EqualTo(1263));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear)); Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None)); Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            using (var sha = SHA256.Create())
                Assert.That(sha.ComputeHash(File.ReadAllBytes(image)), Is.EqualTo(sha.ComputeHash(File.ReadAllBytes("../art_source/paper_cutout/generated/chizuru_paper_idle_v1.png"))));
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(source, File.ReadAllBytes(image)), Is.True);
                var pixels = source.GetPixels32(); int minY = height, maxY = -1;
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                    if (pixels[y * width + x].a >= 32) { minY = System.Math.Min(minY, y); maxY = System.Math.Max(maxY, y); }
                Assert.That((maxY - minY + 1) / motion.Sprite.pixelsPerUnit, Is.EqualTo(2.24f).Within(.00001));
                Assert.That(motion.Sprite.pivot.x, Is.EqualTo(width * .5f).Within(.001));
                Assert.That(motion.Sprite.pivot.y, Is.EqualTo(minY).Within(.001));
            }
            finally { Object.DestroyImmediate(source); }
        }
    }
}
