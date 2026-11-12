using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using PetFramework;

namespace Judge
{
    internal static class ProductChecks
    {
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        public static void Run(Assets art,string path)
        {
            string folder=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)),"product-fixtures-"+(Program.Development?"dev":"production"));Directory.CreateDirectory(folder);
            ReleaseUpdater updater=new ReleaseUpdater(new UpdateSettings{Repository=Product.Repository,AssetName=Product.AssetName,ProductName="The Judge Companion",DisplayName="The Judge",AppId="TheJudge",Cache=Path.Combine(folder,"Updates"),WindowTitle="The Judge"});
            object updaterChecks=updater.Test(folder);
            Require(updater.Repository=="BurtFablehoff/TheJudge"&&updater.LatestApi=="https://api.github.com/repos/BurtFablehoff/TheJudge/releases/latest","Updater feed identity differs.");
            Require(!updater.AssetUrl("https://github.com/BurtFablehoff/Odin/releases/download/v9.0.0/Odin.exe"),"Updater accepts another pet's feed.");
            bool rejected=false;try{updater.Apply(Path.Combine(folder,"outside-plan.json"));}catch(InvalidDataException){rejected=true;}Require(rejected,"Outside-cache update plan accepted.");
            Dictionary<string,string> run=new Dictionary<string,string>();run["Odin"]="leave this alone";string executable=Assembly.GetExecutingAssembly().Location;
            Action<string,string> set=delegate(string key,string command){run[key]=command;};Action<string> remove=delegate(string key){run.Remove(key);};
            WindowsStartup.Change(Product.StartupName,true,executable,set,remove);Require(run[Product.StartupName]=="\""+executable+"\"","Startup executable is not quoted.");
            WindowsStartup.Change(Product.StartupName,false,executable,set,remove);WindowsStartup.Change(Product.StartupName,false,executable,set,remove);Require(!run.ContainsKey(Product.StartupName)&&run["Odin"]=="leave this alone","Startup touched an unrelated entry.");
            Require(!new Preferences().StartWithWindows,"Startup registered without owner opting in.");
            Require(!new JavaScriptSerializer().Deserialize<Preferences>("{\"StartWithWindows\":false,\"AutoUpdates\":false}").AutoUpdates,"Saved update opt-out lost.");
            string product=FileVersionInfo.GetVersionInfo(executable).ProductName;Require(product==(Program.Development?"The Judge Companion Development":"The Judge Companion"),"Production/development binary identity differs.");
            Require(Preferences.Folder.EndsWith(Program.Development?"TheJudgeCompanionDevV1":"TheJudgeCompanionV1"),"Settings identity is not isolated.");
            int autonomousSleeps=0;
            for(int seed=0;seed<8;seed++)
            {
                Routine r=new Routine(art,seed);r.SelectAwake(seed%2==0?"sit":"lay");Require(r.Automatic,"Production pose disabled the routine.");
                for(int tick=0;tick<144000&&!r.Counts.ContainsKey("lay-to-sleep");tick++)r.Step(.1,false);
                Require(r.Counts.ContainsKey("lay-to-sleep"),"No autonomous sleep after production pose selection.");autonomousSleeps++;
                r.SelectAwake("sit");for(int tick=0;tick<30&&r.InTransition;tick++)r.Step(.1,false);Require(r.Pose=="sit"&&r.Automatic,"Manual awake choice failed to wake companion.");
                rejected=false;try{r.SelectAwake("sleep");}catch(ArgumentException){rejected=true;}Require(rejected,"Production exposed a manual sleeping state.");
            }
            File.WriteAllText(path,new JavaScriptSerializer().Serialize(new{passed=true,development=Program.Development,repository=Product.Repository,releaseAsset=Product.AssetName,updater=updaterChecks,startupQuoted=true,startupOptIn=true,startupRegistryName=Product.StartupName,unrelatedStartupPreserved=true,productionSettingsIsolated=true,developmentProductIsolated=true,autonomousSleepAfterPoseSelections=autonomousSleeps,manualSleepingRejected=true,networkUsed=false,registryWritten=false,userSettingsWritten=false,uiOpened=false}));
        }
    }
}
