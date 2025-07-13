namespace WowLib
{
    public class CombatInfo
    {
        public double CritChance { get; set; }
        public double MainhandAtkSpeed { get; set; }
        public double OffhandAtkSpeed { get; set; }
        public double GCD { get; set; }
        public int Rage { get; set; }
        public double MainhandSwing { get; set; }
        public double OffhandSwing { get; set; }
        public int AttackPower { get; set; }
        public double BloodthirstCD { get; set; }
        public double WhirlwindCD { get; set; }
        public bool BloodthirstUsable { get; set; }
        public bool WhirlwindUsable { get; set; }
        public bool ExecuteUsable { get; set; }
        public bool HeroicStrikeUsable { get; set; }
        public bool BloodrageUsable { get; set; }
        public bool ShouldAOE { get; set; }
        public bool NeedSunder { get; set; }
        public bool NeedBattleShout { get; set; }
        public bool BerserkerStance { get; set; }
        public bool HeroicStrikePressed { get; set; }
        public bool GeneratingRage => (MainhandSwing != 0 || OffhandSwing != 0) && InCombat;
        public bool InCombat => AttackPower != 0;
        public bool OnGCD => GCD != 0;
        public bool TargetBanished { get; set; }
        public bool PolymorphNearby { get; set; }
        public bool ChatOpen { get; set; }
    }
}
