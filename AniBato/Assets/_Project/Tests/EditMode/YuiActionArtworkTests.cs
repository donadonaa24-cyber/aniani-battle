using System;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class YuiActionArtworkTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        [Test]
        public void YuiTwelveSlotsReuseNineNativeKeysAndPreserveBasicPoses()
        {
            var data=Setup.Character(5);var paper=data.PaperMotion;var poses=States.Select(s=>paper.Find(s)).ToArray();
            Assert.That(data.ActionArtworkSchemaVersion,Is.EqualTo(1));
            Assert.That(poses.Sum(p=>p.Frames.Length),Is.EqualTo(12));Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(9));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(poses[2].Frames[0],Is.SameAs(poses[1].Frames[0]));Assert.That(poses[5].Frames[0],Is.SameAs(poses[1].Frames[0]));
            Assert.That(poses[6].Frames[1],Is.SameAs(poses[5].Frames[1]));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(pose.Key=="down"||pose.Key=="air"?1:2));
                Assert.That(pose.RotateWithRush,Is.EqualTo(pose.Key=="special"||pose.Key=="super"));
                foreach(var sprite in pose.Frames)
                {
                    Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001f));Assert.That(sprite.pivot,Is.EqualTo(new Vector2(652,122)));
                }
            }
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single().name,Does.StartWith("yui_"));
            Assert.That(data.GroundNormals[0].Definition.Name,Does.Contain("ステッキ"));Assert.That(data.GroundNormals[2].Definition.Name,Does.Contain("魔法"));
        }
        [Test]
        public void SelectedYuiBodyAndBothGeneratedEffectsReachUnityWithoutResampling()
        {
            const string root="../art_source/paper_roster_action/yui/";
            var m=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText(root+"packed-manifest.json"));
            Assert.That(m.complete,Is.True);Assert.That(m.frames.Length,Is.EqualTo(12));
            foreach(var f in m.frames)
            {
                var pose=Setup.Character(5).PaperMotion.Poses.Single(p=>p.Key==f.id.Substring(0,f.id.LastIndexOf('_')));
                string path=AssetDatabase.GetAssetPath(pose.Frames[f.id.EndsWith("prepare")||pose.Frames.Length==1?0:1]);
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+f.prepared)),f.id);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));Assert.That(importer.mipmapEnabled,Is.False);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
            var effects=JsonUtility.FromJson<PaperRosterEffectArtworkBuilder.Manifest>(File.ReadAllText(root+"effects-manifest.json"));
            Assert.That(effects.jobs.Length,Is.EqualTo(4));
            foreach(var job in effects.jobs)
            {
                var entry=effects.entries.Single(e=>e.id==job.id&&e.version==job.version);
                var sprite=Setup.Character(5).PaperMotion.AbilityEffect((AnimationState)Enum.Parse(typeof(AnimationState),entry.state)).Sprite;
                string path=AssetDatabase.GetAssetPath(sprite);
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+entry.source)));
                Assert.That(sprite.texture.width,Is.EqualTo(entry.native.width));Assert.That(sprite.texture.height,Is.EqualTo(entry.native.height));
            }
        }
        [Test]
        public void SharedNormalEffectsStartAtImpactAndDoNotBindUnrelatedDefaultActions()
        {
            var paper=Setup.Character(5).PaperMotion;var first=paper.AbilityEffect(AnimationState.Attack1);
            Assert.That(paper.AbilityEffect(AnimationState.Attack2).Sprite,Is.SameAs(first.Sprite));
            Assert.That(paper.AbilityEffect(AnimationState.BackAttack).Sprite,Is.SameAs(first.Sprite));
            Assert.That(paper.AbilityEffect(AnimationState.Attack3).Sprite,Is.Not.SameAs(first.Sprite));
            Assert.That(paper.AbilityEffects.Select(e=>e.Sprite).Distinct().Count(),Is.EqualTo(2));
            Assert.That(paper.AbilityEffect(AbilityLook.Default),Is.Null);Assert.That(paper.AbilityEffect(AnimationState.CrouchAttack),Is.Null);
            var sample=new AnimationSample{State=AnimationState.Attack1,IsAttack=true,Startup=4,Active=3,Recovery=8};
            for(int frame=0;frame<15;frame++){sample.AttackFrame=frame;Assert.That(first.Visible(sample),Is.EqualTo(frame>=4));}
            sample.State=AnimationState.CrouchAttack;Assert.That(first.Visible(sample),Is.False);
            sample.State=AnimationState.Attack1;sample.IsAttack=false;Assert.That(first.Visible(sample),Is.False);
            Assert.That(first.Offset,Is.EqualTo(new Vector2(1.05f,.15f)));
            Assert.That(Setup.Character(5).Specials[0].Definition.Ability.Motion,Is.EqualTo(AbilityMotion.TwoPhaseRush));
        }
        [Test]
        public void WebCopyPreservesSharedEffectsAndWholePieceRotationWithoutMutatingSource()
        {
            var source=Setup.Character(5).PaperMotion;string before=JsonUtility.ToJson(source);
            try
            {
                var copy=WebGLScenePacking.CopyForBuild(Setup).Character(5).PaperMotion;
                foreach(var art in source.AbilityEffects)Assert.That(copy.AbilityEffect(art.State).Sprite,Is.SameAs(art.Sprite));
                Assert.That(copy.Find(AnimationState.SpecialNeutral).RotateWithRush,Is.True);Assert.That(copy.Find(AnimationState.Ultimate).RotateWithRush,Is.True);
                Assert.That(JsonUtility.ToJson(source),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
    }
}
