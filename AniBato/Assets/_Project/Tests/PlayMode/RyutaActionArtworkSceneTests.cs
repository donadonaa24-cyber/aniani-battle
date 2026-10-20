using System.Collections;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest] public IEnumerator RyutaActualInputsKeepBothFacingsPhasesGravityAndReadableSpellLetters()
        {
            var data=runner.Setup.Character(8);var keys=controller.Bindings.P1;
            var states=new[]{AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate,AnimationState.BackAttack};
            foreach(int facing in new[]{1,-1})for(int action=0;action<states.Length;action++)
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));
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
                var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();
                int prep=0,active=0,recovery=0,limit=0,gravity=0;bool p=false,a=false,r=false;string name=pose.Key+"_"+facing;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[pose.Frames.Length==1?0:impact?1:0]));
                    Assert.That(body.flipX,Is.EqualTo(view.RenderedSample.FlipX^pose.ReverseFacing));
                    Assert.That(effect.enabled,Is.EqualTo(!attack.Definition.IsNormal&&art!=null&&art.Visible(view.RenderedSample)));
                    if(effect.enabled)
                    {
                        Assert.That(effect.sprite,Is.SameAs(art.SpriteFor(view.RenderedSample)));
                        Assert.That(effect.flipX,Is.EqualTo(body.flipX&&!art.UnmirroredArtwork));
                        var offset=art.OffsetFor(view.RenderedSample);var expected=body.bounds.center+new Vector3((body.flipX?-1:1)*offset.x,offset.y,0);
                        if(attack.AbilityChargeReleased&&!art.Anticipating(view.RenderedSample))
                            AssertCueFitsChargedHitbox(attack,sim.P1,art,effect);
                        else Assert.That(Vector3.Distance(effect.transform.position,expected),Is.LessThan(.0001f));
                    }
                    if(!impact){prep++;if(!p){Capture(name+"_prepare",true,"rework_r4_ryuta");if(effect.enabled)CaptureActionEffects("rework_r4_ryuta",name+"_effect_prepare",body,effect);p=true;}}
                    else if(attack.IsActive){active++;if(!a){Capture(name+"_impact",true,"rework_r4_ryuta");if(effect.enabled)CaptureActionEffects("rework_r4_ryuta",name+"_effect",body,effect);a=true;}}
                    else{recovery++;if(!r){Capture(name+"_recovery",true,"rework_r4_ryuta");r=true;}}
                    var position=sim.P1.Position;int tick=sim.Match.TickCount;var rotation=body.transform.localRotation;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(body.transform.localRotation,Is.EqualTo(rotation));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(tick));
                    float vy=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravity++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravity,Is.GreaterThan(10));
                if(action==4){int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1){Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();}Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));}
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames.Last()));Assert.That(effect.enabled,Is.False);Capture(name+"_released",true,"rework_r4_ryuta");
            }
        }
        [UnityTest] public IEnumerator RyutaJabBatAndSpellHitOnceAndDoNotDamageDuringAfterglow()
        {
            var data=runner.Setup.Character(8);var keys=controller.Bindings.P1;
            foreach(int action in new[]{0,1,2})foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                Set(sim.P2,"Position",new BattleVector(facing*(action==0?1.1f:2),sim.P1.Position.Y));
                var button=action==0?keys.Attack:action==1?keys.Special:keys.Ultimate;
                yield return Key(button,true);Tick();yield return Key(button,false);if(action==0)Tick();
                var attack=sim.P1.Attack;Assert.That(attack,Is.Not.Null);int limit=0;bool hit=false,recovery=false;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    if(attack.Frame<attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames)
                    {Assert.That(sim.P2.Hearts,Is.EqualTo(3));Assert.That(sim.P2.NormalHitCount,Is.Zero);}
                    if(sim.P2.Hearts<3||sim.P2.NormalHitCount>0)
                    {
                        Assert.That(sim.P2.Hearts,Is.EqualTo(action==0?3:action==1?2:1));Assert.That(sim.P2.NormalHitCount,Is.EqualTo(action==0?1:0));
                        if(!hit)Capture("hit_"+action+"_"+facing,false,"rework_r4_ryuta");hit=true;
                    }
                    if(attack.Frame>=attack.Definition.StartupFrames+attack.Definition.ActiveFrames)
                    {Assert.That(hit,Is.True);Assert.That(view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>().enabled,Is.EqualTo(action!=0));recovery=true;}
                    Tick();
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(hit&&recovery,Is.True);Assert.That(sim.Projectiles.Count,Is.Zero);
                // The held ultimate impact is presentation-only and can outlive the attack.
                // Check every remaining frame, then require the effect to disappear.
                var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();int tail=0;
                while((tail<24||effect.enabled)&&tail<data.PaperMotion.UltimateImpactMinimumTicks+24)
                {
                    Tick();tail++;
                    Assert.That(sim.P2.Hearts,Is.EqualTo(action==0?3:action==1?2:1));
                    Assert.That(sim.P2.NormalHitCount,Is.EqualTo(action==0?1:0));
                }
                Assert.That(sim.P2.Hearts,Is.EqualTo(action==0?3:action==1?2:1));Assert.That(sim.P2.NormalHitCount,Is.EqualTo(action==0?1:0));
                Assert.That(effect.enabled,Is.False);
            }
        }
    }
}
