using System;
using System.Collections.Generic;

namespace Judge
{
    // Deliberately small state table; missing Odin states cannot be selected.
    internal sealed class Routine
    {
        readonly Assets art;readonly Random random;
        public string Pose="sit",Action="";string afterPose="",destination="";
        public bool SleepTransitions=true;
        public bool InTransition{get{return afterPose.Length>0;}}
        public bool Paused,Automatic=true,DraftAnimations=true,Expressions=true;
        public double Clock,Age;double poseDue,expressionDue,blinkDue,attentionDue;
        public readonly Dictionary<string,int> Counts=new Dictionary<string,int>();
        public double PeakHold{get;private set;}
        public double ActionDuration{get{return Action.Length==0?0:art.Manifest.clips[Action].DurationWithHold(PeakHold);}}
        public string Frame{get{return Action.Length==0?Pose:art.Manifest.clips[Action].At(Age,PeakHold);}}
        public Routine(Assets assets,int seed){art=assets;random=new Random(seed);Schedule();}
        double Range(double a,double b){return a+random.NextDouble()*(b-a);}
        void Schedule(){poseDue=Clock+Range(90,220);expressionDue=Clock+Range(45,100);blinkDue=Clock+Range(9,26);}
        public static bool ValidPose(string pose){return pose=="sit"||pose=="lay"||pose=="sleep";}
        public void SelectAwake(string pose)
        {
            if(pose!="sit"&&pose!="lay")throw new ArgumentException("Choose sitting or lying awake.");
            SetPose(pose,false);Automatic=true;
        }
        void Begin(string action)
        {
            if(!art.Manifest.clips.ContainsKey(action))throw new ArgumentException("Unknown action.");
            Action=action;Age=0;ClipInfo clip=art.Manifest.clips[action];PeakHold=clip.peakHoldMax>0?Range(clip.peakHoldMin,clip.peakHoldMax):0;
            Counts[action]=Counts.ContainsKey(action)?Counts[action]+1:1;
        }
        public void SetPose(string target,bool manual)
        {
            if(!ValidPose(target))throw new ArgumentException("Unknown pose.");
            if(manual)Automatic=false;
            Action="";afterPose="";destination="";Age=0;PeakHold=0;
            MoveToward(target);
            Schedule();
        }
        void Transition(string clip,string next,string target){afterPose=next;destination=target;Begin(clip);}
        void MoveToward(string target)
        {
            if(Pose==target)return;
            if(Pose=="sleep")
            {if(SleepTransitions){Transition("sleep-to-lay","lay",target);return;}Pose="lay";}
            if(Pose=="sit"&&target!="sit")
            {if(DraftAnimations){Transition("sit-to-lay","lay",target);return;}Pose="lay";}
            if(Pose=="lay"&&target=="sleep"&&SleepTransitions){Transition("lay-to-sleep","sleep",target);return;}
            if(Pose=="lay"&&target=="sit"&&DraftAnimations){Transition("lay-to-sit","sit",target);return;}
            Pose=target;
        }
        public string ResolveAction(string action)
        {
            if(action=="blink")return Pose+"-blink";
            if(Pose=="lay"&&(action=="tilt"||action=="smile"||action=="lick"))return "lay-"+action;
            return action;
        }
        public bool CanAction(string action,bool interrupt=false)
        {
            if(Paused||Pose=="sleep"||InTransition||(!interrupt&&Action.Length!=0))return false;action=ResolveAction(action);
            ClipInfo clip;return action!="sit-to-lay"&&action!="lay-to-sit"&&action!="lay-to-sleep"&&action!="sleep-to-lay"&&art.Manifest.clips.TryGetValue(action,out clip)&&clip.pose==Pose;
        }
        public bool TryAction(string action,bool interrupt=false)
        {
            if(!CanAction(action,interrupt))return false;if(interrupt){Action="";afterPose="";Age=0;PeakHold=0;}Begin(ResolveAction(action));return true;
        }
        public void ObserveHover()
        {
            if(Expressions&&Pose!="sleep"&&Action.Length==0&&Clock>=attentionDue)
            {if(TryAction("tilt")){attentionDue=Clock+Range(25,50);expressionDue=Clock+Range(45,100);}}
        }
        public void Step(double dt,bool suppressed)
        {
            if(Paused||suppressed)return;dt=Math.Max(0,Math.Min(.1,dt));Clock+=dt;
            if(Action.Length!=0)
            {
                Age+=dt;
                if(Age>=ActionDuration)
                {
                    Action="";Age=0;PeakHold=0;
                    if(afterPose.Length!=0){string target=destination;Pose=afterPose;afterPose="";destination="";MoveToward(target);}
                }
                return;
            }
            if(Automatic&&Clock>=poseDue)
            {
                // Waking first returns to awake lying. Never express while asleep.
                if(Pose=="sleep")SetPose("lay",false);
                else{double pick=random.NextDouble();SetPose(pick<.52?"sit":pick<.86?"lay":"sleep",false);}
                return;
            }
            if(Pose=="sleep")return;
            if(Clock>=blinkDue)
            {
                blinkDue=Clock+Range(9,26);
                TryAction(Pose+"-blink");
                return;
            }
            if(Expressions&&Clock>=expressionDue)
            {expressionDue=Clock+Range(45,100);TryAction(new[]{"tilt","smile","lick"}[random.Next(3)]);}
        }
    }
}
