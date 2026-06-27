using System.Collections.Generic;

public partial class DebugDraw : Node2D
{

    public static DebugDraw Instance { get; private set; } = null!;

    public DebugDraw()
    {
        Instance = this;
    }

    private List<DebugPoint> _points = [];
    private List<KeyValuePair<Vector2, Vector2>> _arrows = [];
    private List<string> _strings = [];

    public override void _Draw()
    {
        foreach (var point in _points)
        {
            DrawCircle(ToLocal(point.Position), point.Size, point.Color);
        }
        
        foreach (var arrow in _arrows)
        {
            DrawLine(ToLocal(arrow.Key), ToLocal(arrow.Value), Colors.Red, 8);
            DrawCircle(ToLocal(arrow.Value), 4, Colors.Green);
        }

        _points.Clear();
        _arrows.Clear();
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public static void DrawPoint(Vector2 globalPosition)
    {
        DrawPoint(globalPosition, Colors.Red);
    }
    
    public static void DrawPoint(Vector2 globalPosition, float size)
    {
        DrawPoint(globalPosition, Colors.Red, size);
    }
    
    public static void DrawPoint(Vector2 globalPosition, Color color, float size = 8)
    {
        Instance._points.Add(new DebugPoint(globalPosition, color, size));
    }
    
    public static void DrawArrow(Vector2 start, Vector2 end)
    {
        Instance._arrows.Add(new KeyValuePair<Vector2, Vector2>(start, end));
    }

    public record struct DebugPoint(Vector2 Position, Color Color, float Size);

}