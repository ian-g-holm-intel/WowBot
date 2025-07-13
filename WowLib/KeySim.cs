using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WowLib
{
    public static class KeySim
    {
        #region P/Invoke
        private const uint WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101;

        [DllImport("user32", EntryPoint = "PostMessage", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool InternalPostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        #endregion

        public static void KeyDown(IntPtr handle, Keys keyDown)
        {
            if (!InternalPostMessage(handle,  // Insert your WowProcessHandle!
                WM_KEYDOWN,
                new IntPtr(IntPtr.Size == 4 ? (int)keyDown : (long)keyDown), // WPARAM is x64/x86 dependant, therefore cast from long/int to IntPtr
                IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void KeyUp(IntPtr handle, Keys keyUp)
        {
            if (!InternalPostMessage(handle,
                WM_KEYUP,
                new IntPtr(IntPtr.Size == 4 ? (int)keyUp : (long)keyUp),
                IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void KeyPress(IntPtr handle, Keys key)
        {
            KeyDown(handle, key);
            KeyUp(handle, key);
        }
    }
}
