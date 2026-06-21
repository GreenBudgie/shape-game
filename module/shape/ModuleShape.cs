using Godot.Collections;

[GlobalClass]
public abstract partial class ModuleShape : Resource
{

    public abstract Texture2D Texture { get; }
    
    public abstract Array<HexDirection> Tiles { get; }

}