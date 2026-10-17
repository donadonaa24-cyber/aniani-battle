using System;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class KingActionArtworkTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        [Test] public void KingTwelveSlotsUseNineNativeKeysAndPreserveFiveBasicPoses()
        {
            var data=Setup.Character(6);var paper=data.PaperMotion;var poses=States.Select(s=>paper.Find(s)).ToArray();
            Assert.That(data.ActionArtworkSchemaVersion,Is.EqualTo(1));Assert.That(poses.Sum(p=>p.Frames.Length),Is.EqualTo(12));
            Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(9));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(poses[0].Frames));Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(poses[2].Frames[0],Is.SameAs(poses[0].Frames[0]));Assert.That(poses[5].Frames[0],Is.SameAs(poses[0].Frames[0]));
            Assert.That(poses[6].Frames[1],Is.SameAs(poses[5].Frames[1]));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(pose.Key=="down"||pose.Key=="air"?1:2));
                Assert.That(pose.UnmirroredImpact,Is.EqualTo(pose.Key=="special"||pose.Key=="super"));Assert.That(pose.RotateWithRush,Is.False);
                foreach(var sprite in pose.Frames)
                {Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001f));Assert.That(sprite.pivot,Is.EqualTo(new Vector2(652,122)));}
            }
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single().name,Does.StartWith("king_"));
        }
        [Test] public void SelectedKingBodyAndFiveUniqueEffectsReachUnityWithoutResampling()
        {
            const string root="../art_source/paper_roster_action/king/";var paper=Setup.Character(6).PaperMotion;
            var m=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText(root+"packed-manifest.json"));
            Assert.That(m.complete,Is.True);Assert.That(m.frames.Length,Is.EqualTo(12));
            foreach(var f in m.frames)
            {
                var pose=paper.Poses.Single(p=>p.Key==f.id.Substring(0,f.id.LastIndexOf('_')));
                string path=AssetDatabase.GetAssetPath(pose.Frames[f.id.EndsWith("prepare")||pose.Frames.Length==1?0:1]);
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+f.prepared)),f.id);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
            var effects=JsonUtility.FromJson<PaperRosterEffectArtworkBuilder.Manifest>(File.ReadAllText(root+"effects-manifest.json"));
            Assert.That(effects.jobs.Length,Is.EqualTo(7));var summon=paper.Projectile(AbilityLook.King);
            foreach(var job in effects.jobs)
            {
                var e=effects.entries.Single(e=>e.id==job.id&&e.version==job.version);
                var sprite=e.kind=="projectile"?summon.SpriteFor(e.variantIndex):paper.AbilityEffect((AnimationState)Enum.Parse(typeof(AnimationState),e.state)).Sprite;
                Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)),Is.EqualTo(File.ReadAllBytes("../"+e.source)),e.id);
            }
            Assert.That(summon.Variants.Length,Is.EqualTo(3));Assert.That(summon.Variants.Distinct().Count(),Is.EqualTo(3));
            Assert.That(summon.Variants.Select(s=>s.name),Is.EqualTo(new[]{"king_effect_leila","king_effect_shion","king_effect_noa"}));
        }
        [Test] public void EachNamedSummonFitsItsLiveSpecialOrUltimateHitboxAndKeepsSharedDamage()
        {
            var data=Setup.Character(6);var art=data.PaperMotion.Projectile(AbilityLook.King);Assert.That(art.FitToHitbox,Is.True);
            Assert.That(art.FlipWithFacing,Is.False);Assert.That(art.RotateWithVelocity,Is.False);
            foreach(var move in new[]{data.Specials[0].Definition,data.Ultimate.Definition})
            {
                Assert.That(move.Ability.ProjectileCount,Is.EqualTo(3));Assert.That(move.SpawnProjectile,Is.True);
                for(int i=0;i<3;i++)
                {
                    float scale=art.ScaleFor(move.Hitbox.Size,i);var size=art.VariantVisibleSizes[i]*scale;
                    Assert.That(size.x,Is.LessThanOrEqualTo(move.Hitbox.Size.X+.00001f));Assert.That(size.y,Is.LessThanOrEqualTo(move.Hitbox.Size.Y+.00001f));
                    Assert.That(Mathf.Max(size.x/move.Hitbox.Size.X,size.y/move.Hitbox.Size.Y),Is.EqualTo(1).Within(.00001f));
                }
            }
            Assert.That(art.ScaleFor(data.Ultimate.Definition.Hitbox.Size),Is.GreaterThan(art.ScaleFor(data.Specials[0].Definition.Hitbox.Size)));
            Assert.That(data.PaperMotion.AbilityEffect(AbilityLook.Default),Is.Null);
            Assert.That(data.PaperMotion.AbilityEffect(AnimationState.Attack1).Sprite,Is.SameAs(data.PaperMotion.AbilityEffect(AnimationState.Attack2).Sprite));
        }
        [Test] public void WebCopyKeepsThreeSupportVariantsAndUnmirroredKingWithoutMutatingSource()
        {
            var source=Setup.Character(6).PaperMotion;string before=JsonUtility.ToJson(source);
            try
            {
                var copy=WebGLScenePacking.CopyForBuild(Setup).Character(6).PaperMotion;var art=copy.Projectile(AbilityLook.King);
                Assert.That(art.Variants,Is.EqualTo(source.Projectile(AbilityLook.King).Variants));Assert.That(art.VariantVisibleSizes,Is.EqualTo(source.Projectile(AbilityLook.King).VariantVisibleSizes));
                Assert.That(art.FitToHitbox,Is.True);Assert.That(copy.Find(AnimationState.SpecialNeutral).UnmirroredImpact,Is.True);
                Assert.That(copy.Find(AnimationState.Ultimate).UnmirroredImpact,Is.True);Assert.That(JsonUtility.ToJson(source),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
    }
}
