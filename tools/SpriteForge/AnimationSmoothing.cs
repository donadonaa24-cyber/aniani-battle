namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    // Offline pixel-art generation from owned key poses. No alpha blending, new colors,
    // neural service, or rescaling of characters. Source drawings are always retained.
    public static class AnimationSmoothing
    {
        public static int Count(Animation clip) { return clip.inbetweenFrames > 0 ? clip.inbetweenFrames : clip.frames; }
        public static Raster Generate(Raster keys, Animation clip, Manifest manifest, out int[] heads)
        {
            int width = manifest.cellWidth, height = manifest.cellHeight, count = Count(clip);
            var result = new Raster(width * count, height); heads = new int[count];
            var poses = new Raster[clip.frames];
            for (int i = 0; i < poses.Length; i++)
            {
                poses[i] = new Raster(width, height);
                for (int y = 0; y < height; y++) Array.Copy(keys.Pixels, y * keys.Width + i * width, poses[i].Pixels, y * width, width);
            }
            if (!clip.stableIdle && clip.anchor == "feet") AlignLandmarks(poses, manifest);
            for (int i = 0; i < count; i++)
            {
                Raster pose;
                if (clip.stableIdle) pose = Breathe(poses[0], i, count, manifest.baseline);
                else
                {
                    double phase = clip.loop ? (double)i * clip.frames / count : (double)i * (clip.frames - 1) / (count - 1);
                    int a = (int)phase, b = clip.loop ? (a + 1) % clip.frames : Math.Min(clip.frames - 1, a + 1);
                    pose = Morph(poses[a], poses[b], phase - a);
                }
                if (clip.anchor == "feet") AlignFeet(pose, manifest.baseline);
                Rectangle bounds = Pixels.Bounds(pose, 0, width); heads[i] = manifest.baseline - bounds.Top + 1;
                for (int y = 0; y < height; y++) Array.Copy(pose.Pixels, y * width, result.Pixels, y * result.Width + i * width, width);
            }
            return result;
        }
        private static Raster Breathe(Raster reference, int frame, int count, int baseline)
        {
            var pose = new Raster(reference.Width, reference.Height);
            Rectangle bounds = Pixels.Bounds(reference, 0, reference.Width);
            // Fixed torso and planted feet. The upper silhouette moves by at most one pixel
            // over a symmetric cycle; every pixel/color comes from ONE reference drawing.
            int shift = (int)Math.Round(Math.Sin(frame * Math.PI * 2 / count));
            int chest = bounds.Top + bounds.Height * 2 / 5;
            for (int y = Math.Max(0, bounds.Top - 1); y <= baseline; y++) for (int x = 0; x < reference.Width; x++)
            {
                int sourceY = y < chest - 3 ? y - shift : y;
                if (sourceY >= 0 && sourceY < reference.Height) pose.Pixels[y * pose.Width + x] = reference.Pixels[sourceY * reference.Width + x];
            }
            return pose;
        }
        private static Raster Morph(Raster a, Raster b, double phase)
        {
            if (phase < 1e-8) return a;
            var pose = new Raster(a.Width, a.Height);
            Raster source = phase < .5 ? a : b;
            for (int y = 0; y < a.Height; y++)
            {
                var first = Runs(a, y); var second = Runs(b, y);
                // Separate limbs must never be stretched through the transparent gap.
                // Topology changes use the nearest key pose rather than inventing a smear.
                if (first.Count != second.Count) { Array.Copy(source.Pixels, y * a.Width, pose.Pixels, y * a.Width, a.Width); continue; }
                for (int run = 0; run < first.Count; run++)
                {
                    int la = first[run].Item1, ra = first[run].Item2, lb = second[run].Item1, rb = second[run].Item2;
                    int sl = phase < .5 ? la : lb, sr = phase < .5 ? ra : rb;
                    if (Math.Abs(la - lb) > 24 || Math.Abs(ra - rb) > 24 || Math.Max(ra-la+1,rb-lb+1) > 2 * Math.Min(ra-la+1,rb-lb+1))
                    { Array.Copy(source.Pixels, y * source.Width + sl, pose.Pixels, y * pose.Width + sl, sr - sl + 1); continue; }
                    int left = (int)Math.Round(la + (lb - la) * phase), right = (int)Math.Round(ra + (rb - ra) * phase);
                    for (int x = left; x <= right; x++)
                    {
                        int sx = sl + (int)Math.Round((double)(x - left) * (sr - sl) / Math.Max(1, right - left));
                        pose.Pixels[y * pose.Width + x] = source.Pixels[y * source.Width + sx];
                    }
                }
            }
            return pose;
        }
        private static List<Tuple<int,int>> Runs(Raster pose, int y)
        {
            var runs = new List<Tuple<int,int>>(); int start = -1;
            for (int x = 0; x <= pose.Width; x++)
            {
                bool opaque = x < pose.Width && pose.Pixels[y * pose.Width + x] != 0;
                if (opaque && start < 0) start = x;
                if (!opaque && start >= 0) { runs.Add(Tuple.Create(start, x - 1)); start = -1; }
            }
            return runs;
        }
        private static void AlignLandmarks(Raster[] poses, Manifest manifest)
        {
            var bounds = poses.Select(pose => Pixels.Bounds(pose, 0, pose.Width)).ToArray();
            var centers = poses.Select(pose => TorsoY(pose, manifest)).ToArray();
            int top = bounds.Select(box => box.Top).OrderBy(value => value).ElementAt(poses.Length / 2);
            int torso = centers.OrderBy(value => value).ElementAt(poses.Length / 2);
            for (int frame = 0; frame < poses.Length; frame++)
            {
                // Correct small generation drift only. A duck, kick or deliberate lean
                // keeps its own silhouette rather than being stretched into an idle pose.
                if (Math.Abs(bounds[frame].Top - top) > 6 || Math.Abs(centers[frame] - torso) > 6) continue;
                var original = poses[frame]; var aligned = new Raster(original.Width, original.Height);
                for (int y = top; y <= manifest.baseline; y++)
                {
                    double sy = y <= torso ? bounds[frame].Top + (double)(y - top) * (centers[frame] - bounds[frame].Top) / Math.Max(1, torso - top) :
                        centers[frame] + (double)(y - torso) * (manifest.baseline - centers[frame]) / Math.Max(1, manifest.baseline - torso);
                    int sourceY = Math.Max(0, Math.Min(original.Height - 1, (int)Math.Round(sy)));
                    Array.Copy(original.Pixels, sourceY * original.Width, aligned.Pixels, y * original.Width, original.Width);
                }
                poses[frame] = aligned;
            }
        }
        private static int TorsoY(Raster pose, Manifest manifest)
        {
            double sum = 0, count = 0;
            for (int y = 0; y < pose.Height; y++) for (int x = pose.Width / 2 - 18; x <= pose.Width / 2 + 18; x++)
            {
                int color = pose.Pixels[y * pose.Width + x];
                if (color != 0 && Pixels.R(color) >= manifest.anchorRgbMin[0] && Pixels.R(color) <= manifest.anchorRgbMax[0] &&
                    Pixels.G(color) >= manifest.anchorRgbMin[1] && Pixels.G(color) <= manifest.anchorRgbMax[1] && Pixels.B(color) >= manifest.anchorRgbMin[2] && Pixels.B(color) <= manifest.anchorRgbMax[2] &&
                    (manifest.anchorRedGreenRatio <= 0 || Pixels.R(color) > Pixels.G(color) * manifest.anchorRedGreenRatio)) { sum += y; count++; }
            }
            var box = Pixels.Bounds(pose, 0, pose.Width); return count > 0 ? (int)Math.Round(sum / count) : box.Top + box.Height / 2;
        }
        private static void AlignFeet(Raster pose, int baseline)
        {
            var bounds = Pixels.Bounds(pose, 0, pose.Width); int dy = baseline - bounds.Bottom + 1;
            if (dy == 0) return; var copy = (int[])pose.Pixels.Clone(); Array.Clear(pose.Pixels, 0, pose.Pixels.Length);
            for (int y = 0; y < pose.Height; y++) if (y + dy >= 0 && y + dy < pose.Height) Array.Copy(copy, y * pose.Width, pose.Pixels, (y + dy) * pose.Width, pose.Width);
        }
        // Minimal GIF89a encoder. Frequent LZW clear codes keep a fixed 9-bit code width.
        public static void Preview(Raster sheet, int count, int fps, int[] palette, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)); int w = sheet.Width / count, h = sheet.Height;
            var colors = new[] { Pixels.Opaque(34, 39, 49) }.Concat(palette).Distinct().ToArray();
            var index = new Dictionary<int, int>(); for (int i = 0; i < colors.Length; i++) index[colors[i]] = i;
            using (var stream = File.Create(path)) using (var output = new BinaryWriter(stream))
            {
                output.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a")); output.Write((ushort)w); output.Write((ushort)h); output.Write((byte)0xf7); output.Write((byte)0); output.Write((byte)0);
                for (int i = 0; i < 256; i++) { int c = i < colors.Length ? colors[i] : colors[0]; output.Write((byte)Pixels.R(c)); output.Write((byte)Pixels.G(c)); output.Write((byte)Pixels.B(c)); }
                output.Write(new byte[] { 0x21, 0xff, 11 }); output.Write(System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0")); output.Write(new byte[] { 3, 1, 0, 0, 0 });
                for (int frame = 0; frame < count; frame++)
                {
                    output.Write(new byte[] { 0x21, 0xf9, 4, 0 }); output.Write((ushort)Math.Max(1, Math.Round(100d / fps))); output.Write(new byte[] { 0, 0, 0x2c });
                    output.Write((ushort)0); output.Write((ushort)0); output.Write((ushort)w); output.Write((ushort)h); output.Write((byte)0); output.Write((byte)8);
                    var data = new List<byte>(); int bits = 0, used = 0, run = 0;
                    Action<int> code = value => { bits |= value << used; used += 9; while (used >= 8) { data.Add((byte)bits); bits >>= 8; used -= 8; } };
                    code(256);
                    for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                    { if (run++ == 200) { code(256); run = 1; } int c = sheet.Pixels[y * sheet.Width + frame * w + x]; code(c == 0 ? 0 : index[c]); }
                    code(257); if (used > 0) data.Add((byte)bits);
                    for (int start = 0; start < data.Count; start += 255) { int size = Math.Min(255, data.Count - start); output.Write((byte)size); output.Write(data.Skip(start).Take(size).ToArray()); }
                    output.Write((byte)0);
                }
                output.Write((byte)0x3b);
            }
        }
    }
}
