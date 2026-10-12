namespace AniBato.SpriteForge
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;
    using System.Collections.Generic;

    public static class PhaseSixBArt
    {
        public static void Prepare(string root)
        {
            var json = new JavaScriptSerializer();
            for (int index = 0; index < 3; index++)
            {
                string name = new[] { "city", "board", "festival" }[index];
                string output = Path.Combine(root, "AniBato/Assets/_Project/Art/Stages"); Directory.CreateDirectory(output);
                using (var source = new Bitmap(Path.Combine(root,"art_source/phase6b/backgrounds/" + name + ".png")))
                {
                    Resize(source,1280,720,Path.Combine(output,"phase6b_" + index + "_background.png"),false);
                    Resize(source,480,270,Path.Combine(output,"phase6b_" + index + "_thumbnail.png"),true);
                }
                string old = Path.Combine(output,"stage_" + index + ".png");
                if (File.Exists(old)) File.Copy(old,Path.Combine(root,"docs/screenshots/phase6b_before_thumbnail_" + name + ".png"),true);
                File.Copy(Path.Combine(output,"phase6b_" + index + "_thumbnail.png"),Path.Combine(root,"docs/screenshots/phase6b_after_thumbnail_" + name + ".png"),true);
            }
            foreach (string name in new[] { "toma", "ren", "chizuru" })
            {
                string raw = Path.Combine(root,"art_source/raw/" + name);
                var atlas = Raster.Load(Path.Combine(root,"art_source/phase6b/motion/" + name + ".png"));
                Pixels.Key(atlas,90); Pixels.RemoveSpecks(atlas,70);
                var poses = PhaseSixArt.Separate(atlas,6);
                if (poses.Length != 6) throw new InvalidOperationException("Incomplete motion atlas.");
                string path = Path.Combine(root,"art_source/" + name + ".manifest.json");
                var manifest = json.Deserialize<Manifest>(File.ReadAllText(path));
                foreach (string clipName in new[] { "walk", "dash" })
                {
                    var old = Raster.Load(Path.Combine(raw,clipName + ".png")); Pixels.Key(old,90); Pixels.RemoveSpecks(old,70);
                    var oldFrames = Pixels.Split(old,12).Select(b => Crop(old,b)).ToList();
                    var newFrames = (clipName == "walk" ? poses.Take(2) : poses.Skip(2)).ToArray();
                    double oldHeight = oldFrames.Select(p => Pixels.Bounds(p,0,p.Width).Height).Average();
                    double newHeight = newFrames.Select(p => Pixels.Bounds(p,0,p.Width).Height).Average();
                    newFrames = newFrames.Select(p => Scale(p,oldHeight / newHeight)).ToArray();
                    var merged = new List<Raster>();
                    if (clipName == "walk")
                    {
                        for (int i = 0; i < oldFrames.Count; i++)
                        { merged.Add(oldFrames[i]); if (i == 1) merged.Add(newFrames[0]); if (i == oldFrames.Count - 2) merged.Add(newFrames[1]); }
                    }
                    else for (int i = 0; i < oldFrames.Count; i++) { merged.Add(oldFrames[i]); merged.Add(newFrames[i]); }
                    int height = merged.Max(p => p.Height) + 40, width = merged.Sum(p => p.Width + 40);
                    var strip = new Raster(width,height); int offset = 20;
                    foreach (var p in merged)
                    { for (int y = 0; y < p.Height; y++) Array.Copy(p.Pixels,y*p.Width,strip.Pixels,(height-20-p.Height+y)*width+offset,p.Width); offset += p.Width+40; }
                    strip.Save(Path.Combine(raw,clipName + "_phase6b.png"));
                    var clip = manifest.animations.Single(a => a.name == clipName);
                    clip.sourceClip = clipName + "_phase6b"; clip.frames = merged.Count;
                    clip.sourceFrames = null; clip.sourceAnchors = null; clip.inbetweenFrames = 8; clip.fps = 15;
                    Console.WriteLine(name + " " + clipName + ": " + oldFrames.Count + " old + " + newFrames.Length + " drawn keys");
                }
                File.WriteAllText(path,json.Serialize(manifest),new System.Text.UTF8Encoding(false));
            }
        }
        private static Raster Crop(Raster source, Rectangle bounds)
        {
            var result = new Raster(bounds.Width,bounds.Height);
            for (int y = 0; y < bounds.Height; y++) Array.Copy(source.Pixels,(bounds.Y+y)*source.Width+bounds.X,result.Pixels,y*result.Width,bounds.Width);
            return result;
        }
        private static Raster Scale(Raster source, double scale)
        {
            var b = Pixels.Bounds(source,0,source.Width); source = Crop(source,b);
            var output = new Raster((int)Math.Round(source.Width*scale),(int)Math.Round(source.Height*scale));
            for (int y=0;y<output.Height;y++) for (int x=0;x<output.Width;x++)
                output.Pixels[y*output.Width+x] = source.Pixels[Math.Min(source.Height-1,(int)(y/scale))*source.Width+Math.Min(source.Width-1,(int)(x/scale))];
            return output;
        }
        private static void Resize(Bitmap source,int width,int height,string path,bool platforms)
        {
            using (var image = new Bitmap(width,height)) using (var g = Graphics.FromImage(image))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; g.DrawImage(source,0,0,width,height);
                if (platforms)
                {
                    using (var floor = new SolidBrush(Color.FromArgb(255,65,76,83)))
                    using (var edge = new SolidBrush(Color.FromArgb(255,188,203,209)))
                    { g.FillRectangle(floor,65,222,350,12); g.FillRectangle(edge,65,222,350,3); g.FillRectangle(edge,120,183,68,4); g.FillRectangle(edge,292,183,68,4); }
                }
                image.Save(path,ImageFormat.Png);
            }
        }
    }
}
