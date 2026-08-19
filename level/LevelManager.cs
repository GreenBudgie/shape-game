using System;
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

    private double _timeToNextPhase;
    private double _timeToSpawnEnemies;
    private double _timeToSpawnPolysteroids;
    private bool _requirementsMet;
    private bool _isLastPhase;
    private int _phase;
    private bool _isLevelEnding;
    private double _timeToEndLevel;

    private bool _spawnEnemies = true;

    public LevelManager()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Callable.From(StartFirstLevel).CallDeferred();

        EnemyManager.Instance.EnemyDestroyed += OnEnemyDestroyed;
    }

    public override void _Process(double delta)
    {
        if (Level != null && GamePhaseManager.Instance.Phase == GamePhase.Level)
        {
            ProcessLevel(delta, Level);
        }
    }

    public Level RequireLevel()
    {
        if (Level == null)
        {
            throw new Exception("Level is not running");
        }

        return Level;
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

    private bool ShouldEndLevel()
    {
        if (!_requirementsMet)
        {
            return false;
        }
        
        var hasAliveEnemies = EnemyManager.Instance.GetAliveEnemiesCount() > 0;
        var hasSpawnables = SpawnableManager.Instance.GetSpawnablesCount() > 0;
        return !hasAliveEnemies && !hasSpawnables;
    }

    private const double MaxTimeToEndLevel = 0.5;

    private void EndLevelBegin()
    {
        _isLevelEnding = true;
        _timeToEndLevel = MaxTimeToEndLevel;
    }
    
    private void EndLevel()
    {
        _isLevelEnding = false;
        GamePhaseManager.Instance.ChangePhase(GamePhase.Shop);
    }

    private void ProcessLevel(double delta, Level level)
    {
        if (_isLevelEnding)
        {
            if (!ShouldEndLevel())
            {
                _timeToEndLevel = MaxTimeToEndLevel;
                return;
            }
            
            _timeToEndLevel -= delta;
            if (_timeToEndLevel <= 0)
            {
                EndLevel();
            }
            
            return;
        }
        
        if (ShouldEndLevel())
        {
            EndLevelBegin();
            return;
        }
        
        if (_requirementsMet)
        {
            return;
        }

        if (_spawnEnemies)
        {
            _timeToNextPhase -= delta;
            _timeToSpawnPolysteroids -= delta;

            if (!IsMaxEnemiesReached())
            {
                _timeToSpawnEnemies -= delta;
            }
        }
        
        if (_timeToSpawnEnemies < 0)
        {
            _timeToSpawnEnemies = GetCurrentPhase(level).GetSpawnDelay();
            SpawnEnemyBatch();
        }
        
        if (_timeToSpawnPolysteroids < 0)
        {
            _timeToSpawnPolysteroids =
                RandomUtils.Range(level.PolysteroidMinTimeToSpawn, level.PolysteroidMaxTimeToSpawn);
            SpawnPolysteroid();
        }
        
        if (_timeToNextPhase < 0)
        {
            StartNextPhase(level);
            _timeToNextPhase = GetCurrentPhase(level).Duration;
        }
    }

    public void PrepareNextLevel()
    {
        if (Level == null)
        {
            return;
        }

        GamePhaseManager.Instance.ChangePhase(GamePhase.LevelPreparation);
        
        var player = Player.FindPlayer();
        if (player == null)
        {
            return;
        }
        
        // Move player to the bottom of the screen. Make it invisible due to physics interpolation so it's not
        // flying through the screen
        player.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        player.GlobalPosition = player.GlobalPosition with
        {
            Y = ShapeGame.PlayableArea.End.Y + Player.MaxVisibleSize.Y
        };
            
        Callable.From(() => player.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Inherit).CallNextPhysicsFrame(GetTree());

    }
    
    public void StartNextLevel()
    {
        if (Level == null)
        {
            return;
        }

        var nextLevelNumber = Level.Number + 1;
        StartLevel(nextLevelNumber);
    }

    private void StartFirstLevel()
    {
        StartLevel(1);
    }

    private void StartLevel(int level)
    {
        Level = LevelRegistry.GetLevel(level);

        _timeToNextPhase = 0;
        _timeToSpawnEnemies = 0;
        _requirementsMet = false;
        _isLastPhase = false;
        _phase = 0;
        _isLevelEnding = false;
        _timeToEndLevel = 0;
        _timeToSpawnPolysteroids = RandomUtils.Range(Level.PolysteroidMinTimeToSpawn, Level.PolysteroidMaxTimeToSpawn);
        
        SetDestroyProgress(0);
        GamePhaseManager.Instance.ChangePhase(GamePhase.Level);
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
    }

    public bool IsMaxEnemiesReached()
    {
        if (Level == null)
        {
            return false;
        }
        
        var enemyCount = EnemyManager.Instance.GetNonEnvironmentalAliveEnemies().Count();
        return enemyCount >= Level.MaxEnemies;
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        if (Level == null)
        {
            return;
        }

        if (!enemy.IsEnvironmental && DestroyProgress < Level.DestroyRequirement)
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

        SpawnNextEnemyBatchFaster();
    }

    private void SpawnNextEnemyBatchFaster()
    {
        const float nextBatchMinDelay = 0.5f;
        if (_timeToSpawnEnemies < nextBatchMinDelay)
        {
            return;
        }   
        
        _timeToSpawnEnemies = nextBatchMinDelay;
    }

    private void SetDestroyProgress(int progress)
    {
        var prevDestroyProgress = DestroyProgress;
        DestroyProgress = progress;
        EmitSignalDestroyProgressUpdated(prevDestroyProgress, DestroyProgress);
        CheckIfRequirementsMet();
    }

    private void SpawnPolysteroid()
    {
        if (Level == null)
        {
            return;
        }
        
        EnemyManager.Instance.SpawnEnemy(EnemyTypeRegistry.Polysteroid);
    }

    private void SpawnEnemyBatch()
    {
        if (Level == null)
        {
            return;
        }

        foreach (var enemyType in GetCurrentPhase(Level).GetEnemyBatch())
        {
            EnemyManager.Instance.SpawnEnemy(enemyType);
        }
    }

    private LevelPhase GetCurrentPhase(Level level)
    {
        return level.Phases[_phase];
    }
    
    private void StartNextPhase(Level level)
    {
        if (level.Phases.Count >= _phase - 1)
        {
            _isLastPhase = true;
            return;
        }

        _phase++;
    }
}