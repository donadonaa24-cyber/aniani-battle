using System;
using System.IO;
using AniBato.Game;
using AniBato.Presentation;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class PaperCharacterBuilder
    {
        public const string ImagePath = "Assets/_Project/Art/Paper/idle_paper_chizuru.png";
        public const string DataPath = "Assets/_Project/Data/ChizuruPaperMotion.asset";
        public static PaperCharacterMotion Build()
        {
            if (!File.Exists(ImagePath)) throw new InvalidOperationException("千鶴の紙人形原画がありません。");
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            int minX = int.MaxValue, maxX = -1, minY = int.MaxValue, maxY = -1;
            int width, height, footMin = int.MaxValue, footMax = -1;
            try
            {
                if (!ImageConversion.LoadImage(source, File.ReadAllBytes(ImagePath))) throw new InvalidOperationException("紙人形PNGを読み込めません。");
                width = source.width; height = source.height; var pixels = source.GetPixels32();
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                    if (pixels[y * width + x].a >= 32) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
                if (maxX < minX || maxY < minY) throw new InvalidOperationException("紙人形が透明だけです。");
                int footTop = minY + (maxY - minY + 1) / 10;
                for (int y = minY; y <= footTop; y++) for (int x = minX; x <= maxX; x++)
                    if (pixels[y * width + x].a >= 32) { footMin = Math.Min(footMin, x); footMax = Math.Max(footMax, x); }
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
            const float worldHeight = 2.24f;
            float ppu = (maxY - minY + 1) / worldHeight;
            AssetDatabase.ImportAsset(ImagePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(ImagePath) as TextureImporter;
            if (!importer) throw new InvalidOperationException("紙人形PNGのImporterがありません。");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            var textureSettings = new TextureImporterSettings(); importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            textureSettings.spritePivot = new Vector2(.5f, minY / (float)height);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(textureSettings);
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.sRGBTexture = true; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            var platform = importer.GetDefaultPlatformTextureSettings(); platform.format = TextureImporterFormat.RGBA32;
            platform.textureCompression = TextureImporterCompression.Uncompressed; platform.maxTextureSize = 2048;
            importer.SetPlatformTextureSettings(platform); importer.SaveAndReimport();
            var data = AssetDatabase.LoadAssetAtPath<PaperCharacterMotion>(DataPath);
            if (!data) { data = ScriptableObject.CreateInstance<PaperCharacterMotion>(); AssetDatabase.CreateAsset(data, DataPath); }
            data.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ImagePath); data.StandingHeight = worldHeight;
            if (!data.Sprite) throw new InvalidOperationException("紙人形Spriteがインポートされていません。");
            if (data.SchemaVersion < 1)
            {
                data.Motion = new PaperRockingSettings { IdleDegrees = .75f, WalkDegrees = 5, WalkBob = .025f, IdleFps = 4, WalkFps = 8 };
                data.SchemaVersion = 1;
            }
            var motion = data.Motion; motion.FootMinX = (footMin - width * .5f) / ppu; motion.FootMaxX = (footMax + 1 - width * .5f) / ppu;
            data.Motion = motion; data.Motion.Validate(); EditorUtility.SetDirty(data);
            return data;
        }
    }
}
