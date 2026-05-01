using System;
using System.Collections.Generic;
using System.Linq;

public static class ComponentManager
{
    
    public static IEnumerable<ISpawnableComponent> GetComponents(this ISpawnable<Node2D> projectile)
    {
        return projectile.Node.GetChildren().OfType<ISpawnableComponent>();
    }

    public static IEnumerable<T> GetComponents<T>(this ISpawnable<Node2D> projectile) where T : ISpawnableComponent
    {
        return projectile.Node.GetChildren().OfType<T>();
    }

    public static T? GetComponentOrNull<T>(this ISpawnable<Node2D> projectile) where T : ISpawnableComponent
    {
        return GetComponents<T>(projectile).FirstOrDefault();
    }

    public static T GetComponent<T>(this ISpawnable<Node2D> projectile) where T : ISpawnableComponent
    {
        return GetComponentOrNull<T>(projectile) ??
               throw new Exception($"{projectile} does not contain module {typeof(T)}");
    }
    
    public static bool HasComponent<T>(this ISpawnable<Node2D> projectile) where T : ISpawnableComponent
    {
        return projectile.Node.GetChildren().Any(child => child is T);
    }

}