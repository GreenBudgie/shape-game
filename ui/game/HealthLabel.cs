public partial class HealthLabel : Label
{
    public override void _Ready()
    {
        ShapeGame.Instance.PostSetup += UpdateHealth;
        PlayerManager.Instance.HealthChanged += UpdateHealth;
        PlayerManager.Instance.Respawned += UpdateHealth;
        PlayerManager.Instance.Destroyed += UpdateHealth;
    }
    
    private void UpdateHealth()
    {
        var health = Player.FindPlayer()?.HealthController.Health;
        if (health.HasValue)
        {
            UpdateHealth(health.Value);   
        }
        else
        {
            UpdateHealth(0);
        }
    }
    
    private void UpdateHealth(float health)
    {
        Text = $"{RoundToInt(health)}";
    }
}
