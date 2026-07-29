public class PiercingStat : SpawnableStat
{
    
    private const string IconPath = "uid://c216uw8unddjj";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);
    
    public override string Name => "piercing";
    
    public override Texture2D Icon => StatIcon;

}