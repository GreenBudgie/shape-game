public partial class SpawnEnemiesContainer : VBoxContainer
{
    public override void _Ready()
    {
        foreach (var enemyType in EnemyManager.EnemyTypes)
        {
            var hBoxContainer = new HBoxContainer();
            hBoxContainer.AddThemeConstantOverride("separation", 12);
            
            var spawnEnemyButton = new Button();
            spawnEnemyButton.Text = $"Spawn {enemyType.Name}";
            spawnEnemyButton.AddThemeFontSizeOverride("font_size", 64);
            spawnEnemyButton.Pressed += () => EnemyManager.Instance.SpawnEnemy(enemyType);
            hBoxContainer.AddChild(spawnEnemyButton);
            
            var spawn10Button = new Button();
            spawn10Button.Text = "10";
            spawn10Button.AddThemeFontSizeOverride("font_size", 64);
            spawn10Button.Pressed += () => SpawnTenEnemies(enemyType);
            hBoxContainer.AddChild(spawn10Button);
            
            var centerContainer = new CenterContainer();
            centerContainer.AddChild(hBoxContainer);
            AddChild(centerContainer);
        }
    }

    private void SpawnTenEnemies(EnemyType type)
    {
        for (var i = 0; i < 10; i++)
        {
            EnemyManager.Instance.SpawnEnemy(type);
        }
    }
}