public partial class MouseInputManager : Node2D
{

    public static MouseInputManager Instance { get; private set; } = null!;

    private Vector2 _windowCenter;
    private Vector2 _currentFrameMouseDelta;
    public bool IsCharacterControlEnabled { get; private set; }
    
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

        PauseManager.Instance.GameUnpause += EnableCharacterControl;
        PauseManager.Instance.GamePause += ShowCursor;
        InventoryManager.Instance.InventoryClosed += EnableCharacterControl;
        InventoryManager.Instance.InventoryOpened += ShowCursor;
        DebugScreen.Instance.ScreenClosed += EnableCharacterControl;
        DebugScreen.Instance.ScreenOpened += ShowCursor;
    }

    public override void _Process(double delta)
    {
        if (IsCharacterControlEnabled)
        {
            _currentFrameMouseDelta = MoveMouseToWindowCenter(_windowCenter);
        }
        else
        {
            _currentFrameMouseDelta = Vector2.Zero;
        }
    }

    public Vector2 GetMouseDelta()
    {
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
        MoveMouseToWindowCenter(ShapeGame.PlayableArea.GetCenter());
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
