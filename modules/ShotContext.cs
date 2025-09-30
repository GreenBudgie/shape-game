using System.Collections.Generic;
using System.Linq;

public record ShotContext(
    IProjectile<Node2D> Projectile,
    ProjectileModule ProjectileModule,
    List<ModifierModule> Modifiers
)
{
    
    public List<ModifierModule> AppliedModifiers { get; } = [];

    public List<Module> GetAllModules() => [..Modifiers, ProjectileModule];

    public void ApplyStats()
    {
        var stats = GetAllModules()
            .SelectMany(module => module.Stats)
            .DistinctBy(stat => stat.GetType());
        
        foreach (var stat in stats)
        {
            stat.Apply(this);
        }
    }
    
    public float CalculateStat<T>() where T : ModuleStat
    {
        return GetAllModules()
            .SelectMany(module => module.Stats)
            .OfType<T>()
            .Sum(stat => RandomUtils.DeltaRange(stat.Value, stat.ValueDelta));
    }

    public bool IsModifierTypeApplied<T>() where T : ModifierModule
    {
        return AppliedModifiers.Any(modifier => modifier.GetType() == typeof(T));
    }

}