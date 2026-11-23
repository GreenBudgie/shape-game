using System.Collections.Generic;

public partial class ExtraSpeedModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.ExtraSpeed;

    public override ISpawnableModifier CreateModifier()
    {
        return ExtraSpeedModifier.Instance;
    }

    private class ExtraSpeedModifier : ISpawnableModifier
    {

        public static readonly ExtraSpeedModifier Instance = new();

        public List<SpawnableStat> GetStats() => [
            new SpeedStat { ValueMult = 2f },
        ];

        public void Modify(SpawnableContext context)
        {
        }

    }

}
