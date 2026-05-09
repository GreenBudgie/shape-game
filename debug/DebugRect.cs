/// <summary>
/// Used to draw a temporary red rectangle to easily determine on-screen positions
/// </summary>
public partial class DebugRect : ColorRect
{

    private Rect2 _rect;

    public static DebugRect Create(Rect2 globalRect)
    {
        var node = new DebugRect();
        node._rect = globalRect;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public override void _Ready()
    {
        Color = Colors.Red;
        ZIndex = 100;
        GlobalPosition = _rect.Position;
        Size = _rect.Size;

        GetTree().CreateTimer(3).Timeout += QueueFree;
    }
}