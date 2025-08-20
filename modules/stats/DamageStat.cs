[GlobalClass, Icon(IconPath)]
public partial class DamageStat : ModuleStat
{
    
    private const string IconPath = "uid://5nlmhh1tle03";

    private static readonly Texture2D Icon = GD.Load<Texture2D>(IconPath);
    
    [Export] public float Damage { get; private set; }

    public override float Value => Damage;

    public override StatInfo Info => new(Icon, Damage.FormatStat());

}