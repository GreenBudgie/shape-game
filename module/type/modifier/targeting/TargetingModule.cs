using System.Collections.Generic;

public partial class TargetingModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.Targeting;

    public override List<SpawnableStat> GetStats() => [
        new ReloadStat { Value = 0.2f },
    ];

    public override void Modify(SpawnableContext context)
    {
        context.AddComponentToChain(ComponentTypeRegistry.Targeting);
    }

}
