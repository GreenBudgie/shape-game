using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugDraw : Node2D
{

    public static DebugDraw Instance { get; private set; } = null!;

    private const int InfoFontSize = 48;
    private const int InfoOutlineSize = 12;

    /// <summary>
    /// Top-left corner of the info column
    /// </summary>
    private static readonly Vector2 InfoPosition = new(16, 16);

    public DebugDraw()
    {
        Instance = this;
    }

    private Dictionary<DebugPoint, float> _points = [];
    private List<KeyValuePair<Vector2, Vector2>> _arrows = [];
    private List<DebugInfo> _info = [];

    private Font _font = null!;

    public override void _Ready()
    {
        _font = ThemeDB.GetProjectTheme()?.DefaultFont ?? ThemeDB.FallbackFont;
    }

    public override void _Draw()
    {
        if (!Debug.Enabled)
        {
            if (_points.Count != 0 || _arrows.Count != 0 || _info.Count != 0)
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
        
        DrawInfo();

        _arrows.Clear();
    }

    /// <summary>
    /// Draws the info texts in a column, one line under another. Texts can have several lines.
    /// </summary>
    private void DrawInfo()
    {
        _info.RemoveAll(info => info.Owner != null && !IsInstanceValid(info.Owner));

        var lineHeight = _font.GetHeight(InfoFontSize);
        // DrawString takes the baseline position, not the top of the text
        var position = InfoPosition + new Vector2(0, _font.GetAscent(InfoFontSize));

        foreach (var info in _info)
        {
            foreach (var line in info.Text().Split('\n'))
            {
                DrawStringOutline(_font, position, line, fontSize: InfoFontSize, size: InfoOutlineSize,
                    modulate: Colors.Black);
                DrawString(_font, position, line, fontSize: InfoFontSize, modulate: Colors.White);
                position.Y += lineHeight;
            }
        }
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

    /// <summary>
    /// Shows the text in the top-left corner, it is requested again every frame.
    /// </summary>
    /// <param name="text">Returns the text to show, can have several lines</param>
    /// <param name="owner">If set, the info is removed when this node is freed, so the text function
    /// does not access a freed node</param>
    public static void AddInfo(Func<string> text, Node? owner = null)
    {
        Instance._info.Add(new DebugInfo(text, owner));
    }

    public record struct DebugPoint(Vector2 Position, Color Color, float Size);

    private record struct DebugInfo(Func<string> Text, Node? Owner);

}