namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    public static class ActionAtlasBuilder
    {
        public const int Width = 224, Height = 160, Baseline = 159;
        public static readonly string[] Characters = { "toma", "ren", "chizuru" };
        public static readonly string[] Groups = { "jump", "jump_start", "guard", "dodge", "dash" };
        private sealed class PaletteReport { public string[] palette = new string[0]; }
        public static void NormalizeGroup(string root, string group)
        {
            int expected = group == "jump" || group == "dodge" ? 4 : group == "dash" ? 5 : 2;
            if (!Groups.Contains(group)) throw new ArgumentException("Unknown action atlas group.");
            var json = new JavaScriptSerializer(); var reports = new List<object>();
            string selectionPath = Path.Combine(root, "art_source/art3_actions/source-selection.json");
            var selection = File.Exists(selectionPath) ? json.Deserialize<Dictionary<string, string>>(File.ReadAllText(selectionPath)) : new Dictionary<string, string>();
            foreach (string key in Characters)
            {
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(Path.Combine(root, "art_source/" + key + ".manifest.json")));
                string sourcePath = "art_source/art3_actions/generated/" + key + "_" + group + "_keys_v1.png";
                string selected;
                if (selection.TryGetValue(key + "/" + group, out selected)) sourcePath = selected;
                if (!sourcePath.StartsWith("art_source/art3_actions/generated/", StringComparison.Ordinal) || sourcePath.Contains("..") || sourcePath.Contains(":"))
                    throw new InvalidOperationException("Action source must stay in its generated asset directory.");
                var source = Raster.Load(Path.Combine(root, sourcePath)); Pixels.Key(source, manifest.keyTolerance);
                var parts = AtlasPoseSplitter.Split(source, 70);
                if (parts.Count != expected) throw new InvalidOperationException(key + "/" + group + ": expected " + expected + " separated figures, got " + parts.Count);
                if (parts.Any(p => p.SourceBounds.Left < 2 || p.SourceBounds.Top < 2 || p.SourceBounds.Right > source.Width - 2 || p.SourceBounds.Bottom > source.Height - 2))
                    throw new InvalidOperationException(key + "/" + group + ": source touches its image border; regenerate with full silhouettes and margins.");
                double scale = manifest.targetHeight / (double)parts[0].SourceBounds.Height;
                var frames = parts.Select(p => Pixels.ModalShrink(p.Image, new Rectangle(0, 0, p.Image.Width, p.Image.Height), scale)).ToArray();
                var palette = json.Deserialize<PaletteReport>(File.ReadAllText(Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + key + "/forge-report.json"))).palette
                    .Select(c => unchecked((int)0xff000000) | int.Parse(c.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToArray();
                // Keep one common center anchor for the airborne group; reserve a bottom margin for its tallest key.
                double airCenter = Math.Min(Baseline - manifest.targetHeight * .5, Baseline - 2 - (frames.Skip(1).Max(f => f.Height) - 1) * .5);
                var details = new List<object>();
                for (int i = 0; i < parts.Count; i++)
                {
                    var frame = frames[i]; Pixels.Quantize(frame, palette);
                    var bounds = Pixels.Bounds(frame, 0, frame.Width);
                    double anchor = Pixels.TorsoAnchor(parts[i].Image, new Rectangle(0, 0, parts[i].Image.Width, parts[i].Image.Height), manifest.anchorRgbMin, manifest.anchorRgbMax, manifest.anchorRedGreenRatio);
                    int dx = Width / 2 - (int)Math.Round(anchor * scale);
                    int dy = group == "jump" && i > 0 ? (int)Math.Round(airCenter - (bounds.Top + bounds.Bottom - 1) * .5) : Baseline - bounds.Bottom + 1;
                    var cell = new Raster(Width, Height);
                    for (int y = 0; y < frame.Height; y++) for (int x = 0; x < frame.Width; x++)
                    {
                        int color = frame.Pixels[y * frame.Width + x]; if (color == 0) continue;
                        int ox = x + dx, oy = y + dy;
                        if (ox < 2 || ox >= Width - 2 || oy < 3 || oy > Baseline)
                            throw new InvalidOperationException(key + "/" + group + ": key clips its fixed canvas.");
                        cell.Pixels[oy * Width + ox] = color;
                    }
                    var placed = Pixels.Bounds(cell, 0, Width);
                    int depth = key == "toma" ? 25 : key == "ren" ? 31 : 34;
                    int referenceWidth = key == "toma" ? 37 : key == "ren" ? 58 : 43;
                    int left = Width, right = -1;
                    for (int y = placed.Top; y < Math.Min(Height, placed.Top + depth); y++) for (int x = 0; x < Width; x++)
                        if (cell.Pixels[y * Width + x] != 0) { left = Math.Min(left, x); right = Math.Max(right, x); }
                    int headWidth = right - left + 1;
                    if (Math.Abs(headWidth - referenceWidth) > 2)
                        throw new InvalidOperationException(key + "/" + group + "/" + i + ": head band " + headWidth + " differs from approved " + referenceWidth + ". Review the source pose, do not change the scale independently.");
                    if (group == "jump_start" && i > 0 && (placed.Height < manifest.targetHeight * .82 || placed.Height > manifest.targetHeight * .95))
                        throw new InvalidOperationException("Jump anticipation must be a shallow grounded bend.");
                    cell.Save(Path.Combine(root, "art_source/art3_actions/normalized/" + key + "/" + group + "_key_" + i.ToString("D2") + ".png"));
                    details.Add(new { index = i, sourceBounds = new[] { parts[i].SourceBounds.X, parts[i].SourceBounds.Y, parts[i].SourceBounds.Width, parts[i].SourceBounds.Height },
                        componentArea = parts[i].Area, sameStandingScale = scale, xOffset = dx, yOffset = dy, pixelHeight = placed.Height,
                        headBandDepth = depth, headBandWidth = headWidth, approvedHeadBandWidth = referenceWidth, headWidthDelta = headWidth - referenceWidth });
                }
                reports.Add(new { character = key, group = group, source = sourcePath, sourceWidth = source.Width, sourceHeight = source.Height,
                    detectedFigures = parts.Count, standingReferenceHeight = manifest.targetHeight, commonScale = scale, independentPoseResize = false,
                    extraction = "separated-connected-figure-masks", airCenter = group == "jump" ? airCenter : 0, keys = details });
                Console.WriteLine(key + "/" + group + ": " + parts.Count + " fixed-size keys, standing ruler and head band PASS.");
            }
            string reportPath = Path.Combine(root, "art_source/art3_actions/normalized/" + group + "-scale.json");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)); File.WriteAllText(reportPath, json.Serialize(reports), new System.Text.UTF8Encoding(false));
        }
    }
}
