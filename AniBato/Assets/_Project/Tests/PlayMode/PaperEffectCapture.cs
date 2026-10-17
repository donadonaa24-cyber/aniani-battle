using System.Linq;
using UnityEngine;
namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        private static void CaptureActionEffects(string folder,string name,params Renderer[] sprites)
        {
            sprites=sprites.Where(s=>s.enabled&&s.gameObject.activeInHierarchy).ToArray();
            var camera=Camera.main;var position=camera.transform.position;float size=camera.orthographicSize,aspect=camera.aspect;
            var flags=camera.clearFlags;var color=camera.backgroundColor;int mask=camera.cullingMask;
            var target=camera.targetTexture;var active=RenderTexture.active;var layers=sprites.Select(s=>s.gameObject.layer).ToArray();
            var rt=RenderTexture.GetTemporary(768,768,24,RenderTextureFormat.ARGB32);var image=new Texture2D(768,768,TextureFormat.RGBA32,false);
            try
            {
                var bounds=sprites[0].bounds;foreach(var s in sprites){bounds.Encapsulate(s.bounds);s.gameObject.layer=31;}
                camera.cullingMask=1<<31;camera.aspect=1;camera.orthographicSize=Mathf.Max(bounds.size.x,bounds.size.y)*.56f;
                camera.transform.position=new Vector3(bounds.center.x,bounds.center.y,position.z);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.17f,.2f,.25f,1);camera.targetTexture=rt;
                camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();
                string dir=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/screenshots/",folder));System.IO.Directory.CreateDirectory(dir);
                CaptureFile.Write(System.IO.Path.Combine(dir,name+".png"),image.EncodeToPNG());
            }
            finally
            {
                for(int i=0;i<sprites.Length;i++)sprites[i].gameObject.layer=layers[i];
                camera.cullingMask=mask;camera.transform.position=position;camera.orthographicSize=size;camera.aspect=aspect;
                camera.clearFlags=flags;camera.backgroundColor=color;camera.targetTexture=target;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);
            }
        }
    }
}
