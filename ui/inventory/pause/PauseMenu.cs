using Autoload;

namespace Ui.Pause;

public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        PauseManager.Instance.GamePause += () => Visible = true;
        PauseManager.Instance.GameUnpause += () => Visible = false;
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
