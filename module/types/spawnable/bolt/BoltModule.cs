using System.Collections.Generic;

public class BoltModule : SpawnableModule
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://d3af7qu2ct723");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Bolt";

    public override string Description => "Fast and precise projectile. Very reliable!";

    public override int Price => 5;

    public override List<SpawnableStat> Stats => [
        new DamageStat { Value = 2 },
        new SpeedStat { Value = 3000 },
        new ReloadStat { Value = 1 },
        new LifetimeStat { Value = 4 },
    ];

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return BoltProjectile.Create();
    }

}
