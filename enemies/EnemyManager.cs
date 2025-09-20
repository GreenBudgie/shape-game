using System.Collections.Generic;
using System.Diagnostics;

public partial class EnemyManager : Node
{

    private static readonly Rect2 EnemySpawnArea = new(
        ShapeGame.PlayableArea.Position.X + 128,
        ShapeGame.PlayableArea.Position.Y - 256,
        ShapeGame.PlayableArea.Size.X - 128,
        0
    );

    public static readonly List<EnemyType> EnemyTypes =
        ResourceSearcher.FindInnerResources<EnemyType>("res://enemies/types");

    public static EnemyManager Instance { get; private set; } = null!;
    
    [Signal]
    public delegate void EnemyDestroyedEventHandler(Enemy enemy);

    public override void _EnterTree()
    {
        Instance = this;
    }

    public EnemyType GetRandomEnemyType()
    {
        return EnemyTypes[0];
        //return EnemyTypes.GetRandom();
    }

    public Enemy SpawnEnemy(EnemyType type)
    {
        var enemy = type.Scene.Instantiate<Enemy>();
        enemy.GlobalPosition = GetRandomEnemySpawnLocation();
        ShapeGame.Instance.AddChild(enemy);
        return enemy;
    }

    private static Vector2 GetRandomEnemySpawnLocation()
    {
        var x = GD.RandRange(EnemySpawnArea.Position.X, EnemySpawnArea.End.X);
        var y = GD.RandRange(EnemySpawnArea.Position.Y, EnemySpawnArea.End.Y);

        return new Vector2((float)x, (float)y);
    }

}