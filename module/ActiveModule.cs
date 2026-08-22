using System;
using System.Collections.Generic;

public abstract partial class ActiveModule(ActiveModuleData data) : Node, IStatsAware
{
    
    public abstract ModuleType ModuleType { get; }

    public bool IsActive { get; private set; }
    
    public virtual ActiveModuleData Data { get; } = data;
    
    public virtual List<SpawnableStat> GetStats() => [];

    public IEnumerable<SpawnableStat> Stats => GetStats();

    public void Activate()
    {
        if (IsActive)
        {
            throw new Exception("Module is already active");
        }
        
        IsActive = true;
        OnActivate();
    }
    
    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new Exception("Module is already inactive");
        }
        
        IsActive = false;
        OnDeactivate();
    }
    
    protected virtual void OnActivate()
    {
    }

    protected virtual void OnDeactivate()
    {
    }
    
    public void Spawn()
    {
        ShapeGame.Instance.AddChild(this);
    }

    public void Remove()
    {
        QueueFree();
    }
    
}