public partial class DebugScreen : CanvasLayer
{
    [Signal]
    public delegate void ScreenOpenedEventHandler();

    [Signal]
    public delegate void ScreenClosedEventHandler();

    public static DebugScreen Instance { get; private set; } = null!;

    public bool IsOpen => Visible;

    public DebugScreen()
    {
        Instance = this;
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