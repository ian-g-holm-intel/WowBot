using System;
using WowLib;

namespace RotationBot.CombatActions
{
    public class BerserkerStanceAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if(CooldownStopwatch.ElapsedMilliseconds < 500)
                return false;

            return !info.BerserkerStance && info.Rage <= 25;
        }

        public void Execute()
        {
            Console.WriteLine("Berserker Stance");
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.F3);
            CooldownStopwatch.Restart();
        }
    }
}
