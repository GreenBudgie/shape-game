[GlobalClass, Icon(IconPath)]
public partial class ReloadStat : ModuleStat
{

    private const string IconPath = "uid://oxosftrf543w";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    [Export] public float FireDelay { get; private set; }
    
    public override string Name => "reload";

    public override float Value => FireDelay;
    
    public override Texture2D Icon => StatIcon;

    public override string FormattedValue => Value.FormatStat() + " sec";

}