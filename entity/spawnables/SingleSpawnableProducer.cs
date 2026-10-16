using System;
using System.Collections.Generic;

/// <summary>
/// Most basic spawnable producer that does not allow reusing it. Useful for enemy projectiles and other environmental
/// stuff, but not for complex player shooting system.
/// </summary>
public class SingleSpawnableProducer(ISpawnable spawnable) : ISpawnableProducer
{
    public List<SpawnableStat> GetStats() => [];

    private bool _isCreated;
    
    public ISpawnable CreateSpawnable()
    {
        if (_isCreated)
        {
            throw new InvalidOperationException("SingleSpawnableProducer cannot produce spawnables multiple times");
        }
        
        _isCreated = true;
        return spawnable;
    }
}