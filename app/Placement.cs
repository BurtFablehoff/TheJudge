using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using PetFramework;

namespace Judge
{
    internal sealed class Placement
    {
        readonly Assets art;double left,right,above;
        readonly Dictionary<string,RectangleF> gazeBounds = new Dictionary<string,RectangleF>();
        public Placement(Assets assets)
        {
            art=assets;
            foreach(var entry in art.Manifest.frames)
            {
                FrameInfo f=entry.Value;
                left=Math.Max(left,(f.supportX-f.Bounds.Left)*f.scale);
                right=Math.Max(right,(f.Bounds.Right-f.supportX)*f.scale);
                above=Math.Max(above,(FloorY(entry.Key)-f.Bounds.Top)*f.scale);
                string pose=GazePose(entry.Key);
                if(pose!=entry.Key)
                {
                    RectangleF bounds;
                    if(!gazeBounds.TryGetValue(pose,out bounds))
                        bounds=BoundsRelativeToSupport(pose);
                    gazeBounds[pose]=RectangleF.Union(bounds,BoundsRelativeToSupport(entry.Key));
                }
            }
        }
        internal static string GazePose(string key)
        {
            int separator=key.IndexOf("-look-",StringComparison.Ordinal);
            return separator<0?key:key.Substring(0,separator);
        }
        internal double FloorY(string key)
        {
            FrameInfo frame=art.Manifest.frames[key];
            string pose=GazePose(key);
            if(pose==key)return frame.bodyFloorY??frame.supportY;
            FrameInfo neutral=art.Manifest.frames[pose];
            // Apply the same display-space shift to the neutral and all of its approved gaze transforms.
            double offset=((neutral.bodyFloorY??neutral.supportY)-neutral.supportY)*neutral.scale;
            return frame.supportY+offset/frame.scale;
        }
        RectangleF BoundsRelativeToSupport(string key)
        {
            FrameInfo frame=art.Manifest.frames[key];
            Rectangle b=frame.Bounds;
            return new RectangleF((float)((b.Left-frame.supportX)*frame.scale),(float)((b.Top-FloorY(key))*frame.scale),
                (float)(b.Width*frame.scale),(float)(b.Height*frame.scale));
        }
        public double Scale(Rectangle area,double preferred)
        {return Math.Max(.01,Math.Min(preferred,Math.Min(Math.Max(1,area.Width-8)/(left+right),Math.Max(1,area.Height-8)/above)));}
        public double Clamp(Rectangle area,double x,double preferred)
        {double s=Scale(area,preferred);return Math.Max(area.Left+left*s+4,Math.Min(area.Right-right*s-4,x));}
        public RectangleF Sprite(string key,PointF support,double scale)
        {
            FrameInfo f=art.Manifest.frames[key];return SpriteSupport.Place(new Size(f.width,f.height),new PointF((float)f.supportX,(float)FloorY(key)),support,(float)(f.width*f.scale*scale));
        }
        public Rectangle Scene(string key,PointF support,double scale)
        {
            // Share the screen-space crop even when registered sources use different native scales.
            RectangleF bounds;
            if(!gazeBounds.TryGetValue(GazePose(key),out bounds))
                bounds=BoundsRelativeToSupport(key);
            RectangleF visible=new RectangleF(support.X+(float)(bounds.X*scale),support.Y+(float)(bounds.Y*scale),
                (float)(bounds.Width*scale),(float)(bounds.Height*scale));
            Rectangle result=Rectangle.Ceiling(visible);result.Inflate(3,3);return result;
        }
        public Bitmap Paint(string key,PointF support,double scale,out Point location)
        {
            Rectangle scene=Scene(key,support,scale);location=scene.Location;
            Bitmap bitmap=new Bitmap(Math.Max(1,scene.Width),Math.Max(1,scene.Height),PixelFormat.Format32bppPArgb);
            using(Graphics g=Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;
                RectangleF sprite=Sprite(key,support,scale);sprite.Offset(-scene.Left,-scene.Top);g.DrawImage(art.Get(key),sprite);
            }
            return bitmap;
        }
    }
}
