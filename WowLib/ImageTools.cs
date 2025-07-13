using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WowLib
{
    public class LockedFastImage : IDisposable
    {
        private readonly Bitmap image;
        private readonly byte[] rgbValues;
        private readonly BitmapData bmpData;

        private readonly IntPtr ptr;
        private readonly int bytes;

        public LockedFastImage(Bitmap image)
        {
            this.image = image;
            this.Width = image.Width;
            this.Height = image.Height;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            ptr = bmpData.Scan0;
            bytes = Math.Abs(bmpData.Stride) * image.Height;
            rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
        }

        bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                try
                {
                    image.UnlockBits(bmpData);
                }
                catch { }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        /// Returns or sets a pixel of the image. 
        /// </summary>
        /// <param name="x">x parameter of the pixel</param>
        /// <param name="y">y parameter of the pixel</param>
        public Color this[int x, int y]
        {
            get
            {
                int index = (x + (y * Width)) * 4;
                return Color.FromArgb(rgbValues[index + 3], rgbValues[index + 2], rgbValues[index + 1], rgbValues[index]);
            }

            set
            {
                int index = (x + (y * Width)) * 4;
                rgbValues[index] = value.B;
                rgbValues[index + 1] = value.G;
                rgbValues[index + 2] = value.R;
                rgbValues[index + 3] = value.A;
            }
        }

        /// <summary>
        /// Width of the image. 
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the image. 
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Returns the modified Bitmap. 
        /// </summary>
        public Bitmap asBitmap()
        {
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            return image;
        }
    }

    public interface IImageChecker
    {
        /// <summary>
        /// Returns the location of the small image in the big image. Returns CHECKFAILED if not found.
        /// </summary>
        /// <param name="x_speedUp">speeding up at x achsis.</param>
        /// <param name="y_speedUp">speeding up at y achsis.</param>
        /// <param name="begin_percent_x">Reduces the search rect. 0 - 100</param>
        /// <param name="end_percent_x">Reduces the search rect. 0 - 100</param>
        /// <param name="begin_percent_x">Reduces the search rect. 0 - 100</param>
        /// <param name="end_percent_y">Reduces the search rect. 0 - 100</param>
        Task<Point> bigContainsSmall(Bitmap big_image, Bitmap small_image, int tolerance = 0, int begin_percent_x = 0, int end_percent_x = 100, int begin_percent_y = 0, int end_percent_y = 100);

        Task<double> CompareImages(Bitmap FirstImage, Bitmap SecondImage, int tolerance = 5);
        Task<CombatInfo> ParseCombatInfo(Bitmap srcImg);
    }

    public class ImageChecker : IImageChecker
    {
        /// <summary>
        /// The time needed for last operation.
        /// </summary>
        public TimeSpan time_needed = new TimeSpan();

        /// <summary>
        /// Error return value.
        /// </summary>
        static public Point CHECKFAILED = Point.Empty;

        /// <summary>
        /// Returns the location of the small image in the big image. Returns CHECKFAILED if not found.
        /// </summary>
        /// <param name="x_speedUp">speeding up at x achsis.</param>
        /// <param name="y_speedUp">speeding up at y achsis.</param>
        /// <param name="begin_percent_x">Reduces the search rect. 0 - 100</param>
        /// <param name="end_percent_x">Reduces the search rect. 0 - 100</param>
        /// <param name="begin_percent_x">Reduces the search rect. 0 - 100</param>
        /// <param name="end_percent_y">Reduces the search rect. 0 - 100</param>
        public Task<Point> bigContainsSmall(Bitmap big_image, Bitmap small_image, int tolerance = 0, int begin_percent_x = 0, int end_percent_x = 100, int begin_percent_y = 0, int end_percent_y = 100)
        {
            return Task.Run(() =>
            {
                if (big_image == null)
                    return CHECKFAILED;

                using (var bigImage = new LockedFastImage(big_image))
                {
                    using (var smallImage = new LockedFastImage(small_image))
                    {
                        /*
                         * SPEEDUP PARAMETER
                         * It might be enough to check each second or third pixel in the small picture.
                         * However... In most cases it would be enough to check 4 pixels of the small image for diablo porposes.
                         * */

                        /*
                         * BEGIN, END PARAMETER
                         * In most cases we know where the image is located, for this we have the begin and end paramenters.
                         * */

                        DateTime begin = DateTime.Now;

                        if (begin_percent_x < 0 || begin_percent_x > 100) begin_percent_x = 0;
                        if (begin_percent_y < 0 || begin_percent_y > 100) begin_percent_y = 0;
                        if (end_percent_x < 0 || end_percent_x > 100) end_percent_x = 100;
                        if (end_percent_y < 0 || end_percent_y > 100) end_percent_y = 100;

                        int x_start = (int)((double)big_image.Width * ((double)begin_percent_x / 100.0));
                        int x_end = (int)((double)big_image.Width * ((double)end_percent_x / 100.0));
                        int y_start = (int)((double)big_image.Height * ((double)begin_percent_y / 100.0));
                        int y_end = (int)((double)big_image.Height * ((double)end_percent_y / 100.0));

                        /*
                            * We cant speed up the big picture, because then we have to check pixels in the small picture equal to the speeded up size 
                            * for each pixel in the big picture.
                            * Would give no speed improvement.
                            * */

                        //+ 1 because first pixel is in picture. - small because image have to be fully in the other image
                        for (int x = x_start; x < x_end - small_image.Width + 1; x++)
                            for (int y = y_start; y < y_end - small_image.Height + 1; y++)
                            {
                                //now we check if all pixels matches
                                for (int sx = 0; sx < small_image.Width; sx++)
                                    for (int sy = 0; sy < small_image.Height; sy++)
                                    {
                                        if(tolerance == 0)
                                        {
                                            if (smallImage[sx, sy] != bigImage[x + sx, y + sy])
                                                goto CheckFailed;
                                        }
                                        else
                                        {
                                            var first = smallImage[sx, sy];
                                            var second = bigImage[x + sx, y + sy];
                                            var deltaA = Math.Abs(Convert.ToInt32(first.A) - Convert.ToInt32(second.A));
                                            var deltaR = Math.Abs(Convert.ToInt32(first.R) - Convert.ToInt32(second.R));
                                            var deltaB = Math.Abs(Convert.ToInt32(first.B) - Convert.ToInt32(second.B));
                                            var deltaG = Math.Abs(Convert.ToInt32(first.G) - Convert.ToInt32(second.G));
                                            if (new[] { deltaA, deltaR, deltaB, deltaG }.Max() > tolerance)
                                                goto CheckFailed;
                                        }
                                    }

                                //check ok
                                time_needed = DateTime.Now - begin;
                                return new Point(x + small_image.Width / 2, y + small_image.Height / 2);

                            CheckFailed:
                                ;
                            }

                        time_needed = DateTime.Now - begin;
                        return CHECKFAILED;
                    }
                }
            });
        }

        public async Task<CombatInfo> ParseCombatInfo(Bitmap srcImg)
        {
            var gcdColor = Color.FromArgb(255, 0, 251, 253);
            var rageColor = Color.FromArgb(255, 105, 251, 0);
            var mhColor = Color.FromArgb(255, 199, 27, 0);
            var ohColor = Color.FromArgb(255, 199, 102, 0);
            var apColor = Color.FromArgb(255, 255, 251, 0);
            var btColor = Color.FromArgb(255, 255, 0, 0);
            var wwColor = Color.FromArgb(255, 0, 0, 255);
            var statusColor = Color.FromArgb(255, 100, 251, 105);

            using (var image = new LockedFastImage(srcImg))
            {
                var width = image.Width;
                var taskList = new List<Task<int>>();
                taskList.Add(CountColors(image, 0, width, apColor));
                taskList.Add(CountColors(image, 1, width, mhColor));
                taskList.Add(CountColors(image, 2, width, ohColor));
                taskList.Add(CountColors(image, 3, width, gcdColor));
                taskList.Add(CountColors(image, 4, width, rageColor));
                taskList.Add(CountColors(image, 5, width, mhColor));
                taskList.Add(CountColors(image, 6, width, ohColor));
                taskList.Add(CountColors(image, 7, width, apColor));
                taskList.Add(CountColors(image, 8, width, btColor));
                taskList.Add(CountColors(image, 9, width, wwColor));
                var barsInfo = await Task.WhenAll(taskList);
                var statusInfo = await GetStatusInfo(image, 13, 10, width, statusColor);

                return new CombatInfo {
                                        CritChance = Convert.ToDouble(barsInfo[0]) / 10 + 20,
                                        MainhandAtkSpeed = Convert.ToDouble(barsInfo[1]) / 10,
                                        OffhandAtkSpeed = Convert.ToDouble(barsInfo[2]) / 10,
                                        GCD = Convert.ToDouble(barsInfo[3]) / 100,
                                        Rage = barsInfo[4],
                                        MainhandSwing = Convert.ToDouble(barsInfo[5] * barsInfo[1]) / 1000,
                                        OffhandSwing = Convert.ToDouble(barsInfo[6] * barsInfo[2]) / 1000,
                                        AttackPower = (barsInfo[7]) * 10,
                                        BloodthirstCD = Convert.ToDouble(barsInfo[8]) / 20,
                                        WhirlwindCD = Convert.ToDouble(barsInfo[9]) / 20,
                                        BloodthirstUsable = statusInfo[0],
                                        WhirlwindUsable = statusInfo[1],
                                        ExecuteUsable = statusInfo[2],
                                        HeroicStrikeUsable = statusInfo[3],
                                        HeroicStrikePressed = statusInfo[4],
                                        BerserkerStance = statusInfo[5],
                                        BloodrageUsable = statusInfo[6],
                                        NeedBattleShout = statusInfo[7],
                                        ShouldAOE = statusInfo[8],
                                        NeedSunder = statusInfo[9],
                                        TargetBanished = statusInfo[10],
                                        PolymorphNearby = statusInfo[11],
                                        ChatOpen = statusInfo[12]
                };
            }
        }

        private Task<int> CountColors(LockedFastImage image, int yLoc, int width, Color searchColor)
        {
            return Task.Run(() =>
            {
                var count = 0;
                for (int x = 0; x < width; x++)
                {
                    var color = image[x, yLoc];
                    if (color != searchColor)
                        break;

                    count++;
                }
                return count;
            });
        }

        private Task<bool[]> GetStatusInfo(LockedFastImage image, int count, int yLoc, int width, Color searchColor)
        {
            return Task.Run(() =>
            {
                var results = new bool[count];
                for(int i = 0; i < count; i++)
                {
                    var color = image[i, yLoc];
                    results[i] = searchColor == color;
                }
                return results;
            });
        }

        public async Task<double> CompareImages(Bitmap FirstImage, Bitmap SecondImage, int tolerance = 5)
        {
            if(FirstImage == null)
                return 0;

            if (FirstImage.Size != SecondImage.Size)
                throw new Exception("Images are of unequal size");

            BitmapData bmdFirstImage, bmdSecondImage;
            var intPixelSize = 4;
            var totalPixels = FirstImage.Width * FirstImage.Height;
            double unequalPixels = 0;


            // Lock both bitmap bits to initialize comparison of pixels
            bmdFirstImage = FirstImage.LockBits(new Rectangle(0, 0, FirstImage.Width, FirstImage.Height),
                                                 ImageLockMode.ReadOnly,
                                                 FirstImage.PixelFormat);

            bmdSecondImage = SecondImage.LockBits(new Rectangle(0, 0, SecondImage.Width, SecondImage.Height),
                                                   ImageLockMode.ReadOnly,
                                                   SecondImage.PixelFormat);
            try
            { 
                var taskList = new List<Task>();
                for (var task = 0; task < Environment.ProcessorCount; task++)
                { 
                    taskList.Add(Task.Run(() =>
                    {
                        unsafe
                        {
                            for (var y = task; y < bmdFirstImage.Height; y += 8)
                            {
                                byte* rowFirstImage = (byte*)bmdFirstImage.Scan0 + (y * bmdFirstImage.Stride);
                                byte* rowSecondImage = (byte*)bmdSecondImage.Scan0 + (y * bmdSecondImage.Stride);

                                for (var x = 0; x < bmdFirstImage.Width; ++x)
                                {
                                    int first = rowFirstImage[x * intPixelSize];
                                    int second = rowSecondImage[x * intPixelSize];
                                    var difference = first - second;
                                    if (Math.Abs(difference) > tolerance)
                                    {
                                        unequalPixels++;
                                    }
                                }
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
            }
            finally
            {
                // Unlock bitmap bits
                FirstImage.UnlockBits(bmdFirstImage);
                SecondImage.UnlockBits(bmdSecondImage);
            }
            return (totalPixels - unequalPixels ) / totalPixels;
        }
    }

    public interface IScreenCapture
    {
        Bitmap TakeScreenshot(Screen screen, string path = "");
        Bitmap TakeScreenshot(Screen screen, int x, int y, int width, int height, string path = "");
    }

    public class ScreenCapture : IScreenCapture
    {
        public Bitmap TakeScreenshot(Screen screen, string path = "")
        {
            return TakeScreenshot(screen, 0, 0, screen.Bounds.Width, screen.Bounds.Height, path);
        }

        public Bitmap TakeScreenshot(Screen screen, int x, int y, int width, int height, string path = "")
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(screen.Bounds.X + x, screen.Bounds.Y + y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            if(!string.IsNullOrEmpty(path))
                bitmap.Save(path);
            return bitmap;
        }
    }
}