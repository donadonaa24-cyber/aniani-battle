namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;

    public static class PhaseSixArt
    {
        private static readonly string[] Clips = { "crouch", "crouch_attack", "back_attack", "air_neutral", "air_forward", "air_back", "air_up", "air_down", "air_forward_up" };
        public static void Prepare(string root)
        {
            var json = new JavaScriptSerializer();
            string[] characters = { "toma", "ren", "chizuru" };
            var diagonal = Raster.Load(Path.Combine(root, "art_source/phase6a/diagonal-down.png")); Pixels.Key(diagonal, 90); Pixels.RemoveSpecks(diagonal, 70);
            var downBounds = Pixels.Split(diagonal, 12);
            if (downBounds.Count != 3) throw new InvalidOperationException("Expected three diagonal-down poses.");
            for (int character = 0; character < 3; character++)
            {
                string name = characters[character], raw = Path.Combine(root, "art_source/raw/" + name);
                var atlas = Raster.Load(Path.Combine(root, "art_source/phase6a/" + name + "-atlas.png")); Pixels.Key(atlas, 90); Pixels.RemoveSpecks(atlas, 70);
                var poses = Separate(atlas);
                for (int i = 0; i < Clips.Length; i++) poses[i].Save(Path.Combine(raw, Clips[i] + ".png"));
                Crop(diagonal, downBounds[character]).Save(Path.Combine(raw, "air_forward_down.png"));
                string manifestPath = Path.Combine(root, "art_source/" + name + ".manifest.json");
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(manifestPath));
                var idle = Raster.Load(Path.Combine(raw, "idle.png")); Pixels.Key(idle, 90); Pixels.RemoveSpecks(idle, 70);
                var idleBounds = Pixels.Split(idle, 12);
                double referenceHeight = idleBounds.Select(b => b.Height).OrderBy(h => h).ElementAt(idleBounds.Count / 2);
                double poseScale = referenceHeight / Pixels.Bounds(poses[2], 0, poses[2].Width).Height;
                var animations = manifest.animations.ToList();
                foreach (var clip in animations)
                {
                    if (clip.name == "win" || clip.name == "lose")
                    { clip.sourceClip = "idle"; clip.sourceFrames = new[] { 0 }; clip.frames = 1; clip.inbetweenFrames = 8; clip.stableIdle = true; clip.loop = true; clip.fps = 12; clip.anchor = "feet"; clip.sourceAnchors = null; clip.headOffset = clip.name == "win" ? -1 : 1; }
                }
                for (int i = 0; i < 10; i++)
                {
                    string clipName = i < 9 ? Clips[i] : "air_forward_down";
                    var clip = animations.FirstOrDefault(c => c.name == clipName);
                    if (clip == null) { clip = new Animation { name = clipName }; animations.Add(clip); }
                    clip.frames = 1; clip.fps = 12; clip.inbetweenFrames = i == 0 ? 8 : 6;
                    clip.loop = i == 0; clip.stableIdle = i == 0; clip.anchor = i < 3 ? "feet" : "center";
                    clip.sourceScaleFactor = i < 9 ? Math.Min(poseScale, referenceHeight / Pixels.Bounds(poses[i], 0, poses[i].Width).Height) : referenceHeight / downBounds[character].Height;
                    clip.bindings = new[] { new Binding { State = 24 + i, Clip = clipName, ActiveFrame = 0 } };
                }
                manifest.animations = animations.ToArray();
                File.WriteAllText(manifestPath, json.Serialize(manifest), new System.Text.UTF8Encoding(false));
                Console.WriteLine(name + ": nine connected clean drawings; source scale " + poseScale);
            }
            var previews = new System.Text.StringBuilder("# フェーズ6A アニメプレビュー\n\n全106クリップ。新規方向10種/キャラ、Win/Lose、既存14主要クリップの修正を含む。旧コマの重ね合わせなし。新規攻撃は描き下ろした綺麗なキーを保持して再生。\n\n| キャラ | クリップ | GIF |\n|---|---|---|\n");
            foreach (string name in characters)
            {
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(Path.Combine(root, "art_source/" + name + ".manifest.json")));
                foreach (var clip in manifest.animations) previews.AppendLine("| " + name + " | " + clip.name + " | [preview](" + name + "_" + clip.name + "_preview.gif) |");
            }
            File.WriteAllText(Path.Combine(root, "docs/art_wip/phase6a_previews.md"), previews.ToString(), new System.Text.UTF8Encoding(false));
        }
        private static Raster Crop(Raster source, Rectangle bounds)
        {
            var result = new Raster(bounds.Width + 40, bounds.Height + 40);
            for (int y = 0; y < bounds.Height; y++) Array.Copy(source.Pixels, (bounds.Y + y) * source.Width + bounds.X, result.Pixels, (y + 20) * result.Width + 20, bounds.Width);
            return result;
        }
        // Detect connected opaque figures, rather than cutting limbs at nominal grid borders.
        internal static Raster[] Separate(Raster atlas, int count = 9)
        {
            var visited = new bool[atlas.Pixels.Length]; var queue = new Queue<int>(); var islands = new List<List<int>>();
            for (int start = 0; start < visited.Length; start++)
            {
                if (visited[start] || atlas.Pixels[start] == 0) continue;
                var island = new List<int>(); visited[start] = true; queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int p = queue.Dequeue(); island.Add(p); int x = p % atlas.Width, y = p / atlas.Width;
                    for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy; if (nx < 0 || ny < 0 || nx >= atlas.Width || ny >= atlas.Height) continue;
                        int next = ny * atlas.Width + nx; if (!visited[next] && atlas.Pixels[next] != 0) { visited[next] = true; queue.Enqueue(next); }
                    }
                }
                islands.Add(island);
            }
            var primary = islands.OrderByDescending(i => i.Count).Take(count).ToArray();
            if (primary.Length != count || primary.Any(i => i.Count < 2000)) throw new InvalidOperationException("Atlas must contain separate complete figures: " + count);
            var centers = primary.Select(i => new Point((int)i.Average(p => p % atlas.Width), (int)i.Average(p => p / atlas.Width))).ToArray();
            var groups = Enumerable.Range(0, count).Select(i => new Raster(atlas.Width, atlas.Height)).ToArray();
            foreach (var island in islands)
            {
                double x = island.Average(p => p % atlas.Width), y = island.Average(p => p / atlas.Width);
                int nearest = Enumerable.Range(0, count).OrderBy(i => (centers[i].X - x) * (centers[i].X - x) + (centers[i].Y - y) * (centers[i].Y - y)).First();
                foreach (int pixel in island) groups[nearest].Pixels[pixel] = atlas.Pixels[pixel];
            }
            var slots = new Raster[count];
            int columns = count == 6 ? 2 : 3;
            for (int i = 0; i < count; i++)
            {
                int column = Math.Min(columns - 1, centers[i].X * columns / atlas.Width), row = Math.Min(2, centers[i].Y * 3 / atlas.Height), slot = row * columns + column;
                if (slots[slot] != null) throw new InvalidOperationException("Ambiguous atlas pose placement.");
                slots[slot] = Crop(groups[i], Pixels.Bounds(groups[i], 0, atlas.Width));
            }
            if (slots.Any(p => p == null)) throw new InvalidOperationException("Missing atlas cell."); return slots;
        }
    }
}
