[GlobalClass]
public partial class FireDelayStat : ModuleStat
{
    
    private static readonly Texture2D Icon = GD.Load<Texture2D>("uid://oxosftrf543w");
    
    [Export] public float FireDelay { get; private set; }
    
    public override StatInfo Info => new(Icon, FireDelay.FormatStat() + " sec");
    
}