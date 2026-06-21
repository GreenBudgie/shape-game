using Godot.Collections;

public partial class Module : Resource
{

    [Export] public Texture2D Texture { get; private set; } = null!;

    [Export] public ModuleShape Shape { get; private set; } = null!;

    [Export] public Array<ModuleConnection> Connections { get; private set; } = null!;

    [Export] public string Name { get; private set; } = null!;

    [Export(PropertyHint.MultilineText)] public string Description { get; private set; } = null!;

    [Export] public Array<SpawnableStat> Stats { get; private set; } = [];
    
    [Export] public int Price { get; private set; }

    public virtual Color Color { get; } = Colors.White;

}