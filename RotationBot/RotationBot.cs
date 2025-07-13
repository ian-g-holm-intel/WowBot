using RotationBot.CombatActions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WowLib;

namespace RotationBot
{
    public interface IRotationBot
    {
        void Start();
        Task Stop();
    }

    public class RotationBot : IRotationBot
    {
        private bool generatingRage = false;
        private bool inCombat = false, chatOpen = false;
        private int currentRage = 0;
        private CancellationTokenSource ctSource = null;
        private Task mainTask = null;
        private readonly IScreenCapture screenCapture;
        private readonly IImageChecker imageChecker;
        private readonly RageManager rageManager;
        public readonly IEnumerable<ICombatAction> combatActions;

        public RotationBot(IScreenCapture screenCapture, IImageChecker imageChecker, RageManager rageManager, IEnumerable<ICombatAction> combatActions)
        {
            this.screenCapture = screenCapture;
            this.imageChecker = imageChecker;
            this.rageManager = rageManager;
            this.combatActions = combatActions;
        }

        public void Start()
        {
            if (mainTask == null)
            {
                ctSource = new CancellationTokenSource();
                mainTask = Task.Run(async () =>
                {
                    Console.WriteLine("RotationBot Started");
                    var combatStopwatch = new Stopwatch();

                    try
                    {
                        while (!ctSource.Token.IsCancellationRequested)
                        {
                            try
                            {
                                var infoImage = screenCapture.TakeScreenshot(Screen.AllScreens[0], 0, 0, 300, 11);
                                var combatInfo = await imageChecker.ParseCombatInfo(infoImage);

                                if(!combatInfo.GeneratingRage)
                                {
                                    if (generatingRage)
                                    {
                                        if (!combatStopwatch.IsRunning)
                                        {
                                            combatStopwatch.Restart();
                                        }
                                        else if (combatStopwatch.ElapsedMilliseconds > 500)
                                        {
                                            combatStopwatch.Reset();
                                            generatingRage = false;
                                            rageManager.Stop();
                                            Console.WriteLine($"Rage generation stopped: {rageManager.GetRagePerSecond():f2}");
                                        }
                                    }
                                }
                                else
                                {
                                    combatStopwatch.Reset();
                                    if (!generatingRage)
                                    {
                                        generatingRage = true;
                                        rageManager.Start();
                                        Console.WriteLine("Rage generation started");
                                    }
                                }

                                CheckCombatStatus(combatInfo);
                                ProcessRage(combatInfo);

                                if (await OutOfCombat(combatInfo) || await ChatOpen(combatInfo))
                                    continue;

                                foreach (var combatAction in combatActions.ToList())
                                {
                                    if(combatAction.ShouldExecute(combatInfo))
                                    {
                                        combatAction.Execute();
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
                        Console.WriteLine("RotationBot Stopping...");
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

        private async Task<bool> OutOfCombat(CombatInfo combatInfo)
        {
            if (!inCombat)
            {
                await Task.Delay(200);
                return true;
            }
            return false;
        }

        private void ProcessRage(CombatInfo combatInfo)
        {
            if (currentRage != combatInfo.Rage)
            {
                if (combatInfo.Rage > currentRage)
                {
                    rageManager.AddRage(combatInfo.Rage - currentRage);
                }
                currentRage = combatInfo.Rage;
            }
        }

        private void CheckCombatStatus(CombatInfo combatInfo)
        {
            if (!combatInfo.InCombat && inCombat)
            {
                inCombat = false;
                Console.WriteLine("Exiting Combat");
            }
            else if (combatInfo.InCombat && !inCombat)
            {
                inCombat = true;
                Console.WriteLine("Entering Combat");
            }
        }

        private async Task<bool> ChatOpen(CombatInfo combatInfo)
        {
            if (combatInfo.ChatOpen)
            {
                if (!chatOpen)
                {
                    chatOpen = true;
                    Console.WriteLine("Chat Open");
                }
                await Task.Delay(200);
                return true;
            }
            else
            {
                if (chatOpen)
                {
                    chatOpen = false;
                    Console.WriteLine("Chat Closed");
                }
                return false;
            }
        }
    }
}
