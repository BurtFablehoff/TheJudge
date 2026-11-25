using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

namespace Judge
{
    internal static class WatchReview
    {
        public static void Render(Assets art,string pose,string output)
        {
            Directory.CreateDirectory(output);
            var placement = new Placement(art);
            var watch = new WatchGaze();
            const double scale = .24;
            var support = new PointF(pose=="sit"?188:140, 340);
            FrameInfo neutral = art.Manifest.frames[pose];
            RectangleF sprite = placement.Sprite(pose, support, scale);
            PointF head = WatchGaze.HeadOnScreen(pose, sprite, new Size(neutral.width,neutral.height));
            var sectors = new List<int>{-1};
            for (int sector=0; sector<12; sector++) sectors.Add(sector);
            sectors.AddRange(new[] {0,-1,0});
            for (int sector=11; sector>0; sector--) sectors.Add(sector);
            sectors.AddRange(new[] {0,-1});
            var delays = new List<int>();
            watch.Toggle(WatchMode.Mouse);
            for (int index=0; index<sectors.Count; index++)
            {
                int sector=sectors[index];
                double angle=sector*Math.PI/6;
                PointF target=sector<0?head:new PointF(head.X+(float)(480*Math.Sin(angle)),head.Y-(float)(480*Math.Cos(angle)));
                watch.Step(index*.12,pose,pose,head,target,14);
                string expected=sector<0?pose:WatchGaze.Key(pose,sector);
                string key=watch.Frame(pose,pose);
                if (key!=expected) throw new InvalidDataException("Watch review selected "+key+" instead of "+expected);
                string label=sector<0?"Neutral":(sector==0?12:sector).ToString(CultureInfo.InvariantCulture)+" o’clock";
                Point location;
                using (Bitmap frame=placement.Paint(key,support,scale,out location))
                using (Bitmap board=new Bitmap(800,387))
                using (Graphics graphics=Graphics.FromImage(board))
                using (Font heading=new Font("Segoe UI",15,FontStyle.Bold))
                {
                    graphics.Clear(Color.FromArgb(18,23,29));
                    graphics.FillRectangle(Brushes.WhiteSmoke,400,0,400,387);
                    using(Brush darkFloor=new SolidBrush(Color.FromArgb(43,51,61)))
                    using(Brush lightFloor=new SolidBrush(Color.FromArgb(204,213,223)))
                    {
                        graphics.FillRectangle(darkFloor,0,340,400,47);
                        graphics.FillRectangle(lightFloor,400,340,400,47);
                    }
                    graphics.DrawLine(Pens.Gray,0,340,799,340);
                    graphics.DrawString(label,heading,Brushes.WhiteSmoke,14,7);
                    graphics.DrawString(label,heading,Brushes.Black,414,7);
                    graphics.DrawImageUnscaled(frame,location);
                    graphics.DrawImageUnscaled(frame,location.X+400,location.Y);
                    board.Save(Path.Combine(output,index.ToString("00",CultureInfo.InvariantCulture)+".png"),ImageFormat.Png);
                }
                delays.Add(sector<0?620:120);
            }
            File.WriteAllText(Path.Combine(output,"delays.json"),new JavaScriptSerializer().Serialize(delays));
        }
    }
}
