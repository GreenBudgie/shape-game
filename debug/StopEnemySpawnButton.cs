public partial class StopEnemySpawnButton : Button
{
    
    public override void _Pressed()
    {
        var spawn = LevelManager.Instance.ToggleEnemySpawning();
        if (spawn)
        {
            Text = "Stop Enemy\nSpawn";
        }
        else
        {
            Text = "Start Enemy\nSpawn";
        }
    }
    
}
