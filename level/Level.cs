using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

public abstract class Level
{

    public Level()
    {
        LevelRegistry.Levels.Add(this);
    }
    
    public abstract int Number { get; }

    public abstract int DestroyRequirement { get; }
    
    /// <summary>
    /// Phase represents the "wave" of enemy spawns. Duration of the phase controls how frequently new
    /// enemies are spawned. Phases start from 1.
    /// </summary>
    public abstract float PhaseDuration { get; }
    
    /// <summary>
    /// How much faster each next phase will start
    /// </summary>
    public abstract float PhaseDurationDec { get; }

    public abstract float MinPhaseDuration { get; }
    
    /// <summary>
    /// Determines the number of enemies to be spawned each phase.
    /// </summary>
    public abstract int EnemiesPerPhase { get; }
    
    /// <summary>
    /// How much more enemies will spawn each next phase. Can be decimal, but floored to int
    /// </summary>
    public abstract float EnemiesPerPhaseInc { get; }

    public abstract int MaxEnemiesPerPhase { get; }

    public abstract List<EnemyTypeDistribution> EnemyTypeDistributions { get; }

    /// <summary>
    /// Returns a random EnemyType based on the weights of EnemyTypeDistributions, considering phase delays.
    /// </summary>
    /// <param name="phase">The current phase of the level</param>
    /// <returns>An EnemyType selected based on weighted probability, or null if no eligible enemies exist.</returns>
    public EnemyType GetRandomWeightedEnemyType(int phase)
    {
        // Filter eligible enemy types based on phase delay
        var eligibleDistributions = EnemyTypeDistributions
            .Where(dist => dist.PhaseDelay < phase)
            .ToArray();

        if (eligibleDistributions.Length == 0)
        {
            throw new Exception("No eligible enemy types for the current phase.");
        }

        // Calculate total weight
        var totalWeight = eligibleDistributions.Sum(dist => dist.Weight);

        if (totalWeight <= 0)
        {
            throw new Exception("Total weight of eligible enemy types is zero or negative.");
        }

        // Generate random value between 0 and totalWeight
        var randomValue = GD.Randf() * totalWeight;

        // Select enemy type based on weight
        float cumulativeWeight = 0;
        foreach (var dist in eligibleDistributions)
        {
            cumulativeWeight += dist.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return dist.EnemyType;
            }
        }

        // Fallback in case of rounding errors (should be rare)
        return eligibleDistributions.Last().EnemyType;
    }

    public float GetCurrentPhaseDuration(int phase)
    {
        var currentDurationDecrease = (phase - 1) * PhaseDurationDec;
        var currentDuration = PhaseDuration - currentDurationDecrease;
        return Max(MinPhaseDuration, currentDuration);
    }
    
    public int GetCurrentEnemiesPerPhase(int phase)
    {
        var currentEnemiesIncrease = (phase - 1) * EnemiesPerPhaseInc;
        var currentDuration = EnemiesPerPhase + currentEnemiesIncrease;
        return FloorToInt(Min(MaxEnemiesPerPhase, currentDuration));
    }
    
}