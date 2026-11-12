using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

// Pet-specific artwork review. No settings, desktop hooks, startup or network use.
internal static class ArtReview
{
    static readonly Bitmap Sit = Load("sit");
    static readonly Bitmap Lay = Load("lay");
    static readonly Bitmap Sleep = Load("sleep");
    static readonly Bitmap[] Blink = { Lay, Eyes(Load("early")), Eyes(Load("late")), Eyes(Load("closed")) };
    // Native eye centres measured inside bounded dark-eye ROIs. Closed sleep uses
    // eyelid centres, so sleep scale remains a visual-review estimate, not approval.
    const double SitEyeSpan = 160.1894384783217;
    const double LayEyeSpan = 119.21862690032964;
    const double SleepEyeSpan = 119.70077401587679;
    const float SitSupport = 1232, LaySupport = 976, SleepSupport = 903;
    static Bitmap Load(string name)
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JudgeReview." + name))
        using (Bitmap source = new Bitmap(stream)) return new Bitmap(source);
    }
    static double EyeWeight(int x, int y)
    {
        double left = Math.Sqrt(Math.Pow((x-346)/36.0,2)+Math.Pow((y-477)/27.0,2));
        double right = Math.Sqrt(Math.Pow((x-465)/36.0,2)+Math.Pow((y-490)/28.0,2));
        return Math.Max(0,Math.Min(1,(1-Math.Min(left,right))/.20));
    }
    static Bitmap Eyes(Bitmap source)
    {
        if(source.Size != Lay.Size) throw new Exception("Blink source canvas changed.");
        Bitmap result = (Bitmap)Lay.Clone();
        // Generated keys supply eyelid artwork only. Body, alpha, nose and mouth
        // remain the same base bitmap on every animation frame.
        for(int y=448;y<520;y++) for(int x=309;x<502;x++)
        {
            double t=EyeWeight(x,y); if(t<=0)continue;
            Color a=Lay.GetPixel(x,y),b=source.GetPixel(x,y);
            result.SetPixel(x,y,Color.FromArgb(a.A,(int)Math.Round(a.R+(b.R-a.R)*t),
                (int)Math.Round(a.G+(b.G-a.G)*t),(int)Math.Round(a.B+(b.B-a.B)*t)));
        }
        source.Dispose();return result;
    }
    static int Frame(double seconds)
    {
        double t=seconds%3.2;
        if(t<2.70)return 0;if(t<2.755)return 1;if(t<2.81)return 2;
        if(t<2.89)return 3;if(t<2.945)return 2;if(t<3.0)return 1;return 0;
    }
    static void DrawPose(Graphics g,Bitmap image,float centre,float floor,double scale,float support,float nativeCentre)
    {
        float s=(float)scale;
        g.DrawImage(image,new RectangleF(centre-nativeCentre*s,floor-support*s,image.Width*s,image.Height*s));
    }
    internal static void Render(Graphics g,int width,int height,double seconds,bool light)
    {
        g.Clear(light?Color.FromArgb(242,239,230):Color.FromArgb(25,30,38));
        g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        float k=Math.Min(width/1680f,height/660f);g.ScaleTransform(k,k);
        Color ink=light?Color.FromArgb(35,43,52):Color.FromArgb(235,239,244);
        using(Brush text=new SolidBrush(ink))
        using(Font title=new Font("Segoe UI",18,FontStyle.Bold))
        using(Font label=new Font("Segoe UI",12))
        using(Font small=new Font("Segoe UI",10))
        using(Pen floor=new Pen(light?Color.FromArgb(120,134,143):Color.FromArgb(88,111,130),1))
        {
            g.DrawString("The Judge | common scale and blink review",title,text,30,20);
            g.DrawString("Approved sit is the scale reference. Lying and sleeping are candidates. Space: pause blink. B: light/dark.",small,text,30,57);
            g.DrawString("1  Approved sit",label,text,35,100);
            g.DrawString("2  Awake lying + blink",label,text,585,100);
            g.DrawString("3  Head-down sleep",label,text,1135,100);
            g.DrawLine(floor,25,545,1655,545);
            const double scale=.30;
            DrawPose(g,Sit,295,545,scale,SitSupport,664);
            DrawPose(g,Blink[Frame(seconds)],850,545,scale*SitEyeSpan/LayEyeSpan,LaySupport,640);
            DrawPose(g,Sleep,1400,545,scale*SitEyeSpan/SleepEyeSpan,SleepSupport,624);
            g.DrawString("Unchanged approved artwork",small,text,35,570);
            g.DrawString("Open > 1/3 > 2/3 > closed > 2/3 > 1/3 > open",small,text,585,570);
            g.DrawString("Chin supported; closed eyes",small,text,1135,570);
            g.DrawString("Same display head scale; sleeping head angle still needs visual review. Floor anchors are provisional.",small,text,35,613);
        }
    }
    static void Check(string path)
    {
        int outside=0,alpha=0,changed=0;
        for(int n=1;n<Blink.Length;n++)for(int y=0;y<Lay.Height;y++)for(int x=0;x<Lay.Width;x++)
        {
            Color a=Lay.GetPixel(x,y),b=Blink[n].GetPixel(x,y);
            if(a.A!=b.A)alpha++;
            if(a.ToArgb()!=b.ToArgb()){changed++;if(EyeWeight(x,y)<=0)outside++;}
        }
        if(outside!=0||alpha!=0||changed==0)throw new Exception("Blink body/alpha invariant failed.");
        if(Frame(2.72)!=1||Frame(2.78)!=2||Frame(2.85)!=3||Frame(2.92)!=2||Frame(2.97)!=1||Frame(3.1)!=0)
            throw new Exception("Blink sequence failed.");
        string report="{\"passed\":true,\"outsideEyeChanges\":"+outside+",\"alphaChanges\":"+alpha+
            ",\"eyeChangedPixelsAcrossKeys\":"+changed+",\"blinkIntermediateStages\":2,"+
            "\"sequence\":[0,1,2,3,2,1,0],\"scaleMethod\":\"approved sit eye span\",\"ownerApproved\":false}";
        File.WriteAllText(path,report);
    }
    [STAThread] public static void Main(string[] args)
    {
        if(args.Length==2&&args[0]=="--check"){Check(args[1]);return;}
        if(args.Length==2&&args[0]=="--render")
        {
            using(Bitmap output=new Bitmap(1680,660))using(Graphics g=Graphics.FromImage(output))
            {Render(g,1680,660,0,false);output.Save(args[1],ImageFormat.Png);}return;
        }
        if(args.Length==2&&args[0]=="--blink-frames")
        {
            Directory.CreateDirectory(args[1]);int[] keys={0,1,2,3,2,1,0};
            for(int i=0;i<keys.Length;i++)
            using(Bitmap output=new Bitmap(660,460))using(Graphics g=Graphics.FromImage(output))
            {
                g.Clear(Color.FromArgb(25,30,38));g.InterpolationMode=InterpolationMode.HighQualityBicubic;
                g.DrawImage(Blink[keys[i]],new Rectangle(0,0,660,410),new Rectangle(240,410,330,205),GraphicsUnit.Pixel);
                using(Font font=new Font("Segoe UI",12))using(Brush brush=new SolidBrush(Color.White))
                    g.DrawString("Blink detail | two intermediate stages | body stays fixed",font,brush,10,425);
                output.Save(Path.Combine(args[1],i.ToString("00")+".png"),ImageFormat.Png);
            }
            return;
        }
        Application.EnableVisualStyles();Application.Run(new ReviewWindow());
    }
    sealed class ReviewWindow:Form
    {
        readonly Timer timer=new Timer();double seconds;bool paused,light;
        public ReviewWindow()
        {
            Text="The Judge - artwork review (candidates)";DoubleBuffered=true;
            ClientSize=new Size(1680,660);MinimumSize=new Size(1000,440);StartPosition=FormStartPosition.CenterScreen;
            timer.Interval=16;timer.Tick+=delegate{if(!paused)seconds+=.016;Invalidate();};timer.Start();
            KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Space)paused=!paused;if(e.KeyCode==Keys.B)light=!light;Invalidate();};
        }
        protected override void OnPaint(PaintEventArgs e){Render(e.Graphics,ClientSize.Width,ClientSize.Height,seconds,light);}
        protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
    }
}
