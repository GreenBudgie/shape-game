[GlobalClass, Icon(IconPath)]
public partial class LifetimeStat : SpawnableStat
{

    private const string IconPath = "uid://cdwaf3usia8nn";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    [Export(PropertyHint.None, "suffix:s")] 
    public float Lifetime { get; set; }
    
    [Export(PropertyHint.None, "suffix:±s")] 
    public float LifetimeDelta { get; set; }
    
    public override string Name => "lifetime";

    public override float Value => Lifetime;
    
    public override float ValueDelta => LifetimeDelta;
    
    public override Texture2D Icon => StatIcon;

    public override string FormattedValue => Value.FormatStat() + " sec";
    
}