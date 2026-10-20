using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    // CPU regression evidence, not an optimized human matchup ranking.
    public static class StaminaBalanceAudit
    {
        [Serializable] public sealed class Row
        {
            public int a,b,seed,ticks,winner,heartsA,heartsB,attacksA,attacksB,rejectedA,rejectedB,lowTicksA,lowTicksB;
            public float separation;
            public string reason;
        }
        [Serializable] public sealed class Summary
        {
            public string name;
            public int index,matches,wins,losses,draws,attacks,rejections,lowTicks,ticks,heartsRemaining;
            public float scorePercent,lowStaminaPercent,attacksPerMinute;
        }
        [Serializable] public sealed class Report
        { public string label,conditions; public Summary[] ranking; public Row[] matches; }
        public static void Run()
        {
            const string data="Assets/_Project/Data/";
            var setup=AssetDatabase.LoadAssetAtPath<BattleSetup>(data+"BattleSetup.asset");
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>(data+"BattleRules.asset").ToDefinition();
            var roster=setup.Characters.Select(c=>c.ToDefinition()).ToArray();
            var geometry=setup.Stages[0].ToGeometry();
            var platforms=Enumerable.Range(0,geometry.PlatformCount).Select(i=>geometry[i]).ToArray();
            var cpu=setup.CpuDifficulties[2].ToDefinition();var rows=new List<Row>();
            for(int a=0;a<roster.Length;a++) for(int b=a+1;b<roster.Length;b++)
            foreach(float gap in new[]{2f,8f,16f}) for(int seed=1;seed<=3;seed++) for(int swap=0;swap<2;swap++)
            {
                int left=swap==0?a:b,right=swap==0?b:a;
                var stage=new StageGeometry(platforms,geometry.LeftBlastLine,geometry.RightBlastLine,geometry.BottomBlastLine,
                    new BattleVector(-gap/2,geometry.SpawnP1.Y),new BattleVector(gap/2,geometry.SpawnP2.Y));
                var sim=new BattleSimulation(roster[left],roster[right],rules,stage);
                var brainA=new CpuBrain(cpu,(uint)(seed*97+left*13));var brainB=new CpuBrain(cpu,(uint)(seed*97+right*13));
                var row=new Row{a=left,b=right,seed=seed,separation=gap};
                int limit=rules.DurationTicks+roster[left].UltimateCutInFrames+roster[right].UltimateCutInFrames+10;
                bool previousRejectA=false,previousRejectB=false;
                for(int tick=0;tick<limit&&!sim.Match.IsFinished;tick++)
                {
                    sim.Tick(brainA.NextInput(sim,0),brainB.NextInput(sim,1));
                    bool rejectA=sim.P1.StaminaRejectedRemaining==30,rejectB=sim.P2.StaminaRejectedRemaining==30;
                    if(rejectA&&!previousRejectA)row.rejectedA++;if(rejectB&&!previousRejectB)row.rejectedB++;
                    previousRejectA=rejectA;previousRejectB=rejectB;
                    if(sim.CutInRemainingFrames==0)
                    {if(sim.P1.StaminaUnits<20*60)row.lowTicksA++;if(sim.P2.StaminaUnits<20*60)row.lowTicksB++;}
                    foreach(var e in sim.Events)if(e.Kind==BattleEventKind.AttackStarted)
                    {if(e.SourcePlayer==0)row.attacksA++;else row.attacksB++;}
                }
                if(!sim.Match.IsFinished)throw new InvalidOperationException("Audit match did not finish.");
                row.ticks=sim.Match.TickCount;row.heartsA=sim.P1.Hearts;row.heartsB=sim.P2.Hearts;row.reason=sim.Match.EndReason.ToString();
                row.winner=sim.Match.Result==MatchResult.Player1Wins?left:sim.Match.Result==MatchResult.Player2Wins?right:-1;rows.Add(row);
            }
            var summaries=roster.Select((c,i)=>
            {
                var games=rows.Where(r=>r.a==i||r.b==i).ToArray();
                var result=new Summary{index=i,name=c.Name,matches=games.Length,wins=games.Count(r=>r.winner==i),draws=games.Count(r=>r.winner==-1),ticks=games.Sum(r=>r.ticks),
                    attacks=games.Sum(r=>r.a==i?r.attacksA:r.attacksB),rejections=games.Sum(r=>r.a==i?r.rejectedA:r.rejectedB),
                    lowTicks=games.Sum(r=>r.a==i?r.lowTicksA:r.lowTicksB),heartsRemaining=games.Sum(r=>r.a==i?r.heartsA:r.heartsB)};
                result.losses=result.matches-result.wins-result.draws;result.scorePercent=100f*(result.wins+result.draws*.5f)/result.matches;
                result.lowStaminaPercent=100f*result.lowTicks/Math.Max(1,result.ticks);result.attacksPerMinute=3600f*result.attacks/Math.Max(1,result.ticks);return result;
            }).OrderByDescending(r=>r.scorePercent).ToArray();
            string label=Environment.GetEnvironmentVariable("ANIBATO_BALANCE_LABEL")??"current";
            if(label.Any(c=>!char.IsLetterOrDigit(c)&&c!='_'))throw new ArgumentException("Invalid audit label.");
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));string stem=Path.Combine(root,"docs/codex_logs/stamina_"+label);
            var report=new Report{label=label,conditions="648 matches; hard CPU; 36 pairs x mirrored seats x 3 seeds x distances 2/8/16; live shrink; initial gauge 0. CPU usually taps charges. Human playtesting still required.",ranking=summaries,matches=rows.ToArray()};
            File.WriteAllText(stem+".json",JsonUtility.ToJson(report,true),new UTF8Encoding(false));
            var text=new StringBuilder("# スタミナ比較 "+label+"\n\n"+report.conditions+"\n\n|キャラ|勝/負/分|得点率|ST20未満時間|拒否回数|攻撃/分|\n|---|---|---:|---:|---:|---:|\n");
            foreach(var r in summaries)text.AppendLine(string.Format(CultureInfo.InvariantCulture,"|{0}|{1}/{2}/{3}|{4:F1}%|{5:F1}%|{6}|{7:F1}|",r.name,r.wins,r.losses,r.draws,r.scorePercent,r.lowStaminaPercent,r.rejections,r.attacksPerMinute));
            File.WriteAllText(stem+".md",text.ToString(),new UTF8Encoding(false));Debug.Log(text.ToString());
        }
    }
}
