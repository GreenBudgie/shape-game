using System;
using System.Linq;
using Godot.Collections;

[GlobalClass]
public partial class Level : Resource
{
    [Export] public int Number { get; private set; }

    [Export(PropertyHint.None, "suffix:units")]
    public int DestroyRequirement { get; private set; }

    [Export(PropertyHint.None, "suffix:sec")]
    public int SurviveRequirement { get; private set; }
    
    /// <summary>
    /// Phase represents the "wave" of enemy spawns. Duration of the phase controls how frequently new
    /// enemies are spawned. Phases start from 1.
    /// </summary>
    [Export(PropertyHint.None, "suffix:sec")]
    [ExportCategory("Phase Duration")]
    public float PhaseDuration { get; private set; } = 5;
    
    /// <summary>
    /// How much faster each next phase will start
    /// </summary>
    [Export(PropertyHint.None, "suffix:sec")]
    public float PhaseDurationDec { get; private set; }

    [Export(PropertyHint.None, "suffix:sec")]
    public float MinPhaseDuration { get; private set; } = 5;
    
    /// <summary>
    /// Determines the number of enemies to be spawned each phase.
    /// </summary>
    [Export]
    [ExportCategory("Number of Enemies")]
    public int EnemiesPerPhase { get; private set; }
    
    /// <summary>
    /// How much more enemies will spawn each next phase. Can be decimal, but floored to int
    /// </summary>
    [Export]
    public float EnemiesPerPhaseInc { get; private set; }

    [Export]
    public int MaxEnemiesPerPhase { get; private set; }

    [ExportCategory("Distributions")]
    [Export] public Array<EnemyTypeDistribution> EnemyTypeDistributions { get; private set; } = [];

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