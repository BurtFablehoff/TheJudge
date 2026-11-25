using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Judge
{
    internal enum WatchMode { Off, Mouse, Window }

    internal sealed class WatchGaze
    {
        const int Sectors = 12;
        const double StepSeconds = .075;
        const double RadiansPerSector = Math.PI / 6;
        const double BoundaryMargin = Math.PI / 60;
        int direction = -1;
        double nextStep;
        string pose;

        public WatchMode Mode { get; private set; }

        public void Toggle(WatchMode requested)
        {
            Mode = Mode == requested ? WatchMode.Off : requested;
            Reset();
        }

        void Reset()
        {
            pose = null;
            direction = -1;
            nextStep = 0;
        }

        public static bool IsAwake(string pose)
        {
            return pose == "sit" || pose == "lay";
        }

        internal static string Key(string pose, int sector)
        {
            int hour = sector == 0 ? 12 : sector;
            return pose + "-look-" + hour.ToString("00", CultureInfo.InvariantCulture);
        }

        public string Frame(string currentPose, string currentFrame)
        {
            return direction >= 0 && pose == currentPose && currentFrame == currentPose
                ? Key(pose, direction) : currentFrame;
        }

        public void Step(double now, string currentPose, string currentFrame, PointF head, PointF? target, double deadZone)
        {
            if (Mode == WatchMode.Off || !IsAwake(currentPose) || currentFrame != currentPose || !target.HasValue)
            {
                Reset();
                return;
            }

            if (pose != currentPose)
            {
                Reset();
                pose = currentPose;
            }

            double dx = target.Value.X - head.X, dy = target.Value.Y - head.Y;
            if (dx * dx + dy * dy <= deadZone * deadZone)
            {
                Reset();
                return;
            }

            double angle = (Math.Atan2(dx, -dy) + 2 * Math.PI) % (2 * Math.PI);
            int desired = (int)Math.Floor(angle / RadiansPerSector + .5) % Sectors;
            if (direction < 0)
            {
                direction = desired;
                nextStep = now + StepSeconds;
                return;
            }

            double offset = Math.Abs(angle - direction * RadiansPerSector);
            if (Math.Min(offset, 2 * Math.PI - offset) <= RadiansPerSector / 2 + BoundaryMargin
                || desired == direction || now < nextStep)
                return;

            int clockwise = (desired - direction + Sectors) % Sectors;
            direction = (direction + (clockwise <= Sectors / 2 ? 1 : Sectors - 1)) % Sectors;
            nextStep = now + StepSeconds;
        }

        public static PointF HeadOnScreen(string pose, RectangleF sprite, Size nativeSize)
        {
            Point head = pose == "sit" ? new Point(518, 315) : new Point(404, 505);
            return new PointF(sprite.Left + sprite.Width * head.X / nativeSize.Width,
                sprite.Top + sprite.Height * head.Y / nativeSize.Height);
        }
    }

    internal static class WatchedWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Rect { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr handle, out Rect bounds);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr handle);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr handle);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr handle, out uint process);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetClassName(IntPtr handle, StringBuilder name, int capacity);

        internal static PointF? Target(Rectangle bounds, string windowClass)
        {
            if (bounds.Width <= 1 || bounds.Height <= 1 || windowClass == "Progman" || windowClass == "WorkerW"
                || windowClass == "Shell_TrayWnd" || windowClass == "Shell_SecondaryTrayWnd")
                return null;

            return new PointF(bounds.Left + bounds.Width * .5f, bounds.Top + Math.Min(80, bounds.Height * .25f));
        }

        public static PointF? Read(IntPtr handle, uint ownProcess)
        {
            if (handle == IntPtr.Zero || !IsWindowVisible(handle) || IsIconic(handle))
                return null;

            uint owner;
            if (GetWindowThreadProcessId(handle, out owner) == 0 || owner == ownProcess)
                return null;

            Rect bounds;
            if (!GetWindowRect(handle, out bounds))
                return null;

            var name = new StringBuilder(128);
            GetClassName(handle, name, name.Capacity);
            return Target(Rectangle.FromLTRB(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom), name.ToString());
        }
    }
}
