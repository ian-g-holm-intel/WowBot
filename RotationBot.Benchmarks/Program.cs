using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WowLib;

namespace RotationBot.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var summary = BenchmarkRunner.Run<RotationBotBenchmarks>();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }

    [SimpleJob(launchCount: 1, warmupCount: 5, targetCount: 20)]
    public class RotationBotBenchmarks
    {
        private readonly IScreenCapture screenCapture = new ScreenCapture();
        private readonly IImageChecker imageChecker = new ImageChecker();
        private readonly Bitmap image;

        public RotationBotBenchmarks()
        {
            image = screenCapture.TakeScreenshot(Screen.AllScreens[0], 0, 0, 300, 11);
        }

        [Benchmark]
        public void ScreenshotCapture()
        {
            var infoImage = screenCapture.TakeScreenshot(Screen.AllScreens[0], 0, 0, 300, 11);
        }

        [Benchmark]
        public async Task Parsing()
        {
            var combatInfo = await imageChecker.ParseCombatInfo(image);
        }

        [Benchmark]
        public async Task ScreenCaptureAndParsing()
        {
            var infoImage = screenCapture.TakeScreenshot(Screen.AllScreens[0], 0, 0, 300, 11);
            var combatInfo = await imageChecker.ParseCombatInfo(infoImage);
        }
    }
}
