using UnityEngine;

namespace AniBato.Game
{
    // Original cut-paper impact silhouette; no downloaded assets or character redraws.
    public static class PaperBurstSprite
    {
        private static Sprite sprite;
        public static Sprite Sprite
        {
            get
            {
                if (sprite) return sprite;
                const int side = 128;
                var texture = new Texture2D(side,side,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point,hideFlags=HideFlags.HideAndDontSave };
                var pixels = new Color[side*side];
                for (int y=0;y<side;y++) for(int x=0;x<side;x++)
                {
                    float dx=(x-63.5f)/63.5f,dy=(y-63.5f)/63.5f;
                    float radius=Mathf.Sqrt(dx*dx+dy*dy), angle=Mathf.Atan2(dy,dx);
                    float edge=.62f+.34f*Mathf.Pow(Mathf.Abs(Mathf.Cos(angle*7)),5);
                    pixels[y*side+x]=radius>edge?Color.clear:radius>edge-.045f?new Color(1,.96f,.8f):
                        radius>.4f?new Color(1,.46f,.12f):new Color(1,.9f,.45f);
                }
                texture.SetPixels(pixels);texture.Apply(false,true);
                sprite=Sprite.Create(texture,new Rect(0,0,side,side),new Vector2(.5f,.5f),side,0,SpriteMeshType.FullRect);
                sprite.hideFlags=HideFlags.HideAndDontSave;return sprite;
            }
        }
    }
}
