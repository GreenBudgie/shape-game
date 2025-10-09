global using Godot;
global using static Godot.Mathf;
global using static GodotConstants;

public partial class ShapeGame : Node2D
{
    
    [Signal]
    public delegate void PostSetupEventHandler();
    
    public static readonly Vector2 WindowSize = new(3840, 2160);
    public static readonly Rect2 PlayableArea = new(512, 0, 3264, 2160);
    public static readonly Vector2 Center = PlayableArea.GetCenter();
    public const float DistanceToOutsideBorder = 1024;
    
    public static ShapeGame Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        // Engine.MaxFps = 60; 

        EmitSignalPostSetup();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("restart"))
        {
            GetTree().ReloadCurrentScene();
        }
        ProcessFullscreenToggle();
    }
    
    private void ProcessFullscreenToggle()
    {
        if (!Input.IsActionJustPressed("fullscreen"))
        {
            return;
        }

        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.ExclusiveFullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }

}