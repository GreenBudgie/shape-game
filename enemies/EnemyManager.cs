using System.Collections.Generic;
using System.Linq;

public partial class EnemyManager : Node
{
    
    public static readonly StringName AliveEnemiesGroup = "alive_enemies";

    private static readonly Rect2 EnemySpawnArea = new(
        ShapeGame.PlayableArea.Position.X + 128,
        ShapeGame.PlayableArea.Position.Y - 384,
        ShapeGame.PlayableArea.Size.X - 128,
        128
    );

    public static readonly List<EnemyType> EnemyTypes =
        ResourceSearcher.FindInnerResources<EnemyType>("res://enemies/types");

    public static EnemyManager Instance { get; private set; } = null!;
    
    /// <summary>
    /// Emitted after an enemy has been destroyed. At this point, the enemy is still on screen and in the process
    /// of removal, but it's not present in alive_enemies group.
    /// </summary>
    [Signal]
    public delegate void EnemyDestroyedEventHandler(Enemy enemy);

    public EnemyManager()
    {
        Instance = this;
    }

    public EnemyType GetRandomEnemyType()
    {
        return EnemyTypes.GetRandom();
    }

    public Enemy SpawnEnemy(EnemyType type)
    {
        var enemy = type.Scene.Instantiate<Enemy>();
        enemy.GlobalPosition = GetRandomEnemySpawnLocation();
        ShapeGame.Instance.AddChild(enemy);
        return enemy;
    }

    /// <summary>
    /// Returns a list of currently alive enemies on screen
    /// </summary>
    public IEnumerable<Enemy> GetAliveEnemies()
    {
        return GetTree().GetNodesInGroup(AliveEnemiesGroup).Cast<Enemy>();
    }

    private static Vector2 GetRandomEnemySpawnLocation()
    {
        var x = GD.RandRange(EnemySpawnArea.Position.X, EnemySpawnArea.End.X);
        var y = GD.RandRange(EnemySpawnArea.Position.Y, EnemySpawnArea.End.Y);

        return new Vector2((float)x, (float)y);
    }

}