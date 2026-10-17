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
    public sealed class ShiroActionArtworkTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        [Test]
        public void ShiroTwelveSlotsReuseTenOriginalsAndPreserveBasicPoses()
        {
            var data=Setup.Character(4);var paper=data.PaperMotion;var poses=States.Select(s=>paper.Find(s)).ToArray();
            Assert.That(data.ActionArtworkSchemaVersion,Is.EqualTo(1));
            Assert.That(poses.Sum(p=>p.Frames.Length),Is.EqualTo(12));Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(10));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(poses[2].Frames[0],Is.SameAs(poses[1].Frames[0]));
            Assert.That(poses[6].Frames[1],Is.SameAs(poses[5].Frames[1]));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(pose.Key=="down"||pose.Key=="air"?1:2));
                foreach(var sprite in pose.Frames)
                {
                    Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001f));Assert.That(sprite.pivot,Is.EqualTo(new Vector2(652,122)));
                }
            }
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single().name,Does.StartWith("shiro_"));
        }
        [Test]
        public void ShiroSelectedBodyAndEffectBytesReachUnityAtNativeResolution()
        {
            const string root="../art_source/paper_roster_action/shiro/";
            var m=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText(root+"packed-manifest.json"));
            Assert.That(m.complete,Is.True);Assert.That(m.frames.Length,Is.EqualTo(12));
            foreach(var f in m.frames)
            {
                var pose=Setup.Character(4).PaperMotion.Poses.Single(p=>p.Key==f.id.Substring(0,f.id.LastIndexOf('_')));
                string path=AssetDatabase.GetAssetPath(pose.Frames[f.id.EndsWith("prepare")||pose.Frames.Length==1?0:1]);
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+f.prepared)),f.id);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));Assert.That(importer.mipmapEnabled,Is.False);
            }
            var e=JsonUtility.FromJson<PaperRosterEffectArtworkBuilder.Manifest>(File.ReadAllText(root+"effects-manifest.json"));
            foreach(var job in e.jobs)
            {
                var entry=e.entries.Single(a=>a.id==job.id&&a.version==job.version);
                string path="Assets/_Project/Art/PaperAction/shiro_effect_"+entry.id+".png";
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+entry.source)));
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.That(sprite.texture.width,Is.EqualTo(entry.native.width));Assert.That(sprite.texture.height,Is.EqualTo(entry.native.height));
            }
        }
        [Test]
        public void AlmaBinaryPreludeSwitchesAtActualDelayedDamageWindowAndEmpKeepsItsWindow()
        {
            var data=Setup.Character(4);var paper=data.PaperMotion;var emp=paper.AbilityEffect(AbilityLook.Emp);var alma=paper.AbilityEffect(AbilityLook.Alma);
            Assert.That(data.Specials[0].Definition.StartupFrames,Is.EqualTo(18));Assert.That(data.Specials[0].Definition.ActiveFrames,Is.EqualTo(8));
            Assert.That(data.Specials[0].Definition.Hitbox.Size,Is.EqualTo(new BattleVector(6,4)));
            Assert.That(data.Specials[0].Definition.ParalysisFrames,Is.EqualTo(12));
            Assert.That(data.Ultimate.Definition.StartupFrames,Is.EqualTo(24));Assert.That(data.Ultimate.Definition.DamageDelayFrames,Is.EqualTo(24));
            Assert.That(data.Ultimate.Definition.Hitbox.Size,Is.EqualTo(new BattleVector(9,5)));
            Assert.That(alma.AnticipationSprite,Is.Not.Null.And.Not.SameAs(alma.Sprite));
            Assert.That(alma.AnticipationText.Split('\n').Length,Is.EqualTo(4));
            Assert.That(alma.AnticipationText.All(c=>c=='0'||c=='1'||c==' '||c=='\n'),Is.True);
            Assert.That(alma.AlternateAnticipationText,Is.Not.EqualTo(alma.AnticipationText));
            var sample=new AnimationSample{State=AnimationState.Ultimate,IsAttack=true,Startup=24,Active=36,Recovery=30,DamageDelay=24};
            foreach(int frame in new[]{0,23,24,47}){sample.AttackFrame=frame;Assert.That(alma.Visible(sample),Is.True);Assert.That(alma.SpriteFor(sample),Is.SameAs(alma.AnticipationSprite));}
            foreach(int frame in new[]{48,59,60,89}){sample.AttackFrame=frame;Assert.That(alma.Visible(sample),Is.True);Assert.That(alma.SpriteFor(sample),Is.SameAs(alma.Sprite));Assert.That(alma.Anticipating(sample),Is.False);}
            sample.State=AnimationState.SpecialNeutral;sample.Startup=18;sample.Active=8;sample.DamageDelay=0;
            foreach(int frame in new[]{0,17,18,25,26}){sample.AttackFrame=frame;Assert.That(emp.Visible(sample),Is.EqualTo(frame>=18&&frame<26));}
            sample.State=AnimationState.Hitstun;Assert.That(emp.Visible(sample),Is.False);
        }
        [Test]
        public void WebCopyKeepsAlmaBothPhasesAndDoesNotMutateSource()
        {
            var source=Setup.Character(4).PaperMotion;string before=JsonUtility.ToJson(source);
            try
            {
                var copy=WebGLScenePacking.CopyForBuild(Setup).Character(4).PaperMotion;
                foreach(var art in source.AbilityEffects)
                {
                    var cloned=copy.AbilityEffect(art.State);Assert.That(cloned.Sprite,Is.SameAs(art.Sprite));
                    if(art.AnticipationSprite)Assert.That(cloned.AnticipationSprite,Is.SameAs(art.AnticipationSprite));
                    else Assert.That(!cloned.AnticipationSprite,Is.True,"予備なしのEMPはUnity nullを維持する");
                    Assert.That(cloned.AnticipationText,Is.EqualTo(art.AnticipationText));
                }
                Assert.That(JsonUtility.ToJson(source),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
    }
}
