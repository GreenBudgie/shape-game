using System.Collections.Generic;

[GlobalClass]
[Icon(OutlineTexturePath)]
public partial class ModuleShapeTriple : ModuleShape
{
    private const string OutlineTexturePath = "uid://camgoa2syw3u1";
    private const string FillTexturePath = "uid://dl0cj3ys0bchl";

    public override Texture2D OutlineTexture => GD.Load<Texture2D>(OutlineTexturePath);
    public override Texture2D FillTexture => GD.Load<Texture2D>(FillTexturePath);

    public override List<HexCoordinates> Hexes => [
        HexCoordinates.Zero,
        HexCoordinates.Right,
        HexCoordinates.BottomRight
    ];
}