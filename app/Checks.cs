using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using PetFramework;

namespace Judge
{
    internal static class Checks
    {
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
        static byte[] Pixels(Bitmap bitmap)
        {
            Rectangle r=new Rectangle(0,0,bitmap.Width,bitmap.Height);BitmapData data=bitmap.LockBits(r,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
            try{byte[] result=new byte[bitmap.Width*bitmap.Height*4];for(int y=0;y<bitmap.Height;y++)Marshal.Copy(IntPtr.Add(data.Scan0,y*data.Stride),result,y*bitmap.Width*4,bitmap.Width*4);return result;}
            finally{bitmap.UnlockBits(data);}
        }
        static Rectangle Bounds(byte[] pixels,int width,int height)
        {
            int left=width,right=0,top=height,bottom=0;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)if(pixels[(y*width+x)*4+3]>8){left=Math.Min(left,x);right=Math.Max(right,x+1);top=Math.Min(top,y);bottom=Math.Max(bottom,y+1);}
            return Rectangle.FromLTRB(left,top,right,bottom);
        }
        static void Region(byte[] a,byte[] b,int width,int yStart,bool alphaOnly,string description)
        {
            for(int i=yStart*width*4;i<a.Length;i++)if(!alphaOnly||i%4==3)Require(a[i]==b[i],description+" differs at byte "+i);
        }
        static void Order()
        {
            IntPtr self=new IntPtr(1);Rectangle body=new Rectangle(100,600,300,400);uint process=42;
            TopWindow pet=new TopWindow(self,"pet",body,true,process),tray=new TopWindow(new IntPtr(2),"Shell_TrayWnd",new Rectangle(0,1000,1920,80),true,50),popup=new TopWindow(new IntPtr(3),"popup",body,true,60),own=new TopWindow(new IntPtr(4),"menu",body,true,process),away=new TopWindow(new IntPtr(5),"popup",new Rectangle(10,10,40,40),true,70),plain=new TopWindow(new IntPtr(6),"plain",body,false,80),tiny=new TopWindow(new IntPtr(7),"tiny",new Rectangle(100,600,1,1),true,90);
            TopWindow[][] cases={new[]{tiny,tray,pet},new[]{pet,tray},new[]{popup,tray,pet},new[]{away,tray,pet},new[]{own,tray,pet},new[]{plain,tray,pet},new[]{pet},new[]{tray}};
            bool[] expected={true,false,false,true,true,true,false,false};
            for(int i=0;i<cases.Length;i++)Require(TaskbarOrder.ShouldRaise(cases[i],self,process,body)==expected[i],"Taskbar order case "+i);
        }
        public static void Run(Assets art,string path)
        {
            byte[] sit=Pixels(art.Get("sit")),lay=Pixels(art.Get("lay"));int checkedFrames=0;
            foreach(KeyValuePair<string,FrameInfo> pair in art.Manifest.frames)
            {
                using(Stream stream=Assets.Open("Judge.Art."+pair.Key))using(SHA256 sha=SHA256.Create())
                    Require(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant()==pair.Value.sha256,"Embedded art digest: "+pair.Key);
                Bitmap bitmap=art.Get(pair.Key);byte[] data=Pixels(bitmap);Require(Bounds(data,bitmap.Width,bitmap.Height)==pair.Value.Bounds,"Actual alpha bounds: "+pair.Key);
                if(pair.Key.StartsWith("tilt-")||pair.Key.StartsWith("smile-")||pair.Key.StartsWith("lick-"))Region(sit,data,1254,560,false,"Approved sitting body "+pair.Key);
                if(pair.Key.StartsWith("lay-tilt-")||pair.Key.StartsWith("lay-smile-")||pair.Key.StartsWith("lay-lick-"))Region(lay,data,1254,680,false,"Lying expression body "+pair.Key);
                if(pair.Key.StartsWith("lay-lick-"))Region(lay,data,1254,0,true,"Lying lick alpha");
                if(pair.Key.StartsWith("lay-blink-")){Region(lay,data,1254,0,true,"Lying blink alpha");Region(lay,data,1254,520,false,"Lying blink body");}
                if(pair.Key.StartsWith("sit-blink-")){Region(sit,data,1254,0,true,"Sitting blink alpha");Region(sit,data,1254,326,false,"Sitting blink body");}
                checkedFrames++;
            }
            foreach(KeyValuePair<string,ClipInfo> pair in art.Manifest.clips)
            {
                Require(pair.Value.frames.Length==pair.Value.seconds.Length&&pair.Value.Duration>0,"Invalid clip timing.");
                foreach(string frame in pair.Value.frames)Require(art.Manifest.frames.ContainsKey(frame),"Missing clip frame: "+frame);
            }
            Placement placement=new Placement(art);int placements=0,bodyRegistrationCases=0;
            Rectangle[] monitors={new Rectangle(0,0,1920,1080),new Rectangle(-1920,-180,1920,1080),new Rectangle(1920,400,1280,1024),new Rectangle(0,0,420,600),new Rectangle(-300,0,300,500)};
            foreach(Rectangle area in monitors)foreach(double preference in new[]{.24,.30,.36})foreach(KeyValuePair<string,FrameInfo> pair in art.Manifest.frames)
            foreach(double requested in new[]{area.Left-10000.0,area.Right+10000.0})
            {
                double scale=placement.Scale(area,preference),x=placement.Clamp(area,requested,preference);PointF support=new PointF((float)x,area.Bottom);
                RectangleF sprite=placement.Sprite(pair.Key,support,scale);FrameInfo f=pair.Value;double s=f.scale*scale;
                Require(sprite.Left+f.Bounds.Left*s>=area.Left+3.8&&sprite.Left+f.Bounds.Right*s<=area.Right-3.8,"Visible side clipped: "+pair.Key);
                Require(sprite.Top+f.Bounds.Top*s>=area.Top+3.8,"Visible top clipped: "+pair.Key);
                Require(Math.Abs(sprite.Top+f.supportY*s-area.Bottom)<.01,"Support moved: "+pair.Key);
                if(f.hasBodyAnchor){FrameInfo reference=art.Manifest.frames["lay"];double actual=sprite.Left+f.bodyAnchorX*s;double expected=support.X+(reference.bodyAnchorX-reference.supportX)*reference.scale*scale;Require(Math.Abs(actual-expected)<.01,"Rear body anchor slid: "+pair.Key);bodyRegistrationCases++;}
                placements++;
            }
            int transitions=0,actions=0;double total=0,rest=0;
            for(int seed=0;seed<4;seed++)
            {
                Routine routine=new Routine(art,seed){DraftAnimations=seed%2==1};
                for(int tick=0;tick<864000;tick++)
                {
                    string before=routine.Pose;routine.Step(.1,false);Require(Routine.ValidPose(routine.Pose),"Invalid automatic state.");
                    Require(art.Manifest.frames.ContainsKey(routine.Frame),"Unavailable automatic frame.");
                    Require(routine.Pose!="sleep"||routine.Action.Length==0||routine.Action=="sleep-to-lay","Sleep expressed.");
                    if(routine.Action.Length==0)rest+=.1;total+=.1;if(before!=routine.Pose)transitions++;
                }
                foreach(int count in routine.Counts.Values)actions+=count;
            }
            Require(rest/total>.90&&transitions>30&&actions>100,"Quiet routine coverage failed.");
            Routine test=new Routine(art,71){SleepTransitions=false,DraftAnimations=false};test.SetPose("sleep",true);Require(!test.TryAction("tilt")&&!test.TryAction("smile")&&!test.TryAction("lick"),"Sleep accepted action.");
            test.ObserveHover();Require(test.Action=="","Sleep acknowledged hover.");test.Paused=true;double clock=test.Clock;test.Step(.1,false);Require(test.Clock==clock,"Pause advanced clock.");
            test.Paused=false;test.Step(.1,true);Require(test.Clock==clock,"Menu/drag suppression advanced clock.");test.SetPose("lay",true);Require(test.TryAction("tilt"),"Awake action routing failed.");
            for(int i=0;i<100&&test.Action.Length>0;i++)test.Step(.1,false);Require(test.Pose=="lay"&&test.Action=="","Lying action did not return to lying.");Require(!test.TryAction("missing"),"Unknown action accepted.");
            test.SetPose("sit",true);Require(test.TryAction("sit-blink"),"Approved sitting blink unavailable by default.");
            for(int i=0;i<10;i++)test.Step(.1,false);Require(test.Action==""&&test.Frame=="sit","Sitting blink did not return to approved base.");
            Routine sitting=new Routine(art,91){Automatic=false};for(int i=0;i<1200;i++)sitting.Step(.1,false);
            Require(sitting.Counts.ContainsKey("sit-blink")&&sitting.Counts["sit-blink"]>2,"Automatic sitting blink missing without draft flag.");
            test.SetPose("lay",true);Require(test.Action=="","Disabled sit/lie transition played.");
            test.SetPose("sit",true);test.DraftAnimations=true;test.SetPose("lay",true);Require(test.Action=="sit-to-lay","Draft transition did not begin when enabled.");for(int i=0;i<10;i++)test.Step(.1,false);Require(test.Pose=="lay"&&test.Action=="","Sit/lie transition failed.");test.SetPose("sleep",true);test.SetPose("lay",true);Require(test.Pose=="lay","Wake did not enter awake lying.");
            using(Bitmap hit=new Bitmap(3,3)){hit.SetPixel(1,1,Color.White);Require(LayeredSpriteWindow.IsOpaque(hit,new Point(1,1))&&!LayeredSpriteWindow.IsOpaque(hit,new Point(0,0))&&!LayeredSpriteWindow.IsOpaque(hit,new Point(-1,1)),"Alpha hit test failed.");}
            Order();Require(Program.Singleton=="Local\\"+Program.SettingsName&&Preferences.Folder.EndsWith(Program.SettingsName),"Identity isolation failed.");
            test.DraftAnimations=false;double minimumHold=8,maximumHold=2;int awakeActions=0;
            foreach(string pose in new[]{"sit","lay"})foreach(string action in new[]{"blink","tilt","smile","lick"})for(int repeat=0;repeat<20;repeat++)
            {
                test.SetPose(pose,true);Require(test.TryAction(action),"Awake action unavailable: "+pose+" / "+action);double sampled=test.PeakHold;
                if(action=="tilt"||action=="smile"){Require(sampled>=2&&sampled<=8,"Peak hold outside owner range.");minimumHold=Math.Min(minimumHold,sampled);maximumHold=Math.Max(maximumHold,sampled);}
                if(action=="lick")Require(Math.Abs(test.ActionDuration-.60)<.00001,"Nose lick was not slowed to 0.6 seconds.");
                for(int tick=0;tick<100&&test.Action.Length>0;tick++){Require(test.PeakHold==sampled,"Hold duration resampled during action.");test.Step(.1,false);}
                Require(test.Pose==pose&&test.Action==""&&test.Frame==pose,"Action changed awake body pose or failed return.");awakeActions++;
            }
            Require(minimumHold<3&&maximumHold>7,"Hold range is not varying across invocations.");
            int sleepPaths=CheckSleepPaths(art);
            Routine normal=new Routine(art,5){Automatic=false};normal.SetPose("lay",true);Require(normal.Action=="sit-to-lay","Normal sitting-to-lying transition missing.");for(int i=0;i<20&&normal.InTransition;i++)normal.Step(.1,false);normal.SetPose("sit",true);Require(normal.Action=="lay-to-sit","Normal lying-to-sitting transition missing.");
            var report=new{passed=true,allAwakeActionCases=awakeActions,sleepTransitionPaths=sleepPaths,observedMinimumPeakHold=minimumHold,observedMaximumPeakHold=maximumHold,holdsSampledOnce=true,noseLickSeconds=.60,version="0.1.7",embeddedFrames=checkedFrames,geometryPlacements=placements,stationaryRearBodyPlacements=bodyRegistrationCases,simulatedHours=96,restFraction=rest/total,stateChanges=transitions,actions=actions,approvedSitBlinkEnabledByDefault=true,automaticSitBlinksInTwoMinutes=sitting.Counts["sit-blink"],
                approvedBodiesAndBlinkAlphaUnchanged=true,sleepBlocksExpressions=true,sitLieTransitionsEnabledByDefault=true,sleepTransitionPreviewEnabledByDefault=true,pauseAndMenuDragSuppression=true,wakeToAwakeLying=true,cacheLimit=Assets.CacheLimit,
                taskbarOrderCases=8,alphaHitTest=true,identityIsolated=true,uiOpened=false,registryWritten=false,networkUsed=false,manualReviewRequired="Layered window, dragging, tray/menu, mixed-DPI monitors and candidate sit/lie/sleep transitions"};
            File.WriteAllText(path,new JavaScriptSerializer().Serialize(report));
        }
        static int CheckSleepPaths(Assets art)
        {
            int cases=0;
            foreach(bool sitTransitions in new[]{false,true})foreach(string start in new[]{"sit","lay","sleep"})foreach(string end in new[]{"sit","lay","sleep"})
            {
                Routine r=new Routine(art,23){Automatic=false,DraftAnimations=sitTransitions};r.SleepTransitions=false;r.SetPose(start,true);for(int i=0;i<20&&r.InTransition;i++)r.Step(.1,false);r.SleepTransitions=true;
                r.SetPose(end,true);List<string> visited=new List<string>();
                for(int i=0;i<60&&r.InTransition;i++)
                {
                    if(!visited.Contains(r.Action))visited.Add(r.Action);
                    Require(!r.TryAction("tilt",true)&&!r.TryAction("blink",true),"Expression interrupted a pose transition.");
                    string key=r.Frame;double age=r.Age;r.Step(.1,true);Require(r.Frame==key&&r.Age==age,"Menu/drag advanced sleep motion.");r.Paused=true;r.Step(.1,false);Require(r.Frame==key&&r.Age==age,"Pause advanced sleep motion.");r.Paused=false;r.Step(.1,false);
                }
                Require(r.Pose==end&&r.Frame==end&&!r.InTransition&&r.Action=="","Pose route failed: "+start+" -> "+end);
                if(start!="sleep"&&end=="sleep")Require(visited.Contains("lay-to-sleep"),"Skipped head lowering.");
                if(start=="sleep"&&end!="sleep")Require(visited.Contains("sleep-to-lay"),"Skipped waking.");
                if(sitTransitions&&start=="sit"&&end=="sleep")Require(visited.Contains("sit-to-lay"),"Sleep skipped sit/lie motion.");
                if(sitTransitions&&start=="sleep"&&end=="sit")Require(visited.Contains("lay-to-sit"),"Wake skipped lie/sit motion.");cases++;
            }
            Routine interrupted=new Routine(art,9){Automatic=false};interrupted.SetPose("lay",true);interrupted.SetPose("sleep",true);interrupted.Step(.1,false);interrupted.SetPose("lay",true);
            for(int i=0;i<30;i++)interrupted.Step(.1,false);Require(interrupted.Pose=="lay"&&!interrupted.InTransition,"Cancelled sleep retained a stale destination.");cases++;
            interrupted.SetPose("sleep",true);for(int i=0;i<20;i++)interrupted.Step(.1,false);interrupted.Automatic=true;for(int i=0;i<2500&&interrupted.Action!="sleep-to-lay";i++)interrupted.Step(.1,false);
            Require(interrupted.Action=="sleep-to-lay","Automatic waking bypassed transition.");for(int i=0;i<20;i++)interrupted.Step(.1,false);Require(interrupted.Pose=="lay","Automatic waking did not return to lying.");cases++;
            return cases;
        }
        public static void RenderPoseCycle(Assets art,string directory)
        {
            Directory.CreateDirectory(directory);Placement placement=new Placement(art);List<string> keys=new List<string>();List<int> delays=new List<int>();
            Action<string,int> hold=delegate(string key,int delay){keys.Add(key);delays.Add(delay);};
            Action<string> clip=delegate(string name){ClipInfo c=art.Manifest.clips[name];for(int i=0;i<c.frames.Length;i++)hold(c.frames[i],(int)Math.Round(c.seconds[i]*100));};
            hold("sit",100);clip("sit-to-lay");hold("lay",100);clip("lay-to-sleep");hold("sleep",150);clip("sleep-to-lay");hold("lay",100);clip("lay-to-sit");hold("sit",100);
            for(int i=0;i<keys.Count;i++)using(Bitmap output=new Bitmap(800,520))using(Graphics g=Graphics.FromImage(output))
            using(Font font=new Font("Segoe UI",14))using(Brush text=new SolidBrush(Color.White))using(Pen line=new Pen(Color.SteelBlue))
            {
                g.Clear(Color.FromArgb(25,30,38));g.DrawString("The Judge | sit / lie / sleep / lie / sit",font,text,20,20);
                Point location;using(Bitmap scene=placement.Paint(keys[i],new PointF(260,460),.30,out location))g.DrawImageUnscaled(scene,location);
                g.DrawLine(line,20,460,780,460);g.DrawString("Restored sit/lie motion; corrected body registration for sleep",font,text,20,485);
                output.Save(Path.Combine(directory,i.ToString("00")+".png"),ImageFormat.Png);
            }
            File.WriteAllText(Path.Combine(directory,"delays.json"),new JavaScriptSerializer().Serialize(delays));
        }
        public static void RenderSleepTransitions(Assets art,string directory)
        {
            Directory.CreateDirectory(directory);Placement placement=new Placement(art);
            string[] keys={"lay","lay-blink-1","lay-sleep-nod","lay-sleep-early","lay-sleep-mid","lay-sleep-late","sleep","lay-sleep-late","lay-sleep-mid","lay-sleep-early","lay-sleep-nod","lay-blink-1","lay"};
            for(int i=0;i<keys.Length;i++)using(Bitmap output=new Bitmap(800,520))using(Graphics g=Graphics.FromImage(output))
            using(Font font=new Font("Segoe UI",14))using(Brush text=new SolidBrush(Color.White))using(Pen line=new Pen(Color.SteelBlue))
            {
                g.Clear(Color.FromArgb(25,30,38));g.DrawString("The Judge | lying awake / sleep transition candidate",font,text,20,20);
                Point location;using(Bitmap scene=placement.Paint(keys[i],new PointF(260,460),.30,out location))g.DrawImageUnscaled(scene,location);
                g.DrawLine(line,20,460,780,460);g.DrawString(i<=6?"Settling down":"Waking up",font,text,20,485);
                output.Save(Path.Combine(directory,i.ToString("00")+".png"),ImageFormat.Png);
            }
            string[] stages={"lay","lay-sleep-nod","lay-sleep-early","lay-sleep-mid","lay-sleep-late","sleep"};
            using(Bitmap output=new Bitmap(2160,520))using(Graphics g=Graphics.FromImage(output))using(Font font=new Font("Segoe UI",14))using(Brush text=new SolidBrush(Color.White))using(Pen line=new Pen(Color.SteelBlue))
            {
                g.Clear(Color.FromArgb(25,30,38));for(int i=0;i<stages.Length;i++)
                {g.DrawString(new[]{"Awake","Nodding","Lowering","Halfway","Nearly resting","Asleep"}[i],font,text,i*360+15,20);Point location;using(Bitmap scene=placement.Paint(stages[i],new PointF(i*360+110,460),.21,out location))g.DrawImageUnscaled(scene,location);g.DrawLine(line,i*360+10,460,i*360+350,460);}
                output.Save(Path.Combine(directory,"stages.png"),ImageFormat.Png);
            }
        }
        public static void RenderAwakeActions(Assets art,string directory)
        {
            Directory.CreateDirectory(directory);double[] samples={-1,.001,.121,.181,.421,.601,3.121,3.241};Placement placement=new Placement(art);
            for(int index=0;index<=samples.Length;index++)
            using(Bitmap output=new Bitmap(1500,1040))using(Graphics g=Graphics.FromImage(output))
            using(Font title=new Font("Segoe UI",18,FontStyle.Bold))using(Font font=new Font("Segoe UI",13))using(Brush text=new SolidBrush(Color.White))using(Pen line=new Pen(Color.SteelBlue))
            {
                double time=index==samples.Length?.24:samples[index];g.Clear(Color.FromArgb(25,30,38));g.DrawString("The Judge | actions in each awake state",title,text,20,20);
                for(int row=0;row<2;row++)for(int column=0;column<3;column++)
                {
                    string pose=row==0?"sit":"lay",action=new[]{"tilt","smile","lick"}[column];string clipKey=row==0?action:"lay-"+action;ClipInfo clip=art.Manifest.clips[clipKey];
                    string key=time<0||time>=clip.DurationWithHold(3)?pose:clip.At(time,3);
                    g.DrawString((row==0?"Sitting":"Lying awake")+" | "+new[]{"Head tilt","Cheesy smile","Nose lick"}[column],font,text,column*500+20,row*500+75);
                    Point location;using(Bitmap scene=placement.Paint(key,new PointF(column*500+130,row*500+480),.30,out location))g.DrawImageUnscaled(scene,location);
                    g.DrawLine(line,column*500+10,row*500+480,column*500+490,row*500+480);
                }
                g.DrawString("Review shows a 3-second hold; runtime chooses 2-8 seconds once per tilt/smile. Nose lick: 0.60 seconds.",font,text,20,1010);
                output.Save(Path.Combine(directory,index==samples.Length?"peaks.png":index.ToString("00")+".png"),ImageFormat.Png);
            }
        }
        public static void RenderTransitions(Assets art,string directory)
        {
            Directory.CreateDirectory(directory);Placement placement=new Placement(art);
            string[] keys={"sit","sit-lay-early","sit-lay-mid","sit-lay-late","lay","sit-lay-late","sit-lay-mid","sit-lay-early","sit"};
            for(int i=0;i<keys.Length;i++)
            using(Bitmap output=new Bitmap(800,520))using(Graphics g=Graphics.FromImage(output))
            using(Font font=new Font("Segoe UI",14))using(Brush text=new SolidBrush(Color.White))using(Pen line=new Pen(Color.SteelBlue))
            {
                g.Clear(Color.FromArgb(25,30,38));g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;
                g.DrawString("The Judge | sit / lie transition candidate",font,text,20,20);
                Point location;using(Bitmap scene=placement.Paint(keys[i],new PointF(260,460),.30,out location))g.DrawImageUnscaled(scene,location);
                g.DrawLine(line,20,460,780,460);
                g.DrawString("Actual runtime frames, registration and 120 ms transition steps",font,text,20,485);
                output.Save(Path.Combine(directory,i.ToString("00")+".png"),ImageFormat.Png);
            }
        }
        public static void Render(Assets art,string directory)
        {
            Directory.CreateDirectory(directory);Placement placement=new Placement(art);
            string[][] groups={new[]{"sit","lay","sleep"},new[]{"sit","sit-lay-early","sit-lay-mid","sit-lay-late","lay"},new[]{"sit","sit-blink-half","sit-blink-closed"}};
            string[] names={"approved-poses.png","draft-transition.png","draft-sit-blink.png"};
            for(int n=0;n<groups.Length;n++)
            {
                int cell=n==1?360:600;double scale=n==1?.22:.30;int width=cell*groups[n].Length;
                using(Bitmap output=new Bitmap(width,620))using(Graphics g=Graphics.FromImage(output))
                using(Font font=new Font("Segoe UI",15))using(Brush text=new SolidBrush(Color.White))using(Pen line=new Pen(Color.SteelBlue))
                {
                    g.Clear(Color.FromArgb(25,30,38));g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawLine(line,15,550,width-15,550);
                    for(int i=0;i<groups[n].Length;i++)
                    {string key=groups[n][i];g.DrawString((n==0?"APPROVED: ":"DRAFT: ")+key,font,text,i*cell+15,25);RectangleF target=placement.Sprite(key,new PointF(i*cell+(n==1?115:180),550),scale);g.DrawImage(art.Get(key),target);}
                    output.Save(Path.Combine(directory,names[n]),ImageFormat.Png);
                }
            }
        }
    }
}
