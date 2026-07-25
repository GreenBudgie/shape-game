using System.Collections.Generic;

[GlobalClass]
[Icon(TexturePath)]
public partial class ModuleShapeTriple : ModuleShape
{
    private const string TexturePath = "uid://c5it6ohqr4fyg";

    public override Texture2D Texture => GD.Load<Texture2D>(TexturePath);

    public override List<HexCoordinates> Hexes => [
        HexCoordinates.Zero,
        HexCoordinates.Right,
        HexCoordinates.BottomRight
    ];
}