using WowLib;

namespace RotationBot.CombatActions
{
    public class BloodrageAction : CombatActionBase, ICombatAction
    {
        public bool ShouldExecute(CombatInfo info)
        {
            if(info.TargetBanished || CooldownStopwatch.ElapsedMilliseconds < 100)
                return false;

            return info.InCombat && (info.BloodrageUsable || (info.MainhandSwing == 0 && info.OffhandSwing == 0));
        }

        public void Execute()
        {
            KeySim.KeyPress(WindowHandles.RotationBot, System.Windows.Forms.Keys.Q);
            CooldownStopwatch.Restart();
        }
    }
}
