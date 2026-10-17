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
    public sealed class MinatoActionArtworkTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        [Test]
        public void TwelveMinatoSlotsReuseTenNativeImagesAndKeepCorePoses()
        {
            var data=Setup.Character(3);var paper=data.PaperMotion;var poses=States.Select(s=>paper.Find(s)).ToArray();
            Assert.That(data.ActionArtworkSchemaVersion,Is.EqualTo(1));Assert.That(data.ActionArtworkPending,Is.True);
            Assert.That(poses.Sum(p=>p.Frames.Length),Is.EqualTo(12));Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(10));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(poses[2].Frames[0],Is.SameAs(poses[1].Frames[0]),"上の予備は最大の構えを原寸再利用");
            Assert.That(poses[5].Frames[1],Is.SameAs(poses[0].Frames[1]),"瞬間移動後は同じ生身右パンチ");
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(pose.Key=="down"||pose.Key=="air"?1:2));
                Assert.That(pose.FrameHeadHeights.Length,Is.EqualTo(pose.Frames.Length));Assert.That(pose.FrameOriginYs.Length,Is.EqualTo(pose.Frames.Length));
                foreach(var sprite in pose.Frames)
                {
                    Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001f));Assert.That(sprite.pivot,Is.EqualTo(new Vector2(652,122)));
                }
            }
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single().name,Does.StartWith("minato_"));
        }
        [Test]
        public void SelectedPackedBodyAndEffectBytesReachUnityWithoutResampling()
        {
            const string root="../art_source/paper_roster_action/minato/";
            var m=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText(root+"packed-manifest.json"));
            Assert.That(m.complete,Is.True);Assert.That(m.frames.Length,Is.EqualTo(12));
            foreach(var f in m.frames)
            {
                var pose=Setup.Character(3).PaperMotion.Poses.Single(p=>p.Key==f.id.Substring(0,f.id.LastIndexOf('_')));
                int index=f.id.EndsWith("prepare")||pose.Frames.Length==1?0:1;
                string path=AssetDatabase.GetAssetPath(pose.Frames[index]);
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+f.prepared)),f.id);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));Assert.That(importer.mipmapEnabled,Is.False);
            }
            var e=JsonUtility.FromJson<PaperRosterEffectArtworkBuilder.Manifest>(File.ReadAllText(root+"effects-manifest.json"));
            foreach(var job in e.jobs)
            {
                var entry=e.entries.Single(a=>a.id==job.id&&a.version==job.version);
                string path="Assets/_Project/Art/PaperAction/minato_effect_"+entry.id+".png";
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+entry.source)));
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.That(sprite.texture.width,Is.EqualTo(entry.native.width));Assert.That(sprite.texture.height,Is.EqualTo(entry.native.height));
            }
        }
        [Test]
        public void MinatoEffectsUseTheirActualAbilityWindowsAndFieldData()
        {
            var data=Setup.Character(3);var paper=data.PaperMotion;
            var teleport=paper.AbilityEffect(AnimationState.SpecialNeutral);var mist=paper.AbilityEffect(AnimationState.Ultimate);
            Assert.That(teleport.Look,Is.EqualTo(AbilityLook.Teleport));Assert.That(teleport.DuringStartup,Is.True);Assert.That(teleport.DuringRecovery,Is.False);
            Assert.That(mist.Look,Is.EqualTo(AbilityLook.BlackHole));Assert.That(mist.DuringStartup,Is.False);Assert.That(mist.DuringRecovery,Is.True);
            var sample=new AniBato.Presentation.AnimationSample { State=AnimationState.SpecialNeutral,IsAttack=true,Startup=18,Active=5,Recovery=22 };
            foreach(int frame in new[]{0,17,18,22}){sample.AttackFrame=frame;Assert.That(teleport.Visible(sample),Is.True);}
            sample.AttackFrame=23;Assert.That(teleport.Visible(sample),Is.False);sample.State=AnimationState.Hitstun;Assert.That(teleport.Visible(sample),Is.False);
            sample.State=AnimationState.Ultimate;sample.Startup=28;sample.Active=1;sample.AttackFrame=27;Assert.That(mist.Visible(sample),Is.False);
            sample.AttackFrame=28;Assert.That(mist.Visible(sample),Is.True);sample.AttackFrame=58;Assert.That(mist.Visible(sample),Is.True);
            var field=paper.Projectile(AbilityLook.BlackHole);Assert.That(field.Sprite.name,Is.EqualTo("minato_effect_blackhole"));Assert.That(field.RotateWithVelocity,Is.False);
            var definition=data.Ultimate.Definition;Assert.That(definition.ProjectileLifetimeFrames,Is.EqualTo(180));
            Assert.That(definition.ProjectileSpeed,Is.Zero);Assert.That(definition.Hitbox.Size,Is.EqualTo(new BattleVector(5,4)));
        }
        [Test]
        public void BrowserCopiesPreserveNewEffectReferencesWithoutChangingSourceAssets()
        {
            var source=Setup.Character(3).PaperMotion;string before=JsonUtility.ToJson(source);
            try
            {
                var copy=WebGLScenePacking.CopyForBuild(Setup).Character(3).PaperMotion;
                foreach(var art in source.AbilityEffects)Assert.That(copy.AbilityEffect(art.State).Sprite,Is.SameAs(art.Sprite));
                Assert.That(copy.Projectile(AbilityLook.BlackHole).Sprite,Is.SameAs(source.Projectile(AbilityLook.BlackHole).Sprite));
                Assert.That(JsonUtility.ToJson(source),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
    }
}
