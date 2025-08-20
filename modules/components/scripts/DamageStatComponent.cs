public partial class DamageStatComponent : Node, IProjectileComponent
{

    public float Damage { get; private set; }

    public Node Node => this;

    public void Prepare(ShotContext context)
    {
        Damage = context.CalculateStat<DamageStat>();
    }

}