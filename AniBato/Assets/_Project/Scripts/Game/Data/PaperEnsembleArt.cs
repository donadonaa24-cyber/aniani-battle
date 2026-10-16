using UnityEngine;
namespace AniBato.Game
{
    public sealed class PaperEnsembleArt : ScriptableObject
    {
        public Sprite Artwork;
        // Normalized screen coordinates, y down. Indexed by the gameplay roster, not painting order.
        public Rect[] Hotspots;
        public static readonly Vector2 DisplaySize=new Vector2(1700,1700f*941/1672);
        public static readonly Vector2 DisplayPosition=new Vector2(0,-5);
        public Vector2 Position(int index)=>DisplayPosition+new Vector2((Hotspots[index].center.x-.5f)*DisplaySize.x,(.5f-Hotspots[index].center.y)*DisplaySize.y);
        public Vector2 Size(int index)=>Vector2.Scale(Hotspots[index].size,DisplaySize);
        public Sprite Face(int index)
        {
            var r=Hotspots[index];var t=Artwork.texture;
            return GeneratedStageArt.Region(Artwork,new Rect(r.x*t.width,(1-r.yMax)*t.height,r.width*t.width,r.height*t.height),Vector2.one*.5f);
        }
    }
}
