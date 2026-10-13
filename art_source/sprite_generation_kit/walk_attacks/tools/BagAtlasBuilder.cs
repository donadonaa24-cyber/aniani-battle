namespace AniBato.SpriteForge
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;
    public static class BagAtlasBuilder
    {
        private const string Folder = "art_source/art3_walk_attacks/";
        private sealed class PaletteReport { public string[] palette = new string[0]; }
        public static void Prepare(string root)
        {
            var json = new JavaScriptSerializer();
            var selection = json.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(root, Folder+"source-selection.json")));
            string sourcePath;
            if (!selection.TryGetValue("chizuru/bag_throw",out sourcePath) || sourcePath == null) throw new InvalidOperationException("Bag source not selected.");
            var source = Raster.Load(Path.Combine(root,sourcePath)); Pixels.Key(source,90);
            var parts = AtlasPoseSplitter.Split(source,70);
            if(parts.Count != 4) throw new InvalidOperationException("Bag requires four isolated rotations.");
            double scale = 20.0 / parts[0].SourceBounds.Width;
            var palette = json.Deserialize<PaletteReport>(File.ReadAllText(Path.Combine(root,"AniBato/Assets/_Project/Art/Sprites/chizuru/forge-report.json")))
                .palette.Select(p=>unchecked((int)0xff000000)|Convert.ToInt32(p.Substring(1),16)).ToArray();
            var sheet = new Raster(224*4,160); var rows = new List<object>(); var sizes = new List<Size>();
            for(int i=0;i<4;i++)
            {
                var frame = Pixels.ModalShrink(parts[i].Image,new Rectangle(0,0,parts[i].Image.Width,parts[i].Image.Height),scale);
                Pixels.Quantize(frame,palette);
                var b=Pixels.Bounds(frame,0,frame.Width); sizes.Add(b.Size);
                int shortSide=i%2==0?b.Width:b.Height, longSide=i%2==0?b.Height:b.Width;
                if(shortSide<18 || shortSide>22 || longSide<19 || longSide>28)
                    throw new InvalidOperationException("Bag rotation changed physical scale: "+i+" "+b.Size);
                int dx=112-(int)Math.Round((b.Left+b.Right-1)*.5),dy=103-(int)Math.Round((b.Top+b.Bottom-1)*.5);
                for(int y=0;y<frame.Height;y++)for(int x=0;x<frame.Width;x++)
                {
                    int c=frame.Pixels[y*frame.Width+x];if(c==0)continue;
                    if(x+dx<2 || x+dx>221 || y+dy<3 || y+dy>159)throw new InvalidOperationException("Bag outside fixed cell.");
                    sheet.Pixels[(y+dy)*sheet.Width+i*224+x+dx]=c;
                }
                rows.Add(new {index=i,sourceBounds=parts[i].SourceBounds,commonScale=scale,width=b.Width,height=b.Height,xOffset=dx,yOffset=dy});
            }
            for(int i=0;i<2;i++)if(Math.Abs(sizes[i].Width-sizes[i+2].Width)>2 || Math.Abs(sizes[i].Height-sizes[i+2].Height)>2)
                throw new InvalidOperationException("Opposite rotations changed bag size.");
            string prepared=Folder+"normalized/chizuru/bag_throw_sheet.png";sheet.Save(Path.Combine(root,prepared));
            FileOutputs.WriteAllText(Path.Combine(root,Folder+"normalized/chizuru-bag_throw-scale.json"),
                json.Serialize(new {source=sourcePath,standingReference=false,bodyWidth=20,commonScale=scale,independentPoseResize=false,keys=rows}),new System.Text.UTF8Encoding(false));
            string path=Path.Combine(root,"art_source/chizuru.manifest.json");var manifest=json.Deserialize<Manifest>(File.ReadAllText(path));
            var clip=manifest.animations.FirstOrDefault(a=>a.name=="bag_throw");
            if(clip==null){clip=new Animation {name="bag_throw",anchor="effect",frames=4,fps=12,loop=true,bindings=new Binding[0]};manifest.animations=manifest.animations.Concat(new[]{clip}).ToArray();}
            clip.prebuiltOnly=true;clip.prebuiltSheet=prepared;
            FileOutputs.WriteAllText(path,json.Serialize(manifest),new System.Text.UTF8Encoding(false));
            Console.WriteLine("Bag: four rotations, one common scale and original palette PASS.");
        }
        public static void PublishVerify(string root)
        {
            string game=Path.Combine(root,"AniBato/Assets/_Project/Art/Sprites/chizuru/");
            string ready=Path.Combine(root,Folder+"ready/chizuru/");
            var sheet=Raster.Load(game+"bag_throw.png");var expected=Raster.Load(Path.Combine(root,Folder+"normalized/chizuru/bag_throw_sheet.png"));
            var data=new JavaScriptSerializer().Deserialize<Metadata>(File.ReadAllText(game+"bag_throw.json"));
            if(sheet.Width!=896 || sheet.Height!=160 || !sheet.Pixels.SequenceEqual(expected.Pixels) || data.frameCount!=4 ||
                data.fps!=12 || !data.loop || data.pixelsPerUnit!=50 || data.pivotX!=.5f || data.pivotY!=.35f || data.bindings.Length!=0)
                throw new InvalidOperationException("Bag effect pixels/metadata changed.");
            File.Copy(game+"bag_throw.png",ready+"bag_throw_sheet.png",true);File.Copy(game+"bag_throw.json",ready+"bag_throw.json",true);
            for(int i=0;i<4;i++){var cell=new Raster(224,160);for(int y=0;y<160;y++)Array.Copy(sheet.Pixels,y*896+i*224,cell.Pixels,y*224,224);cell.Save(ready+"bag_throw_"+i.ToString("D2")+".png");}
            File.Copy(Path.Combine(root,"docs/art_wip/chizuru_bag_throw_preview.gif"),ready+"bag_throw_preview.gif",true);
            Console.WriteLine("Bag effect: 4 fixed224x160/PPU50 frames, exact prepared pixels and normal-only effect contract PASS.");
        }
    }
}
