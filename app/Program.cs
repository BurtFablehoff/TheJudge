using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using PetFramework;

[assembly:AssemblyTitle("The Judge Companion")]
#if JUDGE_DEV
[assembly:AssemblyProduct("The Judge Companion Development")]
#else
[assembly:AssemblyProduct("The Judge Companion")]
#endif
[assembly:AssemblyVersion("0.1.7.0")]
[assembly:AssemblyFileVersion("0.1.7.0")]

namespace Judge
{
    internal static class Program
    {
#if JUDGE_DEV
        internal static bool Development{get{return true;}}
        internal const string Singleton="Local\\TheJudgeCompanionDevV1";
        internal const string SettingsName="TheJudgeCompanionDevV1";
#else
        internal static bool Development{get{return false;}}
        internal const string Singleton="Local\\TheJudgeCompanionV1";
        internal const string SettingsName="TheJudgeCompanionV1";
#endif
        internal static readonly ReleaseUpdater Updater=Product.CreateUpdater();
        [DllImport("user32.dll")]static extern bool SetProcessDpiAwarenessContext(IntPtr context);
        [DllImport("user32.dll")]static extern bool SetProcessDPIAware();
        [STAThread]static int Main(string[] args)
        {
            try
            {
                if(args.Length==2&&args[0]=="--apply-update"){if(Development)throw new InvalidOperationException("Development builds cannot apply production updates.");return Updater.Apply(args[1]);}
                if(args.Length==3&&args[0]=="--update-health"){if(Development)throw new InvalidOperationException("Development builds cannot replace production.");Updater.HealthFile=args[1];Updater.HealthToken=args[2];args=new string[0];}
                using(Assets art=new Assets())
                {
                    if(args.Length==2&&args[0]=="--render-menu"){using(CompanionWindow window=new CompanionWindow(art,true))window.RenderMenu(args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--test-product"){ProductChecks.Run(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--self-test"){Checks.Run(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--render-review"){Checks.Render(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--test-desktop"){DesktopChecks.Run(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--render-transitions"){Checks.RenderTransitions(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--render-pose-cycle"){Checks.RenderPoseCycle(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--render-sleep-transitions"){Checks.RenderSleepTransitions(art,args[1]);return 0;}
                    if(args.Length==2&&args[0]=="--render-awake-actions"){Checks.RenderAwakeActions(art,args[1]);return 0;}
                    if(args.Length!=0)throw new ArgumentException("Supported commands: --self-test <report.json>, --test-desktop <report.json>, --render-review <directory>, --render-transitions <directory>, --render-awake-actions <directory>, --render-sleep-transitions <directory>, --render-pose-cycle <directory>, --render-menu <image.png>, --test-product <report.json>.");
                    bool created;
                    using(Mutex mutex=new Mutex(true,Singleton,out created))
                    {
                        if(!created)return 0;
                        try{SetProcessDpiAwarenessContext(new IntPtr(-4));}catch(EntryPointNotFoundException){SetProcessDPIAware();}
                        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new CompanionWindow(art));
                    }
                }
                return 0;
            }
            catch(Exception error)
            {
                if(!String.IsNullOrEmpty(Updater.HealthFile)){Updater.FailHealth(error.Message);return 1;}
                if(args.Length>1){try{File.WriteAllText(args[1]+".error.txt",error.ToString());}catch(IOException){}}
                else MessageBox.Show(error.Message,"The Judge couldn't start",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return 1;
            }
        }
    }
}
