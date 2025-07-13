using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class HeroicStrikeOnAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if(Constants.WeaponType == WeaponType.TwoHanded || info.TargetBanished || CooldownStopwatch.ElapsedMilliseconds < 300)
                return false;

            if (!Constants.HeroicStrikeQueuingEnabled && info.Rage < 70)
                return false;

            if (!info.HeroicStrikePressed && info.HeroicStrikeUsable && info.MainhandSwing > 0.4 && info.OffhandSwing < 0.3 && info.OffhandSwing != 0)
            {
                Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.fff")} - HS On: {info.MainhandSwing}/{info.OffhandSwing}, Rage: {info.Rage}");
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