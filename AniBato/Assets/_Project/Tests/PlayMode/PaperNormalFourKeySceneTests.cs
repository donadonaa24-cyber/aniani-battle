using System;
using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator CompletedRosterUsesSharedFirstTwoKeysAndLargeThirdThroughActualComboInputs()
        {
            var keys=controller.Bindings.P1;
            foreach(int index in new[]{1,2,3,4,5})foreach(int facing in new[]{1,-1})
            {
                var data=runner.Setup.Character(index);runner.BeginMatch(data,runner.Setup.Character(0),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P2,"Position",new BattleVector(sim.Stage[0].Bounds.Right-1,sim.P2.Position.Y));
                yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);Tick();
                for(int stage=0;stage<3;stage++)
                {
                    Assert.That(sim.P1.Attack,Is.Not.Null);var attack=sim.P1.Attack;Assert.That(attack.ChainIndex,Is.EqualTo(stage));
                    var state=(AnimationState)((int)AnimationState.Attack1+stage);var pose=data.PaperMotion.Find(state);
                    bool queued=false,prep=false,active=false,recovery=false;int limit=0;
                    while(sim.P1.Attack==attack&&limit++<100)
                    {
                        Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.RenderedSample.State,Is.EqualTo(state));Assert.That(view.UsesPaperSprite,Is.True);
                        bool anticipation=attack.Frame<attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                        Assert.That(body.sprite,Is.SameAs(pose.Frames[anticipation?0:1]));Assert.That(body.flipX,Is.EqualTo(facing<0));
                        if(index==5)
                        {
                            var art=data.PaperMotion.AbilityEffect(state);var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();
                            Assert.That(effect.enabled,Is.EqualTo(!anticipation));if(effect.enabled)Assert.That(effect.sprite,Is.SameAs(art.Sprite));
                        }
                        string name=data.Movement.Name+"_"+stage+"_"+facing;
                        if(anticipation&&!prep){prep=true;Capture(name+"_prepare",true,"rework_fourkeys");}
                        if(attack.IsActive&&!active)
                        {
                            active=true;Capture(name+"_impact",true,"rework_fourkeys");
                            if(index==5)CaptureActionEffects("rework_r4_yui",name+"_effect",body,view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>());
                        }
                        if(!anticipation&&!attack.IsActive&&!recovery){recovery=true;Capture(name+"_recovery",true,"rework_fourkeys");}
                        if(stage<2&&!queued&&attack.IsActive){yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);queued=true;}
                        Tick();
                    }
                    Assert.That(limit,Is.LessThan(100));Assert.That(prep&&active&&recovery,Is.True,"予備・有効・硬直を実入力で確認");
                }
                Assert.That(sim.P1.Attack,Is.Null);Assert.That(view.RenderedSample.State,Is.EqualTo(AnimationState.Idle));Assert.That(body.sprite,Is.SameAs(data.PaperMotion.Sprite));
            }
        }
        [UnityTest]
        public IEnumerator TomaAndRenNeverRenderLegacyBodySpritesForAnyAnimationStateInEitherDirection()
        {
            foreach(int index in new[]{0,1})
            {
                var data=runner.Setup.Character(index);runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);
                var player=((FighterAnimationPlayer[])typeof(BattleRunner).GetField("animations",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(runner))[0];
                var property=typeof(FighterAnimationPlayer).GetProperty("Sample");var original=player.Sample;
                int tick=runner.Simulation.Match.TickCount;var position=runner.Simulation.P1.Position;
                try
                {
                    foreach(bool flip in new[]{false,true})foreach(AnimationState state in Enum.GetValues(typeof(AnimationState)))
                    {
                        var sample=new AnimationSample{State=state,FlipX=flip};property.SetValue(player,sample);view.RenderFrame();
                        Assert.That(view.UsesPaperSprite,Is.True,state.ToString());Assert.That(body.sprite,Is.SameAs(data.PaperMotion.Find(state).Frame(sample)));
                        Assert.That(body.flipX,Is.EqualTo(flip^data.PaperMotion.Find(state).ReverseFacing));
                        if(new[]{AnimationState.Jump,AnimationState.Dodge,AnimationState.Hitstun}.Contains(state))Capture(data.Movement.Name+"_fallback_"+state+"_"+flip,true,"rework_fourkeys");
                    }
                    Assert.That(runner.Simulation.Match.TickCount,Is.EqualTo(tick));Assert.That(runner.Simulation.P1.Position,Is.EqualTo(position));
                }
                finally{property.SetValue(player,original);view.RenderFrame();}
            }
            yield return null;
        }
    }
}
