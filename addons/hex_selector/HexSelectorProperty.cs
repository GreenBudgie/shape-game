#if TOOLS
using System.Collections.Generic;
using System.Linq;

public enum HexSelectorMode { Array, Resource }

[Tool]
public partial class HexSelectorProperty : EditorProperty
{
    private const string VectorsProperty = "_vectors";

    private readonly HexGrid _grid;
    private readonly HexSelectorMode _mode;

    public HexSelectorProperty(int radius, HexSelectorMode mode)
    {
        _mode = mode;
        _grid = new HexGrid(radius);
        _grid.SelectionChanged += OnSelectionChanged;
        AddChild(_grid);
        SetBottomEditor(_grid);
    }

    public override void _UpdateProperty()
    {
        var value = GetEditedObject().Get(GetEditedProperty());
        Godot.Collections.Array<Vector3I> items;

        if (_mode == HexSelectorMode.Resource)
        {
            var resource = value.AsGodotObject();
            items = resource?.Get(VectorsProperty).AsGodotArray<Vector3I>()
                    ?? new Godot.Collections.Array<Vector3I>();
        }
        else
        {
            items = value.AsGodotArray<Vector3I>();
        }

        _grid.SetSelection(items);
    }

    private void OnSelectionChanged()
    {
        var items = new Godot.Collections.Array<Vector3I>();
        foreach (var coord in _grid.Selected)
        {
            items.Add(coord);
        }

        if (_mode == HexSelectorMode.Resource)
        {
            var resource = GetEditedObject().Get(GetEditedProperty()).AsGodotObject();
            if (resource == null)
            {
                var fresh = new HexCoordinatesArray { Vectors = items };
                EmitChanged(GetEditedProperty(), fresh);
            }
            else
            {
                resource.Set(VectorsProperty, items);
                EmitChanged(GetEditedProperty(), resource);
            }
        }
        else
        {
            EmitChanged(GetEditedProperty(), items);
        }
    }
}

[Tool]
public partial class HexGrid : Control
{
    [Signal]
    public delegate void SelectionChangedEventHandler();

    private const float MinHexRadius = 18f;
    private const float Margin = 4f;
    private const float OutlineWidth = 1.5f;

    private static readonly Color UnselectedFill = new(0.18f, 0.18f, 0.22f);
    private static readonly Color SelectedFill = new(0.30f, 0.55f, 0.85f);
    private static readonly Color HoverFill = new(0.40f, 0.40f, 0.46f);
    private static readonly Color SelectedHoverFill = new(0.45f, 0.65f, 0.95f);
    private static readonly Color OriginMarker = new(1f, 1f, 1f, 0.6f);
    private static readonly Color Outline = new(0f, 0f, 0f, 0.7f);

    private readonly int _radius;
    private readonly Vector3I[] _allHexes;
    private readonly HashSet<Vector3I> _selected = [];
    private Vector3I? _hovered;

    public IEnumerable<Vector3I> Selected => _selected;

    public HexGrid(int radius)
    {
        _radius = radius;
        _allHexes = HexCoordinates.Spiral(radius + 1)
            .Select(c => new Vector3I(c.Q, c.R, c.S))
            .ToArray();

        CustomMinimumSize = new Vector2(
            (2 * radius + 1) * Sqrt(3) * MinHexRadius + Margin * 2,
            (3 * radius + 2) * MinHexRadius + Margin * 2);
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void SetSelection(Godot.Collections.Array<Vector3I> coords)
    {
        _selected.Clear();
        foreach (var coord in coords)
        {
            _selected.Add(coord);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        var radius = ComputeHexRadius();
        if (radius <= 0) return;

        foreach (var coord in _allHexes)
        {
            var center = HexCenter(coord, radius);
            var corners = HexCorners(center, radius);
            var isSelected = _selected.Contains(coord);
            var isHovered = _hovered == coord;

            var fill = (isSelected, isHovered) switch
            {
                (true, true) => SelectedHoverFill,
                (true, false) => SelectedFill,
                (false, true) => HoverFill,
                _ => UnselectedFill,
            };

            DrawColoredPolygon(corners, fill);

            var outline = new Vector2[7];
            corners.CopyTo(outline, 0);
            outline[6] = corners[0];
            DrawPolyline(outline, Outline, OutlineWidth, true);

            if (coord == Vector3I.Zero)
            {
                DrawCircle(center, radius * 0.12f, OriginMarker);
            }
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion motion:
                var newHover = HexAt(motion.Position);
                if (newHover != _hovered)
                {
                    _hovered = newHover;
                    QueueRedraw();
                }
                break;

            case InputEventMouseButton click
                when click.ButtonIndex == MouseButton.Left && click.Pressed:
                if (HexAt(click.Position) is { } hit)
                {
                    if (!_selected.Add(hit)) _selected.Remove(hit);
                    EmitSignalSelectionChanged();
                    QueueRedraw();
                    AcceptEvent();
                }
                break;
        }
    }

    private float ComputeHexRadius()
    {
        var w = Size.X - Margin * 2;
        var h = Size.Y - Margin * 2;
        return Min(w / ((2 * _radius + 1) * Sqrt(3)), h / (3 * _radius + 2));
    }

    private Vector2 HexCenter(Vector3I coord, float radius)
    {
        var spacing = Sqrt(3) * radius;
        var math = new HexCoordinates(coord.X, coord.Y, coord.Z).ToVector();
        return Size / 2f + new Vector2(math.X, -math.Y) * spacing;
    }

    private static Vector2[] HexCorners(Vector2 center, float radius)
    {
        var corners = new Vector2[6];
        for (var i = 0; i < 6; i++)
        {
            var angle = -Pi / 2f + i * Pi / 3f;
            corners[i] = center + new Vector2(Cos(angle), Sin(angle)) * radius;
        }
        return corners;
    }

    private Vector3I? HexAt(Vector2 position)
    {
        var radius = ComputeHexRadius();
        if (radius <= 0) return null;

        Vector3I? closest = null;
        var closestDist = float.MaxValue;
        foreach (var coord in _allHexes)
        {
            var dist = HexCenter(coord, radius).DistanceTo(position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = coord;
            }
        }
        return closestDist < radius ? closest : null;
    }
}
#endif
