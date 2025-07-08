using System.Collections.Generic;

public partial class EnemyTypeManager : Node
{

    private static readonly Rect2 EnemySpawnArea = new(
        ShapeGame.PlayableArea.Position.X + 128,
        ShapeGame.PlayableArea.Position.Y - 256,
        ShapeGame.PlayableArea.Size.X - 128,
        0
    );

    private static readonly List<EnemyType> EnemyTypes =
        ResourceSearcher.FindInnerResources<EnemyType>("res://enemies/types");

    public static EnemyTypeManager Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public EnemyType GetRandomEnemyType()
    {
        return EnemyTypes[GD.RandRange(0, EnemyTypes.Count - 1)];
    }

    public Enemy SpawnEnemy(EnemyType type)
    {
        var enemy = type.Scene.Instantiate<Enemy>();
        ShapeGame.Instance.AddChild(enemy);
        enemy.GlobalPosition = GetRandomEnemySpawnLocation();
        return enemy;
    }

    private static Vector2 GetRandomEnemySpawnLocation()
    {
        var x = GD.RandRange(EnemySpawnArea.Position.X, EnemySpawnArea.End.X);
        var y = GD.RandRange(EnemySpawnArea.Position.Y, EnemySpawnArea.End.Y);

        return new Vector2((float)x, (float)y);
    }

}