using AutoIt;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WowLib;

namespace AuctionBot
{
    public interface IOperationsPrimary : IOperations { }
    public interface IOperationsSecondary : IOperations { }

    public interface IOperations
    {
        Task<bool> EnterWorldVisible();
        Task<bool> WaitForEnterWorld(CancellationToken ct = default);
        void EnterWorld();
        Task<bool> RealmSelectionVisible();
        Task<bool> WaitForRealmSelection(CancellationToken ct = default);
        Task<bool> DisconnectedVisible();
        Task<bool> WaitForDisconenctedNotvisible(CancellationToken ct = default);
        Task<bool> LoadingVisible();
        Task<bool> WaitForLoadingNotVisible(CancellationToken ct = default);
        void ClearDisconnected();
        Task OpenAuctionHouse(CancellationToken ct = default);
        Task StartSniper();
        Task ResetCamera(CancellationToken ct = default);
        Task<bool> LoginVisible();
        void Login();
        void SelectFirstRealm();
        Task ClickSniperTab();
        Task<bool> RestartVisible();
        void ClickRestart();
        Task<bool> CacheTsmBounds();
        Task<bool> ItemFound();
        Task ClickFirstItem();
        Task<bool> BuyoutVisible();
        Task<bool> WaitForBuyout(CancellationToken ct = default);
        Task ClickBuyout();
        void TakeScreenshot(string path);
        bool RunAhSniper { get; }
        Rectangle CachedTsmBounds { get; set; }
        Screen Screen { get; set; }
    }

    public abstract class OperationsBase : IOperations
    {
        private double tolerance => 0.95;
        public Screen Screen { get; set; }
        private readonly IImageChecker imageChecker;
        private readonly IScreenCapture screenCapture;
        protected abstract Bitmap SniperType { get; }
        protected abstract string username { get; }
        protected abstract string password { get; }
        public abstract bool RunAhSniper { get; }
        public Rectangle CachedTsmBounds { get; set; }
        public abstract string Name { get; }

        public OperationsBase(IScreenCapture screenCapture, IImageChecker imageChecker)
        {
            this.imageChecker = imageChecker;
            this.screenCapture = screenCapture;
        }

