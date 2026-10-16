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
        public IEnumerator ChizuruDedicatedUpDownAirAndEmptyCurryPlateFollowActualInputsInBothDirections()
        {
            var data=runner.Setup.Character(2);var keys=controller.Bindings.P1;
            var states=new[]{AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.Ultimate};
            foreach(int facing in new[]{1,-1})for(int action=0;action<states.Length;action++)
            {
                runner.BeginMatch(data,runner.Setup.Character(1),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));
                Set(sim.P2,"Position",new BattleVector(0,35));Set(sim.P2,"Grounded",false);Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                if(action==2){Set(sim.P1,"Position",new BattleVector(0,20));Set(sim.P1,"Grounded",false);}
                Key direction=action==0?keys.Up:action==1?keys.Down:UnityEngine.InputSystem.Key.None;
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,true);
                Key button=action==3?keys.Ultimate:keys.Attack;
                yield return Key(button,true);Tick();yield return Key(button,false);
                if(action<2)Tick();Assert.That(sim.P1.Attack,Is.Not.Null);var attack=sim.P1.Attack;int wait=0;
                while(view.RenderedSample.State!=states[action]&&wait++<data.Movement.UltimateCutInFrames+2)Tick();
                Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,false);
                var pose=data.PaperMotion.Find(states[action]);int prep=0,active=0,recovery=0,limit=0,gravityTicks=0;
                bool prepShot=false,impactShot=false,recoveryShot=false;string name=pose.Key+"_"+facing;
                while(sim.P1.Attack==attack&&limit++<160)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    int expectedFrame=action==1||action==2?0:impact?1:0;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[expectedFrame]));
                    Assert.That(body.flipX,Is.EqualTo(facing<0));Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
                    if(action<2)Assert.That(pose.Foot(view.RenderedSample),Is.Zero.Within(.0001),"地上上下の原画は足元で接地");
                    if(!impact){prep++;if(!prepShot){Capture(name+"_prepare",true,"rework_r4_chizuru");prepShot=true;}}
                    else if(attack.IsActive){active++;if(!impactShot){Capture(name+"_impact",true,"rework_r4_chizuru");impactShot=true;}}
                    else{recovery++;if(!recoveryShot){Capture(name+"_recovery",true,"rework_r4_chizuru");recoveryShot=true;}}
                    var position=sim.P1.Position;int ticks=sim.Match.TickCount;var local=body.transform.localPosition;var rotation=body.transform.localRotation;
                    for(int r=0;r<3;r++){view.RenderFrame();Assert.That(body.transform.localPosition,Is.EqualTo(local));Assert.That(body.transform.localRotation,Is.EqualTo(rotation));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(ticks));
                    float vy=sim.P1.Velocity.Y;bool airborne=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(airborne&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravityTicks++;}
                }
                Assert.That(limit,Is.LessThan(160));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravityTicks,Is.GreaterThan(10));
                if(action==3){int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1)Tick();Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));}
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames[pose.Frames.Length-1]));Capture(name+"_released",true,"rework_r4_chizuru");
            }
        }
    }
}
