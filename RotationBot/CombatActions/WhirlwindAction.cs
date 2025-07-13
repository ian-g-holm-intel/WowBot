using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class WhirlwindAction : CombatActionBase, ICombatAction
    {
        private readonly RageManager rageManager;
        public WhirlwindAction(RageManager rageManager)
        {
            this.rageManager = rageManager;
        }

        public bool ShouldExecute(CombatInfo info)
        {
            if(info.OnGCD || !info.WhirlwindUsable || info.TargetBanished || info.PolymorphNearby || CooldownStopwatch.ElapsedMilliseconds < 1000)
                return false;

            if (!info.ShouldAOE && info.BloodthirstCD <= 1.5)
                return false;

            var critMultiplier = (1 - info.CritChance / 100) + info.CritChance / 100 * 2.2;
            var apDPS = Convert.ToDouble(info.AttackPower) / 14;
            var apDamage = apDPS * Constants.MainhandSpeed;
            var mainhandDamage = (Constants.MainhandAvgDamage + apDamage) * critMultiplier;
            var whirlwindDPS = (Constants.MainhandAvgDamage + (3.3 * info.AttackPower / 14)) * critMultiplier / 10;

            if ((mainhandDamage * 0.2 * (1 - info.MainhandSwing / info.MainhandAtkSpeed) > whirlwindDPS * info.MainhandSwing) && info.Rage < 65)
                return false;

            return info.Rage - 25 + info.BloodthirstCD * rageManager.GetRagePerSecond() > 30 || info.ShouldAOE;
        }

        public void Execute()
        {
            Console.WriteLine("Whirlwind");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.D2);
            CooldownStopwatch.Restart();
        }
    }
}
