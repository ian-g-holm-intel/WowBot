using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class BattleShoutAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if(info.OnGCD || CooldownStopwatch.ElapsedMilliseconds < 1000)
                return false;

            return info.NeedBattleShout;
        }

        public void Execute()
        {
            Console.WriteLine("Battle Shout");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.E);
            CooldownStopwatch.Restart();
        }
    }
}
