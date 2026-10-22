using System.IO;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class PaperHudPortraitBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            foreach(var character in roster)
            {
                var sprite=character.PaperMotion.Sprite;
                var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                try
                {
                    texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)));
                    var pixels=texture.GetPixels32();var frame=sprite.rect;
                    int minY=(int)frame.yMax,maxY=(int)frame.yMin;
                    for(int y=(int)frame.yMin;y<frame.yMax;y++)for(int x=(int)frame.xMin;x<frame.xMax;x++)
                        if(pixels[y*texture.width+x].a>=96){minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
                    int side=Mathf.Max(1,Mathf.RoundToInt((maxY-minY+1)*(character.Movement.Name=="シロ"?.66f:.42f)));
                    long sum=0,count=0;
                    for(int y=Mathf.Max(minY,maxY-side+1);y<=maxY;y++)for(int x=(int)frame.xMin;x<frame.xMax;x++)
                        if(pixels[y*texture.width+x].a>=96){sum+=x;count++;}
                    float center=count>0?sum/(float)count:frame.center.x;
                    float left=Mathf.Clamp(center-side*.5f,frame.xMin,frame.xMax-side),bottom=Mathf.Max(frame.yMin,maxY-side+1);
                    character.HudPortraitRegion=new Rect((left-frame.x)/frame.width,(bottom-frame.y)/frame.height,side/frame.width,side/frame.height);
                    EditorUtility.SetDirty(character);
                }
                finally{Object.DestroyImmediate(texture);}
            }
        }
    }
}
