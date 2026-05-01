public partial class LifetimeComponent : Node, ISpawnableComponent
{

    public void Prepare(SpawnableContext context)
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