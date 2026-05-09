public partial class MouseInputManager : Node2D
{

    public static MouseInputManager Instance { get; private set; } = null!;

    private Vector2 _windowCenter;
    private Vector2 _currentFrameMouseDelta;
    public bool IsCharacterControlEnabled { get; private set; }
    public bool IsAttackEnabled { get; set; }
    
    public MouseInputManager()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        EnableCharacterControl();
        
        _windowCenter = new Vector2(
            GetViewportRect().Size.X / 2.0f,
            GetViewportRect().Size.Y / 2.0f
        );

        PauseManager.Instance.Connect(PauseManager.SignalName.GameUnpause, Callable.From(EnableCharacterControl));
        PauseManager.Instance.Connect(PauseManager.SignalName.GamePause, Callable.From(ShowCursor));
        InventoryManager.Instance.Connect(InventoryManager.SignalName.InventoryClosed, Callable.From(EnableCharacterControl));
        InventoryManager.Instance.Connect(InventoryManager.SignalName.InventoryOpened, Callable.From(ShowCursor));
        DebugScreen.Instance.Connect(DebugScreen.SignalName.ScreenClosed, Callable.From(EnableCharacterControl));
        DebugScreen.Instance.Connect(DebugScreen.SignalName.ScreenOpened, Callable.From(ShowCursor));
    }

    public override void _Process(double delta)
    {
        if (IsCharacterControlEnabled && DisplayServer.WindowIsFocused())
        {
            Input.MouseMode = Input.MouseModeEnum.Hidden;
            _currentFrameMouseDelta = MoveMouseToWindowCenter(_windowCenter);
        }
        else
        {
            _currentFrameMouseDelta = Vector2.Zero;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public Vector2 GetMouseDelta()
    {
        if (!DisplayServer.WindowIsFocused())
        {
            return Vector2.Zero;
        }
        
        return _currentFrameMouseDelta;
    }

    public void ShowCursor()
    {
        MoveMouseToWindowCenter(_windowCenter);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        IsCharacterControlEnabled = false;
    }
    
    public void EnableCharacterControl()
    {
        MoveMouseToWindowCenter(_windowCenter);
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        IsCharacterControlEnabled = true;
    }
    
    private Vector2 MoveMouseToWindowCenter(Vector2 windowCenterToUse)
    {
        var mousePosition = GetViewport().GetMousePosition();
        GetViewport().WarpMouse(windowCenterToUse);
        return mousePosition - windowCenterToUse;
    }
    
}
