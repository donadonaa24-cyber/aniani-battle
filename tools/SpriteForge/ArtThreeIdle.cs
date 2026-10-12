namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    // Fixed-grid export of the approved ART3 drawing. Generated key guides are retained separately.
    // An integer row remap preserves the face/costume instead of crossfading different drawings.
    public static class ArtThreeIdle
    {
        public const int Width = 224, Height = 160, Baseline = 159, Count = 8, Fps = 6;
        private static readonly int[] Offsets = { 0, -1, -2, -1, 0, 1, 2, 1 };
        private static readonly string[] Characters = { "toma", "ren", "chizuru" };
        private const string Folder = "art_source/art3_idle/breathing/";

        public static void Prepare(string root)
        {
            var json = new JavaScriptSerializer();
            foreach (string name in Characters)
            {
                string manifestPath = Path.Combine(root, "art_source/" + name + ".manifest.json");
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(manifestPath));
                if (manifest.cellWidth != Width || manifest.cellHeight != Height || manifest.baseline != Baseline || manifest.pixelsPerUnit != 50)
                    throw new InvalidOperationException("Character violates the fixed sprite production specification.");
                var source = Raster.Load(Path.Combine(root, "art_source/art3_idle/" + name + "_idle_v1.png"));
                Pixels.Key(source, manifest.keyTolerance); Pixels.RemoveSpecks(source, 70);
                var bounds = Pixels.Bounds(source, 0, source.Width);
                double scale = manifest.targetHeight / (double)bounds.Height;
                double anchor = Pixels.TorsoAnchor(source, bounds, manifest.anchorRgbMin, manifest.anchorRgbMax, manifest.anchorRedGreenRatio);
                var drawing = Pixels.ModalShrink(source, bounds, scale);
                var neutral = new Raster(Width, Height);
                int xOffset = Width / 2 - (int)Math.Round(anchor * scale), yOffset = Baseline - drawing.Height + 1;
                for (int y = 0; y < drawing.Height; y++) for (int x = 0; x < drawing.Width; x++)
                {
                    int color = drawing.Pixels[y * drawing.Width + x]; if (color == 0) continue;
                    int dx = x + xOffset, dy = y + yOffset;
                    if (dx < 2 || dx >= Width - 2 || dy < 3 || dy > Baseline)
                        throw new InvalidOperationException("Idle drawing clips the fixed canvas: " + name);
                    neutral.Pixels[dy * Width + dx] = color;
                }
                var sheet = new Raster(Width * Count, Height);
                for (int frame = 0; frame < Count; frame++)
                {
                    // Upper body is translated unchanged. Lower legs absorb the 1-2px motion;
                    // bottom 12 rows (shoes/ground contact) are identical in every frame.
                    const int fixedFeetTop = Height - 12, fullBodyBottom = Height - 28;
                    for (int y = 0; y < Height; y++)
                    {
                        double weight = y <= fullBodyBottom ? 1 : y >= fixedFeetTop ? 0 :
                            (double)(fixedFeetTop - y) / (fixedFeetTop - fullBodyBottom);
                        int sourceY = y - (int)Math.Round(Offsets[frame] * weight);
                        if (sourceY < 0 || sourceY >= Height) continue;
                        Array.Copy(neutral.Pixels, sourceY * Width, sheet.Pixels, y * sheet.Width + frame * Width, Width);
                    }
                }
                string relative = Folder + "normalized/" + name + "_idle.png";
                sheet.Save(Path.Combine(root, relative));
                var clip = manifest.animations.Single(a => a.name == "idle");
                clip.prebuiltSheet = relative; clip.stableIdle = false; clip.fps = Fps;
                // Original raw idle/reference frames and all other clips retain their scale and palette.
                File.WriteAllText(manifestPath, json.Serialize(manifest), new System.Text.UTF8Encoding(false));
            }
        }

        public static void Publish(string root)
        {
            foreach (string name in Characters)
            {
                string game = Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + name + "/");
                var sheet = Raster.Load(game + "idle.png");
                string ready = Path.Combine(root, Folder + "ready/" + name + "/"); Directory.CreateDirectory(ready);
                File.Copy(game + "idle.png", ready + "idle_sheet.png", true);
                File.Copy(game + "idle.json", ready + "idle.json", true);
                for (int frame = 0; frame < Count; frame++)
                {
                    var cell = new Raster(Width, Height);
                    for (int y = 0; y < Height; y++) Array.Copy(sheet.Pixels, y * sheet.Width + frame * Width, cell.Pixels, y * Width, Width);
                    cell.Save(ready + "idle_" + frame.ToString("D2") + ".png");
                }
                File.Copy(Path.Combine(root, "docs/art_wip/" + name + "_idle_preview.gif"), ready + "idle_preview.gif", true);
                const int zoom = 3;
                var enlarged = new Raster(sheet.Width * zoom, sheet.Height * zoom);
                for (int y = 0; y < enlarged.Height; y++) for (int x = 0; x < enlarged.Width; x++)
                    enlarged.Pixels[y * enlarged.Width + x] = sheet.Pixels[(y / zoom) * sheet.Width + x / zoom];
                int[] colors = sheet.Pixels.Where(color => color != 0).Distinct().OrderBy(color => color).ToArray();
                AnimationSmoothing.Preview(enlarged, Count, Fps, colors, Path.Combine(root, "docs/art_wip/art3b_" + name + "_idle_preview.gif"));
            }
            // Same nearest-neighbor zoom and feet baseline for all three characters.
            using (var image = new Bitmap(Width * 3 * 3, Height * 3))
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.FromArgb(34, 39, 49));
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor; graphics.PixelOffsetMode = PixelOffsetMode.Half;
                for (int i = 0; i < Characters.Length; i++)
                using (var frame = new Bitmap(Path.Combine(root, Folder + "ready/" + Characters[i] + "/idle_00.png")))
                    graphics.DrawImage(frame, new Rectangle(i * Width * 3, 0, Width * 3, Height * 3), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);
                image.Save(Path.Combine(root, "docs/art_wip/art3b_idle_lineup.png"));
            }
        }

        public static void Verify(string root)
        {
            var json = new JavaScriptSerializer(); var reports = new List<object>();
            foreach (string name in Characters)
            {
                string path = Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + name + "/");
                var sheet = Raster.Load(path + "idle.png");
                var metadata = json.Deserialize<Metadata>(File.ReadAllText(path + "idle.json"));
                if (sheet.Width != Width * Count || sheet.Height != Height || metadata.frameCount != Count || metadata.fps != Fps ||
                    metadata.baseline != Baseline || metadata.pixelsPerUnit != 50 || metadata.pivotX != .5f || metadata.pivotY != 0 || !metadata.loop)
                    throw new InvalidOperationException("Idle metadata violates the production specification.");
                var palette = new HashSet<int>(); var neutralBounds = Pixels.Bounds(sheet, 0, Width);
                int torsoChanges = 0;
                for (int frame = 0; frame < Count; frame++)
                {
                    var bounds = Pixels.Bounds(sheet, frame * Width, (frame + 1) * Width);
                    if (bounds.Bottom != Height || bounds.Top != neutralBounds.Top + Offsets[frame] || metadata.headHeights[frame] != Height - bounds.Top)
                        throw new InvalidOperationException("Idle head/foot trajectory is incorrect: " + name);
                    for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
                    {
                        int color = sheet.Pixels[y * sheet.Width + frame * Width + x];
                        if (color != 0) { if (((uint)color >> 24) != 255) throw new InvalidOperationException("Non-binary idle alpha."); palette.Add(color); }
                        if (y >= Height - 12 && color != sheet.Pixels[y * sheet.Width + x]) throw new InvalidOperationException("Feet slide in the breathing loop.");
                        if (frame == 2 && y >= neutralBounds.Top + neutralBounds.Height / 2 && y < Height - 28 && color != sheet.Pixels[y * sheet.Width + x]) torsoChanges++;
                        // Head and torso are exact translated pixels, not new drawings or blended colors.
                        int sourceY = y - Offsets[frame];
                        if (y < Height - 28 && sourceY >= 0 && sourceY < Height && color != sheet.Pixels[sourceY * sheet.Width + x])
                            throw new InvalidOperationException("Upper-body identity drift in breathing loop.");
                    }
                }
                if (palette.Count > 32 || torsoChanges == 0) throw new InvalidOperationException("Palette limit or visible body breathing failed.");
                using (var gif = Image.FromFile(Path.Combine(root, "docs/art_wip/" + name + "_idle_preview.gif")))
                {
                    if (gif.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) != Count) throw new InvalidOperationException("Breathing GIF frame count mismatch.");
                    for (int i = 0; i < Count; i++) gif.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, i);
                }
                reports.Add(new { character = name, frames = Count, cellWidth = Width, cellHeight = Height, fps = Fps, colors = palette.Count,
                    plantedFeet = true, exactTranslatedUpperBody = true, torsoChanges = torsoChanges, headHeights = metadata.headHeights });
            }
            File.WriteAllText(Path.Combine(root, "TestResults/art3b-breathing-verification.json"), json.Serialize(reports), new System.Text.UTF8Encoding(false));
            Console.WriteLine("ART3B: 24 breathing frames; fixed 224x160/PPU50; planted feet; exact body translation; <=32 colors; GIF decode PASS.");
        }
    }
}
