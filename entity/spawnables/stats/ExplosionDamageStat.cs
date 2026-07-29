public class ExplosionDamageStat : SpawnableStat
{
    
    private const string IconPath = "uid://b3anlp3jqcefc";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    public override string Name => "expl. damage";

    public override Texture2D Icon => StatIcon;

}