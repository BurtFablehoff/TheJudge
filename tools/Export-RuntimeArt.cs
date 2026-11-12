using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

// Explicit promotion of the exact renderer-approved pixels, not new artwork.
internal static class ExportRuntimeArt
{
    static Bitmap Field(Type type,string name){return (Bitmap)type.GetField(name,BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);}
    static Bitmap[] Array(Type type,string name){return (Bitmap[])type.GetField(name,BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);}
    static void Save(Bitmap bitmap,string directory,string name)
    {
        string path=Path.Combine(directory,name+".png");
        if(File.Exists(path))
        {
            using(Bitmap prior=new Bitmap(path))for(int y=0;y<bitmap.Height;y++)for(int x=0;x<bitmap.Width;x++)
                if(prior.GetPixel(x,y).ToArgb()!=bitmap.GetPixel(x,y).ToArgb())throw new Exception("Refusing to replace different promoted art: "+name);
            return;
        }
        bitmap.Save(path,ImageFormat.Png);
    }
    public static void Main(string[] args)
    {
        string directory=args[0];Directory.CreateDirectory(directory);
        Save(Field(typeof(ArtReview),"Sit"),directory,"sit");
        Save(Field(typeof(ArtReview),"Lay"),directory,"lay");
        Save(Field(typeof(ArtReview),"Sleep"),directory,"sleep");
        Bitmap[] blink=Array(typeof(ArtReview),"Blink");
        for(int i=1;i<blink.Length;i++)Save(blink[i],directory,"lay-blink-"+i);
        string[] groups={"Tilt","Smile","Lick"};
        foreach(string group in groups){Bitmap[] frames=Array(typeof(ActionReview),group);for(int i=1;i<frames.Length;i++)Save(frames[i],directory,group.ToLower()+"-"+i);}
        if(args.Length>1)
        {
            Bitmap sit=Field(typeof(ArtReview),"Sit");string root=args[1];
            foreach(string stage in new[]{"half","closed"})
            using(Bitmap key=new Bitmap(Path.Combine(root,"draft-assets/sit-blink-v1/"+stage+".png")))
            using(Bitmap output=(Bitmap)sit.Clone())
            {
                for(int y=245;y<326;y++)for(int x=402;x<652;x++)
                {
                    double a=Math.Sqrt(Math.Pow((x-446.69)/43,2)+Math.Pow((y-280.98)/33,2));
                    double b=Math.Sqrt(Math.Pow((x-606.58)/43,2)+Math.Pow((y-290.77)/33,2));
                    double t=Math.Max(0,Math.Min(1,(1-Math.Min(a,b))/.20));if(t<=0)continue;
                    Color s=sit.GetPixel(x,y),v=key.GetPixel(x,y);
                    output.SetPixel(x,y,Color.FromArgb(s.A,(int)Math.Round(s.R+(v.R-s.R)*t),(int)Math.Round(s.G+(v.G-s.G)*t),(int)Math.Round(s.B+(v.B-s.B)*t)));
                }
                Save(output,directory,"sit-blink-"+stage);
            }
            foreach(string stage in new[]{"early","mid","late"})
            using(Bitmap source=new Bitmap(Path.Combine(root,"draft-assets/sit-lay-v1/"+stage+".png")))Save(source,directory,"sit-lay-"+stage);
        }
    }
}
