using System;
using System.Collections.Generic;
using System.Linq;

public static class ComponentManager
{
    
    public static Component? AddComponentIfPossible(this ISpawnable spawnable, ComponentType type)
    {
        if (!type.IsApplicableTo(spawnable))
        {
            return null;
        }
        
        var createdComponent = type.CreateComponent(spawnable);
        spawnable.Node.AddChild(createdComponent);
        return createdComponent;
    }
    
    public static IEnumerable<Component> GetComponents(this ISpawnable projectile)
    {
        return projectile.GetComponents<Component>();
    }
    
    public static IEnumerable<T> GetComponents<T>(this ISpawnable projectile) where T : Component
    {
        return projectile.Node.GetChildren().OfType<T>();
    }
    
    public static T GetSingleComponent<T>(this ISpawnable projectile) where T : Component
    {
        return projectile.GetComponents<T>().Single();
    }

    public static bool HasComponentType(this ISpawnable spawnable, ComponentType type)
    {
        return spawnable.GetComponents().Any(component => component.Type == type);
    }

    public static void AddAutoComponents(this ISpawnable spawnable)
    {
        foreach (var type in ComponentTypeRegistry.Types)
        {
            if (type.ShouldAutoAdd(spawnable))
            {
                spawnable.AddComponentIfPossible(type);
            }
        }
    }

}