using System;
using System.Threading;
using System.Threading.Tasks;
using WowLib;

namespace AuctionBot
{
    public interface ILoginPrimary : ILogin { }
    public class LoginPrimary : Login, ILoginPrimary
    {
        public LoginPrimary(IOperationsPrimary operations, IActivator activator) : base(operations, activator) { }
    }

    public interface ILoginSecondary : ILogin { }
    public class LoginSecondary : Login, ILoginSecondary
    {
        public LoginSecondary(IOperationsSecondary operations, IActivator activator) : base(operations, activator) { }
    }

    public interface ILogin
    {
        Task Run(CancellationToken ct = default);
    }

    public abstract class Login : ILogin
    {
        private readonly IOperations operations;
        private readonly IActivator activator;
        private TimeSpan openAhDelay => TimeSpan.FromSeconds(1);
        private TimeSpan guiDelay => TimeSpan.FromMilliseconds(50);

        public Login(IOperations operations, IActivator activator)
        {
            this.operations = operations;
            this.activator = activator;
        }

        public async Task Run(CancellationToken ct = default)
        {
            if (await operations.DisconnectedVisible())
            {
                operations.ClearDisconnected();
            }

            if (await operations.LoginVisible())
            {
                Console.WriteLine("Logging in");
                await operations.WaitForDisconenctedNotvisible(ct);
                operations.Login();
                await operations.WaitForEnterWorld(ct);
            }

            if (await operations.EnterWorldVisible())
            {
                Console.WriteLine("Entering World");
                operations.EnterWorld();
                await Task.Delay(5000);
                await operations.WaitForLoadingNotVisible(ct);
            }

            if (!await operations.RestartVisible())
            {
                await activator.ActivateWowWindows(WindowHandles.AuctionBot);
                if(await operations.RestartVisible())
                    return;

                for (int i = 0; i < 3; i++)
                {
                    if(await operations.CacheTsmBounds())
                        break;

                    Console.WriteLine("Opening Auction Window");
                    await operations.ResetCamera(ct);
                    await operations.OpenAuctionHouse(ct);
                    await Task.Delay(openAhDelay, ct);
                }
                
                await operations.ClickSniperTab();
                await Task.Delay(guiDelay, ct);
                await operations.StartSniper();
                await Task.Delay(guiDelay, ct);
            }
        }
    }
}
