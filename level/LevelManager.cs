using System;
using System.Linq;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void LevelStartedEventHandler();

    public Level? Level;

    private double _timeToSpawnEnemies;
    private double _timeToSpawnPolysteroids;
    private int _phase;
    private int _phaseRepetition;
    private bool _phasesEnded;
    private bool _isLevelEnding;
    private double _timeToEndLevel;

    private bool _spawnEnemies = true;
    private bool _levelProgress = true;

    public LevelManager()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Callable.From(StartFirstLevel).CallDeferred();

        if (Debug.Enabled)
        {
            DebugDraw.AddInfo(() => $"Level: {Level?.Number}");
            DebugDraw.AddInfo(() => $"Phase: {_phase}; rep: {_phaseRepetition}");
            DebugDraw.AddInfo(() => $"Time: {_timeToSpawnEnemies:F1}");
            DebugDraw.AddInfo(() => $"End: {_phasesEnded}");
        }

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

        foreach (var enemy in EnemyManager.Instance.GetAliveEnemies())
        {
            enemy.HealthController.Destroy();
        }
    }
    
    public bool ToggleLevelProgress()
    {
        _levelProgress = !_levelProgress;
        return _levelProgress;
    }

    public bool ToggleEnemySpawning()
    {
        _spawnEnemies = !_spawnEnemies;
        return _spawnEnemies;
    }

    private bool ShouldEndLevel()
    {
        if (!_phasesEnded)
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
        
        if (_phasesEnded)
        {
            return;
        }

        if (_spawnEnemies)
        {
            _timeToSpawnPolysteroids -= delta;
            _timeToSpawnEnemies -= delta;
        }
        
        if (_timeToSpawnPolysteroids < 0)
        {
            _timeToSpawnPolysteroids = level.PolysteroidTimeToSpawn.Random();
            SpawnPolysteroid();
        }
        
        if (_timeToSpawnEnemies < 0)
        {
            SpawnEnemyBatch();
            StartNextPhaseOrRepetition(level);
            _timeToSpawnEnemies = GetCurrentPhase(level).GetSpawnDelay();
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

        _timeToSpawnEnemies = 0;
        _phase = 0;
        _phaseRepetition = 0;
        _phasesEnded = false;
        _isLevelEnding = false;
        _timeToEndLevel = 0;
        _timeToSpawnPolysteroids = Level.PolysteroidTimeToSpawn.Random();
        
        GamePhaseManager.Instance.ChangePhase(GamePhase.Level);
        EmitSignalLevelStarted();
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        if (!_levelProgress || Level == null || _phasesEnded)
        {
            return;
        }

        var aliveEnemies = EnemyManager.Instance.GetNonEnvironmentalAliveEnemies();
        if (aliveEnemies.Any())
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
    
    private void StartNextPhaseOrRepetition(Level level)
    {
        var currentPhase = GetCurrentPhase(level);
        if (currentPhase.Repetitions > _phaseRepetition + 1)
        {
            _phaseRepetition++;
            return;
        }

        if (level.Phases.Count <= _phase + 1)
        {
            _phasesEnded = true;
            return;
        }

        _phase++;
        _phaseRepetition = 0;
    }
}