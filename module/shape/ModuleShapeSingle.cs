using Godot.Collections;

[GlobalClass]
[Icon(TexturePath)]
public partial class ModuleShapeSingle : ModuleShape
{
    
    private const string TexturePath = "uid://d326uhbyeeuux";
    
    public override Texture2D Texture => GD.Load<Texture2D>(TexturePath);
    
    public override Array<HexDirection> Tiles => [];
}