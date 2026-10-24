using System;
using UnityEngine;

namespace AniBato.Game
{
    [Serializable]
    public struct AfterimageSettings
    {
        public float Seconds, Interval, Opacity, MinimumDistance, MinimumDegrees, ResetDistance;
        public static AfterimageSettings Default => new AfterimageSettings {
            Seconds = .1f, Interval = .045f, Opacity = .18f,
            MinimumDistance = .04f, MinimumDegrees = 5, ResetDistance = 5 };
    }

    // Two shared-sprite renderers, no texture/material copies, timers or physics components.
    // The caller supplies presentation time; pause/hitstop cannot age or emit a trail.
    public sealed class FighterAfterimageView : IDisposable
    {
        private struct Pose
        {
            public Sprite Sprite;
            public Vector3 Position, Scale;
            public Quaternion Rotation;
            public Color Color;
            public bool FlipX, FlipY;
            public int Layer, Order;
            public Material Material;
            public Pose(SpriteRenderer source)
            {
                Sprite = source.sprite; Position = source.transform.position;
                Scale = source.transform.localScale; Rotation = source.transform.rotation;
                Color = source.color; FlipX = source.flipX; FlipY = source.flipY;
                Layer = source.sortingLayerID; Order = source.sortingOrder;
                Material = source.sharedMaterial;
            }
        }
        private sealed class Image
        {
            public SpriteRenderer Renderer;
            public Pose Pose;
            public float Age;
        }
        private readonly GameObject root;
        private readonly Image[] images = new Image[2];
        private Pose previous;
        private Pose emissionAnchor;
        private bool initialized;
        private double lastTime;
        private float sinceEmission;
        private int next;
        public int VisibleCount { get { int count = 0; foreach (var image in images) if (image.Renderer.enabled) count++; return count; } }

        public FighterAfterimageView(Transform parent)
        {
            root = new GameObject("Motion Afterimages"); root.transform.SetParent(parent, false);
            for (int i = 0; i < images.Length; i++)
            {
                var obj = new GameObject("Afterimage " + (i + 1)); obj.transform.SetParent(root.transform, false);
                var renderer = obj.AddComponent<SpriteRenderer>(); renderer.enabled = false;
                images[i] = new Image { Renderer = renderer };
            }
        }
        public void Clear()
        {
            foreach (var image in images) { image.Renderer.enabled = false; image.Renderer.sprite = null; }
            initialized = false; next = 0; sinceEmission = 0;
        }
        public void Render(SpriteRenderer source, double time, in AfterimageSettings settings,
            bool enabled, bool frozen, bool emitting, bool discontinuity = false)
        {
            if (!enabled || discontinuity || !source || !source.enabled || !source.sprite || source.color.a < .8f || settings.Seconds <= 0)
            { Clear(); return; }
            var now = new Pose(source);
            if (!initialized || time < lastTime || Vector3.Distance(now.Position, previous.Position) > settings.ResetDistance)
            {
                Clear(); previous = emissionAnchor = now; lastTime = time; initialized = true;
                sinceEmission = settings.Interval; return;
            }
            float elapsed = frozen ? 0 : (float)Math.Min(.1, Math.Max(0, time - lastTime)); lastTime = time;
            sinceEmission += elapsed;
            foreach (var image in images)
            {
                if (!image.Renderer.enabled) continue;
                image.Age += elapsed;
                if (image.Age >= settings.Seconds) { image.Renderer.enabled = false; image.Renderer.sprite = null; }
            }
            bool moved = Vector3.Distance(now.Position, emissionAnchor.Position) >= settings.MinimumDistance ||
                Quaternion.Angle(now.Rotation, emissionAnchor.Rotation) >= settings.MinimumDegrees || now.Sprite != emissionAnchor.Sprite;
            if (elapsed > 0 && emitting && moved && sinceEmission >= settings.Interval && now.FlipX == previous.FlipX)
            {
                var image = images[next]; next = (next + 1) % images.Length;
                image.Pose = emissionAnchor; image.Age = 0; image.Renderer.enabled = true; sinceEmission = 0;
                emissionAnchor = now;
            }
            // Reapply world positions after the fighter parent moved. Old silhouettes stay behind.
            foreach (var image in images)
            {
                if (!image.Renderer.enabled) continue;
                var pose = image.Pose; var renderer = image.Renderer;
                renderer.sprite = pose.Sprite; renderer.sharedMaterial = pose.Material;
                renderer.flipX = pose.FlipX; renderer.flipY = pose.FlipY;
                renderer.sortingLayerID = pose.Layer; renderer.sortingOrder = pose.Order - 1;
                renderer.transform.SetPositionAndRotation(pose.Position, pose.Rotation); renderer.transform.localScale = pose.Scale;
                var color = pose.Color; float fade = 1 - image.Age / settings.Seconds;
                color.a *= Mathf.Clamp01(settings.Opacity) * fade * fade; renderer.color = color;
            }
            if (!emitting || frozen) emissionAnchor = now;
            previous = now;
        }
        public void Dispose()
        {
            if (!root) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(root); else UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
