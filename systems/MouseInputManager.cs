public partial class MouseInputManager : Node2D
{

    public static MouseInputManager Instance { get; private set; } = null!;

    private Vector2 _windowCenter;
    private Vector2 _currentFrameMouseDelta;
    public bool IsCharacterControlEnabled { get; private set; }
    public bool IsAttackEnabled { get; private set; }

    private bool _isAttackQueuedToEnable;

    private Vector2 _cachedGlobalMousePosition;
    
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
        _cachedGlobalMousePosition = GetGlobalMousePosition();
        
        if (_isAttackQueuedToEnable && (Input.IsActionJustPressed("inventory_left_click") 
                                        || Input.IsActionJustReleased("inventory_left_click")))
        {
            IsAttackEnabled = true;
        }
        
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

    /**
     * Getting mouse position is heavy for some reason. If many nodes call it in a single frame, the game lags.
     * So, this calculates the mouse position once per frame and caches it. It's better to always call this method
     * rather than getting the mouse position in other nodes again.
     */
    public Vector2 GetCachedGlobalMousePosition()
    {
        return _cachedGlobalMousePosition;
    }

    public void DisableAttack()
    {
        IsAttackEnabled = false;
    }

    public void EnableAttack()
    {
        if (IsAttackEnabled)
        {
            return;
        }
        
        _isAttackQueuedToEnable = true;
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
