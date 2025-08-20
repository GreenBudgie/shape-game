[GlobalClass, Icon(IconPath)]
public partial class SpeedStat : ModuleStat
{
    
    private const string IconPath = "uid://jen4xqupa4bn";

    private static readonly Texture2D Icon = GD.Load<Texture2D>(IconPath);

    [Export] public float Speed { get; private set; }
    
    public override float Value => Speed;

    public override StatInfo Info => new(Icon, Speed.FormatStat());

}