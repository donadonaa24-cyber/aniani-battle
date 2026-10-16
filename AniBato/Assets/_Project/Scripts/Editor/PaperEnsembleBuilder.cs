using System;
using System.IO;
using System.Linq;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
namespace AniBato.EditorTools
{
    public static class PaperEnsembleBuilder
    {
        public static PaperEnsembleArt Import()
        {
            const string source="../art_source/title_paper/ensemble_v1.png",path="Assets/_Project/Art/PaperEnsemble/ensemble.png";
            if(!File.Exists(source))return null;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var bytes=File.ReadAllBytes(source);
            if(!File.Exists(path)||!bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)))File.WriteAllBytes(path,bytes);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            const string dataPath="Assets/_Project/Data/PaperEnsemble.asset";
            var data=AssetDatabase.LoadAssetAtPath<PaperEnsembleArt>(dataPath);
            if(!data){data=ScriptableObject.CreateInstance<PaperEnsembleArt>();AssetDatabase.CreateAsset(data,dataPath);}
            data.Artwork=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            data.Hotspots=new[]{
                new Rect(.455f,.26f,.19f,.225f), // Toma, foreground centre
                new Rect(.194f,.413f,.165f,.205f), // Ren
                new Rect(.681f,.379f,.155f,.22f), // Chizuru
                new Rect(.294f,.075f,.16f,.18f), // Minato
                new Rect(.397f,.555f,.16f,.21f), // Shiro
                new Rect(.025f,.448f,.15f,.22f), // Yui
                new Rect(.104f,.169f,.17f,.20f), // King
                new Rect(.648f,.114f,.155f,.19f), // Akatsuki
                new Rect(.839f,.357f,.145f,.22f) // Ryuta
            };
            EditorUtility.SetDirty(data);return data;
        }
    }
}
