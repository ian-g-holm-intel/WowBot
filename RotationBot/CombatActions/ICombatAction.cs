using WowLib;

namespace RotationBot
{
    public interface ICombatAction
    {
        bool ShouldExecute(CombatInfo info);
        void Execute();
    }
}
