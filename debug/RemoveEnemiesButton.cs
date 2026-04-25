public partial class RemoveEnemiesButton : Button
{
    public override void _Pressed()
    {
        EnemyManager.Instance.EnemiesDropCrystals = false;
        foreach (var enemy in EnemyManager.Instance.GetAliveEnemies())
        {
            enemy.HealthController.Destroy();
        }
        EnemyManager.Instance.EnemiesDropCrystals = true;
    }
}