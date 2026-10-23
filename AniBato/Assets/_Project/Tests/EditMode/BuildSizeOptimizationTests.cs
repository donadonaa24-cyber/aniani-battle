using System.Linq;
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
            Assert.That(WindowsBuilder.Options.HasFlag(BuildOptions.Development),Is.False);
            Assert.That(WebGLScenePacking.SupportsTarget(BuildTarget.StandaloneWindows64),Is.True);
            Assert.That(WebGLSpritePacking.Enabled,Is.False,"Windows does not replace native RGBA textures with lossy GPU textures");
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
    }
}
