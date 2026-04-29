[GlobalClass, Icon(IconPath)]
public partial class LifetimeStat : ModuleStat
{

    private const string IconPath = "uid://cdwaf3usia8nn";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    [Export(PropertyHint.None, "suffix:s")] 
    public float Lifetime { get; private set; }
    
    [Export(PropertyHint.None, "suffix:±s")] 
    public float LifetimeDelta { get; private set; }
    
    public override string Name => "lifetime";

    public override float Value => Lifetime;
    
    public override float ValueDelta => LifetimeDelta;
    
    public override Texture2D Icon => StatIcon;

    public override string FormattedValue => Value.FormatStat() + " sec";

    private static readonly StringName LifetimeTimerName = "LifetimeTimer";
    
    public override void Apply(ShotContext context)
    {
        var projectile = context.Projectile.Node;

        var lifetimeTimer = new Timer();
        lifetimeTimer.Name = LifetimeTimerName;
        lifetimeTimer.OneShot = true;
        lifetimeTimer.Autostart = true;
        lifetimeTimer.WaitTime = context.CalculateStat<LifetimeStat>();
        lifetimeTimer.Timeout += context.Projectile.Remove;
        projectile.AddChild(lifetimeTimer);
    }
    
}