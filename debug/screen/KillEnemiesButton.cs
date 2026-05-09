public partial class KillEnemiesButton : Button
{
    public override void _Pressed()
    {
        foreach (var enemy in EnemyManager.Instance.GetAliveEnemies())
        {
            enemy.HealthController.Destroy();
        }
    }
}