using System.IO;
using System.Linq;
using AniBato.EditorTools;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperSingleAttackKeyTests
    {
        [Test]
        public void AllNineUseOneAirAndDownImageThroughStartupActiveAndRecovery()
        {
            var setup=AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            Assert.That(setup.Characters.Length,Is.EqualTo(9));
            foreach(var data in setup.Characters)
            foreach(var state in new[]{AnimationState.CrouchAttack,AnimationState.AirNeutral})
            {
                var pose=data.PaperMotion.Find(state);
                Assert.That(pose,Is.Not.Null,data.name+state);Assert.That(pose.Frames.Length,Is.EqualTo(1));
                foreach(int tick in new[]{0,5,12,24,90})
                {
                    var sample=new AnimationSample{State=state,IsAttack=true,Age=tick,AttackFrame=tick,Startup=8,Active=7,Recovery=15};
                    Assert.That(pose.Frame(sample),Is.SameAs(pose.Frames[0]));
                    Assert.That(pose.Head(sample),Is.EqualTo(pose.FrameHeadHeights[0]));
                    Assert.That(pose.OriginY(sample),Is.EqualTo(pose.FrameOriginYs[0]));
                }
                if(data.ActionArtworkSchemaVersion==1)
                    foreach(var normal in new[]{AnimationState.Attack1,AnimationState.Attack2,AnimationState.Attack3})
                        Assert.That(data.PaperMotion.Find(normal).Frames.Length,Is.EqualTo(2));
            }
        }

        [Test]
        public void SingleKeySelectionSeparatesSharedFallbackAndIsIdempotent()
        {
            var motion=ScriptableObject.CreateInstance<PaperCharacterMotion>();var data=ScriptableObject.CreateInstance<CharacterData>();
            var texture=new Texture2D(2,2);var a=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.zero);
            var b=Sprite.Create(texture,new Rect(1,0,1,1),Vector2.zero);
            try
            {
                var shared=new PaperPose{Key="shared",States=new[]{AnimationState.Idle,AnimationState.Attack1,AnimationState.CrouchAttack,AnimationState.AirNeutral},
                    Frames=new[]{a,b},FrameHeadHeights=new[]{1f,2f},FrameFootBaseYs=new[]{3f,4f},FrameFootMinXs=new[]{5f,6f},
                    FrameFootMaxXs=new[]{7f,8f},FrameOriginYs=new[]{.1f,.2f}};
                motion.Poses=new[]{shared};data.PaperMotion=motion;
                PaperSingleAttackKeyBuilder.Apply(new[]{data});PaperSingleAttackKeyBuilder.Apply(new[]{data});
                Assert.That(motion.Find(AnimationState.Idle),Is.SameAs(shared));
                Assert.That(motion.Find(AnimationState.Attack1).Frames,Is.EqualTo(new[]{a,b}));
                Assert.That(motion.Poses.Length,Is.EqualTo(3));
                foreach(var state in new[]{AnimationState.CrouchAttack,AnimationState.AirNeutral})
                {
                    var pose=motion.Find(state);Assert.That(pose.Frames.Single(),Is.SameAs(b));
                    Assert.That(pose.HeadHeight,Is.EqualTo(2));Assert.That(pose.FootBaseY,Is.EqualTo(4));
                    Assert.That(pose.FootMinX,Is.EqualTo(6));Assert.That(pose.FootMaxX,Is.EqualTo(8));
                    Assert.That(pose.FrameOriginYs.Single(),Is.EqualTo(.2f));
                }
            }
            finally{Object.DestroyImmediate(data);Object.DestroyImmediate(motion);Object.DestroyImmediate(a);Object.DestroyImmediate(b);Object.DestroyImmediate(texture);}
        }

        [Test]
        public void RevisedEnsembleAndPanGripImportTheSelectedOriginalBytes()
        {
            var selected=JsonUtility.FromJson<PaperEnsembleBuilder.Selection>(File.ReadAllText("../art_source/title_paper/selected.json"));
            var art=AssetDatabase.LoadAssetAtPath<PaperEnsembleArt>("Assets/_Project/Data/PaperEnsemble.asset");
            Assert.That(selected.source,Is.EqualTo("art_source/title_paper/ensemble_v5.png"));
            Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(art.Artwork)),Is.EqualTo(File.ReadAllBytes("../"+selected.source)));
            Assert.That(art.Hotspots,Is.EqualTo(selected.hotspots.Select(h=>h.ToRect()).ToArray()));
            var faceCenters=new[]{new Vector2(915,350),new Vector2(450,470),new Vector2(1280,430),new Vector2(578,166),
                new Vector2(813,610),new Vector2(177,521),new Vector2(302,255),new Vector2(1170,220),new Vector2(1490,415)};
            for(int i=0;i<faceCenters.Length;i++)Assert.That(art.Hotspots[i].Contains(new Vector2(faceCenters[i].x/1672,faceCenters[i].y/941)),Is.True,"選択枠は新しい絵の本人の顔を含む: "+i);
            var chi=AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Chizuru.asset");
            Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(chi.PaperMotion.Find(AnimationState.Attack3).Frames[1])),
                Is.EqualTo(File.ReadAllBytes("../art_source/paper_roster_action/chizuru/normal3_hand_fix/prepared/horizontal_3_impact.png")));
        }
    }
}
