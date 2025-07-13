using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace WowLib
{
    public class Win32
    {
        private const short SWP_NOMOVE = 0X2;
        private const short SWP_NOSIZE = 1;
        private const short SWP_NOACTIVATE = 0x0010;

        private enum WindowPosition
        {
            HWND_NOTOPMOST = -2,
            HWND_TOPMOST = -1,
            HWND_TOP = 0,
            HWND_BOTTOM = 1
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string WindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("KERNEL32.DLL")]
        private static extern IntPtr GetCurrentThreadId();

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("USER32.DLL")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        public static IntPtr FindWindow(string windowName)
        {
            return FindWindow(null, windowName);
        }

        public static IEnumerable<IntPtr> GetWowWindowPointers()
        {
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName);
            foreach (Process proc in processes)
            {
                if (proc.ProcessName.Equals("Wow"))
                    yield return proc.MainWindowHandle;
            }
        }

        public static IntPtr GetCurrentWindow()
        {
            return GetForegroundWindow();
        }

        public static void ActivateWindow(IntPtr hWnd)
        {
            if (hWnd == GetForegroundWindow())
                return;

            IntPtr mainThreadId = GetCurrentThreadId();
            IntPtr foregroundThreadID = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            if (foregroundThreadID != mainThreadId)
            {
                AttachThreadInput(mainThreadId, foregroundThreadID, true);
                SetForegroundWindow(hWnd);
                AttachThreadInput(mainThreadId, foregroundThreadID, false);
            }
            else
                SetForegroundWindow(hWnd);
        }

        public static bool WaitWindowActive(IntPtr hWnd, TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            while (hWnd != GetForegroundWindow() && stopwatch.Elapsed < timeout)
                Thread.Sleep(10);
            return hWnd == GetForegroundWindow();
        }

        public static IntPtr WaitWindowActive(IEnumerable<IntPtr> handles)
        {
            while(true)
            {
                var activeWindow = GetForegroundWindow();
                foreach(var handle in handles)
                {
                    if(activeWindow == handle)
                        return handle;
                }
            }
        }

        public static void SendWindowToBack(IntPtr hWnd)
        {
            SetWindowPos(hWnd, (int)WindowPosition.HWND_BOTTOM, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public static Rectangle GetWindowLocation(IntPtr handle)
        {
            RECT rectangle;
            GetWindowRect(handle, out rectangle);
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top);
        }
    }
}
