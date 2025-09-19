[GlobalClass, Icon(IconPath)]
public partial class LifetimeStat : ModuleStat
{

    private const string IconPath = "uid://cdwaf3usia8nn";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    [Export(PropertyHint.None, "suffix:s")] 
    public float Lifetime { get; private set; }
    
    public override string Name => "lifetime";

    public override float Value => Lifetime;
    
    public override Texture2D Icon => StatIcon;

    public override string FormattedValue => Value.FormatStat() + " sec";

}