using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        private static void Set(object target,string property,object value)=>target.GetType().GetProperty(property).SetValue(target,value);
        [UnityTest]
        public IEnumerator AllChizuruKeyPosesRenderAgainstActualHitboxesAndWriteReview()
        {
            var data=runner.Character(0);var list=new List<(AnimationState state,AttackData attack)>();
            list.Add((AnimationState.Attack1,data.GroundNormals[0]));list.Add((AnimationState.Attack2,data.GroundNormals[1]));list.Add((AnimationState.Attack3,data.GroundNormals[2]));
            AnimationState[] directions={AnimationState.CrouchAttack,AnimationState.BackAttack,AnimationState.AirNeutral,AnimationState.AirForward,AnimationState.AirBack,AnimationState.AirUp,AnimationState.AirDown,AnimationState.AirForwardUp,AnimationState.AirForwardDown};
            for(int i=1;i<10;i++)list.Add((directions[i-1],data.DirectionalNormals[i]));
            AnimationState[] specials={AnimationState.SpecialNeutral,AnimationState.SpecialSide,AnimationState.SpecialUp,AnimationState.SpecialDown};
            for(int i=0;i<4;i++)list.Add((specials[i],data.Specials[i]));list.Add((AnimationState.Ultimate,data.Ultimate));
            var rows=new List<string>{"# 千鶴の絵と判定の点検（2026-10-15）","",
                "実Unityの固定状態描画。攻撃有効フレームの定義とキー原画を設定し、実FighterView／HitboxDebugView／ProjectileViewをCamera.Renderで比較した。対戦中の発生・持続・命中は別の実入力回帰テストで検証。数値は本人中心からのworld座標。灰色抽出のフライパン範囲は柄を含む推定で、最終判断は画像目視。","",
                "|技／状態|絵の道具の横範囲（推定）|攻撃の横範囲|点検／対応|","|---|---|---|---|"};
            var debug=Object.FindAnyObjectByType<HitboxDebugView>();runner.SetDebugBoxes(true,false);
            foreach(var row in list)
            {
                runner.BeginMatch(data,runner.Setup.Character(0),runner.CurrentStage,false);runner.SetPaused(false);
                var sim=runner.Simulation;var definition=row.attack.ToDefinition();bool air=row.state.ToString().StartsWith("Air");
                Set(sim.P1,"Grounded",!air);Set(sim.P1,"Mode",row.state==AnimationState.CrouchAttack?FighterMode.Crouch:FighterMode.Attack);
                Set(sim.P1,"Position",new BattleVector(-5,air?4:1.1f));Set(sim.P2,"Position",new BattleVector(-2.8f,air?4:1.1f));
                var attack=(AttackInstance)System.Activator.CreateInstance(typeof(AttackInstance),BindingFlags.Instance|BindingFlags.NonPublic,null,new object[]{1L,definition,0,air},null);
                Set(attack,"Frame",definition.StartupFrames+definition.DamageDelayFrames);Set(sim.P1,"Attack",attack);
                if(definition.SpawnProjectile)
                {
                    Set(attack,"Frame",definition.StartupFrames-1);sim.Tick(default,default);
                    Object.FindAnyObjectByType<ProjectileView>().SendMessage("LateUpdate");
                    Assert.That(sim.Projectiles.Count,Is.EqualTo(1));
                    Assert.That(Object.FindObjectsByType<SpriteRenderer>().Single(r=>r.name=="Projectile"&&r.enabled).sprite,Is.SameAs(data.PaperMotion.RiceBall));
                }
                runner.RenderState.Reset(sim);
                var players=(FighterAnimationPlayer[])typeof(BattleRunner).GetField("animations",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(runner);
                var sample=new AnimationSample{State=row.state,IsAttack=true,AttackFrame=definition.StartupFrames+definition.DamageDelayFrames,
                    Age=definition.StartupFrames,Startup=definition.StartupFrames,Active=definition.ActiveFrames,Recovery=definition.RecoveryFrames};
                Set(players[0],"Sample",sample);
                foreach(var v in Object.FindObjectsByType<FighterView>())v.RenderFrame();
                int tick=sim.Match.TickCount;var position=sim.P1.Position;debug.RenderFrame();
                Assert.That(sim.Match.TickCount,Is.EqualTo(tick));Assert.That(sim.P1.Position,Is.EqualTo(position));
                var pan=PanRange(body,position);string picture=row.state.ToString();
                CaptureReview(picture,position,Mathf.Max(2.4f,definition.Hitbox.Size.X*.65f+1));
                string range=pan.HasValue?F(pan.Value.x)+"～"+F(pan.Value.y):definition.SpawnProjectile?"おにぎり約0.42幅":"画像を目視";
                string verdict=definition.NonDamaging?"非攻撃の回復技。攻撃判定なし":
                    definition.SpawnProjectile?"飛翔は紙版。手持ちを拡大し両方約0.42幅。投射判定は維持":
                    pan.HasValue&&(Mathf.Abs(pan.Value.y-definition.Hitbox.Right)>.25f||Mathf.Abs(pan.Value.x-definition.Hitbox.Left)>.25f)?
                    "絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述":"横範囲を画像で点検。現判定を維持";
                rows.Add("|["+definition.Name+" / "+picture+"](screenshots/hitbox_review_chizuru/"+picture+".png)|"+range+"|"+
                    (definition.NonDamaging?"なし":F(definition.Hitbox.Left)+"～"+F(definition.Hitbox.Right))+"|"+verdict+"|");
            }
            foreach(var state in new[]{AnimationState.Idle,AnimationState.Crouch,AnimationState.Fall})
            {
                runner.RestartMatch();var sim=runner.Simulation;Set(sim.P1,"Grounded",state!=AnimationState.Fall);
                Set(sim.P1,"Mode",state==AnimationState.Crouch?FighterMode.Crouch:state==AnimationState.Fall?FighterMode.Fall:FighterMode.Idle);
                var player=((FighterAnimationPlayer[])typeof(BattleRunner).GetField("animations",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(runner))[0];
                Set(player,"Sample",new AnimationSample{State=state});runner.RenderState.Reset(sim);view.RenderFrame();debug.RenderFrame();
                CaptureReview("Hurtbox_"+state,sim.P1.Position,2.4f);
                rows.Add("|[やられ判定 "+state+"](screenshots/hitbox_review_chizuru/Hurtbox_"+state+".png)|頭髪・服・靴は緑枠と完全一致させない|"+
                    F(sim.P1.Hurtbox.Left-sim.P1.Position.X)+"～"+F(sim.P1.Hurtbox.Right-sim.P1.Position.X)+"|しゃがみの高さ縮小と空中の全身を画像で確認。体格の仕様変更は保留|");
            }
            rows.AddRange(new[]{"","## 調整案と保留理由","",
                "通常45件の半減とゲージ14/10を保持。今回、判定の数値変更は0件。頭身の大きい紙絵ではフライパン・手足と判定の終端が必ずしも一致しない。横振りに対して長い通常判定、回転した上下空中技の縦方向、復帰技・超必殺の広い効果範囲が調整候補。通常の範囲をさらに絵へ短縮する場合は近接戦の成立とCPU／ゲージ頻度も変わるため、ユーザー確認後に一技ずつ調整する。",
                "","横振りの上下方向は原画を追加せず全身の固定傾き（上60度／下−60度／斜め±30度）で表示。フライパンの柄も抽出範囲に含まれるので、数値だけで判定を自動修正しない。緑枠は髪／服の全輪郭を覆うためのものではなく、しゃがみ・空中を含む戦闘用のやられ判定。","",
                "表示はF9／ポーズ／メニュー設定。緑＝やられ、赤＝有効、黄＝飛翔、薄赤＝予備。練習はF5速度、F6静止、F7単一tick。"});
            File.WriteAllLines(Path.Combine(Application.dataPath,"../../docs/HITBOX_REVIEW_CHIZURU.md"),rows);
            runner.RestartMatch();runner.SetDebugBoxes(false,false);yield return null;
        }
        [UnityTest]
        public IEnumerator DebugObserverKeepsPhysicsAndHitTraceDuringInspection()
        {
            var debug=Object.FindAnyObjectByType<HitboxDebugView>();runner.SetDebugBoxes(true,false);
            var keys=controller.Bindings.P1;yield return Key(keys.Right,true);
            while(runner.Simulation.P2.Position.X-runner.Simulation.P1.Position.X>1.5f)Tick();
            yield return Key(keys.Right,false);yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);
            Assert.That(Object.FindObjectsByType<LineRenderer>().Any(l=>l.enabled&&l.startColor.a<.5f),Is.True,"溜め範囲の半透明表示");
            for(int i=0;i<35;i++)Tick();
            Assert.That(debug.TraceCount,Is.GreaterThan(0));runner.SetPaused(true);
            int tick=runner.Simulation.Match.TickCount;var p=runner.Simulation.P1.Position;
            for(int i=0;i<300;i++)debug.RenderFrame();
            Assert.That(runner.Simulation.Match.TickCount,Is.EqualTo(tick));Assert.That(runner.Simulation.P1.Position,Is.EqualTo(p));
            yield return new WaitForSecondsRealtime(.55f);Assert.That(debug.TraceCount,Is.GreaterThan(0),"静止中は残像を保持する");
            runner.SetPaused(false);yield return new WaitForSecondsRealtime(.6f);Assert.That(debug.TraceCount,Is.Zero);
            runner.SetDebugBoxes(false,false);debug.RenderFrame();Assert.That(Object.FindObjectsByType<LineRenderer>().All(l=>!l.enabled),Is.True);
        }
        private static string F(float value)=>value.ToString("0.###",CultureInfo.InvariantCulture);
        private static Vector2? PanRange(SpriteRenderer renderer,BattleVector origin)
        {
            string p=Path.Combine(Application.dataPath,"_Project/Art/Paper/"+renderer.sprite.texture.name+".png");if(!File.Exists(p))return null;
            var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(p));var pixels=image.GetPixels32();
            float left=float.PositiveInfinity,right=float.NegativeInfinity;int count=0;var sprite=renderer.sprite;
            for(int y=(int)(image.height*.35f);y<image.height;y++)for(int x=0;x<image.width;x++)
            {
                var c=pixels[y*image.width+x];if(c.a<128||c.r<55||c.r>175||Mathf.Abs(c.r-c.g)>20||c.b<c.r*.9f||c.b>c.r*1.5f)continue;
                var point=renderer.transform.TransformPoint(new Vector3((x-sprite.pivot.x)/sprite.pixelsPerUnit*(renderer.flipX?-1:1),(y-sprite.pivot.y)/sprite.pixelsPerUnit,0));
                left=Mathf.Min(left,point.x-origin.X);right=Mathf.Max(right,point.x-origin.X);count++;
            }
            Object.Destroy(image);return count>100?new Vector2(left,right):(Vector2?)null;
        }
        private void CaptureReview(string name,BattleVector origin,float size)
        {
            var camera=Camera.main;var position=camera.transform.position;float zoom=camera.orthographicSize;var color=camera.backgroundColor;var flags=camera.clearFlags;int mask=camera.cullingMask;
            var target=camera.targetTexture;var active=RenderTexture.active;
            var renderers=Object.FindObjectsByType<Renderer>().Where(r=>r is LineRenderer||r.name=="Body"||r.name=="Projectile"||r.transform.IsChildOf(view.transform)).ToArray();
            var layers=renderers.Select(r=>r.gameObject.layer).ToArray();var texture=RenderTexture.GetTemporary(1024,768,24);var image=new Texture2D(1024,768,TextureFormat.RGBA32,false);
            try
            {
                foreach(var r in renderers)r.gameObject.layer=31;camera.cullingMask=1<<31;camera.orthographicSize=size;
                camera.transform.position=new Vector3(origin.X+1,origin.Y+.3f,-10);camera.backgroundColor=new Color(.09f,.12f,.17f);camera.clearFlags=CameraClearFlags.SolidColor;
                camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;image.ReadPixels(new Rect(0,0,1024,768),0,0);image.Apply();
                string folder=Path.Combine(Application.dataPath,"../../docs/screenshots/hitbox_review_chizuru");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
            }
            finally
            {
                for(int i=0;i<renderers.Length;i++)renderers[i].gameObject.layer=layers[i];
                camera.transform.position=position;camera.orthographicSize=zoom;camera.backgroundColor=color;camera.clearFlags=flags;camera.cullingMask=mask;camera.targetTexture=target;
                RenderTexture.active=active;RenderTexture.ReleaseTemporary(texture);Object.Destroy(image);
            }
        }
    }
}

