using System.IO;
using System.Linq;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class RenActionArtworkTests
    {
        private static CharacterData Ren=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset").Character(1);
        [Test]
        public void RenHasFourteenNativeKeysAndKeepsHisCoreAndUnmadeMotionFallbacks()
        {
            var data=Ren;var paper=data.PaperMotion;
            Assert.That(data.ActionArtworkSchemaVersion,Is.EqualTo(1));Assert.That(data.ActionArtworkPending,Is.True);
            var states=new[]{AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,AnimationState.CrouchAttack,
                AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
            var poses=states.Select(state=>paper.Find(state)).ToArray();Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(14));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(2));
                Assert.That(pose.FrameHeadHeights.Length,Is.EqualTo(2));Assert.That(pose.FrameOriginYs.Length,Is.EqualTo(2));
                foreach(var sprite in pose.Frames)
                {
                    Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001));
                    Assert.That(sprite.pivot.x,Is.EqualTo(652).Within(.001));Assert.That(sprite.pivot.y,Is.EqualTo(122).Within(.001));
                }
            }
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(paper.Find(AnimationState.Attack1).Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(paper.Find(AnimationState.Attack1).Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single(),Is.Not.SameAs(paper.Sprite));
            foreach(var state in new[]{AnimationState.Jump,AnimationState.Dash,AnimationState.Hitstun,AnimationState.Knockback})
                Assert.That(paper.Find(state).Frames.Single(),Is.SameAs(paper.Sprite));
        }
        [Test]
        public void RenRuntimePngsMatchAllSelectedPackedSourcesWithoutImportResampling()
        {
            var manifest=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText("../art_source/paper_roster_action/ren/packed-manifest.json"));
            Assert.That(manifest.complete,Is.True);Assert.That(manifest.frames.Length,Is.EqualTo(14));
            foreach(var frame in manifest.frames)
            {
                string path="Assets/_Project/Art/PaperAction/ren_"+frame.id+".png";
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+frame.prepared)),frame.id);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));Assert.That(importer.mipmapEnabled,Is.False);
            }
        }
    }
}
