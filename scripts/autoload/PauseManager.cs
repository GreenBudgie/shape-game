﻿namespace Autoload;

public partial class PauseManager : Node
{
    
    [Signal]
    public delegate void GamePauseEventHandler();
    
    [Signal]
    public delegate void GameUnpauseEventHandler();
    
    public static PauseManager Instance { get; private set; }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Instance = this;
    }

    public override void _Process(double delta)
    {
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