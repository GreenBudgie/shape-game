[GlobalClass, Icon(IconPath)]
public partial class FireDelayStat : ModuleStat
{

    private const string IconPath = "uid://oxosftrf543w";

    private static readonly Texture2D Icon = GD.Load<Texture2D>(IconPath);

    [Export] public float FireDelay { get; private set; }

    public override float Value => FireDelay;

    public override StatInfo Info => new(Icon, FireDelay.FormatStat() + " sec");

}