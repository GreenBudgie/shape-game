public partial class LevelManager : Node
{
    
    public static LevelManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void LevelStartedEventHandler();
    
    [Signal]
    public delegate void DestroyProgressUpdatedEventHandler(int prevProgress, int newProgress);
    
    [Signal]
    public delegate void SurviveProgressUpdatedEventHandler(float prevProgress, float newProgress);
    
    public int DestroyRequirement { get; private set; }
    public int DestroyProgress { get; private set; }
    
    public int SurviveRequirementSeconds { get; private set; }
    public float SurviveProgressSeconds { get; private set; }
    
    private int _level;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Callable.From(() => StartLevel(1)).CallDeferred();

        EnemyManager.Instance.EnemyDestroyed += OnEnemyDestroyed;
    }

    public override void _Process(double delta)
    {
        if (SurviveProgressSeconds < SurviveRequirementSeconds)
        {
            SetSurviveProgress(Min(SurviveRequirementSeconds, SurviveProgressSeconds + (float)delta));
        }
    }

    public void StartLevel(int level)
    {
        _level = level;
        
        SetDestroyRequirement(20);
        SetSurviveRequirement(20);

        SpawnEnemy();
        var tween = CreateTween().SetLoops();
        tween.TweenCallback(Callable.From(SpawnEnemy)).SetDelay(5.0f);
        
        EmitSignalLevelStarted();
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        if (DestroyProgress < DestroyRequirement)
        {
            SetDestroyProgress(DestroyProgress + 1);
        }
    }

    private void SetDestroyRequirement(int requirement)
    {
        DestroyRequirement = requirement;
        SetDestroyProgress(0);
    }

    private void SetDestroyProgress(int progress)
    {
        var prevDestroyProgress = DestroyProgress;
        DestroyProgress = progress;
        EmitSignalDestroyProgressUpdated(prevDestroyProgress, DestroyProgress);
    }
    
    private void SetSurviveRequirement(int requirementSeconds)
    {
        SurviveRequirementSeconds = requirementSeconds;
        SetSurviveProgress(0);
    }

    private void SetSurviveProgress(float progressSeconds)
    {
        var prevSurviveProgress = SurviveProgressSeconds;
        SurviveProgressSeconds = progressSeconds;
        EmitSignalSurviveProgressUpdated(prevSurviveProgress, SurviveProgressSeconds);
    }

    private void SpawnEnemy()
    {
        EnemyManager.Instance.SpawnEnemy(EnemyManager.Instance.GetRandomEnemyType());
    }


}