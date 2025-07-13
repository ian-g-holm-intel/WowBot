using System.Diagnostics;

namespace RotationBot.CombatActions
{
    public abstract class CombatActionBase
    {
        protected Stopwatch CooldownStopwatch { get; set; } = Stopwatch.StartNew();
    }
}
