using System.Collections.Generic;

public partial class EnemyDamageActiveModule(EnemyDamageModuleData data) : ActiveModule(data)
{
    
    public override ModuleType ModuleType => ModuleTypeRegistry.EnemyDamage;

    public override EnemyDamageModuleData Data { get; } = data;

    protected override void OnActivate()
    {
        EnemyManager.Instance.EnemyDestroyed += OnEnemyDestroyed;
    }
    
    protected override void OnDeactivate()
    {
        EnemyManager.Instance.EnemyDestroyed -= OnEnemyDestroyed;
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        if (enemy.IsEnvironmental)
        {
            return;
        }

        Data.AdditionalDamagePercent++;
    }

    public override List<SpawnableStat> GetStats()
    {
        if (Data.AdditionalDamagePercent == 0)
        {
            return [];
        }
        
        return [new DamageStat { ValuePercent = Data.AdditionalDamagePercent }];
    }
    
}