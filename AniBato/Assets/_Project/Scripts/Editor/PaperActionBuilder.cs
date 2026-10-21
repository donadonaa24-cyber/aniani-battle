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
        { public int schemaVersion; public string normalWeapon; public PoseSource[] poses; public EffectSource[] effects; }
        [Serializable] public sealed class FrameSource
        { public string key, source, sha256; public int headTopY, footY, footMinX, footMaxX; }
        [Serializable] public sealed class PoseSource
        { public string key, source, sha256; public int[] states; public bool airborne, reverseFacing; public float tiltDegrees; public int headTopY = -1, footY, footMinX, footMaxX; public FrameSource[] frames; }
        [Serializable] public sealed class EffectSource
        { public string key, source, sha256; public float worldWidth; }
        public const string ManifestPath = "../art_source/paper_cutout/runtime-manifest.json";
        public static void Apply(PaperCharacterMotion data, float ppu)
        {
            if (!File.Exists(ManifestPath)) throw new InvalidOperationException("紙アクションの指示データがありません。");
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if (manifest == null || (manifest.schemaVersion != 1 && manifest.schemaVersion != 2) || manifest.poses == null || manifest.effects == null)
                throw new InvalidOperationException("紙アクションの指示データが不正です。");
            var poses = new List<PaperPose>(); var covered = new HashSet<AnimationState>();
            foreach (var source in manifest.poses)
            {
                VerifySource(source.source, source.sha256);
                var pose = source.frames != null && source.frames.Length > 0 ? BuildFrames(source, ppu) :
                    PaperSourcePacking.Body(source.key, "../" + source.source, ppu, source.airborne, source.headTopY, source.footY, source.footMinX, source.footMaxX);
                pose.ReverseFacing = source.reverseFacing;
                pose.SwitchOnProjectileRelease = source.key=="special_neutral";
                pose.TiltDegrees = source.tiltDegrees;
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
            if(!covered.Contains(AnimationState.LedgeHang))
            { poses.Add(PaperLedgeBuilder.HangingPose(2));covered.Add(AnimationState.LedgeHang); }
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
        private static PaperPose BuildFrames(PoseSource source, float ppu)
        {
            int count = source.frames.Length;
            bool singleAllowed = source.states != null && Array.Exists(source.states, s =>
                s == (int)AnimationState.CrouchAttack || s == (int)AnimationState.AirNeutral);
            if (count != 2 && !(count == 1 && singleAllowed)) throw new InvalidOperationException("通常は2原画、空中・下は1原画です: " + source.key);
            var result = new PaperPose { Key = source.key, Airborne = source.airborne, Frames = new Sprite[count], HoldTicks = 8,
                FrameHeadHeights = new float[count], FrameFootBaseYs = new float[count], FrameFootMinXs = new float[count], FrameFootMaxXs = new float[count] };
            for (int i = 0; i < count; i++)
            {
                var frame = source.frames[i]; VerifySource(frame.source, frame.sha256);
                var packed = PaperSourcePacking.Body(source.key + "_" + (count == 2 && i == 0 ? "anticipation" : "impact"), "../" + frame.source, ppu,
                    source.airborne, frame.headTopY, frame.footY, frame.footMinX, frame.footMaxX);
                result.Frames[i] = packed.Frames[0]; result.FrameHeadHeights[i] = packed.HeadHeight;
                result.FrameFootBaseYs[i] = packed.FootBaseY; result.FrameFootMinXs[i] = packed.FootMinX; result.FrameFootMaxXs[i] = packed.FootMaxX;
            }
            result.HeadHeight = result.FrameHeadHeights[0]; result.FootBaseY = result.FrameFootBaseYs[0];
            result.FootMinX = result.FrameFootMinXs[0]; result.FootMaxX = result.FrameFootMaxXs[0]; return result;
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
