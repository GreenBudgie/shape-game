[GlobalClass]
public partial class SpeedStat : ModuleStat
{

    private static readonly Texture2D Icon = GD.Load<Texture2D>("uid://jen4xqupa4bn");

    [Export] public float Speed { get; private set; }
    
    public override float Value => Speed;

    public override StatInfo Info => new(Icon, Speed.FormatStat());

}