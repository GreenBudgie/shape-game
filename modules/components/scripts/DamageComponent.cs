public partial class DamageComponent : Node, IProjectileComponent
{

    [Export] public float BaseDamage { get; private set; }

    public Node Node => this;

}