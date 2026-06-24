using System.Collections.Generic;

public partial class DebugDraw : Node2D
{

    public static DebugDraw Instance { get; private set; } = null!;

    public DebugDraw()
    {
        Instance = this;
    }

    private List<Vector2> _points = [];

    public override void _Draw()
    {
        foreach (var point in _points)
        {
            DrawCircle(ToLocal(point), 8, Colors.Red);
        }

        _points.Clear();
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public static void DrawPoint(Vector2 globalPosition)
    {
        Instance._points.Add(globalPosition);
    }
    
}