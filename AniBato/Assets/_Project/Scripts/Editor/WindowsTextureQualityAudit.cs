using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace AniBato.EditorTools
{
    public static class WindowsTextureQualityAudit
    {
        [Serializable] private sealed class BuildEntries { public Entry[] entries; }
        [Serializable] private sealed class Entry { public string path; }
        [Serializable] private sealed class Sample { public string source; public bool originalFormat; public int width,height; public long visiblePixels; public double psnr,meanAbsoluteError; }
        [Serializable] private sealed class Result { public string format="BC7 quality 100 with original-format exceptions; native resolution", preview="original left / shipping right"; public int checkedCount,geometryChecks,originalFormatCount; public double minimumPsnr,weightedMeanAbsoluteError; public Sample[] samples; }
        public static void Run()
        {
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)throw new InvalidOperationException("Win64 required");
            var report=JsonUtility.FromJson<BuildEntries>(File.ReadAllText("../TestResults/size_StandaloneWindows64.json"));
            string output="../TestResults/windows_quality";Directory.CreateDirectory(output);
            var samples=new List<Sample>();int geometry=0;double weightedError=0;long visibleTotal=0;
            foreach(string packedPath in report.entries.Select(e=>e.path).Where(p=>(p.StartsWith("Assets/_WebGLPacked/")||p.StartsWith("Assets/_Project/Art/"))&&p.EndsWith(".png")).Distinct())
            {
                bool originalFormat=packedPath.StartsWith("Assets/_Project/Art/");
                string sourcePath=originalFormat?packedPath:AssetDatabase.GUIDToAssetPath(Path.GetFileName(packedPath).Substring(0,32));
                var source=AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
                var packed=AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath);
                if(!source||!packed||(!originalFormat&&packed.format!=TextureFormat.BC7))throw new InvalidOperationException("Missing BC7 source: "+sourcePath);
                var png=File.ReadAllBytes(sourcePath);
                int width=ReadInt(png,16),height=ReadInt(png,20);
                if(source.width!=width||source.height!=height)throw new InvalidOperationException("Source import was resized: "+sourcePath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(sourcePath);
                bool mipmaps=importer.mipmapEnabled;
                int expectedWidth=mipmaps?Mathf.NextPowerOfTwo(width):(width+3)/4*4;
                int expectedHeight=mipmaps?Mathf.NextPowerOfTwo(height):(height+3)/4*4;
                if(!originalFormat&&(packed.width!=expectedWidth||packed.height!=expectedHeight))throw new InvalidOperationException("Packed image was resized: "+sourcePath);
                if(source.filterMode!=packed.filterMode||(source.mipmapCount>1)!=(packed.mipmapCount>1))throw new InvalidOperationException("Native sampling changed: "+sourcePath);
                var scale=new Vector2(width/(float)packed.width,height/(float)packed.height);
                Texture2D original=null,compressed=null;
                try
                {
                    // Native resolution readback selects mip 0. Premultiply RGB so invisible
                    // colour dilation is not counted as a visible quality difference.
                    original=Read(source,width,height,Vector2.one);compressed=Read(packed,width,height,scale);
                    var a=original.GetPixels32();var b=compressed.GetPixels32();double squared=0,absolute=0;long visible=0;
                    for(int i=0;i<a.Length;i++)
                    {
                        if(a[i].a<=4&&b[i].a<=4)continue;
                        visible++;double aa=a[i].a/255.0,ba=b[i].a/255.0;
                        Accumulate(a[i].r*aa-b[i].r*ba,ref squared,ref absolute);
                        Accumulate(a[i].g*aa-b[i].g*ba,ref squared,ref absolute);
                        Accumulate(a[i].b*aa-b[i].b*ba,ref squared,ref absolute);
                        Accumulate(a[i].a-b[i].a,ref squared,ref absolute);
                    }
                    double mse=squared/Math.Max(1,visible*4),psnr=mse==0?99:10*Math.Log10(255*255/mse);
                    var sample=new Sample{source=sourcePath,originalFormat=originalFormat,width=width,height=height,visiblePixels=visible,psnr=psnr,meanAbsoluteError=absolute/Math.Max(1,visible*4)};
                    samples.Add(sample);weightedError+=absolute;visibleTotal+=visible;
                    foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(sourcePath).OfType<Sprite>())
                    {
                        var copy=WebGLSpritePacking.CopyIntoTexture(sprite,packed);
                        try
                        {
                            if(copy.rect!=sprite.rect||Vector2.Distance(copy.pivot,sprite.pivot)>.0005f||copy.pixelsPerUnit!=sprite.pixelsPerUnit||
                                Vector3.Distance(copy.bounds.center,sprite.bounds.center)>.00001f||Vector3.Distance(copy.bounds.size,sprite.bounds.size)>.00001f)
                                throw new InvalidOperationException("Sprite geometry changed: "+sourcePath);
                            geometry++;
                        }finally{Object.DestroyImmediate(copy);}
                    }
                    if(sourcePath.Contains("idle")||sourcePath.Contains("ensemble"))SavePreview(source,packed,scale,output+"/"+Path.GetFileNameWithoutExtension(sourcePath)+"_original_left_bc7_right.png");
                    Debug.Log("QUALITY "+sourcePath+" PSNR="+psnr.ToString("F2")+" MAE="+sample.meanAbsoluteError.ToString("F4"));
                }
                finally{if(original)Object.DestroyImmediate(original);if(compressed)Object.DestroyImmediate(compressed);}
            }
            if(samples.Count==0)throw new InvalidOperationException("No shipped BC7 textures audited");
            var result=new Result{checkedCount=samples.Count,geometryChecks=geometry,originalFormatCount=samples.Count(s=>s.originalFormat),minimumPsnr=samples.Min(s=>s.psnr),weightedMeanAbsoluteError=weightedError/Math.Max(1,visibleTotal*4),samples=samples.OrderBy(s=>s.psnr).ToArray()};
            File.WriteAllText("../TestResults/windows_texture_quality.json",JsonUtility.ToJson(result,true));
            if(result.minimumPsnr<35||result.weightedMeanAbsoluteError>1.5)throw new InvalidOperationException("Visible quality threshold failed; keep lossless build and review samples");
            Debug.Log("QUALITY_PASS "+result.checkedCount+" textures; "+geometry+" sprite frames; minimum PSNR "+result.minimumPsnr);
        }
        private static int ReadInt(byte[] bytes,int offset)=>(bytes[offset]<<24)|(bytes[offset+1]<<16)|(bytes[offset+2]<<8)|bytes[offset+3];
        private static void Accumulate(double value,ref double squared,ref double absolute){squared+=value*value;absolute+=Math.Abs(value);}
        private static Texture2D Read(Texture texture,int width,int height,Vector2 scale)
        {
            var target=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var previous=RenderTexture.active;
            try
            {
                Graphics.Blit(texture,target,scale,Vector2.zero);RenderTexture.active=target;
                var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();return image;
            }
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);}
        }
        private static void SavePreview(Texture a,Texture b,Vector2 scale,string path)
        {
            int height=Mathf.Clamp(Mathf.RoundToInt(320f*a.height/a.width),1,640);
            var left=Read(a,320,height,Vector2.one);var right=Read(b,320,height,scale);var image=new Texture2D(640,height,TextureFormat.RGBA32,false);
            try{image.SetPixels32(0,0,320,height,left.GetPixels32());image.SetPixels32(320,0,320,height,right.GetPixels32());image.Apply();File.WriteAllBytes(path,ImageConversion.EncodeToPNG(image));}
            finally{Object.DestroyImmediate(left);Object.DestroyImmediate(right);Object.DestroyImmediate(image);}
        }
    }
}
