using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PetFramework
{
    // Extracted from Odin's layered-window upload and nonactivating window path.
    // Input is a premultiplied-alpha scene rendered by the consuming application.
    public class LayeredSpriteWindow:Form
    {
        [StructLayout(LayoutKind.Sequential)]struct XY{public int X,Y;public XY(int x,int y){X=x;Y=y;}}
        [StructLayout(LayoutKind.Sequential)]struct WH{public int W,H;public WH(int w,int h){W=w;H=h;}}
        [StructLayout(LayoutKind.Sequential,Pack=1)]struct Blend{public byte Op,Flags,Alpha,Format;}
        [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr window);
        [DllImport("user32.dll")]static extern int ReleaseDC(IntPtr window,IntPtr dc);
        [DllImport("gdi32.dll")]static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")]static extern IntPtr SelectObject(IntPtr dc,IntPtr bitmap);
        [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr bitmap);
        [DllImport("gdi32.dll")]static extern bool DeleteDC(IntPtr dc);
        [DllImport("user32.dll",SetLastError=true)]static extern bool UpdateLayeredWindow(IntPtr window,IntPtr screen,ref XY location,ref WH size,IntPtr source,ref XY origin,int key,ref Blend blend,int flags);
        Bitmap hitSurface;Point hitOrigin;
        public Rectangle PresentedBounds{get{return hitSurface==null?Rectangle.Empty:new Rectangle(hitOrigin,hitSurface.Size);}}
        public LayeredSpriteWindow()
        {
            FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;
            StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;
        }
        protected override bool ShowWithoutActivation{get{return true;}}
        protected override CreateParams CreateParams{get{CreateParams p=base.CreateParams;p.ExStyle|=0x80000|0x08000000|0x80;return p;}}
        public static bool IsOpaque(Bitmap image,Point point)
        {return image!=null&&point.X>=0&&point.Y>=0&&point.X<image.Width&&point.Y<image.Height&&image.GetPixel(point.X,point.Y).A>16;}
        protected override void WndProc(ref Message message)
        {
            if(message.Msg==0x21){message.Result=new IntPtr(3);return;}
            if(message.Msg==0x84&&hitSurface!=null)
            {
                long packed=message.LParam.ToInt64();Point p=new Point((short)(packed&65535),(short)((packed>>16)&65535));p.Offset(-hitOrigin.X,-hitOrigin.Y);
                message.Result=new IntPtr(IsOpaque(hitSurface,p)?1:-1);return;
            }
            base.WndProc(ref message);
        }
        // Takes ownership of the scene only after a successful native upload.
        protected void Present(Bitmap scene,Point location)
        {
            // WinForms uses cached client bounds to route mouse/click events.
            // Keep the entire visible sprite interactive, including below the
            // default form height, before the native layered upload.
            SetBounds(location.X,location.Y,scene.Width,scene.Height,BoundsSpecified.All);
            IntPtr screen=GetDC(IntPtr.Zero),memory=IntPtr.Zero,bitmap=IntPtr.Zero,old=IntPtr.Zero;
            try
            {
                memory=CreateCompatibleDC(screen);bitmap=scene.GetHbitmap(Color.FromArgb(0));old=SelectObject(memory,bitmap);
                XY destination=new XY(location.X,location.Y),origin=new XY(0,0);WH size=new WH(scene.Width,scene.Height);
                Blend blend=new Blend{Op=0,Flags=0,Alpha=255,Format=1};
                if(!UpdateLayeredWindow(Handle,screen,ref destination,ref size,memory,ref origin,0,ref blend,2))throw new Win32Exception(Marshal.GetLastWin32Error());
                // UpdateLayeredWindow can precede WinForms' managed Bounds cache.
                // Hit testing and z-order checks use the actual uploaded scene.
                Bitmap prior=hitSurface;hitOrigin=location;hitSurface=scene;if(prior!=null)prior.Dispose();
            }
            catch{scene.Dispose();throw;}
            finally{if(old!=IntPtr.Zero)SelectObject(memory,old);if(bitmap!=IntPtr.Zero)DeleteObject(bitmap);if(memory!=IntPtr.Zero)DeleteDC(memory);if(screen!=IntPtr.Zero)ReleaseDC(IntPtr.Zero,screen);}
        }
        protected override void Dispose(bool disposing){if(disposing&&hitSurface!=null){hitSurface.Dispose();hitSurface=null;}base.Dispose(disposing);}
    }
}
