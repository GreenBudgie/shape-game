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
    
    public static ShapeGame Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        // Engine.MaxFps = 60; 

        var beam = Beam.Create()
            .SetFromTo(new Vector2(800, WindowSize.Y / 2), new Vector2(3000, WindowSize.Y / 2))
            .SetThickness(30)
            .SetOutlineThickness(150);
        AddChild(beam);
        
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