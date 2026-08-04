public partial class PauseMenu : Control, IScreen
{

    public bool IsOpen => PauseManager.Instance.IsPaused();

    public override void _Ready()
    {
        PauseManager.Instance.GamePause += Show;
        PauseManager.Instance.GameUnpause += Hide;
        ScreenManager.Instance.RegisterScreen(this);
    }

    public override void _ExitTree()
    {
        PauseManager.Instance.GamePause -= Show;
        PauseManager.Instance.GameUnpause -= Hide;
    }

    public void OnResume()
    {
        PauseManager.Instance.TogglePause();
    }
    
    public void OnQuit()
    {
        GetTree().Quit();
    }
    
}
