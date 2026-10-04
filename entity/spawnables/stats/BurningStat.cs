public class BurningStat : SpawnableStat
{
    
    private const string IconPath = "uid://dmd8l5cd3ddxd";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "burning";

    public override Texture2D Icon => StatIcon;
    
    public override string ValuePostfix => "sec";

}