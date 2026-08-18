using System.Collections.Generic;
using System.Linq;

public partial class ScreenManager : Node
{
    public static ScreenManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void AllScreensClosedEventHandler();

    /// <summary>
    /// Emitted whenever any screen opens but previously all screens were closed 
    /// </summary>
    [Signal]
    public delegate void AnyScreenOpenedEventHandler();
    
    private readonly HashSet<IScreen> _screens = [];
    private HashSet<IScreen> _openedScreens = [];

    public ScreenManager()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        var previouslyOpenedScreens = _openedScreens.ToHashSet();
        _openedScreens = _screens.Where(screen => screen.IsOpen).ToHashSet();

        if (_openedScreens.Count == 0 && previouslyOpenedScreens.Count != 0)
        {
            EmitSignalAllScreensClosed();
            return;
        }
        
        if (_openedScreens.Count != 0 && previouslyOpenedScreens.Count == 0)
        {
            EmitSignalAnyScreenOpened();
        }
    }

    public bool IsAnyScreenOpen()
    {
        return _openedScreens.Count != 0;
    }

    public void RegisterScreen(IScreen screen)
    {
        _screens.Add(screen);
        if (screen.IsOpen)
        {
            _openedScreens.Add(screen);
        }
    }
}