using System.Collections.Generic;

public class MineModule : SpawnableModule
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b4uocapwodajq");

    public override ModuleShape Shape => ModuleShapeRegistry.Triple;

    public override string Name => "Mine";

    public override string Description => "A mine";

    public override int Price => 15;

    public override List<SpawnableStat> Stats => [
        new SpeedStat { Value = 5000 },
        new ReloadStat { Value = 3 },
        new ExplosionDamageStat { Value = 10 },
        new ExplosionRadiusStat { Value = 400 },
        new LifetimeStat { Value = 0.5f },
    ];

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return MineProjectile.Create();
    }

}
