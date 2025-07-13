using RotationBot.CombatActions;
using SimpleInjector;
using System;
using System.Threading.Tasks;
using WowLib;

namespace RotationBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = Bootstrap();
            var rotationBot = container.GetInstance<IRotationBot>();

            try
            {
                var handles = Win32.GetWowWindowPointers();
                Console.WriteLine("Waiting for WoW window to become active");
                WindowHandles.RotationBot = Win32.WaitWindowActive(handles);

                rotationBot.Start();

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
                await rotationBot.Stop();
            }
        }

        private static Container Bootstrap()
        {
            var container = new Container();

            container.Register<IScreenCapture, ScreenCapture>(Lifestyle.Singleton);
            container.Register<IImageChecker, ImageChecker>(Lifestyle.Singleton);
            container.Register<IRotationBot , RotationBot>(Lifestyle.Singleton);
            container.Register<RageManager>(Lifestyle.Singleton);
            container.Collection.Register<ICombatAction>(new[]{ 
                Lifestyle.Singleton.CreateRegistration(typeof(BloodrageAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(BattleShoutAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(SunderArmorAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(BerserkerStanceAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(HeroicStrikeOffAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(HeroicStrikeOnAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(ExecuteAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(BloodthirstAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(WhirlwindAction), container),
                Lifestyle.Singleton.CreateRegistration(typeof(HamstringAction), container)
                });

            return container;
        }
    }
}
