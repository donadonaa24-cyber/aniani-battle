using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class PracticePlaybackTests
    {
        [TestCase(30)] [TestCase(60)] [TestCase(144)]
        public void HalfQuarterAndSteppedPlaybackKeepTheSameInputTimeline(int fps)
        {
            var outcomes=new List<string>();var rates=new[]{1f,.5f,.25f};
            for(int speed=0;speed<4;speed++)
            {
                var sim=BattleTestFactory.Create();var clock=new FixedTickClock();var events=new List<string>();
                System.Action tick=()=>{if(sim.Match.TickCount>=180)return;int n=sim.Match.TickCount;
                    sim.Tick(new FrameInput(n<65?1:0,pressed:n==75||n==110?InputButtons.Attack:InputButtons.None),default);
                    events.AddRange(sim.Events.Select(e=>e.Tick+"/"+e.Kind+"/"+e.Value));};
                if(speed==3)for(int n=0;n<180;n++){Assert.That(PracticePlayback.Scale(1d/fps,0,true,rates),Is.Zero);tick();}
                else while(sim.Match.TickCount<180)clock.Advance(PracticePlayback.Scale(1d/fps,speed,false,rates),tick,5);
                outcomes.Add(sim.P1.Position.X.ToString("R",CultureInfo.InvariantCulture)+"/"+sim.P2.Hearts+"/"+sim.P2.NormalHitCount+"/"+sim.Match.RemainingTicks+"/"+string.Join(",",events));
            }
            Assert.That(outcomes.Distinct().Count(),Is.EqualTo(1));
        }
        [Test]
        public void ClaudeReachRowsAndGaugeStayExactlyAtTheSavedValuesAfterBuildAll()
        {
            var rows=File.ReadAllLines("../docs/HITBOX_REACH_20261015.csv").Skip(1).Where(l=>!string.IsNullOrWhiteSpace(l)).ToArray();
            Assert.That(rows.Select(l=>l.Split(',')[0]).Distinct().Count(),Is.EqualTo(45));
            foreach(var row in rows)
            {
                var c=row.Split(',');var asset=AssetDatabase.LoadAssetAtPath<AttackData>("Assets/_Project/Data/"+c[0]+".asset");
                var box=c[1]=="secondary"?asset.Definition.SecondaryHitbox:asset.Definition.Hitbox;
                Assert.That(box.Left,Is.EqualTo(float.Parse(c[4],CultureInfo.InvariantCulture)).Within(.0001),c[0]);
                Assert.That(box.Right,Is.EqualTo(float.Parse(c[5],CultureInfo.InvariantCulture)).Within(.0001),c[0]);
            }
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset");
            Assert.That(rules.Definition.Gauge.OnHit,Is.EqualTo(14));Assert.That(rules.Definition.Gauge.OnDamaged,Is.EqualTo(10));
        }
    }
}
