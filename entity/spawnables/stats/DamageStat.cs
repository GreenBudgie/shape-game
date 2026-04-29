[GlobalClass, Icon(IconPath)]
public partial class DamageStat : ModuleStat
{
    
    private const string IconPath = "uid://5nlmhh1tle03";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);
    
    [Export] public float Damage { get; private set; }

    public override string Name => "damage";

    public override float Value => Damage;

    public override Texture2D Icon => StatIcon;

}