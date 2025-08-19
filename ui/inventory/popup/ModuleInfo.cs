public partial class ModuleInfo : Control
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://c47f74u04f8m2");

    private Module _module = null!;
    
    public static ModuleInfo Create(Module module)
    {
        var node = Scene.Instantiate<ModuleInfo>();
        node._module = module;
        return node;
    }

    public override void _Ready()
    {
        
    }

}