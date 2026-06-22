using System.Collections.Generic;

[GlobalClass]
[Icon(TexturePath)]
public partial class ModuleShapeDouble : ModuleShape
{
    private const string TexturePath = "uid://dm3v02c87i440";

    public override Texture2D Texture => GD.Load<Texture2D>(TexturePath);

    public override List<HexCoordinates> Tiles => [HexCoordinates.Zero, HexCoordinates.Right];
}