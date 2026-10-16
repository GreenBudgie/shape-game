using System;
using System.Collections.Generic;

public abstract partial class Module : Node, IStatsAware
{

    public Module()
    {
        ShapeGame.Instance.AddChild(this);
    }

    public abstract List<SpawnableStat> GetStats();

    public abstract ModuleType Type { get; }

    public bool IsActive { get; private set; }

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

    protected ModuleEffect ShowEffect(string text, Color textColor)
    {
        return ModuleEffectManager.Instance.ShowEffect(this, text, textColor);
    }

    public void Remove()
    {
        QueueFree();
    }
    
}