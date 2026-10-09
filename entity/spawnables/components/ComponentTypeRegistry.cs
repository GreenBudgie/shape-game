using System.Collections.Generic;

public static class ComponentTypeRegistry
{
    // Filled automatically in SpawnableComponentType constructor
    public static readonly List<ComponentType> Types = [];
    
    public static readonly InitialImpulseComponentType InitialImpulse = new();
    public static readonly LifetimeComponentType Lifetime = new();
    public static readonly TargetingComponentType Targeting = new();
    public static readonly IgnitionComponentType Ignition = new();
}