using System.Collections;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator RenKicksAndRookTransformationsSwitchOnActualAttackTicksInBothDirections()
        {
            var data=runner.Setup.Character(1);var keys=controller.Bindings.P1;
            var states=new[]{AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.AirNeutral,
                AnimationState.SpecialNeutral,AnimationState.Ultimate,AnimationState.BackAttack};
            foreach(int facing in new[]{1,-1})for(int action=0;action<states.Length;action++)
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));
                Set(sim.P2,"Position",new BattleVector(0,35));Set(sim.P2,"Grounded",false);Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                if(action==2){Set(sim.P1,"Position",new BattleVector(0,20));Set(sim.P1,"Grounded",false);}
                Key direction=action==0?keys.Up:action==1?keys.Down:action==5?(facing>0?keys.Left:keys.Right):UnityEngine.InputSystem.Key.None;
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,true);
                Key button=action==3?keys.Special:action==4?keys.Ultimate:keys.Attack;
                yield return Key(button,true);Tick();yield return Key(button,false);
                if(button==keys.Attack&&action!=2)Tick();
                Assert.That(sim.P1.Attack,Is.Not.Null);var attack=sim.P1.Attack;int wait=0;
                while(view.RenderedSample.State!=states[action]&&wait++<data.Movement.UltimateCutInFrames+2)Tick();
                Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,false);
                var pose=data.PaperMotion.Find(states[action]);int prep=0,active=0,recovery=0,limit=0,airGravityTicks=0,rushTicks=0;
                string name=pose.Key+"_"+facing;
                bool prepShot=false,activeShot=false,recoveryShot=false;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    int expectedFrame=action==1||action==2?0:impact?1:0;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[expectedFrame]));
                    Assert.That(body.flipX,Is.EqualTo(view.RenderedSample.FlipX^pose.ReverseFacing));
                    Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
                    if(!impact){prep++;if(!prepShot){Capture(name+"_prepare",true,"rework_r4_ren");prepShot=true;}}
                    else if(attack.IsActive)
                    {
                        active++;if(!activeShot){Capture(name+"_impact",true,"rework_r4_ren");activeShot=true;}
                        if(action==3||action==4)
                        {
                            Assert.That(body.sprite.name,Is.EqualTo("ren_"+(action==3?"special":"super")+"_impact"));
                            Assert.That(view.transform.Find("Ability Cue").gameObject.activeSelf,Is.False,"生成済み飛車の文字へ仮ラベルを重ねない");
                            if(Mathf.Abs(sim.P1.Velocity.X)>30)rushTicks++;
                        }
                    }
                    else{recovery++;if(!recoveryShot){Capture(name+"_recovery",true,"rework_r4_ren");recoveryShot=true;}}
                    var position=sim.P1.Position;int ticks=sim.Match.TickCount;var rotation=body.transform.localRotation;var local=body.transform.localPosition;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(body.transform.localRotation,Is.EqualTo(rotation));Assert.That(body.transform.localPosition,Is.EqualTo(local));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(ticks));
                    float velocityY=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;
                    Tick();
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(velocityY));airGravityTicks++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(airGravityTicks,Is.GreaterThan(10),"空中攻撃中も通常の重力で流れる");
                if(action==3||action==4)Assert.That(rushTicks,Is.GreaterThan(0),"実際の高速突進と変身表示が重なる");
                Assert.That(sim.P1.Attack,Is.Null);
                if(action==4)
                {
                    int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1)Tick();
                    Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));
                }
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames[pose.Frames.Length-1]));Capture(name+"_released",true,"rework_r4_ren");
            }
        }
    }
}
