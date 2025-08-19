[GlobalClass]
public partial class DamageStat : ModuleStat
{

    private static readonly Texture2D Icon = GD.Load<Texture2D>("uid://5nlmhh1tle03");
    
    [Export] public float Damage { get; private set; }

    public override StatInfo Info => new(Icon, Damage.FormatStat());

}