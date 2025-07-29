public partial class FireRateComponent : Node, IModuleComponent
{

    private float _baseFireRate;

    [Export(PropertyHint.Range, "0,2,0.05,or_greater")]
    public float BaseFireRate
    {
        get => _baseFireRate;
        private set
        {
            _baseFireRate = value;
            FireRate = value;
        }
    }

    public float FireRate { get; set; }

    public Node Node => this;

}