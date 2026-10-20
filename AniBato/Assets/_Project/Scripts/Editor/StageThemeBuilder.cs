using System.IO;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class StageThemeBuilder
    {
        public static StageData[] Build(StageData original)
        {
            var geometry = Asset<StageGeometryData>("CommonGeometry");
            if (geometry.Platforms == null || geometry.Platforms.Length == 0)
            {
                geometry.Platforms = (AniBato.Battle.StagePlatform[])original.Platforms.Clone();
                geometry.LeftBlastLine = original.LeftBlastLine; geometry.RightBlastLine = original.RightBlastLine;
                geometry.BottomBlastLine = original.BottomBlastLine; geometry.SpawnP1 = original.SpawnP1; geometry.SpawnP2 = original.SpawnP2;
            }
            if (geometry.BlastZoneSchemaVersion < 1)
            { geometry.LeftBlastLine=-32;geometry.RightBlastLine=32;geometry.BottomBlastLine=-16;geometry.BlastZoneSchemaVersion=1; }
            original.SharedGeometry = geometry; EditorUtility.SetDirty(original); EditorUtility.SetDirty(geometry);
            var stages = new[] { original, Asset<StageData>("StageBoard"), Asset<StageData>("StageFestival") };
            string[] names = { "星の終わり", "推し駒Battle", "Battle à la carte" };
            string[] warnings = { "AREA RESTRICTION", "盤面の外側が消えます", "文化祭終了・中央へ集合" };
            Color[] backgrounds = { new Color(0.025f, 0.04f, 0.09f), new Color(0.06f, 0.035f, 0.045f), new Color(0.055f, 0.045f, 0.065f) };
            Color[] mid = { new Color(0.12f, 0.2f, 0.3f), new Color(0.24f, 0.14f, 0.12f), new Color(0.22f, 0.17f, 0.19f) };
            Color[] accent = { new Color(0.25f, 0.5f, 0.65f), new Color(0.55f, 0.36f, 0.22f), new Color(0.48f, 0.37f, 0.28f) };
            for (int i = 0; i < stages.Length; i++)
            {
                var data = stages[i]; data.SharedGeometry = geometry;
                if (string.IsNullOrEmpty(data.DisplayName))
                {
                    data.DisplayName = names[i]; data.Theme = (StageTheme)i; data.BgmKey = "stage_" + i;
                    data.ShrinkMessage = warnings[i]; data.Background = backgrounds[i];
                    data.MidColor = mid[i]; data.FarColor = mid[i] * 0.55f; data.FloorColor = mid[i]; data.Accent = accent[i];
                }
                if (data.AppearanceSchemaVersion < 1)
                {
                    // Replace only the old generated palette; preserve user-tuned colors.
                    if (data.Background == backgrounds[i]) data.Background = new[] { new Color(.13f,.2f,.32f), new Color(.35f,.32f,.28f), new Color(.38f,.31f,.27f) }[i];
                    if (data.FloorColor == mid[i]) data.FloorColor = new[] { new Color(.24f,.34f,.44f), new Color(.48f,.41f,.29f), new Color(.46f,.36f,.29f) }[i];
                    if (data.Accent == accent[i]) data.Accent = new[] { new Color(.4f,.67f,.78f), new Color(.69f,.58f,.39f), new Color(.76f,.56f,.36f) }[i];
                    data.AppearanceSchemaVersion = 1;
                }
                data.FarArtwork = ImportArtwork("Assets/_Project/Art/Stages/phase6b_" + i + "_background.png");
                data.Thumbnail = ImportArtwork("Assets/_Project/Art/Stages/phase6b_" + i + "_thumbnail.png");
                if(data.AppearanceSchemaVersion<2)
                {
                    data.Background=new[]{new Color(.09f,.14f,.23f),new Color(.29f,.25f,.3f),new Color(.64f,.55f,.4f)}[i];
                    data.FloorColor=new[]{new Color(.18f,.3f,.43f),new Color(.55f,.42f,.25f),new Color(.46f,.36f,.25f)}[i];
                    data.Accent=new[]{new Color(.42f,.75f,.84f),new Color(.88f,.71f,.43f),new Color(.88f,.66f,.39f)}[i];
                    data.FarParallax=.025f;data.MidParallax=.055f;data.AppearanceSchemaVersion=2;
                }
                data.Thumbnail=Thumbnail(data,i);
                EditorUtility.SetDirty(data);
            }
            return stages;
        }
        private static Sprite ImportArtwork(string path)
        {
            string physicalPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
            if (!File.Exists(physicalPath)) throw new FileNotFoundException("Run tools/SpriteForge/BuildPhase6B.ps1 first.", path);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 20; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = "Assets/_Project/Data/" + name + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!data) { data = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(data, path); }
            return data;
        }
        private static Sprite Thumbnail(StageData stage, int index)
        {
            const string folder = "Assets/_Project/Art/Stages";
            string physicalFolder = Path.Combine(Application.dataPath, "_Project/Art/Stages");
            Directory.CreateDirectory(physicalFolder);
            string path = folder + "/paper_stage_" + index + ".png";
            string physicalPath = Path.Combine(physicalFolder, "paper_stage_" + index + ".png");
            if (!File.Exists(physicalPath))
            {
                const int w = 480, h = 270;
                var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var pixels = new Color[w * h];
                for (int p = 0; p < pixels.Length; p++) pixels[p] = stage.Background;
                void Rect(int x, int y, int width, int height, Color color)
                {
                    for (int py = Mathf.Max(0, y); py < Mathf.Min(h, y + height); py++)
                        for (int px = Mathf.Max(0, x); px < Mathf.Min(w, x + width); px++) pixels[py * w + px] = color;
                }
                if (index == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        int height = 60 + i * i % 100; Rect(i * 42, 70, 32, height, stage.FarColor);
                        for (int j = 0; j < 5; j++) Rect(i * 42 + 6, 80 + j * 20, 18, 2, stage.MidColor);
                    }
                    Rect(370, 70, 10, 175, stage.MidColor); Rect(345, 215, 60, 3, stage.Accent);
                }
                else if (index == 1)
                {
                    for (int i = 0; i < 6; i++) for (int j = 0; j < 6; j++)
                        Rect(30 + i * 70, 30 + j * 24, 68, 22, (i + j) % 2 == 0 ? stage.FarColor : stage.MidColor);
                    foreach (int x in new[] { 30, 410 }) { Rect(x, 110, 24, 80, stage.Accent); Rect(x - 8, 100, 40, 12, stage.Accent); Rect(x - 5, 187, 34, 10, stage.Accent); }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Rect(15 + i * 120, 60, 95, 100, stage.FarColor); Rect(10 + i * 120, 150, 105, 15, stage.Accent);
                        Rect(10 + i * 120, 65, 105, 12, stage.MidColor); Rect(40 + i * 120, 80, 30, 25, stage.MidColor);
                    }
                    Rect(0, 225, 480, 2, stage.MidColor);
                    for (int i = 0; i < 12; i++) Rect(i * 40 + 10, 206, 14, 18, stage.Accent);
                }
                Rect(65, 45, 350, 14, stage.FloorColor); Rect(65, 59, 350, 3, stage.Accent);
                Rect(110, 112, 65, 5, stage.Accent); Rect(305, 112, 65, 5, stage.Accent);
                texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(physicalPath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
