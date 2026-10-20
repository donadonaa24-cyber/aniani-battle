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
                    Assert.That(text.gameObject.activeSelf,Is.EqualTo(action==3&&!impact));
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
        // The new special-N march is covered in RoyalMarchSceneTests with emergence/command/exit checks.
        [UnityTest] public IEnumerator KingUltimateSummonsThreeNamedSupportsOnAlternatingPathsWithOneFinisher()
        {
            var data=runner.Setup.Character(6);var keys=controller.Bindings.P1;var projectiles=Object.FindAnyObjectByType<ProjectileView>();
            foreach(bool ultimate in new[]{true})foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Position",new BattleVector(-facing*4,sim.P1.Position.Y));Set(sim.P1,"Facing",facing);Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                Set(sim.P2,"Position",new BattleVector(facing*3,sim.P2.Position.Y));var button=ultimate?keys.Ultimate:keys.Special;
                yield return Key(button,true);Tick();yield return Key(button,false);
                int wait=0;while(sim.Projectiles.Count==0&&wait++<100)Tick();Assert.That(sim.Projectiles.Count,Is.EqualTo(3));
                var shots=sim.Projectiles.ToArray();Assert.That(shots.Select(p=>p.Group).Distinct().Count(),Is.EqualTo(1));
                Assert.That(shots.Select(p=>p.Attack).Distinct().Count(),Is.EqualTo(ultimate?3:1));
                var group=shots[0].Group;var art=data.PaperMotion.Projectile(AbilityLook.King);
                Assert.That(shots[0].Position.X,Is.LessThan(sim.P2.Position.X));Assert.That(shots[1].Position.X,Is.GreaterThan(sim.P2.Position.X));
                if(ultimate)
                {
                    Assert.That(shots[2].Position.X,Is.LessThan(sim.P2.Position.X));
                    Assert.That(shots.Select(p=>p.DelayRemaining),Is.EqualTo(new[]{0,18,36}));
                }
                else{Assert.That(shots[2].Position.Y,Is.GreaterThan(sim.P2.Position.Y));Assert.That(shots.All(p=>p.Visible),Is.True);}
                var registry=new ProjectileArtworkSlots();registry.Refresh(shots);Assert.That(shots.Select(registry.Index),Is.EqualTo(new[]{0,1,2}));
                registry.Refresh(new[]{shots[1],shots[2]});Assert.That(registry.Index(shots[1]),Is.EqualTo(1));Assert.That(registry.Index(shots[2]),Is.EqualTo(2));
                registry=new ProjectileArtworkSlots();registry.Refresh(shots);
                int ticks=0;var visible=new System.Collections.Generic.HashSet<int>();var hits=new System.Collections.Generic.HashSet<long>();
                while(sim.Projectiles.Count>0&&ticks++<240)
                {
                    projectiles.SendMessage("LateUpdate");var renderers=projectiles.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled).ToArray();
                    Assert.That(renderers.Length,Is.EqualTo(sim.Projectiles.Count(p=>p.Visible)));
                    for(int i=0;i<sim.Projectiles.Count;i++)
                    {
                        var shot=sim.Projectiles[i];var renderer=projectiles.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.name=="Projectile").ElementAt(i);int variant=registry.Index(shot);
                        Assert.That(renderer.enabled,Is.EqualTo(shot.Visible));if(!shot.Visible)continue;
                        Assert.That(renderer.sprite,Is.SameAs(art.SpriteFor(variant)));Assert.That(renderer.flipX,Is.EqualTo(shot.Facing<0));
                        Assert.That(renderer.transform.localScale.x,Is.EqualTo(art.ScaleFor(shot.Hitbox.Size,variant)).Within(.00001f));
                        var position=runner.RenderPosition(shot);Assert.That(renderer.transform.position.x,Is.EqualTo(position.X).Within(.0001f));
                        Assert.That(renderer.transform.position.y,Is.EqualTo(position.Y).Within(.0001f));
                        if(visible.Add(variant))
                        {Capture((ultimate?"super":"special")+"_support_"+variant+"_"+facing,false,"rework_r4_king");CaptureActionEffects("rework_r4_king",(ultimate?"super":"special")+"_support_"+variant+"_"+facing,renderers);}
                    }
                    foreach(var hit in sim.Events.Where(e=>e.Kind==BattleEventKind.Hit&&e.SourcePlayer==0))
                    {hits.Add(hit.AttackId);if(!ultimate)Assert.That(sim.P2.ParalysisRemaining,Is.EqualTo(120));}
                    Assert.That(sim.P2.NormalHitCount,Is.Zero);
                    if(!ultimate)Assert.That(sim.P2.Hearts,Is.EqualTo(3));
                    else Assert.That(sim.P2.Hearts,Is.EqualTo(hits.Contains(shots[2].Attack.Id)?1:3));
                    Tick();
                }
                Assert.That(ticks,Is.LessThan(240));Assert.That(visible.OrderBy(i=>i),Is.EqualTo(new[]{0,1,2}));Assert.That(hits.Count,Is.EqualTo(ultimate?3:1));
                Assert.That(sim.P2.Hearts,Is.EqualTo(ultimate?1:3));Assert.That(shots.All(p=>p.Group==group),Is.True);
                projectiles.SendMessage("LateUpdate");Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled),Is.False);
            }
        }
    }
}
