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
        public IEnumerator ShiroActionsAndGeneratedAlmaPhasesFollowActualInputAndAirGravityInBothDirections()
        {
            var data=runner.Setup.Character(4);var keys=controller.Bindings.P1;
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
                int prep=0,active=0,recovery=0,limit=0,gravity=0;bool p=false,a=false,r=false;string name=pose.Key+"_"+facing;
                while(sim.P1.Attack==attack&&limit++<200)
                {
                    Assert.That(sim.Match.IsFinished,Is.False);Assert.That(view.UsesPaperSprite,Is.True);
                    bool impact=attack.Frame>=attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames;
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[pose.Frames.Length==1?0:impact?1:0]));
                    Assert.That(body.flipX,Is.EqualTo(view.RenderedSample.FlipX^pose.ReverseFacing));Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
                    Assert.That(effect.enabled,Is.EqualTo(art!=null&&art.Visible(view.RenderedSample)));
                    if(effect.enabled)
                    {
                        Assert.That(effect.sprite,Is.SameAs(art.SpriteFor(view.RenderedSample)));
                        Assert.That(effect.transform.localScale,Is.EqualTo(Vector3.one*art.ScaleFor(view.RenderedSample)));
                        Assert.That(effect.sortingOrder,Is.LessThan(body.sortingOrder));
                    }
                    bool binary=action==4&&!impact;
                    Assert.That(text.gameObject.activeSelf,Is.EqualTo(binary));
                    if(binary)
                    {
                        Assert.That(text.text==art.AnticipationText||text.text==art.AlternateAnticipationText,Is.True);
                        var textBounds=text.GetComponent<MeshRenderer>().bounds;
                        Assert.That(textBounds.size.x,Is.LessThan(effect.bounds.size.x),"二進数の列を生成枠の幅へ収める");
                        Assert.That(textBounds.size.y,Is.LessThan(effect.bounds.size.y),"二進数の行を生成枠の高さへ収める");
                    }
                    if(!impact){prep++;if(!p){Capture(name+"_prepare",true,"rework_r4_shiro");if(effect.enabled)CaptureShiroEffects(name+"_effect_prepare",body,effect,text.GetComponent<MeshRenderer>());p=true;}}
                    else if(attack.IsActive){active++;if(!a){Capture(name+"_impact",true,"rework_r4_shiro");if(art!=null)CaptureShiroEffects(name+"_effect",body,effect);a=true;}}
                    else{recovery++;if(!r){Capture(name+"_recovery",true,"rework_r4_shiro");r=true;}}
                    var position=sim.P1.Position;int tick=sim.Match.TickCount;var rotation=effect.transform.localRotation;var offset=effect.transform.position;
                    for(int render=0;render<3;render++){view.RenderFrame();Assert.That(effect.transform.localRotation,Is.EqualTo(rotation));Assert.That(effect.transform.position,Is.EqualTo(offset));}
                    Assert.That(sim.P1.Position,Is.EqualTo(position));Assert.That(sim.Match.TickCount,Is.EqualTo(tick));
                    float vy=sim.P1.Velocity.Y;bool falling=action==2&&!sim.P1.Grounded&&sim.P1.HitstopRemaining==0;Tick();
                    if(falling&&sim.P1.Attack==attack){Assert.That(sim.P1.Velocity.Y,Is.LessThan(vy));gravity++;}
                }
                Assert.That(limit,Is.LessThan(200));Assert.That(prep,Is.GreaterThan(0));Assert.That(active,Is.GreaterThan(0));Assert.That(recovery,Is.GreaterThan(0));
                if(action==2)Assert.That(gravity,Is.GreaterThan(10));
                if(action==4)
                {
                    int held=0;while(body.sprite==pose.Frames[1]&&held++<data.PaperMotion.UltimateImpactMinimumTicks+1)
                    {Set(sim.P2,"Position",new BattleVector(facing*6,20));Set(sim.P2,"Velocity",default(BattleVector));Tick();}
                    Assert.That(held,Is.LessThanOrEqualTo(data.PaperMotion.UltimateImpactMinimumTicks));
                }
                Assert.That(body.sprite,Is.Not.SameAs(pose.Frames.Last()));Assert.That(effect.enabled,Is.False);Assert.That(text.gameObject.activeSelf,Is.False);
                Capture(name+"_released",true,"rework_r4_shiro");
            }
        }
        [UnityTest]
        public IEnumerator ShiroEmpAndAlmaDealDamageOnlyWhenGeneratedImpactAppears()
        {
            var data=runner.Setup.Character(4);var keys=controller.Bindings.P1;
            foreach(bool ultimate in new[]{false,true})
            {
                runner.BeginMatch(data,runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
                Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));Set(sim.P2,"Position",new BattleVector(2,sim.P1.Position.Y));
                Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);var button=ultimate?keys.Ultimate:keys.Special;
                yield return Key(button,true);Tick();yield return Key(button,false);
                var attack=sim.P1.Attack;Assert.That(attack,Is.Not.Null);int limit=0;bool damaged=false;
                while(sim.P1.Attack==attack&&limit++<240)
                {
                    if(sim.P2.Hearts<3)
                    {
                        damaged=true;Assert.That(attack.IsDamageActive,Is.True);Assert.That(attack.Frame,Is.EqualTo(attack.Definition.StartupFrames+attack.Definition.DamageDelayFrames));
                        var effect=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();var art=data.PaperMotion.AbilityEffect(attack.Definition.Ability.Look);
                        Assert.That(effect.enabled,Is.True);Assert.That(effect.sprite,Is.SameAs(art.Sprite));
                        if(!ultimate)Assert.That(sim.P2.ParalysisRemaining,Is.EqualTo(12));break;
                    }
                    if(ultimate&&view.RenderedSample.IsAttack)
                        Assert.That(view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>().sprite,
                            Is.SameAs(data.PaperMotion.AbilityEffect(AbilityLook.Alma).AnticipationSprite));
                    Tick();
                }
                Assert.That(damaged,Is.True);Assert.That(sim.P2.Hearts,Is.EqualTo(ultimate?1:2));int hearts=sim.P2.Hearts;
                while(sim.P1.Attack==attack&&limit++<240)Tick();Assert.That(sim.P2.Hearts,Is.EqualTo(hearts),"同じ攻撃で重複被弾しない");
            }
        }
        private static void CaptureShiroEffects(string name,params Renderer[] sprites)
        {
            sprites=sprites.Where(s=>s.enabled&&s.gameObject.activeInHierarchy).ToArray();
            var camera=Camera.main;var position=camera.transform.position;float size=camera.orthographicSize,aspect=camera.aspect;
            var flags=camera.clearFlags;var color=camera.backgroundColor;int mask=camera.cullingMask;
            var target=camera.targetTexture;var active=RenderTexture.active;var layers=sprites.Select(s=>s.gameObject.layer).ToArray();
            var rt=RenderTexture.GetTemporary(768,768,24,RenderTextureFormat.ARGB32);var image=new Texture2D(768,768,TextureFormat.RGBA32,false);
            try
            {
                var bounds=sprites[0].bounds;foreach(var s in sprites){bounds.Encapsulate(s.bounds);s.gameObject.layer=31;}
                camera.cullingMask=1<<31;camera.aspect=1;camera.orthographicSize=Mathf.Max(bounds.size.x,bounds.size.y)*.56f;
                camera.transform.position=new Vector3(bounds.center.x,bounds.center.y,position.z);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.17f,.2f,.25f,1);camera.targetTexture=rt;
                camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();
                string dir=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/screenshots/rework_r4_shiro"));System.IO.Directory.CreateDirectory(dir);
                CaptureFile.Write(System.IO.Path.Combine(dir,name+".png"),image.EncodeToPNG());
            }
            finally
            {
                for(int i=0;i<sprites.Length;i++)sprites[i].gameObject.layer=layers[i];
                camera.cullingMask=mask;camera.transform.position=position;camera.orthographicSize=size;camera.aspect=aspect;
                camera.clearFlags=flags;camera.backgroundColor=color;camera.targetTexture=target;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);
            }
        }
    }
}
