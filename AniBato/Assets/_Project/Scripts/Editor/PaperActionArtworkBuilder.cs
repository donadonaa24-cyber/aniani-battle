using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    // Presentation-only merge, reapplied after the core poses on every BuildAll.
    public static class PaperActionArtworkBuilder
    {
        public const string Root = "../art_source/paper_roster_action/toma/";
        public const float PixelsPerUnit = 532.5893f;
        [Serializable] public sealed class Manifest { public int schemaVersion; public string key; public bool complete; public Frame[] frames; }
        [Serializable] public sealed class Frame { public string id, prepared, sha256; public int headTopY, footMinX, footMaxX; public float originOffsetY; }
        [Serializable] public sealed class Effects { public Effect[] entries; }
        [Serializable] public sealed class Effect { public string id, look, source, sha256; public Native native; }
        [Serializable] public sealed class Native { public int width, height; public Bounds bounds; }
        [Serializable] public sealed class Bounds { public int left, top, right, bottom; }
        public static void Apply(CharacterData[] roster)
        {
            string path = Root + "packed-manifest.json";
            if (!File.Exists(path)) return;
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            if (manifest.schemaVersion != 1 || manifest.key != "toma" || !manifest.complete || manifest.frames.Length != 16)
                throw new InvalidOperationException("透真の攻撃16原画が未完です。");
            var data = roster[0]; var paper = data.PaperMotion;
            if (data.Movement.Name != "透真" || !paper) throw new InvalidOperationException("透真の基本原画が未設定です。");
            var additions = new List<PaperPose>();
            string[] keys = { "normal1", "normal2", "normal3", "up", "down", "air", "special", "super" };
            AnimationState[] states = { AnimationState.Attack1, AnimationState.Attack2, AnimationState.Attack3, AnimationState.AirUp,
                AnimationState.CrouchAttack, AnimationState.AirNeutral, AnimationState.SpecialNeutral, AnimationState.Ultimate };
            for (int i = 0; i < keys.Length; i++)
            {
                var phases = keys[i] == "down" || keys[i] == "air" ? new[] { "impact" } : new[] { "prepare", "impact" };
                var frames = phases.Select(p => manifest.frames.Single(f => f.id == keys[i] + "_" + p)).ToArray();
                var pose = new PaperPose { Key = keys[i], States = new[] { states[i] }, HoldTicks = 8,
                    Frames = frames.Select(f => Import("toma_" + f.id, f.prepared, f.sha256, 1304, 1450, new Vector2(.5f, 122f / 1450))).ToArray(),
                    FrameHeadHeights = frames.Select(f => (1328 - f.headTopY) / PixelsPerUnit).ToArray(),
                    FrameFootMinXs = frames.Select(f => (f.footMinX - 652) / PixelsPerUnit).ToArray(),
                    FrameFootMaxXs = frames.Select(f => (f.footMaxX - 652) / PixelsPerUnit).ToArray(),
                    FrameOriginYs = frames.Select(f => f.originOffsetY).ToArray() };
                pose.HeadHeight = pose.FrameHeadHeights[0]; pose.FootMinX = pose.FrameFootMinXs[0]; pose.FootMaxX = pose.FrameFootMaxXs[0];
                additions.Add(pose);
            }
            var front = additions[0];
            additions.Add(new PaperPose { Key = "back", States = new[] { AnimationState.BackAttack }, Frames = front.Frames, HoldTicks = front.HoldTicks,
                ReverseFacing = true, FrameHeadHeights = front.FrameHeadHeights, FrameFootMinXs = front.FrameFootMinXs, FrameFootMaxXs = front.FrameFootMaxXs,
                HeadHeight = front.HeadHeight, FootMinX = front.FootMinX, FootMaxX = front.FootMaxX });
            var replaced = new HashSet<AnimationState>(additions.SelectMany(p => p.States));
            var poses = paper.Poses.ToList();
            foreach (var pose in poses) pose.States = pose.States.Where(s => !replaced.Contains(s)).ToArray();
            poses.RemoveAll(p => p.States.Length == 0); poses.AddRange(additions); paper.Poses = poses.ToArray();
            var effects = JsonUtility.FromJson<Effects>(File.ReadAllText(Root + "effects-manifest.json"));
            paper.Projectiles = new[] {
                Projectile(effects, "bullet", data.GroundNormals[0].Definition, true, 0, false),
                Projectile(effects, "drone", data.Specials[0].Definition, false, 0, true),
                Projectile(effects, "rocket", data.Ultimate.Definition, true, 90, false) };
            data.ActionArtworkSchemaVersion = 1;
            // Movement, dodge and damage still include legacy frames; do not mark all actions finished.
            data.ActionArtworkPending = true;
            EditorUtility.SetDirty(paper); EditorUtility.SetDirty(data);
        }
        private static PaperProjectileArt Projectile(Effects effects, string id, AttackDefinition attack, bool rotate, float rotation, bool flip)
        {
            var effect = effects.entries.Single(e => e.id == id); var n = effect.native; var b = n.bounds;
            var look = (AbilityLook)Enum.Parse(typeof(AbilityLook), effect.look);
            if (look != attack.Ability.Look) throw new InvalidOperationException("飛翔原画の技が一致しません。");
            var pivot = new Vector2((b.left + b.right + 1) * .5f / n.width, 1 - (b.top + b.bottom + 1) * .5f / n.height);
            var sprite = Import("toma_effect_" + id, effect.source, effect.sha256, n.width, n.height, pivot);
            float width = (b.right - b.left + 1) / PixelsPerUnit, height = (b.bottom - b.top + 1) / PixelsPerUnit;
            float fit=Mathf.Min(attack.Hitbox.Size.X / width, attack.Hitbox.Size.Y / height);
            // Readable props intentionally exceed the unchanged combat hitbox.
            float scale=id=="drone"?Mathf.Max(fit,1.65f/width):id=="rocket"?Mathf.Max(fit,3.3f/height):fit;
            return new PaperProjectileArt { Look = look, Sprite = sprite, Scale = scale,
                RotateWithVelocity = rotate, RotationOffsetDegrees = rotation, FlipWithFacing = flip };
        }
        internal static Sprite Import(string name, string source, string expectedSha, int width, int height, Vector2 pivot)
        {
            byte[] bytes = File.ReadAllBytes("../" + source);
            using (var hash = SHA256.Create())
                if (!string.Equals(BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", ""), expectedSha, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("攻撃原画のSHA不一致: " + source);
            const string folder = "Assets/_Project/Art/PaperAction"; Directory.CreateDirectory(folder);
            string path = folder + "/" + name + ".png";
            if (!File.Exists(path) || !bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)))
            {
                // A previewer may map the previous PNG. Replace it atomically instead of truncating that mapped file.
                string pending=path+".pending";
                File.WriteAllBytes(pending,bytes);
                try { if(File.Exists(path))File.Replace(pending,path,null);else File.Move(pending,path); }
                finally { if(File.Exists(pending))File.Delete(pending); }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            if (w != width || h != height) throw new InvalidOperationException("攻撃原画の寸法不一致: " + source);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = width > 2048 || height > 2048 ? 4096 : 2048;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot; settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
