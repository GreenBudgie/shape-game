using Godot.Collections;

public partial class Module : Resource
{

    [Export] public Texture2D Texture { get; private set; } = null!;

    [Export] public string Name { get; private set; } = null!;
    
    [Export] public string Description { get; private set; } = null!;

    [Export] public Array<ModuleStat> Stats { get; private set; } = null!;

}