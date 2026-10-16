using System.Collections;
using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest]
        public IEnumerator SameGeneratedEnsembleRemainsSelectableDuringTheFade()
        {
            yield return Tap(keyboard.enterKey);yield return new WaitForSecondsRealtime(1.3f);
            var decor=Object.FindAnyObjectByType<PaperScreenDecor>();
            Assert.That(decor.Artwork,Is.Not.Null);Assert.That(decor.EnsembleAlpha,Is.EqualTo(1).Within(.01f));
            CaptureScreen("01_集合トップ",720,"rework_r5");
            yield return Tap(keyboard.enterKey);
            var grid=Object.FindAnyObjectByType<PaperRosterGrid>();Assert.That(grid.Artwork,Is.SameAs(decor.Artwork));
            yield return new WaitForSecondsRealtime(.6f);Assert.That(decor.EnsembleAlpha,Is.EqualTo(.66f).Within(.01f));
            foreach(int height in new[]{720,800,960})CaptureScreen("07_集合選択_"+height,height,"rework_r5");
            var cards=Object.FindObjectsByType<Image>().Where(i=>i.name.StartsWith("Roster Card ")).ToArray();
            Assert.That(cards.Length,Is.EqualTo(9));
            foreach(var card in cards)
            {
                int i=int.Parse(card.name.Substring("Roster Card ".Length));
                Assert.That(card.rectTransform.anchoredPosition,Is.EqualTo(grid.Artwork.Position(i)));
            }
            yield return Tap(keyboard.sKey);Assert.That(Flow.Player1Index,Is.EqualTo(3));
            yield return Tap(keyboard.jKey);yield return Pick(0,4,8,2,7);
            Assert.That(Flow.Screen,Is.EqualTo(GameScreen.StageSelect));
        }
        [UnityTest]
        public IEnumerator GeneratedDeckSurfaceMatchesPhysicsAtAllThemesAndAspectRatios()
        {
            yield return SelectLocalBattle();Flow.enabled=false;var runner=Flow.Runner;runner.SetPaused(true);
            var stageView=Object.FindAnyObjectByType<StageView>();
            for(int theme=0;theme<3;theme++)
            {
                var data=runner.Setup.Stages[theme];runner.BeginMatch(runner.Setup.Character(2),runner.Setup.Character(0),data,false);
                stageView.Rebuild(data);yield return null;yield return null;
                var sim=runner.Simulation;int tick=sim.Match.TickCount;var p=sim.P1.Position;
                Assert.That(Object.FindAnyObjectByType<GeneratedStageScenery>(),Is.Not.Null);
                foreach(float aspect in new[]{16f/9,16f/10,4f/3})
                {
                    Camera.main.aspect=aspect;yield return null;yield return null;
                    for(int i=0;i<3;i++)
                    {
                        var floor=Object.FindObjectsByType<SpriteRenderer>().Single(s=>s.name=="Platform "+i);
                        Assert.That(floor.bounds.max.y,Is.EqualTo(sim.Stage[i].Bounds.Top).Within(.0001f));
                        Assert.That(floor.bounds.size.x,Is.EqualTo(sim.Stage[i].Bounds.Size.X).Within(.0001f));
                        Assert.That(floor.sprite.texture,Is.SameAs(i==0?data.GeneratedArt.Main.texture:data.GeneratedArt.Upper.texture));
                    }
                    Assert.That(sim.Match.TickCount,Is.EqualTo(tick));Assert.That(sim.P1.Position,Is.EqualTo(p));
                    CaptureScreen("02_生成舞台_"+theme+"_"+Mathf.RoundToInt(aspect*100),Mathf.RoundToInt(1280/aspect),"rework_r5");
                }
                while(sim.Shrink.CompletedSteps<1&&!sim.Match.IsFinished)sim.Tick(default,default);
                yield return null;yield return null;
                Assert.That(Object.FindObjectsByType<SpriteRenderer>().Any(r=>r.name.StartsWith("Generated Removed Deck ")&&r.enabled),Is.True);
                CaptureScreen("03_縮小紙片_"+theme,720,"rework_r5");
            }
        }
    }
}
