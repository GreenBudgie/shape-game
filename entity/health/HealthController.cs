public partial class HealthController : Node2D
{
    
    [Signal] public delegate void DamagedEventHandler(float damage);
    [Signal] public delegate void DestroyedEventHandler();
    
    [Export] public float MaxHealth { get; set; }
    
    public float Health { get; private set; }

    public static HealthController? GetHealthControllerIfExists(Node2D owner)
    {
        return owner.GetNodeOrNull<HealthController>("HealthController");
    }
    
    public static HealthController GetHealthController(Node2D owner)
    {
        return owner.GetNode<HealthController>("HealthController");
    }

    public override void _Ready()
    {
        Health = MaxHealth;
    }

    public bool IsDestroyed()
    {
        return Health <= 0f;
    }

    public float GetHealthRatio()
    {
        return Clamp(Health / MaxHealth, 0f, 1f);
    }

    public void Damage(float damage, Vector2? labelPosition = null)
    {
        if (IsDestroyed())
        {
            return;
        }

        if (labelPosition.HasValue)
        {
            DamageLabel.Create(damage, labelPosition.Value);
        }

        Health = Max(Health - damage, 0);
        EmitSignalDamaged(damage);
        
        if (IsDestroyed())
        {
            EmitSignalDestroyed();
        }
    }

    public void Destroy()
    {
        if (IsDestroyed())
        {
            return;
        }

        Health = 0;
        EmitSignalDestroyed();
    }
    
}
