public partial class GameOverScreen : Control, IScreen
{

    public bool IsOpen => Visible;
    
    public override void _Ready()
    {
        ScreenManager.Instance.RegisterScreen(this);
        
        GetNode<Button>("%RestartButton").Pressed += Restart;
        
        var respawnButton = GetNode<Button>("%RespawnButton");
        if (Debug.Enabled)
        {
            respawnButton.Show();
            respawnButton.Pressed += Respawn;
        }

        PlayerManager.Instance.Destroyed += Show;
        PlayerManager.Instance.Respawned += Hide;
    }

    private void Respawn()
    {
        Player.Respawn();
    }

    private void Restart()
    {
        ShapeGame.Instance.Restart();
    }
    
}
