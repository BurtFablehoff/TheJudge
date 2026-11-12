using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;
using PetFramework;

namespace Judge
{
    internal sealed class Preferences
    {
        [ScriptIgnore]public bool Ephemeral;
        public double Size=.24;public bool Expressions=true,AutoUpdates=true,StartWithWindows;public string Monitor="";public double Fraction=.65;
        internal static string Folder{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),Program.SettingsName);}}
        internal static Preferences Load()
        {
            try{Preferences p=new JavaScriptSerializer().Deserialize<Preferences>(File.ReadAllText(Path.Combine(Folder,"settings.json")));
                if(p==null)return new Preferences();if(p.Size!=.24&&p.Size!=.30&&p.Size!=.36)p.Size=.24;
                if(Double.IsNaN(p.Fraction)||Double.IsInfinity(p.Fraction))p.Fraction=.65;p.Fraction=Math.Max(0,Math.Min(1,p.Fraction));return p;
            }catch(IOException){return new Preferences();}catch(ArgumentException){return new Preferences();}catch(InvalidOperationException){return new Preferences();}
        }
        public void Save()
        {
            if(Ephemeral)return;
            try
            {
                Directory.CreateDirectory(Folder);string target=Path.Combine(Folder,"settings.json"),temp=target+".tmp";
                File.WriteAllText(temp,new JavaScriptSerializer().Serialize(this));
                if(File.Exists(target))File.Replace(temp,target,null);else File.Move(temp,target);
            }catch(IOException){}catch(UnauthorizedAccessException){}
        }
    }
    internal sealed class CompanionWindow:LayeredSpriteWindow
    {
        internal static string Title{get{return Program.Development?"The Judge - Development 0.1.7":"The Judge";}}
        readonly Assets art;readonly Placement placement;readonly Routine routine;readonly Preferences preferences;
        readonly Timer timer=new Timer();readonly Stopwatch clock=Stopwatch.StartNew();
        readonly UpdateCoordinator updates;
        readonly ContextMenuStrip menu=new ContextMenuStrip();readonly NotifyIcon tray=new NotifyIcon();
        Icon trayIcon;Rectangle area;double x,lastTick,orderAt,hoverSince=-1;bool inMenu,dismissedPress;double dragX,suppressMenuUntil;int menuButtons;readonly PointerGesture gesture=new PointerGesture();ToolStripMenuItem actionMenu;
        string drawn="";Point drawnPoint;double drawnScale=-1;readonly uint process=(uint)Process.GetCurrentProcess().Id;
        [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr icon);
        [DllImport("user32.dll")]static extern bool SetForegroundWindow(IntPtr window);
        [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
        IntPtr previousForeground;
        public CompanionWindow(Assets assets,bool headless=false)
        {
            art=assets;placement=new Placement(art);preferences=headless?new Preferences{Ephemeral=true}:Preferences.Load();routine=new Routine(art,Environment.TickCount){Expressions=preferences.Expressions};
            Screen screen=Screen.FromPoint(Cursor.Position);foreach(Screen candidate in Screen.AllScreens)if(candidate.DeviceName==preferences.Monitor)screen=candidate;
            area=screen.WorkingArea;x=placement.Clamp(area,area.Left+area.Width*preferences.Fraction,preferences.Size);
            if(!headless&&!Program.Development)updates=new UpdateCoordinator(Program.Updater,Application.ExecutablePath);
            Text=Title;BuildMenu();menu.AutoClose=true;
            menu.Opening+=delegate{inMenu=true;previousForeground=GetForegroundWindow();menuButtons=MouseButtonsDown();UpdateActions();};
            menu.Opened+=delegate{SetForegroundWindow(menu.Handle);menu.Focus();};
            menu.Closed+=delegate(object sender,ToolStripDropDownClosedEventArgs e){inMenu=false;if(e.CloseReason==ToolStripDropDownCloseReason.AppClicked||e.CloseReason==ToolStripDropDownCloseReason.AppFocusChange){suppressMenuUntil=clock.Elapsed.TotalSeconds+.25;gesture.Cancel();Capture=false;}IntPtr current=GetForegroundWindow();bool restore=e.CloseReason==ToolStripDropDownCloseReason.Keyboard||e.CloseReason==ToolStripDropDownCloseReason.ItemClicked;if(restore&&previousForeground!=IntPtr.Zero&&(current==menu.Handle||current==Handle||current==IntPtr.Zero))SetForegroundWindow(previousForeground);};
            menu.KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Escape){menu.Close(ToolStripDropDownCloseReason.Keyboard);e.Handled=true;}};
            using(Bitmap icon=new Bitmap(32,32))
            {
                using(Graphics g=Graphics.FromImage(icon)){g.DrawImage(art.Get("sit"),new Rectangle(0,0,32,32),new Rectangle(180,0,760,760),GraphicsUnit.Pixel);}
                IntPtr handle=icon.GetHicon();try{using(Icon original=Icon.FromHandle(handle))trayIcon=(Icon)original.Clone();}finally{DestroyIcon(handle);}
            }
            tray.Icon=trayIcon;tray.Text="The Judge 0.1.7"+(Program.Development?" (development)":"");tray.ContextMenuStrip=menu;tray.Visible=!headless;
            tray.DoubleClick+=delegate{BringHere();Show();drawn="";PaintFrame();};
            MouseDown+=PointerDown;MouseMove+=PointerMove;MouseUp+=PointerUp;MouseCaptureChanged+=delegate{if(!Capture)gesture.Cancel();};
            MouseEnter+=delegate{hoverSince=routine.Clock;};MouseLeave+=delegate{hoverSince=-1;};
            timer.Interval=16;timer.Tick+=Tick;
            Shown+=delegate{lastTick=clock.Elapsed.TotalSeconds;PaintFrame();if(!Program.Development){ApplyStartupPreference();Program.Updater.MarkHealthy();}timer.Start();};
            if(!headless)SystemEvents.DisplaySettingsChanged+=DisplayChanged;
        }
        ToolStripMenuItem Add(ToolStripItemCollection parent,string text,Action click)
        {ToolStripMenuItem item=new ToolStripMenuItem(text);item.Click+=delegate{click();};parent.Add(item);return item;}
        void BuildMenu()
        {
            if(!Program.Development){BuildProductionMenu();return;}
            ToolStripMenuItem states=new ToolStripMenuItem("State");menu.Items.Add(states);
            ToolStripMenuItem automatic=Add(states.DropDownItems,"Automatic",delegate{routine.Automatic=true;routine.SetPose(routine.Pose,false);});
            menu.Opening+=delegate{automatic.Checked=routine.Automatic;};
            string[] poses={"sit","lay","sleep"},poseNames={"Sitting","Lying awake","Sleeping"};
            for(int i=0;i<poses.Length;i++)
            {string pose=poses[i];ToolStripMenuItem item=Add(states.DropDownItems,poseNames[i],delegate{routine.SetPose(pose,true);});menu.Opening+=delegate{item.Checked=routine.Pose==pose;};}
            states.DropDownItems.Add(new ToolStripSeparator());Add(states.DropDownItems,"Review poses and actions",delegate{new ReviewWindow(art).Show();});
            menu.Items.Add(new ToolStripSeparator());actionMenu=new ToolStripMenuItem("Actions (in current state)");menu.Items.Add(actionMenu);
            string[] actions={"blink","tilt","smile","lick"},labels={"Blink","Head tilt","Cheesy smile","Nose lick"};
            for(int i=0;i<actions.Length;i++)
            {string action=actions[i];ToolStripMenuItem item=Add(actionMenu.DropDownItems,labels[i],delegate{routine.TryAction(action,true);});item.Tag=action;}
            menu.Items.Add(new ToolStripSeparator());ToolStripMenuItem windows=new ToolStripMenuItem("Windows");menu.Items.Add(windows);
            ToolStripMenuItem sizes=new ToolStripMenuItem("Size");windows.DropDownItems.Add(sizes);
            string[] names={"Small","Normal","Large"};double[] values={.24,.30,.36};
            for(int i=0;i<3;i++)
            {int n=i;ToolStripMenuItem item=Add(sizes.DropDownItems,names[i],delegate{preferences.Size=values[n];x=placement.Clamp(area,x,preferences.Size);drawn="";preferences.Save();PaintFrame();});menu.Opening+=delegate{item.Checked=preferences.Size==values[n];};}
            ToolStripMenuItem pause=Add(windows.DropDownItems,"Pause",delegate{routine.Paused=!routine.Paused;});menu.Opening+=delegate{pause.Checked=routine.Paused;};
            ToolStripMenuItem expressions=Add(windows.DropDownItems,"Expressions",delegate{routine.Expressions=!routine.Expressions;preferences.Expressions=routine.Expressions;preferences.Save();});menu.Opening+=delegate{expressions.Checked=routine.Expressions;};
            ToolStripMenuItem draft=Add(windows.DropDownItems,"Sit / lie transitions",delegate{routine.DraftAnimations=!routine.DraftAnimations;routine.SetPose(routine.Pose,false);});menu.Opening+=delegate{draft.Checked=routine.DraftAnimations;};
            ToolStripMenuItem sleepMotion=Add(windows.DropDownItems,"Sleep transitions (preview)",delegate{routine.SleepTransitions=!routine.SleepTransitions;routine.SetPose(routine.Pose,false);});menu.Opening+=delegate{sleepMotion.Checked=routine.SleepTransitions;};
            ToolStripMenuItem screens=new ToolStripMenuItem("Screen");windows.DropDownItems.Add(screens);Add(screens.DropDownItems,"Bring to this screen",BringHere);
            screens.DropDownOpening+=delegate
            {
                while(screens.DropDownItems.Count>1){ToolStripItem remove=screens.DropDownItems[1];screens.DropDownItems.RemoveAt(1);remove.Dispose();}
                int index=0;foreach(Screen screen in Screen.AllScreens){Screen selected=screen;Add(screens.DropDownItems,"Screen "+(++index)+(screen.Primary?" (primary)":""),delegate{MoveScreen(selected);});}
            };
            menu.Items.Add(new ToolStripSeparator());Add(menu.Items,"Quit The Judge",Close);UpdateActions();
        }
        int requestedUpdateChecks;
        void Section(string title){menu.Items.Add(new ToolStripLabel(title){Font=new Font(menu.Font,FontStyle.Bold),ForeColor=SystemColors.GrayText});}
        void BuildProductionMenu()
        {
            ToolStripMenuItem sizes=new ToolStripMenuItem("Size");menu.Items.Add(sizes);
            string[] labels={"Small","Normal","Large"};double[] values={.24,.30,.36};
            for(int i=0;i<labels.Length;i++)
            {int n=i;ToolStripMenuItem item=Add(sizes.DropDownItems,labels[i],delegate{preferences.Size=values[n];x=placement.Clamp(area,x,preferences.Size);preferences.Save();drawn="";PaintFrame();});menu.Opening+=delegate{item.Checked=preferences.Size==values[n];};}
            menu.Items.Add(new ToolStripSeparator());Section("Options");
            ToolStripMenuItem sit=Add(menu.Items,"Sit",delegate{routine.SelectAwake("sit");}),lay=Add(menu.Items,"Lay Down",delegate{routine.SelectAwake("lay");});
            sit.Checked=routine.Pose=="sit";lay.Checked=routine.Pose=="lay";
            menu.Opening+=delegate{sit.Checked=routine.Pose=="sit";lay.Checked=routine.Pose=="lay";};
            menu.Items.Add(new ToolStripSeparator());Section("Windows");
            ToolStripMenuItem startup=Add(menu.Items,"Start with Windows",delegate{ChangeStartup();});menu.Opening+=delegate{startup.Checked=preferences.StartWithWindows;};
            startup.Checked=preferences.StartWithWindows;
            ToolStripMenuItem automatic=Add(menu.Items,"Automatic updates",delegate{preferences.AutoUpdates=!preferences.AutoUpdates;preferences.Save();});menu.Opening+=delegate{automatic.Checked=preferences.AutoUpdates;};
            automatic.Checked=preferences.AutoUpdates;
            Add(menu.Items,"Check for updates",delegate{requestedUpdateChecks++;if(updates!=null){updates.Check(true);tray.ShowBalloonTip(2000,"The Judge updates","Checking for a newer version...",ToolTipIcon.Info);}});
            menu.Items.Add(new ToolStripSeparator());Section("Screen");
            Add(menu.Items,"Bring to this screen",BringHere);Add(menu.Items,"Send to other screen",SendToOtherScreen);
            menu.Items.Add(new ToolStripSeparator());Add(menu.Items,"Close",Close);
        }
        void ChangeStartup()
        {
            bool enabled=!preferences.StartWithWindows;
            try{if(!preferences.Ephemeral)WindowsStartup.Set(Product.StartupName,enabled,Application.ExecutablePath);preferences.StartWithWindows=enabled;preferences.Save();}
            catch(Exception error){MessageBox.Show("Could not change Windows startup: "+error.Message,"The Judge",MessageBoxButtons.OK,MessageBoxIcon.Information);}
        }
        void ApplyStartupPreference()
        {
            // Opt-in via the production menu. Development/test builds never register.
            if(preferences.Ephemeral||!preferences.StartWithWindows)return;
            try{WindowsStartup.Set(Product.StartupName,true,Application.ExecutablePath);}
            catch(Exception error){tray.ShowBalloonTip(4000,"The Judge startup",error.Message,ToolTipIcon.Info);}
        }
        void SendToOtherScreen()
        {
            Screen[] screens=Screen.AllScreens;int index=Array.FindIndex(screens,delegate(Screen s){return s.WorkingArea==area;});MoveScreen(screens[(index+1+screens.Length)%screens.Length]);
        }
        void TickUpdates()
        {
            if(updates==null)return;updates.Tick(preferences.AutoUpdates);string message=updates.TakeMessage();if(message!=null)tray.ShowBalloonTip(4500,"The Judge updates",message,ToolTipIcon.Info);
            if(updates.Ready!=null&&(preferences.AutoUpdates||updates.ManualReady)&&!gesture.Pressed&&!inMenu&&routine.Action.Length==0)
            {try{if(updates.Install())Close();}catch(Exception error){tray.ShowBalloonTip(4500,"The Judge kept this version",error.Message,ToolTipIcon.Info);}}
        }
        int CheckProductionMenu()
        {
            string[] expected={"Size","-","Options","Sit","Lay Down","-","Windows","Start with Windows","Automatic updates","Check for updates","-","Screen","Bring to this screen","Send to other screen","-","Close"};
            if(menu.Items.Count!=expected.Length)throw new Exception("Production menu item count differs.");
            for(int i=0;i<expected.Length;i++)if(expected[i]=="-"?!(menu.Items[i] is ToolStripSeparator):menu.Items[i].Text!=expected[i])throw new Exception("Production menu differs at "+i);
            Func<string,ToolStripMenuItem> find=delegate(string text){foreach(ToolStripItem item in menu.Items)if(item.Text==text)return (ToolStripMenuItem)item;throw new Exception("Missing production item: "+text);};int count=1;
            foreach(string text in new[]{"Sit","Lay Down"})
            {find(text).PerformClick();if(!routine.Automatic)throw new Exception("Pose selection disabled autonomous sleep.");for(int i=0;i<30&&routine.InTransition;i++)routine.Step(.1,false);if(routine.Pose!=(text=="Sit"?"sit":"lay"))throw new Exception("Production pose failed.");count++;}
            find("Start with Windows").PerformClick();if(!preferences.StartWithWindows)throw new Exception("Startup toggle failed.");find("Start with Windows").PerformClick();if(preferences.StartWithWindows)throw new Exception("Startup toggle restore failed.");count+=2;
            find("Automatic updates").PerformClick();if(preferences.AutoUpdates)throw new Exception("Automatic updates off failed.");find("Automatic updates").PerformClick();if(!preferences.AutoUpdates)throw new Exception("Automatic updates restore failed.");count+=2;
            find("Check for updates").PerformClick();if(requestedUpdateChecks!=1||updates!=null)throw new Exception("Headless update command isolation failed.");count++;
            ToolStripMenuItem size=find("Size");foreach(ToolStripMenuItem item in size.DropDownItems)item.PerformClick();if(preferences.Size!=.36)throw new Exception("Size menu wiring failed.");count+=3;
            find("Bring to this screen").PerformClick();find("Send to other screen").PerformClick();count+=2;
            if(Visible||tray.Visible||IsHandleCreated)throw new Exception("Production menu checks opened UI.");return count;
        }
        void UpdateActions()
        {
            if(actionMenu==null)return;
            actionMenu.Enabled=!routine.Paused&&routine.Pose!="sleep"&&!routine.InTransition;
            foreach(ToolStripMenuItem item in actionMenu.DropDownItems)item.Enabled=routine.CanAction((string)item.Tag,true);
        }
        [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
        static int MouseButtonsDown()
        {int mask=0,index=0;foreach(int key in new[]{1,2,4,5,6}){if((GetAsyncKeyState(key)&0x8000)!=0)mask|=1<<index;index++;}return mask;}
        static void MenuBounds(ToolStripDropDown branch,System.Collections.Generic.List<Rectangle> result)
        {
            if(!branch.Visible)return;result.Add(branch.Bounds);
            foreach(ToolStripItem item in branch.Items){ToolStripMenuItem menuItem=item as ToolStripMenuItem;if(menuItem!=null&&menuItem.HasDropDownItems&&menuItem.DropDown.Visible)MenuBounds(menuItem.DropDown,result);}
        }
        void DismissOutside()
        {suppressMenuUntil=clock.Elapsed.TotalSeconds+.25;gesture.Cancel();Capture=false;menu.Close(ToolStripDropDownCloseReason.AppClicked);}
        void PollMenu()
        {
            if(!inMenu)return;int buttons=MouseButtonsDown();System.Collections.Generic.List<Rectangle> bounds=new System.Collections.Generic.List<Rectangle>();MenuBounds(menu,bounds);
            if(MenuInput.Dismiss(Cursor.Position,buttons,menuButtons,(GetAsyncKeyState(27)&0x8000)!=0,bounds))DismissOutside();menuButtons=buttons;
        }
        void PointerDown(object sender,MouseEventArgs e)
        {
            hoverSince=-1;
            if(inMenu||clock.Elapsed.TotalSeconds<suppressMenuUntil){dismissedPress=true;if(inMenu)DismissOutside();return;}
            if(gesture.Pressed)return;dismissedPress=false;gesture.Begin(e.Button,Cursor.Position);dragX=x;Capture=true;
        }
        void PointerMove(object sender,MouseEventArgs e)
        {
            if(!gesture.Pressed)return;gesture.Move(Cursor.Position);if(!gesture.Dragging)return;
            area=Screen.FromPoint(Cursor.Position).WorkingArea;x=placement.Clamp(area,dragX+Cursor.Position.X-gesture.Origin.X,preferences.Size);PaintFrame();
        }
        void PointerUp(object sender,MouseEventArgs e)
        {
            if(dismissedPress){dismissedPress=false;gesture.Cancel();Capture=false;return;}
            gesture.Move(Cursor.Position);PointerResult result=gesture.End(e.Button);if(result==PointerResult.None)return;Capture=false;
            if(result==PointerResult.Drag){StorePosition();return;}
            if(result==PointerResult.Click&&clock.Elapsed.TotalSeconds>=suppressMenuUntil)menu.Show(Cursor.Position);
        }
        void StorePosition()
        {
            preferences.Monitor=Screen.FromPoint(new Point((int)x,area.Top+area.Height/2)).DeviceName;
            preferences.Fraction=(x-area.Left)/Math.Max(1,area.Width);preferences.Save();
        }
        void BringHere(){MoveScreen(Screen.FromPoint(Cursor.Position));}
        void MoveScreen(Screen screen){area=screen.WorkingArea;x=placement.Clamp(area,area.Left+area.Width*.65,preferences.Size);StorePosition();drawn="";PaintFrame();}
        void DisplayChanged(object sender,EventArgs e)
        {if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)delegate{area=Screen.FromPoint(new Point((int)x,area.Top+area.Height/2)).WorkingArea;x=placement.Clamp(area,x,preferences.Size);drawn="";PaintFrame();});}
        void Tick(object sender,EventArgs args)
        {
            double now=clock.Elapsed.TotalSeconds,dt=now-lastTick;lastTick=now;
            PollMenu();routine.Step(dt,gesture.Pressed||inMenu||!Visible);
            TickUpdates();
            if(hoverSince>=0&&routine.Clock-hoverSince>.75&&!gesture.Pressed&&!inMenu){routine.ObserveHover();hoverSince=-1;}
            if(Visible)PaintFrame();
            if(Visible&&now>=orderAt&&!inMenu&&!PresentedBounds.IsEmpty){orderAt=now+.25;TaskbarOrder.Check(Handle,process,PresentedBounds);}
        }
        void PaintFrame()
        {
            if(!IsHandleCreated)return;x=placement.Clamp(area,x,preferences.Size);double scale=placement.Scale(area,preferences.Size);
            string key=routine.Frame;PointF support=new PointF((float)x,area.Bottom);Point location;
            Rectangle scene=placement.Scene(key,support,scale);
            if(key==drawn&&scene.Location==drawnPoint&&scale==drawnScale)return;
            Bitmap image=placement.Paint(key,support,scale,out location);Present(image,location);
            drawn=key;drawnPoint=location;drawnScale=scale;
        }
        internal void RenderMenu(string path)
        {
            if(!preferences.Ephemeral)throw new Exception("Menu render requires an isolated window.");IntPtr before=GetForegroundWindow();menu.PerformLayout();menu.Size=menu.GetPreferredSize(Size.Empty);
            using(Bitmap bitmap=new Bitmap(menu.Width,menu.Height)){menu.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(path,System.Drawing.Imaging.ImageFormat.Png);}
            if(Visible||tray.Visible||menu.Visible||GetForegroundWindow()!=before)throw new Exception("Menu preview changed desktop state.");
        }
        internal int CheckMenuWiring()
        {
            if(!preferences.Ephemeral||Visible||tray.Visible||IsHandleCreated)throw new Exception("Headless menu isolation failed.");if(!Program.Development)return CheckProductionMenu();int count=0;
            Func<ToolStripItemCollection,string,ToolStripMenuItem> find=delegate(ToolStripItemCollection items,string label)
            {foreach(ToolStripItem item in items)if(item.Text==label)return (ToolStripMenuItem)item;throw new Exception("Missing menu item: "+label);};
            string[] top={"State","-","Actions (in current state)","-","Windows","-","Quit The Judge"};if(menu.Items.Count!=top.Length)throw new Exception("Top-level menu layout differs.");
            for(int i=0;i<top.Length;i++)if(top[i]=="-"?!(menu.Items[i] is ToolStripSeparator):menu.Items[i].Text!=top[i])throw new Exception("Top-level menu item differs: "+i);count++;
            ToolStripItemCollection windows=find(menu.Items,"Windows").DropDownItems,states=find(menu.Items,"State").DropDownItems;
            find(find(windows,"Size").DropDownItems,"Normal").PerformClick();if(preferences.Size!=.30)throw new Exception("Size wiring failed.");count++;
            ToolStripMenuItem pause=find(windows,"Pause");pause.PerformClick();UpdateActions();if(!routine.Paused||actionMenu.Enabled)throw new Exception("Pause failed.");pause.PerformClick();count+=2;
            ToolStripMenuItem expressions=find(windows,"Expressions");expressions.PerformClick();if(routine.Expressions)throw new Exception("Expressions off failed.");expressions.PerformClick();if(!routine.Expressions)throw new Exception("Expressions restore failed.");count+=2;
            ToolStripMenuItem draft=find(windows,"Sit / lie transitions");draft.PerformClick();if(routine.DraftAnimations)throw new Exception("Sit/lie switch off failed.");draft.PerformClick();if(!routine.DraftAnimations)throw new Exception("Sit/lie switch restore failed.");count+=2;
            ToolStripMenuItem sleepMotion=find(windows,"Sleep transitions (preview)");sleepMotion.PerformClick();if(routine.SleepTransitions)throw new Exception("Sleep transitions off failed.");sleepMotion.PerformClick();if(!routine.SleepTransitions)throw new Exception("Sleep transitions restore failed.");count+=2;
            foreach(string state in new[]{"Sitting","Lying awake"})
            {
                find(states,state).PerformClick();for(int tick=0;tick<20&&routine.InTransition;tick++)routine.Step(.1,false);string expected=state=="Sitting"?"sit":"lay";
                foreach(string label in new[]{"Blink","Head tilt","Cheesy smile","Nose lick"})
                {
                    UpdateActions();ToolStripMenuItem item=find(actionMenu.DropDownItems,label);if(!item.Enabled||!actionMenu.Enabled)throw new Exception("Awake action missing: "+state+" / "+label);
                    string action=(string)item.Tag;item.PerformClick();if(routine.Action!=routine.ResolveAction(action)||routine.Pose!=expected)throw new Exception("Action left current state.");
                    for(int i=0;i<100&&routine.Action.Length>0;i++)routine.Step(.1,false);if(routine.Pose!=expected||routine.Action!="")throw new Exception("Action did not return to current state.");count++;
                }
            }
            find(states,"Sleeping").PerformClick();UpdateActions();if(actionMenu.Enabled)throw new Exception("Sleep menu allows actions.");count++;
            find(states,"Automatic").PerformClick();if(!routine.Automatic)throw new Exception("Automatic menu failed.");count++;
            find(find(windows,"Screen").DropDownItems,"Bring to this screen").PerformClick();if(area.Width<=0||area.Height<=0)throw new Exception("Screen menu failed.");count++;
            find(states,"Review poses and actions");find(menu.Items,"Quit The Judge");count+=2;
            if(Visible||tray.Visible||IsHandleCreated)throw new Exception("Menu check opened a window.");return count;
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing){SystemEvents.DisplaySettingsChanged-=DisplayChanged;timer.Stop();timer.Dispose();tray.Visible=false;tray.Dispose();menu.Dispose();if(trayIcon!=null)trayIcon.Dispose();}
            base.Dispose(disposing);
        }
    }
}
