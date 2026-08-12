using System.Collections.Generic;

public class Level1 : Level
{
    public override int Number => 1;
    public override int DestroyRequirement => 5;

    public override float PolysteroidMinTimeToSpawn => 10f;
    public override float PolysteroidMaxTimeToSpawn => 15f;

    public override List<LevelPhase> Phases =>
    [
        new()
        {
            Duration = 20,
            MinEnemyBatch = 1,
            MaxEnemyBatch = 2,
            MinSpawnDelay = 6,
            MaxSpawnDelay = 10,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square)
            ]
        },

        new()
        {
            Duration = 20,
            MinEnemyBatch = 2,
            MaxEnemyBatch = 3,
            MinSpawnDelay = 5,
            MaxSpawnDelay = 10,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square)
            ]
        },
        
        new()
        {
            MinEnemyBatch = 3,
            MaxEnemyBatch = 4,
            MinSpawnDelay = 4,
            MaxSpawnDelay = 10,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square),
            ]
        },
    ];
}