using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class HeroicStrikeOffAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if (!Constants.HeroicStrikeQueuingEnabled || Constants.WeaponType == WeaponType.TwoHanded || info.TargetBanished || CooldownStopwatch.ElapsedMilliseconds < 300)
                return false;

            if (info.HeroicStrikePressed && info.Rage < 70 && (info.MainhandSwing <= 0.4 || info.OffhandSwing > 0.5))
            {
                Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.fff")} - HS Off: {info.MainhandSwing}/{info.OffhandSwing}, Rage: {info.Rage}");
                return true;
            }
            return false;
        }

        public void Execute()
        {
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.D5);
            CooldownStopwatch.Restart();
        }
    }
}