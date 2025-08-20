public partial class FireDelayStatComponent : Node, IProjectileComponent
{
    
    public float FireDelay { get; private set; }

    public Node Node => this;
    
    public void Prepare(ShotContext context)
    {
        FireDelay = context.CalculateStat<FireDelayStat>();
    }

}