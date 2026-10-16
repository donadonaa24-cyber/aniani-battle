using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
namespace AniBato.EditorTools
{
    public static class GeneratedStageArtBuilder
    {
        public const string ManifestPath="../art_source/paper_stages/runtime-manifest.json";
        [Serializable] public sealed class Region { public float x,y,width,height;public Rect Value=>new Rect(x,y,width,height); }
        [Serializable] public sealed class Manifest { public int schemaVersion;public Source[] entries;public Region boardPanel; }
        [Serializable] public sealed class Source { public string id,key,layer,source,sha256;public Region rect;public Native native; }
        [Serializable] public sealed class Native { public int width,height; }
        public static void Apply(StageData[] stages)
        {
            if(!File.Exists(ManifestPath))return;
            var m=JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if(m.schemaVersion!=1||m.entries.Length!=21)throw new InvalidOperationException("紙舞台21レイヤーが未完です。");
            var shader=Shader.Find("AniBato/PaperStageCutout");if(!shader)throw new InvalidOperationException("紙舞台shader未設定");
            const string materialPath="Assets/_Project/Data/PaperStageCutout.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,materialPath);}
            material.SetFloat("_Cutoff",.8f);EditorUtility.SetDirty(material);
            string[] keys={"future","board","festival"};
            for(int i=0;i<keys.Length;i++)
            {
                string key=keys[i],path="Assets/_Project/Data/"+key+"GeneratedStage.asset";
                var data=AssetDatabase.LoadAssetAtPath<GeneratedStageArt>(path);
                if(!data){data=ScriptableObject.CreateInstance<GeneratedStageArt>();AssetDatabase.CreateAsset(data,path);}
                Source Row(string layer)=>m.entries.Single(e=>e.key==key&&e.layer==layer);
                data.Far=Import(Row("far"));data.Mid=Import(Row("mid"));data.Frame=Import(Row("frame"));
                data.Main=Import(Row("main"));data.Upper=Import(Row("upper"));data.Warning=Import(Row("warning"));data.Removal=Import(Row("removal"));
                data.MainRegion=Row("main").rect.Value;data.UpperRegion=Row("upper").rect.Value;data.WarningRegion=Row("warning").rect.Value;data.RemovalRegion=Row("removal").rect.Value;
                if(data.MainRegion.width<=0||data.UpperRegion.width<=0)throw new InvalidOperationException("足場領域が空です。");
                data.BoardPanel=m.boardPanel.Value;data.Cutout=material;stages[i].GeneratedArt=data;stages[i].Thumbnail=data.Far;
                EditorUtility.SetDirty(data);EditorUtility.SetDirty(stages[i]);
            }
        }
        private static Sprite Import(Source source)
        {
            byte[] bytes=File.ReadAllBytes("../"+source.source);
            using(var sha=SHA256.Create())if(!string.Equals(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-",""),source.sha256,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("舞台原画SHA不一致");
            const string folder="Assets/_Project/Art/GeneratedStages";Directory.CreateDirectory(folder);
            string path=folder+"/"+source.id+".png";
            if(!File.Exists(path)||!bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)))File.WriteAllBytes(path,bytes);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);if(w!=source.native.width||h!=source.native.height)throw new InvalidOperationException("舞台原画の寸法不一致");
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
