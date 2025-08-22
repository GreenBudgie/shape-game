[GlobalClass, Icon(IconPath)]
public partial class ExplosionRadiusStat : ModuleStat
{
    
    private const string IconPath = "uid://bahh717wv8wh8";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);
    
    [Export] public float ExplosionRadius { get; private set; }

    public override string Name => "expl. radius";

    public override float Value => ExplosionRadius;

    public override Texture2D Icon => StatIcon;

}