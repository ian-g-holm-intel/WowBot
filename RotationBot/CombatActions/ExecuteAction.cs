using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class ExecuteAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if (info.OnGCD || info.TargetBanished || CooldownStopwatch.ElapsedMilliseconds < 1000)
                return false;

            return info.ExecuteUsable;
        }

        public void Execute()
        {
            Console.WriteLine("Execute");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.D3);
            CooldownStopwatch.Restart();
        }
    }
}
