using System.Collections.Generic;

public partial class ExtraFireRateModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.ExtraFireRate;

    public override List<SpawnableStat> GetStats() => [
        new ReloadStat { ValuePercent = -50f },
    ];

}
