public partial class UiModule : Control
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private Vector2 _initialCursorPosition;
    private bool _isFollowingCursor;

    public Module Module { get; private set; } = null!;
    
    public static UiModule Create(Module module)
    {
        var uiModule = Scene.Instantiate<UiModule>();
        uiModule.Module = module;
        return uiModule;
    }

    public override void _Ready()
    {
        var textureRect = GetNode<TextureRect>("Texture");
        textureRect.Texture = Module.Texture;
    }

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