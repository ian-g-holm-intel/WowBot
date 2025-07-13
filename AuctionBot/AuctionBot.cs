using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WowLib;

namespace AuctionBot
{
    public interface IAuctionBotPrimary : IAuctionBot { }
    public class AuctionBotPrimary : AuctionBot, IAuctionBotPrimary
    {
        public AuctionBotPrimary(ILoginPrimary login, IOperationsPrimary operations, IActivator activator) : base(login, operations, activator) { }
    }
    public interface IAuctionBotSecondary : IAuctionBot { }
    public class AuctionBotSecondary : AuctionBot, IAuctionBotSecondary
    {
        public AuctionBotSecondary(ILoginSecondary login, IOperationsSecondary operations, IActivator activator) : base(login, operations, activator) { }
    }

    public interface IAuctionBot
    {
        void Start();
        Task Stop();
    }

    public abstract class AuctionBot : IAuctionBot
    {
        private CancellationTokenSource ctSource = null;
        private readonly ILogin login;
        private readonly IOperations operations;
        private readonly IActivator activator;
        private Task mainTask = null;

        public AuctionBot(ILogin login, IOperations operations, IActivator activator)
        {
            this.login = login;
            this.operations = operations;
            this.activator = activator;
        }

        public void Start()
        {
            if (mainTask == null)
            {
                ctSource = new CancellationTokenSource();
                mainTask = Task.Run(async () =>
                {
                    Console.WriteLine("AuctionBot Started");
                   
                    try
                    {
                        await login.Run(ctSource.Token);

                        //var callCount = 0;
                        //var timer = new System.Timers.Timer(1000) { AutoReset = true };
                        //timer.Elapsed += (sender, args) =>
                        //{
                        //    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} - Calls: {callCount}");
                        //    callCount = 0;
                        //};
                        //timer.Start();

                        while (!ctSource.Token.IsCancellationRequested)
                        {
                            try
                            {
                                //callCount++;
                                if (operations.RunAhSniper && await operations.ItemFound())
                                {
                                    await operations.ClickFirstItem();
                                    operations.TakeScreenshot($@"C:\Temp\Items\{DateTime.Now.ToFileTime()}.bmp");
                                    if (await operations.WaitForBuyout())
                                    {
                                        while(await operations.BuyoutVisible())
                                        {
                                            await operations.ClickBuyout();
                                            await Task.Delay(500);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Unhandled Exception: {ex}");
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unhandled Exception: " + ex.ToString());
                    }
                    finally
                    {
                        Console.WriteLine("AuctionBot Stopping...");
                    }
                }, ctSource.Token);
            }
        }

        public async Task Stop()
        {
            if (mainTask != null)
            {
                ctSource.Cancel();
                await mainTask;
                mainTask = null;
            }
        }
    }
}
