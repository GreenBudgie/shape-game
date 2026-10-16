using System.Collections.Generic;

public partial class TargetingModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.Targeting;

    public override ISpawnableModifier CreateModifier(SpawnableContext context)
    {
        return TargetingModifier.Instance;
    }

    private class TargetingModifier : ISpawnableModifier
    {

        public static readonly TargetingModifier Instance = new();
        
        public List<SpawnableStat> GetStats() => [
            new ReloadStat { Value = 0.2f },
        ];

        
        public void Modify(SpawnableContext context)
        {
            context.AddComponentToChain(ComponentTypeRegistry.Targeting);
        }
        
    }

}
