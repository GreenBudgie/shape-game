[GlobalClass, Icon(IconPath)]
public partial class ExplosionDamageStat : ModuleStat
{
    
    private const string IconPath = "uid://b3anlp3jqcefc";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);
    
    [Export] public float ExplosionDamage { get; private set; }

    public override string Name => "expl. damage";

    public override float Value => ExplosionDamage;

    public override Texture2D Icon => StatIcon;

}