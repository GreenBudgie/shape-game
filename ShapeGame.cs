global using Godot;
global using static Godot.Mathf;

public partial class ShapeGame : Node2D
{
    
    public static readonly Rect2I PlayableArea = new(256, 0, 1664, 1080);
    
    public static ShapeGame Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Engine.TimeScale = 0.1;
        // Input.MouseMode = Input.MouseModeEnum.Hidden;
        // Engine.MaxFps = 30; 
    }

}