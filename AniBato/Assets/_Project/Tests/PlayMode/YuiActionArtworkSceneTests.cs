using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator YuiActionsFollowActualInputAndBothRushPhasesWithoutMirroringThePieceGlyph()
        {
            var data=runner.Setup.Character(5);var keys=controller.Bindings.P1;
            var states=new[]{AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.AirNeutral,
                AnimationState.SpecialNeutral,AnimationState.Ultimate,AnimationState.BackAttack};
            foreach(int facing in new[]{1,-1})for(int action=0;action<states.Length;action++)
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));
                Set(sim.P2,"Position",new BattleVector(facing*6,35));Set(sim.P2,"Grounded",false);Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                if(action==2){Set(sim.P1,"Position",new BattleVector(0,20));Set(sim.P1,"Grounded",false);}
                var direction=action==0?keys.Up:action==1?keys.Down:action==5?(facing>0?keys.Left:keys.Right):UnityEngine.InputSystem.Key.None;
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,true);
                var button=action==3?keys.Special:action==4?keys.Ultimate:keys.Attack;
                yield return Key(button,true);Tick();yield return Key(button,false);if(button==keys.Attack&&action!=2)Tick();
                Assert.That(sim.P1.Attack,Is.Not.Null);var attack=sim.P1.Attack;int wait=0;
                while(view.RenderedSample.State!=states[action]&&wait++<data.Movement.UltimateCutInFrames+2)Tick();
                Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,false);
                var pose=data.PaperMotion.Find(states[action]);var art=data.PaperMotion.AbilityEffect(states[action]);
                var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();var text=view.transform.Find("Ability Cue").GetComponent<TextMesh>();
                int prep=0,active=0,recovery=0,limit=0,gravity=0;bool p=false,a=false,r=false;bool[] phases={false,false};string name=pose.Key+"_"+facing;
                var start=sim.P1.Position;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[pose.Frames.Length==1?0:impact?1:0]));
                    bool piece=pose.RotateWithRush&&impact;
                    Assert.That(body.flipX,Is.EqualTo(!piece&&(view.RenderedSample.FlipX^pose.ReverseFacing)));
                    Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
                    Assert.That(effect.enabled,Is.EqualTo(art!=null&&art.Visible(view.RenderedSample)));
                    if(effect.enabled)
                    {
                        Assert.That(effect.sprite,Is.SameAs(art.Sprite));Assert.That(effect.flipX,Is.EqualTo(body.flipX));
                        Assert.That(effect.sortingOrder,Is.LessThan(body.sortingOrder));
                        Assert.That(effect.transform.position,Is.EqualTo(body.bounds.center+new Vector3((body.flipX?-1:1)*art.Offset.x,art.Offset.y,0)));
                    }
                    Assert.That(text.gameObject.activeSelf,Is.EqualTo((action==3||action==4)&&!impact));
                    if(!impact){prep++;if(!p){Capture(name+"_prepare",true,"rework_r4_yui");p=true;}}
                    else if(attack.IsActive)
                    {
                        active++;if(!a){Capture(name+"_impact",true,"rework_r4_yui");if(art!=null)CaptureActionEffects("rework_r4_yui",name+"_effect",body,effect);a=true;}
                        if(piece)
                        {
                            float expected=Mathf.Atan2(sim.P1.Velocity.Y,sim.P1.Velocity.X)*Mathf.Rad2Deg-90;
                            Assert.That(Mathf.DeltaAngle(expected,Angle(body)),Is.EqualTo(0).Within(.001f));
                            int phase=attack.Frame-attack.Definition.StartupFrames<attack.Definition.Ability.ChangeFrame?0:1;
                            if(!phases[phase])
                            {
                                Assert.That(Mathf.Abs(sim.P1.Velocity.X),Is.GreaterThan(0));
                                if(phase==0)Assert.That(sim.P1.Velocity.Y,Is.GreaterThan(0));
                                else Assert.That(sim.P1.Velocity.Y,Is.LessThan(0));
                                Capture(name+"_rush_"+phase,true,"rework_r4_yui");CaptureActionEffects("rework_r4_yui",name+"_piece_"+phase,body);phases[phase]=true;
                            }
                        }
                    }
                    else{recovery++;if(!r){Capture(name+"_recovery",true,"rework_r4_yui");r=true;}}
                    var position=sim.P1.Position;int tick=sim.Match.TickCount;var rotation=body.transform.localRotation;var offset=body.transform.localPosition;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(body.transform.localRotation,Is.EqualTo(rotation));Assert.That(body.transform.localPosition,Is.EqualTo(offset));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(tick));
                    float vy=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravity++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravity,Is.GreaterThan(10));
                if(action==3||action==4){Assert.That(phases,Is.EqualTo(new[]{true,true}));Assert.That(sim.P1.Position,Is.Not.EqualTo(start));}
                if(action==4)
                {
                    int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1)
                    {Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();}
                    Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));
                }
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames.Last()));Assert.That(effect.enabled,Is.False);Assert.That(text.gameObject.activeSelf,Is.False);
                Capture(name+"_released",true,"rework_r4_yui");
            }
        }
        [UnityTest]
        public IEnumerator YuiNormalPaperEffectCanRemainDuringRecoveryWithoutAnExtraHit()
        {
            var data=runner.Setup.Character(5);var keys=controller.Bindings.P1;
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));
                Set(sim.P2,"Position",new BattleVector(facing*1.1f,sim.P1.Position.Y));
                yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);Tick();
                var attack=sim.P1.Attack;Assert.That(attack,Is.Not.Null);int limit=0;bool recovery=false;
                while(sim.P1.Attack==attack&&limit++<70)
                {
                    if(attack.Frame>=attack.Definition.StartupFrames+attack.Definition.ActiveFrames)
                    {
                        Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));Assert.That(sim.P2.Hearts,Is.EqualTo(3));
                        var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();Assert.That(effect.enabled,Is.True);
                        if(!recovery)CaptureActionEffects("rework_r4_yui","normal_recovery_"+facing,body,effect);recovery=true;
                    }
                    Tick();
                }
                Assert.That(limit,Is.LessThan(70));Assert.That(recovery,Is.True);Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));
                Assert.That(view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>().enabled,Is.False);
            }
        }
    }
}
