namespace AniBato.SpriteForge
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Web.Script.Serialization;
    using System.Collections.Generic;

    public static class PhaseSixBPreviews
    {
        public static void Render(string root)
        {
            var rows=new JavaScriptSerializer().Deserialize<List<Dictionary<string,object>>>(File.ReadAllText(Path.Combine(root,"TestResults/phase6b-camera-frames.json")));
            foreach(var row in rows)
            {
                int w=Convert.ToInt32(row["width"]),h=Convert.ToInt32(row["height"]); string mode=(string)row["mode"];
                float cx=Convert.ToSingle(row["centerX"]),cy=Convert.ToSingle(row["centerY"]),size=Convert.ToSingle(row["size"]),scale=h/(size*2);
                Func<float,float> X=x=>w*.5f+(x-cx)*scale, Y=y=>h*.5f-(y-cy)*scale;
                using(var image=new Bitmap(w,h)) using(var g=Graphics.FromImage(image))
                using(var font=new Font("Consolas",18)) using(var floor=new SolidBrush(Color.FromArgb(120,110,90))) using(var edge=new SolidBrush(Color.FromArgb(203,187,150)))
                {
                    g.Clear(Color.FromArgb(25,34,48));
                    using(var background=new Bitmap(Path.Combine(root,"AniBato/Assets/_Project/Art/Stages/phase6b_1_background.png")))
                    {
                        var attributes=new ImageAttributes(); attributes.SetColorMatrix(new ColorMatrix(new float[][]{new[]{.84f,0,0,0,0},new[]{0,.84f,0,0,0},new[]{0,0,.84f,0,0},new[]{0,0,0,1f,0},new[]{0,0,0,0,1f}}));
                        g.DrawImage(background,new Rectangle(-(int)(w*.025f),-(int)(h*.025f),(int)(w*1.05f),(int)(h*1.05f)),0,0,background.Width,background.Height,GraphicsUnit.Pixel,attributes); attributes.Dispose();
                    }
                    float width=Convert.ToSingle(row["floorWidth"]);
                    g.FillRectangle(floor,X(-width/2),Y(0),width*scale,1.2f*scale); g.FillRectangle(edge,X(-width/2),Y(0),width*scale,Math.Max(2,.06f*scale));
                    if((bool)row["upper"]) foreach(float x in new[]{-8f,8f}) g.FillRectangle(edge,X(x-3.5f),Y(4.175f),7*scale,.35f*scale);
                    g.InterpolationMode=InterpolationMode.NearestNeighbor; g.PixelOffsetMode=PixelOffsetMode.Half;
                    for(int p=1;p<=2;p++)
                    {
                        string character=p==1?"toma":"ren";
                        using(var sheet=new Bitmap(Path.Combine(root,"AniBato/Assets/_Project/Art/Sprites/"+character+"/idle.png")))
                        {
                            if(p==2) sheet.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            float x=Convert.ToSingle(row["p"+p+"X"]),y=Convert.ToSingle(row["p"+p+"Y"]);
                            // 224px cells, 50px/unit, feet at y=159 (same sprite pivot as Forge).
                            g.DrawImage(sheet,new RectangleF(X(x)-2.24f*scale,Y(y-1.1f)-3.2f*scale,4.48f*scale,3.2f*scale),new RectangleF(p==2?sheet.Width-224:0,0,224,160),GraphicsUnit.Pixel);
                        }
                    }
                    g.FillRectangle(Brushes.Black,0,0,w,74);
                    g.DrawString("OFFLINE FRAMING PREVIEW - Unity capture pending",font,Brushes.White,20,10);
                    g.DrawString(mode+" / "+w+"x"+h+" / Toma "+Convert.ToSingle(row["tomaHeightPercent"]).ToString("F1")+"% / Ren "+Convert.ToSingle(row["renHeightPercent"]).ToString("F1")+"%",font,Brushes.White,20,40);
                    image.Save(Path.Combine(root,"docs/screenshots/phase6b_camera_"+mode+"_"+w+"x"+h+".png"),ImageFormat.Png);
                }
            }
        }
    }
}
