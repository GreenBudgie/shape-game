public partial class PlayerInvulnerableButton : Button
{
    public override void _Pressed()
    {
        var healthController = Player.FindPlayer()?.HealthController;
        if (healthController == null)
        {
            return;
        }
        
        healthController.IsInvulnerable = !healthController.IsInvulnerable;
        if (healthController.IsInvulnerable)
        {
            Text = "Invulnerable: ON";
        }
        else
        {
            Text = "Invulnerable: OFF";
        }
    }
}
