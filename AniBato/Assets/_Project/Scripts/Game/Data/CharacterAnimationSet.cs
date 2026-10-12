using System;
using AniBato.Presentation;
using UnityEngine;

namespace AniBato.Game
{
    [Serializable]
    public sealed class AnimationClipData
    {
        public string Name;
        public Sprite[] Frames;
        public int[] HeadHeights;
        public int Fps;
        public bool Loop;
        public bool Valid => Frames != null && Frames.Length > 0 && Array.TrueForAll(Frames, sprite => sprite);
    }
    [CreateAssetMenu(menuName = "あにバト/アニメセット")]
    public sealed class CharacterAnimationSet : ScriptableObject
    {
        public AnimationTiming Timing = AnimationTiming.Default;
        public AnimationClipData[] Clips;
        public AnimationBinding[] Bindings;
        public float PixelsPerUnit = 50;
        public float StandingHeight = 112;
        public AnimationClipData Find(string name) => Clips == null ? null : Array.Find(Clips, clip => clip != null && clip.Name == name);
        public bool TrySprite(in AnimationSample sample, out Sprite sprite)
        {
            sprite = null;
            if (Bindings == null) return false;
            foreach (var binding in Bindings)
            {
                if (binding.State != sample.State) continue;
                var clip = Find(binding.Clip);
                if (clip == null || !clip.Valid) return false;
                sprite = clip.Frames[sample.FrameIndex(clip.Frames.Length, clip.Fps, clip.Loop, binding.ActiveFrame)];
                return true;
            }
            return false;
        }
        public float HeadHeight(in AnimationSample sample)
        {
            if (Bindings != null) foreach (var binding in Bindings)
            {
                if (binding.State != sample.State) continue;
                var clip = Find(binding.Clip);
                if (clip == null || !clip.Valid || clip.HeadHeights == null || clip.HeadHeights.Length != clip.Frames.Length) return StandingHeight;
                return clip.HeadHeights[sample.FrameIndex(clip.Frames.Length, clip.Fps, clip.Loop, binding.ActiveFrame)];
            }
            return StandingHeight;
        }
        public Sprite Effect(string name, int age)
        {
            var clip = Find(name);
            if (clip == null || !clip.Valid) return null;
            int index = Math.Max(0, age) * Math.Max(1, clip.Fps) / 60;
            return clip.Frames[clip.Loop ? index % clip.Frames.Length : Math.Min(clip.Frames.Length - 1, index)];
        }
    }
    // JsonUtility-compatible metadata shared with SpriteForge, with schema validation in the builder.
    [Serializable]
    public sealed class SpriteSheetMetadata
    {
        public int schemaVersion;
        public string name;
        public int frameCount, cellWidth, cellHeight, fps;
        public bool loop;
        public float pivotX, pivotY, pixelsPerUnit;
        public int targetHeight, baseline;
        public int[] headHeights;
        public AnimationBinding[] bindings;
    }
}


