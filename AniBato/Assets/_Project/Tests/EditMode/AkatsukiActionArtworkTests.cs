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
    public sealed class AkatsukiActionArtworkTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static CharacterData Data=>Setup.Character(7);
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        [Test] public void AkatsukiTwelveSlotsShareNineNativeKeysAndKeepFiveBasicPoses()
        {
            var paper=Data.PaperMotion;var poses=States.Select(s=>paper.Find(s)).ToArray();
            Assert.That(Data.ActionArtworkSchemaVersion,Is.EqualTo(1));Assert.That(poses.Sum(p=>p.Frames.Length),Is.EqualTo(12));
            Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(9));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(poses[0].Frames));Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(poses[2].Frames[0],Is.SameAs(poses[0].Frames[0]));Assert.That(poses[5].Frames,Is.EqualTo(poses[1].Frames));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True);Assert.That(pose.Frames.Length,Is.EqualTo(pose.Key=="down"||pose.Key=="air"?1:2));
                Assert.That(pose.SwitchOnProjectileRelease,Is.EqualTo(pose.Key=="normal3"||pose.Key=="special"));
                foreach(var sprite in pose.Frames)
                {Assert.That(sprite.texture.width,Is.EqualTo(1304));Assert.That(sprite.texture.height,Is.EqualTo(1450));Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001f));Assert.That(sprite.pivot,Is.EqualTo(new Vector2(652,122)));}
            }
            foreach(var state in new[]{AnimationState.Guard,AnimationState.Crouch,AnimationState.Counter,AnimationState.Win,AnimationState.Lose})
                Assert.That(paper.Find(state).Frames.Single().name,Does.StartWith("akatsuki_"));
        }
        [Test] public void SelectedAkatsukiBodiesAndFourEffectsAreCopiedWithoutResampling()
        {
            const string root="../art_source/paper_roster_action/akatsuki/";var paper=Data.PaperMotion;
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
            Assert.That(effects.jobs.Length,Is.EqualTo(6));
            var selected=effects.jobs.Select(j=>effects.entries.Single(e=>e.id==j.id&&e.version==j.version)).ToArray();
            Assert.That(selected.Select(e=>e.sha256).Distinct().Count(),Is.EqualTo(4));
            foreach(var e in selected)
            {
                var sprite=e.kind=="projectile"?paper.Projectile((AbilityLook)Enum.Parse(typeof(AbilityLook),e.look)).Sprite:paper.AbilityEffect((AnimationState)Enum.Parse(typeof(AnimationState),e.state)).Sprite;
                Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)),Is.EqualTo(File.ReadAllBytes("../"+e.source)),e.id);
            }
        }
        [Test] public void CardKeepsSpecialItemSizeForNormalThrowAndPyramidStaysUpright()
        {
            var paper=Data.PaperMotion;var card=paper.Projectile(AbilityLook.Card);var pyramid=paper.Projectile(AbilityLook.Pyramid);
            Assert.That(card.FitToHitbox,Is.False);Assert.That(card.ScaleFor(Data.GroundNormals[2].Definition.Hitbox.Size),Is.EqualTo(card.ScaleFor(Data.Specials[0].Definition.Hitbox.Size)));
            Assert.That(card.VisibleSize.y*card.Scale,Is.EqualTo(.4f).Within(.0001f));
            Assert.That(card.FlipWithFacing||card.RotateWithVelocity,Is.False);Assert.That(pyramid.FlipWithFacing||pyramid.RotateWithVelocity,Is.False);
            Assert.That(pyramid.VisibleSize.x*pyramid.Scale,Is.EqualTo(2.8f).Within(.0001f));
            Assert.That(paper.AbilityEffect(AbilityLook.Default),Is.Null);
            foreach(var state in new[]{AnimationState.Attack1,AnimationState.Attack2,AnimationState.BackAttack})
            {var a=paper.AbilityEffect(state);Assert.That(a.Sprite,Is.SameAs(paper.AbilityEffect(AnimationState.Attack1).Sprite));Assert.That(a.DuringStartup,Is.False);Assert.That(a.DuringRecovery,Is.True);}
            Assert.That(paper.AbilityEffect(AnimationState.Attack3),Is.Null);
            Assert.That(paper.AbilityEffect(AnimationState.Ultimate).DuringStartup,Is.True);Assert.That(paper.AbilityEffect(AnimationState.Ultimate).DuringRecovery,Is.False);
        }
        [Test] public void WebCopyPreservesReleaseTimingAndItemArtworkWithoutMutatingSource()
        {
            var source=Data.PaperMotion;string before=JsonUtility.ToJson(source);
            try
            {
                var copy=WebGLScenePacking.CopyForBuild(Setup).Character(7).PaperMotion;
                Assert.That(copy.Find(AnimationState.Attack3).SwitchOnProjectileRelease,Is.True);Assert.That(copy.Find(AnimationState.SpecialNeutral).SwitchOnProjectileRelease,Is.True);
                Assert.That(copy.Projectile(AbilityLook.Card).Sprite,Is.SameAs(source.Projectile(AbilityLook.Card).Sprite));
                Assert.That(copy.Projectile(AbilityLook.Pyramid).Scale,Is.EqualTo(source.Projectile(AbilityLook.Pyramid).Scale));Assert.That(JsonUtility.ToJson(source),Is.EqualTo(before));
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
        [TestCase(1)] [TestCase(-1)] public void NormalCardFlightKeepsOriginalThreeActiveTicksAndTunedFarthestReach(int facing)
        {
            var move=Data.GroundNormals[2].Definition;
            Assert.That(move.SpawnProjectile&&move.Ability.IndependentNormalProjectile&&move.Ability.ProjectileAtDamageFrame,Is.True);
            Assert.That(move.IsNormal,Is.True);Assert.That(move.StartupFrames,Is.EqualTo(7));Assert.That(move.ActiveFrames,Is.EqualTo(3));Assert.That(move.RecoveryFrames,Is.EqualTo(12));
            Assert.That(move.NormalForwardDistance,Is.EqualTo(.18f));Assert.That(move.Hitbox.Size.Y,Is.EqualTo(1.8f));
            var sim=BattleTestFactory.Create(false,definition:Data.ToDefinition());sim.P1.Position=new BattleVector(0,sim.P1.Position.Y);sim.P1.Facing=facing;
            sim.P2.Position=new BattleVector(facing*15,sim.P2.Position.Y);
            sim.P1.Attack=new AttackInstance(100,move,2,false){Frame=-1};sim.P1.Mode=FighterMode.Attack;
            int visible=0;float far=0;
            for(int tick=0;tick<move.StartupFrames+move.ActiveFrames+1;tick++)
            {
                sim.Tick(default,default);
                Assert.That(sim.Match.IsFinished,Is.False,"射程の測定中も両者は場内で試合を継続する");
                if(sim.Projectiles.Count==0)continue;
                var shot=sim.Projectiles.Single();visible++;
                float edge=facing>0?shot.Hitbox.Right:-shot.Hitbox.Left;far=Mathf.Max(far,edge);
                if(visible==1){float near=facing>0?shot.Hitbox.Left:-shot.Hitbox.Right;Assert.That(near,Is.EqualTo(.3f+facing*sim.P1.Position.X).Within(.0001f));}
            }
            Assert.That(visible,Is.EqualTo(3));Assert.That(far,Is.EqualTo(1.8f+.18f).Within(.0001f));Assert.That(sim.Projectiles.Count,Is.Zero);
            Assert.That(facing*sim.P1.Position.X,Is.EqualTo(.18f).Within(.0001f));
            string once=JsonUtility.ToJson(move);Assert.That(JsonUtility.ToJson(AkatsukiCardThrowBuilder.Convert(move)),Is.EqualTo(once));
        }
        [Test] public void NormalCardDamagesOnceAndStillClashesWithAnotherNormalCard()
        {
            var move=Data.GroundNormals[2].Definition;var sim=BattleTestFactory.Create(false,definition:Data.ToDefinition());
            sim.P1.Position=new BattleVector(0,sim.P1.Position.Y);sim.P2.Position=new BattleVector(1.5f,sim.P2.Position.Y);
            sim.P1.Attack=new AttackInstance(100,move,2,false){Frame=6};sim.P1.Mode=FighterMode.Attack;
            BattleTestFactory.Idle(sim,30);Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));Assert.That(sim.P2.Hearts,Is.EqualTo(3));
            var rules=PhaseSixADefaults.Rules();rules.CombatInput=CombatInputDefinition.Default;
            sim=BattleTestFactory.Create(false,rules,Data.ToDefinition());sim.P1.Position=new BattleVector(-1,sim.P1.Position.Y);sim.P2.Position=new BattleVector(1,sim.P2.Position.Y);
            sim.P1.Facing=1;sim.P2.Facing=-1;
            sim.P1.Attack=new AttackInstance(100,move,2,false){Frame=6};sim.P2.Attack=new AttackInstance(101,move,2,false){Frame=6};
            sim.P1.Mode=sim.P2.Mode=FighterMode.Attack;sim.Tick(default,default);
            Assert.That(sim.Events.Count(e=>e.Kind==BattleEventKind.Clash),Is.EqualTo(1));Assert.That(sim.Projectiles.Count,Is.Zero);
            Assert.That(sim.P1.NormalHitCount+sim.P2.NormalHitCount,Is.Zero);Assert.That(sim.P1.Hearts+sim.P2.Hearts,Is.EqualTo(6));
        }
    }
}
