using System.Collections.Generic;

public class MiniSphereModule : SpawnableModule
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://4p56hm2gcdfy");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Mini Sphere";

    public override string Description => "Many short-lived and inaccurate projectiles";

    public override int Price => 5;

    public override List<SpawnableStat> Stats => [
        new DamageStat { Value = 1 },
        new SpeedStat { Value = 2000 },
        new ReloadStat { Value = 0.1f },
        new LifetimeStat { Value = 1, ValueDelta = 0.1f },
    ];

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return MiniSphereProjectile.Create();
    }

}
