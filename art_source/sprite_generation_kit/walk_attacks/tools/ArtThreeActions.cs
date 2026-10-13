namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    public static class ArtThreeActions
    {
        private const string Folder = "art_source/art3_actions/";
        private const int Width = 224, Height = 160;
        private static readonly string[] Clips = { "jump", "fall", "land", "guard", "dodge", "dash" };
        private static readonly int[][] Keys = { new[] { 1, 1, 1, 2, 2, 2 }, new[] { 3, 3, 3, 3, 3, 3 },
            new[] { 1, -1 }, new[] { 1, 1, 1, 1, 1, 1 }, new[] { 1, 2, 3 }, new[] { 1, 1, 2, 2, 3, 3, 4, 4 } };
        private static string Group(string clip) { return clip == "fall" ? "jump" : clip == "land" ? "jump_start" : clip; }
        private static Raster Key(string root, string name, string clip, int index, bool useCurrentAdoption = false)
        {
            if (useCurrentAdoption && clip == "dash")
            {
                var manifest = new JavaScriptSerializer().Deserialize<Manifest>(File.ReadAllText(Path.Combine(root, "art_source/" + name + ".manifest.json")));
                if ((manifest.animations.Single(a => a.name == clip).prebuiltSheet ?? "").StartsWith("art_source/art3_walk_attacks/", StringComparison.Ordinal))
                    return Raster.Load(Path.Combine(root, "art_source/art3_walk_attacks/normalized/" + name + "/dash_key_" + index.ToString("D2") + ".png"));
            }
            return Raster.Load(Path.Combine(root, index < 0 ? "art_source/art3_idle/breathing/ready/" + name + "/idle_00.png" :
                Folder + "normalized/" + name + "/" + Group(clip) + "_key_" + index.ToString("D2") + ".png"));
        }
        public static void Prepare(string root)
        {
            foreach (string group in ActionAtlasBuilder.Groups) ActionAtlasBuilder.NormalizeGroup(root, group);
            var json = new JavaScriptSerializer();
            foreach (string name in ActionAtlasBuilder.Characters)
            {
                string path = Path.Combine(root, "art_source/" + name + ".manifest.json");
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(path));
                for (int c = 0; c < Clips.Length; c++)
                {
                    string clipName = Clips[c]; var clip = manifest.animations.Single(a => a.name == clipName);
                    if (AnimationSmoothing.Count(clip) != Keys[c].Length) throw new InvalidOperationException("Action frame count changed.");
                    var sheet = new Raster(Width * Keys[c].Length, Height);
                    for (int i = 0; i < Keys[c].Length; i++)
                    {
                        var cell = Key(root, name, clipName, Keys[c][i]);
                        for (int y = 0; y < Height; y++) Array.Copy(cell.Pixels, y * Width, sheet.Pixels, y * sheet.Width + i * Width, Width);
                    }
                    clip.prebuiltSheet = Folder + "normalized/" + name + "/" + clipName + "_sheet.png";
                    sheet.Save(Path.Combine(root, clip.prebuiltSheet));
                    if (clipName == "land" && !clip.bindings.Any(b => b.State == 34))
                        clip.bindings = clip.bindings.Concat(new[] { new Binding { State = 34, Clip = "land", ActiveFrame = 0 } }).ToArray();
                }
                File.WriteAllText(path, json.Serialize(manifest), new System.Text.UTF8Encoding(false));
            }
        }
        public static void Publish(string root)
        {
            foreach (string name in ActionAtlasBuilder.Characters) foreach (string clip in Clips)
            {
                string game = Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + name + "/");
                string ready = Path.Combine(root, Folder + "ready/" + name + "/"); Directory.CreateDirectory(ready);
                var sheet = Raster.Load(game + clip + ".png"); int count = sheet.Width / Width;
                File.Copy(game + clip + ".png", ready + clip + "_sheet.png", true);
                File.Copy(game + clip + ".json", ready + clip + ".json", true);
                for (int frame = 0; frame < count; frame++)
                {
                    var cell = new Raster(Width, Height);
                    for (int y = 0; y < Height; y++) Array.Copy(sheet.Pixels, y * sheet.Width + frame * Width, cell.Pixels, y * Width, Width);
                    cell.Save(ready + clip + "_" + frame.ToString("D2") + ".png");
                }
                File.Copy(Path.Combine(root, "docs/art_wip/" + name + "_" + clip + "_preview.gif"), ready + clip + "_preview.gif", true);
                var metadata = new JavaScriptSerializer().Deserialize<Metadata>(File.ReadAllText(game + clip + ".json"));
                var enlarged = new Raster(sheet.Width * 3, sheet.Height * 3);
                for (int y = 0; y < enlarged.Height; y++) for (int x = 0; x < enlarged.Width; x++)
                    enlarged.Pixels[y * enlarged.Width + x] = sheet.Pixels[(y / 3) * sheet.Width + x / 3];
                AnimationSmoothing.Preview(enlarged, count, metadata.fps, sheet.Pixels.Where(p => p != 0).Distinct().OrderBy(p => p).ToArray(),
                    Path.Combine(root, "docs/art_wip/art3d_" + name + "_" + clip + "_preview.gif"));
            }
            using (var board = new Bitmap(Width * 9, Height * 3 * 7))
            using (var g = Graphics.FromImage(board))
            {
                g.Clear(Color.FromArgb(34, 39, 49)); g.InterpolationMode = InterpolationMode.NearestNeighbor; g.PixelOffsetMode = PixelOffsetMode.Half;
                for (int col = 0; col < 3; col++) for (int row = 0; row < 7; row++)
                {
                    string key = ActionAtlasBuilder.Characters[col];
                    string file = row == 0 ? "art_source/art3_idle/breathing/ready/" + key + "/idle_00.png" :
                        Folder + "ready/" + key + "/" + Clips[row - 1] + "_" + (row == 5 ? "01" : "00") + ".png";
                    using (var image = new Bitmap(Path.Combine(root, file)))
                        g.DrawImage(image, new Rectangle(col * Width * 3, row * Height * 3, Width * 3, Height * 3), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);
                }
                board.Save(Path.Combine(root, "docs/art_wip/art3d_pose_comparison.png"));
            }
        }
        public static void Verify(string root)
        {
            var json = new JavaScriptSerializer(); var reports = new List<object>();
            int[] fps = { 15, 15, 15, 15, 12, 15 }; bool[] loops = { false, true, false, true, false, true };
            foreach (string name in ActionAtlasBuilder.Characters) for (int c = 0; c < Clips.Length; c++)
            {
                string clip = Clips[c], game = Path.Combine(root, "AniBato/Assets/_Project/Art/Sprites/" + name + "/");
                var sheet = Raster.Load(game + clip + ".png"); var data = json.Deserialize<Metadata>(File.ReadAllText(game + clip + ".json"));
                if (sheet.Width != Width * Keys[c].Length || sheet.Height != Height || data.frameCount != Keys[c].Length ||
                    data.fps != fps[c] || data.loop != loops[c] || data.baseline != 159 || data.pixelsPerUnit != 50 || data.pivotX != .5f || data.pivotY != 0)
                    throw new InvalidOperationException("Action metadata contract failed: " + name + "/" + clip);
                var colors = new HashSet<int>();
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(Path.Combine(root, "art_source/" + name + ".manifest.json")));
                bool currentRun = clip == "dash" && (manifest.animations.Single(a => a.name == clip).prebuiltSheet ?? "").StartsWith("art_source/art3_walk_attacks/", StringComparison.Ordinal);
                for (int f = 0; f < Keys[c].Length; f++)
                {
                    var cell = Key(root, name, clip, Keys[c][f], true); var bounds = Pixels.Bounds(cell, 0, Width);
                    bool flight = currentRun && (Keys[c][f] == 2 || Keys[c][f] == 4);
                    if (data.headHeights[f] != Height - bounds.Top || bounds.Left < 2 || bounds.Right > Width - 2 || bounds.Top < 3 ||
                        (clip != "jump" && clip != "fall" && !flight && bounds.Bottom != Height)) throw new InvalidOperationException("Action anchor or margin failed.");
                    if (flight) ActionAtlasBuilder.VerifyRunFlight(bounds, manifest.targetHeight);
                    for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
                    {
                        int color = sheet.Pixels[y * sheet.Width + f * Width + x];
                        if (color != cell.Pixels[y * Width + x]) throw new InvalidOperationException("Action mixes key pixels or rescales a pose.");
                        if (color == 0) continue;
                        if (((uint)color >> 24) != 255) throw new InvalidOperationException("Non-binary action alpha."); colors.Add(color);
                    }
                }
                if (colors.Count > 32) throw new InvalidOperationException("Action palette failed.");
                using (var gif = Image.FromFile(Path.Combine(root, "docs/art_wip/" + name + "_" + clip + "_preview.gif")))
                {
                    if (gif.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) != Keys[c].Length) throw new InvalidOperationException("Action GIF frame count failed.");
                    for (int i = 0; i < Keys[c].Length; i++) gif.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, i);
                }
                reports.Add(new { character = name, clip = clip, frames = Keys[c].Length, fps = data.fps, loop = data.loop,
                    cellWidth = Width, cellHeight = Height, drawnKeyIndices = Keys[c], exactKeyPixels = true, colors = colors.Count, headHeights = data.headHeights });
            }
            File.WriteAllText(Path.Combine(root, Folder + "verification.json"), json.Serialize(reports), new System.Text.UTF8Encoding(false));
            Console.WriteLine("ART3D: 18 clips / 93 frames; fixed cells, anchors, exact key pixels, binary alpha, palette, GIF decode PASS.");
        }
    }
}
