using System.Collections.Generic;

public partial class EnemyDamageModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.EnemyDamage;

    private EnemyDamageModuleData _data = new();

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

        _data.AdditionalDamagePercent++;
    }

    public override List<SpawnableStat> GetStats()
    {
        if (_data.AdditionalDamagePercent == 0)
        {
            return [];
        }

        return [new DamageStat { ValuePercent = _data.AdditionalDamagePercent }];
    }

}
