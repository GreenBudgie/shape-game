using System.Collections.Generic;

public class Level2 : Level
{
    public override int Number => 2;

    public override FloatRange PolysteroidTimeToSpawn => new(10f, 15f);

    public override List<LevelPhase> Phases =>
    [
        new()
        {
            Repetitions = 2,
            EnemySpawnDelay = 7,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Rhombus)
            ]
        },

        new()
        {
            EnemySpawnDelay = 8,
            EnemyBatchSize = 2,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Rhombus)
            ]
        },

        new()
        {
            Repetitions = 4,
            EnemySpawnDelay = 7,
            EnemyBatchSize = 2,
            EnemyTypeDistributions =
            [
                new EnemyTypeDistribution(EnemyTypeRegistry.Square),
                new EnemyTypeDistribution(EnemyTypeRegistry.Rhombus)
            ]
        },
    ];
}