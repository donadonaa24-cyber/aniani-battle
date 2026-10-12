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
            for (int i = 0; i < count; i++)
            {
                Raster pose;
                if (clip.stableIdle) pose = Breathe(poses[0], i, count, manifest.baseline, clip.headOffset);
                else
                {
                    double phase = clip.loop ? (double)i * clip.frames / count : (double)i * (clip.frames - 1) / (count - 1);
                    int a = (int)phase, b = clip.loop ? (a + 1) % clip.frames : Math.Min(clip.frames - 1, a + 1);
                    // Hold one complete key drawing. Never merge pixels from adjacent poses.
                    pose = poses[phase - a < .5 ? a : b];
                }
                if (clip.anchor == "feet") AlignFeet(pose, manifest.baseline);
                Rectangle bounds = Pixels.Bounds(pose, 0, width); heads[i] = manifest.baseline - bounds.Top + 1;
                for (int y = 0; y < height; y++) Array.Copy(pose.Pixels, y * width, result.Pixels, y * result.Width + i * width, width);
            }
            return result;
        }
        private static Raster Breathe(Raster reference, int frame, int count, int baseline, int offset)
        {
            var pose = new Raster(reference.Width, reference.Height);
            Rectangle bounds = Pixels.Bounds(reference, 0, reference.Width);
            // Fixed torso and planted feet. The upper silhouette moves by at most one pixel
            // over a symmetric cycle; every pixel/color comes from ONE reference drawing.
            int shift = (int)Math.Round(Math.Sin(frame * Math.PI * 2 / count)) + offset;
            int chest = bounds.Top + bounds.Height * 2 / 5;
            for (int y = Math.Max(0, bounds.Top - 1); y <= baseline; y++) for (int x = 0; x < reference.Width; x++)
            {
                int sourceY = y < chest - 3 ? y - shift : y;
                if (sourceY >= 0 && sourceY < reference.Height) pose.Pixels[y * pose.Width + x] = reference.Pixels[sourceY * reference.Width + x];
            }
            return pose;
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
