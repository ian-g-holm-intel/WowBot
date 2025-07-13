using System;
using System.Threading.Tasks;
using WowLib;

namespace RotationBot.CombatActions
{
    public class SunderArmorAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if (info.OnGCD || info.TargetBanished | CooldownStopwatch.ElapsedMilliseconds < 1000)
                return false;

            return info.NeedSunder;
        }

        public void Execute()
        {
            Console.WriteLine("Sunder");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.D0);
            CooldownStopwatch.Restart();
        }
    }
}
