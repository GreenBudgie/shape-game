using System.Collections.Generic;

public static class EntityEffectTypeRegistry
{
    
    // Filled automatically in EntityEffectType constructor
    public static readonly List<EntityEffectType> Types = [];
    
    public static readonly FireEntityEffectType Fire = new();
    
}