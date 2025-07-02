public partial class LevelManager : Node
{


    public static LevelManager Instance { get; private set; } = null!;

    private int _level;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        StartLevel(1);
    }

    public void StartLevel(int level)
    {
        _level = level;

        CallDeferred(MethodName.SpawnEnemy);
        var tween = GetTree().CreateTween().SetLoops();
        tween.TweenCallback(Callable.From(SpawnEnemy)).SetDelay(5.0f);
    }

    private void SpawnEnemy()
    {
        EnemyManager.Instance.SpawnEnemy(EnemyManager.Instance.GetRandomEnemyType());
    }


}