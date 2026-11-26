public partial class PauseManager : Node
{
    
    private static readonly StringName VisualEffectsTime = "visual_effects_time";
    private double _visualEffectsTime;

    [Signal]
    public delegate void GamePauseEventHandler();
    
    [Signal]
    public delegate void GameUnpauseEventHandler();

    public static PauseManager Instance { get; private set; } = null!;

    public PauseManager()
    {
        Instance = this;
    }
    
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        RenderingServer.GlobalShaderParameterSet(VisualEffectsTime, 0f);
    }

    public override void _Process(double delta)
    {
        if (!IsPaused())
        {
            _visualEffectsTime += delta;
            RenderingServer.GlobalShaderParameterSet(VisualEffectsTime, (float)_visualEffectsTime);
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        GetTree().Paused = !IsPaused();
        EmitSignal(IsPaused() ? SignalName.GamePause : SignalName.GameUnpause);

    }

    public bool IsPaused()
    {
        return GetTree().Paused;
    }
    
}