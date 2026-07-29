public class ExplosionRadiusStat : SpawnableStat
{
    
    private const string IconPath = "uid://bahh717wv8wh8";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "expl. radius";

    public override Texture2D Icon => StatIcon;

}