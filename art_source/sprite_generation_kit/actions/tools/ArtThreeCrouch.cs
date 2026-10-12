namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    public static class ArtThreeCrouch
    {
        public const int Width = 224, Height = 160, Baseline = 159, Count = 8, Fps = 6;
        private static readonly int[] Offsets = { 0, -1, -2, -1, 0, 1, 2, 1 };
        private static readonly string[] Characters = { "toma", "ren", "chizuru" };
        private const string Folder = "art_source/art3_crouch/";

        public static void Prepare(string root)
        {
            var json = new JavaScriptSerializer(); var records = new List<object>();
            foreach (string name in Characters)
            {
                string manifestPath = Path.Combine(root, "art_source/" + name + ".manifest.json");
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(manifestPath));
                if (manifest.cellWidth != Width || manifest.cellHeight != Height || manifest.baseline != Baseline || manifest.pixelsPerUnit != 50)
                    throw new InvalidOperationException("Crouch violates the shared sprite production specification.");
                string sourceRelative = Folder + "generated/" + name + "_crouch_pair_v1.png";
                var source = Raster.Load(Path.Combine(root, sourceRelative));
                Pixels.Key(source, manifest.keyTolerance); Pixels.RemoveSpecks(source, 70);
                var poses = Pixels.Split(source, 12);
                if (poses.Count != 2) throw new InvalidOperationException("Expected standing-left and crouch-right: " + name);
                var standing = poses[0]; var crouching = poses[1];
                if (Math.Abs(standing.Bottom - crouching.Bottom) > Math.Ceiling(standing.Height * .025))
                    throw new InvalidOperationException("Source poses do not share a ground baseline: " + name);
                // One ruler for BOTH poses. Never fit the crouch to a target bounding-box height.
                double scale = manifest.targetHeight / (double)standing.Height;
                var drawing = Pixels.ModalShrink(source, crouching, scale);
                if (drawing.Height < manifest.targetHeight * .55 || drawing.Height > manifest.targetHeight * .85)
                    throw new InvalidOperationException("Crouch height is outside the reviewed pose range: " + name);
                double anchor = Pixels.TorsoAnchor(source, crouching, manifest.anchorRgbMin, manifest.anchorRgbMax, manifest.anchorRedGreenRatio);
                int xOffset = Width / 2 - (int)Math.Round(anchor * scale), yOffset = Baseline - drawing.Height + 1;
                var neutral = new Raster(Width, Height);
                for (int y = 0; y < drawing.Height; y++) for (int x = 0; x < drawing.Width; x++)
                {
                    int color = drawing.Pixels[y * drawing.Width + x]; if (color == 0) continue;
                    int dx = x + xOffset, dy = y + yOffset;
                    if (dx < 2 || dx >= Width - 2 || dy < 3 || dy > Baseline)
                        throw new InvalidOperationException("Crouch clips the fixed canvas: " + name);
                    neutral.Pixels[dy * Width + dx] = color;
                }
                var sheet = new Raster(Width * Count, Height);
                for (int frame = 0; frame < Count; frame++) for (int y = 0; y < Height; y++)
                {
                    const int fixedFeetTop = Height - 12, fullBodyBottom = Height - 28;
                    double weight = y <= fullBodyBottom ? 1 : y >= fixedFeetTop ? 0 :
                        (double)(fixedFeetTop - y) / (fixedFeetTop - fullBodyBottom);
                    int sourceY = y - (int)Math.Round(Offsets[frame] * weight);
                    if (sourceY >= 0 && sourceY < Height)
                        Array.Copy(neutral.Pixels, sourceY * Width, sheet.Pixels, y * sheet.Width + frame * Width, Width);
                }
                string relative = Folder + "normalized/" + name + "_crouch.png";
                sheet.Save(Path.Combine(root, relative));
                var clip = manifest.animations.Single(a => a.name == "crouch");
                if (AnimationSmoothing.Count(clip) != Count) throw new InvalidOperationException("Crouch frame count changed unexpectedly.");
                clip.prebuiltSheet = relative; clip.stableIdle = false; clip.fps = Fps;
                File.WriteAllText(manifestPath, json.Serialize(manifest), new System.Text.UTF8Encoding(false));
                records.Add(new { character = name, source = sourceRelative, sourceWidth = source.Width, sourceHeight = source.Height,
                    standingBounds = new[] { standing.X, standing.Y, standing.Width, standing.Height },
                    crouchBounds = new[] { crouching.X, crouching.Y, crouching.Width, crouching.Height },
                    standingReferenceHeight = manifest.targetHeight, commonScale = scale, crouchHeight = drawing.Height,
                    independentCrouchResize = false, xOffset = xOffset, yOffset = yOffset });
            }
            File.WriteAllText(Path.Combine(root, Folder + "normalization.json"), json.Serialize(records), new System.Text.UTF8Encoding(false));
        }

        public static void Publish(string root)
        {
            foreach (string name in Characters)
            {
                string game = Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + name + "/");
                var sheet = Raster.Load(game + "crouch.png");
                string ready = Path.Combine(root, Folder + "ready/" + name + "/"); Directory.CreateDirectory(ready);
                File.Copy(game + "crouch.png", ready + "crouch_sheet.png", true);
                File.Copy(game + "crouch.json", ready + "crouch.json", true);
                for (int frame = 0; frame < Count; frame++)
                {
                    var cell = new Raster(Width, Height);
                    for (int y = 0; y < Height; y++) Array.Copy(sheet.Pixels, y * sheet.Width + frame * Width, cell.Pixels, y * Width, Width);
                    cell.Save(ready + "crouch_" + frame.ToString("D2") + ".png");
                }
                File.Copy(Path.Combine(root, "docs/art_wip/" + name + "_crouch_preview.gif"), ready + "crouch_preview.gif", true);
                var enlarged = new Raster(sheet.Width * 3, sheet.Height * 3);
                for (int y = 0; y < enlarged.Height; y++) for (int x = 0; x < enlarged.Width; x++)
                    enlarged.Pixels[y * enlarged.Width + x] = sheet.Pixels[(y / 3) * sheet.Width + x / 3];
                AnimationSmoothing.Preview(enlarged, Count, Fps, sheet.Pixels.Where(c => c != 0).Distinct().OrderBy(c => c).ToArray(),
                    Path.Combine(root, "docs/art_wip/art3c_" + name + "_crouch_preview.gif"));
            }
            using (var image = new Bitmap(Width * 3 * 3, Height * 3 * 2))
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.FromArgb(34, 39, 49));
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor; graphics.PixelOffsetMode = PixelOffsetMode.Half;
                for (int i = 0; i < Characters.Length; i++) for (int row = 0; row < 2; row++)
                using (var frame = new Bitmap(Path.Combine(root, row == 0 ?
                    "art_source/art3_idle/breathing/ready/" + Characters[i] + "/idle_00.png" :
                    Folder + "ready/" + Characters[i] + "/crouch_00.png")))
                    graphics.DrawImage(frame, new Rectangle(i * Width * 3, row * Height * 3, Width * 3, Height * 3),
                        new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);
                image.Save(Path.Combine(root, "docs/art_wip/art3c_pose_comparison.png"));
            }
        }

        public static void Verify(string root)
        {
            var json = new JavaScriptSerializer(); var reports = new List<object>();
            foreach (string name in Characters)
            {
                string path = Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + name + "/");
                var sheet = Raster.Load(path + "crouch.png");
                var metadata = json.Deserialize<Metadata>(File.ReadAllText(path + "crouch.json"));
                if (sheet.Width != Width * Count || sheet.Height != Height || metadata.frameCount != Count || metadata.fps != Fps ||
                    metadata.baseline != Baseline || metadata.pixelsPerUnit != 50 || metadata.pivotX != .5f || metadata.pivotY != 0 || !metadata.loop)
                    throw new InvalidOperationException("Crouch metadata violates the production specification.");
                var palette = new HashSet<int>(); var neutral = Pixels.Bounds(sheet, 0, Width); int bodyChanges = 0;
                int standingHeight = metadata.targetHeight;
                if (neutral.Height < standingHeight * .55 || neutral.Height > standingHeight * .85)
                    throw new InvalidOperationException("Crouch does not lower the body at its standing scale.");
                for (int frame = 0; frame < Count; frame++)
                {
                    var bounds = Pixels.Bounds(sheet, frame * Width, (frame + 1) * Width);
                    if (bounds.Bottom != Height || bounds.Top != neutral.Top + Offsets[frame] || metadata.headHeights[frame] != Height - bounds.Top)
                        throw new InvalidOperationException("Crouch head/foot trajectory is incorrect: " + name);
                    for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
                    {
                        int color = sheet.Pixels[y * sheet.Width + frame * Width + x];
                        if (color != 0) { if (((uint)color >> 24) != 255) throw new InvalidOperationException("Non-binary crouch alpha."); palette.Add(color); }
                        if (y >= Height - 12 && color != sheet.Pixels[y * sheet.Width + x]) throw new InvalidOperationException("Crouch feet slide.");
                        int sourceY = y - Offsets[frame];
                        if (y < Height - 28 && sourceY >= 0 && sourceY < Height && color != sheet.Pixels[sourceY * sheet.Width + x])
                            throw new InvalidOperationException("Crouch face/costume pixel drift.");
                        if (frame == 2 && y >= neutral.Top + neutral.Height / 2 && y < Height - 28 && color != sheet.Pixels[y * sheet.Width + x]) bodyChanges++;
                    }
                }
                if (palette.Count > 32 || bodyChanges == 0) throw new InvalidOperationException("Crouch palette/body motion failed.");
                using (var gif = Image.FromFile(Path.Combine(root, "docs/art_wip/" + name + "_crouch_preview.gif")))
                {
                    if (gif.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) != Count) throw new InvalidOperationException("Crouch GIF frame count mismatch.");
                    for (int i = 0; i < Count; i++) gif.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, i);
                }
                reports.Add(new { character = name, frames = Count, cellWidth = Width, cellHeight = Height, fps = Fps,
                    neutralHeight = neutral.Height, colors = palette.Count, plantedFeet = true, exactTranslatedUpperBody = true,
                    bodyChanges = bodyChanges, headHeights = metadata.headHeights });
            }
            File.WriteAllText(Path.Combine(root, "TestResults/art3c-crouch-verification.json"), json.Serialize(reports), new System.Text.UTF8Encoding(false));
            Console.WriteLine("ART3C: 24 crouch frames; fixed 224x160/PPU50; standing-based scale; planted feet; preserved face/costume pixels; <=32 colors; GIF decode PASS.");
        }
    }
}
