global using Godot;
global using static Godot.Mathf;

public partial class ShapeGame : Node2D
{
    
    public static readonly Rect2I PlayableArea = new(512, 0, 3264, 2160);
    
    public static ShapeGame Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        // Input.MouseMode = Input.MouseModeEnum.Hidden;
        // Engine.MaxFps = 30; 
    }

}