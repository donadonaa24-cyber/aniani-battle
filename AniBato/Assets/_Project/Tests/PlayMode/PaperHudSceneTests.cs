using System.Collections;
using System.IO;
using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator SavedPaperHudShowsEveryNameAndUpdatesBothGaugesAfterRealInput()
        {
            var hud=Object.FindAnyObjectByType<HUDController>();
            var cards=Object.FindObjectsByType<PaperFighterHud>().OrderBy(c=>c.name).ToArray();
            Assert.That(cards.Length,Is.EqualTo(2));
            foreach(var card in cards)
            {
                Assert.That(card.transform.Find("Opaque Paper Base").GetComponent<PaperHudShape>().color.a,Is.EqualTo(1));
                Assert.That(card.GetComponentsInChildren<PaperHudShape>().All(s=>!s.raycastTarget),Is.True);
                Assert.That(card.GetComponentsInChildren<PaperHudShape>().All(s=>s.GetComponent<CanvasRenderer>()!=null),Is.True,"紙台紙の描画部品はシーン再読込後も必要");
            }
            for(int i=0;i<9;i++)
            {
                runner.BeginMatch(runner.Setup.Character(i),runner.Setup.Character((i+1)%9),runner.Setup.Stages[i%3],false);
                hud.SendMessage("LateUpdate"); Canvas.ForceUpdateCanvases();
                Assert.That(cards[0].NameLabel.text,Is.EqualTo(runner.Simulation.P1.Definition.Name));
                var ruby=cards[0].transform.Find("English Ruby").GetComponent<Text>();
                Assert.That(ruby.text,Is.EqualTo(PaperFighterHud.EnglishName(runner.Simulation.P1.Definition.Name)));
                Assert.That(ruby.text,Does.Match("^[A-Z]+$"));
                Assert.That(ruby.rectTransform.anchoredPosition.y,Is.GreaterThan(cards[0].NameLabel.rectTransform.anchoredPosition.y));
                Assert.That(cards[0].StaminaFraction,Is.EqualTo(1));
                Assert.That(cards[0].LinkFraction,Is.Zero);
                foreach(var text in cards.SelectMany(c=>c.GetComponentsInChildren<Text>()).Where(t=>!string.IsNullOrEmpty(t.text)))
                    Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(text.rectTransform.rect.height),text.name+"の日本語フォント行高が枠に収まる");
            }
            runner.BeginMatch(runner.Setup.Character(0),runner.Setup.Character(1),runner.Setup.Stages[0],false);
            yield return Key(controller.Bindings.P1.Attack,true); Tick(); yield return Key(controller.Bindings.P1.Attack,false);
            for(int i=0;i<30;i++)Tick(); hud.SendMessage("LateUpdate");
            Assert.That(runner.Simulation.P1.Stamina,Is.EqualTo(93));
            Assert.That(cards[0].StaminaFraction,Is.EqualTo(.93f).Within(.0001));
            Assert.That(cards[0].transform.Find("ST Paper Fill").GetComponent<PaperHudShape>().rectTransform.rect.width,Is.EqualTo(266*.93f).Within(.001));
            Assert.That(cards[1].StaminaFraction,Is.EqualTo(1));
            Set(runner.Simulation.P1,"Gauge",50); Set(runner.Simulation.P1,"Hearts",1); Set(runner.Simulation.P1,"NormalHitCount",2);
            hud.SendMessage("LateUpdate");
            Assert.That(cards[0].LinkFraction,Is.EqualTo(.5f));
            Assert.That(cards[0].transform.Find("LINK Paper Fill").GetComponent<PaperHudShape>().rectTransform.rect.width,Is.EqualTo(133));
            Assert.That(cards[0].transform.Find("Paper Heart 0").GetComponent<PaperHudShape>().color,Is.EqualTo(PaperFighterHud.Red));
            Assert.That(cards[0].transform.Find("Paper Heart 1").GetComponent<PaperHudShape>().color,Is.Not.EqualTo(PaperFighterHud.Red));
            var upper=runner.Simulation.Stage[1].Bounds;
            Set(runner.Simulation.P1,"Position",new AniBato.Battle.BattleVector(upper.Center.X,upper.Top+runner.Simulation.P1.Definition.HurtboxSize.Y*.5f));
            runner.RenderState.Reset(runner.Simulation);view.RenderFrame();
            foreach(var shape in new[]{new Vector2Int(1920,1080),new Vector2Int(1600,1200),new Vector2Int(2520,1080)})
            {
                yield return CapturePaperHud(shape.x,shape.y);
                Assert.That(runner.Simulation.P1.Stamina,Is.EqualTo(93));
                Assert.That(runner.Simulation.P1.Gauge,Is.EqualTo(50));
            }
        }
        private IEnumerator CapturePaperHud(int width,int height)
        {
            var camera=Camera.main;var canvas=Object.FindAnyObjectByType<HUDController>().GetComponent<Canvas>();
            var target=RenderTexture.GetTemporary(width,height,24);var previous=camera.targetTexture;
            var active=RenderTexture.active;var mode=canvas.renderMode;var canvasCamera=canvas.worldCamera;
            var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                yield return null; Canvas.ForceUpdateCanvases(); yield return null;
                foreach(var card in Object.FindObjectsByType<PaperFighterHud>())
                {
                    var mesh=card.transform.Find("Opaque Paper Base").GetComponent<CanvasRenderer>();
                    Assert.That(mesh.cull,Is.False);Assert.That(mesh.GetMesh().vertexCount,Is.GreaterThan(3));
                    foreach(var text in card.GetComponentsInChildren<Text>().Where(t=>!string.IsNullOrEmpty(t.text)))
                        Assert.That(text.canvasRenderer.GetMesh().vertexCount,Is.GreaterThan(3),text.name+"は文字列だけでなく実頂点も描画する");
                }
                camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                var corners=new Vector3[4];
                foreach(var card in Object.FindObjectsByType<PaperFighterHud>())
                {
                    card.GetComponent<RectTransform>().GetWorldCorners(corners);
                    foreach(var corner in corners){var p=camera.WorldToViewportPoint(corner);Assert.That(p.x,Is.InRange(0f,1f));Assert.That(p.y,Is.InRange(0f,1f));}
                }
                var fighter=runner.Simulation.P1;var head=camera.WorldToViewportPoint(new Vector3(fighter.Position.X,fighter.Position.Y+fighter.Definition.HurtboxSize.Y*.5f,0));
                Assert.That(head.y,Is.LessThan(1-PaperFighterHud.SafeTopHeight/canvas.GetComponent<RectTransform>().rect.height),"上段のキャラは不透明HUDの下に収まる");
                Assert.That(head.y,Is.GreaterThan(0));
                CaptureFile.Write(Path.GetFullPath(Path.Combine(Application.dataPath,"../../docs/screenshots/paper_hud",width+"x"+height+".png")),image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode=mode;canvas.worldCamera=canvasCamera;camera.targetTexture=previous;RenderTexture.active=active;
                RenderTexture.ReleaseTemporary(target);Object.Destroy(image);
            }
        }
    }
}
