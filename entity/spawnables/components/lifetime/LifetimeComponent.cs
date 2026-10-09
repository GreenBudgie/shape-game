public partial class LifetimeComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.Lifetime;

    public override void Apply(SpawnableContext context)
    {
        var spawnable = context.Spawnable;

        var lifetimeTimer = new Timer();
        lifetimeTimer.OneShot = true;
        lifetimeTimer.Autostart = true;
        lifetimeTimer.WaitTime = context.CalculateStat<LifetimeStat>();
        
        lifetimeTimer.Timeout += spawnable.Remove;
        
        spawnable.Node.AddChild(lifetimeTimer);
    }


}