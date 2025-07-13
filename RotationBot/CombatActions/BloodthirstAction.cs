using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class BloodthirstAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if(info.OnGCD || info.TargetBanished || CooldownStopwatch.ElapsedMilliseconds < 1000) 
                return false;

            if(info.ShouldAOE && info.WhirlwindUsable && !info.PolymorphNearby)
                return false;

            var critMultiplier = (1 - info.CritChance / 100) + info.CritChance / 100 * 2.2;
            var apDPS = Convert.ToDouble(info.AttackPower) / 14;
            var apDamage = apDPS * Constants.MainhandSpeed;
            var mainhandDamage = (Constants.MainhandAvgDamage + apDamage) * critMultiplier;
            var bloodthirstDPS = (info.AttackPower * 0.45 * critMultiplier) / 6;
            
            if((mainhandDamage * 0.2 * (1 - info.MainhandSwing / info.MainhandAtkSpeed) > bloodthirstDPS * info.MainhandSwing) && info.Rage < 65)
                return false;

            return info.BloodthirstUsable;
        }

        public void Execute()
        {
            Console.WriteLine("Bloodthirst");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.D6);
            CooldownStopwatch.Restart();
        }
    }
}