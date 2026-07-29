public class SpeedStat : SpawnableStat
{
    
    private const string IconPath = "uid://jen4xqupa4bn";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "speed";
    
    public override Texture2D Icon => StatIcon;

}