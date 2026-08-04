using System.Collections.Generic;

public static class ModuleRegistry
{
    // Filled automatically in Module constructor
    public static readonly List<Module> Modules = [];

    public static readonly ExtraDamageModule ExtraDamageModule = new();
    public static readonly ExtraFireRateModule ExtraFireRateModule = new();
    public static readonly PiercingModule PiercingModule = new();
    public static readonly MassiveShotModule MassiveShotModule = new();

    public static readonly BoltModule BoltModule = new();
    public static readonly MineModule MineModule = new();
    public static readonly MiniSphereModule MiniSphereModule = new();
    public static readonly BarrierModule BarrierModule = new();
    public static readonly YinYangModule YinYangModule = new();
}