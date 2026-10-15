using System;
using System.IO;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class PaperSourcePacking
    {
        public const int CanvasWidth = 1304, CanvasHeight = 1450;
        public static PaperPose Body(string key, string sourcePath, float ppu, bool airborne, int headTopY, int footY, int footMinX, int footMaxX)
        {
            string destination = "Assets/_Project/Art/Paper/pose_" + key + "_paper_chizuru.png";
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Texture2D packed = null;
            try
            {
                if (!ImageConversion.LoadImage(source, File.ReadAllBytes(sourcePath))) throw new InvalidOperationException("紙原画が読めません: " + key);
                int dx = (CanvasWidth - source.width) / 2, dy = (CanvasHeight - source.height) / 2;
                var pixels = new Color32[CanvasWidth * CanvasHeight]; var input = source.GetPixels32();
                for (int y = 0; y < source.height; y++) for (int x = 0; x < source.width; x++)
                {
                    Color32 color = input[y * source.width + x]; int xx = x + dx, yy = y + dy;
                    if (xx < 0 || yy < 0 || xx >= CanvasWidth || yy >= CanvasHeight)
                    { if (color.a != 0) throw new InvalidOperationException("共通キャンバスで人物が切れます: " + key); continue; }
                    pixels[yy * CanvasWidth + xx] = color;
                }
                if (source.width == CanvasWidth && source.height == CanvasHeight) WriteIfChanged(destination,File.ReadAllBytes(sourcePath));
                else
                {
                    // Packing only: add/crop transparent margin. Never resample or reshape the artwork.
                    packed = new Texture2D(CanvasWidth, CanvasHeight, TextureFormat.RGBA32, false);
                    packed.SetPixels32(pixels); packed.Apply(); WriteIfChanged(destination, packed.EncodeToPNG());
                }
                int minY = CanvasHeight, maxY = -1, minX = CanvasWidth, maxX = -1;
                for (int y = 0; y < CanvasHeight; y++) for (int x = 0; x < CanvasWidth; x++)
                    if (pixels[y * CanvasWidth + x].a >= 32) { minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); }
                if (maxY < minY) throw new InvalidOperationException("紙原画が透明だけです: " + key);
                if (footY <= 0 || footY > source.height || footMinX < 0 || footMinX >= footMaxX || footMaxX > source.width || headTopY < 0 || headTopY >= footY)
                    throw new InvalidOperationException("紙原画の頭と靴の支点が不正です: " + key);
                int sole = source.height - footY + dy;
                Vector2 pivot = new Vector2(.5f, airborne ? .5f : sole / (float)CanvasHeight);
                Sprite sprite = Import(destination, ppu, pivot);
                var frames = new Sprite[4]; for (int frame = 0; frame < frames.Length; frame++) frames[frame] = sprite;
                int head = source.height - headTopY + dy;
                return new PaperPose { Key = key, Frames = frames, HoldTicks = 8, Airborne = airborne,
                    HeadHeight = (head - pivot.y * CanvasHeight) / ppu,
                    FootBaseY = (sole - pivot.y * CanvasHeight) / ppu,
                    FootMinX = (footMinX + dx - CanvasWidth * .5f) / ppu, FootMaxX = (footMaxX + dx - CanvasWidth * .5f) / ppu };
            }
            finally { UnityEngine.Object.DestroyImmediate(source); if (packed) UnityEngine.Object.DestroyImmediate(packed); }
        }
        public static Sprite Effect(string key, string sourcePath, float worldWidth)
        {
            string destination = "Assets/_Project/Art/Paper/effect_" + key + "_chizuru.png";
            WriteIfChanged(destination,File.ReadAllBytes(sourcePath));
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(source, File.ReadAllBytes(sourcePath))) throw new InvalidOperationException("紙の効果原画が読めません: " + key);
                var pixels = source.GetPixels32(); int minX = source.width, maxX = -1, minY = source.height, maxY = -1;
                for (int y = 0; y < source.height; y++) for (int x = 0; x < source.width; x++)
                    if (pixels[y * source.width + x].a >= 32) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
                if (maxX < minX || worldWidth <= 0) throw new InvalidOperationException("紙の効果の寸法が不正です: " + key);
                return Import(destination, (maxX - minX + 1) / worldWidth,
                    new Vector2((minX + maxX + 1) / (2f * source.width), (minY + maxY + 1) / (2f * source.height)));
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
        }
        private static void WriteIfChanged(string path,byte[] bytes)
        {
            // Repeated BuildAll/tests can still have the imported file memory-mapped.
            // Reading it is safe; truncating identical content is both unnecessary and unreliable on Windows.
            if(File.Exists(path)&&bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)))return;
            File.WriteAllBytes(path,bytes);
        }
        private static Sprite Import(string path, float ppu, Vector2 pivot)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.sRGBTexture = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
            var platform = importer.GetDefaultPlatformTextureSettings(); platform.format = TextureImporterFormat.RGBA32;
            platform.textureCompression = TextureImporterCompression.Uncompressed; platform.maxTextureSize = 4096;
            importer.SetPlatformTextureSettings(platform); importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) throw new InvalidOperationException("紙Spriteをインポートできません: " + path);
            return sprite;
        }
    }
}
