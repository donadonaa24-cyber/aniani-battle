using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AniBato.Battle;
using AniBato.Presentation;

internal static class PhaseSixBCameraReport
{
    public static int Write()
    {
        var rows = new List<object>();
        foreach (var resolution in new[] { (1920,1080), (1920,1200), (1440,1080) })
        {
            float aspect = (float)resolution.Item1/resolution.Item2;
            var stage = PhaseOneDefaults.Stage();
            var old = new CameraFrame(new BattleVector(0,2.6f),6,default);
            Add("before",old,stage.SpawnP1,stage.SpawnP2,36,true);
            var overview = CameraFraming.Calculate(stage,stage.SpawnP1,stage.SpawnP2,aspect,true,5.2f,1.2f,2,2.5f);
            Add("overview",overview,stage.SpawnP1,stage.SpawnP2,36,true);
            var combat = CameraFraming.Calculate(stage,stage.SpawnP1,stage.SpawnP2,aspect,false,5.2f,1.2f,2,2.5f);
            Add("combat",combat,stage.SpawnP1,stage.SpawnP2,36,true);
            var sim = new BattleSimulation(PhaseSixADefaults.Character(0),PhaseSixADefaults.Character(1),PhaseSixADefaults.Rules(),stage);
            while(!sim.Match.IsFinished) sim.Tick(default,default);
            var p1=new BattleVector(-1,1.1f); var p2=new BattleVector(1,1.1f);
            var shrink = CameraFraming.Calculate(sim.Stage,p1,p2,aspect,false,5.2f,1.2f,2,2.5f);
            Add("shrink",shrink,p1,p2,sim.Stage[0].Bounds.Size.X,false);
            void Add(string mode,CameraFrame frame,BattleVector first,BattleVector second,float floorWidth,bool upper)
            { rows.Add(new { mode,width=resolution.Item1,height=resolution.Item2,aspect,centerX=frame.Center.X,centerY=frame.Center.Y,size=frame.Size,
                p1X=first.X,p1Y=first.Y,p2X=second.X,p2Y=second.Y,floorWidth,upper,tomaHeightPercent=2.48f/(2*frame.Size)*100,renHeightPercent=2f/(2*frame.Size)*100 }); }
        }
        File.WriteAllText("TestResults/phase6b-camera-frames.json",JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
        System.Console.WriteLine("12 framing cases exported from production CameraFraming; offline previews, not Unity captures.");
        return 0;
    }
}
