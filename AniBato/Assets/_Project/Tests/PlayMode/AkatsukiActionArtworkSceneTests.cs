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
        [UnityTest] public IEnumerator AkatsukiActionsUseActualInputsBothFacingsAndReleaseCardWithContinuousAirGravity()
        {
            var data=runner.Setup.Character(7);var keys=controller.Bindings.P1;
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
                var pose=data.PaperMotion.Find(states[action]);
                int prep=0,active=0,recovery=0,limit=0,gravity=0;bool p=false,a=false,r=false;string name=pose.Key+"_"+facing;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[pose.Frames.Length==1?0:impact?1:0]));
                    Assert.That(body.flipX,Is.EqualTo(view.RenderedSample.FlipX^pose.ReverseFacing));
                    if(action==3)Assert.That(sim.Projectiles.Count,Is.EqualTo(impact?1:0),"手を開くフレームで実カードが1枚出る");
                    if(!impact){prep++;if(!p){Capture(name+"_prepare",true,"rework_r4_akatsuki");p=true;}}
                    else if(attack.IsActive){active++;if(!a){Capture(name+"_impact",true,"rework_r4_akatsuki");a=true;}}
                    else{recovery++;if(!r){Capture(name+"_recovery",true,"rework_r4_akatsuki");r=true;}}
                    var position=sim.P1.Position;int tick=sim.Match.TickCount;var rotation=body.transform.localRotation;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(body.transform.localRotation,Is.EqualTo(rotation));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(tick));
                    float vy=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravity++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravity,Is.GreaterThan(10));
                if(action==4){int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1){Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();}Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));}
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames.Last()));Capture(name+"_released",true,"rework_r4_akatsuki");
            }
        }
        [UnityTest] public IEnumerator AkatsukiSpecialAndPyramidUseSelectedSpritesFallAndDamageOnlyOnce()
        {
            var data=runner.Setup.Character(7);var keys=controller.Bindings.P1;var projectiles=Object.FindAnyObjectByType<ProjectileView>();
            foreach(bool ultimate in new[]{false,true})foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Position",new BattleVector(-facing*4,sim.P1.Position.Y));Set(sim.P1,"Facing",facing);Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                Set(sim.P2,"Position",new BattleVector(ultimate?0:facing*3,sim.P2.Position.Y));var button=ultimate?keys.Ultimate:keys.Special;
                yield return Key(button,true);Tick();yield return Key(button,false);
                int wait=0;while(sim.Projectiles.Count==0&&wait++<100)Tick();Assert.That(sim.Projectiles.Count,Is.EqualTo(1));
                var shot=sim.Projectiles.Single();var art=data.PaperMotion.Projectile(ultimate?AbilityLook.Pyramid:AbilityLook.Card);
                int ticks=0;bool captured=false,hit=false,afterAction=false;float oldY=shot.Position.Y;
                bool landedWall=false,groundedArtwork=false; int landingLife=0;
                while(sim.Projectiles.Count>0&&ticks++<800)
                {
                    projectiles.SendMessage("LateUpdate");var renderer=projectiles.GetComponentsInChildren<SpriteRenderer>().Single(s=>s.enabled);
                    Assert.That(renderer.sprite,Is.SameAs(art.Sprite));Assert.That(renderer.flipX,Is.False);
                    if(ultimate){Assert.That(renderer.transform.localScale.x*art.VisibleSize.x,Is.EqualTo(shot.Hitbox.Size.X).Within(.00001f));Assert.That(renderer.transform.localScale.y*art.VisibleSize.y,Is.EqualTo(shot.Hitbox.Size.Y).Within(.00001f));}
                    else Assert.That(renderer.transform.localScale.x,Is.EqualTo(art.Scale).Within(.00001f));
                    if(ultimate){Assert.That(shot.Position.Y,Is.LessThanOrEqualTo(oldY));oldY=shot.Position.Y;}
                    if(ultimate&&shot.Landed&&!landedWall){landedWall=true;landingLife=shot.RemainingFrames;Assert.That(sim.Stage.IsActive(sim.Stage.CollisionPlatformCount-1),Is.True);}
                    if(ultimate&&shot.Landed&&shot.RemainingFrames<600&&!groundedArtwork)
                    {
                        float artBottom=renderer.transform.position.y-art.VisibleSize.y*renderer.transform.localScale.y*.5f;
                        Assert.That(artBottom,Is.EqualTo(shot.Hitbox.Bottom).Within(.001f));groundedArtwork=true;
                        Capture("pyramid_landed_wall_"+facing,false,"special_refinement");
                    }
                    if(!captured&&(!ultimate||shot.Position.Y<sim.P2.Position.Y+4))
                    {Capture((ultimate?"super":"special")+"_projectile_"+facing,false,"rework_r4_akatsuki");CaptureActionEffects("rework_r4_akatsuki",(ultimate?"super":"special")+"_item_"+facing,renderer);captured=true;}
                    if(sim.P2.Hearts<3){if(!hit)Capture((ultimate?"super":"special")+"_hit_"+facing,false,"rework_r4_akatsuki");hit=true;Assert.That(sim.P2.Hearts,Is.EqualTo(ultimate?1:2));Assert.That(sim.P2.NormalHitCount,Is.Zero);}
                    if(sim.P1.Attack==null)afterAction=true;
                    Tick();
                }
                Assert.That(ticks,Is.LessThan(800));Assert.That(captured&&hit&&afterAction,Is.True);
                if(ultimate){Assert.That(landedWall&&groundedArtwork,Is.True);Assert.That(landingLife,Is.EqualTo(600));Assert.That(sim.Stage.IsActive(sim.Stage.CollisionPlatformCount-1),Is.False);}
                Assert.That(sim.P2.Hearts,Is.EqualTo(ultimate?1:2));
                projectiles.SendMessage("LateUpdate");Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled),Is.False);
            }
        }
    }
}
