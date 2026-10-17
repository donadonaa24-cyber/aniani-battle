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
        public IEnumerator MinatoNativeActionsAndEffectsFollowInputTeleportGravityAndFieldLifetimeInBothDirections()
        {
            var data=runner.Setup.Character(3);var keys=controller.Bindings.P1;
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
                Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,false);
                var pose=data.PaperMotion.Find(states[action]);var art=data.PaperMotion.AbilityEffect(states[action]);
                var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();
                int prep=0,active=0,recovery=0,limit=0,gravity=0,teleports=0;bool p=false,a=false,r=false;
                string name=pose.Key+"_"+facing;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[pose.Frames.Length==1?0:impact?1:0]));
                    Assert.That(body.flipX,Is.EqualTo(view.RenderedSample.FlipX^pose.ReverseFacing));Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
                    Assert.That(effect.enabled,Is.EqualTo(art!=null&&art.Visible(view.RenderedSample)));
                    if(effect.enabled){Assert.That(effect.sprite,Is.SameAs(art.Sprite));Assert.That(effect.sortingOrder,Is.LessThan(body.sortingOrder));}
                    if(art!=null)Assert.That(view.transform.Find("Ability Cue").gameObject.activeSelf,Is.False);
                    if(!impact){prep++;if(!p){Capture(name+"_prepare",true,"rework_r4_minato");if(art!=null&&effect.enabled)CaptureMinatoEffects(name+"_effect_prepare",body,effect);p=true;}}
                    else if(attack.IsActive){active++;if(!a){Capture(name+"_impact",true,"rework_r4_minato");if(art!=null)CaptureMinatoEffects(name+"_effect",body,effect);a=true;}}
                    else{recovery++;if(!r){Capture(name+"_recovery",true,"rework_r4_minato");r=true;}}
                    if(action==4&&sim.Projectiles.Count>0)AssertMinatoFieldArt();
                    var position=sim.P1.Position;int tick=sim.Match.TickCount;var rotation=effect.transform.localRotation;var offset=effect.transform.position;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(effect.transform.localRotation,Is.EqualTo(rotation));Assert.That(effect.transform.position,Is.EqualTo(offset));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(tick));
                    float vy=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(action==3&&Mathf.Abs(sim.P1.Position.X-position.X)>1)teleports++;
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravity++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravity,Is.GreaterThan(10));if(action==3)Assert.That(teleports,Is.EqualTo(1));
                if(action==4)
                {
                    Assert.That(sim.Projectiles.Count,Is.EqualTo(1));var shot=sim.Projectiles.Single();var center=shot.Position;int held=0;
                    var fieldView=Object.FindAnyObjectByType<ProjectileView>();fieldView.SendMessage("LateUpdate");
                    var fieldSprite=fieldView.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    CaptureMinatoEffects(name+"_field",body,effect,fieldSprite);
                    while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1)
                    {Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();AssertMinatoFieldArt();}
                    Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));
                    Assert.That(effect.enabled,Is.False);Assert.That(sim.Projectiles,Does.Contain(shot),"人物の余韻後も場は持続する");
                    CaptureMinatoEffects(name+"_field_after_pose",body,fieldSprite);
                    while(sim.Projectiles.Contains(shot))
                    {
                        int remaining=shot.RemainingFrames;Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();
                        Assert.That(shot.Position,Is.EqualTo(center));Assert.That(shot.RemainingFrames,Is.EqualTo(remaining-1));
                    }
                }
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames.Last()));Assert.That(effect.enabled,Is.False);Capture(name+"_released",true,"rework_r4_minato");
            }
        }
        private void AssertMinatoFieldArt()
        {
            var sim=runner.Simulation;var shot=sim.Projectiles.Single();var view=Object.FindAnyObjectByType<ProjectileView>();view.SendMessage("LateUpdate");
            var sprite=view.transform.GetChild(0).GetComponent<SpriteRenderer>();var art=runner.Character(0).PaperMotion.Projectile(AbilityLook.BlackHole);
            Assert.That(sprite.enabled,Is.True);Assert.That(sprite.sprite,Is.SameAs(art.Sprite));Assert.That(sprite.color,Is.EqualTo(Color.white));
            Assert.That(sprite.transform.localScale,Is.EqualTo(Vector3.one*art.Scale));Assert.That(sprite.flipX,Is.False);
            Assert.That(sprite.sortingOrder,Is.EqualTo(art.SortingOrder).And.LessThan(body.sortingOrder));
            var pos=runner.RenderPosition(shot);Assert.That(sprite.transform.position,Is.EqualTo(new Vector3(pos.X,pos.Y,0)));
        }
        private static void CaptureMinatoEffects(string name,params SpriteRenderer[] sprites)=>CaptureActionEffects("rework_r4_minato",name,sprites);
    }
}
