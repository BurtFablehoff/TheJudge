using System;
using System.IO;
using Microsoft.Win32;

namespace PetFramework
{
    // Odin's quoted per-user Run entry, parameterized by the consuming pet.
    internal static class WindowsStartup
    {
        const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
        internal static string Command(string executable)
        {
            if(!Path.IsPathRooted(executable)||!File.Exists(executable)||executable.IndexOf('"')>=0)throw new IOException("The companion executable path is unavailable.");
            return "\""+executable+"\"";
        }
        internal static void Change(string valueName,bool enabled,string executable,Action<string,string> set,Action<string> remove)
        {
            if(String.IsNullOrEmpty(valueName)||valueName.IndexOfAny(new[]{'\\','/'})>=0)throw new ArgumentException("Invalid startup identity.");
            if(enabled)set(valueName,Command(executable));else remove(valueName);
        }
        public static bool IsEnabled(string valueName,string executable)
        {using(RegistryKey key=Registry.CurrentUser.OpenSubKey(RunKey))return key!=null&&String.Equals(key.GetValue(valueName) as string,Command(executable),StringComparison.OrdinalIgnoreCase);}
        public static void Set(string valueName,bool enabled,string executable)
        {
            if(String.IsNullOrEmpty(valueName)||valueName.IndexOfAny(new[]{'\\','/'})>=0)throw new ArgumentException("Invalid startup identity.");
            using(RegistryKey key=Registry.CurrentUser.CreateSubKey(RunKey))Change(valueName,enabled,executable,delegate(string name,string value){key.SetValue(name,value,RegistryValueKind.String);},delegate(string name){key.DeleteValue(name,false);});
        }
    }
}
