[GlobalClass, Icon(IconPath)]
public partial class PiercingStat : ModuleStat
{
    
    private const string IconPath = "uid://c216uw8unddjj";

    private static readonly Texture2D StatIcon = GD.Load<Texture2D>(IconPath);

    [Export] public int Piercing { get; private set; }
    
    public override string Name => "piercing";
    
    public override float Value => Piercing;
    
    public override Texture2D Icon => StatIcon;

}