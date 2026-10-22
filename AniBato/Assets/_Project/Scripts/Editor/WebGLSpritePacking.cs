using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace AniBato.EditorTools
{
    // GPU block compression needs dimensions divisible by four. Add transparent rows/columns;
    // keep every original pixel, sprite rectangle, pivot and PPU. Never resample the character.
    public static class WebGLSpritePacking
    {
        private static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        public static bool Enabled {get;private set;}
        public static Color32[] Pad(Color32[] source,int width,int height,out int packedWidth,out int packedHeight)
        {
            packedWidth=(width+3)/4*4;packedHeight=(height+3)/4*4;
            var pixels=new Color32[packedWidth*packedHeight];
            for(int y=0;y<height;y++)Array.Copy(source,y*width,pixels,y*packedWidth,width);
            return pixels;
        }
        public static bool UsesMipmaps(string path)=>new[]{"Paper","PaperAction","PaperCore","PaperRoster"}
            .Any(folder=>path.StartsWith("Assets/_Project/Art/"+folder+"/",StringComparison.Ordinal));
        public static Color32[] PadMipChain(Color32[] source,int width,int height,out int packedWidth,out int packedHeight)
        {
            packedWidth=Mathf.NextPowerOfTwo(width);packedHeight=Mathf.NextPowerOfTwo(height);
            var pixels=new Color32[packedWidth*packedHeight];
            for(int y=0;y<height;y++)Array.Copy(source,y*width,pixels,y*packedWidth,width);
            return pixels;
        }
        public static void Prepare() => PreparePlatform("WebGL", TextureImporterFormat.DXT5, TextureFormat.DXT5);
        public static void PrepareAndroid() => PreparePlatform("Android", TextureImporterFormat.ETC2_RGBA8, TextureFormat.ETC2_RGBA8);
        private static void PreparePlatform(string platform, TextureImporterFormat format, TextureFormat expected)
        {
            Enabled=true;textures.Clear();const string folder="Assets/_WebGLPacked";Directory.CreateDirectory(folder);
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Project/Art"}))
            {
                string source=AssetDatabase.GUIDToAssetPath(guid);if(!source.EndsWith(".png",StringComparison.OrdinalIgnoreCase))continue;
                byte[] bytes=File.ReadAllBytes(source);string hash;
                using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").Substring(0,12);
                bool mipmaps=UsesMipmaps(source);
                string path=folder+"/"+guid+"_"+hash+(mipmaps?"_mip1":"")+".png";
                if(!File.Exists(path))
                {
                    var decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);Texture2D padded=null;
                    try
                    {
                        if(!ImageConversion.LoadImage(decoded,bytes))throw new InvalidOperationException("PNGの復号失敗");
                        int w,h;var sourcePixels=decoded.GetPixels32();
                        var pixels=mipmaps?PadMipChain(sourcePixels,decoded.width,decoded.height,out w,out h):Pad(sourcePixels,decoded.width,decoded.height,out w,out h);
                        padded=new Texture2D(w,h,TextureFormat.RGBA32,false);padded.SetPixels32(pixels);padded.Apply();
                        File.WriteAllBytes(path,ImageConversion.EncodeToPNG(padded));
                    }
                    finally{Object.DestroyImmediate(decoded);if(padded)Object.DestroyImmediate(padded);}
                }
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                var filtering=mipmaps?FilterMode.Trilinear:((TextureImporter)AssetImporter.GetAtPath(source)).filterMode;
                var platformSettings=importer.GetPlatformTextureSettings(platform);
                if(importer.textureType!=TextureImporterType.Sprite||importer.textureCompression!=TextureImporterCompression.CompressedHQ||importer.mipmapEnabled!=mipmaps||importer.filterMode!=filtering||!platformSettings.overridden||platformSettings.format!=format)
                {
                    importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                    importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=16384;importer.mipmapEnabled=mipmaps;
                    importer.filterMode=filtering;
                    importer.alphaIsTransparency=true;importer.isReadable=false;importer.textureCompression=TextureImporterCompression.CompressedHQ;
                    var p=importer.GetPlatformTextureSettings(platform);p.name=platform;p.overridden=true;p.format=format;p.maxTextureSize=16384;
                    importer.SetPlatformTextureSettings(p);importer.SaveAndReimport();
                }
                textures[source]=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            foreach(var pair in textures)
                if(pair.Value.format!=expected&&!(expected==TextureFormat.DXT5&&pair.Value.format==TextureFormat.DXT1))throw new InvalidOperationException(platform+"画像がGPU圧縮されていません: "+pair.Key+" "+pair.Value.format);
            Debug.Log(platform.ToUpperInvariant()+"_PACKED_TEXTURES "+textures.Count+" native pixels preserved; "+expected+" compression confirmed");
        }
        public static Sprite Copy(Sprite source)
        {
            if(!Enabled||!source)return source;
            if(!textures.TryGetValue(AssetDatabase.GetAssetPath(source.texture),out var texture))return source;
            return CopyIntoTexture(source,texture);
        }
        public static Sprite CopyIntoTexture(Sprite source,Texture2D texture)
        {
            var r=source.rect;
            var copy=Sprite.Create(texture,r,new Vector2(source.pivot.x/r.width,source.pivot.y/r.height),source.pixelsPerUnit,0,SpriteMeshType.FullRect,source.border);
            copy.name=source.name;return copy;
        }
        public static void Reset(){Enabled=false;textures.Clear();}
    }
}
