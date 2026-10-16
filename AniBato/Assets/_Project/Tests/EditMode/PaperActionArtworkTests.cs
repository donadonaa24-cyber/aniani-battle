using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperActionArtworkTests
    {
        private static CharacterData Toma => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset").Character(0);
        [Test]
        public void TomaSharesFirstTwoNormalKeysAndKeepsFourteenDistinctAttackImagesAlongsideCorePoses()
        {
            var data=Toma; var paper=data.PaperMotion;
            Assert.That(data.ActionArtworkSchemaVersion,Is.EqualTo(1)); Assert.That(data.ActionArtworkPending,Is.True);
            string[] keys={"normal1","normal2","normal3","up","down","air","special","super"};
            var poses=keys.Select(k=>paper.Poses.Single(p=>p.Key==k)).ToArray();
            Assert.That(poses.SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(14));
            Assert.That(poses[1].Frames,Is.EqualTo(poses[0].Frames));
            Assert.That(poses.Take(3).SelectMany(p=>p.Frames).Distinct().Count(),Is.EqualTo(4));
            foreach(var pose in poses)
            {
                Assert.That(pose.Valid,Is.True); Assert.That(pose.Frames.Length,Is.EqualTo(2));
                Assert.That(pose.FrameHeadHeights.Length,Is.EqualTo(2)); Assert.That(pose.FrameOriginYs.Length,Is.EqualTo(2));
                foreach(var sprite in pose.Frames)
                {
                    Assert.That(sprite.texture.width,Is.EqualTo(1304)); Assert.That(sprite.texture.height,Is.EqualTo(1450));
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001));
                    Assert.That(sprite.pivot.x,Is.EqualTo(652).Within(.001)); Assert.That(sprite.pivot.y,Is.EqualTo(122).Within(.001));
                }
            }
            foreach(string key in new[]{"guard","crouch","counter","win","lose"})Assert.That(paper.Poses.Count(p=>p.Key==key),Is.EqualTo(1));
            Assert.That(paper.Poses.SelectMany(p=>p.States).Distinct().Count(),Is.EqualTo(paper.Poses.Sum(p=>p.States.Length)));
            Assert.That(paper.Find(AnimationState.BackAttack).ReverseFacing,Is.True);
            Assert.That(paper.Find(AnimationState.BackAttack).Frames,Is.EqualTo(paper.Find(AnimationState.Attack1).Frames));
            Assert.That(paper.Supports(AnimationState.Dodge),Is.True);
            Assert.That(paper.Find(AnimationState.Dodge).Frames.Single(),Is.SameAs(paper.Sprite),"未生成姿勢は本人の紙待機、完成した専用原画とは区別");
        }
        [Test]
        public void TomaImportedBodiesAndProjectilesAreByteIdenticalToSelectedSources()
        {
            var manifest=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText(PaperActionArtworkBuilder.Root+"packed-manifest.json"));
            Assert.That(manifest.complete,Is.True); Assert.That(manifest.frames.Length,Is.EqualTo(16));
            foreach(var f in manifest.frames)
            {
                string path="Assets/_Project/Art/PaperAction/toma_"+f.id+".png";
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("../"+f.prepared)));
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None)); Assert.That(importer.mipmapEnabled,Is.False);
            }
            var effects=JsonUtility.FromJson<PaperActionArtworkBuilder.Effects>(File.ReadAllText(PaperActionArtworkBuilder.Root+"effects-manifest.json"));
            foreach(var e in effects.entries)
                Assert.That(File.ReadAllBytes("Assets/_Project/Art/PaperAction/toma_effect_"+e.id+".png"),Is.EqualTo(File.ReadAllBytes("../"+e.source)));
        }
        [Test]
        public void TwoKeyPoseSwitchesAtDamageStartHoldsRecoveryAndRespectsFreezeSample()
        {
            var paper=Toma.PaperMotion; var pose=paper.Find(AnimationState.Attack1);
            var sample=new AnimationSample{IsAttack=true,State=AnimationState.Attack1,Startup=5,Active=6,Recovery=12,DamageDelay=3};
            for(int tick=0;tick<23;tick++)
            {
                sample.AttackFrame=tick; sample.Age=tick;
                Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[tick<8?0:1]));
                for(int render=0;render<4;render++)Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[tick<8?0:1]));
                var performance=PaperPerformance.Evaluate(paper.Performance,sample);
                if(tick<8)Assert.That(performance.Degrees,Is.EqualTo(paper.Performance.AnticipationDegrees));
            }
            var air=paper.Find(AnimationState.AirNeutral);
            Assert.That(air.FrameOriginYs.All(y=>y>0&&y<.3f),Is.True);
            Assert.That(air.FrameHeadHeights.Zip(air.FrameOriginYs,(h,y)=>h+y).All(h=>Mathf.Abs(h-paper.StandingHeight)<.001f),Is.True);
        }
        [Test]
        public void ProjectileArtFitsItsOwnPhysicsBoxWithUniformScaleAndVisibleCenterPivot()
        {
            var data=Toma; var effects=JsonUtility.FromJson<PaperActionArtworkBuilder.Effects>(File.ReadAllText(PaperActionArtworkBuilder.Root+"effects-manifest.json"));
            var attacks=new[]{data.GroundNormals[0].Definition,data.Specials[0].Definition,data.Ultimate.Definition};
            Assert.That(data.PaperMotion.Projectiles.Length,Is.EqualTo(3));
            foreach(var attack in attacks)
            {
                var art=data.PaperMotion.Projectile(attack.Ability.Look);Assert.That(art,Is.Not.Null);
                var e=effects.entries.Single(e=>e.look==attack.Ability.Look.ToString());var b=e.native.bounds;
                float w=b.right-b.left+1,h=b.bottom-b.top+1;
                Assert.That(w/art.Sprite.pixelsPerUnit*art.Scale,Is.LessThanOrEqualTo(attack.Hitbox.Size.X+.0001f));
                Assert.That(h/art.Sprite.pixelsPerUnit*art.Scale,Is.LessThanOrEqualTo(attack.Hitbox.Size.Y+.0001f));
                Assert.That(art.Sprite.pivot.x,Is.EqualTo((b.left+b.right+1)*.5f).Within(.001));
                Assert.That(art.Sprite.pivot.y,Is.EqualTo(e.native.height-(b.top+b.bottom+1)*.5f).Within(.001));
            }
            Assert.That(data.PaperMotion.Projectile(AbilityLook.Rocket).RotationOffsetDegrees,Is.EqualTo(90));
            Assert.That(data.PaperMotion.Projectile(AbilityLook.Drone).FlipWithFacing,Is.True);
        }
    }
}
