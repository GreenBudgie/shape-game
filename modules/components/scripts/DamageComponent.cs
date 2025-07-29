public partial class DamageComponent : Node, IModuleComponent
{

    [Export] public float BaseDamage { get; private set; }

    public Node Node => this;

}