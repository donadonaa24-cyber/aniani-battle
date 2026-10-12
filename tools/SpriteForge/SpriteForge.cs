namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    public static class Forge
    {
        private sealed class Clip { public Animation Definition; public Raster[] Frames; public Rectangle[] SourceBounds; public double[] Anchors; }
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };
        public static void Run(string manifestPath, bool verifyOnly)
        {
            string full = Path.GetFullPath(manifestPath), root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(full), ".."));
            var manifest = Json.Deserialize<Manifest>(File.ReadAllText(full)); ValidateManifest(manifest);
            string input = Within(root, manifest.sourceDirectory), output = Within(root, manifest.outputDirectory);
            if (verifyOnly) { Verify(manifest, output); Console.WriteLine("SpriteForge: 全シート検証成功"); return; }
            var sources = new List<Tuple<Animation, Raster, List<Rectangle>>>();
            foreach (var animation in manifest.animations)
            {
                var source = Raster.Load(Path.Combine(input, (animation.sourceClip ?? animation.name) + ".png"));
                Pixels.Key(source, manifest.keyTolerance); Pixels.RemoveSpecks(source, animation.anchor == "effect" ? 12 : 70);
                var bounds = Pixels.Split(source, 12);
                if (animation.sourceFrames != null) bounds = animation.sourceFrames.Select(index => bounds[index]).ToList();
                if (bounds.Count != animation.frames) throw new InvalidOperationException(animation.name + ": expected " + animation.frames + " frames, detected " + bounds.Count + ". Repair the raw strip; no guessed grid slicing.");
                sources.Add(Tuple.Create(animation, source, bounds));
            }
            var reference = sources.First(s => s.Item1.name == "idle");
            double referenceHeight = reference.Item3.Select(r => r.Height).OrderBy(h => h).ElementAt(reference.Item3.Count / 2);
            double scale = manifest.targetHeight / referenceHeight;
            var clips = new List<Clip>();
            foreach (var source in sources)
            {
                var clip = new Clip { Definition = source.Item1, Frames = new Raster[source.Item3.Count], SourceBounds = source.Item3.ToArray(), Anchors = new double[source.Item3.Count] };
                for (int i = 0; i < clip.Frames.Length; i++)
                {
                    clip.Anchors[i] = source.Item1.anchor == "effect" ? source.Item3[i].Width * 0.5 : source.Item1.sourceAnchors != null ? source.Item1.sourceAnchors[i] : Pixels.TorsoAnchor(source.Item2, source.Item3[i], manifest.anchorRgbMin, manifest.anchorRgbMax, manifest.anchorRedGreenRatio);
                    clip.Frames[i] = Pixels.ModalShrink(source.Item2, source.Item3[i], scale * source.Item1.sourceScaleFactor);
                }
                clips.Add(clip);
            }
            int[] palette = Pixels.Palette(clips.SelectMany(c => c.Frames), manifest.paletteSize);
            Directory.CreateDirectory(output); var reports = new List<object>();
            foreach (var clip in clips)
            {
                var sheet = new Raster(manifest.cellWidth * clip.Frames.Length, manifest.cellHeight);
                var details = new List<object>(); var headHeights = new int[clip.Frames.Length];
                for (int i = 0; i < clip.Frames.Length; i++)
                {
                    var frame = clip.Frames[i]; Pixels.Quantize(frame, palette);
                    var bounds = Pixels.Bounds(frame, 0, frame.Width);
                    int xOffset = manifest.cellWidth / 2 - (int)Math.Round(clip.Anchors[i] * scale * clip.Definition.sourceScaleFactor);
                    int yOffset = clip.Definition.anchor == "feet" ? manifest.baseline - bounds.Bottom + 1 :
                        (int)Math.Round(manifest.baseline - manifest.targetHeight * 0.5 - (bounds.Top + bounds.Bottom - 1) * 0.5);
                    headHeights[i] = manifest.baseline - yOffset - bounds.Top + 1;
                    for (int y = 0; y < frame.Height; y++) for (int x = 0; x < frame.Width; x++)
                    {
                        int c = frame.Pixels[y * frame.Width + x]; if (c == 0) continue;
                        int dx = x + xOffset, dy = y + yOffset;
                        if (dx < 0 || dx >= manifest.cellWidth || dy < 0 || dy >= manifest.cellHeight)
                            throw new InvalidOperationException(clip.Definition.name + ": frame " + i + " clips at cell boundary; increase cell size or repair source anchor.");
                        sheet.Pixels[dy * sheet.Width + i * manifest.cellWidth + dx] = c;
                    }
                    details.Add(new { frame = i, sourceBounds = new { x = clip.SourceBounds[i].X, y = clip.SourceBounds[i].Y, width = clip.SourceBounds[i].Width, height = clip.SourceBounds[i].Height }, sourceTorsoAnchor = clip.Anchors[i], height = bounds.Height, xOffset, yOffset });
                }
                if (clip.Definition.inbetweenFrames > 0)
                    sheet = AnimationSmoothing.Generate(sheet, clip.Definition, manifest, out headHeights);
                int outputCount = AnimationSmoothing.Count(clip.Definition);
                if (!String.IsNullOrEmpty(clip.Definition.prebuiltSheet))
                {
                    // Keep the original raw idle as the common scale/palette reference for other motions.
                    // A prepared replacement changes this clip alone, with the same cell/PPU contract.
                    sheet = Raster.Load(Within(root, clip.Definition.prebuiltSheet));
                    if (sheet.Width != manifest.cellWidth * outputCount || sheet.Height != manifest.cellHeight)
                        throw new InvalidOperationException("Prepared sheet dimensions mismatch: " + clip.Definition.name);
                    Pixels.Quantize(sheet, palette);
                    headHeights = Enumerable.Range(0, outputCount).Select(frame => manifest.baseline -
                        Pixels.Bounds(sheet, frame * manifest.cellWidth, (frame + 1) * manifest.cellWidth).Top + 1).ToArray();
                }
                var outputBindings = (clip.Definition.bindings ?? new Binding[0]).Select(binding => new Binding { State = binding.State, Clip = binding.Clip,
                    ActiveFrame = clip.Definition.inbetweenFrames > 0 ? (int)Math.Round((double)binding.ActiveFrame * (outputCount - 1) / Math.Max(1, clip.Frames.Length - 1)) : binding.ActiveFrame }).ToArray();
                sheet.Save(Path.Combine(output, clip.Definition.name + ".png"));
                AnimationSmoothing.Preview(sheet, outputCount, clip.Definition.fps, palette,
                        Within(root, "docs/art_wip/" + manifest.character + "_" + clip.Definition.name + "_preview.gif"));
                var metadata = new Metadata { name = clip.Definition.name, frameCount = outputCount, cellWidth = manifest.cellWidth, cellHeight = manifest.cellHeight,
                    fps = clip.Definition.fps, loop = clip.Definition.loop, pivotY = (manifest.cellHeight - 1 - manifest.baseline + (clip.Definition.anchor == "effect" ? manifest.targetHeight * 0.5f : 0)) / manifest.cellHeight,
                    pixelsPerUnit = manifest.pixelsPerUnit, targetHeight = manifest.targetHeight, baseline = manifest.baseline, bindings = outputBindings, headHeights = headHeights };
                WriteJson(Path.Combine(output, clip.Definition.name + ".json"), metadata);
                reports.Add(new { name = clip.Definition.name, frames = details, outputFrames = outputCount,
                    synthesis = !String.IsNullOrEmpty(clip.Definition.prebuiltSheet) ? "single-reference-body-breathing" : clip.Definition.stableIdle ? "single-reference-breathing" : (clip.Definition.sourceClip ?? "").EndsWith("_phase6b") ? "drawn-key-inbetweens" : "clean-key-hold", headHeights = headHeights });
            }
            WriteJson(Path.Combine(output, "forge-report.json"), new { schemaVersion = 1, sourceReferenceHeight = referenceHeight, commonScale = scale,
                keyTolerance = manifest.keyTolerance, palette = palette.Select(c => "#" + (c & 0xffffff).ToString("X6")).ToArray(), clips = reports });
            Verify(manifest, output); Contact(manifest, output, Within(root, manifest.contactPath));
            Console.WriteLine("SpriteForge: " + clips.Count + " sheets / " + clips.Sum(c => AnimationSmoothing.Count(c.Definition)) + " output frames, scale=" + scale.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + ", palette=" + palette.Length);
        }
        private static string Within(string root, string relative)
        {
            string path = Path.GetFullPath(Path.Combine(root, relative));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Path outside workspace: " + relative);
            return path;
        }
        private static void ValidateManifest(Manifest m)
        {
            if (m.animations == null || m.animations.Length == 0 || m.paletteSize < 1 || m.paletteSize > 32 || m.targetHeight < 1 || m.pixelsPerUnit != 50 || m.cellWidth != 224 || m.cellHeight != 160 || m.baseline != 159)
                throw new InvalidOperationException("Invalid manifest settings.");
            if (m.anchorRgbMin == null || m.anchorRgbMax == null || m.anchorRgbMin.Length != 3 || m.anchorRgbMax.Length != 3) throw new InvalidOperationException("Invalid anchor color range.");
            var names = new HashSet<string>();
            foreach (var a in m.animations)
                if (String.IsNullOrWhiteSpace(a.name) || a.name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || a.name.Contains("..") || !names.Add(a.name) || (a.sourceAnchors != null && a.sourceAnchors.Length != a.frames) || a.frames < 1 || a.frames > 8 || a.inbetweenFrames < 0 || a.inbetweenFrames > 8 || a.fps < 1 || (a.anchor != "feet" && a.anchor != "center" && a.anchor != "effect"))
                    throw new InvalidOperationException("Invalid animation in manifest: " + a.name);
        }
        private static void WriteJson(string path, object value) { File.WriteAllText(path, Json.Serialize(value), new System.Text.UTF8Encoding(false)); }
        public static void Verify(Manifest m, string output)
        {
            var palette = new HashSet<int>(); int frames = 0;
            foreach (var animation in m.animations)
            {
                var metadata = Json.Deserialize<Metadata>(File.ReadAllText(Path.Combine(output, animation.name + ".json")));
                var sheet = Raster.Load(Path.Combine(output, animation.name + ".png"));
                int count = AnimationSmoothing.Count(animation);
                if (metadata.frameCount != count || metadata.name != animation.name || metadata.cellWidth != m.cellWidth || metadata.cellHeight != m.cellHeight || metadata.fps != animation.fps || metadata.loop != animation.loop || metadata.pixelsPerUnit != m.pixelsPerUnit || sheet.Width != m.cellWidth * count || sheet.Height != m.cellHeight)
                    throw new InvalidOperationException(animation.name + ": metadata/sheet/manifest mismatch.");
                foreach (int c in sheet.Pixels)
                {
                    int alpha = (int)((uint)c >> 24);
                    if (alpha != 0 && alpha != 255) throw new InvalidOperationException("Non-binary alpha.");
                    if (alpha == 0) continue;
                    if (Pixels.Magenta(c, m.keyTolerance)) throw new InvalidOperationException("Magenta remains.");
                    palette.Add(c);
                }
                for (int i = 0; i < count; i++)
                {
                    Rectangle bounds = Pixels.Bounds(sheet, i * m.cellWidth, (i + 1) * m.cellWidth);
                    if (bounds.IsEmpty) throw new InvalidOperationException(animation.name + ": empty frame.");
                    if (animation.anchor == "feet" && bounds.Bottom - 1 != m.baseline) throw new InvalidOperationException(animation.name + ": foot baseline mismatch.");
                    if (animation.validateHeight && Math.Abs(bounds.Height - m.targetHeight) > 2) throw new InvalidOperationException(animation.name + ": standing height outside tolerance: " + bounds.Height);
                    frames++;
                }
            }
            if (palette.Count > m.paletteSize) throw new InvalidOperationException("Shared palette limit exceeded: " + palette.Count);
            Console.WriteLine("Verify: " + frames + " frames; magenta=0; binary alpha; shared colors=" + palette.Count + "; feet/standing-height OK");
        }
        private static void Contact(Manifest m, string output, string path)
        {
            int width = 220 + 8 * m.cellWidth * 2, rowHeight = m.cellHeight * 2 + 16;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var bitmap = new Bitmap(width, rowHeight * m.animations.Length))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var font = new Font("Consolas", 13))
            {
                graphics.Clear(Color.FromArgb(34, 39, 49)); graphics.InterpolationMode = InterpolationMode.NearestNeighbor; graphics.PixelOffsetMode = PixelOffsetMode.Half;
                for (int row = 0; row < m.animations.Length; row++)
                {
                    var animation = m.animations[row]; int y = row * rowHeight;
                    graphics.DrawString(animation.name + "\n" + AnimationSmoothing.Count(animation) + " frames\n" + animation.fps + "fps", font, Brushes.White, 10, y + 16);
                    for (int i = 0; i < AnimationSmoothing.Count(animation); i++)
                    {
                        int x = 220 + i * m.cellWidth * 2;
                        for (int cy = 0; cy < m.cellHeight * 2; cy += 16) for (int cx = 0; cx < m.cellWidth * 2; cx += 16)
                            graphics.FillRectangle(((cx / 16 + cy / 16) % 2) == 0 ? Brushes.DimGray : Brushes.Gray, x + cx, y + cy, 16, 16);
                    }
                    using (var sheet = new Bitmap(Path.Combine(output, animation.name + ".png"))) graphics.DrawImage(sheet, new Rectangle(220, y, sheet.Width * 2, sheet.Height * 2), new Rectangle(0, 0, sheet.Width, sheet.Height), GraphicsUnit.Pixel);
                }
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                for (int startRow = 0, pageIndex = 1; startRow < m.animations.Length; startRow += 5, pageIndex++)
                {
                    int pageHeight = Math.Min(rowHeight * 5, bitmap.Height - startRow * rowHeight);
                    using (var page = bitmap.Clone(new Rectangle(0, startRow * rowHeight, width, pageHeight), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                        page.Save(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_" + pageIndex.ToString("D2") + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }
        public static void SelfTest()
        {
            var image = new Raster(40, 24); for (int i = 0; i < image.Pixels.Length; i++) image.Pixels[i] = Pixels.Opaque(252, 3, 250);
            for (int y = 3; y < 20; y++) for (int x = 3; x < 10; x++) image.Pixels[y * image.Width + x] = Pixels.Opaque(240, 100, 20);
            for (int y = 5; y < 22; y++) for (int x = 24; x < 33; x++) image.Pixels[y * image.Width + x] = Pixels.Opaque(40, 50, 80);
            image.Pixels[0] = Pixels.Opaque(20, 20, 20); Pixels.Key(image, 90); Pixels.RemoveSpecks(image, 5);
            var frames = Pixels.Split(image, 2); if (frames.Count != 2 || image.Pixels[0] != 0) throw new Exception("split/speck/key self-test failed");
            var shrunk = frames.Select(f => Pixels.ModalShrink(image, f, 0.5)).ToArray(); int[] palette = Pixels.Palette(shrunk, 4);
            foreach (var frame in shrunk) { Pixels.Quantize(frame, palette); foreach (int c in frame.Pixels) if (c != 0 && ((uint)c >> 24) != 255) throw new Exception("alpha self-test failed"); }
            if (shrunk[0].Height != 8 || palette.Length > 4) throw new Exception("scale/palette self-test failed");
            Console.WriteLine("SpriteForge synthetic tests: key, despeckle, split, modal scale, palette, alpha PASS");
        }
    }
}




