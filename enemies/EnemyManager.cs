using System.Collections.Generic;

public partial class EnemyManager : Node
{

    private static readonly Rect2I EnemySpawnArea = new(
        ShapeGame.PlayableArea.Position.X + 128,
        ShapeGame.PlayableArea.Position.Y - 256,
        ShapeGame.PlayableArea.Size.X - 128,
        0
    );

    private static readonly List<EnemyType> _enemyTypes =
        ResourceSearcher.FindInnerResources<EnemyType>("res://enemies");

    public static EnemyManager Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public EnemyType GetRandomEnemyType()
    {
        return _enemyTypes[GD.RandRange(0, _enemyTypes.Count - 1)];
    }

    public Enemy SpawnEnemy(EnemyType type)
    {
        var enemy = type.Scene.Instantiate<Enemy>();
        ShapeGame.Instance.AddChild(enemy);
        enemy.GlobalPosition = GetRandomEnemySpawnLocation();
        return enemy;
    }

    private static Vector2I GetRandomEnemySpawnLocation()
    {
        var x = GD.RandRange(EnemySpawnArea.Position.X, EnemySpawnArea.End.X);
        var y = GD.RandRange(EnemySpawnArea.Position.Y, EnemySpawnArea.End.Y);

        return new Vector2I(x, y);
    }

}