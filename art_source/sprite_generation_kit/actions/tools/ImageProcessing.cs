namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Linq;

    public sealed class Raster
    {
        public readonly int Width, Height;
        public readonly int[] Pixels;
        public Raster(int width, int height) { Width = width; Height = height; Pixels = new int[width * height]; }
        public static Raster Load(string path)
        {
            using (var source = new Bitmap(path))
            using (var image = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(image)) graphics.DrawImageUnscaled(source, 0, 0);
                var raster = new Raster(image.Width, image.Height);
                var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < image.Height; y++) Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), raster.Pixels, y * image.Width, image.Width);
                image.UnlockBits(data); return raster;
            }
        }
        public void Save(string path)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)));
            using (var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                var data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < Height; y++) Marshal.Copy(Pixels, y * Width, IntPtr.Add(data.Scan0, y * data.Stride), Width);
                bitmap.UnlockBits(data); bitmap.Save(path, ImageFormat.Png);
            }
        }
    }
    public static class Pixels
    {
        public static int R(int c) { return (c >> 16) & 255; }
        public static int G(int c) { return (c >> 8) & 255; }
        public static int B(int c) { return c & 255; }
        public static int Opaque(int r, int g, int b) { return unchecked((int)0xff000000) | (r << 16) | (g << 8) | b; }
        public static bool Magenta(int c, int tolerance)
        {
            return R(c) >= 255 - tolerance && B(c) >= 255 - tolerance && G(c) <= tolerance && Math.Min(R(c), B(c)) - G(c) >= 80;
        }
        public static void Key(Raster image, int tolerance)
        {
            for (int i = 0; i < image.Pixels.Length; i++)
            {
                int c = image.Pixels[i];
                if (((uint)c >> 24) < 128 || Magenta(c, tolerance)) { image.Pixels[i] = 0; continue; }
                int r = R(c), g = G(c), b = B(c);
                // Purple fringing is removed without desaturating orange clothes or navy fabric.
                if (r > g + 35 && b > g + 35 && b > r * 0.65)
                { int spill = Math.Min(r - g, b - g); r -= spill; b -= spill; }
                image.Pixels[i] = Opaque(r, g, b);
            }
        }
        public static void RemoveSpecks(Raster image, int minimumArea)
        {
            var seen = new bool[image.Pixels.Length]; var queue = new Queue<int>(); var island = new List<int>();
            for (int start = 0; start < image.Pixels.Length; start++)
            {
                if (seen[start] || image.Pixels[start] == 0) continue;
                seen[start] = true; queue.Enqueue(start); island.Clear();
                while (queue.Count > 0)
                {
                    int p = queue.Dequeue(); island.Add(p); int x = p % image.Width, y = p / image.Width;
                    if (x > 0) Enqueue(p - 1, image, seen, queue);
                    if (x + 1 < image.Width) Enqueue(p + 1, image, seen, queue);
                    if (y > 0) Enqueue(p - image.Width, image, seen, queue);
                    if (y + 1 < image.Height) Enqueue(p + image.Width, image, seen, queue);
                }
                if (island.Count < minimumArea) foreach (int p in island) image.Pixels[p] = 0;
            }
        }
        private static void Enqueue(int p, Raster image, bool[] seen, Queue<int> queue)
        { if (!seen[p] && image.Pixels[p] != 0) { seen[p] = true; queue.Enqueue(p); } }
        public static List<Rectangle> Split(Raster image, int bridgeGap)
        {
            var runs = new List<Rectangle>(); int start = -1, last = -1;
            for (int x = 0; x < image.Width; x++)
            {
                bool occupied = false;
                for (int y = 0; y < image.Height; y++) if (image.Pixels[y * image.Width + x] != 0) { occupied = true; break; }
                if (!occupied) continue;
                if (start >= 0 && x - last - 1 > bridgeGap) { runs.Add(Bounds(image, start, last + 1)); start = -1; }
                if (start < 0) start = x;
                last = x;
            }
            if (start >= 0) runs.Add(Bounds(image, start, last + 1)); return runs;
        }
        public static Rectangle Bounds(Raster image, int left, int right)
        {
            int top = image.Height, bottom = -1, minX = right, maxX = left;
            for (int y = 0; y < image.Height; y++) for (int x = left; x < right; x++)
                if (image.Pixels[y * image.Width + x] != 0) { top = Math.Min(top, y); bottom = Math.Max(bottom, y); minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); }
            return bottom < 0 ? Rectangle.Empty : Rectangle.FromLTRB(minX, top, maxX + 1, bottom + 1);
        }
        public static double TorsoAnchor(Raster image, Rectangle frame, int[] min, int[] max, double redGreenRatio)
        {
            double sum = 0, count = 0;
            // Identify the sweater, not the silhouette bounds (extended fists would shift those bounds).
            for (int y = frame.Top; y < frame.Bottom; y++) for (int x = frame.Left; x < frame.Right; x++)
            {
                int c = image.Pixels[y * image.Width + x];
                if (c != 0 && R(c) >= min[0] && R(c) <= max[0] && G(c) >= min[1] && G(c) <= max[1] && B(c) >= min[2] && B(c) <= max[2] && (redGreenRatio <= 0 || R(c) > G(c) * redGreenRatio))
                { sum += x - frame.Left; count++; }
            }
            return count > 0 ? sum / count : frame.Width * 0.5;
        }
        public static Raster ModalShrink(Raster image, Rectangle frame, double scale)
        {
            var output = new Raster(Math.Max(1, (int)Math.Round(frame.Width * scale)), Math.Max(1, (int)Math.Round(frame.Height * scale)));
            var counts = new Dictionary<int, int>();
            for (int oy = 0; oy < output.Height; oy++) for (int ox = 0; ox < output.Width; ox++)
            {
                counts.Clear(); int opaque = 0, total = 0, dark = 0, darkest = int.MaxValue, darkColor = 0;
                int x0 = frame.Left + (int)Math.Floor(ox / scale), x1 = Math.Min(frame.Right, frame.Left + (int)Math.Ceiling((ox + 1) / scale));
                int y0 = frame.Top + (int)Math.Floor(oy / scale), y1 = Math.Min(frame.Bottom, frame.Top + (int)Math.Ceiling((oy + 1) / scale));
                for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++)
                {
                    total++; int c = image.Pixels[y * image.Width + x]; if (c == 0) continue; opaque++;
                    int q = Opaque(R(c) & 248, G(c) & 248, B(c) & 248); int n; counts.TryGetValue(q, out n); counts[q] = n + 1;
                    int light = R(c) + G(c) + B(c);
                    if (light < 140) { dark++; if (light < darkest) { darkest = light; darkColor = c; } }
                }
                if (opaque == 0 || opaque < total * 0.35) continue;
                int winner = 0, best = 0;
                foreach (var pair in counts.OrderBy(p => p.Key)) if (pair.Value > best) { winner = pair.Key; best = pair.Value; }
                // Preserve a thin silhouette line when an opaque block includes a substantial dark contour.
                output.Pixels[oy * output.Width + ox] = dark >= opaque * 0.22 ? darkColor : winner;
            }
            return output;
        }
        public static int[] Palette(IEnumerable<Raster> frames, int size)
        {
            var histogram = new Dictionary<int, long>();
            foreach (var frame in frames) foreach (int c in frame.Pixels)
            { if (c == 0) continue; int q = Opaque(R(c) & 248, G(c) & 248, B(c) & 248); long n; histogram.TryGetValue(q, out n); histogram[q] = n + 1; }
            var boxes = new List<List<KeyValuePair<int, long>>> { histogram.OrderBy(p => p.Key).ToList() };
            while (boxes.Count < size)
            {
                int chosen = -1; long score = -1;
                for (int i = 0; i < boxes.Count; i++)
                { if (boxes[i].Count < 2) continue; long value = boxes[i].Sum(p => p.Value) * Spread(boxes[i]); if (value > score) { score = value; chosen = i; } }
                if (chosen < 0) break;
                var box = boxes[chosen]; int channel = WidestChannel(box);
                box = box.OrderBy(p => Channel(p.Key, channel)).ThenBy(p => p.Key).ToList();
                long half = box.Sum(p => p.Value) / 2, accumulated = 0; int split = 0;
                while (split < box.Count - 1 && accumulated < half) accumulated += box[split++].Value;
                split = Math.Max(1, split); boxes[chosen] = box.Take(split).ToList(); boxes.Add(box.Skip(split).ToList());
            }
            return boxes.Select(box => {
                long n = box.Sum(p => p.Value);
                return Opaque((int)(box.Sum(p => (long)R(p.Key) * p.Value) / n), (int)(box.Sum(p => (long)G(p.Key) * p.Value) / n), (int)(box.Sum(p => (long)B(p.Key) * p.Value) / n));
            }).OrderBy(c => R(c) + G(c) + B(c)).ThenBy(c => c).ToArray();
        }
        private static int Channel(int c, int channel) { return channel == 0 ? R(c) : channel == 1 ? G(c) : B(c); }
        private static int WidestChannel(List<KeyValuePair<int, long>> box)
        { int best = -1, channel = 0; for (int i = 0; i < 3; i++) { int d = box.Max(p => Channel(p.Key, i)) - box.Min(p => Channel(p.Key, i)); if (d > best) { best = d; channel = i; } } return channel; }
        private static int Spread(List<KeyValuePair<int, long>> box) { int c = WidestChannel(box); return box.Max(p => Channel(p.Key, c)) - box.Min(p => Channel(p.Key, c)); }
        public static void Quantize(Raster frame, int[] palette)
        {
            var cache = new Dictionary<int, int>();
            for (int i = 0; i < frame.Pixels.Length; i++)
            {
                int c = frame.Pixels[i]; if (c == 0) continue; int selected;
                if (!cache.TryGetValue(c, out selected))
                {
                    int distance = int.MaxValue; selected = palette[0];
                    foreach (int p in palette) { int dr = R(c) - R(p), dg = G(c) - G(p), db = B(c) - B(p); int d = dr * dr * 2 + dg * dg * 3 + db * db; if (d < distance) { distance = d; selected = p; } }
                    cache[c] = selected;
                }
                frame.Pixels[i] = selected;
            }
        }
    }
}

