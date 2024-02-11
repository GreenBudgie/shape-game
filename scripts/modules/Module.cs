namespace Modules;

public partial class Module : Control
{

    private Vector2 _initialCursorPosition;
    private bool _isFollowingCursor = false;

    public override void _Process(double delta)
    {
        if (_isFollowingCursor)
        {
            Position = GetViewport().GetMousePosition() - _initialCursorPosition;
        }
    }

    public void StartFollowingCursor()
    {
        var mousePosition = GetViewport().GetMousePosition();
        _initialCursorPosition = new Vector2(mousePosition.X, mousePosition.Y);
        _isFollowingCursor = true;
    }

    public void StopFollowingCursor()
    {
        Position = Vector2.Zero;
        _isFollowingCursor = false;
    }
}