using System;
using System.Collections.Generic;
using System.Linq;

public static class ComponentManager
{

    public static IEnumerable<T> GetComponents<T>(this IProjectile<Node2D> projectile) where T : IModuleComponent
    {
        return projectile.Node.GetChildren().OfType<T>();
    }

    public static T? GetComponentOrNull<T>(this IProjectile<Node2D> projectile) where T : IModuleComponent
    {
        return GetComponents<T>(projectile).FirstOrDefault();
    }

    public static T GetComponent<T>(this IProjectile<Node2D> projectile) where T : IModuleComponent
    {
        return GetComponentOrNull<T>(projectile) ??
               throw new Exception($"{projectile} does not contain module {typeof(T)}");
    }
    
    public static bool HasComponent<T>(this IProjectile<Node2D> projectile) where T : IModuleComponent
    {
        return projectile.Node.GetChildren().Any(child => child is T);
    }

}