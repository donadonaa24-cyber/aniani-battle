using System;
using System.IO;
using System.Linq;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    // Charge is a presentation-only override; release keeps the existing directional attack keys.
    public static class PaperChargeArtworkBuilder
    {
        public const string ManifestPath = "../art_source/paper_charge/packed-manifest.json";
        public static readonly string[] Keys = { "toma", "ren", "chizuru", "minato", "shiro", "yui", "king", "akatsuki", "ryuta" };
        [Serializable] public sealed class Character
        {
            public string key, name;
            public int characterIndex;
            public PaperActionArtworkBuilder.Frame[] frames;
        }
        [Serializable] public sealed class Manifest
        {
            public int schemaVersion;
            public bool complete;
            public Character[] characters;
        }
        public static Manifest Read()
        {
            if (!File.Exists(ManifestPath)) return null;
            var m = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if (m == null || m.schemaVersion != 1 || !m.complete || m.characters == null || m.characters.Length != Keys.Length ||
                !m.characters.Select(c => c.key).SequenceEqual(Keys) ||
                m.characters.Any(c => c.frames == null || c.frames.Length != 1 || c.frames[0].id != "charge"))
                throw new InvalidOperationException("溜め原画9人分の完成データが不正です。");
            return m;
        }
        public static void Apply(CharacterData[] roster)
        {
            var manifest = Read(); if (manifest == null) return;
            foreach (var entry in manifest.characters)
            {
                if (entry.characterIndex < 0 || entry.characterIndex >= roster.Length || Keys[entry.characterIndex] != entry.key ||
                    roster[entry.characterIndex].Movement.Name != entry.name || !roster[entry.characterIndex].PaperMotion)
                    throw new InvalidOperationException("溜め原画の人物が一致しません: " + entry.key);
                var source = entry.frames[0]; const float ppu = PaperActionArtworkBuilder.PixelsPerUnit;
                var paper = roster[entry.characterIndex].PaperMotion;
                paper.Charging = new PaperPose { Key = "charge", HoldTicks = 8, States = Array.Empty<AniBato.Presentation.AnimationState>(),
                    Frames = new[] { PaperActionArtworkBuilder.Import(entry.key + "_charge", source.prepared, source.sha256,
                        1304, 1450, new Vector2(.5f, 122f / 1450)) },
                    HeadHeight = (1328 - source.headTopY) / ppu,
                    FootMinX = (source.footMinX - 652) / ppu, FootMaxX = (source.footMaxX - 652) / ppu,
                    FrameOriginYs = new[] { source.originOffsetY } };
                EditorUtility.SetDirty(paper);
            }
        }
    }
}
