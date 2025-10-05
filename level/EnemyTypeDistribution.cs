[GlobalClass]
public partial class EnemyTypeDistribution : Resource
{
    [Export] public EnemyType EnemyType { get; private set; } = null!;

    /// <summary>
    /// Determines the enemy relative likelihood to spawn from the set of enemy type distributions.
    /// Higher weights mean a greater chance of being chosen.
    /// </summary>
    /// <value>A positive number. Defaults to 1.</value>
    /// <remarks>
    /// For example, in a collection with two enemy (weight 4 and weight 2), the first enemy has a 
    /// 4/(4+2) = 66.67% chance to spawn, and the second has a 2/(4+2) = 33.33% chance.
    /// </remarks>
    [Export(PropertyHint.Range, "1,50,1,or_greater")]
    public int Weight { get; private set; } = 1;
    
    /// <summary>
    /// Determines the minimum amount of phases needed to pass before this enemy can be spawned on a level
    /// </summary>
    [Export(PropertyHint.None, "suffix:phases")] public int PhaseDelay { get; private set; }
}