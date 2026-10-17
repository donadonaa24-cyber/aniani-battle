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
        [UnityTest] public IEnumerator KingActionsUseActualInputsBothFacingsAndUnmirroredPieceWithContinuousAirGravity()
        {
            var data=runner.Setup.Character(6);var keys=controller.Bindings.P1;
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
                var pose=data.PaperMotion.Find(states[action]);var text=view.transform.Find("Ability Cue").GetComponent<TextMesh>();
                int prep=0,active=0,recovery=0,limit=0,gravity=0;bool p=false,a=false,r=false;string name=pose.Key+"_"+facing;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[pose.Frames.Length==1?0:impact?1:0]));
                    Assert.That(body.flipX,Is.EqualTo(!(pose.UnmirroredImpact&&impact)&&(view.RenderedSample.FlipX^pose.ReverseFacing)));
                    Assert.That(text.gameObject.activeSelf,Is.EqualTo((action==3||action==4)&&!impact));
                    if(!impact){prep++;if(!p){Capture(name+"_prepare",true,"rework_r4_king");p=true;}}
                    else if(attack.IsActive){active++;if(!a){Capture(name+"_impact",true,"rework_r4_king");a=true;}}
                    else{recovery++;if(!r){Capture(name+"_recovery",true,"rework_r4_king");r=true;}}
                    var position=sim.P1.Position;int tick=sim.Match.TickCount;var rotation=body.transform.localRotation;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(body.transform.localRotation,Is.EqualTo(rotation));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(tick));
                    float vy=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravity++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravity,Is.GreaterThan(10));
                if(action==4){int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1){Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();}Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));}
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames.Last()));Assert.That(text.gameObject.activeSelf,Is.False);Capture(name+"_released",true,"rework_r4_king");
            }
        }
        [UnityTest] public IEnumerator KingSummonsLeilaShionNoaOnceEachWithStableIdentityAndOneSharedHit()
        {
            var data=runner.Setup.Character(6);var keys=controller.Bindings.P1;var projectiles=Object.FindAnyObjectByType<ProjectileView>();
            foreach(bool ultimate in new[]{false,true})foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Position",new BattleVector(-facing*4,sim.P1.Position.Y));Set(sim.P1,"Facing",facing);Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                Set(sim.P2,"Position",new BattleVector(facing*3,sim.P2.Position.Y));var button=ultimate?keys.Ultimate:keys.Special;
                yield return Key(button,true);Tick();yield return Key(button,false);
                int wait=0;while(sim.Projectiles.Count==0&&wait++<100)Tick();Assert.That(sim.Projectiles.Count,Is.EqualTo(3));
                var shots=sim.Projectiles.ToArray();Assert.That(shots.Select(p=>p.Attack).Distinct().Count(),Is.EqualTo(1));
                var attack=shots[0].Attack;var art=data.PaperMotion.Projectile(AbilityLook.King);float spacing=ultimate?2.6f:1.5f;
                Assert.That(shots[1].Position.X-shots[0].Position.X,Is.EqualTo(spacing).Within(.0001f));
                Assert.That(shots[2].Position.X-shots[1].Position.X,Is.EqualTo(spacing).Within(.0001f));
                var registry=new ProjectileArtworkSlots();registry.Refresh(shots);Assert.That(shots.Select(registry.Index),Is.EqualTo(new[]{0,1,2}));
                registry.Refresh(new[]{shots[1],shots[2]});Assert.That(registry.Index(shots[1]),Is.EqualTo(1));Assert.That(registry.Index(shots[2]),Is.EqualTo(2));
                int ticks=0;bool captured=false,hit=false;
                while(sim.Projectiles.Count>0&&ticks++<130)
                {
                    projectiles.SendMessage("LateUpdate");var renderers=projectiles.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled).ToArray();
                    Assert.That(renderers.Length,Is.EqualTo(sim.Projectiles.Count));
                    for(int i=0;i<renderers.Length;i++)
                    {
                        Assert.That(renderers[i].sprite,Is.SameAs(art.SpriteFor(i)));Assert.That(renderers[i].flipX,Is.False);
                        Assert.That(renderers[i].transform.localScale.x,Is.EqualTo(art.ScaleFor(sim.Projectiles[i].Hitbox.Size,i)).Within(.00001f));
                        if(i>0)Assert.That(renderers[i].transform.position.x-renderers[i-1].transform.position.x,Is.EqualTo(spacing).Within(.0001f),"Each named support keeps the actual formation spacing when drawn");
                    }
                    if(!captured&&sim.Projectiles[0].Position.Y<sim.P2.Position.Y+4)
                    {Capture((ultimate?"super":"special")+"_summons_"+facing,false,"rework_r4_king");CaptureActionEffects("rework_r4_king",(ultimate?"super":"special")+"_three_supports_"+facing,renderers);captured=true;}
                    if(sim.P2.Hearts<3){hit=true;Assert.That(sim.P2.Hearts,Is.EqualTo(ultimate?1:2));Assert.That(sim.P2.NormalHitCount,Is.EqualTo(0));}
                    Tick();
                }
                Assert.That(ticks,Is.LessThan(130));Assert.That(captured,Is.True);Assert.That(hit,Is.True);
                Assert.That(sim.P2.Hearts,Is.EqualTo(ultimate?1:2));Assert.That(shots.All(p=>p.Attack==attack),Is.True);
                projectiles.SendMessage("LateUpdate");Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled),Is.False);
            }
        }
    }
}
