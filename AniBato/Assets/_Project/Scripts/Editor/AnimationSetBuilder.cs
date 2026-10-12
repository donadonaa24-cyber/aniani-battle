using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AniBato.Game;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class AnimationSetBuilder
    {
        public const string SpriteRoot = "Assets/_Project/Art/Sprites/";
        public static CharacterAnimationSet Build(string character)
        {
            string folder = SpriteRoot + character;
            if (!Directory.Exists(folder)) return null;
            var metadataPaths = Directory.GetFiles(folder, "*.json").Where(path => !path.EndsWith("forge-report.json", StringComparison.Ordinal)).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            if (metadataPaths.Length == 0) throw new InvalidOperationException("SpriteForgeを先に実行してください: " + folder);
            var clips = new List<AnimationClipData>(); var bindings = new List<AniBato.Presentation.AnimationBinding>();
            float ppu = 0; int height = 0;
            foreach (string metadataPath in metadataPaths)
            {
                var metadata = JsonUtility.FromJson<SpriteSheetMetadata>(File.ReadAllText(metadataPath));
                Validate(metadata);
                if (ppu != 0 && metadata.pixelsPerUnit != ppu) throw new InvalidOperationException("PPU must match across all clips.");
                ppu = metadata.pixelsPerUnit; height = metadata.targetHeight;
                string path = folder + "/" + metadata.name + ".png";
                Import(path, metadata);
                Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
                if (frames.Length != metadata.frameCount) throw new InvalidOperationException("Slice count mismatch: " + path);
                clips.Add(new AnimationClipData { Name = metadata.name, Frames = frames, Fps = metadata.fps, Loop = metadata.loop, HeadHeights = metadata.headHeights });
                if (metadata.bindings != null) foreach (var binding in metadata.bindings)
                {
                    if (binding.Clip != metadata.name || binding.ActiveFrame < 0 || binding.ActiveFrame >= frames.Length || bindings.Any(existing => existing.State == binding.State))
                        throw new InvalidOperationException("Invalid/duplicate animation binding in " + path);
                    bindings.Add(binding);
                }
            }
            foreach (AniBato.Presentation.AnimationState state in Enum.GetValues(typeof(AniBato.Presentation.AnimationState)))
                if (!bindings.Any(binding => binding.State == state)) throw new InvalidOperationException("Missing animation state: " + state);
            string assetPath = "Assets/_Project/Data/" + character + "AnimationSet.asset";
            var set = AssetDatabase.LoadAssetAtPath<CharacterAnimationSet>(assetPath);
            if (!set) { set = ScriptableObject.CreateInstance<CharacterAnimationSet>(); AssetDatabase.CreateAsset(set, assetPath); }
            set.Clips = clips.ToArray(); set.Bindings = bindings.ToArray(); set.PixelsPerUnit = ppu; set.StandingHeight = height;
            EditorUtility.SetDirty(set); return set;
        }
        private static void Validate(SpriteSheetMetadata data)
        {
            if (data == null || data.schemaVersion != 1 || String.IsNullOrEmpty(data.name) || data.name != Path.GetFileName(data.name) ||
                data.frameCount < 1 || data.frameCount > 8 || data.cellWidth != 224 || data.cellHeight != 160 || data.baseline != 159 || data.fps < 1 || data.pixelsPerUnit != 50 ||
                data.pivotX < 0 || data.pivotX > 1 || data.pivotY < 0 || data.pivotY > 1 || data.headHeights == null || data.headHeights.Length != data.frameCount)
                throw new InvalidOperationException("Invalid SpriteForge metadata.");
        }
        private static void Import(string path, SpriteSheetMetadata data)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new InvalidOperationException("Missing importer: " + path);
            // Check the source PNG size: a first import with default settings may NPOT-rescale the texture.
            importer.GetSourceTextureWidthAndHeight(out int sourceWidth, out int sourceHeight);
            if (sourceWidth != data.cellWidth * data.frameCount || sourceHeight != data.cellHeight) throw new InvalidOperationException("Sheet dimensions mismatch: " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = data.pixelsPerUnit; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
            var platform = importer.GetDefaultPlatformTextureSettings(); platform.format = TextureImporterFormat.RGBA32;
            platform.textureCompression = TextureImporterCompression.Uncompressed; platform.maxTextureSize = 4096; importer.SetPlatformTextureSettings(platform);
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var oldRects = provider.GetSpriteRects();
            var rects = new SpriteRect[data.frameCount];
            for (int i = 0; i < rects.Length; i++)
            {
                string name = data.name + "_" + i.ToString("D2");
                var previous = Array.Find(oldRects, rect => rect.name == name);
                rects[i] = new SpriteRect { name = name, rect = new Rect(i * data.cellWidth, 0, data.cellWidth, data.cellHeight),
                    alignment = SpriteAlignment.Custom, pivot = new Vector2(data.pivotX, data.pivotY), spriteID = previous == null ? GUID.Generate() : previous.spriteID };
            }
            provider.SetSpriteRects(rects);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names == null) throw new InvalidOperationException("Sprite name/file ID provider missing.");
            names.SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
    }
}
