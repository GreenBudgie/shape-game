public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        PauseManager.Instance.GamePause += Show;
        PauseManager.Instance.GameUnpause += Hide;
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
