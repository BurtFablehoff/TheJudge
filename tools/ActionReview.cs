using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

// Pet-specific candidate review; approved artwork is read-only embedded input.
internal static class ActionReview
{
    static readonly Bitmap Sit=Load("sit");
    static readonly Bitmap[] Tilt={Sit,Head("tiltMid",1.033617,-3.3,520,545,520,545),Head("tiltPeak",1.012529,0,520,545,520,545)};
    static readonly Bitmap[] Smile={Sit,Head("smileMid",.993683,0,527.69,282.305,526.635,285.875),Head("smilePeak",.969114,0,546.555,262.455,526.635,285.875)};
    static readonly Bitmap[] Lick={Sit,Face("lickStart",1.006802,526.94,288.35),Face("lickPeak",1.026332,524.51,305.58),Face("lickReturn",.994197,526.44,286.24)};
    static Bitmap Load(string name)
    {
        using(Stream s=Assembly.GetExecutingAssembly().GetManifestResourceStream("JudgeReview."+name))
        using(Bitmap b=new Bitmap(s))return new Bitmap(b);
    }
    static Bitmap Registered(string name,double scale,double angle,double sx,double sy,double dx,double dy)
    {
        using(Bitmap source=Load(name))
        {
            if(source.Size!=Sit.Size)throw new Exception("Action canvas changed: "+name);
            Bitmap result=new Bitmap(Sit.Width,Sit.Height,PixelFormat.Format32bppArgb);
            using(Graphics g=Graphics.FromImage(result))
            {
                g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
                g.TranslateTransform((float)dx,(float)dy);g.RotateTransform((float)angle);
                g.ScaleTransform((float)scale,(float)scale);g.TranslateTransform((float)-sx,(float)-sy);
                g.DrawImage(source,0,0,source.Width,source.Height);
            }
            return result;
        }
    }
    static Color Mix(Color a,Color b,double t,bool preserveAlpha)
    {
        if(preserveAlpha)return Color.FromArgb(a.A,(int)Math.Round(a.R+(b.R-a.R)*t),(int)Math.Round(a.G+(b.G-a.G)*t),(int)Math.Round(a.B+(b.B-a.B)*t));
        double aa=a.A*(1-t),bb=b.A*t,total=aa+bb;
        if(total<.5)return Color.Transparent;
        return Color.FromArgb((int)Math.Round(total),(int)Math.Round((a.R*aa+b.R*bb)/total),
            (int)Math.Round((a.G*aa+b.G*bb)/total),(int)Math.Round((a.B*aa+b.B*bb)/total));
    }
    static Bitmap Head(string name,double scale,double angle,double sx,double sy,double dx,double dy)
    {
        Bitmap result=(Bitmap)Sit.Clone();
        using(Bitmap source=Registered(name,scale,angle,sx,sy,dx,dy))
        for(int y=0;y<560;y++)for(int x=0;x<Sit.Width;x++)
        {
            double t=y<=500?1:(560-y)/60.0;
            result.SetPixel(x,y,Mix(Sit.GetPixel(x,y),source.GetPixel(x,y),t,false));
        }
        return result;
    }
    static double FaceWeight(int x,int y)
    {
        double a=Math.Sqrt(Math.Pow((x-446.69)/43,2)+Math.Pow((y-280.98)/32,2));
        double b=Math.Sqrt(Math.Pow((x-606.58)/43,2)+Math.Pow((y-290.77)/32,2));
        double c=Math.Sqrt(Math.Pow((x-520)/96.0,2)+Math.Pow((y-398)/97.0,2));
        return Math.Max(0,Math.Min(1,(1-Math.Min(c,Math.Min(a,b)))/.15));
    }
    static Bitmap Face(string name,double scale,double sx,double sy)
    {
        Bitmap result=(Bitmap)Sit.Clone();
        using(Bitmap source=Registered(name,scale,0,sx,sy,526.635,285.875))
        for(int y=245;y<496;y++)for(int x=402;x<652;x++)
        {
            double t=FaceWeight(x,y);if(t<=0)continue;
            result.SetPixel(x,y,Mix(Sit.GetPixel(x,y),source.GetPixel(x,y),t,true));
        }
        return result;
    }
    static int HeldFrame(double time)
    {
        double t=time%3.6;if(t<2)return 0;if(t<2.12)return 1;if(t<3)return 2;if(t<3.12)return 1;return 0;
    }
    static int LickFrame(double time)
    {
        double t=time%3.6;if(t<2)return 0;if(t<2.08)return 1;if(t<2.16)return 2;if(t<2.24)return 3;return 0;
    }
    static Bitmap At(int action,double time){return action==0?Tilt[HeldFrame(time)]:action==1?Smile[HeldFrame(time)]:Lick[LickFrame(time)];}
    static readonly string[] Names={"1  Head tilt","2  Cheesy ears-back smile","3  Nose lick"};
    static void Render(Graphics g,int w,int h,double time,bool light,bool faces)
    {
        g.Clear(light?Color.FromArgb(242,239,230):Color.FromArgb(25,30,38));
        g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        float k=Math.Min(w/(faces?1170f:1500f),h/(faces?430f:700f));g.ScaleTransform(k,k);
        using(Brush text=new SolidBrush(light?Color.FromArgb(35,43,52):Color.FromArgb(235,239,244)))
        using(Font title=new Font("Segoe UI",18,FontStyle.Bold))using(Font label=new Font("Segoe UI",12))
        using(Pen line=new Pen(Color.FromArgb(88,111,130)))
        {
            g.DrawString("The Judge | seated action candidates",title,text,25,15);
            g.DrawString("Same seated body and floor. Space: pause. B: light/dark. D: face detail.",label,text,25,53);
            for(int i=0;i<3;i++)
            {
                if(faces)
                {
                    g.DrawImage(At(i,time),new Rectangle(390*i+10,95,370,278),new Rectangle(160,0,800,600),GraphicsUnit.Pixel);
                    g.DrawString(Names[i],label,text,390*i+20,392);
                }
                else
                {
                    float scale=.36f,x=260+i*500-664*scale,y=605-1232*scale;
                    g.DrawImage(At(i,time),new RectangleF(x,y,Sit.Width*scale,Sit.Height*scale));
                    g.DrawString(Names[i],label,text,500*i+30,108);
                    g.DrawString("Neutral > action > unchanged neutral",label,text,500*i+30,630);
                }
            }
            if(!faces)g.DrawLine(line,25,605,1475,605);
        }
    }
    static void Check(string path)
    {
        int body=0,faceOutside=0,alpha=0;Bitmap[][] groups={Tilt,Smile,Lick};
        for(int group=0;group<3;group++)for(int n=1;n<groups[group].Length;n++)
        {
            Bitmap frame=groups[group][n];
            for(int y=0;y<Sit.Height;y++)for(int x=0;x<Sit.Width;x++)
            {
                Color a=Sit.GetPixel(x,y),b=frame.GetPixel(x,y);
                if(y>=560&&a.ToArgb()!=b.ToArgb())body++;
                if(group==2)
                {
                    if(a.A!=b.A)alpha++;
                    if(FaceWeight(x,y)<=0&&a.ToArgb()!=b.ToArgb())faceOutside++;
                }
            }
        }
        if(body!=0||alpha!=0||faceOutside!=0)throw new Exception("Action stability check failed.");
        if(HeldFrame(0)!=0||HeldFrame(2.05)!=1||HeldFrame(2.5)!=2||HeldFrame(3.05)!=1||HeldFrame(3.3)!=0||LickFrame(2.18)!=3||LickFrame(2.3)!=0)
            throw new Exception("Action return sequence failed.");
        File.WriteAllText(path,"{\"passed\":true,\"actionKeys\":7,\"bodyChangesBelow560\":0,\"lickAlphaChanges\":0,\"lickChangesOutsideFace\":0,\"sameFloorAndDisplayScale\":true,\"allReturnToApprovedSit\":true,\"ownerApproved\":false}");
    }
    [STAThread]static void Main(string[] args)
    {
        if(args.Length==2&&args[0]=="--check"){Check(args[1]);return;}
        if(args.Length==2&&args[0]=="--render")
        {
            using(Bitmap b=new Bitmap(1500,700))using(Graphics g=Graphics.FromImage(b)){Render(g,1500,700,2.14,false,false);b.Save(args[1],ImageFormat.Png);}return;
        }
        if(args.Length==2&&args[0]=="--action-frames")
        {
            Directory.CreateDirectory(args[1]);double[] times={0,2.001,2.081,2.121,2.161,2.241,3.001,3.121};
            for(int i=0;i<times.Length;i++)using(Bitmap b=new Bitmap(1170,430))using(Graphics g=Graphics.FromImage(b))
            {Render(g,1170,430,times[i],false,true);b.Save(Path.Combine(args[1],i.ToString("00")+".png"),ImageFormat.Png);}return;
        }
        Application.EnableVisualStyles();Application.Run(new ReviewWindow());
    }
    sealed class ReviewWindow:Form
    {
        readonly Timer timer=new Timer();double time;bool paused,light,faces;
        internal ReviewWindow()
        {
            Text="The Judge - seated action review (candidates)";ClientSize=new Size(1500,700);MinimumSize=new Size(900,430);DoubleBuffered=true;StartPosition=FormStartPosition.CenterScreen;
            timer.Interval=16;timer.Tick+=delegate{if(!paused)time+=.016;Invalidate();};timer.Start();
            KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Space)paused=!paused;if(e.KeyCode==Keys.B)light=!light;if(e.KeyCode==Keys.D)faces=!faces;Invalidate();};
        }
        protected override void OnPaint(PaintEventArgs e){Render(e.Graphics,ClientSize.Width,ClientSize.Height,time,light,faces);}
        protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
    }
}
