using AniBato.Battle;
using UnityEngine;
namespace AniBato.Game
{
    // Crop the source UVs at the shrinking edges; do not squeeze the whole illustration.
    public sealed class GeneratedPlatformView
    {
        private readonly SpriteRenderer floor;
        private readonly Sprite source;
        private readonly Rect region;
        private readonly BattleRect original;
        private readonly StageTheme theme;
        private readonly SpriteRenderer[] pieces=new SpriteRenderer[12], props=new SpriteRenderer[12];
        private readonly Vector3[] origins=new Vector3[12];
        private BattleRect previous;
        private bool wasActive=true;
        private int removedAt=-1000;
        public GeneratedPlatformView(Transform parent,StageData data,BattleRect bounds,int index,SpriteRenderer renderer)
        {
            floor=renderer;original=previous=bounds;theme=data.Theme;
            var art=data.GeneratedArt;source=index==0?art.Main:art.Upper;region=index==0?art.MainRegion:art.UpperRegion;
            floor.sharedMaterial=art.Cutout;floor.color=Color.white;
            for(int i=0;i<12;i++)
            {
                pieces[i]=Create(parent,"Generated Removed Deck "+index+"-"+i,art.Cutout);
                props[i]=Create(parent,"Generated Removal Prop "+index+"-"+i,art.Cutout);
                props[i].sprite=GeneratedStageArt.Region(art.Removal,art.RemovalRegion,Vector2.one*.5f);
                props[i].transform.localScale=Vector3.one*(.5f/Mathf.Max(props[i].sprite.bounds.size.x,props[i].sprite.bounds.size.y));
            }
            SetDeck(floor,bounds.Left,bounds.Right,bounds.Top,bounds.Size.Y+.12f);
        }
        private static SpriteRenderer Create(Transform parent,string label,Material material)
        {
            var go=new GameObject(label);go.transform.SetParent(parent,false);var r=go.AddComponent<SpriteRenderer>();
            r.sharedMaterial=material;r.sortingOrder=2;r.enabled=false;return r;
        }
        private void SetDeck(SpriteRenderer r,float left,float right,float top,float height)
        {
            if(r.sprite&&r.sprite.name.EndsWith(" region"))Object.Destroy(r.sprite);
            float x=region.x+(left-original.Left)/original.Size.X*region.width;
            float width=(right-left)/original.Size.X*region.width;
            r.sprite=GeneratedStageArt.Region(source,new Rect(x,region.y,Mathf.Max(.01f,width),region.height),new Vector2(0,1));
            r.transform.localPosition=new Vector3(left,top,0);
            r.transform.localScale=new Vector3((right-left)/r.sprite.bounds.size.x,height/r.sprite.bounds.size.y,1);
        }
        public void Render(BattleRect bounds,bool active,int tick,bool warn=false)
        {
            bool changed=active&&(Mathf.Abs(bounds.Size.X-previous.Size.X)>.001f);
            if((wasActive&&!active)||changed)
            {
                removedAt=tick;
                for(int i=0;i<12;i++)
                {
                    float a,b;
                    if(!active){a=Mathf.Lerp(previous.Left,previous.Right,i/12f);b=Mathf.Lerp(previous.Left,previous.Right,(i+1)/12f);}
                    else if(i<6){a=Mathf.Lerp(previous.Left,bounds.Left,i/6f);b=Mathf.Lerp(previous.Left,bounds.Left,(i+1)/6f);}
                    else{a=Mathf.Lerp(bounds.Right,previous.Right,(i-6)/6f);b=Mathf.Lerp(bounds.Right,previous.Right,(i-5)/6f);}
                    SetDeck(pieces[i],a,b,previous.Top,previous.Size.Y+.12f);
                    origins[i]=pieces[i].transform.localPosition;
                }
            }
            if(changed)SetDeck(floor,bounds.Left,bounds.Right,bounds.Top,bounds.Size.Y+.12f);
            floor.enabled=active;floor.color=warn?new Color(1,.6f,.5f):Color.white;
            float progress=Mathf.Clamp01((tick-removedAt)/24f),p=Mathf.Floor(progress*4)/4;
            for(int i=0;i<12;i++)
            {
                pieces[i].enabled=props[i].enabled=progress<1;
                if(progress>=1)continue;
                float drop=theme==StageTheme.FutureCity?0:p*(theme==StageTheme.BoardArena?1.7f:2.4f);
                pieces[i].transform.localPosition=origins[i]+new Vector3(0,-drop,0);
                pieces[i].transform.localRotation=Quaternion.Euler(0,theme==StageTheme.BoardArena?p*120:0,theme==StageTheme.FutureCity?0:(i%2==0?1:-1)*p*90);
                pieces[i].color=props[i].color=new Color(1,1,1,1-progress);
                props[i].transform.localPosition=origins[i]+new Vector3(.2f,-drop-.25f,0);
                props[i].transform.localRotation=Quaternion.Euler(0,0,p*(i%2==0?120:-120));
            }
            previous=bounds;wasActive=active;
        }
    }
}
