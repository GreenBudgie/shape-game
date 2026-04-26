public partial class PlayerFullHealButton : Button
{
    public override void _Pressed()
    {
        Player.FindPlayer()?.HealthController.FullHeal();
    }
}
