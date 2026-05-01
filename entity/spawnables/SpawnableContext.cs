using System.Collections.Generic;
using System.Linq;

public class SpawnableContext(ISpawnable<Node2D> spawnable)
{

    public ISpawnable<Node2D> Spawnable { get; } = spawnable;

    /// <summary>
    /// Where the spawnable should be created, in global coords
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// How the spawnable should be oriented. Used by some, but not all spawnables 
    /// </summary>
    public Vector2 Direction { get; set; } = Vector2.Up;
    
    /// <summary>
    /// Who directly created this spawnable. Might be null if source is programmatic
    /// </summary>
    public Node2D? Source { get; set; }

    private Node2D? _originalSource;
    
    /// <summary>
    /// Stores the original creator of a chain of spawnables. Propagates if some spawnable created another spawnable,
    /// creating a chain.
    ///
    /// Might be null if source is programmatic
    /// </summary>
    public Node2D? OriginalSource
    {
        get => _originalSource ?? Source;
        set => _originalSource = value;
    }

    public List<SpawnableStat> Stats { get; } = [];
    public List<ISpawnableModifier> AppliedModifiers { get; } = [];
    
    public List<TStat> GetStats<TStat>() where TStat : SpawnableStat
    {
        return Stats.OfType<TStat>().ToList();
    }
    
    public float CalculateStat<TStat>() where TStat : SpawnableStat
    {
        return Stats.OfType<TStat>().Sum(stat => RandomUtils.DeltaRange(stat.Value, stat.ValueDelta));
    }

    public bool HasStat<TStat>() where TStat : SpawnableStat
    {
        return Stats.OfType<TStat>().Any();
    }
    
    public bool IsModifierTypeApplied<T>() where T : ISpawnableModifier
    {
        return AppliedModifiers.Any(modifier => modifier.GetType() == typeof(T));
    }

    /// <summary>
    /// Performs all the preparation logic and adds the spawnable to the tree
    /// </summary>
    public void Spawn()
    {
        foreach (var component in Spawnable.GetComponents())
        {
            component.Prepare(this);
        }

        Spawnable.Node.GlobalPosition = Position;

        Spawnable.Prepare(this);
        ShapeGame.Instance.AddChild(Spawnable.Node);
    }

}
