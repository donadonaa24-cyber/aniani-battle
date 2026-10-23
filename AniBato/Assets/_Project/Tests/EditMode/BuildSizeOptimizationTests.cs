using System.Linq;
using System.IO;
using System.Security.Cryptography;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class BuildSizeOptimizationTests
    {
        [Test]
        public void WindowsBuildUsesLosslessArchiveAndTheSameDependencyPruningAsMobile()
        {
            Assert.That(WindowsBuilder.Options.HasFlag(BuildOptions.CompressWithLz4HC),Is.True);
            Assert.That(WindowsBuilder.Options.HasFlag(BuildOptions.DetailedBuildReport),Is.True);
            Assert.That(WindowsBuilder.Options.HasFlag(BuildOptions.CleanBuildCache),Is.True,"Transient texture maps cannot reuse scene serialization from another profile");
            Assert.That(WindowsBuilder.Options.HasFlag(BuildOptions.Development),Is.False);
            Assert.That(WebGLScenePacking.SupportsTarget(BuildTarget.StandaloneWindows64),Is.True);
            Assert.That(WebGLSpritePacking.Enabled,Is.False,"Scene-copy unit tests must begin without a build-only texture map");
            var source=AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            var before=source.Characters.Select(c=>JsonUtility.ToJson(c.PaperMotion)).ToArray();
            try
            {
                var originalTextures=EditorUtility.CollectDependencies(new Object[]{source}).OfType<Texture2D>().ToArray();
                var copy=WebGLScenePacking.CopyForBuild(source);
                var packedTextures=EditorUtility.CollectDependencies(new Object[]{copy}).OfType<Texture2D>().ToArray();
                Assert.That(packedTextures.Length,Is.LessThan(originalTextures.Length),"Unreferenced poses and old backdrops must actually leave the build graph");
                foreach(var texture in packedTextures)Assert.That(originalTextures,Does.Contain(texture),"Every shipped texture uses the exact original image/import");
                for(int index=0;index<9;index++)
                {
                    var original=source.Character(index).PaperMotion;var packed=copy.Character(index).PaperMotion;
                    Assert.That(packed.Bag,Is.Null);
                    Assert.That(packed.Sprite,Is.SameAs(original.Sprite));
                    Assert.That(packed.LedgeHang,Is.SameAs(original.LedgeHang));
                    Assert.That(packed.Charging.Frames,Is.EqualTo(original.Charging.Frames));
                    Assert.That(packed.HeavyDamage.Frames,Is.EqualTo(original.HeavyDamage.Frames));
                    foreach(var pose in packed.Poses)
                    {
                        var originalPose=original.Poses.Single(p=>p.Key==pose.Key);
                        Assert.That(pose.Frames,Is.EqualTo(originalPose.Frames));
                        foreach(var frame in pose.Frames)
                        {
                            Assert.That(frame.rect,Is.EqualTo(originalPose.Frames.First(f=>f==frame).rect));
                            Assert.That(frame.pivot,Is.EqualTo(originalPose.Frames.First(f=>f==frame).pivot));
                        }
                    }
                }
                Assert.That(source.Characters.Select(c=>JsonUtility.ToJson(c.PaperMotion)),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
        [Test]
        public void HighQualityCompressionKeepsNewChangedAndLowPrecisionArtworkInOriginalFormat()
        {
            string accepted=AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Project/Art/PaperEnsemble"}).Single());
            string Hash(string path){using(var sha=SHA256.Create())return System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
            Assert.That(WindowsTextureQualityPolicy.Allows(accepted,Hash(accepted)),Is.True);
            Assert.That(WindowsTextureQualityPolicy.Allows(accepted,new string('0',64)),Is.False,"A replacement image needs a new quality measurement");
            Assert.That(WindowsTextureQualityPolicy.Allows("Assets/_Project/Art/new_unmeasured.png",new string('0',64)),Is.False);
            foreach(string source in new[]{"Assets/_Project/Art/Sprites/toma/pulse.png","Assets/_Project/Art/PaperAction/akatsuki_effect_sand.png"})
                Assert.That(WindowsTextureQualityPolicy.Allows(source,Hash(source)),Is.False,"Precision-sensitive effects stay in their original format");
        }
    }
}
