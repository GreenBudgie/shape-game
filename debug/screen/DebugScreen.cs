public partial class DebugScreen : CanvasLayer, IScreen
{
    [Signal]
    public delegate void ScreenOpenedEventHandler();

    [Signal]
    public delegate void ScreenClosedEventHandler();

    public static DebugScreen Instance { get; private set; } = null!;

    public bool IsOpen => Visible;

    private MarginContainer _mainContainer = null!;
    private Control _moduleSelectorContainer = null!;

    public DebugScreen()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _mainContainer = GetNode<MarginContainer>("Control/MainContainer");
        _moduleSelectorContainer = GetNode<Control>("Control/ModuleSelector");
        
        InventoryManager.Instance.Connect(InventoryManager.SignalName.InventoryClosed, Callable.From(OnInventoryClose));
        InventoryManager.Instance.Connect(InventoryManager.SignalName.InventoryOpened, Callable.From(OnInventoryOpen));
        
        ScreenManager.Instance.RegisterScreen(this);
    }

    private void OnInventoryClose()
    {
        _mainContainer.Show();
        _moduleSelectorContainer.Hide();
    }

    private void OnInventoryOpen()
    {
        _mainContainer.Hide();
        _moduleSelectorContainer.Show();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("debug_timescale"))
        {
            if (IsEqualApprox(Engine.TimeScale, 0.2f))
            {
                Engine.TimeScale = 1f;
            }
            else
            {
                Engine.TimeScale = 0.2f;
            }
        }
        
        if (IsOpen && (Input.IsActionJustPressed("open_debug_screen") || Input.IsActionJustPressed("ui_cancel")))
        {
            Close();
            return;
        }

        if (!IsOpen && Input.IsActionJustPressed("open_debug_screen"))
        {
            Open();
        }
    }

    private void Close()
    {
        Visible = false;
        EmitSignalScreenClosed();
    }

    private void Open()
    {
        Visible = true;
        EmitSignalScreenOpened();
    }
}