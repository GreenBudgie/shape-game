public class LifetimeStat : SpawnableStat
{
    private const string IconPath = "uid://cdwaf3usia8nn";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "lifetime";

    public override Texture2D Icon => StatIcon;

    public override string ValuePostfix => "sec";
}