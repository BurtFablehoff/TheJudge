using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Explicit pet-specific authoring. Only generated head/face pixels are used;
// the approved lying body and contact silhouette remain exact base pixels.
internal static class BuildLayExpressions
{
    static Bitmap basis;static string root,output;
    static double Clamp(double v){return Math.Max(0,Math.Min(1,v));}
    static double HeadWeight(int x,int y)
    {if(x>=720||y>=680)return 0;if(y<=600)return 1;return Clamp((680-y)/80.0)*Clamp((575-x)/50.0);}
    static double FaceWeight(int x,int y)
    {
        double a=Math.Sqrt(Math.Pow((x-346.25)/33,2)+Math.Pow((y-475.47)/26,2));
        double b=Math.Sqrt(Math.Pow((x-464.68)/33,2)+Math.Pow((y-489.16)/26,2));
        double c=Math.Sqrt(Math.Pow((x-392)/73.0,2)+Math.Pow((y-564)/77.0,2));
        return Clamp((1-Math.Min(c,Math.Min(a,b)))/.20);
    }
    static Color Mix(Color a,Color b,double t,bool keepAlpha)
    {
        if(keepAlpha)return Color.FromArgb(a.A,(int)Math.Round(a.R+(b.R-a.R)*t),(int)Math.Round(a.G+(b.G-a.G)*t),(int)Math.Round(a.B+(b.B-a.B)*t));
        double aa=a.A*(1-t),bb=b.A*t,total=aa+bb;if(total<.5)return Color.Transparent;
        return Color.FromArgb((int)Math.Round(total),(int)Math.Round((a.R*aa+b.R*bb)/total),(int)Math.Round((a.G*aa+b.G*bb)/total),(int)Math.Round((a.B*aa+b.B*bb)/total));
    }
    static void Make(string folder,string stage,string key,double scale,bool face,double sx,double sy)
    {
        using(Bitmap raw=new Bitmap(Path.Combine(root,"draft-assets/"+folder+"/"+stage+".png")))
        using(Bitmap source=new Bitmap(basis.Width,basis.Height,PixelFormat.Format32bppArgb))
        using(Bitmap result=(Bitmap)basis.Clone())
        {
            if(raw.Size!=basis.Size)throw new Exception("Canvas changed.");
            using(Graphics g=Graphics.FromImage(source))
            {
                g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
                if(face||folder=="lay-smile-v1")
                {g.TranslateTransform(405.465f,482.315f);g.ScaleTransform((float)scale,(float)scale);g.TranslateTransform((float)-sx,(float)-sy);}
                else{g.TranslateTransform(405,675);g.ScaleTransform((float)scale,(float)scale);g.TranslateTransform(-405,-675);}
                g.DrawImage(raw,0,0,raw.Width,raw.Height);
            }
            for(int y=0;y<680;y++)for(int x=0;x<720;x++)
            {double weight=face?FaceWeight(x,y):HeadWeight(x,y);if(weight>0)result.SetPixel(x,y,Mix(basis.GetPixel(x,y),source.GetPixel(x,y),weight,face));}
            string path=Path.Combine(output,key+".png");
            if(File.Exists(path))
            {using(Bitmap old=new Bitmap(path))for(int y=0;y<old.Height;y++)for(int x=0;x<old.Width;x++)if(old.GetPixel(x,y).ToArgb()!=result.GetPixel(x,y).ToArgb())throw new Exception("Refusing to overwrite different expression: "+key);}
            else result.Save(path,ImageFormat.Png);
        }
    }
    public static void Main(string[] args)
    {
        root=args[0];output=Path.Combine(root,"art/frames");
        using(Bitmap original=new Bitmap(Path.Combine(output,"lay.png")))basis=(Bitmap)original.Clone();
        using(basis)
        {
            Make("lay-tilt-v1","mid","lay-tilt-1",.999921,false,0,0);Make("lay-tilt-v1","peak","lay-tilt-2",.993453,false,0,0);
            Make("lay-smile-v1","mid","lay-smile-1",1.005866,false,405.84,483.41);Make("lay-smile-v1","peak","lay-smile-2",.929367,false,412.76,480.95);
            Make("lay-lick-v1","start","lay-lick-1",.999642,true,404.77,481.98);Make("lay-lick-v1","peak","lay-lick-2",1.038345,true,405.92,485.35);Make("lay-lick-v1","return","lay-lick-3",.992427,true,405.33,482.55);
        }
    }
}
