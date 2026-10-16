using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class SpawnableContext(ISpawnableProducer spawnableProducer)
{

    public SpawnableContextState State = SpawnableContextState.New;
    
    public ISpawnableProducer SpawnableProducer => spawnableProducer;

    private List<DelegatorData> _delegators = [];
    
    private List<SpawnableContext> GetAllDelegatedContexts()
    {
        var directDelegatedContexts = _delegators.Select(delegator => delegator.TargetContext).ToList();
        return directDelegatedContexts.SelectMany(context => context.GetAllDelegatedContexts())
            .Concat(directDelegatedContexts)
            .ToList();
    }

    public void AddLifecycleDelegator(ISpawnableContextLifecycleDelegator delegator, SpawnableContext targetContext)
    {
        if (State is not SpawnableContextState.New)
        {
            throw new InvalidOperationException("Delegators can only be added to SpawnableContext in New state");
        }
        
        _delegators.Add(new DelegatorData(delegator, targetContext));
    }

    /// <summary>
    /// The spawnable itself. Not null if state of the context is not New. For outside callers this is almost always
    /// true, so it's not null for convenience. 
    /// </summary>
    public ISpawnable Spawnable { get; private set; } = null!;

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

    /// <summary>
    /// Stats that are externally added to the context on its creation
    /// </summary>
    private List<SpawnableStat> _customStats = [];
    
    /// <summary>
    /// Stats from spawnable producer, modifiers and delegators
    /// </summary>
    private List<SpawnableStat> _bakedStats = [];
    
    /// <summary>
    /// Modifiers that are about to be applied to this module when it spawns
    /// </summary>
    public IEnumerable<ISpawnableModifier> Modifiers { get; set; } = [];
    
    /// <summary>
    /// Modifiers that were already applied
    /// </summary>
    public List<ISpawnableModifier> AppliedModifiers { get; } = [];

    private void ValidateStatModificationAllowed()
    {
        if (State is not SpawnableContextState.New)
        {
            throw new InvalidOperationException(
                $"SpawnableContext in state {State} does not allow stat modifications"
            );
        }
    }
    
    private void ValidateStatRetrievalAllowed()
    {
        if (State is SpawnableContextState.New)
        {
            throw new InvalidOperationException(
                $"SpawnableContext in state {State} still allows stat modification, so retrieving stats is currently prohibited"
            );
        }
    }
    
    public void AddStat(SpawnableStat stat)
    {
        ValidateStatModificationAllowed();
        _customStats.Add(stat);
    }
    
    public void AddStats(IEnumerable<SpawnableStat> stats)
    {
        ValidateStatModificationAllowed();
        _customStats.AddRange(stats);
    }
    
    public ImmutableList<TStat> GetStats<TStat>() where TStat : SpawnableStat
    {
        ValidateStatRetrievalAllowed();
        return _customStats.Concat(_bakedStats).OfType<TStat>().ToImmutableList();
    }

    private readonly Dictionary<Type, float> _calculatedStats = [];
    private readonly Dictionary<Type, float> _calculatedStatsWithDelegated = [];
    
    public float GetStat<TStat>() where TStat : SpawnableStat
    {
        return GetStat<TStat>(false);
    }
    
    public float GetStatWithDelegated<TStat>() where TStat : SpawnableStat
    {
        return GetStat<TStat>(true);
    }
    
    private float GetStat<TStat>(bool withDelegated) where TStat : SpawnableStat
    {
        ValidateStatRetrievalAllowed();
        
        var statType = typeof(TStat);
        
        if (withDelegated && _calculatedStatsWithDelegated.TryGetValue(statType, out var calculatedStatWithDelegated))
        {
            return calculatedStatWithDelegated;
        }
        
        if (!withDelegated && _calculatedStats.TryGetValue(statType, out var calculatedStat))
        {
            return calculatedStat;
        }
        
        var statsOfType = _customStats.Concat(_bakedStats).OfType<TStat>().ToList();
        if (withDelegated)
        {
            foreach (var delegatedContext in GetAllDelegatedContexts())
            {
                statsOfType.AddRange(delegatedContext.GetStats<TStat>());
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

        if (withDelegated)
        {
            _calculatedStatsWithDelegated.Add(statType, result);
        }
        else
        {
            _calculatedStats.Add(statType, result);
        }
        
        return result;
    }

    public bool IsModifierTypeApplied<T>() where T : ISpawnableModifier
    {
        return AppliedModifiers.Any(modifier => modifier.GetType() == typeof(T));
    }

    /// <summary>
    /// Calculates stats without creating the spawnable. Only possible for New context.
    /// </summary>
    public void CalculateStats()
    {
        if (State is not SpawnableContextState.New)
        {
            throw new InvalidOperationException("Stats can only be calculated in New state");
        }
        
        _bakedStats.AddRange(spawnableProducer.GetStats());
        
        var modifierStats = Modifiers.SelectMany(modifier => modifier.GetStats());
        _bakedStats.AddRange(modifierStats);

        var delegatorsStats = _delegators.SelectMany(delegator => delegator.Delegator.GetStats());
        _bakedStats.AddRange(delegatorsStats);

        State = SpawnableContextState.StatsCalculated;
    }

    /// <summary>
    /// Creates the spawnable and changes State to SpawnableCreated. Does not add the spawnable to the tree.
    /// Spawn() must be called if this method was used, otherwise the created spawnable will be leaked.
    /// </summary>
    public void CreateSpawnable()
    {
        if (State is not SpawnableContextState.StatsCalculated)
        {
            throw new InvalidOperationException("Spawnable can only created after StatsCalculated state");
        }
        
        Spawnable = spawnableProducer.CreateSpawnable();
        Spawnable.Context = this;

        foreach (var delegator in _delegators)
        {
            delegator.Delegator.DelegateLifecycle(this, delegator.TargetContext);
        }
                
        Spawnable.Setup(this);
                
        State = SpawnableContextState.SpawnableCreated;
    }

    /// <summary>
    /// Prepares the spawnable and adds it to the tree. This method should only be called after CreateSpawnable().
    /// </summary>
    public void Spawn()
    {
        if (State is not SpawnableContextState.SpawnableCreated)
        {
            throw new InvalidOperationException("Spawnable can only be spawned in SpawnableCreated state");
        }
        
        foreach (var modifier in Modifiers)
        {
            modifier.Modify(this);
            AppliedModifiers.Add(modifier);
        }

        Spawnable.AddAutoComponents();
        
        foreach (var component in Spawnable.GetComponents())
        {
            component.Prepare(this);
        }

        Spawnable.Node.GlobalPosition = Position;

        Spawnable.Prepare(this);

        if (Engine.IsInPhysicsFrame())
        {
            Callable.From(AddToTree).CallDeferred();
        }
        else
        {
            AddToTree();
        }
    }

    /// <summary>
    /// Performs all the remaining preparation logic and adds the spawnable to the tree.
    /// Use this method if precise control on context lifecycle is not required.
    /// </summary>
    public void ForceSpawn()
    {
        if (State is SpawnableContextState.New)
        {
            CalculateStats();
        }

        if (State is SpawnableContextState.StatsCalculated)
        {
            CreateSpawnable();
        }

        Spawn();
    }

    private void AddToTree()
    {
        ShapeGame.Instance.AddChild(Spawnable!.Node);

        State = SpawnableContextState.Spawned;
        
        Spawnable.Node.AddToGroup(SpawnableManager.SpawnablesGroupName);

        foreach (var component in Spawnable.GetComponents())
        {
            component.Apply(this);
        }
    }

    public SpawnableContext Clone()
    {
        return Clone(spawnableProducer);
    }
    
    /// <summary>
    /// Creates a deep copy of this context with the same parameters, but different spawnable, and independently
    /// cloned triggers. Used so that a context can be spawned more than once (e.g. one per YinYang sphere).
    /// </summary>
    public SpawnableContext Clone(ISpawnableProducer producer)
    {
        var clonedDelegators = _delegators
            .Select(delegator => delegator with { TargetContext = delegator.TargetContext.Clone() })
            .ToList();
        return new SpawnableContext(producer)
        {
            Position = Position,
            Direction = Direction,
            Source = Source,
            OriginalSource = OriginalSource,
            _customStats = _customStats.ToList(),
            Modifiers = Modifiers.ToList(),
            _delegators = clonedDelegators,
        };
    }

    private readonly record struct DelegatorData(
        ISpawnableContextLifecycleDelegator Delegator,
        SpawnableContext TargetContext
    );

}
