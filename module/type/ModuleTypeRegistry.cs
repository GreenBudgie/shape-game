using System.Collections.Generic;

public static class ModuleTypeRegistry
{
    // Filled automatically in Module constructor
    public static readonly List<ModuleType> Types = [];

    public static readonly ExtraDamageModuleType ExtraDamage = new();
    public static readonly ExtraFireRateModuleType ExtraFireRate = new();
    public static readonly PiercingModuleType Piercing = new();
    public static readonly MassiveShotModuleType MassiveShot = new();

    public static readonly BoltModuleType Bolt = new();
    public static readonly MineModuleType Mine = new();
    public static readonly MiniSphereModuleType MiniSphere = new();
    public static readonly BarrierModuleType Barrier = new();
    public static readonly YinYangModuleType YinYang = new();
    
    public static readonly TriggerModuleType Trigger = new();
}