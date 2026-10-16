using System.Collections.Generic;

public partial class IgnitionModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.Ignition;

    public override ISpawnableModifier CreateModifier()
    {
        return IgnitionModifier.Instance;
    }

    private class IgnitionModifier : ISpawnableModifier
    {

        public static readonly IgnitionModifier Instance = new();

        public List<SpawnableStat> GetStats() => [
            new BurningStat { Value = 5 },
        ];

        public void Modify(SpawnableContext context)
        {
        }

    }

}
