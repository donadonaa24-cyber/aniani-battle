using UnityEngine;
using System.Collections.Generic;

namespace AniBato.Game
{
    [DefaultExecutionOrder(60)]
    public sealed class StageBackgroundView : MonoBehaviour
    {
        private Transform far, mid;
        private StageData data;
        private Camera cameraComponent;
        private readonly List<SpriteRenderer> boardCells = new List<SpriteRenderer>();
        private SpriteRenderer backing, leftClosed, rightClosed, warningPanel;
        private float boardLeft, boardWidth;
        public void Build(StageData stage)
        {
            data = stage; cameraComponent = Camera.main;
            far = new GameObject("Far Scenery").transform; far.SetParent(transform, false);
            mid = new GameObject("Mid Scenery").transform; mid.SetParent(transform, false);
            if (cameraComponent) cameraComponent.backgroundColor = data.Background;
            backing=ProceduralSprites.Create(far,"Paper Cyclorama",new Vector2(160,90),data.Background,sortingOrder:-50);
            PaperStageScenery.Build(data,far,mid);
            boardCells.Clear();
            foreach(var sprite in mid.GetComponentsInChildren<SpriteRenderer>())
                if(sprite.name.StartsWith("Board Backdrop Cell "))boardCells.Add(sprite);
            boardLeft=-12;boardWidth=24;
            Color closed=data.Theme==StageTheme.FutureCity?new Color(.09f,.16f,.27f):data.Theme==StageTheme.BoardArena?new Color(.28f,.24f,.29f):new Color(.47f,.39f,.33f);
            leftClosed=ProceduralSprites.Create(mid,"Closed Left Scenery",Vector2.one,closed,sortingOrder:-18);
            rightClosed=ProceduralSprites.Create(mid,"Closed Right Scenery",Vector2.one,closed,sortingOrder:-18);
            leftClosed.enabled=rightClosed.enabled=false;
            warningPanel=ProceduralSprites.Create(mid,data.Theme==StageTheme.FutureCity?"AI Restriction Signal":data.Theme==StageTheme.BoardArena?"Outer Cells Warning":"Festival Closing Signal",new Vector2(28,.12f),data.Accent,sortingOrder:-17);
            warningPanel.transform.localPosition=new Vector3(0,7,0);warningPanel.enabled=false;
        }
        public void SetShrinkStep(int step,int total,bool warning=false,int tick=0,float remaining=1)
        {
            if(warningPanel){warningPanel.enabled=warning;warningPanel.color=new Color(data.Accent.r,data.Accent.g,data.Accent.b,tick/12%2==0?.65f:.25f);}
            float removed=boardWidth*.5f*(1-remaining),left=boardLeft+removed,right=boardLeft+boardWidth-removed;
            leftClosed.enabled=rightClosed.enabled=removed>.01f;
            leftClosed.transform.localPosition=new Vector3(boardLeft+removed*.5f,3.4f,0);
            rightClosed.transform.localPosition=new Vector3(boardLeft+boardWidth-removed*.5f,3.4f,0);
            leftClosed.transform.localScale=rightClosed.transform.localScale=new Vector3(Mathf.Max(.001f,removed),7,1);
            foreach(var cell in boardCells)
            {
                int col=int.Parse(cell.name.Substring(cell.name.LastIndexOf('-')+1));
                float a=Mathf.Max(left,boardLeft+col*boardWidth/6),b=Mathf.Min(right,boardLeft+(col+1)*boardWidth/6);
                cell.enabled=b>a;var p=cell.transform.localPosition;p.x=(a+b)*.5f;cell.transform.localPosition=p;
                cell.transform.localScale=new Vector3(Mathf.Max(.001f,b-a-.08f),1.01f,1);
            }
        }
        private void LateUpdate()
        {
            if(!cameraComponent||!data)return;var p=cameraComponent.transform.position;
            far.localPosition=new Vector3(p.x*data.FarParallax,p.y*data.FarParallax,0);
            mid.localPosition=data.Theme==StageTheme.BoardArena?Vector3.zero:new Vector3(p.x*data.MidParallax,p.y*data.MidParallax,0);
            backing.transform.position=new Vector3(p.x,p.y,0);
        }
    }
}

