using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleInjector;
using WowLib;

namespace AuctionBot
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var container = Bootstrap();
            var activator = container.GetInstance<IActivator>();
            var auctionBotSecondary = container.GetInstance<IAuctionBotSecondary>();
            var login = container.GetInstance<ILoginSecondary>();
            var operationsSecondary = container.GetInstance<IOperationsSecondary>();

            try
            {
                var handles = Win32.GetWowWindowPointers();
                Console.WriteLine("Waiting for WoW window to become active");
                WindowHandles.AuctionBot = Win32.WaitWindowActive(handles);
                var windowLocation = Win32.GetWindowLocation(WindowHandles.AuctionBot);

                operationsSecondary.Screen = windowLocation.X == 0 ? Screen.AllScreens[0] : Screen.AllScreens[1];

                auctionBotSecondary.Start();

                var random = new Random();
                var logoutTime = random.Next(0, 59);
                var logoutCheckTimer = Stopwatch.StartNew();
                var antiAfkInterval = GetAntiAfkInterval();
                var antiAfkTimer = Stopwatch.StartNew();
                var timer = new System.Timers.Timer(1000);
                timer.Elapsed += async (source, eventArgs) =>
                {
                    if (logoutCheckTimer.Elapsed >= TimeSpan.FromMinutes(3))
                    {
                        logoutCheckTimer.Restart();
                        await login.Run();
                    }

                    if(antiAfkTimer.Elapsed.TotalSeconds >= antiAfkInterval)
                    {
                        antiAfkTimer.Restart();
                        activator.AntiAFK(WindowHandles.AuctionBot);
                        antiAfkInterval = GetAntiAfkInterval();
                    }

                    if (DateTime.Now.Hour == 3 && DateTime.Now.Minute == logoutTime)
                    {
                        await auctionBotSecondary.Stop();
                        foreach(var wowProc in Process.GetProcessesByName("Wow"))
                            wowProc.Kill();
                        timer.Stop();
                    }
                };
                timer.Start();

                Console.WriteLine("Press <ENTER> to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
            finally
            {
                await auctionBotSecondary.Stop();
            }
        }

        private static int GetAntiAfkInterval()
        {
            return new Random().Next(Convert.ToInt32(TimeSpan.FromMinutes(13).TotalSeconds), Convert.ToInt32(TimeSpan.FromMinutes(17).TotalSeconds));
        }

        private static Container Bootstrap()
        {
            var container = new Container();

            container.Register<IScreenCapture, ScreenCapture>(Lifestyle.Singleton);
            container.Register<IImageChecker, ImageChecker>(Lifestyle.Singleton);
            container.Register<IActivator, WowLib.Activator>();
            container.Register<ILoginPrimary, LoginPrimary>();
            container.Register<ILoginSecondary, LoginSecondary>();
            container.Register<IOperationsPrimary, OperationsPrimary>(Lifestyle.Singleton);
            container.Register<IOperationsSecondary, OperationsSecondary>(Lifestyle.Singleton);
            container.Register<IAuctionBotPrimary, AuctionBotPrimary>();
            container.Register<IAuctionBotSecondary, AuctionBotSecondary>();
            container.Collection.Register<IAuctionBot>(typeof(AuctionBotPrimary), typeof(AuctionBotSecondary));

            return container;
        }
    }
}
