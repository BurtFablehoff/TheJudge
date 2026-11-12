using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Judge
{
    internal sealed class ReviewWindow:Form
    {
        readonly Assets art;readonly Placement placement;readonly Routine routine;readonly Timer timer=new Timer();
        readonly FlowLayoutPanel controls=new FlowLayoutPanel();readonly Panel scene=new SmoothPanel();double nextPoseChange;
        sealed class SmoothPanel:Panel{public SmoothPanel(){DoubleBuffered=true;ResizeRedraw=true;}}
        public ReviewWindow(Assets assets)
        {
            art=assets;placement=new Placement(art);routine=new Routine(art,17){Automatic=false,DraftAnimations=true};
            Text="The Judge - runtime art review (transition candidates)";ClientSize=new Size(1050,720);MinimumSize=new Size(850,600);
            controls.Dock=DockStyle.Top;controls.Height=125;controls.Padding=new Padding(10);scene.Dock=DockStyle.Fill;Controls.Add(scene);Controls.Add(controls);
            foreach(string pose in new[]{"sit","lay","sleep"}){string selected=pose;Button b=new Button{Text=pose,Width=95};b.Click+=delegate{routine.SetPose(selected,true);};controls.Controls.Add(b);}
            foreach(string action in new[]{"tilt","smile","lick"}){string selected=action;Button b=new Button{Text=action,Width=95,Tag=action};b.Click+=delegate{routine.TryAction(selected,true);};controls.Controls.Add(b);}
            Button blink=new Button{Text="Blink",Width=95,Tag="blink"};blink.Click+=delegate{routine.TryAction("blink",true);};controls.Controls.Add(blink);
            CheckBox draft=new CheckBox{Text="Sit / lie transitions",Checked=true,AutoSize=true};draft.CheckedChanged+=delegate{routine.DraftAnimations=draft.Checked;routine.SetPose(routine.Pose,false);};controls.Controls.Add(draft);
            CheckBox pause=new CheckBox{Text="Pause",AutoSize=true};pause.CheckedChanged+=delegate{routine.Paused=pause.Checked;};controls.Controls.Add(pause);
            CheckBox loop=new CheckBox{Text="Loop sit / lie",AutoSize=true};loop.CheckedChanged+=delegate
            {
                if(loop.Checked){draft.Checked=true;routine.SetPose("sit",true);nextPoseChange=routine.Clock+1;}
            };controls.Controls.Add(loop);
            CheckBox sleepLoop=new CheckBox{Text="Loop lie / sleep",AutoSize=true};sleepLoop.CheckedChanged+=delegate
            {if(sleepLoop.Checked){loop.Checked=false;routine.SetPose("lay",true);nextPoseChange=routine.Clock+1;}};controls.Controls.Add(sleepLoop);
            loop.CheckedChanged+=delegate{if(loop.Checked)sleepLoop.Checked=false;};
            CheckBox light=new CheckBox{Text="Light background",AutoSize=true};controls.Controls.Add(light);
            scene.Paint+=delegate(object s,PaintEventArgs e)
            {
                e.Graphics.Clear(light.Checked?Color.Beige:Color.FromArgb(25,30,38));e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
                float floor=scene.Height-45;PointF support=new PointF(scene.Width*.38f,floor);
                RectangleF target=placement.Sprite(routine.Frame,support,.36);e.Graphics.DrawImage(art.Get(routine.Frame),target);
                using(Pen pen=new Pen(Color.SteelBlue))e.Graphics.DrawLine(pen,10,floor,scene.Width-10,floor);
                using(Brush text=new SolidBrush(light.Checked?Color.Black:Color.White))e.Graphics.DrawString(routine.Pose+(routine.Action.Length>0?" / "+routine.Action:""),Font,text,15,15);
            };
            timer.Interval=16;timer.Tick+=delegate
            {
                routine.Step(.016,false);
                if(loop.Checked&&!routine.Paused&&routine.Action.Length==0&&routine.Clock>=nextPoseChange)
                {routine.SetPose(routine.Pose=="sit"?"lay":"sit",true);nextPoseChange=routine.Clock+1.36;}
                if(sleepLoop.Checked&&!routine.Paused&&routine.Action.Length==0&&routine.Clock>=nextPoseChange)
                {routine.SetPose(routine.Pose=="lay"?"sleep":"lay",true);nextPoseChange=routine.Clock+2.0;}
                foreach(Control control in controls.Controls)if(control.Tag is string)control.Enabled=routine.CanAction((string)control.Tag,true);
                scene.Invalidate();
            };timer.Start();
        }
        protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
    }
}