        public async Task<bool> EnterWorldVisible()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, 1198, 1309, 72, 24), Resource.EnterWorld);
            if (result > tolerance)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> WaitForEnterWorld(CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(60) && !ct.IsCancellationRequested)
            {
                if(await EnterWorldVisible())
                {
                    await Task.Delay(1000);
                    if(await EnterWorldVisible())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> RealmSelectionVisible()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, 1211, 244, 181, 21), Resource.RealmSelection);
            if (result > tolerance)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> WaitForRealmSelection(CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(60) && !ct.IsCancellationRequested)
            {
                if (await RealmSelectionVisible())
                {
                    await Task.Delay(1000);
                    if (await RealmSelectionVisible())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> LoadingVisible()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, 0, 1367, 32, 32), Resource.Loading);
            if (result > tolerance)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> WaitForLoadingNotVisible(CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(60) && !ct.IsCancellationRequested)
            {
                if (!await LoadingVisible())
                {
                    await Task.Delay(1000);
                    if (!await LoadingVisible())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void EnterWorld()
        {
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 1190 + 85 / 2, 1300 + 40 / 2, 1, 0);
        }

        public async Task<bool> DisconnectedVisible()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, 1243, 747, 72, 29), Resource.Okay);
            if (result > tolerance)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> WaitForDisconenctedNotvisible(CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(5) && !ct.IsCancellationRequested)
            {
                if (!await DisconnectedVisible())
                {
                    await Task.Delay(1000);
                    if (!await DisconnectedVisible())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ClearDisconnected()
        {
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 1281 + 72 / 2, 758 + 29 / 2, 1, 0);
        }

        public void SelectFirstRealm()
        {
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 842, 360, 2, 0);
        }

        public async Task<bool> LoginVisible()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, 1261, 1037, 78, 30), Resource.Login);
            if (result > tolerance)
            {
                return true;
            }
            return false;
        }

        public void Login()
        {
            // Username
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 1289, 753, 1, 0);
            AutoItX.Send("^a");
            AutoItX.Send("{BS}");
            AutoItX.Send(username, 1);

            // Password
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 1289, 897, 1, 0);
            AutoItX.Send("^a");
            AutoItX.Send("{BS}");
            AutoItX.Send(password, 1);

            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 1304, 1011, 1, 0);
        }

        public async Task OpenAuctionHouse(CancellationToken ct = default)
        {
            var auctioneerLoc = Point.Empty;
            for (int y = 150; y <= 300; y += 150)
            {
                for (int x = 1; x < Screen.PrimaryScreen.Bounds.Width; x += 100)
                {
                    AutoItX.MouseMove(Screen.Bounds.X + x, y, 0);
                    await Task.Delay(150, ct);
                    var result = await imageChecker.bigContainsSmall(screenCapture.TakeScreenshot(Screen, 2437, 948, 105, 155), Resource.AuctioneerGrimful);
                    if (result != Point.Empty)
                    {
                        AutoItX.MouseClick("RIGHT", Screen.Bounds.X + x, y, 1, 0);
                        return;
                    }
                }
            }
        }

        public async Task StartSniper()
        {
            var result = await imageChecker.bigContainsSmall(screenCapture.TakeScreenshot(Screen, CachedTsmBounds.X, CachedTsmBounds.Y, CachedTsmBounds.Width, CachedTsmBounds.Height), SniperType);
            if (result != Point.Empty)
            {
                AutoItX.MouseClick("LEFT", Screen.Bounds.X + result.X + CachedTsmBounds.X, result.Y + CachedTsmBounds.Y, 1, 0);
            }
        }

        public async Task<bool> RestartVisible()
        {
            if (CachedTsmBounds.Width == 0 || CachedTsmBounds.Height == 0)
            {
                await CacheTsmBounds();
            }

            var result = await imageChecker.bigContainsSmall(screenCapture.TakeScreenshot(Screen, CachedTsmBounds.X + CachedTsmBounds.Width - 132, CachedTsmBounds.Y + CachedTsmBounds.Height - 37, 71, 11), Resource.RestartSniper);
            if (result != Point.Empty)
            {
               return true;
            }
            return false;
        }

        public void ClickRestart()
        {
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + CachedTsmBounds.X + 826, CachedTsmBounds.Y + 629, 1, 0);
        }

        public async Task ClickSniperTab()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, CachedTsmBounds.X + 721, CachedTsmBounds.Y + 58, 87, 24, @"C:\Temp\SniperTab.bmp"), Resource.SniperUnselected);
            if (result > tolerance)
            {
                AutoItX.MouseClick("LEFT", CachedTsmBounds.X + 764, CachedTsmBounds.Y + 70, 1, 0);
            }
        }

        public async Task ResetCamera(CancellationToken ct = default)
        {
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + 100, 100, 1, 0);
            await Task.Delay(100, ct);
            AutoItX.Send("{F9}{F9}");
        }

        public async Task<bool> CacheTsmBounds()
        {
            var topLeftCorner = await imageChecker.bigContainsSmall(screenCapture.TakeScreenshot(Screen, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), Resource.TSM);
            if (topLeftCorner == Point.Empty)
            {
                Console.WriteLine($"{Name} TSM Window NOT Found!!!");
                CachedTsmBounds = new Rectangle();
                return false;
            }

            var bottomRightCorner = await imageChecker.bigContainsSmall(screenCapture.TakeScreenshot(Screen, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), Resource.Corner, 1);
            if (bottomRightCorner == Point.Empty)
            {
                Console.WriteLine($"{Name} TSM Window NOT Found!!!");
                CachedTsmBounds = new Rectangle();
                return false;
            }

            CachedTsmBounds = new Rectangle(topLeftCorner.X - 66, topLeftCorner.Y - 28, (bottomRightCorner.X + 7) - (topLeftCorner.X - 66), (bottomRightCorner.Y + 7) - (topLeftCorner.Y - 28));
            return true;
        }

        public async Task<bool> ItemFound()
        {
            var result = await imageChecker.CompareImages(screenCapture.TakeScreenshot(Screen, CachedTsmBounds.X + 24, CachedTsmBounds.Y + 198, 12, 12), Resource.Item);
            if (result == 1.0)
            {
                return true;
            }
            return false;
        }

        public async Task ClickFirstItem()
        {
            await WaitForUnclick();
            Win32.ActivateWindow(WindowHandles.AuctionBot);
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + CachedTsmBounds.X + 300, CachedTsmBounds.Y + 203, 1, 0);
        }

        public async Task<bool> BuyoutVisible()
        {
            var result = await imageChecker.bigContainsSmall(screenCapture.TakeScreenshot(Screen, CachedTsmBounds.X + CachedTsmBounds.Width - 325, CachedTsmBounds.Y + CachedTsmBounds.Height - 37, 67, 11), Resource.Buyout);
            if (result != Point.Empty)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> WaitForBuyout(CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.Elapsed < TimeSpan.FromSeconds(10) && !ct.IsCancellationRequested)
            {
                if(await BuyoutVisible())
                    return true;
            } 
            return false;
        }

        public async Task ClickBuyout()
        {
            await WaitForUnclick();
            AutoItX.MouseClick("LEFT", Screen.Bounds.X + CachedTsmBounds.X + 643, CachedTsmBounds.Y + 629, 1, 0);
        }

        public void TakeScreenshot(string path)
        {
            screenCapture.TakeScreenshot(Screen, path);
        }

        private async Task WaitForUnclick()
        {
            if(Control.MouseButtons == MouseButtons.Left || Control.MouseButtons == MouseButtons.Right)
            {
                Console.WriteLine("Waiting for mouse to unclick");
                while(Control.MouseButtons == MouseButtons.Left || Control.MouseButtons == MouseButtons.Right)
                    await Task.Delay(50);
            }
        }
    }

    public class OperationsPrimary : OperationsBase, IOperationsPrimary
    {
        protected override Bitmap SniperType => Resource.RunBidSniper;
        protected override string username => Environment.GetEnvironmentVariable("WOW_PRIMARY_USERNAME") ?? throw new InvalidOperationException("WOW_PRIMARY_USERNAME environment variable not set");
        protected override string password => Environment.GetEnvironmentVariable("WOW_PRIMARY_PASSWORD") ?? throw new InvalidOperationException("WOW_PRIMARY_PASSWORD environment variable not set");
        public override bool RunAhSniper => false;

        public override string Name => "Primary";

        public OperationsPrimary(IScreenCapture screenCapture, IImageChecker imageChecker) : base(screenCapture, imageChecker) { }
    }

    public class OperationsSecondary : OperationsBase, IOperationsSecondary
    {
        protected override Bitmap SniperType => Resource.RunBuyoutSniper;
        protected override string username => Environment.GetEnvironmentVariable("WOW_SECONDARY_USERNAME") ?? throw new InvalidOperationException("WOW_SECONDARY_USERNAME environment variable not set");
        protected override string password => Environment.GetEnvironmentVariable("WOW_SECONDARY_PASSWORD") ?? throw new InvalidOperationException("WOW_SECONDARY_PASSWORD environment variable not set");
        public override bool RunAhSniper => true;

        public override string Name => "Secondary";

        public OperationsSecondary(IScreenCapture screenCapture, IImageChecker imageChecker) : base(screenCapture, imageChecker) { }
    }
}
