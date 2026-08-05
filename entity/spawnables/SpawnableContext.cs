using System.Collections.Generic;
using System.Linq;

public class SpawnableContext(ISpawnable<Node2D> spawnable)
{
    
    public List<SpawnableContext> Triggers { get; private set; } = [];
    
    public List<SpawnableContext> GetTriggerChain()
    {
        return Triggers.SelectMany(trigger => trigger.GetTriggerChain()).Concat(Triggers).ToList();
    }

    /// <summary>
    /// Context that this context was inherited from.
    /// </summary>
    public SpawnableContext? ParentContext { get; set; }

    /// <summary>
    /// Contexts that inherited from this context
    /// </summary>
    public List<SpawnableContext> ChildContexts { get; } = [];

    /// <summary>
    /// Recursively collects all child contexts (children of children, and so on) and returns them as well as the
    /// current context.
    ///
    /// Useful when needed to perform operations on all inherited contexts like they are one spawnable.
    /// </summary>
    public List<SpawnableContext> GetContextChain()
    {
        return ChildContexts.SelectMany(childContext => childContext.GetContextChain())
            .Prepend(this)
            .ToList();
    }
    
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

    public List<SpawnableStat> Stats { get; private set; } = [];
    
    /// <summary>
    /// Modifiers that are about to be applied to this module when it spawns
    /// </summary>
    public IEnumerable<ISpawnableModifier> Modifiers { get; set; } = [];
    
    /// <summary>
    /// Modifiers that were already applied
    /// </summary>
    public List<ISpawnableModifier> AppliedModifiers { get; } = [];
    
    public List<TStat> GetStats<TStat>() where TStat : SpawnableStat
    {
        return Stats.OfType<TStat>().ToList();
    }
    
    public float CalculateStat<TStat>() where TStat : SpawnableStat
    {
        return CalculateStat<TStat>(false);
    }
    
    public float CalculateStatWithTriggers<TStat>() where TStat : SpawnableStat
    {
        return CalculateStat<TStat>(true);
    }
    
    private float CalculateStat<TStat>(bool includeTriggers) where TStat : SpawnableStat
    {
        var statsOfType = Stats.OfType<TStat>().ToList();
        if (includeTriggers)
        {
            foreach (var trigger in GetTriggerChain())
            {
                statsOfType.AddRange(trigger.GetStats<TStat>());
            }
        }
        
        var additiveStats = statsOfType.Where(stat => stat.IsAdditive);
        var multiplicativeStats = statsOfType.Where(stat => !stat.IsAdditive);

        var result = 0f;
        
        foreach (var additiveStat in additiveStats)
        {
            result = additiveStat.Calculate(result);
        }
    
        foreach (var multiplicativeStat in multiplicativeStats)
        {
            result = multiplicativeStat.Calculate(result);
        }

        return result;
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
        ApplyModifiers();
        
        foreach (var component in Spawnable.GetComponents())
        {
            component.Prepare(this);
        }

        Spawnable.Node.GlobalPosition = Position;

        Spawnable.Prepare(this);
        ShapeGame.Instance.AddChild(Spawnable.Node);
        
        foreach (var component in Spawnable.GetComponents())
        {
            component.Apply(this);
        }
    }

    private void ApplyModifiers()
    {
        var modifierStats = Modifiers.SelectMany(modifier => modifier.Stats);
        Stats.AddRange(modifierStats);

        foreach (var modifier in Modifiers)
        {
            modifier.Modify(this);
            AppliedModifiers.Add(modifier);
        }
    }

    public void LaunchTriggers(Vector2 position, Vector2 direction, Node2D source)
    {
        foreach (var trigger in Triggers)
        {
            trigger.Position = position;
            trigger.Direction = direction;
            trigger.Source = source;

            trigger.Spawn();
        }
    }
    
    /// <summary>
    /// Inherits ALL parameters from the parent context. Used when a spawnable is created by another spawnable and
    /// SHOULD inherit its stats and modifiers, like it was created directly. Useful for proxy-like spawnables.
    /// </summary>
    /// <param name="parentContext">Context to inherit from</param>
    public void InheritFrom(SpawnableContext parentContext)
    {
        ParentContext = parentContext;
        parentContext.ChildContexts.Add(this);
        
        Position = parentContext.Position;
        Direction = parentContext.Direction;
        Source = parentContext.Source;
        OriginalSource = parentContext.OriginalSource;
        Stats = parentContext.Stats.ToList();
        Modifiers = parentContext.Modifiers.ToList();
        Triggers = parentContext.Triggers.ToList();
    }

}
