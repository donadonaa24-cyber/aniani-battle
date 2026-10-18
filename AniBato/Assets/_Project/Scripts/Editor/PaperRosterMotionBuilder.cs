using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    // Reapply completed whole-illustration motion batches after the core/attack builders.
    public static class PaperRosterMotionBuilder
    {
        public static readonly string[] Keys = { "toma", "ren", "minato", "shiro", "yui", "king", "akatsuki", "ryuta" };
        public static string PathFor(string key) => "../art_source/paper_roster_motion/" + key + "/packed-manifest.json";
        [Serializable] public sealed class Manifest
        {
            public int schemaVersion, characterIndex;
            public string key, name;
            public bool complete;
            public PaperActionArtworkBuilder.Frame[] frames;
        }
        public static IEnumerable<Manifest> Completed()
        {
            foreach (string key in Keys)
            {
                string path = PathFor(key);
                if (!File.Exists(path)) continue;
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
                if (manifest == null || manifest.schemaVersion != 1 || manifest.key != key || !manifest.complete ||
                    manifest.frames == null || manifest.frames.Length != 7 ||
                    !manifest.frames.Select(f => f.id).OrderBy(s => s).SequenceEqual(
                        new[] { "run", "jump", "fall", "dodge", "counter_success", "hit_light", "hit_heavy" }.OrderBy(s => s)))
                    throw new InvalidOperationException("専用7動作の完成データが不正です: " + key);
                yield return manifest;
            }
        }
        public static void Apply(CharacterData[] roster)
        {
            foreach (var entry in Completed())
            {
                if (entry.characterIndex < 0 || entry.characterIndex >= roster.Length || entry.characterIndex == 2 ||
                    roster[entry.characterIndex].Movement.Name != entry.name)
                    throw new InvalidOperationException("専用7動作の人物が一致しません: " + entry.key);
                var paper = roster[entry.characterIndex].PaperMotion;
                if (!paper) throw new InvalidOperationException("基本原画の接続が必要です: " + entry.key);
                var additions = new List<PaperPose>();
                foreach (var source in entry.frames)
                {
                    const float ppu = PaperActionArtworkBuilder.PixelsPerUnit;
                    var pose = new PaperPose { Key = source.id, HoldTicks = 8,
                        Frames = new[] { PaperActionArtworkBuilder.Import(entry.key + "_motion_" + source.id,
                            source.prepared, source.sha256, 1304, 1450, new Vector2(.5f, 122f / 1450)) },
                        HeadHeight = (1328 - source.headTopY) / ppu,
                        FootMinX = (source.footMinX - 652) / ppu, FootMaxX = (source.footMaxX - 652) / ppu,
                        FrameOriginYs = new[] { source.originOffsetY } };
                    switch (source.id)
                    {
                        case "run": pose.States = new[] { AnimationState.Dash }; break;
                        case "jump": pose.States = new[] { AnimationState.Jump }; break;
                        case "fall": pose.States = new[] { AnimationState.Fall }; break;
                        case "dodge": pose.States = new[] { AnimationState.Dodge }; break;
                        case "counter_success": pose.States = new[] { AnimationState.CounterSuccess }; break;
                        case "hit_light": pose.States = new[] { AnimationState.Hitstun, AnimationState.Knockback }; break;
                        case "hit_heavy": paper.HeavyDamage = pose; continue;
                    }
                    additions.Add(pose);
                }
                var replaced = new HashSet<AnimationState>(additions.SelectMany(p => p.States));
                var poses = paper.Poses.ToList();
                foreach (var pose in poses) pose.States = pose.States.Where(s => !replaced.Contains(s)).ToArray();
                poses.RemoveAll(p => p.States.Length == 0); poses.AddRange(additions); paper.Poses = poses.ToArray();
                EditorUtility.SetDirty(paper);
            }
        }
    }
}
