using System;
using System.Collections.Generic;
using System.Linq;

public class LevelPhase
{

    /// <summary>
    /// The number of phase repetitions. To avoid ambiguity, 1 repetition means that enemies in this phase will only
    /// spawn once, 2 is twice, and so on.
    /// </summary>
    public uint Repetitions { get; set; } = 1;

    public UIntRange EnemyBatchSize { get; set; } = 1;

    public FloatRange EnemySpawnDelay { get; set; } = 1f;
    
    public List<EnemyTypeDistribution> EnemyTypeDistributions { get; set; } = [];

    public float GetSpawnDelay()
    {
        return EnemySpawnDelay.Random();
    }
    
    public List<EnemyType> GetEnemyBatch()
    {
        var enemyBatchSize = EnemyBatchSize.Random();
        List<EnemyType> batch = [];
        for (var i = 0; i < enemyBatchSize; i++)
        {
            batch.Add(GetRandomWeightedEnemyType());
        }

        return batch;
    }
    
    /// <summary>
    /// Returns a random EnemyType based on the weights of EnemyTypeDistributions, considering phase delays.
    /// </summary>
    /// <param name="phase">The current phase of the level</param>
    /// <returns>An EnemyType selected based on weighted probability, or null if no eligible enemies exist.</returns>
    public EnemyType GetRandomWeightedEnemyType()
    {
        
        if (EnemyTypeDistributions.Count == 0)
        {
            throw new Exception("No eligible enemy types for the current phase.");
        }

        // Calculate total weight
        var totalWeight = EnemyTypeDistributions.Sum(dist => dist.Weight);

        if (totalWeight <= 0)
        {
            throw new Exception("Total weight of eligible enemy types is zero or negative.");
        }

        // Generate random value between 0 and totalWeight
        var randomValue = GD.Randf() * totalWeight;

        // Select enemy type based on weight
        float cumulativeWeight = 0;
        foreach (var dist in EnemyTypeDistributions)
        {
            cumulativeWeight += dist.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return dist.EnemyType;
            }
        }

        // Fallback in case of rounding errors (should be rare)
        return EnemyTypeDistributions.Last().EnemyType;
    }

}