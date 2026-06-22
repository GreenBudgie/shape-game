using Godot.Collections;

[GlobalClass]
[Icon(TexturePath)]
public partial class ModuleShapeTriple : ModuleShape
{
    private const string TexturePath = "uid://ddbmie8cjsnsb";

    public override Texture2D Texture => GD.Load<Texture2D>(TexturePath);

    public override Array<HexDirection> AdditionalTiles => [HexDirection.Right, HexDirection.BottomRight];
}