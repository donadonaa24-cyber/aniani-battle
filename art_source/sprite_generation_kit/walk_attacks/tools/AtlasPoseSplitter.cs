namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    // Detect actual separated figures, including capes whose x-ranges overlap another pose.
    // Never cut an assumed grid or include another figure's pixels in a bounding-box crop.
    public sealed class AtlasPose
    {
        public Raster Image;
        public Rectangle SourceBounds;
        public int Area;
    }
    public static class AtlasPoseSplitter
    {
        public static List<AtlasPose> Split(Raster source, int minimumArea)
        {
            var seen = new bool[source.Pixels.Length]; var queue = new Queue<int>();
            var found = new List<AtlasPose>(); var pixels = new List<int>();
            for (int start = 0; start < source.Pixels.Length; start++)
            {
                if (seen[start] || source.Pixels[start] == 0) continue;
                seen[start] = true; queue.Enqueue(start); pixels.Clear();
                int left = source.Width, right = -1, top = source.Height, bottom = -1;
                while (queue.Count > 0)
                {
                    int p = queue.Dequeue(), x = p % source.Width, y = p / source.Width;
                    pixels.Add(p); left = Math.Min(left, x); right = Math.Max(right, x);
                    top = Math.Min(top, y); bottom = Math.Max(bottom, y);
                    if (x > 0) Visit(p - 1, source, seen, queue);
                    if (x + 1 < source.Width) Visit(p + 1, source, seen, queue);
                    if (y > 0) Visit(p - source.Width, source, seen, queue);
                    if (y + 1 < source.Height) Visit(p + source.Width, source, seen, queue);
                }
                if (pixels.Count < minimumArea) continue;
                var bounds = Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
                var image = new Raster(bounds.Width, bounds.Height);
                foreach (int p in pixels)
                    image.Pixels[(p / source.Width - top) * image.Width + p % source.Width - left] = source.Pixels[p];
                found.Add(new AtlasPose { Image = image, SourceBounds = bounds, Area = pixels.Count });
            }
            // An eye or a fabric highlight can be separated by transparent pixels while
            // remaining wholly inside one character's bounds. Keep those pixels with
            // that character; never merge adjacent figures or guess a sprite grid.
            foreach (var detail in found.OrderBy(p => p.Area).ToArray())
            {
                var parents = found.Where(p => p != detail && p.Area > detail.Area &&
                    p.SourceBounds.Contains(detail.SourceBounds)).ToArray();
                if (parents.Length > 1) throw new InvalidOperationException("An isolated detail belongs to ambiguous figure bounds.");
                if (parents.Length == 0) continue;
                var parent = parents[0];
                int dx = detail.SourceBounds.Left - parent.SourceBounds.Left, dy = detail.SourceBounds.Top - parent.SourceBounds.Top;
                for (int y = 0; y < detail.Image.Height; y++) for (int x = 0; x < detail.Image.Width; x++)
                {
                    int color = detail.Image.Pixels[y * detail.Image.Width + x];
                    if (color != 0) parent.Image.Pixels[(y + dy) * parent.Image.Width + x + dx] = color;
                }
                parent.Area += detail.Area; found.Remove(detail);
            }
            return found.OrderBy(p => p.SourceBounds.Left + p.SourceBounds.Width * .5).ToList();
        }
        private static void Visit(int p, Raster source, bool[] seen, Queue<int> queue)
        {
            if (seen[p] || source.Pixels[p] == 0) return;
            seen[p] = true; queue.Enqueue(p);
        }
    }
}
