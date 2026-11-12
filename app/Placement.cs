using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using PetFramework;

namespace Judge
{
    internal sealed class Placement
    {
        readonly Assets art;double left,right,above;
        public Placement(Assets assets)
        {
            art=assets;
            foreach(FrameInfo f in art.Manifest.frames.Values)
            {left=Math.Max(left,(f.supportX-f.Bounds.Left)*f.scale);right=Math.Max(right,(f.Bounds.Right-f.supportX)*f.scale);above=Math.Max(above,(f.supportY-f.Bounds.Top)*f.scale);}
        }
        public double Scale(Rectangle area,double preferred)
        {return Math.Max(.01,Math.Min(preferred,Math.Min(Math.Max(1,area.Width-8)/(left+right),Math.Max(1,area.Height-8)/above)));}
        public double Clamp(Rectangle area,double x,double preferred)
        {double s=Scale(area,preferred);return Math.Max(area.Left+left*s+4,Math.Min(area.Right-right*s-4,x));}
        public RectangleF Sprite(string key,PointF support,double scale)
        {
            FrameInfo f=art.Manifest.frames[key];return SpriteSupport.Place(new Size(f.width,f.height),new PointF((float)f.supportX,(float)f.supportY),support,(float)(f.width*f.scale*scale));
        }
        public Rectangle Scene(string key,PointF support,double scale)
        {
            FrameInfo f=art.Manifest.frames[key];RectangleF sprite=Sprite(key,support,scale);double s=f.scale*scale;
            RectangleF visible=new RectangleF(sprite.X+(float)(f.Bounds.Left*s),sprite.Y+(float)(f.Bounds.Top*s),(float)(f.Bounds.Width*s),(float)(f.Bounds.Height*s));
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
