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
        public IEnumerator ChizuruHasRiceInPreparationAndAnEmptyThrowingHandFromTheFirstProjectileTick()
        {
            var data=runner.Setup.Character(2);var keys=controller.Bindings.P1;
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(data,runner.Setup.Character(0),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P2,"Position",new BattleVector(sim.Stage[0].Bounds.Right-1,sim.P2.Position.Y));
                yield return Key(keys.Special,true);Tick();yield return Key(keys.Special,false);
                var attack=sim.P1.Attack;Assert.That(attack,Is.Not.Null);var pose=data.PaperMotion.Find(AnimationState.SpecialNeutral);
                bool before=false,released=false;int limit=0;
                while(sim.P1.Attack==attack&&limit++<100)
                {
                    bool emitted=sim.Projectiles.Any(p=>p.Attack==attack);
                    Assert.That(body.sprite,Is.SameAs(pose.Frames[emitted?1:0]));
                    if(!emitted&&!before){before=true;Capture("千鶴_おにぎり_構え_"+facing,true,"paper_release");}
                    if(emitted&&!released){released=true;Capture("千鶴_おにぎり_投げ終わり_"+facing,true,"paper_release");}
                    Tick();
                }
                Assert.That(before&&released,Is.True);Assert.That(limit,Is.LessThan(100));
                Assert.That(body.sprite,Is.SameAs(data.PaperMotion.Sprite));
            }
        }
        [UnityTest]
        public IEnumerator UltimateIllustrationLingersAndCurryFadesWithoutExtendingDamageFrames()
        {
            foreach(int index in new[]{0,2})
            {
                var data=runner.Setup.Character(index);var keys=controller.Bindings.P1;
                runner.BeginMatch(data,runner.Setup.Character(1),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                Set(sim.P2,"Position",new BattleVector(sim.Stage[0].Bounds.Right-1,sim.P2.Position.Y));
                yield return Key(keys.Ultimate,true);Tick();yield return Key(keys.Ultimate,false);
                var attack=sim.P1.Attack;Assert.That(attack,Is.Not.Null);
                var pose=data.PaperMotion.Find(AnimationState.Ultimate);int impactTick=-1,limit=0,tailFrames=0,lingerFrames=0;
                bool activeSeen=false;var wave=view.transform.Find("Curry Wave").GetComponent<SpriteRenderer>();
                int activeDefinition=attack.Definition.ActiveFrames;
                while(limit++<260)
                {
                    if(sim.P1.Attack==attack&&attack.IsDamageActive)
                    {if(impactTick<0)impactTick=sim.Match.TickCount;activeSeen=true;}
                    if(impactTick>=0)
                    {
                        int age=sim.Match.TickCount-impactTick;
                        if(age<data.PaperMotion.UltimateImpactMinimumTicks)
                        {
                            Assert.That(body.sprite,Is.SameAs(pose.Frames.Last()));
                            if(sim.P1.Attack==null)lingerFrames++;
                        }
                        else if(sim.P1.Attack==null){Assert.That(body.sprite,Is.SameAs(data.PaperMotion.Sprite));break;}
                        if(index==2&&!attack.IsActive&&wave.enabled)
                        {
                            Assert.That(wave.color.a,Is.GreaterThan(0).And.LessThan(1));tailFrames++;
                            if(tailFrames==1)Capture("千鶴_カレー余韻",true,"paper_release");
                        }
                    }
                    Assert.That(attack.Definition.ActiveFrames,Is.EqualTo(activeDefinition));Tick();
                }
                Assert.That(activeSeen,Is.True);Assert.That(limit,Is.LessThan(260));Assert.That(lingerFrames,Is.GreaterThan(0));
                if(index==2)Assert.That(tailFrames,Is.EqualTo(data.PaperMotion.CurryAfterglowTicks-1));
            }
        }
    }
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest]
        public IEnumerator HorizontalMainMenuAndTwoRowsFollowTheVisibleCardsWithActualKeyboardInput()
        {
            yield return Tap(keyboard.enterKey);Assert.That(Flow.Screen,Is.EqualTo(GameScreen.MainMenu));
            yield return Tap(keyboard.dKey);Assert.That(Flow.MenuIndex,Is.EqualTo(1));
            yield return Tap(keyboard.aKey);Assert.That(Flow.MenuIndex,Is.Zero);
            yield return Tap(keyboard.enterKey);Assert.That(Flow.Screen,Is.EqualTo(GameScreen.CharacterSelect));
            Flow.SelectRosterCard(5);yield return null; // Leave cursor at Yui; same-team duplicate rejection stays enabled.
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(1));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(4));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(0));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(2));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(8));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(5));
            yield return Tap(keyboard.wKey);Assert.That(Flow.Player1Index,Is.EqualTo(6));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(3));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(7));
            yield return Tap(keyboard.dKey);Assert.That(Flow.Player1Index,Is.EqualTo(6));
            yield return Tap(keyboard.sKey);Assert.That(Flow.Player1Index,Is.EqualTo(1));
            for(int height=720;height<=960;height+=120)CaptureScreen("07_二段選択_"+height,height,"paper_release");
        }
    }
}
