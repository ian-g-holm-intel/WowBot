using AutoIt;
using System;
using System.Threading.Tasks;

namespace WowLib
{
    public interface IActivator
    {
        Task ActivateWowWindows(IntPtr windowHandle);
        void AntiAFK(IntPtr windowHandle);
    }

    public class Activator : IActivator
    {
        public async Task ActivateWowWindows(IntPtr windowHandle)
        {
            for(int i = 0; i < 5; i++)
            {
                AutoItX.WinActivate(windowHandle);
                if (AutoItX.WinActive(windowHandle) != 0)
                    break;
                await Task.Delay(10);
            }
                
            if(AutoItX.WinActive(windowHandle) == 0)
                Console.WriteLine("Failed to activate WoW");
        }

        public void AntiAFK(IntPtr windowHandle)
        {
            KeySim.KeyPress(windowHandle, System.Windows.Forms.Keys.Space);
        }
    }
}
