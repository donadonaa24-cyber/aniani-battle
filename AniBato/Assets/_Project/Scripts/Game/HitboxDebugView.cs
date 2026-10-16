using System.Collections.Generic;
using AniBato.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    // An observer only. Boxes always use the simulation's world coordinates.
    public sealed class HitboxDebugView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private PresentationSettings presentation;
        private readonly List<LineRenderer> lines=new List<LineRenderer>();
        private readonly List<Trace> traces=new List<Trace>();
        private sealed class Trace { public BattleRect Rect;public Color Color;public float Seconds; }
        private Material material;
        private Text legend;
        private int used;
        public int TraceCount => traces.Count;
        public void Configure(BattleRunner value,PresentationSettings settings){runner=value;presentation=settings;}
        private void OnEnable(){if(runner){runner.EventProduced+=OnEvent;runner.MatchRestarted+=ClearTraces;}}
        private void OnDisable(){if(runner){runner.EventProduced-=OnEvent;runner.MatchRestarted-=ClearTraces;}if(legend)legend.enabled=false;}
        private void ClearTraces(){traces.Clear();}
        private void Start()
        {
            var go=new GameObject("Hitbox Legend Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));go.transform.SetParent(transform,false);
            go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;go.GetComponent<Canvas>().sortingOrder=28;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);
            var label=new GameObject("Hitbox Legend",typeof(RectTransform),typeof(Text));label.transform.SetParent(go.transform,false);
            var rect=label.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.02f,0);rect.anchorMax=new Vector2(.98f,0);rect.sizeDelta=new Vector2(0,68);rect.anchoredPosition=new Vector2(0,46);
            legend=label.GetComponent<Text>();legend.font=RuntimeFonts.Japanese;legend.fontSize=22;legend.alignment=TextAnchor.MiddleCenter;
            legend.supportRichText=true;legend.color=Color.white;legend.raycastTarget=false;
            var shadow=label.AddComponent<Outline>();shadow.effectColor=Color.black;shadow.effectDistance=new Vector2(2,-2);
        }
        private void OnEvent(BattleEvent value)
        {
            if(value.Kind!=BattleEventKind.Hit&&value.Kind!=BattleEventKind.Guarded&&value.Kind!=BattleEventKind.GuardBroken)return;
            if(value.SourcePlayer<0||value.TargetPlayer<0)return;
            var sim=runner.Simulation;var source=value.SourcePlayer==0?sim.P1:sim.P2;
            AttackInstance attack=source.Attack;
            foreach(var shot in sim.Projectiles)if(shot.Attack.Id==value.AttackId){Remember(shot.Hitbox,Color.yellow);attack=null;break;}
            if(attack!=null&&attack.Id==value.AttackId&&!attack.Definition.NonDamaging&&!attack.Definition.SpawnProjectile)
            {
                Remember(attack.WorldHitbox(source),new Color(1,.4f,.3f));
                if(attack.Definition.HasSecondaryHitbox)Remember(attack.Definition.SecondaryHitbox.At(source.Position,source.Facing),new Color(1,.4f,.3f));
            }
            Remember((value.TargetPlayer==0?sim.P1:sim.P2).Hurtbox,Color.green);
        }
        private void Remember(BattleRect rect,Color color)
        {
            if(traces.Count>=32)traces.RemoveAt(0);
            traces.Add(new Trace{Rect=rect,Color=color,Seconds=presentation.DebugTraceSeconds});
        }
        private void LateUpdate()
        {
            if(!runner.Paused&&!runner.PracticePlaybackPaused)
                for(int i=traces.Count-1;i>=0;i--){traces[i].Seconds-=Time.unscaledDeltaTime;if(traces[i].Seconds<=0)traces.RemoveAt(i);}
            RenderFrame();
        }
        public void RenderFrame()
        {
            used=0;bool show=runner&&runner.DebugBoxes&&runner.Simulation!=null;
            if(legend)
            {
                legend.enabled=show;
                legend.text="<color=#78ff8a>緑：やられ判定</color>　<color=#ff6868>赤：攻撃有効</color>　<color=#ffe265>黄：飛び道具</color>　<color=#ffaaaa>薄赤：予備範囲</color>　残像：命中時の位置（0.5秒）　F9 表示切替";
                if(runner.Practice!=null)legend.text+="\nF5 速度："+runner.PracticeRateLabel+"　F6 "+(runner.PracticePlaybackPaused?"静止中":"再生中")+"　F7 1コマ進める";
                legend.text=WebInputPolicy.Text(legend.text);
            }
            if(show)
            {
                DrawFighter(runner.Simulation.P1);DrawFighter(runner.Simulation.P2);
                foreach(var shot in runner.Simulation.Projectiles)Draw(shot.Hitbox,Color.yellow);
                foreach(var trace in traces){var color=trace.Color;color.a=.5f;Draw(trace.Rect,color);}
            }
            for(int i=used;i<lines.Count;i++)lines[i].enabled=false;
        }
        private void DrawFighter(FighterState fighter)
        {
            Draw(fighter.Hurtbox,Color.green);var attack=fighter.Attack;
            if(attack==null||attack.Definition.NonDamaging||attack.Frame>=attack.Definition.StartupFrames+attack.Definition.ActiveFrames)return;
            bool preview=!attack.IsDamageActive;
            if(attack.Definition.SpawnProjectile&&!preview)return;
            Color color=preview?new Color(1,.45f,.45f,.35f):Color.red;
            Draw(attack.WorldHitbox(fighter),color);
            if(attack.Definition.HasSecondaryHitbox)Draw(attack.Definition.SecondaryHitbox.At(fighter.Position,fighter.Facing),color);
        }
        private void Draw(in BattleRect rect,Color color)
        {
            if(!material)material=new Material(Shader.Find("Sprites/Default"));
            if(used==lines.Count)
            {
                var go=new GameObject("Debug Box "+used);go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;
                line.positionCount=5;line.startWidth=line.endWidth=presentation.DebugLineWidth;line.sortingOrder=50;lines.Add(line);
            }
            var current=lines[used++];current.enabled=true;current.startColor=current.endColor=color;
            current.SetPosition(0,new Vector3(rect.Left,rect.Bottom,0));current.SetPosition(1,new Vector3(rect.Left,rect.Top,0));
            current.SetPosition(2,new Vector3(rect.Right,rect.Top,0));current.SetPosition(3,new Vector3(rect.Right,rect.Bottom,0));current.SetPosition(4,new Vector3(rect.Left,rect.Bottom,0));
        }
        private void OnDestroy(){if(material)Destroy(material);}
    }
}

