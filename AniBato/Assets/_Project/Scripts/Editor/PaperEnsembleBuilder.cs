using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
namespace AniBato.EditorTools
{
    public static class PaperEnsembleBuilder
    {
        [Serializable] public sealed class Hotspot
        { public float x,y,width,height; public Rect ToRect()=>new Rect(x,y,width,height); }
        [Serializable] public sealed class Selection
        { public string source, sha256; public int width, height; public Hotspot[] hotspots; }
        public static PaperEnsembleArt Import()
        {
            var selected=JsonUtility.FromJson<Selection>(File.ReadAllText("../art_source/title_paper/selected.json"));
            string source="../"+selected.source;
            const string path="Assets/_Project/Art/PaperEnsemble/ensemble.png";
            if(selected.hotspots==null||selected.hotspots.Length!=9)throw new InvalidOperationException("集合絵の9人の領域が未設定です");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var bytes=File.ReadAllBytes(source);
            using(var sha=SHA256.Create())
                if(!string.Equals(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-",""),selected.sha256,StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("集合絵の選定原画とSHAが一致しません");
            if(!File.Exists(path)||!bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)))
            {
                string pending=path+".pending";File.WriteAllBytes(pending,bytes);
                try{if(File.Exists(path))File.Replace(pending,path,null);else File.Move(pending,path);}
                finally{if(File.Exists(pending))File.Delete(pending);}
            }
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            if(width!=selected.width||height!=selected.height)throw new InvalidOperationException("集合絵の原画寸法が一致しません");
            const string dataPath="Assets/_Project/Data/PaperEnsemble.asset";
            var data=AssetDatabase.LoadAssetAtPath<PaperEnsembleArt>(dataPath);
            if(!data){data=ScriptableObject.CreateInstance<PaperEnsembleArt>();AssetDatabase.CreateAsset(data,dataPath);}
            data.Artwork=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            data.Hotspots=selected.hotspots.Select(h=>h.ToRect()).ToArray();
            EditorUtility.SetDirty(data);return data;
        }
    }
}
