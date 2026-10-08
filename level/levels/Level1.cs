using System.Collections.Generic;

public class Level1 : Level
{
    public override int Number => 1;

    public override FloatRange PolysteroidTimeToSpawn => new(10f, 15f);

    public override List<LevelPhase> Phases =>
    [
        new()
        {
            Repetitions = 2,
            EnemySpawnDelay = 7,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square)
            ]
        },

        new()
        {
            Repetitions = 2,
            EnemySpawnDelay = 3,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square)
            ]
        },
        
        new()
        {
            Repetitions = 2,
            EnemyBatchSize = 2,
            EnemySpawnDelay = 2,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square),
            ]
        },
    ];
}