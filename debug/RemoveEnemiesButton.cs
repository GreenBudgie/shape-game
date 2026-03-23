public partial class RemoveEnemiesButton : Button
{
    public override void _Pressed()
    {
        foreach (var enemy in EnemyManager.Instance.GetAliveEnemies())
        {
            enemy.Destroy(dropCrystals: false);
        }
    }
}