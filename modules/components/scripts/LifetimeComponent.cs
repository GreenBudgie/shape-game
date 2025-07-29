public partial class LifetimeComponent : Node, IModuleComponent
{

    [Export] public Node2D Projectile { get; private set; } = null!;
    [Export] public float BaseLifetimeSeconds { get; private set; }

    public Node Node => this;
    
    public override void _Ready()
    {
        var lifetimeTimer = GetTree().CreateTimer(BaseLifetimeSeconds, processAlways: false);
        lifetimeTimer.Timeout += EndLifetime;
    }

    private void EndLifetime()
    {
        Projectile.QueueFree();
    }

}