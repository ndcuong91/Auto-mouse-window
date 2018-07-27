using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auto
{
    public class WinAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;

        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static Point GetAppLocation()
        {
            RECT rct = new RECT();
            try
            {
                IntPtr hWnd = FindWindow(null, "Messenger for Desktop");
                MoveWindow(hWnd, 0, 0, 1420, 990, true);

                hWnd = FindWindow(null, "Bluestacks");
                MoveWindow(hWnd, 0, 0, 1420, 884, true);
                if (hWnd != IntPtr.Zero)
                    GetWindowRect(hWnd, ref rct);
            }
            catch (Exception ex)
            {
            }
            //Console.WriteLine("GetAppLocation. End");
            if (rct.Left > 0 && rct.Top > 0)
                return new Point(rct.Left, rct.Top);
            return new Point(0, 0);
        }

        public static Bitmap GetRectImage(Rectangle rect, bool bShow = true)
        {
            Bitmap rectImage;
            try
            {
                rectImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
                Graphics rectImageGraphic = Graphics.FromImage(rectImage);

                rectImageGraphic.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size);
                rectImageGraphic.Dispose();
            }
            catch (Exception ex)
            {
                rectImage = null;
            }
            return rectImage;
        }




        public static void LeftClick(Point target, int sleep)
        {
            SetCursorPos(target.X, target.Y);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(sleep);
        }

        public static void RightClick(Point target, int sleep)
        {
            SetCursorPos(target.X, target.Y);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            Thread.Sleep(sleep);
        }

        public static void DragAndDrop(Point source, Point target, int sleep)
        {
            SetCursorPos(source.X, source.Y);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(100);
            SetCursorPos(target.X, target.Y);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(sleep);
        }

        public static Bitmap GetRoiFromImage(Bitmap src, Rectangle rect)
        {
            try
            {
                return src.Clone(rect, PixelFormat.Format24bppRgb);
            }
            catch (Exception ex)
            {
            }
            return new Bitmap(0, 0);
        }

    }
}
