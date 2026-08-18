using System.Collections.Generic;
using System.Linq;

public partial class DebugDraw : Node2D
{

    public static DebugDraw Instance { get; private set; } = null!;

    public DebugDraw()
    {
        Instance = this;
    }

    private Dictionary<DebugPoint, float> _points = [];
    private List<KeyValuePair<Vector2, Vector2>> _arrows = [];
    private List<string> _strings = [];

    public override void _Draw()
    {
        if (!Debug.Enabled)
        {
            if (_points.Count != 0 || _arrows.Count != 0)
            {
                Debug.PrintDebugNotEnabledError();
            }
            
            return;
        }
        
        foreach (var pointEntry in _points)
        {
            var point = pointEntry.Key;
            DrawCircle(ToLocal(point.Position), point.Size, point.Color);
        }
        
        foreach (var arrow in _arrows)
        {
            DrawLine(ToLocal(arrow.Key), ToLocal(arrow.Value), Colors.Red, 8);
            DrawCircle(ToLocal(arrow.Value), 4, Colors.Green);
        }

        var pointsToRemove = _points.Where(pointEntry => pointEntry.Value <= 0).ToList();
        foreach (var pointEntry in pointsToRemove)
        {
            _points.Remove(pointEntry.Key);
        }
        
        _arrows.Clear();
    }

    public override void _Process(double delta)
    {
        var pointsCopy = _points.ToDictionary();
        foreach (var pointEntry in pointsCopy)
        {
            _points[pointEntry.Key] = pointEntry.Value - (float)delta;
        }

        QueueRedraw();
    }

    public static void DrawPoint(Vector2 globalPosition)
    {
        DrawPoint(globalPosition, Colors.Red);
    }
    
    public static void DrawPointForTime(Vector2 globalPosition, float time = 10f)
    {
        DrawPoint(globalPosition, Colors.Red, time: time);
    }
    
    public static void DrawPoint(Vector2 globalPosition, float size)
    {
        DrawPoint(globalPosition, Colors.Red, size);
    }
    
    public static void DrawPoint(Vector2 globalPosition, Color color, float size = 8f, float time = 0f)
    {
        Instance._points.Add(new DebugPoint(globalPosition, color, size), time);
    }
    
    public static void DrawArrow(Vector2 start, Vector2 end)
    {
        Instance._arrows.Add(new KeyValuePair<Vector2, Vector2>(start, end));
    }

    public record struct DebugPoint(Vector2 Position, Color Color, float Size);

}