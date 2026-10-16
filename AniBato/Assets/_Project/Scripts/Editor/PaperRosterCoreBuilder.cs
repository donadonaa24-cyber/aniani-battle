using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AniBato.Game;
using AniBato.Presentation;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    // Import only the completed character batches; combat definitions are never written here.
    public static class PaperRosterCoreBuilder
    {
        public const string ManifestPath = "../art_source/paper_roster_core/runtime-manifest.json";
        [Serializable] public sealed class Manifest { public int schemaVersion, canvasWidth, canvasHeight; public float pixelsPerUnit; public Character[] characters; }
        [Serializable] public sealed class Character { public string key, name; public int characterIndex; public Source idle; public Source[] poses; }
        [Serializable] public sealed class Source { public string key, prepared, sha256; public int headTopY, footMinX, footMaxX; }
        public static void Apply(CharacterData[] roster)
        {
            if (!File.Exists(ManifestPath)) return;
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if (manifest.schemaVersion != 1 || manifest.canvasWidth != 1304 || manifest.canvasHeight != 1450 || Math.Abs(manifest.pixelsPerUnit - 532.5893f) > .001f)
                throw new InvalidOperationException("基本ポーズの共通枠が不正です。");
            foreach (var entry in manifest.characters)
            {
                if (entry.characterIndex < 0 || entry.characterIndex >= roster.Length || entry.characterIndex == 2 || entry.poses.Length != 5)
                    throw new InvalidOperationException("基本ポーズの人物／5種類が不正です。");
                var character = roster[entry.characterIndex];
                if (character.Movement.Name != entry.name) throw new InvalidOperationException("基本ポーズの人物が一致しません。");
                string profilePath = "Assets/_Project/Data/" + entry.key + "CorePaperMotion.asset";
                var paper = AssetDatabase.LoadAssetAtPath<PaperCharacterMotion>(profilePath);
                if (!paper) { paper = ScriptableObject.CreateInstance<PaperCharacterMotion>(); AssetDatabase.CreateAsset(paper, profilePath); }
                var poses = new List<PaperPose>(); var covered = new HashSet<AnimationState>();
                var idle = Import(entry.key, "idle", entry.idle, manifest.pixelsPerUnit);
                idle.States = new[] { AnimationState.Idle, AnimationState.Move };
                poses.Add(idle); foreach (var state in idle.States) covered.Add(state);
                foreach (var source in entry.poses)
                {
                    var pose = Import(entry.key, source.key, source, manifest.pixelsPerUnit);
                    switch (source.key)
                    {
                        case "guard": pose.States = new[] { AnimationState.Guard }; break;
                        case "crouch": pose.States = new[] { AnimationState.Crouch, AnimationState.Land, AnimationState.JumpStartup }; break;
                        case "counter": pose.States = new[] { AnimationState.Counter, AnimationState.CounterSuccess }; break;
                        case "win": pose.States = new[] { AnimationState.Win }; break;
                        case "lose": pose.States = new[] { AnimationState.Lose, AnimationState.KO }; break;
                        default: throw new InvalidOperationException("不明な基本ポーズ: " + source.key);
                    }
                    foreach (var state in pose.States) if (!covered.Add(state)) throw new InvalidOperationException("基本ポーズの状態重複");
                    poses.Add(pose);
                }
                // New art replaces this explicit standing placeholder state by state.
                // Toma/Ren must never fall back to the retired pixel body artwork.
                if (entry.characterIndex != 2)
                {
                    var fallback = Import(entry.key, "idle", entry.idle, manifest.pixelsPerUnit);
                    fallback.Key = "prototype_whole_illustration";
                    fallback.States = ((AnimationState[])Enum.GetValues(typeof(AnimationState))).Where(s => !covered.Contains(s)).ToArray();
                    poses.Add(fallback);
                }
                paper.Sprite = idle.Frames[0]; paper.StandingHeight = idle.HeadHeight;
                paper.Enabled = true; paper.Poses = poses.ToArray(); paper.SchemaVersion = 3;
                paper.Motion = new PaperRockingSettings { IdleDegrees = .75f, IdleFps = 4, WalkDegrees = 5, WalkFps = 8,
                    WalkBob = .025f, FootMinX = idle.FootMinX, FootMaxX = idle.FootMaxX };
                paper.WalkLeanDegrees = 15; paper.DashLeanDegrees = 45;
                paper.Performance = PaperPerformanceSettings.Default; paper.PerformanceSchemaVersion = 1;
                character.PaperMotion = paper; EditorUtility.SetDirty(paper); EditorUtility.SetDirty(character);
            }
        }
        private static PaperPose Import(string character, string key, Source source, float ppu)
        {
            byte[] bytes = File.ReadAllBytes("../" + source.prepared);
            using (var hash = SHA256.Create())
                if (!string.Equals(BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", ""), source.sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("基本ポーズのSHA不一致: " + source.prepared);
            string folder = key == "idle" ? "Assets/_Project/Art/PaperRoster" : "Assets/_Project/Art/PaperCore"; Directory.CreateDirectory(folder);
            string destination = folder + "/" + character + "_" + key + ".png";
            if (!File.Exists(destination) || !bytes.AsSpan().SequenceEqual(File.ReadAllBytes(destination))) File.WriteAllBytes(destination, bytes);
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(destination);
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width != 1304 || height != 1450) throw new InvalidOperationException("基本ポーズの画像枠が不正です。");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = new Vector2(.5f, 122f / 1450);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            return new PaperPose { Key = key, Frames = new[] { AssetDatabase.LoadAssetAtPath<Sprite>(destination) }, HoldTicks = 8,
                HeadHeight = (1328 - source.headTopY) / ppu, FootMinX = (source.footMinX - 652) / ppu, FootMaxX = (source.footMaxX - 652) / ppu };
        }
    }
}
