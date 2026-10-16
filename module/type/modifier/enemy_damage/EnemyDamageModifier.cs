using System.Collections.Generic;

public class EnemyDamageModifier(float additionalDamagePercent) : ISpawnableModifier
{
    public List<SpawnableStat> GetStats()
    {
        if (additionalDamagePercent == 0)
        {
            return [];
        }

        return [new DamageStat { ValuePercent = additionalDamagePercent }];
    }

    public void Modify(SpawnableContext context)
    {
    }
}