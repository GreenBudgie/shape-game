public class ReloadStat : SpawnableStat
{
    private const string IconPath = "uid://oxosftrf543w";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "reload";

    public override Texture2D Icon => StatIcon;

    public override string Postfix => "sec";
}