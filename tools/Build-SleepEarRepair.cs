using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;

// Explicit authoring: imagegen repairs the ears; bounded compositing preserves
// every original pixel outside those local ear regions. Never run at build time.
internal static class BuildSleepEarRepair
{
    static PointF[] Points(float[] xy){PointF[] p=new PointF[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new PointF(xy[2*i],xy[2*i+1]);return p;}
    static double Distance(PointF p,PointF a,PointF b)
    {double dx=b.X-a.X,dy=b.Y-a.Y,t=Math.Max(0,Math.Min(1,((p.X-a.X)*dx+(p.Y-a.Y)*dy)/(dx*dx+dy*dy)));return Math.Sqrt(Math.Pow(p.X-a.X-t*dx,2)+Math.Pow(p.Y-a.Y-t*dy,2));}
    static Color Mix(Color a,Color b,double t)
    {
        if(t>=1)return b;if(t<=0)return a;double aa=a.A*(1-t),bb=b.A*t,total=aa+bb;if(total<.5)return Color.Transparent;
        return Color.FromArgb((int)Math.Round(total),(int)Math.Round((a.R*aa+b.R*bb)/total),(int)Math.Round((a.G*aa+b.G*bb)/total),(int)Math.Round((a.B*aa+b.B*bb)/total));
    }
    static void Main(string[] args)
    {
        string root=Path.GetFullPath(args[0]);string[] stages={"nod","early","mid","late"};
        float[][][] shapes={
            new[]{new float[]{130,290,355,290,375,420,330,485,300,540,245,620,135,620},new float[]{450,325,700,325,700,630,535,630,525,560,485,510,440,465}},
            new[]{new float[]{40,335,340,335,350,440,302,472,255,545,225,635,100,670,40,650},new float[]{405,370,690,370,700,675,505,675,490,585,440,545,420,480}},
            new[]{new float[]{25,415,315,415,320,515,285,570,240,635,200,720,70,725,25,665},new float[]{385,435,680,435,680,730,490,730,465,655,420,590,390,545}},
            new[]{new float[]{0,505,285,505,305,590,270,655,225,715,175,820,30,830,0,765},new float[]{395,520,685,520,685,815,525,815,495,755,440,690,405,630}}
        };
        var reports=new List<object>();
        for(int stage=0;stage<stages.Length;stage++)
        {
            string name=stages[stage],output=Path.Combine(root,"art/frames/lay-sleep-"+name+"-v2.png");
            using(Bitmap basis=new Bitmap(Path.Combine(root,"draft-assets/lay-sleep-v1/"+name+".png")))
            using(Bitmap edit=new Bitmap(Path.Combine(root,"draft-assets/lay-sleep-v2/"+name+"-source.png")))
            using(Bitmap result=(Bitmap)basis.Clone())
            {
                if(basis.Size!=edit.Size)throw new Exception("Ear repair canvas changed: "+name);
                var paths=new List<GraphicsPath>();var polygons=new List<PointF[]>();
                foreach(float[] xy in shapes[stage]){PointF[] polygon=Points(xy);GraphicsPath path=new GraphicsPath();path.AddPolygon(polygon);paths.Add(path);polygons.Add(polygon);}
                int changed=0,outside=0;
                try
                {
                    for(int y=0;y<basis.Height;y++)for(int x=0;x<basis.Width;x++)
                    {
                        double weight=0;PointF point=new PointF(x+.5f,y+.5f);
                        for(int ear=0;ear<paths.Count;ear++)if(paths[ear].IsVisible(point))
                        {
                            double distance=10000;PointF[] polygon=polygons[ear];for(int i=0;i<polygon.Length;i++)distance=Math.Min(distance,Distance(point,polygon[i],polygon[(i+1)%polygon.Length]));
                            // Feather only the join through opaque fur. Replace the
                            // transparent contour outright rather than retaining its haze.
                            Color old=basis.GetPixel(x,y),fresh=edit.GetPixel(x,y);
                            weight=Math.Max(weight,(old.A<240||fresh.A<240)?1:Math.Min(1,distance/12));
                        }
                        Color original=basis.GetPixel(x,y);if(weight>0)result.SetPixel(x,y,Mix(original,edit.GetPixel(x,y),weight));
                        if(result.GetPixel(x,y).ToArgb()!=original.ToArgb()){changed++;if(weight<=0)outside++;}
                    }
                    if(changed==0||outside!=0)throw new Exception("Ear region preservation failed: "+name);
                    if(File.Exists(output))throw new Exception("Refusing to replace existing repaired sprite: "+output);
                    result.Save(output,ImageFormat.Png);
                    reports.Add(new{stage=name,changedPixels=changed,changesOutsideEarMasks=outside,bodyFacePawsTailPreserved=true,polygons=shapes[stage]});
                }
                finally{foreach(GraphicsPath path in paths)path.Dispose();}
            }
        }
        File.WriteAllText(Path.Combine(root,"planning/sleep-ear-repair-checks.json"),new JavaScriptSerializer().Serialize(reports));
        // Review only: exact PNG pixels on dark and light backgrounds at 1:1.
        for(int stage=0;stage<stages.Length;stage++)
        using(Bitmap before=new Bitmap(Path.Combine(root,"draft-assets/lay-sleep-v1/"+stages[stage]+".png")))
        using(Bitmap after=new Bitmap(Path.Combine(root,"art/frames/lay-sleep-"+stages[stage]+"-v2.png")))
        using(Bitmap review=new Bitmap(1440,1120))using(Graphics g=Graphics.FromImage(review))
        using(Font font=new Font("Segoe UI",16))
        {
            int top=new[]{285,330,410,500}[stage];Rectangle crop=new Rectangle(0,top,720,500);
            for(int row=0;row<2;row++)for(int column=0;column<2;column++)
            {
                g.FillRectangle(row==0?Brushes.Black:Brushes.Beige,column*720,row*560,720,560);
                g.DrawString((column==0?"Before":"Ear repair candidate")+" | "+stages[stage],font,row==0?Brushes.White:Brushes.Black,column*720+15,row*560+12);
                g.DrawImage(column==0?before:after,new Rectangle(column*720,row*560+60,720,500),crop,GraphicsUnit.Pixel);
            }
            review.Save(Path.Combine(root,"build/sleep-ear-"+stages[stage]+"-0.1.5.png"),ImageFormat.Png);
        }
    }
}
