using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator AllNineRecoveriesUseRealUpInputOneIllustrationAndTheirOwnCooldownOnBothSides()
        {
            var keys = controller.Bindings.P1;
            for (int index = 0; index < 9; index++) foreach (int side in new[] { 1, -1 })
            {
                var data = runner.Setup.Character(index); runner.BeginMatch(data, runner.Setup.Character(2), runner.CurrentStage, false);
                var fighter = runner.Simulation.P1; Set(fighter, "Facing", side); Set(fighter, "Position", new BattleVector(0, 5));
                Set(fighter, "Grounded", false); Set(fighter, "GroundPlatform", -1);
                yield return Key(keys.Up, true); yield return Key(keys.Special, true); Tick();
                yield return Key(keys.Special, false); yield return Key(keys.Up, false);
                Assert.That(fighter.CurrentSpecial, Is.EqualTo(SpecialSlot.Up)); Assert.That(fighter.UpSpecialUsed, Is.True);
                for (int tick = 0; tick < fighter.Attack.Definition.StartupFrames + 1; tick++) Tick();
                view.RenderFrame(); Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.SpecialUp));
                Assert.That(view.UsesPaperSprite, Is.True); Assert.That(fighter.Position.Y, Is.GreaterThan(4));
                if (new[] {0,2,7,8}.Contains(index))
                {
                    Assert.That(data.PaperMotion.UpRecovery.Valid, Is.True);
                    Assert.That(data.PaperMotion.UpRecovery.Frames.Length, Is.EqualTo(1));
                    Assert.That(body.sprite, Is.SameAs(data.PaperMotion.UpRecovery.Frames[0]));
                }
                else if (index == 1 || index == 5 || index == 6)
                    Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(AnimationState.SpecialNeutral).Frames[1]));
                else Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(AnimationState.Dodge).Frames[0]));
                if (index == 3 || index == 4) Assert.That(body.color.b, Is.GreaterThan(.9f));
                Object.FindAnyObjectByType<AniBato.Game.HUDController>().SendMessage("LateUpdate");
                var hud = Object.FindObjectsByType<Text>().Single(t => t.name == "Player 1 HUD");
                Assert.That(hud.text, Does.Contain("↑")); Assert.That(hud.text, Does.Contain("秒"));
                Capture("recovery_" + index + "_" + side, true, "directional_recovery");
            }
        }
        [UnityTest]
        public IEnumerator AllNineReturnFromBelowBothLedgesThroughRealKeyboardInput()
        {
            var keys=controller.Bindings.P1;
            for(int index=0;index<9;index++)foreach(int side in new[]{-1,1})
            {
                runner.BeginMatch(runner.Setup.Character(index),runner.Setup.Character(2),runner.CurrentStage,false);
                var sim=runner.Simulation; var fighter=sim.P1; var floor=sim.Stage[0].Bounds;
                Set(fighter,"Position",new BattleVector(side<0?floor.Left-2:floor.Right+2,floor.Top-3));
                Set(fighter,"Grounded",false);Set(fighter,"GroundPlatform",-1);Set(fighter,"Velocity",default(BattleVector));
                var travel=side<0?keys.Right:keys.Left;
                yield return Key(travel,true); yield return Key(keys.Up,true); yield return Key(keys.Special,true); Tick();
                yield return Key(keys.Special,false); bool landed=false;
                for(int tick=0;tick<180&&!sim.Match.IsFinished;tick++)
                {
                    Tick();
                    if(fighter.Grounded&&fighter.Position.X>floor.Left&&fighter.Position.X<floor.Right){landed=true;break;}
                }
                Assert.That(landed,Is.True,index+" side "+side+" at "+fighter.Position.X+","+fighter.Position.Y);
                Assert.That(sim.Match.IsFinished,Is.False);Assert.That(fighter.Hearts,Is.EqualTo(sim.Rules.InitialHearts));
                Capture("returned_"+index+"_"+side,false,"directional_recovery");
                yield return Key(keys.Up,false);yield return Key(travel,false);
            }
        }
    }
}
