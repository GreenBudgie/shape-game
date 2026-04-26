public partial class PlayerDestroyButton : Button
{
    public override void _Pressed()
    {
        Player.FindPlayer()?.HealthController.Destroy();
    }
}
