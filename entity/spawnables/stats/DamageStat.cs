public class DamageStat : SpawnableStat
{
    
    private const string IconPath = "uid://5nlmhh1tle03";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "damage";

    public override Texture2D Icon => StatIcon;

}