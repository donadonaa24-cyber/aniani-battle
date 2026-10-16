using System.Collections;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator TomaUpDownAirDroneAndRocketUseTwoKeyPosesFromActualInputsInBothDirections()
        {
            var data=runner.Setup.Character(0);var keys=controller.Bindings.P1;
            AnimationState[] states={AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate,AnimationState.BackAttack};
            foreach(int facing in new[]{1,-1})for(int action=0;action<states.Length;action++)
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P2,"Position",new BattleVector(sim.Stage[0].Bounds.Right-1,sim.P2.Position.Y));
                Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                if(action==2){Set(sim.P1,"Position",new BattleVector(0,20));Set(sim.P1,"Grounded",false);}
                Key direction=action==0?keys.Up:action==1?keys.Down:action==5?(facing>0?keys.Left:keys.Right):UnityEngine.InputSystem.Key.None;
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,true);
                Key button=action==3?keys.Special:action==4?keys.Ultimate:keys.Attack;
                yield return Key(button,true);Tick();yield return Key(button,false);
                if(button==keys.Attack&&action!=2)Tick();
                Assert.That(sim.P1.Attack,Is.Not.Null);var attack=sim.P1.Attack;
                int cutInWait=0;
                while(view.RenderedSample.State!=states[action]&&cutInWait++<data.Movement.UltimateCutInFrames+2)Tick();
                Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                if(direction!=UnityEngine.InputSystem.Key.None)yield return Key(direction,false);
                var pose=data.PaperMotion.Find(states[action]);int prep=0,active=0,recovery=0,limit=0;
                bool capturedPrep=false,capturedActive=false,capturedRecovery=false;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False,"検査用の相手も場内に保持する");
                    Assert.That(view.UsesPaperSprite,Is.True);Assert.That(view.RenderedSample.State,Is.EqualTo(states[action]));
                    bool damaging=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[damaging?1:0]));Assert.That(body.flipX,Is.EqualTo((facing<0)^pose.ReverseFacing));
                    string suffix=facing>0?"_right":"_left";
                    if(!damaging){prep++;if(!capturedPrep){Capture(pose.Key+"_prepare"+suffix,true,"rework_r4_toma");capturedPrep=true;}}
                    else if(attack.IsActive)
                    {
                        active++;
                        if(!capturedActive){Capture(pose.Key+"_impact"+suffix,true,"rework_r4_toma");capturedActive=true;}
                        if(attack.Definition.SpawnProjectile&&sim.Projectiles.Any(p=>p.Attack==attack))AssertTomaProjectileArt(attack);
                    }
                    else {recovery++;if(!capturedRecovery){Capture(pose.Key+"_recovery"+suffix,true,"rework_r4_toma");capturedRecovery=true;}}
                    var before=sim.P1.Position;int ticks=sim.Match.TickCount;var rotation=body.transform.localRotation;var local=body.transform.localPosition;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(body.transform.localRotation,Is.EqualTo(rotation));Assert.That(body.transform.localPosition,Is.EqualTo(local));}
                    Assert.That(sim.P1.Position,Is.EqualTo(before));Assert.That(sim.Match.TickCount,Is.EqualTo(ticks));
                    Tick();
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                Assert.That(sim.P1.Attack,Is.Null);Assert.That(body.sprite,Is.Not.SameAs(pose.Frames[1]));
            }
        }
        [UnityTest]
        public IEnumerator TomaAllThreeGunComboStagesHoldImpactThroughRecoveryAndReturnToOwnIdle()
        {
            var data=runner.Setup.Character(0);var keys=controller.Bindings.P1;
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P2,"Position",new BattleVector(sim.Stage[0].Bounds.Right-1,sim.P2.Position.Y));
                var gun=data.GroundNormals[0].Definition;
                Assert.That(sim.P2.Hurtbox.Left-sim.P1.Position.X,Is.GreaterThan(gun.Hitbox.Right+gun.ProjectileSpeed*gun.ProjectileLifetimeFrames/60f+gun.NormalForwardDistance*3));
                yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);Tick();
                for(int stage=0;stage<3;stage++)
                {
                    Assert.That(sim.P1.Attack,Is.Not.Null);var attack=sim.P1.Attack;
                    Assert.That(attack.ChainIndex,Is.EqualTo(stage));var state=(AnimationState)((int)AnimationState.Attack1+stage);
                    var pose=data.PaperMotion.Find(state);bool queued=false,prep=false,active=false,recovery=false;int limit=0;
                    while(sim.P1.Attack==attack&&limit++<80)
                    {
                        Assert.That(sim.Match.IsFinished,Is.False);
                        Assert.That(view.RenderedSample.State,Is.EqualTo(state));Assert.That(view.UsesPaperSprite,Is.True);
                        Assert.That(body.sprite,Is.SameAs(pose.Frames[attack.Frame<attack.Definition.StartupFrames?0:1]));
                        Assert.That(body.flipX,Is.EqualTo(facing<0));
                        string phase=attack.Frame<attack.Definition.StartupFrames?"prepare":attack.IsActive?"impact":"recovery";
                        if(phase=="prepare"&&!prep){prep=true;Capture(pose.Key+"_prepare_"+facing,true,"rework_r4_toma");}
                        if(phase=="impact"&&!active){active=true;AssertTomaProjectileArt(attack);Capture(pose.Key+"_impact_"+facing,true,"rework_r4_toma");}
                        if(phase=="recovery"&&!recovery){recovery=true;Capture(pose.Key+"_recovery_"+facing,true,"rework_r4_toma");}
                        if(stage<2&&!queued&&attack.IsActive)
                        {yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);queued=true;}
                        Tick();
                    }
                    Assert.That(limit,Is.LessThan(80));Assert.That(prep&&active&&recovery,Is.True,"予備・発射・硬直をすべて確認");
                }
                Assert.That(sim.P1.Attack,Is.Null);Assert.That(view.RenderedSample.State,Is.EqualTo(AnimationState.Idle));
                Assert.That(body.sprite,Is.SameAs(data.PaperMotion.Sprite));Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
            }
        }
        private void AssertTomaProjectileArt(AttackInstance attack)
        {
            var sim=runner.Simulation;var shot=sim.Projectiles.Single(p=>p.Attack==attack);
            var shots=Object.FindAnyObjectByType<ProjectileView>();shots.SendMessage("LateUpdate");
            int index=sim.Projectiles.ToList().IndexOf(shot);var sprite=shots.transform.GetChild(index).GetComponent<SpriteRenderer>();
            var art=runner.Character(0).PaperMotion.Projectile(attack.Definition.Ability.Look);
            Assert.That(sprite.enabled,Is.True);Assert.That(sprite.sprite,Is.SameAs(art.Sprite));Assert.That(sprite.color,Is.EqualTo(Color.white));
            Assert.That(sprite.flipX,Is.EqualTo(art.FlipWithFacing&&shot.Facing<0));
            Assert.That(sprite.transform.localScale,Is.EqualTo(Vector3.one*art.Scale));
            float angle=(art.RotateWithVelocity?Mathf.Atan2(shot.Velocity.Y,shot.Velocity.X)*Mathf.Rad2Deg:0)+art.RotationOffsetDegrees;
            Assert.That(Mathf.DeltaAngle(sprite.transform.eulerAngles.z,angle),Is.EqualTo(0).Within(.001));
            var position=runner.RenderPosition(shot);Assert.That(sprite.transform.position.x,Is.EqualTo(position.X).Within(.00001));
            Assert.That(sprite.transform.position.y,Is.EqualTo(position.Y).Within(.00001));
            if(attack.Frame==attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames)
                CaptureTomaProjectile(sprite,shot,art.Look+"_"+view.RenderedSample.State+"_"+shot.Facing);
        }
        private static void CaptureTomaProjectile(SpriteRenderer sprite,Projectile shot,string name)
        {
            var camera=Camera.main;var position=camera.transform.position;float size=camera.orthographicSize;
            var clear=camera.clearFlags;var color=camera.backgroundColor;int mask=camera.cullingMask,layer=sprite.gameObject.layer;
            var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            var target=RenderTexture.GetTemporary(512,512,24,RenderTextureFormat.ARGB32);var image=new Texture2D(512,512,TextureFormat.RGBA32,false);
            try
            {
                sprite.gameObject.layer=31;camera.cullingMask=1<<31;
                camera.orthographicSize=Mathf.Max(.3f,Mathf.Max(shot.Hitbox.Size.X,shot.Hitbox.Size.Y)*.65f);
                camera.transform.position=new Vector3(sprite.transform.position.x,sprite.transform.position.y,position.z);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.125f,.15f,.19f,1);
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();
                string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../docs/screenshots/rework_r4_toma"));Directory.CreateDirectory(folder);
                CaptureFile.Write(Path.Combine(folder,"projectile_"+name+".png"),image.EncodeToPNG());
            }
            finally
            {
                sprite.gameObject.layer=layer;camera.cullingMask=mask;camera.orthographicSize=size;camera.transform.position=position;
                camera.clearFlags=clear;camera.backgroundColor=color;camera.targetTexture=previousTarget;RenderTexture.active=previousActive;
                RenderTexture.ReleaseTemporary(target);Object.Destroy(image);
            }
        }
    }
}
