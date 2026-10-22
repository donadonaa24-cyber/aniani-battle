using System.Collections;
using System.Reflection;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest]
        public IEnumerator AllNineFightersEmergeFromSupportRiftsDuringCountdownWithoutMovingSimulation()
        {
            yield return CpuToCountdown();
            var flow=Flow;flow.enabled=false;
            var elapsed=typeof(GameFlowController).GetField("countdownElapsed",BindingFlags.Instance|BindingFlags.NonPublic);
            var settings=(GameFlowSettings)typeof(GameFlowController).GetField("settings",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(flow);
            for(int i=0;i<9;i++)
            {
                var data=flow.Setup.Character(i);
                flow.Runner.BeginMatch(data,data,flow.Setup.Stages[0],false);flow.Runner.SetPaused(true);
                yield return null;
                var sim=flow.Runner.Simulation;var spawn=sim.P1.Position;
                foreach(float progress in new[]{0f,.45f,1f})
                {
                    elapsed.SetValue(flow,settings.CountdownNumberSeconds*1.5f*progress);
                    foreach(var view in Object.FindObjectsByType<FighterView>())
                    {
                        view.RenderFrame();var body=view.transform.Find("Body").GetComponent<SpriteRenderer>();
                        var rift=view.transform.Find("Fighter Entrance Rift").GetComponent<SpriteRenderer>();
                        if(progress==0)Assert.That(body.color.a,Is.Zero);
                        if(progress==.45f)
                        {
                            Assert.That(rift.enabled,Is.True);Assert.That(rift.sprite,Is.SameAs(flow.Setup.SupportArt.Rift));
                            Assert.That(body.color.a,Is.GreaterThan(0).And.LessThan(1));
                        }
                        if(progress==1){Assert.That(rift.enabled,Is.False);Assert.That(body.color.a,Is.EqualTo(1));}
                    }
                    Assert.That(sim.Match.TickCount,Is.Zero);Assert.That(sim.P1.Position,Is.EqualTo(spawn));Assert.That(sim.P1.Attack,Is.Null);
                    if(progress==.45f)CaptureScreen("入場_"+data.Movement.Name,720,"paper_entrance");
                }
            }
            elapsed.SetValue(flow,settings.CountdownNumberSeconds*3);
            flow.enabled=true;
            Assert.That(flow.CountdownText,Is.EqualTo("FIGHT!"));
            yield return WaitFor(()=>flow.Screen==GameScreen.Battle,"裂け目演出後も従来のFIGHT終了で操作解禁する");
            Assert.That(flow.Runner.Paused,Is.False);
        }
    }
}
