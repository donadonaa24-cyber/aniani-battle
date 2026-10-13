namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    public static class TomaActionAssets
    {
        private const string Folder = "art_source/toma_actions/";
        private sealed class PaletteReport { public string[] palette = new string[0]; }
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
        private static int[] Palette(string root)
        {
            return Json.Deserialize<PaletteReport>(File.ReadAllText(Path.Combine(root,
                "AniBato/Assets/_Project/Art/Sprites/toma/forge-report.json"))).palette.Select(p => unchecked((int)0xff000000) | Convert.ToInt32(p.Substring(1), 16)).ToArray();
        }
        private static Manifest ReadManifest(string root)
        { return Json.Deserialize<Manifest>(File.ReadAllText(Path.Combine(root, "art_source/toma.manifest.json"))); }
        private static void WriteManifest(string root, Manifest manifest)
        { FileOutputs.WriteAllText(Path.Combine(root, "art_source/toma.manifest.json"), Json.Serialize(manifest), new System.Text.UTF8Encoding(false)); }
        public static void PrepareBody(string root, string group, string clipName, bool newClip = false, int newFrames = 3)
        {
            var selection = Json.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(root, Folder + "source-selection.json")));
            if (!selection.ContainsKey("toma/" + group)) throw new InvalidOperationException("Motion has not passed normalization: " + group);
            var manifest = ReadManifest(root);
            var clip = manifest.animations.FirstOrDefault(a => a.name == clipName);
            if (clip == null)
            {
                if (!newClip) throw new InvalidOperationException("Unknown existing body clip: " + clipName);
                clip = new Animation { name = clipName, anchor = "feet", frames = newFrames, fps = 15, loop = false,
                    prebuiltOnly = true, bindings = new Binding[0] };
                manifest.animations = manifest.animations.Concat(new[] { clip }).ToArray();
            }
            int count = AnimationSmoothing.Count(clip);
            int peak = clip.bindings.Length == 0 ? -1 : clip.bindings[0].ActiveFrame;
            if (peak >= 0 && clip.inbetweenFrames > 0) peak = (int)Math.Round((double)peak * (count - 1) / Math.Max(1, clip.frames - 1));
            var sheet = new Raster(224 * count, 160); var order = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int key = peak >= 0 ? i < peak ? 1 : i == peak ? 2 : 3 : Math.Min(3, 1 + i * 3 / count);
                var cell = Raster.Load(Path.Combine(root, Folder + "normalized/toma/" + group + "_key_" + key.ToString("D2") + ".png"));
                if (cell.Width != 224 || cell.Height != 160) throw new InvalidOperationException("Invalid normalized body cell.");
                for (int y = 0; y < 160; y++) Array.Copy(cell.Pixels, y * 224, sheet.Pixels, y * sheet.Width + i * 224, 224);
                order.Add(key);
            }
            clip.prebuiltSheet = Folder + "normalized/toma/" + clipName + "_sheet.png";
            sheet.Save(Path.Combine(root, clip.prebuiltSheet)); WriteManifest(root, manifest);
            FileOutputs.WriteAllText(Path.Combine(root, Folder + "normalized/" + clipName + "-order.json"),
                Json.Serialize(new { clip = clipName, sourceGroup = group, count, peak, keys = order, body = true }), new System.Text.UTF8Encoding(false));
        }
        public static void PrepareProp(string root, string group, string sourcePath, int firstWidth, int fps = 15)
        {
            if (!sourcePath.StartsWith(Folder + "generated/", StringComparison.Ordinal) || sourcePath.Contains("..") || sourcePath.Contains(":"))
                throw new InvalidOperationException("Prop source must remain in the Toma generated directory.");
            var source = Raster.Load(Path.Combine(root, sourcePath)); Pixels.Key(source, 90);
            var parts = AtlasPoseSplitter.Split(source, 70);
            if (parts.Count != 4) throw new InvalidOperationException("Prop needs four isolated keys, got " + parts.Count);
            if (parts.Any(p => p.SourceBounds.Left < 2 || p.SourceBounds.Top < 2 || p.SourceBounds.Right > source.Width - 2 || p.SourceBounds.Bottom > source.Height - 2))
                throw new InvalidOperationException("Prop touches source border.");
            double scale = firstWidth / (double)parts[0].SourceBounds.Width;
            var palette = Palette(root); var sheet = new Raster(896, 160); var rows = new List<object>();
            for (int i = 0; i < 4; i++)
            {
                var frame = Pixels.ModalShrink(parts[i].Image, new Rectangle(0, 0, parts[i].Image.Width, parts[i].Image.Height), scale);
                Pixels.Quantize(frame, palette); var bounds = Pixels.Bounds(frame, 0, frame.Width);
                if (Math.Abs(bounds.Width - firstWidth) > 2 || bounds.Height < 3 || bounds.Height > 40)
                    throw new InvalidOperationException("Prop changed physical scale: " + i + " " + bounds.Size);
                int dx = 112 - (int)Math.Round((bounds.Left + bounds.Right - 1) * .5);
                int dy = 98 - (int)Math.Round((bounds.Top + bounds.Bottom - 1) * .5);
                for (int y = 0; y < frame.Height; y++) for (int x = 0; x < frame.Width; x++)
                {
                    int color = frame.Pixels[y * frame.Width + x]; if (color == 0) continue;
                    if (x + dx < 2 || x + dx >= 222 || y + dy < 3 || y + dy > 159) throw new InvalidOperationException("Prop clips fixed cell.");
                    sheet.Pixels[(y + dy) * 896 + i * 224 + x + dx] = color;
                }
                rows.Add(new { index = i, sourceBounds = parts[i].SourceBounds, commonScale = scale, width = bounds.Width, height = bounds.Height, xOffset = dx, yOffset = dy });
            }
            string prepared = Folder + "normalized/toma/" + group + "_sheet.png"; sheet.Save(Path.Combine(root, prepared));
            FileOutputs.WriteAllText(Path.Combine(root, Folder + "normalized/" + group + "-scale.json"),
                Json.Serialize(new { source = sourcePath, firstWidth, commonScale = scale, independentPoseResize = false, keys = rows }), new System.Text.UTF8Encoding(false));
            var selection = Json.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(root, Folder + "source-selection.json")));
            selection["toma/" + group] = sourcePath;
            FileOutputs.WriteAllText(Path.Combine(root, Folder + "source-selection.json"), Json.Serialize(selection), new System.Text.UTF8Encoding(false));
            var manifest = ReadManifest(root); var clip = manifest.animations.FirstOrDefault(a => a.name == group);
            if (clip == null)
            {
                clip = new Animation { name = group, anchor = "effect", frames = 4, fps = fps, loop = true, bindings = new Binding[0] };
                manifest.animations = manifest.animations.Concat(new[] { clip }).ToArray();
            }
            clip.prebuiltOnly = true; clip.prebuiltSheet = prepared; WriteManifest(root, manifest);
        }
        public static void PublishVerify(string root, string clipName)
        {
            var manifest = ReadManifest(root); var clip = manifest.animations.Single(a => a.name == clipName);
            int count = AnimationSmoothing.Count(clip); string output = Path.Combine(root, manifest.outputDirectory + "/");
            var sheet = Raster.Load(output + clipName + ".png"); var prepared = Raster.Load(Path.Combine(root, clip.prebuiltSheet));
            var metadata = Json.Deserialize<Metadata>(File.ReadAllText(output + clipName + ".json"));
            if (sheet.Width != 224 * count || sheet.Height != 160 || !sheet.Pixels.SequenceEqual(prepared.Pixels) ||
                metadata.cellWidth != 224 || metadata.cellHeight != 160 || metadata.pixelsPerUnit != 50 || metadata.frameCount != count ||
                metadata.fps != clip.fps || metadata.loop != clip.loop || metadata.baseline != 159 || metadata.pivotX != .5f)
                throw new InvalidOperationException("Prepared/output art or contract changed: " + clipName);
            var palette = new HashSet<int>(Palette(root));
            foreach (int color in sheet.Pixels) if (color != 0 && ((uint)color >> 24 != 255 || !palette.Contains(color)))
                throw new InvalidOperationException("New colors or nonbinary alpha: " + clipName);
            string ready = Path.Combine(root, Folder + "ready/toma/"); Directory.CreateDirectory(ready);
            FileOutputs.WriteAllText(ready + clipName + ".json", File.ReadAllText(output + clipName + ".json"), new System.Text.UTF8Encoding(false));
            sheet.Save(ready + clipName + "_sheet.png");
            for (int i = 0; i < count; i++)
            {
                var cell = new Raster(224, 160); for (int y = 0; y < 160; y++) Array.Copy(sheet.Pixels, y * sheet.Width + i * 224, cell.Pixels, y * 224, 224);
                cell.Save(ready + clipName + "_" + i.ToString("D2") + ".png");
            }
            var enlarged = new Raster(sheet.Width * 3, 480);
            for (int y = 0; y < enlarged.Height; y++) for (int x = 0; x < enlarged.Width; x++)
                enlarged.Pixels[y * enlarged.Width + x] = sheet.Pixels[(y / 3) * sheet.Width + x / 3];
            AnimationSmoothing.Preview(enlarged, count, metadata.fps, palette.ToArray(), Path.Combine(root, "docs/art_wip/toma_actions_" + clipName + ".gif"));
            Console.WriteLine(clipName + ": exact prepared pixels, original palette, fixed 224x160/PPU50 and ready frames PASS.");
        }
    }
}
