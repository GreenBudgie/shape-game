using System.Collections.Generic;
using System.Linq;

public record ShotContext(
    IProjectile<Node2D> Projectile,
    ProjectileModule ProjectileModule,
    List<ModifierModule> Modifiers
)
{

    public List<Module> GetAllModules() => [..Modifiers, ProjectileModule];
    
    public float CalculateStat<T>() where T : ModuleStat
    {
        return GetAllModules()
            .SelectMany(module => module.Stats)
            .OfType<T>()
            .Sum(stat => stat.Value);
    }

}