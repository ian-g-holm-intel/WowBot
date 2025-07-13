using System.Linq;

namespace RotationBot
{
    public static class Constants
    {
        private static int[] MainhandDamage = new[] { 78, 146 };
        public static double MainhandSpeed = 2.7;
        public static double MainhandAvgDamage = MainhandDamage.Average();
        public static double MainhandDPS = MainhandAvgDamage / MainhandSpeed;
        public static WeaponType WeaponType = WeaponType.DualWield;
        public static bool HeroicStrikeQueuingEnabled = true;
    }

    public enum WeaponType
    {
        TwoHanded,
        DualWield
    }
}
