using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AniBato.Game;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    public static class PaperActionBuilder
    {
        [Serializable] public sealed class Manifest
        { public int schemaVersion; public PoseSource[] poses; public EffectSource[] effects; }
        [Serializable] public sealed class PoseSource
        { public string key, source, sha256; public int[] states; public bool airborne; public int headTopY = -1, footY, footMinX, footMaxX; }
        [Serializable] public sealed class EffectSource
        { public string key, source, sha256; public float worldWidth; }
        public const string ManifestPath = "../art_source/paper_cutout/runtime-manifest.json";
        public static void Apply(PaperCharacterMotion data, float ppu)
        {
            if (!File.Exists(ManifestPath)) throw new InvalidOperationException("紙アクションの指示データがありません。");
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if (manifest == null || manifest.schemaVersion != 1 || manifest.poses == null || manifest.effects == null)
                throw new InvalidOperationException("紙アクションの指示データが不正です。");
            var poses = new List<PaperPose>(); var covered = new HashSet<AnimationState>();
            foreach (var source in manifest.poses)
            {
                VerifySource(source.source, source.sha256);
                var pose = PaperSourcePacking.Body(source.key, "../" + source.source, ppu, source.airborne, source.headTopY, source.footY, source.footMinX, source.footMaxX);
                if (source.key == "idle") data.Sprite = pose.Frame(0);
                if (source.key == "hit_heavy") { data.HeavyDamage = pose; continue; }
                if (source.states == null || source.states.Length == 0) throw new InvalidOperationException("紙ポーズの状態指定がありません: " + source.key);
                pose.States = new AnimationState[source.states.Length];
                for (int i = 0; i < source.states.Length; i++)
                {
                    var state = (AnimationState)source.states[i];
                    if (!Enum.IsDefined(typeof(AnimationState), state) || !covered.Add(state)) throw new InvalidOperationException("紙ポーズの状態が重複または不正: " + source.key);
                    pose.States[i] = state;
                }
                poses.Add(pose);
            }
            foreach (AnimationState state in Enum.GetValues(typeof(AnimationState)))
                if (!covered.Contains(state)) throw new InvalidOperationException("紙ポーズが未対応です: " + state);
            data.Poses = poses.ToArray();
            foreach (var source in manifest.effects)
            {
                VerifySource(source.source, source.sha256);
                var sprite = PaperSourcePacking.Effect(source.key, "../" + source.source, source.worldWidth);
                switch (source.key)
                {
                    case "bag_throw": data.Bag = sprite; break;
                    case "rice_ball": data.RiceBall = sprite; break;
                    case "mogu": data.Mogu = sprite; break;
                    case "curry_wave": data.CurryWave = sprite; break;
                    default: throw new InvalidOperationException("不明な紙効果です: " + source.key);
                }
            }
            if (data.HeavyDamage == null || !data.HeavyDamage.Valid || !data.Bag || !data.RiceBall || !data.Mogu || !data.CurryWave)
                throw new InvalidOperationException("強被弾または紙効果が未完成です。");
            data.SchemaVersion = 2;
        }
        private static void VerifySource(string source, string expected)
        {
            using (var sha = SHA256.Create())
            {
                string actual = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes("../" + source))).Replace("-", "");
                if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("紙原画のSHA不一致: " + source);
            }
        }
    }
}
