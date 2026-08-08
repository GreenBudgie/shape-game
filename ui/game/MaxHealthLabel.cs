public partial class MaxHealthLabel : Label
{
    public override void _Ready()
    {
        ShapeGame.Instance.PostSetup += UpdateMaxHealth;
    }
    
    private void UpdateMaxHealth()
    {
        var maxHealth = Player.FindPlayer()?.HealthController.MaxHealth;
        if (maxHealth.HasValue)
        {
            UpdateMaxHealth(maxHealth.Value);   
        }
        else
        {
            UpdateMaxHealth(0);
        }
    }
    
    private void UpdateMaxHealth(float maxHealth)
    {
        Text = $"{maxHealth}";
    }
}
