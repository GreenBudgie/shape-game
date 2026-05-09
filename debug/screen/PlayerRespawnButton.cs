public partial class PlayerRespawnButton : Button
{
    public override void _Pressed()
    {
        Player.Respawn();
    }

    public override void _Process(double delta)
    {
        Visible = Player.FindPlayer() == null;
    }
}
