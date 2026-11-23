using System.Collections.Generic;

public partial class LessSpeedModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.LessSpeed;

    public override ISpawnableModifier CreateModifier()
    {
        return LessSpeedModifier.Instance;
    }

    private class LessSpeedModifier : ISpawnableModifier
    {

        public static readonly LessSpeedModifier Instance = new();

        public List<SpawnableStat> GetStats() => [
            new SpeedStat { ValueMult = 0.5f },
        ];

        public void Modify(SpawnableContext context)
        {
        }

    }

}
