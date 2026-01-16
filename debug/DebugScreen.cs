using Godot;
using System;

public partial class DebugScreen : CanvasLayer
{
    
    [Signal]
    public delegate void ScreenOpenedEventHandler();

    [Signal]
    public delegate void ScreenClosedEventHandler();

    public static DebugScreen Instance { get; private set; } = null!;
    
    public bool IsOpen => Visible;
    
    public DebugScreen()
    {
        Instance = this;
    }
    
    public override void _Process(double delta)
    {
        if (!Input.IsActionJustPressed("open_debug_screen"))
        {
            return;
        }
        
        Visible = !Visible;
            
        if (Visible)
        {
            EmitSignalScreenOpened();
        }
        else
        {
            EmitSignalScreenClosed();
        }
    }
}
