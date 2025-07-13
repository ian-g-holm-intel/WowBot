using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class HamstringAction : CombatActionBase, ICombatAction
    {
        private readonly RageManager rageManager;
        public HamstringAction(RageManager rageManager)
        {
            this.rageManager = rageManager;
        }

        public bool ShouldExecute(CombatInfo info)
        {
            if (info.OnGCD || info.TargetBanished || CooldownStopwatch.ElapsedMilliseconds < 1000)
                return false;

            if (Constants.WeaponType == WeaponType.DualWield)
                return false;

            if(info.BloodthirstCD > 1.5 && info.WhirlwindCD > 1.5)
            {
                var timeLeft = Math.Max(info.BloodthirstCD, info.WhirlwindCD);
                var incomingRage = timeLeft * rageManager.GetRagePerSecond();
                if(info.Rage + incomingRage > 65)
                    return true;
            }
            return false;
        }

        public void Execute()
        {
            Console.WriteLine("Hamstring");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.D7);
            CooldownStopwatch.Restart();
        }
    }
}