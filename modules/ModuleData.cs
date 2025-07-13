[GlobalClass]
public partial class ModuleData : Resource
{

    [Export] public Texture2D Texture { get; private set; } = null!;

    [Export] public ModuleBehavior Behavior { get; private set; } = null!;

    public bool IsModifier() => Behavior is ModifierModuleBehavior;
    public bool IsProjectile() => Behavior is ProjectileModuleBehavior;

}