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
                Require(Math.Abs(sprite.Top+placement.FloorY(pair.Key)*s-area.Bottom)<.01,"Body floor anchor moved: "+pair.Key);
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
            int gazeCases = CheckGaze(art);
            int bodyFloorCases = CheckBodyFloorRendering(art);
            int seatedGazeBodyRenders = CheckWatchBodyRendering(art,"sit",660,new[]{new[]{260,350},new[]{556,642},new[]{708,801},new[]{462,530}});
            int lyingGazeBodyRenders = CheckWatchBodyRendering(art,"lay",760,new[]{new[]{125,171},new[]{270,350},
                new[]{555,607},new[]{679,735},new[]{800,866},new[]{970,1090,2}});
            var report=new{passed=true,allAwakeActionCases=awakeActions,sleepTransitionPaths=sleepPaths,observedMinimumPeakHold=minimumHold,observedMaximumPeakHold=maximumHold,holdsSampledOnce=true,noseLickSeconds=.60,version=Program.Updater.CurrentVersion,embeddedFrames=checkedFrames,geometryPlacements=placements,stationaryRearBodyPlacements=bodyRegistrationCases,gazeCases=gazeCases,bodyFloorCases=bodyFloorCases,seatedGazeBodyRenders=seatedGazeBodyRenders,lyingGazeBodyRenders=lyingGazeBodyRenders,simulatedHours=96,restFraction=rest/total,stateChanges=transitions,actions=actions,approvedSitBlinkEnabledByDefault=true,automaticSitBlinksInTwoMinutes=sitting.Counts["sit-blink"],
                approvedBodiesAndBlinkAlphaUnchanged=true,sleepBlocksExpressions=true,sitLieTransitionsEnabledByDefault=true,sleepTransitionPreviewEnabledByDefault=true,pauseAndMenuDragSuppression=true,wakeToAwakeLying=true,cacheLimit=Assets.CacheLimit,
                taskbarOrderCases=8,alphaHitTest=true,identityIsolated=true,uiOpened=false,registryWritten=false,networkUsed=false,manualReviewRequired="Layered window, dragging, tray/menu, mixed-DPI monitors and candidate sit/lie/sleep transitions"};
            File.WriteAllText(path,new JavaScriptSerializer().Serialize(report));
        }
        static int CheckBodyFloorRendering(Assets art)
        {
            var placement=new Placement(art);
            var floor=new PointF(-540.65f,900);
            int cases=0;
            foreach(string key in art.Manifest.frames.Keys)
            {
                string basis=Placement.GazePose(key);
                FrameInfo frame=art.Manifest.frames[basis];
                Require(frame.bodyFloorY.HasValue&&frame.bodyFloorBand!=null&&frame.bodyFloorBand.Length==2,
                    "Missing measured body floor for "+key);
                foreach(double scale in new[]{.24,.30,.36})
                {
                    RectangleF sprite=placement.Sprite(basis,floor,scale);
                    double nativeScale=frame.scale*scale;
                    int left=(int)Math.Ceiling(sprite.Left+frame.bodyFloorBand[0]*nativeScale);
                    int right=(int)Math.Floor(sprite.Left+frame.bodyFloorBand[1]*nativeScale);
                    Point origin;
                    using(Bitmap rendered=placement.Paint(key,floor,scale,out origin))
                    {
                        byte[] pixels=Pixels(rendered);
                        int row=RenderedUnderside(pixels,rendered.Width,rendered.Height,left-origin.X,right-origin.X,165,.50);
                        Require(Math.Abs(row+origin.Y-floor.Y)<=2,"Body is above or below the floor: "+key+" at "+scale);
                        int overhangRow=(int)floor.Y+7-origin.Y;
                        Require(overhangRow>=0&&overhangRow<rendered.Height,"Overhang is outside the renderer: "+key);
                        int covered=0;
                        for(int x=0;x<rendered.Width;x++)
                            if(pixels[(overhangRow*rendered.Width+x)*4+3]>165)covered++;
                        Require(covered>=4,"Visible overhang was clipped: "+key);
                    }
                    cases++;
                }
            }
            return cases;
        }
        static byte[] RenderWatchBody(Placement placement, string key, PointF support, double scale, Rectangle viewport)
        {
            Point location;
            using (Bitmap rendered = placement.Paint(key,support,scale,out location))
            using (Bitmap body = new Bitmap(viewport.Width,viewport.Height,PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(body))
                    graphics.DrawImageUnscaled(rendered,location.X-viewport.X,location.Y-viewport.Y);
                return Pixels(body);
            }
        }
        static int RenderedUnderside(byte[] pixels, int width, int height, int left, int right, int alpha=64, double quantile=.80)
        {
            var bottoms = new List<int>();
            for(int x=left;x<right;x++)
                for(int y=height-1;y>=0;y--)
                    if(pixels[(y*width+x)*4+3]>alpha)
                    {
                        bottoms.Add(y);
                        break;
                    }
            Require(bottoms.Count>=right-left-2,"Watch contact area is missing.");
            bottoms.Sort();
            return bottoms[(int)(quantile*(bottoms.Count-1))];
        }
        static int CheckWatchBodyRendering(Assets art, string pose, int bodyStart, int[][] regions)
        {
            var placement = new Placement(art);
            var support = new PointF(-540.65f,900);
            int count = 0;
            foreach (double scale in new[] {.24,.30,.36})
            {
                RectangleF sprite = placement.Sprite(pose,support,scale);
                double nativeScale = art.Manifest.frames[pose].scale*scale;
                var viewport = new Rectangle((int)Math.Floor(sprite.Left)-4,
                    (int)Math.Ceiling(sprite.Top+bodyStart*nativeScale),
                    (int)Math.Ceiling(sprite.Width)+8,
                    (int)Math.Ceiling(sprite.Height-bodyStart*nativeScale)+8);
                byte[] neutral = RenderWatchBody(placement,pose,support,scale,viewport);
                for (int sector = 0; sector < 12; sector++)
                {
                    string key = WatchGaze.Key(pose,sector);
                    byte[] watch = RenderWatchBody(placement,key,support,scale,viewport);
                    int overlap=0,union=0;
                    for(int i=3;i<neutral.Length;i+=4)
                    {
                        bool baseline=neutral[i]>96, candidate=watch[i]>96;
                        if(baseline||candidate)union++;
                        if(baseline&&candidate)overlap++;
                    }
                    Require(overlap>=union*.96,"Body silhouette changed in the desktop renderer: "+key+" at "+scale);
                    foreach(var region in regions)
                    {
                        int left=(int)Math.Ceiling(sprite.Left+region[0]*nativeScale)-viewport.X;
                        int right=(int)Math.Floor(sprite.Left+region[1]*nativeScale)-viewport.X;
                        int a=RenderedUnderside(neutral,viewport.Width,viewport.Height,left,right);
                        int b=RenderedUnderside(watch,viewport.Width,viewport.Height,left,right);
                        int tolerance=region.Length==3?region[2]:1;
                        Require(Math.Abs(a-b)<=tolerance,"Body contact changed in the desktop renderer: "+key+" at "+scale+" near native x="+region[0]);
                    }
                    count++;
                }
            }
            return count;
        }
        static int CheckGaze(Assets art)
        {
            var gaze = new WatchGaze();
            PointF head = new PointF(-730, 480);
            foreach (string pose in new[] {"sit", "lay"})
                foreach (WatchMode mode in new[] {WatchMode.Mouse, WatchMode.Window})
                    for (int sector = 0; sector < 12; sector++)
                    {
                        gaze.Toggle(mode);
                        double angle = sector * Math.PI / 6;
                        PointF target = new PointF(head.X + (float)(300 * Math.Sin(angle)), head.Y - (float)(300 * Math.Cos(angle)));
                        gaze.Step(0, pose, pose, head, target, 12);
                        string key = gaze.Frame(pose, pose);
                        Require(key == WatchGaze.Key(pose, sector) && art.Manifest.frames.ContainsKey(key), "Gaze sector failed.");
                        gaze.Toggle(mode);
                    }

            gaze.Toggle(WatchMode.Mouse);
            gaze.Step(0, "sit", "sit", head, new PointF(head.X - 150, head.Y - 260), 12);
            gaze.Step(.10, "sit", "sit", head, new PointF(head.X, head.Y - 300), 12);
            Require(gaze.Frame("sit", "sit") == "sit-look-12", "Clock wrap failed.");
            gaze.Step(.20, "sit", "sit", head, new PointF(head.X + 86, head.Y - 290), 12);
            Require(gaze.Frame("sit", "sit") == "sit-look-12", "Clock boundary jittered.");
            gaze.Step(.30, "sit", "sit", head, new PointF(head.X - 300, head.Y), 12);
            Require(gaze.Frame("sit", "sit") == "sit-look-11", "Clock did not start shortest turn.");
            gaze.Step(.31, "sit", "sit", head, new PointF(head.X - 300, head.Y), 12);
            Require(gaze.Frame("sit", "sit") == "sit-look-11", "Clock step interval was skipped.");
            gaze.Step(.40, "sit", "sit", head, head, 12);
            Require(gaze.Frame("sit", "sit") == "sit", "Pointer dead zone failed.");
            gaze.Step(.50, "lay", "lay", head, new PointF(head.X, head.Y + 300), 12);
            Require(gaze.Frame("lay", "lay") == "lay-look-06", "Awake pose switch missed target.");
            gaze.Step(.60, "lay", "lay-blink-1", head, head, 12);
            Require(gaze.Frame("lay", "lay-blink-1") == "lay-blink-1" && gaze.Frame("lay", "lay") == "lay", "Gaze overrode expression.");
            gaze.Step(.70, "sleep", "sleep", head, new PointF(head.X, head.Y - 300), 12);
            Require(gaze.Frame("sleep", "sleep") == "sleep", "Gaze overrode sleep.");
            gaze.Toggle(WatchMode.Window);
            PointF? window = WatchedWindow.Target(new Rectangle(-900, -100, 500, 600), "TestWindow");
            Require(window == new PointF(-650, -20) && !WatchedWindow.Target(new Rectangle(-900, -100, 500, 600), "Shell_TrayWnd").HasValue
                && !WatchedWindow.Read(IntPtr.Zero, 0).HasValue, "Foreground target failed.");
            gaze.Step(.80, "lay", "lay", head, window, 12);
            Require(gaze.Frame("lay", "lay") != "lay", "Window target did not select a key.");
            gaze.Step(.90, "lay", "lay", head, null, 12);
            Require(gaze.Frame("lay", "lay") == "lay", "Missing foreground window did not return to neutral.");
            var sprite = new RectangleF(-1000, 200, 627, 627);
            Require(WatchGaze.HeadOnScreen("sit", sprite, new Size(1254, 1254)) == new PointF(-741, 357.5f), "Head anchor did not follow sprite scale and position.");
            return 60;
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
