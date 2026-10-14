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
            Assert.That(motion, Is.Not.Null); Assert.That(motion.Enabled, Is.True); Assert.That(motion.SchemaVersion, Is.EqualTo(2));
            Assert.That(motion.Sprite, Is.Not.Null); Assert.That(motion.StandingHeight, Is.EqualTo(112f / 50));
            Assert.That(motion.WalkLeanDegrees, Is.EqualTo(15)); Assert.That(motion.DashLeanDegrees, Is.EqualTo(45));
            Assert.That(motion.Motion.IdleDegrees, Is.EqualTo(.75f)); Assert.That(motion.Motion.IdleFps, Is.EqualTo(4));
            Assert.That(motion.Supports(AnimationState.Idle), Is.True); Assert.That(motion.Supports(AnimationState.Move), Is.True);
            foreach (AnimationState state in System.Enum.GetValues(typeof(AnimationState)))
                Assert.That(motion.Supports(state), Is.True, state.ToString());
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
                Assert.That(motion.Sprite.pivot.x, Is.EqualTo(AniBato.EditorTools.PaperSourcePacking.CanvasWidth * .5f).Within(.001));
                Assert.That(motion.Sprite.pivot.y, Is.EqualTo(minY + (AniBato.EditorTools.PaperSourcePacking.CanvasHeight - height) / 2).Within(.001));
            }
            finally { Object.DestroyImmediate(source); }
        }
        [Test]
        public void AllBodySourcesArePackedWithoutScalingOrDiscardingAnyPixel()
        {
            var manifest = JsonUtility.FromJson<AniBato.EditorTools.PaperActionBuilder.Manifest>(File.ReadAllText(AniBato.EditorTools.PaperActionBuilder.ManifestPath));
            var motion = AssetDatabase.LoadAssetAtPath<PaperCharacterMotion>(AniBato.EditorTools.PaperCharacterBuilder.DataPath);
            Assert.That(manifest.poses.Length, Is.EqualTo(30)); Assert.That(manifest.effects.Length, Is.EqualTo(4));
            Assert.That(motion.Poses.Length, Is.EqualTo(29));
            foreach (var row in manifest.poses)
            {
                var pose = row.key == "hit_heavy" ? motion.HeavyDamage : System.Array.Find(motion.Poses, p => p.Key == row.key);
                Assert.That(pose, Is.Not.Null, row.key); Assert.That(pose.Valid, Is.True); Assert.That(pose.HoldTicks, Is.EqualTo(8));
                Assert.That(pose.Frames.Length, Is.EqualTo(4));
                foreach (var frame in pose.Frames) { Assert.That(frame, Is.SameAs(pose.Frames[0])); Assert.That(frame.pixelsPerUnit, Is.EqualTo(motion.Sprite.pixelsPerUnit)); }
                var source = new Texture2D(2, 2, TextureFormat.RGBA32, false); var packed = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    Assert.That(ImageConversion.LoadImage(source, File.ReadAllBytes("../" + row.source)), Is.True);
                    Assert.That(ImageConversion.LoadImage(packed, File.ReadAllBytes(AssetDatabase.GetAssetPath(pose.Frames[0]))), Is.True);
                    Assert.That(packed.width, Is.EqualTo(1304)); Assert.That(packed.height, Is.EqualTo(1450));
                    var input = source.GetPixels32(); var output = packed.GetPixels32();
                    int dx = (packed.width - source.width) / 2, dy = (packed.height - source.height) / 2, mismatch = 0, outside = 0;
                    for (int y = 0; y < packed.height; y++) for (int x = 0; x < packed.width; x++)
                    {
                        var actual = output[y * packed.width + x]; int sx = x - dx, sy = y - dy;
                        if (sx >= 0 && sy >= 0 && sx < source.width && sy < source.height) { if (!actual.Equals(input[sy * source.width + sx])) mismatch++; }
                        else if (actual.a != 0) outside++;
                    }
                    Assert.That(mismatch, Is.Zero, row.key + " source pixels changed"); Assert.That(outside, Is.Zero, row.key + " padding");
                    Assert.That(pose.HeadHeight, Is.EqualTo((source.height - row.headTopY + dy - pose.Frames[0].pivot.y) / motion.Sprite.pixelsPerUnit).Within(.00001));
                    if (!row.airborne) Assert.That(pose.FootBaseY, Is.EqualTo(0).Within(.00001));
                }
                finally { Object.DestroyImmediate(source); Object.DestroyImmediate(packed); }
            }
            Assert.That(motion.Bag, Is.Not.Null); Assert.That(motion.RiceBall, Is.Not.Null); Assert.That(motion.Mogu, Is.Not.Null); Assert.That(motion.CurryWave, Is.Not.Null);
            foreach (AnimationState state in System.Enum.GetValues(typeof(AnimationState)))
            {
                var pose = motion.Find(state); Assert.That(pose, Is.Not.Null, state.ToString());
                for (int age = 0; age < 120; age++)
                {
                    var sample = new AnimationSample { State = state, Age = age, IsAttack = true, AttackFrame = age, Startup = 5, Active = 5, Recovery = 25 };
                    Assert.That(motion.TrySprite(sample, false, out var sprite, out var selected), Is.True);
                    Assert.That(sprite, Is.SameAs(pose.Frames[0])); Assert.That(selected, Is.SameAs(pose));
                }
            }
            Assert.That(motion.Find(AnimationState.Hitstun, true), Is.SameAs(motion.HeavyDamage));
            Assert.That(motion.Find(AnimationState.Knockback, true), Is.SameAs(motion.HeavyDamage));
            Assert.That(motion.Find(AnimationState.Hitstun, false).Key, Is.EqualTo("hit_light"));
        }
    }
}
