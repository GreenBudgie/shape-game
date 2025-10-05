using System.Collections.Generic;
using System.Linq;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void LevelStartedEventHandler(Level level);

    [Signal]
    public delegate void DestroyProgressUpdatedEventHandler(int prevProgress, int newProgress);

    [Signal]
    public delegate void SurviveProgressUpdatedEventHandler(int prevProgress, int newProgress);

    public static readonly List<Level> Levels = ResourceSearcher.FindResourcesInDirectory<Level>("res://level/config");

    private static readonly Dictionary<int, Level> LevelByNumber = Levels
        .GroupBy(level => level.Number)
        .ToDictionary(
            group => group.Key,
            group => group.Single()
        );

    public Level? Level;

    public int DestroyProgress { get; private set; }

    public int SurviveProgress { get; private set; }

    private double _surviveProgressRealSeconds;
    private double _timeToNextPhase;
    private bool _requirementsMet;

    public LevelManager()
    {
        Instance = this;
    }

    public static Level GetLevelByNumber(int number)
    {
        return LevelByNumber[number];
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

    private void ProcessLevel(double delta, Level level)
    {
        if (_requirementsMet)
        {
            return;
        }
        
        _surviveProgressRealSeconds += delta;
        var fullSecondsProgress = FloorToInt(_surviveProgressRealSeconds);
        if (fullSecondsProgress > SurviveProgress && SurviveProgress < level.SurviveRequirement)
        {
            SetSurviveProgress(Min(level.SurviveRequirement, fullSecondsProgress));
        }

        _timeToNextPhase -= delta;
        if (_timeToNextPhase < 0)
        {
            _timeToNextPhase = level.PhaseDuration;
            SpawnEnemies();
        }
    }

    private int _phase = 1;

    public void StartLevel(int level)
    {
        Level = GetLevelByNumber(level);

        SetSurviveProgress(0);
        SetDestroyProgress(0);

        _timeToNextPhase = Level.PhaseDuration;
        SpawnEnemies();

        EmitSignalLevelStarted(Level);
    }

    public void CheckIfRequirementsMet()
    {
        if (Level == null)
        {
            return;
        }

        if (DestroyProgress < Level.DestroyRequirement || SurviveProgress < Level.SurviveRequirement)
        {
            return;
        }

        _requirementsMet = true;
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
    }

    private void SetDestroyProgress(int progress)
    {
        var prevDestroyProgress = DestroyProgress;
        DestroyProgress = progress;
        EmitSignalDestroyProgressUpdated(prevDestroyProgress, DestroyProgress);
        CheckIfRequirementsMet();
    }

    private void SetSurviveProgress(int progressSeconds)
    {
        var prevSurviveProgress = SurviveProgress;
        SurviveProgress = progressSeconds;
        EmitSignalSurviveProgressUpdated(prevSurviveProgress, SurviveProgress);
        CheckIfRequirementsMet();
    }

    private void SpawnEnemies()
    {
        if (Level == null)
        {
            return;
        }

        for (var i = 0; i < Level.EnemiesPerPhase; i++)
        {
            EnemyManager.Instance.SpawnEnemy(Level.GetRandomWeightedEnemyType(_phase));
        }

        _phase++;
    }
}