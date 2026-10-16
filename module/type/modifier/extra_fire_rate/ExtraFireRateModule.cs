using System.Collections.Generic;

public partial class ExtraFireRateModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.ExtraFireRate;

    public override ISpawnableModifier CreateModifier(SpawnableContext context)
    {
        return ExtraFireRateModifier.Instance;
    }

    private class ExtraFireRateModifier : ISpawnableModifier
    {

        public static readonly ExtraFireRateModifier Instance = new();

        public List<SpawnableStat> GetStats() => [
            new ReloadStat { ValuePercent = -50f },
        ];

        public void Modify(SpawnableContext context)
        {
        }

    }

}
