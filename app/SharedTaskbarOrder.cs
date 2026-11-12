using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace PetFramework
{
    public sealed class TopWindow
    {
        public IntPtr Handle;public string Class;public Rectangle Bounds;public bool Topmost;public uint Process;
        public TopWindow(IntPtr handle,string name,Rectangle bounds,bool topmost,uint process){Handle=handle;Class=name;Bounds=bounds;Topmost=topmost;Process=process;}
    }
    // Odin's vetted z-order decision, with no species/contact/size policy.
    public static class TaskbarOrder
    {
        [StructLayout(LayoutKind.Sequential)]struct Rect{public int Left,Top,Right,Bottom;}
        [DllImport("user32.dll")]static extern IntPtr GetTopWindow(IntPtr window);
        [DllImport("user32.dll")]static extern IntPtr GetWindow(IntPtr window,uint command);
        [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr window);
        [DllImport("user32.dll")]static extern bool IsIconic(IntPtr window);
        [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr window,out Rect rect);
        [DllImport("dwmapi.dll")]static extern int DwmGetWindowAttribute(IntPtr window,int attr,out int value,int size);
        [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr window,IntPtr after,int x,int y,int width,int height,uint flags);
        [DllImport("user32.dll")]static extern int GetClassName(IntPtr window,StringBuilder name,int capacity);
        [DllImport("user32.dll")]static extern int GetWindowLong(IntPtr window,int index);
        [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
        public static bool ShouldRaise(IList<TopWindow> order,IntPtr self,uint ownProcess,Rectangle bounds)
        {
            bool taskbar=false;
            foreach(TopWindow w in order)
            {
                if(w.Handle==self)return taskbar;
                if(!w.Topmost||w.Process==ownProcess)continue;
                if(w.Class=="Shell_TrayWnd"||w.Class=="Shell_SecondaryTrayWnd"){taskbar=true;continue;}
                if(w.Bounds.Width>1&&w.Bounds.Height>1&&w.Bounds.IntersectsWith(bounds))return false;
            }
            return false;
        }
        public static void Check(IntPtr self,uint process,Rectangle bounds)
        {
            List<TopWindow> order=new List<TopWindow>();int limit=0;
            for(IntPtr h=GetTopWindow(IntPtr.Zero);h!=IntPtr.Zero&&limit++<10000;h=GetWindow(h,2))
            {
                if(!IsWindowVisible(h)||IsIconic(h))continue;Rect r;if(!GetWindowRect(h,out r))continue;
                try{int cloaked;if(DwmGetWindowAttribute(h,14,out cloaked,4)==0&&cloaked!=0)continue;}catch(DllNotFoundException){}
                StringBuilder name=new StringBuilder(128);GetClassName(h,name,128);uint pid;GetWindowThreadProcessId(h,out pid);
                order.Add(new TopWindow(h,name.ToString(),Rectangle.FromLTRB(r.Left,r.Top,r.Right,r.Bottom),(GetWindowLong(h,-20)&8)!=0,pid));
            }
            if(ShouldRaise(order,self,process,bounds))SetWindowPos(self,new IntPtr(-1),0,0,0,0,1|2|0x10);
        }
    }
}
