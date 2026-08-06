using System.Linq;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void LevelStartedEventHandler();

    [Signal]
    public delegate void DestroyProgressUpdatedEventHandler(int prevProgress, int newProgress);

    public Level? Level;

    public int DestroyProgress { get; private set; }

    public int SurviveProgress { get; private set; }

    private double _timeToNextPhase;
    private bool _requirementsMet;
    private int _phase = 1;

    private bool _spawnEnemies = true;

    public LevelManager()
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
        if (Level != null)
        {
            ProcessLevel(delta, Level);
        }
    }

    /// <summary>
    /// Forcefully kills every remaining enemy and ends the level
    /// </summary>
    public void ForceEndLevel()
    {
        if (Level == null)
        {
            return;
        }

        if (!_requirementsMet)
        {
            SetDestroyProgress(Level.DestroyRequirement);
        }

        foreach (var enemy in EnemyManager.Instance.GetAliveEnemies())
        {
            enemy.HealthController.Destroy();
        }
    }

    public bool ToggleEnemySpawning()
    {
        _spawnEnemies = !_spawnEnemies;
        return _spawnEnemies;
    }

    private void ProcessLevel(double delta, Level level)
    {
        if (_requirementsMet)
        {
            return;
        }

        if (_spawnEnemies)
        {
            _timeToNextPhase -= delta;
        }

        if (_timeToNextPhase < 0)
        {
            _timeToNextPhase = level.GetCurrentPhaseDuration(_phase);
            SpawnEnemyBatch();
        }
    }

    private const float PhaseStartMinDuration = 0.3f;

    public void StartNextLevel()
    {
        if (Level == null)
        {
            return;
        }

        var nextLevelNumber = Level.Number + 1;
        StartLevel(nextLevelNumber);
    }

    public void StartLevel(int level)
    {
        GamePhaseManager.Instance.ChangePhase(GamePhase.Level);

        Level = LevelRegistry.GetLevel(level);
        _phase = 1;
        _requirementsMet = false;

        SetDestroyProgress(0);

        PrepareNextPhase();

        EmitSignalLevelStarted();
    }

    public void CheckIfRequirementsMet()
    {
        if (Level == null)
        {
            return;
        }

        if (DestroyProgress < Level.DestroyRequirement)
        {
            return;
        }

        _requirementsMet = true;
        GamePhaseManager.Instance.ChangePhase(GamePhase.Shop);
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        if (Level == null)
        {
            return;
        }

        if (DestroyProgress < Level.DestroyRequirement)
        {
            SetDestroyProgress(DestroyProgress + 1);
        }

        var aliveEnemies = EnemyManager.Instance.GetAliveEnemies();
        if (aliveEnemies.Any())
        {
            return;
        }

        if (_requirementsMet)
        {
            return;
        }

        PrepareNextPhase();
    }

    private void PrepareNextPhase()
    {
        _timeToNextPhase = PhaseStartMinDuration;
    }

    private void SetDestroyProgress(int progress)
    {
        var prevDestroyProgress = DestroyProgress;
        DestroyProgress = progress;
        EmitSignalDestroyProgressUpdated(prevDestroyProgress, DestroyProgress);
        CheckIfRequirementsMet();
    }

    private const float EnemyInBatchSpawnDelay = 0.25f;
    private const float EnemyInBatchSpawnDelayDelta = 0.1f;

    private void SpawnEnemyBatch()
    {
        if (Level == null)
        {
            return;
        }

        for (var i = 0; i < Level.GetCurrentEnemiesPerPhase(_phase); i++)
        {
            var delay = i * RandomUtils.DeltaRange(EnemyInBatchSpawnDelay, EnemyInBatchSpawnDelayDelta);
            if (delay == 0)
            {
                SpawnEnemy();
            }
            else
            {
                GetTree().CreateTimer(delay).Timeout += SpawnEnemy;
            }

            continue;

            void SpawnEnemy() => EnemyManager.Instance.SpawnEnemy(Level.GetRandomWeightedEnemyType(_phase));
        }

        _phase++;
    }
}