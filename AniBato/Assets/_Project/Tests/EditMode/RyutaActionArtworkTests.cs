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
    public sealed class RyutaActionArtworkTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static CharacterData Data=>Setup.Character(8);
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        [Test] public void RyutaTwelveSlotsUseTenNativeKeysAndKeepBasicPoses()
        {
            var paper=Data.PaperMotion;var poses=States.Select(s=>paper.Find(s)).ToArray();
            Assert.That(Data.ActionArtworkSchemaVersion,Is.EqualTo(1));Assert.That(poses.Sum(p=>p.Frames.Length),Is.EqualTo(12));
            Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(10));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(poses[0].Frames));Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(poses[2].Frames,Is.EqualTo(poses[1].Frames));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(pose.Key=="down"||pose.Key=="air"?1:2));
                Assert.That(pose.SwitchOnProjectileRelease||pose.RotateWithRush,Is.False);
                foreach(var sprite in pose.Frames)
                {Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001f));Assert.That(sprite.pivot,Is.EqualTo(new Vector2(652,122)));}
            }
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single().name,Does.StartWith("ryuta_"));
        }
        [Test] public void RyutaBodyAndFourEffectsKeepOriginalPixels()
        {
            const string root="../art_source/paper_roster_action/ryuta/";var paper=Data.PaperMotion;
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
            Assert.That(effects.jobs.Length,Is.EqualTo(7));
            var selected=effects.jobs.Select(j=>effects.entries.Single(e=>e.id==j.id&&e.version==j.version)).ToArray();
            Assert.That(selected.Select(e=>e.sha256).Distinct().Count(),Is.EqualTo(4));
            foreach(var e in selected)
            {
                var art=paper.AbilityEffect((AnimationState)System.Enum.Parse(typeof(AnimationState),e.state));
                var sprite=e.phase=="prepare"?art.AnticipationSprite:art.Sprite;
                Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)),Is.EqualTo(File.ReadAllBytes("../"+e.source)),e.id);
            }
        }
        [Test] public void SpellSwitchesAtDelayedDamageAndLettersKeepTheirOwnPositions()
        {
            var spell=Data.Ultimate.Definition;var bat=Data.Specials[0].Definition;var art=Data.PaperMotion.AbilityEffect(AnimationState.Ultimate);
            Assert.That(new[]{spell.StartupFrames,spell.ActiveFrames,spell.RecoveryFrames,spell.DamageDelayFrames},Is.EqualTo(new[]{34,20,30,8}));
            Assert.That(spell.Hitbox.Size,Is.EqualTo(new BattleVector(7,4.5f)));Assert.That(spell.SpawnProjectile,Is.False);
            Assert.That(new[]{bat.StartupFrames,bat.ActiveFrames,bat.RecoveryFrames},Is.EqualTo(new[]{15,5,22}));Assert.That(bat.SpawnProjectile,Is.False);
            Assert.That(art.UseAnticipationOffset&&art.UnmirroredArtwork,Is.True);
            Assert.That(art.AnticipationOffset,Is.EqualTo(new Vector2(.4f,.5f)));Assert.That(art.Offset,Is.EqualTo(new Vector2(2.5f,0)));
            var sample=new AnimationSample{State=AnimationState.Ultimate,IsAttack=true,Startup=34,Active=20,Recovery=30,DamageDelay=8};
            foreach(int frame in new[]{0,33,34,41,42,53,54,83})
            {
                sample.AttackFrame=frame;bool prep=frame<42;Assert.That(art.Visible(sample),Is.True);
                Assert.That(art.SpriteFor(sample),Is.SameAs(prep?art.AnticipationSprite:art.Sprite));
                Assert.That(art.OffsetFor(sample),Is.EqualTo(prep?art.AnticipationOffset:art.Offset));
            }
            sample.State=AnimationState.Hitstun;Assert.That(art.Visible(sample),Is.False);
            foreach(var state in new[]{AnimationState.Attack1,AnimationState.Attack2,AnimationState.Attack3,AnimationState.BackAttack})
                Assert.That(Data.PaperMotion.AbilityEffect(state).Sprite,Is.SameAs(Data.PaperMotion.AbilityEffect(AnimationState.Attack1).Sprite));
            foreach(int index in Enumerable.Range(0,8))foreach(var old in Setup.Character(index).PaperMotion.AbilityEffects??new PaperAbilityArt[0])
                Assert.That(old.UseAnticipationOffset||old.UnmirroredArtwork,Is.False,"既存の表示は従来の位置と反転を維持");
        }
        [Test] public void WebCopyKeepsSpellPhasesOffsetsAndSourceUnchanged()
        {
            var source=Data.PaperMotion;string before=JsonUtility.ToJson(source);
            try
            {
                var copy=WebGLScenePacking.CopyForBuild(Setup).Character(8).PaperMotion;
                var a=copy.AbilityEffect(AnimationState.Ultimate);var b=source.AbilityEffect(AnimationState.Ultimate);
                Assert.That(a.UnmirroredArtwork&&a.UseAnticipationOffset,Is.True);Assert.That(a.AnticipationOffset,Is.EqualTo(b.AnticipationOffset));
                Assert.That(a.Offset,Is.EqualTo(b.Offset));Assert.That(a.Sprite,Is.SameAs(b.Sprite));Assert.That(a.AnticipationSprite,Is.SameAs(b.AnticipationSprite));
                Assert.That(JsonUtility.ToJson(source),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
    }
}
