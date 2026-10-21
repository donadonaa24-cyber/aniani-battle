using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator EveryFighterUsesRealAirJumpAndUpRecoveryKeysFromDeepBelowBothEdges()
        {
            for(int index=0;index<9;index++)foreach(int side in new[]{-1,1})
            {
                runner.BeginMatch(runner.Setup.Character(index),runner.Setup.Character(0),runner.Setup.Stages[0],false);
                var f=runner.Simulation.P1;var floor=runner.Simulation.Stage[0].Bounds;
                Set(f,"Position",new BattleVector((side<0?floor.Left:floor.Right)+side*2.2f,floor.Top-5));
                Set(f,"Grounded",false);Set(f,"GroundPlatform",-1);Set(f,"Mode",FighterMode.Fall);Set(f,"JumpsUsed",1);
                yield return Key(controller.Bindings.P1.Jump,true);Tick();yield return Key(controller.Bindings.P1.Jump,false);
                for(int tick=0;tick<20;tick++)Tick();
                var inward=side<0?controller.Bindings.P1.Right:controller.Bindings.P1.Left;
                yield return Key(inward,true);yield return Key(controller.Bindings.P1.Up,true);yield return Key(controller.Bindings.P1.Special,true);Tick();
                yield return Key(controller.Bindings.P1.Special,false);
                for(int tick=0;tick<240&&!f.Grounded&&!runner.Simulation.Match.IsFinished;tick++)Tick();
                yield return Key(inward,false);yield return Key(controller.Bindings.P1.Up,false);
                Assert.That(f.Grounded,Is.True,index+" side="+side+" "+f.Position.X+","+f.Position.Y+" "+f.Mode);
                Assert.That(runner.Simulation.Match.IsFinished,Is.False);
            }
        }
        [UnityTest]
        public IEnumerator AllNineWholeIllustrationsAnchorTheGrippingHandToEitherLedgeAndExitWithRealKeys()
        {
            for(int index=0;index<9;index++)foreach(int side in new[]{-1,1})
            {
                runner.BeginMatch(runner.Setup.Character(index),runner.Setup.Character(0),runner.Setup.Stages[0],false);
                var f=runner.Simulation.P1;var floor=runner.Simulation.Stage[0].Bounds;
                Set(f,"Position",new BattleVector((side<0?floor.Left:floor.Right)+side*(f.Definition.HurtboxSize.X*.5f+.1f),floor.Top-f.Definition.HurtboxSize.Y*.5f-.1f));
                Set(f,"Grounded",false);Set(f,"GroundPlatform",-1);Set(f,"Mode",FighterMode.Fall);
                Tick();runner.RenderState.Reset(runner.Simulation);view.RenderFrame();
                Assert.That(f.Mode,Is.EqualTo(FighterMode.LedgeHang));
                Assert.That(view.UsesPaperSprite,Is.True);Assert.That(body.sprite,Is.SameAs(runner.Character(0).PaperMotion.LedgeHang));
                Assert.That(view.RenderedSample.State,Is.EqualTo(Presentation.AnimationState.LedgeHang));
                Assert.That(body.flipX,Is.EqualTo(side>0));Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one));
                // Sprite origin is the native-pixel contact of the single gripping hand, not the shoes.
                Assert.That(body.transform.position.x,Is.EqualTo(f.LedgePoint.X-side*.06f).Within(.0001));
                Assert.That(body.transform.position.y,Is.EqualTo(floor.Top).Within(.0001));
                var camera=Camera.main;var oldPosition=camera.transform.position;float oldSize=camera.orthographicSize;
                camera.transform.position=new Vector3(f.LedgePoint.X,floor.Top-.8f,-10);camera.orthographicSize=3.3f;
                Capture("character_"+index+"_side_"+side,false,"paper_ledge");
                camera.transform.position=oldPosition;camera.orthographicSize=oldSize;
                var key=side<0?controller.Bindings.P1.Right:controller.Bindings.P1.Left;
                yield return Key(key,true);Tick();yield return Key(key,false);
                for(int i=0;i<30&&!f.Grounded;i++)Tick();
                Assert.That(f.Grounded,Is.True,index+" side="+side);Assert.That(f.Attack,Is.Null);
                Assert.That(Object.FindObjectsByType<PaperFighterHud>().Length,Is.EqualTo(2));
            }
        }
        [UnityTest]
        public IEnumerator LedgeHudDescribesTheStaminaDrainAndTheJumpKeyReallyRecovers()
        {
            var f=runner.Simulation.P1;var floor=runner.Simulation.Stage[0].Bounds;
            Set(f,"Position",new BattleVector(floor.Left-.8f,floor.Top-f.Definition.HurtboxSize.Y*.5f-.1f));
            Set(f,"Grounded",false);Set(f,"GroundPlatform",-1);Set(f,"Mode",FighterMode.Fall);
            Tick();Assert.That(f.Mode,Is.EqualTo(FighterMode.LedgeHang));
            for(int i=0;i<60;i++)Tick();
            var hud=Object.FindAnyObjectByType<HUDController>();hud.SendMessage("LateUpdate");
            var label=hud.GetComponentsInChildren<UnityEngine.UI.Text>().Single(t=>t.name=="P1 Status");
            float expectedSeconds=Mathf.Min((runner.Simulation.Rules.Ledge.MaximumHoldFrames-60)/60f,
                (f.Definition.Stamina.Maximum-25)/(float)runner.Simulation.Rules.Ledge.StaminaPerSecond);
            Assert.That(label.text,Does.Contain("崖 ST消費").And.Contain("内側で復帰").And.Contain(expectedSeconds.ToString("0.0")+"秒"));
            Assert.That(f.Stamina,Is.EqualTo(f.Definition.Stamina.Maximum-25));
            yield return Key(controller.Bindings.P1.Jump,true);Tick();yield return Key(controller.Bindings.P1.Jump,false);
            for(int i=0;i<100&&!f.Grounded;i++)Tick();
            Assert.That(f.Grounded,Is.True);Assert.That(f.LedgeRegrabBlocked,Is.False);
        }
    }
}
