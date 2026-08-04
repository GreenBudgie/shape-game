using System.Collections.Generic;

public static class ModuleRegistry
{
    // Filled automatically in Module constructor
    public static readonly List<Module> Modules = [];

    public static readonly ExtraDamageModule ExtraDamage = new();
    public static readonly ExtraFireRateModule ExtraFireRate = new();
    public static readonly PiercingModule Piercing = new();
    public static readonly MassiveShotModule MassiveShot = new();

    public static readonly BoltModule Bolt = new();
    public static readonly MineModule Mine = new();
    public static readonly MiniSphereModule MiniSphere = new();
    public static readonly BarrierModule Barrier = new();
    public static readonly YinYangModule YinYang = new();
    
    public static readonly TriggerModule Trigger = new();
}