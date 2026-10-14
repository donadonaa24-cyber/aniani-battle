using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    // Thickness is drawn below the collision surface, never above it.
    public sealed class PaperPlatformView
    {
        private readonly SpriteRenderer border, thickness;
        private readonly SpriteRenderer[] flutes = new SpriteRenderer[18], debris = new SpriteRenderer[12];
        private readonly StageTheme theme;
        private BattleRect previous;
        private bool wasActive;
        private int removedAt = -1000;
        public PaperPlatformView(Transform parent, StageTheme value, BattleRect bounds, int index)
        {
            theme=value; previous=bounds; wasActive=true;
            border=ProceduralSprites.Create(parent,"Paper Platform Border "+index,Vector2.one,PaperStageScenery.Paper,sortingOrder:-1);
            thickness=ProceduralSprites.Create(parent,"Paper Platform Thickness "+index,Vector2.one,new Color(.3f,.25f,.2f),sortingOrder:0);
            for(int i=0;i<flutes.Length;i++)flutes[i]=ProceduralSprites.Create(parent,"Cardboard Flute "+index+"-"+i,Vector2.one,new Color(.63f,.51f,.34f),sortingOrder:1);
            for(int i=0;i<debris.Length;i++){debris[i]=ProceduralSprites.Create(parent,"Paper Removal "+index+"-"+i,Vector2.one,Color.white,sortingOrder:3);debris[i].enabled=false;}
        }
        public void Render(BattleRect bounds, bool active, int tick)
        {
            if((wasActive&&!active)||(active&&bounds.Size.X<previous.Size.X-.001f))
            {
                removedAt=tick;
                for(int i=0;i<debris.Length;i++)
                {
                    float x;
                    if(!active)x=Mathf.Lerp(previous.Left,previous.Right,(i+.5f)/debris.Length);
                    else x=i<6?Mathf.Lerp(previous.Left,bounds.Left,(i+.5f)/6):Mathf.Lerp(bounds.Right,previous.Right,(i-5.5f)/6);
                    debris[i].transform.position=new Vector3(x,previous.Center.Y,0);
                    debris[i].transform.localScale=new Vector3(theme==StageTheme.BoardArena?.65f:.35f,.28f,1);
                }
            }
            float progress=Mathf.Clamp01((tick-removedAt)/24f);
            for(int i=0;i<debris.Length;i++)
            {
                debris[i].enabled=progress<1;
                var c=theme==StageTheme.FutureCity?new Color(.25f,.8f,1):theme==StageTheme.BoardArena?new Color(.76f,.59f,.34f):new Color(.87f,.7f,.5f);
                c.a=(1-progress)*.8f;debris[i].color=c;
                if(progress<1)
                {
                    // Held poses at 6-tick intervals; no movement while the simulation is paused.
                    float p=Mathf.Floor(progress*4)/4;
                    debris[i].transform.localRotation=Quaternion.Euler(0,0,theme==StageTheme.FutureCity?0:(i%2==0?1:-1)*p*120);
                    // Position derives from a fixed origin, rather than accumulating per rendered frame.
                    float x=debris[i].transform.localPosition.x;
                    float y=previous.Center.Y-(theme==StageTheme.FutureCity?0:p*1.5f);
                    if(theme==StageTheme.SchoolFestival)y=previous.Bottom-p*2;
                    debris[i].transform.localPosition=new Vector3(x,y,0);
                }
            }
            border.enabled=thickness.enabled=active;
            border.transform.localPosition=new Vector3(bounds.Center.X,bounds.Center.Y-.035f,0);
            border.transform.localScale=new Vector3(bounds.Size.X+.12f,bounds.Size.Y+.08f,1);
            thickness.transform.localPosition=new Vector3(bounds.Center.X,bounds.Bottom+.1f,0);
            thickness.transform.localScale=new Vector3(bounds.Size.X,.18f,1);
            for(int i=0;i<flutes.Length;i++)
            {
                flutes[i].enabled=active;
                flutes[i].transform.localPosition=new Vector3(bounds.Left+bounds.Size.X*(i+.5f)/flutes.Length,bounds.Bottom+.1f,0);
                flutes[i].transform.localScale=new Vector3(.035f,.13f,1);
            }
            previous=bounds;wasActive=active;
        }
    }
}

