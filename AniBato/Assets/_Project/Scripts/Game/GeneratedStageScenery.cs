using UnityEngine;
namespace AniBato.Game
{
    // These are scenery layers only. Simulation geometry never reads this class.
    public sealed class GeneratedStageScenery : MonoBehaviour
    {
        private StageData data;
        private Camera view;
        private SpriteRenderer far, mid, frame;
        private readonly SpriteRenderer[] cells = new SpriteRenderer[36], warnings = new SpriteRenderer[2];
        private readonly Vector3[] cellOrigins = new Vector3[36];
        private float ratio = 1;
        public void Build(StageData stage)
        {
            data = stage; view = Camera.main; var art = stage.GeneratedArt;
            far = Add("Paper Cyclorama", art.Far, -50);
            mid = Add("Generated Middle Scenery", art.Mid, -35);
            frame = Add("Generated Paper Proscenium", art.Frame, -12);
            float scale = 40 / mid.sprite.bounds.size.x;
            mid.transform.localScale = Vector3.one * scale;
            mid.transform.localPosition = new Vector3(0, 3.1f, 0);
            if(stage.Theme == StageTheme.BoardArena)
            {
                var panel = art.BoardPanel; var source = art.Mid;
                var ink = ProceduralSprites.Create(mid.transform,"Board Grid Ink",Vector2.one,new Color(.18f,.09f,.045f),sortingOrder:-34);
                ink.transform.localPosition = new Vector3((panel.center.x-source.texture.width*.5f)/100,(panel.center.y-source.texture.height*.5f)/100,0);
                ink.transform.localScale = new Vector3(panel.width/100,panel.height/100,1);
                for(int r=0;r<6;r++)for(int c=0;c<6;c++)
                {
                    int i=r*6+c;
                    var rect=new Rect(panel.x+c*panel.width/6,panel.y+r*panel.height/6,panel.width/6,panel.height/6);
                    cells[i]=Add("Board Backdrop Cell "+r+"-"+c,GeneratedStageArt.Region(source,rect,Vector2.one*.5f),-33);
                    cells[i].transform.SetParent(mid.transform,false);
                    cellOrigins[i]=new Vector3((rect.center.x-source.texture.width*.5f)/100,(rect.center.y-source.texture.height*.5f)/100,0);
                    cells[i].transform.localPosition=cellOrigins[i];
                    cells[i].transform.localScale=new Vector3(.97f,.97f,1);
                }
            }
            for(int i=0;i<2;i++)
            {
                warnings[i]=Add("Generated Shrink Warning "+i,GeneratedStageArt.Region(art.Warning,art.WarningRegion,Vector2.one*.5f),3);
                warnings[i].transform.localScale=Vector3.one*(.8f/Mathf.Max(warnings[i].sprite.bounds.size.x,warnings[i].sprite.bounds.size.y));
                warnings[i].enabled=false;
            }
        }
        private SpriteRenderer Add(string label,Sprite sprite,int order)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;
            renderer.sharedMaterial=data.GeneratedArt.Cutout;return renderer;
        }
        public void Shrink(bool warning,int tick,float remaining)
        {
            ratio=remaining;var main=data.ToGeometry()[0].Bounds;
            for(int i=0;i<2;i++)
            {
                warnings[i].enabled=warning&&tick/12%2==0;
                warnings[i].transform.localPosition=new Vector3((i==0?-1:1)*main.Size.X*remaining*.5f,main.Top+.7f,0);
            }
            for(int i=0;i<cells.Length;i++)if(cells[i])
                cells[i].enabled=Mathf.Abs((i%6+.5f)/6f-.5f)<remaining*.5f+.001f;
        }
        private void LateUpdate()
        {
            if(!view||!data)return;
            var p=view.transform.position;float h=view.orthographicSize*2,w=h*view.aspect;
            float cover=Mathf.Max(w/far.sprite.bounds.size.x,h/far.sprite.bounds.size.y)*1.12f;
            far.transform.localScale=Vector3.one*cover;
            far.transform.position=new Vector3(p.x*(1-data.FarParallax),p.y*(1-data.FarParallax),0);
            mid.transform.position=new Vector3(data.Theme==StageTheme.BoardArena?0:p.x*data.MidParallax,3.1f,0);
            frame.transform.position=new Vector3(p.x,p.y,0);
            frame.transform.localScale=new Vector3(w*1.17f/frame.sprite.bounds.size.x,h*1.19f/frame.sprite.bounds.size.y,1);
            mid.color=new Color(1,1,1,Mathf.Lerp(.6f,1,ratio));
        }
        private void OnDestroy()
        {
            foreach(var cell in cells)if(cell&&cell.sprite)Destroy(cell.sprite);
            foreach(var warning in warnings)if(warning&&warning.sprite)Destroy(warning.sprite);
        }
    }
}
