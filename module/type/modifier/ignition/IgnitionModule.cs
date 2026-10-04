using System.Collections.Generic;
using System.Linq;

public partial class IgnitionModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.Ignition;
    
    public override List<SpawnableStat> GetStats() => [
        new BurningStat { Value = 5 },
    ];

}
