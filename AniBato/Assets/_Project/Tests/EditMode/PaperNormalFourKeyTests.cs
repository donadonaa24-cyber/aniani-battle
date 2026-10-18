using System;
using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperNormalFourKeyTests
    {
        private static BattleSetup Setup=>AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        [Test]
        public void CompletedNormalSetsShareExactlyFourNativeSpritesWithoutSharingAttackDefinitions()
        {
            foreach(int index in new[]{0,1,2})
            {
                var data=Setup.Character(index);var paper=data.PaperMotion;
                var first=paper.Find(AnimationState.Attack1);var second=paper.Find(AnimationState.Attack2);var third=paper.Find(AnimationState.Attack3);
                Assert.That(first.Frames.Length,Is.EqualTo(2),data.name);Assert.That(second.Frames.Length,Is.EqualTo(2));Assert.That(third.Frames.Length,Is.EqualTo(2));
                for(int phase=0;phase<2;phase++)Assert.That(second.Frames[phase],Is.SameAs(first.Frames[phase]));
                Assert.That(first.Frames.Concat(second.Frames).Concat(third.Frames).Distinct().Count(),Is.EqualTo(4));
                Assert.That(data.GroundNormals[1],Is.Not.SameAs(data.GroundNormals[0]),"判定と発生は各段独立");
                Assert.That(first.Frames.All(s=>s.texture.width==1304&&s.texture.height==1450),Is.True);
                Assert.That(third.Frames.All(s=>s.texture.width==1304&&s.texture.height==1450),Is.True);
            }
        }
        [Test]
        public void TomaAndRenCoverEveryAnimationStateAndUseTheirOwnIdleForUnmadeActions()
        {
            foreach(int index in new[]{0,1})
            {
                var paper=Setup.Character(index).PaperMotion;
                foreach(AnimationState state in Enum.GetValues(typeof(AnimationState)))
                {
                    Assert.That(paper.Supports(state),Is.True,state.ToString());
                    Assert.That(paper.Poses.Count(p=>p.States.Contains(state)),Is.EqualTo(1));
                }
                bool dedicated = AniBato.EditorTools.PaperRosterMotionBuilder.Completed().Any(m=>m.characterIndex==index);
                foreach(var state in new[]{AnimationState.Dash,AnimationState.Dodge,AnimationState.Jump,AnimationState.Fall,AnimationState.Hitstun,AnimationState.Knockback})
                    Assert.That(paper.Find(state).Frames.Single(),dedicated?Is.Not.SameAs(paper.Sprite):Is.SameAs(paper.Sprite),state.ToString());
                Assert.That(paper.Find(AnimationState.Guard).Frames.Single(),Is.Not.SameAs(paper.Sprite));
            }
        }
    }
}
